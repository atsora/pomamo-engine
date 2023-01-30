// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Plugin.AutoReasonShiftStart;
using Lemoine.GDBPersistentClasses;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonShiftStart
  /// </summary>
  [TestFixture]
  public class AutoReasonShiftStart_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonShiftEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonShiftStart_UnitTest () : base (DateTime.Today.AddDays (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with two different shifts
    /// * a first shift from T0 to T8
    /// * a second shift from T8 to T16 (same kind of shift)
    /// * a third shift from T16 to T24 (different kind of shift)
    /// Configuration of the plugin
    /// * maximum duration of a late start is 5
    /// * detection margin is 3
    /// </summary>
    [Test]
    public void ShiftStart_LateStartDetected ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes / Machine observation states / Shifts
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var shiftA = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
            var shiftB = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

            // Shift slots
            CreateMachineObservationState (0, 8, machine, attended, shiftA);
            CreateMachineObservationState (8, 16, machine, attended, shiftA);
            CreateMachineObservationState (16, 24, machine, attended, shiftB);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 5, 3);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            
            // Check that an autoreason appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ();
              Assert.AreEqual (2, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (16), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (16 + 5), reasonAssociation.End.Value, "wrong end 1");
              reasonAssociation = reasonAssociations[1];
              Assert.AreEqual (T (24), reasonAssociation.Begin.Value, "wrong start 2");
              Assert.AreEqual (T (24 + 5), reasonAssociation.End.Value, "wrong end 2");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine, int maxDurationForShiftStart, int detectionMargin)
    {
      var duration = TimeSpan.FromSeconds (maxDurationForShiftStart);
      string limitText = string.Format ("{0:D2}:{1:D2}:{2:D2}", duration.Hours, duration.Minutes, duration.Seconds);

      duration = TimeSpan.FromSeconds (detectionMargin);
      string marginText = string.Format ("{0:D2}:{1:D2}:{2:D2}", duration.Hours, duration.Minutes, duration.Seconds);

      var plugin = new Lemoine.Plugin.AutoReasonShiftStart.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.TestMode = true;
      autoReason.SetTestConfiguration (
  "{ \"ReasonScore\": 60.0," +
  "\"ManualScore\": 100.0," +
  "\"MaxDuration\": \"" + limitText + "\"," +
  "\"DetectionMargin\": \"" + marginText + "\"}"
);

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, autoReason.GetKey ("DateTime"));
      autoReasonState.Value = T (0);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
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
