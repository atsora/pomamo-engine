// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Business.CncAlarm;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Business.UnitTests.CncAlarm
{
  /// <summary>
  /// Unit tests of <see cref="CncAlarmColorDAO"/>
  ///
  /// The result with the business severity must be the same as with the dynamic column cncalarmseverityid
  ///
  /// Note: the business service is cached in this project,
  /// so each test uses its own cnc info not to re-use the cached patterns of another test
  /// </summary>
  [TestFixture]
  public class CncAlarmColorDAO_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmColorDAO_UnitTest).FullName);

    const string RED = "#FF0000";
    const string ORANGE = "#FFA500";

    static DateTime T (int hour, int minute) => new DateTime (2018, 05, 02, hour, minute, 00, DateTimeKind.Utc);

    /// <summary>
    /// Create a severity (with a color and a focus) and its pattern on the alarm type
    /// </summary>
    static void CreateSeverity (string cncInfo, string type, string color, bool? focus)
    {
      var severity = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity (cncInfo, "severity " + type);
      severity.Color = color;
      severity.Focus = focus;
      ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakePersistent (severity);
      var rules = new CncAlarmSeverityPatternRules ();
      rules.Type = "^" + type + "$";
      ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.MakePersistent (
        ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern (cncInfo, rules, severity));
    }

    readonly IList<ICncAlarm> m_alarms = new List<ICncAlarm> ();

    void CreateAlarm (IMachineModule machineModule, string cncInfo, string type, DateTime lower, DateTime upper)
    {
      var alarm = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, new UtcDateTimeRange (lower, upper), cncInfo, "", type, "1");
      ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm);
      m_alarms.Add (alarm);
    }

    /// <summary>
    /// Flush the data and reload the alarms,
    /// else the dynamic columns (severity, display) of the alarms of the session are not set
    /// </summary>
    void FlushAndReload ()
    {
      ModelDAOHelper.DAOFactory.Flush ();
      foreach (var alarm in m_alarms) {
        ModelDAOHelper.DAOFactory.CncAlarmDAO.Reload (alarm);
      }
    }

    [SetUp]
    public void SetUp ()
    {
      m_alarms.Clear ();
    }

    /// <summary>
    /// Existing monitored machine of the unit test database (with a single machine module)
    /// </summary>
    const int MACHINE_ID = 1;

    /// <summary>
    /// Get the existing monitored machine MACHINE_ID
    /// with at least the specified number of machine modules:
    /// the missing machine modules are created (in the test transaction)
    ///
    /// There must be no cnc alarm in the test range on the machine
    /// </summary>
    static IMonitoredMachine GetExistingMachine (int minNbMachineModules, UtcDateTimeRange range)
    {
      var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
      Assert.That (machine, Is.Not.Null, $"Precondition: no monitored machine with id {MACHINE_ID}");
      var nbMissingMachineModules = minNbMachineModules - machine.MachineModules.Count;
      if (0 < nbMissingMachineModules) {
        var machineModules = machine.MachineModules.ToList ();
        for (int i = 0; i < nbMissingMachineModules; ++i) {
          var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (machine, "CncAlarmColorDAO_UnitTest " + i);
          ModelDAOHelper.DAOFactory.MachineModuleDAO.MakePersistent (machineModule);
          machineModules.Add (machineModule);
        }
        // machine.MachineModules is not updated in memory by CreateMachineModuleFromName:
        // flush, evict the machine and its machine modules and load them again
        ModelDAOHelper.DAOFactory.Flush ();
        foreach (var machineModule in machineModules) {
          ModelDAOHelper.DAOFactory.Evict (machineModule);
        }
        ModelDAOHelper.DAOFactory.Evict (machine);
        machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
      }
      Assert.That (machine.MachineModules, Has.Count.GreaterThanOrEqualTo (minNbMachineModules));
      Assert.That (ModelDAOHelper.DAOFactory.CncAlarmDAO.FindOverlapsRange (machine, range), Is.Empty,
        $"Precondition: there are already cnc alarms on machine {machine.Id} in {range}, change the test date");
      return machine;
    }

    static void CheckSlot (ICncAlarmColor slot, string color, DateTime lower, DateTime upper)
    {
      Assert.Multiple (() => {
        Assert.That (slot.Color, Is.EqualTo (color));
        Assert.That (slot.DateTimeRange.Lower.Value, Is.EqualTo (lower));
        Assert.That (slot.DateTimeRange.Upper.Value, Is.EqualTo (upper));
      });
    }

    /// <summary>
    /// Check the business severity gives the same result as the dynamic column
    /// </summary>
    static void CheckSameAsDynamicColumn (IList<ICncAlarmColor> slots, IMonitoredMachine machine, UtcDateTimeRange range)
    {
      var reference = new CncAlarmColorDAO (false).FindOverlapsRange (machine, range);
      Assert.That (slots.Select (s => (s.Color, s.DateTimeRange.ToString ())),
        Is.EqualTo (reference.Select (s => (s.Color, s.DateTimeRange.ToString ()))),
        "The business severity does not give the same slots as the dynamic column");
    }

    /// <summary>
    /// Alarms without any color (no severity or a severity without color) are dismissed
    /// and consecutive slots of different colors are kept
    /// </summary>
    [Test]
    public void TestColorsAndDismissedAlarms ()
    {
      const string cncInfo = "CncAlarmColorDAO test 1";
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var range = new UtcDateTimeRange (T (9, 00), T (13, 00));
        var machine = GetExistingMachine (1, range);
        var machineModule = machine.MachineModules.OrderBy (m => m.Id).First ();
        CreateSeverity (cncInfo, "red", RED, true);
        CreateSeverity (cncInfo, "orange", ORANGE, null);
        CreateSeverity (cncInfo, "nocolor", null, true);

        CreateAlarm (machineModule, cncInfo, "red", T (10, 00), T (10, 30));
        CreateAlarm (machineModule, cncInfo, "orange", T (10, 10), T (11, 00));
        CreateAlarm (machineModule, cncInfo, "nocolor", T (11, 00), T (11, 30)); // Severity without color
        CreateAlarm (machineModule, cncInfo, "unknown", T (11, 30), T (12, 00)); // No severity
        FlushAndReload ();

        var slots = new CncAlarmColorDAO ().FindOverlapsRange (machine, range);

        Assert.That (slots, Has.Count.EqualTo (2));
        CheckSlot (slots[0], RED, T (10, 00), T (10, 30));
        CheckSlot (slots[1], ORANGE, T (10, 30), T (11, 00));
        CheckSameAsDynamicColumn (slots, machine, range);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// A more critical alarm on another machine module splits a less critical alarm
    /// </summary>
    [Test]
    public void TestPriorityOnSeveralMachineModules ()
    {
      const string cncInfo = "CncAlarmColorDAO test 2";
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var range = new UtcDateTimeRange (T (9, 00), T (13, 00));
        var machine = GetExistingMachine (2, range);
        var machineModules = machine.MachineModules.OrderBy (m => m.Id).ToList ();
        var machineModule1 = machineModules[0];
        var machineModule2 = machineModules[1];
        CreateSeverity (cncInfo, "red", RED, true); // Priority 1xx
        CreateSeverity (cncInfo, "orange", ORANGE, null); // Priority 5xx

        CreateAlarm (machineModule1, cncInfo, "orange", T (10, 00), T (11, 00));
        CreateAlarm (machineModule2, cncInfo, "red", T (10, 20), T (10, 40));
        FlushAndReload ();

        var slots = new CncAlarmColorDAO ().FindOverlapsRange (machine, range);

        Assert.That (slots, Has.Count.EqualTo (3));
        CheckSlot (slots[0], ORANGE, T (10, 00), T (10, 20));
        CheckSlot (slots[1], RED, T (10, 20), T (10, 40));
        CheckSlot (slots[2], ORANGE, T (10, 40), T (11, 00));
        CheckSameAsDynamicColumn (slots, machine, range);

        transaction.Rollback ();
      }
    }
  }
}
