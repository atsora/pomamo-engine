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
using Lemoine.Plugin.AutoReasonBreak;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the auto-reason break
  /// </summary>
  [TestFixture]
  public class AutoReasonBreak_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonBreak_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonBreak_UnitTest ()
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
    /// Test case 1:
    /// 
    /// <para>
    /// Feature: No reason <c>Break</c> applies if the machine is idle before the break time + margin
    /// </para>
    /// <para>
    /// Scenario: The machine stops at T2, the breaks starts at T5 with a margin of 2</para>
    /// <para>Given a break time between T5 and T7 and a margin of 2</para>
    /// <para>When the machine stops at T2</para>
    /// <para>Then the reason <c>Break</c> does not apply</para>
    /// 
    /// <code>
    ///                   --xx Break xx--
    /// ++++++++++++++++------------++++++++++++++++++++++
    /// 1               2 3 5       6 7 9                10
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </code>
    /// </summary>
    [Test]
    public void TestCase1 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T5
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 5));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T5-T7
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 7));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T7-T8
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (7, 8));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T2: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 2));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (2), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T2-T5: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (2, 5));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (2), T (5), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T5-T6: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (5), T (6), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T6-T7: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (6), T (7), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T7-T9: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 9));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (9), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (9);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (7));
            CheckNoReasonMachineAssociation ();
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
    /// Feature: A reason <c>Break</c> applies if the machine is only idle in the break period + margin
    /// </para>
    /// <para>
    /// Scenario: The machine stops at T2 and restarts at 6, the break period is T4-T5 and the margin is 2</para>
    /// <para>Given a break time between T4 and T5 and a margin of 2</para>
    /// <para>When the machine stops at T3 and restarts at T6</para>
    /// <para>Then the reason <c>Break</c> applies between T2 and T6</para>
    /// 
    /// <code>
    ///                   ----xx Break xx---
    /// ++++++++++++++++++++--------------++++++++++++++++++++++
    /// 1                 2 3 4         5 6 7                  8
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
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
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T4
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 4));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T4-T5
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (4, 5));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T5-T8
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 8));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T3: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (3), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T3-T4: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (3, 4));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (3), T (4), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T4-T5: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 5));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (4), T (5), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T5-T6: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (5), T (6), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T6-T8: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 8));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (6), T (8), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (8);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:03"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (5));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (3, 6), "ReasonBreak", "Break (auto)", 90.0);
            }
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
    /// Feature: The machine stops after the break start, but the idle period is not too long,
    /// it is shorter than the break duration, then apply the auto-reaon break
    /// </para>
    /// <para>
    /// Scenario: The break period is between T4 and T8 and the machine stops late at T7 but restarts after 4</para>
    /// <para>Given a break time between T4 and T8 (the duration is 4) and a margin of 2</para>
    /// <para>When the machine stops at T7 and restarts at T11 (the duration is 4)</para>
    /// <para>Then the reason <c>Break</c> applies</para>
    ///
    /// <code>
    ///                   --xx Break xx---
    /// ++++++++++++++++++++++++++++--------++++++++++++++++++++++
    /// 1                 2 4      7  8  10 11                   12
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
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
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T4
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 4));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T4-T8
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (4, 8));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T8-T12
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (8, 12));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T4: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 4));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (4), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T4-T7: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (4), T (7), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T7-T8: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 8));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (8), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T8-T11: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (8, 11));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (8), T (11), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T11-T12: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (11, 12));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (11), T (12), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (12);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (8));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (7, 12), "ReasonBreak", "Break (auto)", 90.0);
            }

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (12));
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }


    /// <summary>
    /// Test case 4:
    /// 
    /// <para>
    /// Feature: The machine stops after the break start, but the idle period is too long,
    /// it is longer than the break duration, then do not apply the auto-reaon break
    /// </para>
    /// <para>
    /// Scenario: The break period is between T4 and T8 and the machine stops late at T7 but restarts really late at T20</para>
    /// <para>Given a break time between T4 and T8 (the duration is 4) and a margin of 2</para>
    /// <para>When the machine stops at T7 and restarts at T20 (the duration is 13, longer than 4)</para>
    /// <para>Then the reason <c>Break</c> does not apply</para>
    /// 
    /// <code>
    ///                   --xx Break xx---
    /// ++++++++++++++++++++++++++++------------++++++++++++++++++++++
    /// 1                 2 4      7  8  10     20                   22
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </code>
    /// </summary>
    [Test]
    public void TestCase4 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T4
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 4));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T4-T8
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (4, 8));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T8-T22
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (8, 22));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T4: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 4));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T4-T7: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T7-T8: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 8));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T8-T20: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (8, 20));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T20-T22: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (20, 22));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (22);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (8));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (7, 12), "ReasonBreak", "Break (auto)", 90.0);
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }


    /// <summary>
    /// Test case 5:
    /// 
    /// <para>
    /// Feature: Consider a machine mode change to detect the auto-reason break
    /// </para>
    /// <para>
    /// Scenario: Apply the auto-reason break only from the latest machine mode change before the break period (but in the margin). The machine restarts before the break end</para>
    /// <para>Given a break time between T5 and T7 with a margin of 5</para>
    /// <para>When the machine mode changes at T3 and the machine restarts at T6</para>
    /// <para>Then the reason <c>Break</c> applies between T3 and T6</para>
    /// 
    /// <code>
    /// --------------------xx Break xx-------------------
    /// --//////////////------------++++++++++++++++++++++
    /// 1 2            3    5       6 7 8                9
    /// 
    /// -: MdiInactive
    /// /: MdiNoMotion
    /// +: running
    /// Margin: 5
    /// </code>
    /// </summary>
    [Test]
    public void TestCase5 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mdiNoMotion = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MdiNoMotion);
            var mdiInactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MdiInactive);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T5
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 5));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T5-T7
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 7));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T7-T9
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (7, 9));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T2: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 2));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T2-T3: NoMotion (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (2, 3));
              reasonSlot.MachineMode = mdiNoMotion;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T3-T5: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (3, 5));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T5-T6: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T6-T7: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T7-T9: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 9));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (9);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:05"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (7));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (3, 9), "ReasonBreak", "Break (auto)", 90.0);
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }


    /// <summary>
    /// Test case 6:
    /// 
    /// <para>
    /// Feature: Consider a machine mode change to detect the auto-reason break
    /// </para>
    /// <para>
    /// Scenario: Apply the auto-reason break only from the latest machine mode change before the break period (but in the margin). The machine restarts at the break end</para>
    /// <para>Given a break time between T5 and T6 with a margin of 5</para>
    /// <para>When the machine mode changes at T2 and the machine restarts at T6</para>
    /// <para>Then the reason <c>Break</c> applies between T2 and T6</para>
    /// 
    /// <code>
    /// --------------------xx Break xx--------------------
    /// ////////////////--------------++++++++++++++++++++++
    /// 1               2 3 5         6  8                9
    /// 
    /// -: MdiInactive
    /// /: MdiNoMotion
    /// +: running
    /// Margin: 5
    /// </code>
    /// </summary>
    [Test]
    public void TestCase6 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mdiNoMotion = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MdiNoMotion);
            var mdiInactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MdiInactive);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T5
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 5));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T5-T6
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 6));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T6-T9
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (6, 9));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T2: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 2));
              reasonSlot.MachineMode = mdiNoMotion;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T2-T5: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (2, 5));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T5-T6: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T6-T9: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 9));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (9);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:05"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (6));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (2, 7), "ReasonBreak", "Break (auto)", 90.0);
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }


    /// <summary>
    /// Test case 7:
    /// 
    /// <para>
    /// Feature: Consider a machine mode change to detect the auto-reason break
    /// </para>
    /// <para>
    /// Scenario: Apply the auto-reason break only from the latest machine mode change before the break period (but in the margin). The machine restarts after the break end but not too late after it</para>
    /// <para>Given a break time between T5 and T6 with a margin of 5</para>
    /// <para>When the machine mode changes at T2 and the machine restarts at T7 (the duration is 5, shorter than the break + margin duration)</para>
    /// <para>Then the reason <c>Break</c> applies between T2 and T7</para>
    ///  
    /// <code>
    /// --------------------xx Break xx-------------------
    /// ////////////////----------------++++++++++++++++++
    /// 1               2 3 5         6 7                9
    /// 
    /// -: MdiInactive
    /// /: MdiNoMotion
    /// +: running
    /// Margin: 5
    /// </code>
    /// </summary>
    [Test]
    public void TestCase7 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mdiNoMotion = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MdiNoMotion);
            var mdiInactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MdiInactive);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T5
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 5));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T5-T6
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 6));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T6-T9
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (6, 9));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T2: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 2));
              reasonSlot.MachineMode = mdiNoMotion;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T2-T5: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (2, 5));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T5-T6: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T6-T7: Inactive
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 7));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T7-T9: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 9));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (9);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:05"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (6));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (2, 7), "ReasonBreak", "Break (auto)", 90.0);
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 8:
    /// 
    /// <para>
    /// Feature: Consider a machine mode change to detect the auto-reason break
    /// </para>
    /// <para>
    /// Scenario: The machine mode changes before the break+margin</para>
    /// <para>Given a break time between T5 and T7 with a margin of 2</para>
    /// <para>When the machine mode changes at T2 and the machine restarts at T6</para>
    /// <para>Then the reason <c>Break</c> does not apply</para>
    /// 
    /// <code>
    ///                   --xx Break xx--
    /// ////////////////------------++++++++++++++++++++++
    /// 1               2 3 5       6 7 8                9
    /// 
    /// -: MdiInactive
    /// /: MdiNoMotion
    /// +: running
    /// Margin: 2
    /// </code>
    /// </summary>
    [Test]
    public void TestCase8 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mdiNoMotion = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MdiNoMotion);
            var mdiInactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MdiInactive);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T5
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 5));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T5-T7
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 7));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T7-T9
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (7, 9));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T2: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 2));
              reasonSlot.MachineMode = mdiNoMotion;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T2-T5: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (2, 5));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T5-T6: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = mdiInactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T6-T7: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T7-T9: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 9));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (9);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (7));
            CheckNoReasonMachineAssociation ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 9a: two different periods apply (all periods processed in the same time)
    /// 
    /// <para>
    /// Feature: Consider different periods when the auto-reason break may apply
    /// </para>
    /// <para>
    /// Scenario: two different periods apply (all periods processed in the same time)</para>
    /// <para>Given a break time between T4 and T8 with a margin of 2</para>
    /// <para>When there are two idle periods, one at T5-T6, one at T7-T11</para>
    /// <para>Then the reason <c>Break</c> applies twice</para>
    /// 
    /// <code>
    ///                   --xx Break xx---
    /// +++++++++++++++++++++++--+++--------++++++++++++++++++++++
    /// 1                 2 4 5 6  7  8  10 11                   12
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </code>
    /// </summary>
    [Test]
    public void TestCase9a ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T4
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 4));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T4-T8
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (4, 8));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T8-T12
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (8, 12));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T4: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 4));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T4-T5: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 5));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T5-T6: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T6-T7: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T7-T8: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 8));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T8-T11: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (8, 11));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T11-T12: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (11, 12));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (12);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (8));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (2, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (5, 12), "ReasonBreak", "Break (auto)", 90.0);
              ++i;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (7, 12), "ReasonBreak", "Break (auto)", 90.0);
            }

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (12));
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }


    /// <summary>
    /// Test case 9a: two different periods apply (all periods processed in different times)
    /// 
    /// <para>
    /// Feature: Consider different periods when the auto-reason break may apply
    /// </para>
    /// <para>
    /// Scenario: two different periods apply (all periods processed in different times)</para>
    /// <para>Given a break time between T4 and T8 with a margin of 2</para>
    /// <para>When there are two idle periods, one at T5-T6, one at T7-T11</para>
    /// <para>Then the reason <c>Break</c> applies twice</para>
    /// 
    /// <code>
    ///                   --xx Break xx---
    /// +++++++++++++++++++++++--+++--------++++++++++++++++++++++
    /// 1                 2 4 5 6  7  8  10 11                   12
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </code>
    /// </summary>
    [Test]
    public void TestCase9b ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T4
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 4));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T4-T8
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (4, 8));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T8-T12
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (8, 12));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T4: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 4));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T4-T5: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 5));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T5-T6: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 6));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T6-T7: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (6, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (7);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (7));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (5, 12), "ReasonBreak", "Break (auto)", 90.0);
            }

            { // T7-T8: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 8));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T8-T11: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (8, 11));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T11-T12: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (11, 12));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (12);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (8));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (2, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (5, 12), "ReasonBreak", "Break (auto)", 90.0);
              ++i;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (7, 12), "ReasonBreak", "Break (auto)", 90.0);
            }

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (12));
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case for story #158969446:
    /// 
    /// <para>
    /// Feature: The machine stops after the break start, the idle period is not too long,
    /// it is shorter than the break duration, but there is no period after it,
    /// then do not apply the reason yet
    /// </para>
    /// <para>
    /// Scenario: The break period is between T4 and T8 and the machine stops late at T7 and did not restart yet at T11</para>
    /// <para>Given a break time between T4 and T8 (the duration is 4) and a margin of 2</para>
    /// <para>When the machine stops at T7 and did not restart yet at T11 (the duration is 4)</para>
    /// <para>Then the reason <c>Break</c> did not apply and the auto-reason internal parameters did not move</para>
    ///
    /// <code>
    ///                   --xx Break xx---
    /// ++++++++++++++++++++++++++++---------
    /// 1                 2 4      7  8  10 11
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </code>
    /// </summary>
    [Test]
    public void TestCaseStory158969446 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            // Machine observation state slots
            { // Production: T1-T4
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 4));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T4-T8
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (4, 8));
              observationStateSlot.MachineObservationState = breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T8-T12
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (8, 12));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            { // T1-T4: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 4));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T4-T7: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T7-T8: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 8));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T8-T11: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (8, 11));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (11);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 9,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));

            autoReason.Initialize (machine, null);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (8));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (7, 12), "ReasonBreak", "Break (auto)", 90.0);
            }


            // Start again the machine
            { // T11-T12: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (11, 12));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // T12-T13: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (12, 13));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }
            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (13);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            autoReason.RunOnce (); // For zone 1
            CheckAutoReasonState (machine, dateTimeKey, T (13));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (7, 12), "ReasonBreak", "Break (auto)", 90.0);
            }

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (13));
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
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.SameMachineMode.PreviousMachineMode));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.SameMachineMode.NextMachineMode));
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }
  }
}
