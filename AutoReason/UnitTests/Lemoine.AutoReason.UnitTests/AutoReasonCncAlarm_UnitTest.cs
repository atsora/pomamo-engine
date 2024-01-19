// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonCncAlarm;
using Lemoine.Core.Log;
using System.Linq;
using Lemoine.Database.Persistent;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonFocusedAlarm
  /// </summary>
  [TestFixture]
  public class AutoReasonCncAlarm_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonCncAlarm_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonCncAlarm_UnitTest () : base (DateTime.Today.AddDays (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with several alarms
    /// * Alarm 1 focused from T1 to T3
    /// * Alarm 2 focused from T2 to T4
    /// * Alarm 3 not focused from T4 to T7
    /// * Alarm 4 focused from T8 to T9
    /// The alarm acquisition status is set to T7
    /// Associated reason slots "focused alarm"
    /// => from T1 to no end
    /// => from T2 to no end
    /// </summary>
    [Test]
    public void FocusedAlarm_AlarmsDetected ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Constant
            const string CNC_TYPE = "Fanuc";
            const string FOCUSED_TYPE = "PMC error";
            const string IGNORED_TYPE = "Background P/S";

            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            var machineModule = machine.MachineModules.First ();
            Assert.That (machineModule, Is.Not.Null);

            // Acquisition state for the alarm is after the period we are analyzing
            var acquisitionState = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (machineModule, AcquisitionStateKey.Alarms);
            acquisitionState ??= ModelDAOHelper.ModelFactory.CreateAcquisitionState (machineModule, AcquisitionStateKey.Alarms);

            acquisitionState.DateTime = T (7);
            ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent (acquisitionState);

            // Create alarms
            var alarm1 = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, R (1, 3), CNC_TYPE, "", FOCUSED_TYPE, "1");
            alarm1.Message = "message 1";
            ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm1);
            var alarm2 = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, R (2, 4), CNC_TYPE, "", FOCUSED_TYPE, "2");
            alarm2.Message = "message 2";
            ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm2);
            var alarm3 = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, R (4, 7), CNC_TYPE, "", IGNORED_TYPE, "3");
            alarm3.Message = "message 3";
            ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm3);
            var alarm4 = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, R (8, 9), CNC_TYPE, "", FOCUSED_TYPE, "4");
            alarm4.Message = "message 4";
            ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm4);
            ModelDAOHelper.DAOFactory.Flush ();

            // Remove the objects from the session, so that they are retrieved again
            // with the severity
            NHibernateHelper.GetCurrentSession ().Evict (alarm1);
            NHibernateHelper.GetCurrentSession ().Evict (alarm2);
            NHibernateHelper.GetCurrentSession ().Evict (alarm3);
            NHibernateHelper.GetCurrentSession ().Evict (alarm4);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);

            // Run the analyze
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 2 autoreasons appeared
            {
              // Should have been working but the severity is not detected (always null)
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (2), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (1)), "wrong start for the first reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the first reason should have no end");
                Assert.That (reasonAssociation.ReasonDetails, Is.EqualTo ("1: message 1"), "wrong detail of reason 1");
              });
              reasonAssociation = reasonAssociations[1];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (2)), "wrong start for the second reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the second reason should have no end");
                Assert.That (reasonAssociation.ReasonDetails, Is.EqualTo ("2: message 2"), "wrong detail of reason 2");
              });
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine)
    {
      var plugin = new Lemoine.Plugin.AutoReasonCncAlarm.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
@"{
  ""ReasonScore"": 60.0,
  ""ManualScore"": 100.0,
  ""DefaultReasonTranslationKey"": ""FocusedCncAlarm"",
  ""DefaultReasonTranslationValue"": ""Focused cnc alarm"",
  ""FocusedOnly"": true
}"
      );

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, autoReason.GetKey ("DateTime"));
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
    }

    /// <summary>
    /// Test case with several alarms
    /// * Alarm 1 focused from T1 to T3
    /// * Alarm 2 focused from T2 to T4
    /// * Alarm 3 not focused from T4 to T7
    /// * Alarm 4 focused from T8 to T9
    /// The alarm acquisition status is set to T7
    /// Associated reason slots "focused alarm"
    /// => from T1 to no end
    /// => from T2 to no end
    /// </summary>
    [Test]
    public void MessageRegex ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Constant
            const string CNC_TYPE = "Fanuc";
            const string FOCUSED_TYPE = "PMC error";
            const string IGNORED_TYPE = "Background P/S";

            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);

            // Associated machine module
            IMachineModule mamo = null;
            foreach (IMachineModule mamoTmp in machine.MachineModules) {
              mamo = mamoTmp;
              break;
            }
            Assert.That (mamo, Is.Not.Null);

            // Acquisition state for the alarm is after the period we are analyzing
            var acquisitionState = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (mamo, AcquisitionStateKey.Alarms);
            acquisitionState ??= ModelDAOHelper.ModelFactory.CreateAcquisitionState (mamo, AcquisitionStateKey.Alarms);

            acquisitionState.DateTime = T (7);
            ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent (acquisitionState);

            // Create alarms
            var alarm1 = ModelDAOHelper.ModelFactory.CreateCncAlarm (mamo, R (1, 3), CNC_TYPE, "", FOCUSED_TYPE, "1");
            alarm1.Message = "abc";
            ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm1);
            var alarm2 = ModelDAOHelper.ModelFactory.CreateCncAlarm (mamo, R (2, 4), CNC_TYPE, "", FOCUSED_TYPE, "2");
            alarm2.Message = "message 2";
            ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm2);
            var alarm3 = ModelDAOHelper.ModelFactory.CreateCncAlarm (mamo, R (4, 7), CNC_TYPE, "", IGNORED_TYPE, "3");
            alarm3.Message = "message 3";
            ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm3);
            var alarm4 = ModelDAOHelper.ModelFactory.CreateCncAlarm (mamo, R (8, 9), CNC_TYPE, "", FOCUSED_TYPE, "4");
            alarm4.Message = "message 4";
            ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (alarm4);
            ModelDAOHelper.DAOFactory.Flush ();

            // Remove the objects from the session, so that they are retrieved again
            // with the severity
            NHibernateHelper.GetCurrentSession ().Evict (alarm1);
            NHibernateHelper.GetCurrentSession ().Evict (alarm2);
            NHibernateHelper.GetCurrentSession ().Evict (alarm3);
            NHibernateHelper.GetCurrentSession ().Evict (alarm4);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtensionWithMessageRegex (machine);

            // Run the analyze
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 2 autoreasons appeared
            {
              // Should have been working but the severity is not detected (always null)
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (1), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (1)), "wrong start for the first reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the first reason should have no end");
                Assert.That (reasonAssociation.ReasonDetails, Is.EqualTo ("1: abc"), "wrong detail of reason 1");
              });
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtensionWithMessageRegex (IMonitoredMachine machine)
    {
      var plugin = new Lemoine.Plugin.AutoReasonCncAlarm.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
@"{
  ""ReasonScore"": 60.0,
  ""ManualScore"": 100.0,
  ""DefaultReasonTranslationKey"": ""FocusedCncAlarm"",
  ""DefaultReasonTranslationValue"": ""Focused cnc alarm"",
  ""MessageRegex"": ""ab.*""
}"
      );

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, autoReason.GetKey ("DateTime"));
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
    }

    void CreateReasonSlot (int start, int end, IMachine machine, IMachineMode machineMode, IMachineObservationState mos, IReason reason)
    {
      var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (start, end));
      reasonSlot.MachineMode = machineMode;
      reasonSlot.MachineObservationState = mos;
      ((ReasonSlot)reasonSlot).SetDefaultReason (reason, 10.0, true, true);
      ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
    }

    void CreateStackLight (int start, int end, IMachineModule machineModule, IField field, StackLight color)
    {
      var cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (start));
      cncValue.End = T (end);
      cncValue.Value = color;
      ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue);
    }
  }
}
