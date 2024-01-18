// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonRestartAfterBreak;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the auto-reason break
  /// </summary>
  [TestFixture]
  public class AutoReasonRestartAfterBreak_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonRestartAfterBreak_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonRestartAfterBreak_UnitTest ()
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
      Assert.That (autoReasonState.Value, Is.EqualTo (v));
    }

    /// <summary>
    /// Test case 1:
    /// 
    /// Three machine observation states
    /// * production from T1 to T5
    /// * break from T5 to T10
    /// * production from T10 to T20
    /// 
    /// Two different machine modes after a break:
    /// * MM3 from T10 to T12 and from T12 to T14
    /// * MM1 from T14 to T20
    /// 
    /// Result:
    /// * 1 auto reason "restart after break" applied from 10 to no end (dynamic end)
    /// 
    /// |    Prod.    |     Break     |              Prod.              |   (machine observation states)
    /// |     MM1     |      MM2      |  MM3  |  MM3  |       MM1       |   (reason slots)
    /// 1             5              10      12      14                20
    /// 
    /// </summary>
    [Test]
    public void TestCase1 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);

            // Machine observation state slots
            var mos_production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Production);
            var mos_breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Break);
            var shift = ModelDAOHelper.ModelFactory.CreateShiftFromCode ("Test");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift);

            { // Production: T1-T5
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (1, 5));
              observationStateSlot.MachineObservationState = mos_production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Break: T5-T10
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (5, 10));
              observationStateSlot.MachineObservationState = mos_breakTime;
              observationStateSlot.Production = false;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }
            { // Production: T10-T20
              var observationStateSlot = ModelDAOHelper.ModelFactory.CreateObservationStateSlot (machine, R (10, 20));
              observationStateSlot.MachineObservationState = mos_production;
              observationStateSlot.Production = true;
              observationStateSlot.Shift = shift;
              ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
            }

            // Reason slots
            var mm_inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var mm_active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mm_unknown = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.AutoUnknown);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);

            { // T1-T5: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 5));
              reasonSlot.MachineMode = mm_active;
              reasonSlot.MachineObservationState = mos_production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (5), mm_active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T5-T10: Inactive (break)
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (5, 10));
              reasonSlot.MachineMode = mm_inactive;
              reasonSlot.MachineObservationState = mos_breakTime;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonUnanswered, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (5), T (6), mm_inactive);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T10-T12: Unknown
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (10, 12));
              reasonSlot.MachineMode = mm_unknown;
              reasonSlot.MachineObservationState = mos_production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (10), T (12), mm_unknown);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T12-T14: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (12, 14));
              reasonSlot.MachineMode = mm_unknown;
              reasonSlot.MachineObservationState = mos_production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (12), T (14), mm_unknown);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }
            { // T14-T20: Active
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (14, 20));
              reasonSlot.MachineMode = mm_active;
              reasonSlot.MachineObservationState = mos_production;
              ((ReasonSlot)reasonSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);

              var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (14), T (20), mm_active);
              ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
            }

            { // MachineStatus
              var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (machine.Id);
              machineStatus.CncMachineMode = mm_active;
              machineStatus.MachineMode = mm_active;
              machineStatus.MachineObservationState = mos_production;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonMotion;
              machineStatus.ReasonSlotEnd = T (29);
              ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonRestartAfterBreak.Plugin ();
            plugin.Install (0);
            var autoReason = new AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""MachineObservationStateId"": 11
}
");
            string dateTimeKey = autoReason.GetKey ("DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (0));
            autoReason.Initialize (machine, null);
            autoReason.RunOnce ();

            // Check that we are at the end of the break
            CheckAutoReasonState (machine, dateTimeKey, T (10));

            // Number of reasons created
            var reasonMachineAssociations = new ReasonMachineAssociationDAO ().FindAll ();
            Assert.That (reasonMachineAssociations, Has.Count.EqualTo (1), "Wrong number of reasons");

            // Properties of the reason
            var reasonMachineAssociation = reasonMachineAssociations[0];
            Assert.IsTrue (reasonMachineAssociation.Range.Lower.HasValue);
            Assert.IsFalse (reasonMachineAssociation.Range.Upper.HasValue);
            Assert.Multiple (() => {
              Assert.That (reasonMachineAssociation.Range.Lower.Value, Is.EqualTo (T (10)));
              Assert.That (reasonMachineAssociation.DynamicEnd, Is.EqualTo ("NextMachineMode"));
              Assert.That (reasonMachineAssociation.Machine, Is.EqualTo (machine));
            });

            // After a next run, we are at the end of the machine status
            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (29));
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
