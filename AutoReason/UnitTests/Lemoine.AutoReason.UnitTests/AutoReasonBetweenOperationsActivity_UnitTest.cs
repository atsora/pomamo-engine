// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonBetweenOperationsActivity;
using Pulse.Extensions.Extension;
using Pulse.Extensions.Database;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonBetweenOperationsActivity
  /// </summary>
  [TestFixture]
  public class AutoReasonBetweenOperationsActivity_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonBetweenOperationsActivity_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonBetweenOperationsActivity_UnitTest ()
      : base (DateTime.Today.AddDays (-1))
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
      Assert.That (autoReasonState.Value, Is.EqualTo (v));
    }

    void CheckNoReasonMachineAssociation ()
    {
      var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
        .FindAll ();
      Assert.That (reasonMachineAssociations, Is.Empty);
    }

    void CheckReasonMachineAssociation (IReasonMachineAssociation reasonMachineAssociation, IMonitoredMachine machine, UtcDateTimeRange range, string translationKey, string details, double score)
    {
      Assert.Multiple (() => {
        Assert.That (new UtcDateTimeRange (reasonMachineAssociation.Begin, reasonMachineAssociation.End), Is.EqualTo (range));
        Assert.That (reasonMachineAssociation.ReasonSource, Is.EqualTo (ReasonSource.Auto));
        Assert.That (reasonMachineAssociation.Machine, Is.EqualTo (machine));
        Assert.That (reasonMachineAssociation.Reason.TranslationKey, Is.EqualTo (translationKey));
        Assert.That (reasonMachineAssociation.ReasonDetails, Is.EqualTo (details));
        Assert.That (reasonMachineAssociation.ReasonScore, Is.EqualTo (score));
      });
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
    /// ++++++++++++++++++++-----+++------++++++++++++++++++++++
    /// 1                 2 3    4 5     6 7                   8
    /// 
    /// -: not running
    /// +: running
    /// Margin: 1
    /// </code>
    /// </summary>
    [Test]
    public void TestCase1a ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
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
            var plugin = new Lemoine.Plugin.AutoReasonBetweenOperationsActivity.Plugin ();
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
            autoReason.Initialize (machine, null);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.That (reasonMachineAssociations, Has.Count.EqualTo (1));
              CheckReasonMachineAssociation (reasonMachineAssociations[0],
                machine, R (3, 6), "ReasonBetweenOperationsActivity", "Between  and ", 60.0);
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 1b: with a long margin
    /// 
    /// <para>
    /// Feature: Apply an auto-reason when the machine stops between two operations and there is no activity between them
    /// </para>
    /// <para>
    /// Scenario: The machine stops just after the first operation and restarts just before the second operation, the margin is longer than the time between the two operations</para>
    /// <para>Given Op1 between T1 and T2, Op2 between T5 and T6 and a margin of 20</para>
    /// <para>When the machine stops at T3 and restarts at T4</para>
    /// <para>Then the auto-reason applies between T3 and T4</para>
    /// 
    /// <code>
    /// xxxxxxx Op1 xxxxxx------------------xxxxxxxx Op2 xxxxxxx
    /// ++++++++++++++++++++------+++------++++++++++++++++++++++
    /// 1                 2 3     4 5     6 7                  8
    /// 
    /// -: not running
    /// +: running
    /// Margin: 20
    /// </code>
    /// </summary>
    [Test]
    public void TestCase1b ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
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
            var plugin = new Lemoine.Plugin.AutoReasonBetweenOperationsActivity.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 60.0,
  ""ManualScore"": 100.0,
  ""Margin"": ""0:00:20""
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            autoReason.Initialize (machine, null);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.That (reasonMachineAssociations, Has.Count.EqualTo (1));
              CheckReasonMachineAssociation (reasonMachineAssociations[0],
                machine, R (2, 7), "ReasonBetweenOperationsActivity", "Between  and ", 60.0);
            }
          } finally {
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
    /// <para>When the machine stops at T3 and restarts finally at T6</para>
    /// <para>Then the auto-reason does not apply since there is no activity</para>
    /// 
    /// <code>
    /// xxxxxx Op1 xxxxxxx--        --xxxxxx Op2 xxxxxxxxx
    /// ++++++++++++++++++++--------++++++++++++++++++++++
    /// 1                 2 3       6 7                  8
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
            Assert.That (machine, Is.Not.Null);
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
            { // T4-T5: Inactive
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 5));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
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
            var plugin = new Lemoine.Plugin.AutoReasonBetweenOperationsActivity.Plugin ();
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
            CheckNoReasonMachineAssociation ();

            autoReason.RunOnce ();
            CheckNoReasonMachineAssociation ();

            autoReason.RunOnce ();
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
    /// Feature: Apply an auto-reason when the machine stops between two operations and there is activity between them
    /// </para>
    /// <para>
    /// Scenario: The machine stops before the end of the first operation and restarts after the start of the second operation</para>
    /// <para>Given Op1 between T1 and T3, Op2 between T6 and T8 and a margin of 20</para>
    /// <para>When the machine stops at T2 and restarts at T4</para>
    /// <para>When the machine stops at T5 and restarts at T6</para>
    /// <para>Then the auto-reason applies between T2 and T7</para>
    /// 
    /// <code>
    /// xxxxxxx Op1 xxxxx             xxxxxxxx Op2 xxxxxxx
    /// +++++++++++++++-------+++-------+++++++++++++++++++
    /// 1             2 3     4 5     6 7                 8
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
            Assert.That (machine, Is.Not.Null);
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
            { // Op2: T6-T8
              var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, op2, null, null, null, null, null, null, R (6, 8));
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
            { // T2-T4: Inactive
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (2, 4));
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
            { // T5-T7: Inactive
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 7));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = attended;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T7-T8: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 8));
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
            var plugin = new Lemoine.Plugin.AutoReasonBetweenOperationsActivity.Plugin ();
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
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.That (reasonMachineAssociations, Has.Count.EqualTo (1));
              CheckReasonMachineAssociation (reasonMachineAssociations[0],
                machine, R (2, 7), "ReasonBetweenOperationsActivity", "Between  and ", 60.0);
            }
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
