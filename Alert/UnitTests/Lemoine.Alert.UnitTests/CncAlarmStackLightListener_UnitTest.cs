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
  /// Unit tests for <see cref="CncAlarmStackLightListener"/>
  /// </summary>
  [TestFixture]
  public class CncAlarmStackLightListener_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmStackLightListener_UnitTest).FullName);

    const int MACHINE_ID = 1;
    const string ALARM_NUMBER = "TESTSTACKLIGHTALARM";

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
    /// The alarms are searched around the begin of each red/yellow stack light value,
    /// not around the begin of the first value of the batch
    /// </summary>
    [Test]
    public void TestAlarmOfSecondValue ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var monitoredMachine = daoFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
          var machineModule = monitoredMachine.MachineModules.OrderBy (m => m.Id).First ();
          var field = daoFactory.FieldDAO.FindById ((int)FieldId.StackLight);

          // The listener starts one day ago
          var t1 = DateTime.UtcNow.AddHours (-2);
          t1 = new DateTime (t1.Year, t1.Month, t1.Day, t1.Hour, t1.Minute, t1.Second, DateTimeKind.Utc);
          AddStackLight (machineModule, field, t1, t1.AddSeconds (30), StackLight.GreenOn | StackLight.YellowOff | StackLight.RedOff);
          AddStackLight (machineModule, field, t1.AddSeconds (30), t1.AddSeconds (60), StackLight.GreenOff | StackLight.YellowOff | StackLight.RedOn);
          var alarm = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule,
            new UtcDateTimeRange (t1.AddSeconds (30), t1.AddSeconds (40)), "TestInfo", "TestSubInfo", "TestType", ALARM_NUMBER);
          daoFactory.CncAlarmDAO.MakePersistent (alarm);
          daoFactory.Flush ();

          var listener = new CncAlarmStackLightListener ();
          var datas = new List<XmlElement> ();
          for (var data = listener.GetData (); null != data; data = listener.GetData ()) {
            datas.Add (data);
          }

          var matching = datas.Where (d => d.OuterXml.Contains (ALARM_NUMBER)).ToList ();
          Assert.That (matching, Has.Count.EqualTo (1), "alarm at the begin of the second (red) value");
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    static void AddStackLight (IMachineModule machineModule, IField field, DateTime begin, DateTime end, StackLight stackLight)
    {
      var cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, begin);
      cncValue.End = end;
      cncValue.Int = (int)stackLight;
      ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue);
    }
  }
}
