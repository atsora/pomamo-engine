// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonShiftEnd;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonShiftEnd
  /// </summary>
  [TestFixture]
  public class AutoReasonShiftEnd_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonShiftEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonShiftEnd_UnitTest () : base (DateTime.Today.AddDays (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with two different shifts
    /// * a first shift from T0 to T8
    /// * a second shift from T8 to T16
    /// * a third shift from T16 to T24
    /// Configuration of the plugin
    /// * maximum duration of an early end is 3
    /// Associated reason slots
    /// * running from T1 to T6
    /// * not running from T4 to T9
    /// * running from T9 to T15
    /// * not running from T15 to T16
    /// * running from T16 to T24
    /// => auto reason "early end" applied from T8 to T8 (the beginning will move)
    /// => auto reason "early end" applied from T16 to T16 (the beginning will move)
    /// </summary>
    [Test]
    public void ShiftEnd_EarlyEndDetected ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes / Machine observation states / Shifts
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

            // Shift slots
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);
            CreateMachineObservationState (16, 24, machine, attended, shiftA);

            // ReasonSlots
            CreateReasonSlot (1, 6, machine, active, attended, reasonMotion);
            CreateReasonSlot (4, 9, machine, inactive, attended, reasonUnanswered);
            CreateReasonSlot (9, 15, machine, active, attended, reasonMotion);
            CreateReasonSlot (15, 16, machine, inactive, attended, reasonUnanswered);
            CreateReasonSlot (16, 24, machine, active, attended, reasonMotion);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 3);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that an autoreason appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (3, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (5), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (8), reasonAssociation.End.Value, "wrong end 1");
              reasonAssociation = reasonAssociations[1];
              Assert.AreEqual (T (13), reasonAssociation.Begin.Value, "wrong start 2");
              Assert.AreEqual (T (16), reasonAssociation.End.Value, "wrong end 2");
              reasonAssociation = reasonAssociations[2];
              Assert.AreEqual (T (21), reasonAssociation.Begin.Value, "wrong start 3");
              Assert.AreEqual (T (24), reasonAssociation.End.Value, "wrong end 3");

            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with two different shifts
    /// * a first shift from T0 to T8
    /// * a second shift from T8 to T16
    /// Configuration of the plugin
    /// * maximum duration of an early end is 1
    /// Associated reason slots
    /// * running from T1 to T6
    /// * not running from T6 to T9
    /// * running from T9 to T15
    /// => no auto reason "early end" applied
    /// </summary>
    /*[Test] // Cannot test because of the BOUCHON
    public void ShiftEnd_TooLongEarlyEnd ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes / Machine observation states / Shifts
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

            // Shift slots
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);

            // ReasonSlots
            CreateReasonSlot (1, 6, machine, active, attended, reasonMotion);
            CreateReasonSlot (6, 9, machine, inactive, attended, reasonUnanswered);
            CreateReasonSlot (9, 15, machine, active, attended, reasonMotion);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 1);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that an autoreason appeared
            {
              var reasonAssociations = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO.FindAll ();
              Assert.IsEmpty (reasonAssociations, "wrong number of auto reason created");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }*/

    /// <summary>
    /// Test case with two different shifts, the first one comprising a break
    /// * a first shift from T0 to T8, splitted this way
    ///   * attended from T0 to T4
    ///   * break from T4 to T6
    ///   * attended from T6 to T8
    /// * a second shift from T8 to T16
    /// Configuration of the plugin
    /// * maximum duration of an early end is 3
    /// Associated reason slots
    /// * running from T1 to T5
    /// * not running from T3 to T7
    /// * running from T7 to T16
    /// => no auto reason "early end" applied from T3 to T4 since this is in the same shift
    /// </summary>
    /*[Test] // Cannot test because of the BOUCHON
    public void ShiftEnd_BreakInSameShift ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes / Machine observation states / Shifts
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var mosBreak = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

            // Shift slots
            CreateMachineObservationState (0, 4, machine, attended, shiftA);
            CreateMachineObservationState (4, 6, machine, mosBreak, shiftA);
            CreateMachineObservationState (6, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);

            // ReasonSlots
            CreateReasonSlot (1, 5, machine, active, attended, reasonMotion);
            CreateReasonSlot (3, 7, machine, inactive, attended, reasonUnanswered);
            CreateReasonSlot (7, 16, machine, active, attended, reasonMotion);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 3);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that an autoreason appeared
            {
              var reasonAssociations = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO.FindAll ();
              Assert.IsEmpty (reasonAssociations, "wrong number of auto reason created");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }*/

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine, int maxDurationForShiftEnd)
    {
      var duration = TimeSpan.FromSeconds (maxDurationForShiftEnd);
      string limitText = string.Format ("{0:D2}:{1:D2}:{2:D2}", duration.Hours, duration.Minutes, duration.Seconds);

      var plugin = new Lemoine.Plugin.AutoReasonShiftEnd.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
        "{ \"ReasonScore\": 60.0," +
        "\"ManualScore\": 100.0," +
        "\"MaxDuration\": \"" + limitText + "\"}"
      );

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, dateTimeKey);
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      autoReason.TestMode = true;
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

    void CreateMachineObservationState (int start, int end, IMachine machine, IMachineObservationState mos, IShift shift)
    {
      var association = ModelDAOHelper.ModelFactory.CreateMachineObservationStateAssociation (machine, mos, R (start, end));
      association.Shift = shift;
      association.Apply ();
    }

    [SetUp]
    public void Setup ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CycleIsProduction.LastProductionEnd));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CycleIsProduction.NextProductionStart));
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }
  }
}
