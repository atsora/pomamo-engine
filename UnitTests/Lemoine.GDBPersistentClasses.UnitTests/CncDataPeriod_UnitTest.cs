// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="CncDataPeriod"/> and <see cref="CncDataPeriodDAO"/>
  ///
  /// The dates are after 2090 (no partition in advance): the partitions are created in the transaction
  /// of the test, that is rolled back
  /// </summary>
  [TestFixture]
  public class CncDataPeriod_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncDataPeriod_UnitTest).FullName);

    const int MACHINE_ID = 1;
    const int FIELD_ID = 1;
    static readonly DateTime T0 = new DateTime (2091, 03, 07, 10, 00, 00, 123, DateTimeKind.Utc).AddTicks (4567);

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
    /// Insert, extend (update with the secondary keys), find and delete a period
    /// </summary>
    [Test]
    public void TestInsertExtendFindDelete ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var (machineModule, field) = GetMachineModuleField ();
        var begin = CncDataWeek.TruncateToMilliseconds (T0);

        var period = ModelDAOHelper.ModelFactory.CreateCncDataPeriod (machineModule, field, T0);
        Assert.That (period.Begin, Is.EqualTo (begin), "begin not truncated");
        period.Extend (begin.AddSeconds (1), CncDataStatistics.FromValues (new double[] { 1.0, 3.0 }));
        daoFactory.CncDataPeriodDAO.MakePersistent (period);
        daoFactory.FlushData ();
        var id = period.Id;
        Assert.That (id, Is.Not.EqualTo (0));

        // Update: the request must find the row with the secondary keys
        period.Extend (begin.AddSeconds (2), CncDataStatistics.FromValue (5.0));
        period.JsonValue = @"{""a"": 1}";
        daoFactory.FlushData ();
        NHibernateHelper.GetCurrentSession ().Evict (period);

        var reloaded = daoFactory.CncDataPeriodDAO.FindById (id, machineModule, T0);
        Assert.That (reloaded, Is.Not.Null);
        Assert.Multiple (() => {
          Assert.That (reloaded.Version, Is.EqualTo (2), "update not run");
          Assert.That (reloaded.Begin, Is.EqualTo (begin));
          Assert.That (reloaded.End, Is.EqualTo (begin.AddSeconds (2)));
          Assert.That (reloaded.Count, Is.EqualTo (3));
          Assert.That (reloaded.Average, Is.EqualTo (3.0).Within (1e-9));
          Assert.That (reloaded.Min, Is.EqualTo (1.0));
          Assert.That (reloaded.Max, Is.EqualTo (5.0));
          Assert.That (reloaded.LastValue, Is.EqualTo (5.0));
          Assert.That (reloaded.JsonValue, Does.Contain ("\"a\""));
          Assert.That (daoFactory.CncDataPeriodDAO.FindWithEnd (machineModule, field, begin.AddSeconds (2))?.Id, Is.EqualTo (id));
          Assert.That (daoFactory.CncDataPeriodDAO.FindAt (machineModule, field, begin.AddSeconds (1))?.Id, Is.EqualTo (id));
          Assert.That (daoFactory.CncDataPeriodDAO.FindAt (machineModule, field, begin.AddSeconds (3)), Is.Null);
          Assert.That (daoFactory.CncDataPeriodDAO.FindLast (machineModule, field)?.Id, Is.EqualTo (id));
          Assert.That (daoFactory.CncDataPeriodDAO.FindOverlapsRange (machineModule, field,
            new UtcDateTimeRange (begin.AddSeconds (1), begin.AddSeconds (10))).Select (p => p.Id), Is.EqualTo (new long[] { id }));
          Assert.That (daoFactory.CncDataPeriodDAO.FindOverlapsRange (machineModule, field,
            new UtcDateTimeRange (begin.AddSeconds (2), begin.AddSeconds (10))), Is.Empty);
        });

        var values = daoFactory.CncDataPeriodDAO.FindValuesOverlapsRange (machineModule, field,
          new UtcDateTimeRange (begin.AddMinutes (-1), begin.AddMinutes (1)));
        Assert.That (values, Has.Count.EqualTo (1));
        Assert.Multiple (() => {
          Assert.That (values[0].Id, Is.EqualTo (id));
          Assert.That (values[0].Archived, Is.False);
          Assert.That (values[0].Begin, Is.EqualTo (begin));
          Assert.That (values[0].End, Is.EqualTo (begin.AddSeconds (2)));
          Assert.That (values[0].Count, Is.EqualTo (3));
          Assert.That (values[0].JsonValue, Does.Contain ("\"a\""));
        });

        // Delete: with the secondary keys too
        daoFactory.CncDataPeriodDAO.MakeTransient (reloaded);
        daoFactory.FlushData ();
        Assert.That (daoFactory.CncDataPeriodDAO.FindById (id, machineModule, T0), Is.Null);
      }
    }

    /// <summary>
    /// A period can't cross a week bound, nor end before its begin
    /// </summary>
    [Test]
    public void TestEndLimits ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var (machineModule, field) = GetMachineModuleField ();
        var cutOffDay = (DayOfWeek)daoFactory.CncDataPartitionDAO.GetWeekCutOffDay ();
        var weekEnd = CncDataWeek.GetWeekEnd (T0, cutOffDay);

        var period = ModelDAOHelper.ModelFactory.CreateCncDataPeriod (machineModule, field, weekEnd.AddSeconds (-1));
        Assert.Multiple (() => {
          Assert.That (period.MaxEnd, Is.EqualTo (weekEnd));
          Assert.Throws<ArgumentOutOfRangeException> (() => period.End = weekEnd.AddMilliseconds (1));
          Assert.Throws<ArgumentOutOfRangeException> (() => period.End = weekEnd.AddSeconds (-2));
        });
        period.End = weekEnd;
        daoFactory.CncDataPeriodDAO.MakePersistent (period);
        daoFactory.FlushData ();
        Assert.Throws<ArgumentOutOfRangeException> (() => period.Extend (weekEnd.AddSeconds (-1), CncDataStatistics.FromValue (1.0)),
          "end before the current end");

        // The next period begins at the week bound, in the next partition
        var next = ModelDAOHelper.ModelFactory.CreateCncDataPeriod (machineModule, field, weekEnd);
        next.Extend (weekEnd.AddSeconds (1), CncDataStatistics.FromValue (2.0));
        daoFactory.CncDataPeriodDAO.MakePersistent (next);
        daoFactory.FlushData ();
        Assert.Multiple (() => {
          Assert.That (daoFactory.CncDataPeriodDAO.FindWithEnd (machineModule, field, weekEnd)?.Id, Is.EqualTo (period.Id));
          Assert.That (daoFactory.CncDataPeriodDAO.FindAt (machineModule, field, weekEnd)?.Id, Is.EqualTo (next.Id));
          Assert.That (daoFactory.CncDataPeriodDAO.FindOverlapsRange (machineModule, field,
            new UtcDateTimeRange (weekEnd.AddMinutes (-1), weekEnd.AddMinutes (1))), Has.Count.EqualTo (2));
        });
      }
    }

    static (IMachineModule, IField) GetMachineModuleField ()
    {
      var monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
      Assert.That (monitoredMachine, Is.Not.Null);
      var machineModule = monitoredMachine.MachineModules.OrderBy (m => m.Id).First ();
      var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById (FIELD_ID);
      Assert.That (field, Is.Not.Null);
      return (machineModule, field);
    }
  }
}
