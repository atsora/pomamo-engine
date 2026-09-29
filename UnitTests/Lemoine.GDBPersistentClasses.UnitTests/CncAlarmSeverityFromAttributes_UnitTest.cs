// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Business;
using Lemoine.Business.CncAlarm;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests of <see cref="CncAlarmSeverityFromAttributes"/>:
  /// the result must be the same as the one of the SQL function cncalarmseverityid
  /// </summary>
  [TestFixture]
  public class CncAlarmSeverityFromAttributes_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverityFromAttributes_UnitTest).FullName);

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
    /// Compare the severity from the business request with the one of the dynamic column
    /// </summary>
    [Test]
    public void TestSameAsDynamicColumn ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
        machine.Name = "machine_test";
        machine.MonitoringType = daoFactory.MachineMonitoringTypeDAO.FindById (2); // Monitored
        daoFactory.MonitoredMachineDAO.MakePersistent (machine);
        var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (machine, "machinemodule_test");
        daoFactory.MachineModuleDAO.MakePersistent (machineModule);

        var severity1 = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity ("Cnc test", "severity 1");
        daoFactory.CncAlarmSeverityDAO.MakePersistent (severity1);
        var severity2 = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity ("Cnc test", "severity 2");
        daoFactory.CncAlarmSeverityDAO.MakePersistent (severity2);
        var disabledSeverity = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity ("Cnc test", "disabled severity");
        disabledSeverity.Status = EditStatus.DEFAULT_VALUE_DELETED;
        daoFactory.CncAlarmSeverityDAO.MakePersistent (disabledSeverity);

        // Type + number + properties
        var rules1 = new CncAlarmSeverityPatternRules ();
        rules1.Type = "type 1";
        rules1.Number = "^C[0-9]{3}-[A-Z]+$";
        rules1.Properties["severity"] = "high";
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent (
          ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern ("Cnc test", rules1, severity1));
        // Message only
        var rules2 = new CncAlarmSeverityPatternRules ();
        rules2.Message = "^Door";
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent (
          ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern ("Cnc test", rules2, severity2));
        // Disabled severity
        var rules3 = new CncAlarmSeverityPatternRules ();
        rules3.Type = "type 3";
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent (
          ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern ("Cnc test", rules3, disabledSeverity));
        // Disabled pattern
        var rules4 = new CncAlarmSeverityPatternRules ();
        rules4.Type = "type 4";
        var disabledPattern = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern ("Cnc test", rules4, severity1);
        disabledPattern.Status = EditStatus.DEFAULT_VALUE_DELETED;
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent (disabledPattern);
        // Sub-info (citext => case insensitive)
        var rules5 = new CncAlarmSeverityPatternRules ();
        rules5.CncSubInfo = "^path";
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent (
          ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern ("Cnc test", rules5, severity2));

        var range = new UtcDateTimeRange (new DateTime (2000, 1, 1), new DateTime (2000, 1, 2));
        var alarms = new List<ICncAlarm> ();
        var currentAlarms = new List<ICurrentCncAlarm> ();
        ICncAlarm AddAlarm (string cncSubInfo, string type, string number, string message, IDictionary<string, object> properties)
        {
          var alarm = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, range, "Cnc test", cncSubInfo, type, number);
          alarm.Message = message;
          var currentAlarm = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm (machineModule, range.Lower.Value, "Cnc test", cncSubInfo, type, number);
          currentAlarm.Message = message;
          foreach (var property in properties) {
            alarm.Properties[property.Key] = property.Value;
            currentAlarm.Properties[property.Key] = property.Value;
          }
          daoFactory.CncAlarmDAO.MakePersistent (alarm);
          alarms.Add (alarm);
          daoFactory.CurrentCncAlarmDAO.MakePersistent (currentAlarm);
          currentAlarms.Add (currentAlarm);
          return alarm;
        }
        var noProperty = new Dictionary<string, object> ();
        var high = new Dictionary<string, object> { { "severity", "high" }, { "foo", "bar" } };
        var alarm1 = AddAlarm ("", "type 1", "C234-H", null, high); // severity1
        var alarm2 = AddAlarm ("", "TYPE 1", "C234-H", null, high); // severity1 (citext)
        var alarm3 = AddAlarm ("", "type 1", "C234-h", null, high); // no (number is case sensitive)
        var alarm4 = AddAlarm ("", "type 1", "C234-H", null, noProperty); // no (property)
        var alarm5 = AddAlarm ("", "type 1", "C234-H", null, new Dictionary<string, object> { { "severity", "low" } }); // no
        var alarm6 = AddAlarm ("", "type 2", "n", "Door open", noProperty); // severity2
        var alarm7 = AddAlarm ("", "type 2", "n", "door open", noProperty); // no (message is case sensitive)
        var alarm8 = AddAlarm ("", "type 3", "n", null, noProperty); // no (disabled severity)
        var alarm9 = AddAlarm ("", "type 4", "n", null, noProperty); // no (disabled pattern)
        var alarm10 = AddAlarm ("PATH1", "type 5", "n", null, noProperty); // severity2 (citext)

        daoFactory.FlushData ();
        foreach (var alarm in alarms) {
          daoFactory.CncAlarmDAO.Reload (alarm);
        }
        foreach (var currentAlarm in currentAlarms) {
          daoFactory.CurrentCncAlarmDAO.Reload (currentAlarm);
        }

        var at = daoFactory.CncAlarmDAO.FindAtWithoutSeverity (machineModule, new DateTime (2000, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        Assert.That (at.Select (a => a.Id), Is.EquivalentTo (alarms.Select (a => a.Id)));
        foreach (var alarm in alarms) {
          var detached = at.Single (a => a.Id == alarm.Id);
          Assert.Multiple (() => {
            Assert.That (detached.Display, Is.EqualTo (alarm.Display));
            Assert.That (detached.Severity, Is.Null);
            Assert.That (ServiceProvider.Get (new CncAlarmSeverityFromAttributes (detached))?.Id,
              Is.EqualTo (alarm.Severity?.Id), $"Wrong severity for alarm at {alarm.Type} {alarm.Number} {alarm.Message}");
          });
        }

        var current = daoFactory.CurrentCncAlarmDAO.FindByMachineModuleWithoutSeverity (machineModule);
        Assert.That (current.Select (a => a.Id), Is.EquivalentTo (currentAlarms.Select (a => a.Id)));
        foreach (var currentAlarm in currentAlarms) {
          var detached = current.Single (a => a.Id == currentAlarm.Id);
          Assert.Multiple (() => {
            Assert.That (detached.Display, Is.EqualTo (currentAlarm.Display));
            Assert.That (detached.Message, Is.EqualTo (currentAlarm.Message));
            Assert.That (detached.Properties, Is.EquivalentTo (currentAlarm.Properties));
            Assert.That (detached.Severity, Is.Null);
            Assert.That (ServiceProvider.Get (new CncAlarmSeverityFromAttributes (detached))?.Id,
              Is.EqualTo (currentAlarm.Severity?.Id), $"Wrong severity for current alarm {currentAlarm.Type} {currentAlarm.Number} {currentAlarm.Message}");
          });
        }

        var withoutSeverity = daoFactory.CncAlarmDAO.FindOverlapsRangeWithoutSeverity (machineModule, range);
        Assert.That (withoutSeverity, Has.Count.EqualTo (alarms.Count));
        foreach (var alarm in alarms) {
          var detached = withoutSeverity.Single (a => a.Id == alarm.Id);
          Assert.Multiple (() => {
            Assert.That (detached.Message, Is.EqualTo (alarm.Message));
            Assert.That (detached.Properties, Is.EquivalentTo (alarm.Properties));
            Assert.That (detached.Severity, Is.Null);
            Assert.That (ServiceProvider.Get (new CncAlarmSeverityFromAttributes (detached))?.Id,
              Is.EqualTo (alarm.Severity?.Id), $"Wrong severity for alarm {alarm.Type} {alarm.Number} {alarm.Message}");
          });
        }

        Assert.Multiple (() => {
          Assert.That (alarm1.Severity, Is.EqualTo (severity1));
          Assert.That (alarm2.Severity, Is.EqualTo (severity1));
          Assert.That (alarm3.Severity, Is.Null);
          Assert.That (alarm4.Severity, Is.Null);
          Assert.That (alarm5.Severity, Is.Null);
          Assert.That (alarm6.Severity, Is.EqualTo (severity2));
          Assert.That (alarm7.Severity, Is.Null);
          Assert.That (alarm8.Severity, Is.Null);
          Assert.That (alarm9.Severity, Is.Null);
          Assert.That (alarm10.Severity, Is.EqualTo (severity2));
        });

        transaction.Rollback ();
      }
    }
  }
}
