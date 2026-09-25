// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Extensions.Web.Responses;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Web.CncAlarm;

namespace Pulse.Web.UnitTests.CncAlarm
{
  /// <summary>
  /// Unit tests of the services CncAlarmAtService, CncAlarmCurrentService and CncAlarmColorService
  /// with the option Web.CncAlarm.BusinessSeverity:
  /// the result with the business severity must be the same as with the dynamic column cncalarmseverityid
  /// </summary>
  [TestFixture]
  public class CncAlarmServices_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmServices_UnitTest).FullName);

    static readonly string BUSINESS_SEVERITY_KEY = "Web.CncAlarm.BusinessSeverity";

    const string CNC_INFO = "CncAlarmServices test";
    const string RED = "#FF0000";
    const string ORANGE = "#FFA500";
    const string GREY = "#808080";

    readonly IList<ICncAlarm> m_alarms = new List<ICncAlarm> ();
    readonly IList<ICurrentCncAlarm> m_currentAlarms = new List<ICurrentCncAlarm> ();

    static DateTime T (int hour, int minute) => new DateTime (2018, 05, 02, hour, minute, 00, DateTimeKind.Utc);

    [SetUp]
    public void SetUp ()
    {
      m_alarms.Clear ();
      m_currentAlarms.Clear ();
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Info.ConfigSet.ResetForceValues ();
    }

    /// <summary>
    /// Existing monitored machine of the unit test database
    /// </summary>
    const int MACHINE_ID = 1;

    /// <summary>
    /// Get the existing machine MACHINE_ID and its machine module where the alarms are created
    /// (the main machine module if any)
    ///
    /// There must be no cnc alarm of the test in the test range on the machine
    /// </summary>
    static (IMonitoredMachine, IMachineModule) GetExistingMachine (UtcDateTimeRange range)
    {
      var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
      Assert.That (machine, Is.Not.Null, $"Precondition: no monitored machine with id {MACHINE_ID}");
      Assert.That (machine.MachineModules, Is.Not.Empty, $"Precondition: no machine module on machine {MACHINE_ID}");
      Assert.That (ModelDAOHelper.DAOFactory.CncAlarmDAO.FindOverlapsRange (machine, range), Is.Empty,
        $"Precondition: there are already cnc alarms on machine {MACHINE_ID} in {range}, change the test date");
      var machineModule = machine.MainMachineModule ?? machine.MachineModules.OrderBy (m => m.Id).First ();
      return (machine, machineModule);
    }

    /// <summary>
    /// Create a severity and its pattern on the alarm type
    /// </summary>
    static void CreateSeverity (string type, string color, bool? focus)
    {
      var severity = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity (CNC_INFO, "severity " + type);
      severity.Color = color;
      severity.Focus = focus;
      severity.Description = "description " + type;
      ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakePersistent (severity);
      var rules = new CncAlarmSeverityPatternRules ();
      rules.Type = "^" + type + "$";
      ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.MakePersistent (
        ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern (CNC_INFO, rules, severity));
    }

    /// <summary>
    /// Severities:
    /// <item>red: focus (priority 1xx)</item>
    /// <item>orange: unknown focus (priority 5xx)</item>
    /// <item>grey: ignored (priority 7xx)</item>
    /// <item>unknown: no severity (priority 1000)</item>
    /// </summary>
    static void CreateSeverities ()
    {
      CreateSeverity ("red", RED, true);
      CreateSeverity ("orange", ORANGE, null);
      CreateSeverity ("grey", GREY, false);
    }

    void CreateAlarm (IMachineModule machineModule, string type, DateTime lower, DateTime upper)
    {
      var alarm = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, new UtcDateTimeRange (lower, upper), CNC_INFO, "", type, "1");
      alarm.Message = "message " + type;
      alarm.Properties["p"] = "v";
      ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm);
      m_alarms.Add (alarm);
    }

    void CreateCurrentAlarm (IMachineModule machineModule, string type, DateTime dateTime)
    {
      var alarm = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm (machineModule, dateTime, CNC_INFO, "", type, "1");
      alarm.Message = "message " + type;
      alarm.Properties["p"] = "v";
      ModelDAOHelper.DAOFactory.CurrentCncAlarmDAO.MakePersistent (alarm);
      m_currentAlarms.Add (alarm);
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
      foreach (var alarm in m_currentAlarms) {
        ModelDAOHelper.DAOFactory.CurrentCncAlarmDAO.Reload (alarm);
      }
    }

    static void SetBusinessSeverity (bool businessSeverity)
    {
      Lemoine.Info.ConfigSet.ForceValue (BUSINESS_SEVERITY_KEY, businessSeverity);
    }

    static string Summary (CncAlarmAtByMachineModuleDataDTO d) =>
      $"{d.Range}|{d.Display}|{d.Color}|{d.CncInfo}|{d.CncSubInfo}|{d.Type}|{d.Number}|{d.Message}|{string.Join (",", d.Properties.Select (p => p.Key + "=" + p.Value))}|{d.Severity}|{d.SeverityDescription}|{d.Stop}|{d.Focus}";

    static string Summary (CncAlarmCurrentByMachineModuleDataDTO d) =>
      $"{d.DateTime}|{d.Display}|{d.Color}|{d.CncInfo}|{d.CncSubInfo}|{d.Type}|{d.Number}|{d.Message}|{string.Join (",", d.Properties.Select (p => p.Key + "=" + p.Value))}|{d.Severity}|{d.SeverityDescription}|{d.Stop}|{d.Focus}";

    static string Summary (CncAlarmColorBlockDTO b) =>
      $"{b.Range}|{b.Day}|{b.Color}|{(null == b.Details ? "" : string.Join (",", b.Details.Select (x => x.Color + "=" + x.Duration)))}";

    IList<CncAlarmAtByMachineModuleDataDTO> GetAt (IMonitoredMachine machine, DateTime at, bool keepFocusOnly, bool includeIgnored, bool businessSeverity)
    {
      SetBusinessSeverity (businessSeverity);
      var request = new CncAlarmAtRequestDTO {
        MachineId = machine.Id,
        At = ConvertDTO.DateTimeUtcToIsoString (at),
        KeepFocusOnly = keepFocusOnly,
        IncludeIgnored = includeIgnored
      };
      var response = new CncAlarmAtService ().GetWithoutCache (request) as CncAlarmAtResponseDTO;
      Assert.That (response, Is.Not.Null);
      return response.ByMachineModule.SelectMany (m => m.CncAlarms)
        .Where (d => CNC_INFO.Equals (d.CncInfo)) // Only the alarms of the test (the machine is an existing one)
        .ToList ();
    }

    IList<CncAlarmCurrentByMachineModuleDataDTO> GetCurrent (IMonitoredMachine machine, bool keepFocusOnly, bool includeIgnored, bool businessSeverity)
    {
      SetBusinessSeverity (businessSeverity);
      var request = new CncAlarmCurrentRequestDTO {
        MachineId = machine.Id,
        KeepFocusOnly = keepFocusOnly,
        IncludeIgnored = includeIgnored
      };
      var response = new CncAlarmCurrentService ().GetWithoutCache (request) as CncAlarmCurrentResponseDTO;
      Assert.That (response, Is.Not.Null);
      return response.ByMachineModule.SelectMany (m => m.CncAlarms)
        .Where (d => CNC_INFO.Equals (d.CncInfo)) // Only the alarms of the test (the machine is an existing one)
        .ToList ();
    }

    IList<CncAlarmColorBlockDTO> GetColor (IMonitoredMachine machine, UtcDateTimeRange range, bool businessSeverity)
    {
      SetBusinessSeverity (businessSeverity);
      var request = new CncAlarmColorRequestDTO {
        MachineId = machine.Id,
        Range = range.ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt))
      };
      var response = new CncAlarmColorService ().GetWithoutCache (request) as CncAlarmColorResponseDTO;
      Assert.That (response, Is.Not.Null);
      return response.Blocks;
    }

    /// <summary>
    /// Test CncAlarmAtService with the different filters
    /// </summary>
    [TestCase (false, false, new string[] { "red", "orange", "unknown" })]
    [TestCase (false, true, new string[] { "red", "orange", "grey", "unknown" })]
    [TestCase (true, false, new string[] { "red" })]
    [TestCase (true, true, new string[] { "red" })]
    public void TestAt (bool keepFocusOnly, bool includeIgnored, string[] expectedTypes)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var (machine, machineModule) = GetExistingMachine (new UtcDateTimeRange (T (9, 00), T (13, 00)));
        CreateSeverities ();
        CreateAlarm (machineModule, "unknown", T (10, 00), T (11, 00));
        CreateAlarm (machineModule, "grey", T (10, 01), T (11, 00));
        CreateAlarm (machineModule, "orange", T (10, 02), T (11, 00));
        CreateAlarm (machineModule, "red", T (10, 03), T (11, 00));
        CreateAlarm (machineModule, "red", T (11, 00), T (12, 00)); // Not active at 10:30
        FlushAndReload ();

        var at = T (10, 30);
        var business = GetAt (machine, at, keepFocusOnly, includeIgnored, true);
        var reference = GetAt (machine, at, keepFocusOnly, includeIgnored, false);

        Assert.That (business.Select (d => d.Type), Is.EqualTo (expectedTypes), "Wrong alarms or wrong priority order");
        Assert.That (business.Select (Summary), Is.EqualTo (reference.Select (Summary)),
          "The business severity does not give the same response as the dynamic column");
        var red = business.First (d => d.Type.Equals ("red"));
        Assert.Multiple (() => {
          Assert.That (red.Color, Is.EqualTo (RED));
          Assert.That (red.Severity, Is.EqualTo ("severity red"));
          Assert.That (red.SeverityDescription, Is.EqualTo ("description red"));
          Assert.That (red.Focus, Is.True);
          Assert.That (red.Message, Is.EqualTo ("message red"));
          Assert.That (red.Properties["p"], Is.EqualTo ("v"));
        });
        if (!keepFocusOnly) {
          var unknown = business.Single (d => d.Type.Equals ("unknown"));
          Assert.Multiple (() => {
            Assert.That (unknown.Color, Is.Null);
            Assert.That (unknown.Severity, Is.Null);
          });
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test CncAlarmCurrentService with the different filters
    /// </summary>
    [TestCase (false, false, new string[] { "red", "orange", "unknown" })]
    [TestCase (false, true, new string[] { "red", "orange", "grey", "unknown" })]
    [TestCase (true, false, new string[] { "red" })]
    [TestCase (true, true, new string[] { "red" })]
    public void TestCurrent (bool keepFocusOnly, bool includeIgnored, string[] expectedTypes)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var (machine, machineModule) = GetExistingMachine (new UtcDateTimeRange (T (9, 00), T (13, 00)));
        CreateSeverities ();
        CreateCurrentAlarm (machineModule, "unknown", T (10, 00));
        CreateCurrentAlarm (machineModule, "grey", T (10, 00));
        CreateCurrentAlarm (machineModule, "orange", T (10, 00));
        CreateCurrentAlarm (machineModule, "red", T (10, 00));
        FlushAndReload ();

        var business = GetCurrent (machine, keepFocusOnly, includeIgnored, true);
        var reference = GetCurrent (machine, keepFocusOnly, includeIgnored, false);

        Assert.That (business.Select (d => d.Type), Is.EqualTo (expectedTypes), "Wrong alarms or wrong priority order");
        Assert.That (business.Select (Summary), Is.EqualTo (reference.Select (Summary)),
          "The business severity does not give the same response as the dynamic column");
        var red = business.First (d => d.Type.Equals ("red"));
        Assert.Multiple (() => {
          Assert.That (red.Color, Is.EqualTo (RED));
          Assert.That (red.Severity, Is.EqualTo ("severity red"));
          Assert.That (red.SeverityDescription, Is.EqualTo ("description red"));
          Assert.That (red.Focus, Is.True);
          Assert.That (red.Message, Is.EqualTo ("message red"));
          Assert.That (red.Properties["p"], Is.EqualTo ("v"));
        });

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test CncAlarmColorService
    /// </summary>
    [Test]
    public void TestColor ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var (machine, machineModule) = GetExistingMachine (new UtcDateTimeRange (T (9, 00), T (13, 00)));
        CreateSeverities ();
        CreateAlarm (machineModule, "red", T (10, 00), T (10, 30));
        CreateAlarm (machineModule, "orange", T (10, 10), T (11, 00));
        CreateAlarm (machineModule, "grey", T (11, 00), T (11, 30)); // The focus is not considered here
        CreateAlarm (machineModule, "unknown", T (11, 30), T (12, 00)); // No color
        FlushAndReload ();

        var range = new UtcDateTimeRange (T (9, 00), T (13, 00));
        var business = GetColor (machine, range, true);
        var reference = GetColor (machine, range, false);

        Assert.That (business.Select (b => b.Color), Is.EqualTo (new string[] { RED, ORANGE, GREY }));
        Assert.Multiple (() => {
          Assert.That (new UtcDateTimeRange (business[0].Range), Is.EqualTo (new UtcDateTimeRange (T (10, 00), T (10, 30))));
          Assert.That (new UtcDateTimeRange (business[1].Range), Is.EqualTo (new UtcDateTimeRange (T (10, 30), T (11, 00))));
          Assert.That (new UtcDateTimeRange (business[2].Range), Is.EqualTo (new UtcDateTimeRange (T (11, 00), T (11, 30))));
        });
        Assert.That (business.Select (Summary), Is.EqualTo (reference.Select (Summary)),
          "The business severity does not give the same response as the dynamic column");

        transaction.Rollback ();
      }
    }
  }
}
