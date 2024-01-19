// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonRedStackLight;
using Lemoine.Core.Log;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonRedStackLight
  /// </summary>
  [TestFixture]
  public class AutoReasonRedStackLight_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonRedStackLight_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonRedStackLight_UnitTest () : base (DateTime.Today.AddMonths (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with several reason slots
    /// * running from T1 to T6
    /// * not running from T6 to T9
    /// * not running from T9 to T10
    /// * not running from T10 to T13
    /// * running from T13 to T15
    /// Configured stack lights
    /// * red from T2 to T4
    /// * yellow from T7 to T8
    /// * red from T9 to T11
    /// Associated reason slots "red stack light"
    /// => from T2 to no end
    /// => from T9 to no end
    /// </summary>
    [Test]
    public void ShiftStart_RedStackLightDetected ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            ShiftStart_RedStackLightDetected (T (20));
            
            // Check that 2 autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (2), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (2)), "wrong start for the first reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the first reason should have no end");
                Assert.That (reasonAssociation.ReasonDetails, Is.EqualTo ("no alarms"), "wrong detail for the first reason");
              });
              reasonAssociation = reasonAssociations[1];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (9)), "wrong start for the second reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the second reason should have no end");
                Assert.That (reasonAssociation.ReasonDetails, Is.EqualTo ("no alarms"), "wrong detail for the second reason");
              });
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Same test than before but the alarms have not been computed yet (acquisition state is different)
    /// </summary>
    [Test]
    public void ShiftStart_RedStackLightDetectedAlarmsLate ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            ShiftStart_RedStackLightDetected (T (1));

            // Check that no autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ();
              Assert.That (reasonAssociations, Is.Empty, "wrong number of auto reason created");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Same test than before but the alarms have not been computed since a very long time (acquisition state is different)
    /// </summary>
    [Test]
    public void ShiftStart_RedStackLightDetectedAlarmsVeryLate ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            ShiftStart_RedStackLightDetected (T (-7*24*60*60 - 1)); // Late of at least one weak (converted into seconds here)

            // Check that no autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (2), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (2)), "wrong start for the first reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the first reason should have no end");
                Assert.That (reasonAssociation.ReasonDetails, Is.EqualTo (""), "wrong detail for the first reason");
              });
              reasonAssociation = reasonAssociations[1];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (9)), "wrong start for the second reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the second reason should have no end");
                Assert.That (reasonAssociation.ReasonDetails, Is.EqualTo (""), "wrong detail for the second reason");
              });
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    void ShiftStart_RedStackLightDetected (DateTime alarmAcquisition)
    {
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

      acquisitionState.DateTime = alarmAcquisition;
      ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent (acquisitionState);
      ModelDAOHelper.DAOFactory.Flush ();

      // Machine modes / Machine observation states / Shifts
      var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
      var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
      var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
      var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
      var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);

      // ReasonSlots
      CreateReasonSlot (1, 6, machine, active, attended, reasonMotion);
      CreateReasonSlot (6, 9, machine, inactive, attended, reasonUnanswered);
      CreateReasonSlot (9, 10, machine, inactive, attended, reasonUnanswered);
      CreateReasonSlot (10, 13, machine, inactive, attended, reasonUnanswered);
      CreateReasonSlot (13, 15, machine, active, attended, reasonMotion);

      // Stacklights
      var stackLightField = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.StackLight);
      CreateStackLight (2, 4, mamo, stackLightField, StackLight.RedOn);
      CreateStackLight (7, 8, mamo, stackLightField, StackLight.YellowOn);
      CreateStackLight (9, 11, mamo, stackLightField, StackLight.RedOn);


      // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

      // Plugin
      var autoReason = GetAutoReasonExtension (machine);

      // Run the analyze
      autoReason.RunOnce ();
      autoReason.RunOnce ();
      autoReason.RunOnce ();
      autoReason.RunOnce ();
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine)
    {
      var plugin = new Lemoine.Plugin.AutoReasonRedStackLight.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
        "{ \"ReasonScore\": 60.0," +
        "\"ManualScore\": 100.0," +
        "\"WriteAllAlarms\": false}"
      );

      // Initialize the date
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

    void CreateStackLight(int start, int end, IMachineModule machineModule, IField field, StackLight color)
    {
      var cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (start));
      cncValue.End = T (end);
      cncValue.Value = color;
      ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue);
    }
  }
}
