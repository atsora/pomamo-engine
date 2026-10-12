// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="CncDataSampleDAO"/>
  ///
  /// The dates are after 2090 (no partition in advance): the partitions are created in the transaction
  /// of the test, that is rolled back
  /// </summary>
  [TestFixture]
  public class CncDataSample_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncDataSample_UnitTest).FullName);

    const int MACHINE_ID = 1;
    const int FIELD_ID = 1;
    const int JSON_FIELD_ID = 2;
    static readonly DateTime T0 = new DateTime (2091, 03, 07, 10, 00, 00, DateTimeKind.Utc);

    string previousDSNName;

    [OneTimeSetUp]
    public void Init ()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName", Lemoine.Info.Constants.DEFAULT_DSN_UNIT_TEST_NAME);
      ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName", previousDSNName);
      }
    }

    /// <summary>
    /// Split of the values into rows
    /// </summary>
    [Test]
    public void TestPack ()
    {
      var weekEnd = CncDataWeek.GetWeekEnd (T0, DayOfWeek.Sunday);
      var values = new List<CncDataSampleValue> {
        new CncDataSampleValue (T0.AddMilliseconds (10), 1.0),
        new CncDataSampleValue (T0, 0.0), // Not ordered
        new CncDataSampleValue (T0.AddMilliseconds (32767), 2.0),
        new CncDataSampleValue (T0.AddMilliseconds (32768), 3.0), // Too long => new row
        new CncDataSampleValue (T0.AddMilliseconds (32778), "{\"a\": 1}"), // Other kind => new row
        new CncDataSampleValue (weekEnd.AddMilliseconds (-10), 4.0), // New row
        new CncDataSampleValue (weekEnd, 5.0), // Next week => new row
      };
      var rows = CncDataSampleDAO.Pack (FIELD_ID, values, DayOfWeek.Sunday).ToList ();
      Assert.That (rows, Has.Count.EqualTo (5));
      Assert.Multiple (() => {
        Assert.That (rows[0].Begin, Is.EqualTo (T0));
        Assert.That (rows[0].End, Is.EqualTo (T0.AddMilliseconds (32767)));
        Assert.That (rows[0].Deltas, Is.EqualTo (new short[] { 0, 10, 32767 }));
        Assert.That (rows[0].DoubleValues, Is.EqualTo (new double[] { 0.0, 1.0, 2.0 }));
        Assert.That (rows[1].Deltas, Is.EqualTo (new short[] { 0 }));
        Assert.That (rows[2].IsJson, Is.True);
        Assert.That (rows[3].Begin, Is.EqualTo (weekEnd.AddMilliseconds (-10)));
        Assert.That (rows[4].Begin, Is.EqualTo (weekEnd));
      });
    }

    /// <summary>
    /// Insert samples with COPY and read them, by page too
    /// </summary>
    [Test]
    public void TestInsertFind ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var monitoredMachine = daoFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
        var machineModule = monitoredMachine.MachineModules.OrderBy (m => m.Id).First ();
        var field = daoFactory.FieldDAO.FindById (FIELD_ID);
        var jsonField = daoFactory.FieldDAO.FindById (JSON_FIELD_ID);
        var cutOffDay = (DayOfWeek)daoFactory.CncDataPartitionDAO.GetWeekCutOffDay ();
        var weekEnd = CncDataWeek.GetWeekEnd (T0, cutOffDay);

        // 100 Hz during 1 minute, then 2 values across the week bound
        var values = Enumerable.Range (0, 6000)
          .Select (i => new CncDataSampleValue (T0.AddMilliseconds (10 * i), i))
          .Concat (new CncDataSampleValue[] {
            new CncDataSampleValue (weekEnd.AddMilliseconds (-10), -1.0),
            new CncDataSampleValue (weekEnd.AddMilliseconds (10), -2.0),
          })
          .ToList ();
        var jsonValues = new List<CncDataSampleValue> {
          new CncDataSampleValue (T0, "{\"a\": 1}"),
          new CncDataSampleValue (T0.AddMilliseconds (10), "[1, 2]"),
        };
        var rowCount = daoFactory.CncDataSampleDAO.Insert (machineModule, new CncDataSampleSeries[] {
          new CncDataSampleSeries (field, values),
          new CncDataSampleSeries (jsonField, jsonValues),
        });
        Assert.That (rowCount, Is.EqualTo (2 + 2 + 1), "60 s in 2 rows of at most 32.767 s, 2 rows across the week bound, 1 json row");

        var all = daoFactory.CncDataSampleDAO.FindInRange (machineModule, field, new UtcDateTimeRange (T0, weekEnd.AddDays (1)));
        Assert.That (all.Select (v => v.DateTime), Is.EqualTo (values.Select (v => v.DateTime)));
        Assert.That (all.Select (v => v.DoubleValue), Is.EqualTo (values.Select (v => v.DoubleValue)));

        // By page
        var page1 = daoFactory.CncDataSampleDAO.FindInRange (machineModule, field, new UtcDateTimeRange (T0), 100);
        var page2 = daoFactory.CncDataSampleDAO.FindInRange (machineModule, field,
          new UtcDateTimeRange (page1.Last ().DateTime.AddMilliseconds (1)), 100);
        Assert.Multiple (() => {
          Assert.That (page1, Has.Count.EqualTo (100));
          Assert.That (page1.First ().DateTime, Is.EqualTo (T0));
          Assert.That (page2, Has.Count.EqualTo (100));
          Assert.That (page2.First ().DateTime, Is.EqualTo (T0.AddMilliseconds (1000)));
        });

        var json = daoFactory.CncDataSampleDAO.FindInRange (machineModule, jsonField, new UtcDateTimeRange (T0, T0.AddSeconds (1)));
        Assert.That (json, Has.Count.EqualTo (2));
        Assert.Multiple (() => {
          Assert.That (json[0].DoubleValue, Is.Null);
          Assert.That (json[0].JsonValue, Does.Contain ("\"a\""));
          Assert.That (json[1].JsonValue, Does.Contain ("2"));
        });

        var last = daoFactory.CncDataSampleDAO.FindLast (machineModule, field);
        Assert.That (last.HasValue, Is.True);
        Assert.Multiple (() => {
          Assert.That (last.Value.DateTime, Is.EqualTo (weekEnd.AddMilliseconds (10)));
          Assert.That (last.Value.DoubleValue, Is.EqualTo (-2.0));
        });
      }
    }
  }
}
