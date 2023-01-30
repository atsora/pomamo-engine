// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonShiftNoProduction;
using System.Linq;
using Pulse.Extensions.Extension;
using Pulse.Extensions.Database;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonShiftProduction
  /// </summary>
  [TestFixture]
  public class AutoReasonShiftNoProduction_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonShiftNoProduction_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonShiftNoProduction_UnitTest () : base (DateTime.Today.AddDays (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with two kinds of shift
    /// * a first shift slot from T0 to T8 (shift A)
    /// * a second shift slot from T8 to T16 (shift B)
    /// * a third shift slot from T16 to T24 (shift A)
    /// Associated fact slots
    /// * active from T0 to T8 (running)
    /// * inactive from T8 to T16 (not running)
    /// * active from T16 to T24 (running)
    /// => auto reason "shift no activity" applied from T8 to T16
    /// </summary>
    [Test]
    public void ShiftNoProduction_SimpleDetection ()
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
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

            // Shift slots
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);
            CreateMachineObservationState (16, 24, machine, attended, shiftA);

            // ReasonSlots
            CreateFact (0, 8, machine, active);
            CreateFact (8, 16, machine, inactive);
            CreateFact (16, 24, machine, active);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
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
              Assert.AreEqual (1, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (8), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (16), reasonAssociation.End.Value, "wrong end 1");
              Assert.AreEqual ("Machine mode: MachineModeInactive", reasonAssociation.ReasonDetails, "wrong reason detail");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with two different shifts
    /// * a first shift slot from T0 to T8 (shift A)
    /// * a second shift slot from T8 to T16 (shift B)
    /// * a third shift slot from T16 to T24 (shift A)
    /// Associated fact slots
    /// * active from T0 to T8 (running)
    /// * active from T16 to T124 (running)
    /// => auto reason "shift no activity" applied from T8 to T16
    /// </summary>
    [Test]
    public void ShiftNoProduction_DetectionWithNoFacts ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes / Machine observation states / Shifts
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

            // Shift slots
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);
            CreateMachineObservationState (16, 24, machine, attended, shiftA);

            // ReasonSlots
            CreateFact (0, 8, machine, active);
            CreateFact (16, 24, machine, active);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
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
              Assert.AreEqual (1, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (8), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (16), reasonAssociation.End.Value, "wrong end 1");
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with two different shifts
    /// * a first shift slot from T0 to T8 (shift A)
    /// * a second shift slot from T8 to T16 (shift B)
    /// * a third shift slot from T16 to T24 (shift A)
    /// Associated fact slots
    /// * active from T0 to T8 (running)
    /// * inactive from T9 to T12 (not running)
    /// * inactive from T13 to T14 (not running)
    /// * active from T16 to T24 (running)
    /// => auto reason "shift no activity" applied from T8 to T16
    /// </summary>
    [Test]
    public void ShiftNoProduction_DetectionWithFragments ()
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
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
            var shiftC = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);

            // Shift slots
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);
            CreateMachineObservationState (16, 24, machine, attended, shiftA);

            // ReasonSlots
            CreateFact (0, 8, machine, active);
            CreateFact (9, 12, machine, inactive);
            CreateFact (13, 14, machine, inactive);
            CreateFact (16, 24, machine, active);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
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
              Assert.AreEqual (1, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (8), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (16), reasonAssociation.End.Value, "wrong end 1");
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }
    /// <summary>
    /// Test case with two different shifts
    /// * a first shift slot from T0 to T8 (shift A)
    /// * a second shift slot from T8 to T16 (shift B)
    /// * a third shift slot from T16 to T24 (shift A)
    /// Associated fact slots
    /// * active from T0 to T7 (running)
    /// * inactive from T7 to T12 (not running)
    /// * inactive from T12 to T16 (running)
    /// * active from T16 to T24 (running)
    /// => auto reason "shift no activity" applied from T8 to T16
    /// </summary>
    [Test]
    public void ShiftNoProduction_DetectionWithFactsAcrossShifts ()
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
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
            var shiftC = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);

            // Shift slots
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);
            CreateMachineObservationState (16, 24, machine, attended, shiftA);

            // ReasonSlots
            CreateFact (0, 7, machine, active);
            CreateFact (7, 12, machine, inactive);
            CreateFact (12, 16, machine, active);
            CreateFact (16, 24, machine, active);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
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
              Assert.AreEqual (0, reasonAssociations.Count, "wrong number of auto reason created");
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }
    /// <summary>
    /// Test case with two different shifts
    /// * a first shift slot from T0 to T8 (shift A)
    /// * a second shift slot from T8 to T16 (shift B)
    /// * a third shift slot from T16 to T24 (shift A)
    /// Associated reason slots
    /// * active from T0 to T8 (running)
    /// * inactive from T9 to T12 (not running)
    /// * off from T13 to T14 (not running)
    /// * active from T16 to T124 (running)
    /// => auto reason "shift no activity" applied from T8 to T16
    /// </summary>
    [Test]
    public void ShiftNoProduction_TwoKindsOfIdle ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes / Machine observation states / Shifts
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var off = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Off);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

            // Machine observation states
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);
            CreateMachineObservationState (16, 24, machine, attended, shiftA);

            // Facts
            CreateFact (0, 8, machine, active);
            CreateFact (9, 12, machine, inactive);
            CreateFact (13, 14, machine, off);
            CreateFact (16, 24, machine, active);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that no autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (0, reasonAssociations.Count, "wrong number of auto reason created");
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with two consecutives shifts without production
    /// * a first shift slot from T0 to T8 (shift A) with production
    /// * a second shift slot from T8 to T16 (shift B) without production
    /// * a third shift slot from T16 to T24 (shift A) without production
    /// Associated reason slots
    /// * active from T0 to T8 (running)
    /// * inactive from T8 to T16 (not running)
    /// * active from T16 to T24 (running)
    /// => auto reason "shift no activity" applied from T8 to T16
    /// </summary>
    [Test]
    public void ShiftNoProduction_TwoShiftsWithoutProduction ()
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
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

            // Shift slots
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftB);
            CreateMachineObservationState (16, 20, machine, attended, shiftA);
            CreateMachineObservationState (20, 24, machine, attended, shiftB);

            // ReasonSlots
            CreateFact (0, 8, machine, active);
            CreateFact (8, 16, machine, inactive);
            CreateFact (16, 20, machine, inactive);
            CreateFact (20, 24, machine, active);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
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
              Assert.AreEqual (2, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (8), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (16), reasonAssociation.End.Value, "wrong end 1");
              reasonAssociation = reasonAssociations[1];
              Assert.AreEqual (T (16), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (20), reasonAssociation.End.Value, "wrong end 1");
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
      var plugin = new Lemoine.Plugin.AutoReasonShiftNoProduction.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
        "{ \"ReasonScore\": 60.0," +
        "\"ManualScore\": 100.0}"
      );

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, dateTimeKey);
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
    }

    void CreateFact (int start, int end, IMonitoredMachine machine, IMachineMode machineMode)
    {
      var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (start), T (end), machineMode);
      ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
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
      Lemoine.Extensions.ExtensionManager.Add (typeof (OperationDetectionStatusExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CycleIsProduction.LastProductionEnd));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CycleIsProduction.NextProductionStart));
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }

    public class OperationDetectionStatusExtension
      : Lemoine.Extensions.NotConfigurableExtension
      , IOperationDetectionStatusExtension
    {
      ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusExtension).FullName);

      public int OperationDetectionStatusPriority
      {
        get
        {
          return 1;
        }
      }

      public DateTime? GetOperationDetectionDateTime ()
      {
        return DateTime.UtcNow;
      }

      public bool Initialize (IMachine machine)
      {
        return true;
      }
    }
  }
}
