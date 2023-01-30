// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonWeekend;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the auto-reason break
  /// </summary>
  [TestFixture]
  public class AutoReasonWeekend_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonWeekend_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonWeekend_UnitTest ()
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

    /// <summary>
    /// Test case 1:
    /// 
    /// non running period covering the whole weekend
    /// => Reason "Weekend" applied on the full length of the weekend
    /// 
    ///                   | Weekend |
    /// ++++++++++++++++----------------+++++++++++++++++++
    /// 1              4  5         7   9                20
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </summary>
    [Test]
    public void TestCase1 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
            machine.Name = machine.Code = "j'adore les cerises";
            machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById ((int)MachineMonitoringTypeId.Monitored);
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
            
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var weekend = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Weekend);

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
            { // Weekend: T5-T7
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 7));
              observationStateSlot.MachineObservationState = weekend;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T7-T20
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (7, 20));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            // Keep only the first observation states that have been explicitely written (the last one is triggered)
            {
              var allOss = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindAll (machine);
              for (int i = allOss.Count - 1; i >= 3; i--) {
                ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakeTransient (allOss[i]);
              }
            }

            // Reason slots
            { // T1-T4: Active (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 4));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (4), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T4-T5: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (4, 5));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (4), T (5), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T5-T7: Inactive (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 7));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (5), T (7), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T7-T9: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 9));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (9), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T9-T20: Active (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (9, 20));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (9), T (20), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.ModelFactory.CreateMachineStatus (machine);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (20);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonWeekend.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 13
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));
            autoReason.Initialize (machine, null);

            // First run => until the weekend beginning
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (5));
            CheckNoReasonMachineAssociation ();

            // Second run => until the end of the weekend
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (7));
            CheckNoReasonMachineAssociation ();


            // Third run => after the weekend, the weekend has been processed
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (20));

            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ().FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              Assert.AreEqual (reasonMachineAssociations[0].Range, R (5, 7));
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
    /// non-running period starting after the beginning of the weekend
    /// running period starting before the end of the weekend, within the margin
    /// => Reason "Weekend" applied on from the start of the non-running period to the beginning of the running period
    /// 
    ///                   | Weekend   |
    /// +++++++++++++++++++++------+++++++++++++++++++++++++
    /// 1                 5 7     14 15                   30
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </summary>
    [Test]
    public void TestCase2 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
            machine.Name = machine.Code = "j'adore les cerises";
            machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById ((int)MachineMonitoringTypeId.Monitored);
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);

            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var weekend = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Weekend);

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
            { // Weekend: T5-T15
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 15));
              observationStateSlot.MachineObservationState = weekend;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T15-T30
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (15, 30));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            // Keep only the first observation states that have been explicitely written (the last one is triggered)
            {
              var allOss = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindAll (machine);
              for (int i = allOss.Count - 1; i >= 3; i--) {
                ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakeTransient (allOss[i]);
              }
            }

            // Reason slots
            { // T1-T5: Active (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 5));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (4), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T5-T7: Active (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (5), T (7), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T7-T14: Inactive (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 14));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (14), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T14-T15: Active (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (14, 15));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (14), T (15), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T15-T30: Active (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (15, 30));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (15), T (30), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.ModelFactory.CreateMachineStatus (machine);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (30);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonWeekend.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 13
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));
            autoReason.Initialize (machine, null);

            // First run => until the weekend beginning
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (5));
            CheckNoReasonMachineAssociation ();

            // Second run => until the end of the weekend
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (15));
            CheckNoReasonMachineAssociation ();


            // Third run => after the weekend, the weekend has been processed
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (30));

            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ().FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              Assert.AreEqual (reasonMachineAssociations[0].Range, R (7, 14));
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 3:
    /// 
    /// non-running period starting after the beginning of the weekend
    /// running period starting before the end of the weekend, outside the margin
    /// => No reason "weekend" applied
    /// 
    ///                   | Weekend   |
    /// +++++++++++++++++++++------+++++++++++++++++++++++++
    /// 1                 5 7     11 15                   30
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </summary>
    [Test]
    public void TestCase3 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
            machine.Name = machine.Code = "j'adore les cerises";
            machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById ((int)MachineMonitoringTypeId.Monitored);
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);

            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var weekend = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Weekend);

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
            { // Weekend: T5-T15
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 15));
              observationStateSlot.MachineObservationState = weekend;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T15-T30
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (15, 30));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            // Keep only the first observation states that have been explicitely written (the last one is triggered)
            {
              var allOss = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindAll (machine);
              for (int i = allOss.Count - 1; i >= 3; i--) {
                ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakeTransient (allOss[i]);
              }
            }

            // Reason slots
            { // T1-T5: Active (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 5));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (4), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T5-T7: Active (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (5), T (7), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T7-T11: Inactive (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 11));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (11), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T11-T15: Active (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (11, 15));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (11), T (15), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T15-T30: Active (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (15, 30));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (15), T (30), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.ModelFactory.CreateMachineStatus (machine);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (30);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonWeekend.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 13
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));
            autoReason.Initialize (machine, null);

            // First run => until the weekend beginning
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (5));
            CheckNoReasonMachineAssociation ();

            // Second run => until the end of the weekend
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (15));
            CheckNoReasonMachineAssociation ();


            // Third run => after the weekend, the weekend has been processed
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (30));
            CheckNoReasonMachineAssociation ();
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 4:
    /// 
    /// 2 non-running periods starting after the beginning of the weekend
    /// running period starting after the end of the weekend
    /// => Reason "Weekend" applied from the start of the second non-running period to the end of the weekend
    /// 
    ///                   |  Weekend  |
    /// +++++++++++++++++++++--+++--------+++++++++++++++++++++++++
    /// 1                 5 7  8 9   15  18                      30
    /// 
    /// -: not running
    /// +: running
    /// Margin: 2
    /// </summary>
    [Test]
    public void TestCase4 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
            machine.Name = machine.Code = "j'adore les cerises";
            machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById ((int)MachineMonitoringTypeId.Monitored);
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);

            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

            var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var weekend = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Weekend);

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
            { // Weekend: T5-T15
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 15));
              observationStateSlot.MachineObservationState = weekend;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T15-T30
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (15, 30));
              observationStateSlot.MachineObservationState = production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            // Keep only the first observation states that have been explicitely written (the last one is triggered)
            {
              var allOss = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindAll (machine);
              for (int i = allOss.Count - 1; i >= 3; i--) {
                ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakeTransient (allOss[i]);
              }
            }

            // Reason slots
            { // T1-T5: Active (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 5));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (4), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T5-T7: Active (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 7));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (5), T (7), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T7-T8: Inactive (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (7, 8));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (8), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T8-T9: Active (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (8, 9));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (8), T (9), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T9-T15: Inactive (weekend)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (9, 15));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = weekend;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (9), T (15), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T15-T18: Inactive (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (15, 18));
              reasonSlot.MachineMode = inactive;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (15), T (18), inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T18-T30: Active (production)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (18, 30));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (18), T (30), active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.ModelFactory.CreateMachineStatus (machine);
              machineStatus.CncMachineMode = active;
              machineStatus.MachineMode = active;
              machineStatus.MachineObservationState = production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (30);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonWeekend.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""Margin"": ""0:00:02"",
  ""MachineObservationStateId"": 13
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));
            autoReason.Initialize (machine, null);

            // First run => until the weekend beginning
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (5));
            CheckNoReasonMachineAssociation ();

            // Second run => until the end of the weekend
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (15));
            CheckNoReasonMachineAssociation ();


            // Third run => after the weekend, the weekend has been processed
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (30));

            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ().FindAll ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              Assert.AreEqual (reasonMachineAssociations[0].Range, R (9, 15));
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }
  }
}
