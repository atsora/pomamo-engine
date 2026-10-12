// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Lemoine.Alert.GDBListeners;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Alert.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="CncValueListener"/>: the cnc values from cncvalue and from cncdataperiod
  /// are serialized with the same XML format
  /// </summary>
  [TestFixture]
  public class CncValueListener_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncValueListener_UnitTest).FullName);

    const int MACHINE_ID = 1;

    string m_previousDSNName;

    [OneTimeSetUp]
    public void Init ()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName", Lemoine.Info.Constants.DEFAULT_DSN_UNIT_TEST_NAME);
      ModelDAOHelper.ModelFactory = new Lemoine.GDBPersistentClasses.GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName", m_previousDSNName);
      }
    }

    /// <summary>
    /// Historical value in cncvalue, then a period in cncdataperiod
    /// </summary>
    [Test]
    public void TestCncValueAndPeriod ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var monitoredMachine = daoFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
          var machineModule = monitoredMachine.MachineModules.OrderBy (m => m.Id).First ();
          var field = ModelDAOHelper.ModelFactory.CreateFieldFromName ("TestCncValueListener", "TestCncValueListener");
          field.Type = FieldType.Double;
          field.CncDataAggregationType = CncDataAggregationType.Average;
          field.CncDataStorage = CncDataStorage.CncDataPeriod;
          daoFactory.FieldDAO.MakePersistent (field);

          // The listener starts one day ago
          var t0 = DateTime.UtcNow.AddHours (-2);
          t0 = new DateTime (t0.Year, t0.Month, t0.Day, t0.Hour, t0.Minute, t0.Second, DateTimeKind.Utc);
          var cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, t0.AddHours (-1));
          cncValue.End = t0;
          cncValue.Double = 10.0;
          cncValue.Deviation = 1.0;
          daoFactory.CncValueDAO.MakePersistent (cncValue);
          var period = ModelDAOHelper.ModelFactory.CreateCncDataPeriod (machineModule, field, t0);
          period.End = t0.AddMinutes (1);
          period.Statistics = CncDataStatistics.FromValues (new double[] { 1.0, 3.0 });
          daoFactory.CncDataPeriodDAO.MakePersistent (period);
          daoFactory.Flush ();

          var listener = new CncValueListener { FieldId = field.Id };
          var datas = new List<XmlElement> ();
          for (var data = listener.GetData (); null != data; data = listener.GetData ()) {
            datas.Add (data);
          }

          Assert.That (datas, Has.Count.EqualTo (2));
          Assert.Multiple (() => {
            Assert.That (datas.Select (d => d.LocalName), Is.EqualTo (new[] { "CncValue", "CncValue" }));
            Assert.That (datas[0].GetAttribute ("Begin"), Is.EqualTo (t0.AddHours (-1).ToString ("yyyy-MM-dd HH:mm:ss")));
            Assert.That (datas[0].GetAttribute ("End"), Is.EqualTo (t0.ToString ("yyyy-MM-dd HH:mm:ss")));
            Assert.That (datas[1].GetAttribute ("Begin"), Is.EqualTo (t0.ToString ("yyyy-MM-dd HH:mm:ss")));
            Assert.That (datas[1].GetAttribute ("End"), Is.EqualTo (t0.AddMinutes (1).ToString ("yyyy-MM-dd HH:mm:ss")));
            Assert.That (datas[0]["Value"]?.InnerText, Is.EqualTo ("10"));
            Assert.That (datas[1]["Value"]?.InnerText, Is.EqualTo ("2"));
            Assert.That (datas[1]["MachineModule"], Is.Not.Null, "same elements");
            Assert.That (datas[1]["Field"], Is.Not.Null, "same elements");
            Assert.That (datas[1].Attributes.Cast<XmlAttribute> ().Select (a => a.Name).OrderBy (n => n),
              Is.EqualTo (datas[0].Attributes.Cast<XmlAttribute> ().Select (a => a.Name).OrderBy (n => n)), "same attributes");
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
