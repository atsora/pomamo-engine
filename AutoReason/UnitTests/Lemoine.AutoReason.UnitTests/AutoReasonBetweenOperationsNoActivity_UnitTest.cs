// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonBetweenOperationsNoActivity;
using Pulse.Extensions.Extension;
using Pulse.Extensions.Database;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonBetweenOperationsNoActivity
  /// </summary>
  [TestFixture]
  public class AutoReasonBetweenOperationsNoActivity_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonBetweenOperationsNoActivity_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonBetweenOperationsNoActivity_UnitTest ()
      : base (DateTime.Today.AddMonths (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    void CreateAutoReasonState (IMonitoredMachine machine, string key, object v)
    {
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, key);
      autoReasonState.Value = v;
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);
    }

    void CheckAutoReasonState (IMonitoredMachine machine, string key, object v)
    {
      var autoReasonState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (machine, key);
      Assert.AreEqual (v, autoReasonState.Value);
    }

    void CheckNoReasonMachineAssociation ()
    {
      var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
        .FindAll ();
      Assert.AreEqual (0, reasonMachineAssociations.Count);
    }

    void CheckReasonMachineAssociation (IReasonMachineAssociation reasonMachineAssociation, IMonitoredMachine machine, UtcDateTimeRange range, string translationKey, string details, double score)
    {
      Assert.AreEqual (range, reasonMachineAssociation.Range);
      Assert.AreEqual (ReasonSource.Auto, reasonMachineAssociation.ReasonSource);
      Assert.AreEqual (machine, reasonMachineAssociation.Machine);
      Assert.AreEqual (translationKey, reasonMachineAssociation.Reason.TranslationKey);
      Assert.AreEqual (details, reasonMachineAssociation.ReasonDetails);
      Assert.AreEqual (score, reasonMachineAssociation.ReasonScore);
    }

    /// <summary>
    /// Test case 1a:
    /// 
    /// <para>
    /// Feature: Apply an auto-reason when the machine stops between two operations and there is no activity between them
    /// </para>
    /// <para>
    /// Scenario: The machine stops just after the first operation (before the margin time) and restarts just before the second operation (after the margin time)</para>
    /// <para>Given Op1 between T1 and T2, Op2 between T5 and T6 and a margin of 1</para>
    /// <para>When the machine stops at T3 and restarts at T4</para>
    /// <para>Then the auto-reason applies between T3 and T4</para>
    /// 
    /// <code>
    /// xxxxxxx Op1 xxxxxx--              --xxxxxxxx Op2 xxxxxxx
    /// ++++++++++++++++++++--------------++++++++++++++++++++++
    /// 1                 2 3             4 5                  6
    /// 
    /// -: not running
    /// +: running
    /// Margin: 1
    /// </code>
    /// </summary>
    [Test]
    public void TestCase1a ()
    {
      TestCase1 (1, true);
    }

    /// <summary>
    /// Test case 1b:
    /// 
    /// <para>
    /// Feature: Apply an auto-reason when the machine stops between two operations and there is no activity between them
    /// </para>
    /// <para>
    /// Scenario: The machine stops just after the first operation and restarts just before the second operation, the margin is longer than the time between the two operations</para>
    /// <para>Given Op1 between T1 and T2, Op2 between T5 and T6 and a margin of 20</para>
    /// <para>When the machine stops at T3 and restarts at T4</para>
    /// <para>Then no auto-reason applies</para>
    /// 
    /// <code>
    /// xxxxxxx Op1 xxxxxx------------------xxxxxxxx Op2 xxxxxxx
    /// ++++++++++++++++++++--------------++++++++++++++++++++++
    /// 1                 2 3             4 5                  6
    /// 
    /// -: not running
    /// +: running
    /// Margin: 20
    /// </code>
    /// </summary>
    [Test]
    public void TestCase1b ()
    {
      TestCase1 (20, false);
    }

    /// <summary>
    /// Test case 1:
    /// 
    /// <code>
    /// xxxxxxx Op1 xxxxxx--..............--xxxxxxxx Op2 xxxxxxx
    /// ++++++++++++++++++++--------------++++++++++++++++++++++
    /// 1                 2 3             4 5                  6
    /// 
    /// -: not running
    /// +: running
    /// Margin: to be determined
    /// </code>
    /// </summary>
    /// <param name="margin">margin in seconds (0 to 59)</param>
    /// <param name="apply">Apply the auto-reason</param>
    void TestCase1 (int margin, bool apply)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Operations
            var op1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
            var op2 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (2);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);

            // Operation slots
            { // Op1: T1-T2
              var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, op1, null, null, null, null, null, null, R (1, 2));
              ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
            }
            { // Op2: T5-T6
              var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, op2, null, null, null, null, null, null, R (5, 6));
              ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
            }

            // ReasonSlots
            { // T1-T3: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T3-T4: Inactive
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (3, 4));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T4-T6: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 6));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = attended;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (6);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBetweenOperationsNoActivity.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            var marginString = margin.ToString ("00");
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 60.0,
  ""ManualScore"": 100.0,
  ""Margin"": ""0:00:" + marginString + @"""
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();

            if (apply) {
              CheckAutoReasonState (machine, dateTimeKey, T (1));
              CheckNoReasonMachineAssociation ();

              autoReason.RunOnce ();
              CheckAutoReasonState (machine, dateTimeKey, T (2));
              CheckNoReasonMachineAssociation ();

              autoReason.RunOnce ();
              CheckAutoReasonState (machine, dateTimeKey, T (6));
              { // Test ReasonMachineAssociation
                var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                  .FindAll ();
                Assert.AreEqual (1, reasonMachineAssociations.Count);
                int i = 0;
                CheckReasonMachineAssociation (reasonMachineAssociations[i],
                  machine, R (3, 4), "ReasonBetweenOperationsNoActivity", "Between  and ", 60.0);
              }
            }
            else {
              CheckAutoReasonState (machine, dateTimeKey, T (1));
              CheckNoReasonMachineAssociation ();

              autoReason.RunOnce ();
              CheckAutoReasonState (machine, dateTimeKey, T (2));
              CheckNoReasonMachineAssociation ();

              autoReason.RunOnce ();
              CheckNoReasonMachineAssociation ();
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }


    /// <summary>
    /// Test case 2:
    /// 
    /// <para>
    /// Feature: Apply an auto-reason when the machine stops between two operations and there is no activity between them
    /// </para>
    /// <para>
    /// Scenario: The machine stops just after the first operation (before the margin time) and restarts just before the second operation (after the margin time) but there is a small activity between them</para>
    /// <para>Given Op1 between T1 and T2, Op2 between T7 and T8 and a margin of 1</para>
    /// <para>When the machine stops at T3, restarts at T4 to stop at T5 and restarts finally at T6</para>
    /// <para>Then the auto-reason does not apply</para>
    /// 
    /// <code>
    /// xxxxxx Op1 xxxxxxx--        --xxxxxx Op2 xxxxxxxxx
    /// ++++++++++++++++++++---+++--++++++++++++++++++++++
    /// 1                 2 3  4 5  6 7                  8
    /// 
    /// -: not running
    /// +: running
    /// Margin: 1
    /// </code>
    /// </summary>
    [Test]
    public void TestCase2 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Operations
            var op1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
            var op2 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (2);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);

            // Operation slots
            { // Op1: T1-T2
              var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, op1, null, null, null, null, null, null, R (1, 2));
              ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
            }
            { // Op2: T7-T8
              var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, op2, null, null, null, null, null, null, R (7, 8));
              ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
            }

            // ReasonSlots
            { // T1-T3: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T3-T4: Inactive
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (3, 4));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T4-T5: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 5));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T5-T6: Inactive
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T6-T8: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 8));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = attended;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (8);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBetweenOperationsNoActivity.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 60.0,
  ""ManualScore"": 100.0,
  ""Margin"": ""0:00:01""
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (1));
            CheckNoReasonMachineAssociation ();

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (2));
            CheckNoReasonMachineAssociation ();

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (8));
            CheckNoReasonMachineAssociation ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 3:
    /// 
    /// <para>
    /// Feature: Apply an auto-reason when the machine stops between two operations and there is no activity between them
    /// </para>
    /// <para>
    /// Scenario: The machine stops before the end of the first operation and restarts after the start of the second operation</para>
    /// <para>Given Op1 between T1 and T3, Op2 between T4 and T6 and a margin of 20</para>
    /// <para>When the machine stops at T2 and restarts at T5</para>
    /// <para>Then no auto-reason applies</para>
    /// 
    /// <code>
    /// xxxxxxx Op1 xxxxx             xxxxxxxx Op2 xxxxxxx
    /// +++++++++++++++-----------------+++++++++++++++++++
    /// 1             2 3             4 5                6
    /// 
    /// -: not running
    /// +: running
    /// Margin: 20
    /// </code>
    /// </summary>
    [Test]
    public void TestCase3 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Operations
            var op1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
            var op2 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (2);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);

            // Operation slots
            { // Op1: T1-T3
              var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, op1, null, null, null, null, null, null, R (1, 3));
              ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
            }
            { // Op2: T4-T6
              var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, op2, null, null, null, null, null, null, R (4, 6));
              ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
            }

            // ReasonSlots
            { // T1-T2: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 2));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T2-T5: Inactive
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (2, 5));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T5-T6: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = attended;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (6);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBetweenOperationsNoActivity.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 60.0,
  ""ManualScore"": 100.0,
  ""Margin"": ""0:00:05""
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (1));
            CheckNoReasonMachineAssociation ();

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (3));
            CheckNoReasonMachineAssociation ();

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (6));
            CheckNoReasonMachineAssociation ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    [SetUp]
    public void Setup ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.SameMachineMode.NextMachineMode));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.SameMachineMode.PreviousMachineMode));
      Lemoine.Extensions.ExtensionManager.Add (typeof (OperationDetectionStatusExtension));
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
