// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class ReasonModificationManual_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (ReasonModificationManual_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonModificationManual_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestTryResetReason ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Reference data
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var autoNullOverride = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.AutoNullOverride);
            var reasonUndefined = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Undefined);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var reasonShort = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Short);
            var reasonProcessing = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Processing);
            var newReasonGroup = ModelDAOHelper.ModelFactory.CreateReasonGroup ();
            newReasonGroup.Name = "Test1";
            ModelDAOHelper.DAOFactory.ReasonGroupDAO.MakePersistent (newReasonGroup);
            var newReason = ModelDAOHelper.ModelFactory.CreateReason (newReasonGroup);
            newReason.Name = "Test1";
            ModelDAOHelper.DAOFactory.ReasonDAO.MakePersistent (newReason);

            {
              var reasonSelection = ModelDAOHelper.ModelFactory.CreateReasonSelection (autoNullOverride, attended);
              reasonSelection.Reason = newReason;
              ModelDAOHelper.DAOFactory.ReasonSelectionDAO.MakePersistent (reasonSelection);
            }

            // Plugin
            var reasonExtension = new Lemoine.Plugin.ReasonDefaultManagement.ReasonModificationManual ();
            reasonExtension.Initialize (machine);

            {
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3600));
              reasonSlot.MachineMode = autoNullOverride;
              reasonSlot.MachineObservationState = attended;
              reasonSlot.SwitchToProcessing ();
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
              ModelDAOHelper.DAOFactory.Flush ();
              var modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                .InsertManualReason (machine, R (1, 300), newReason, 100, "details");
              ModelDAOHelper.DAOFactory.Flush ();
              var manualModification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                .FindById (modificationId, machine);
              var reasonProposal = ModelDAOHelper.ModelFactory.CreateReasonProposal (manualModification, R (1, 300));
              ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakePersistent (reasonProposal);
              reasonSlot.CancelData ();
              ModelDAOHelper.DAOFactory.Flush ();

              reasonExtension.TryResetReason (ref reasonSlot);
              Assert.Multiple (() => {
                Assert.That (reasonSlot.Reason, Is.EqualTo (newReason));
                Assert.That (reasonSlot.ReasonScore, Is.EqualTo (100));
                Assert.That (reasonSlot.DateTimeRange, Is.EqualTo (R (1, 300)));
                Assert.That (reasonSlot.ReasonDetails, Is.EqualTo ("details"));
              });
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestTryResetReason2 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Reference data
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var autoNullOverride = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.AutoNullOverride);
            var reasonUndefined = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Undefined);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var reasonShort = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Short);
            var reasonProcessing = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Processing);
            var newReasonGroup = ModelDAOHelper.ModelFactory.CreateReasonGroup ();
            newReasonGroup.Name = "Test1";
            ModelDAOHelper.DAOFactory.ReasonGroupDAO.MakePersistent (newReasonGroup);
            var newReason1 = ModelDAOHelper.ModelFactory.CreateReason (newReasonGroup);
            newReason1.Name = "Test1";
            ModelDAOHelper.DAOFactory.ReasonDAO.MakePersistent (newReason1);
            var newReason2 = ModelDAOHelper.ModelFactory.CreateReason (newReasonGroup);
            newReason2.Name = "Test2";
            ModelDAOHelper.DAOFactory.ReasonDAO.MakePersistent (newReason2);

            {
              var reasonSelection = ModelDAOHelper.ModelFactory.CreateReasonSelection (autoNullOverride, attended);
              reasonSelection.Reason = newReason1;
              ModelDAOHelper.DAOFactory.ReasonSelectionDAO.MakePersistent (reasonSelection);
            }
            {
              var reasonSelection = ModelDAOHelper.ModelFactory.CreateReasonSelection (autoNullOverride, attended);
              reasonSelection.Reason = newReason2;
              ModelDAOHelper.DAOFactory.ReasonSelectionDAO.MakePersistent (reasonSelection);
            }

            // Plugin
            var reasonExtension = new Lemoine.Plugin.ReasonDefaultManagement.ReasonModificationManual ();
            reasonExtension.Initialize (machine);

            {
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3600));
              reasonSlot.MachineMode = autoNullOverride;
              reasonSlot.MachineObservationState = attended;
              reasonSlot.SwitchToProcessing ();
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
              ModelDAOHelper.DAOFactory.Flush ();
              {
                var modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                  .InsertManualReason (machine, R (1, 300), newReason1, 100, "details");
                ModelDAOHelper.DAOFactory.Flush ();
                var manualModification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                  .FindById (modificationId, machine);
                var reasonProposal = ModelDAOHelper.ModelFactory.CreateReasonProposal (manualModification, R (1, 300));
                ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakePersistent (reasonProposal);
              }
              {
                var modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                  .InsertManualReason (machine, R (300, 3600), newReason2, 100, "details");
                ModelDAOHelper.DAOFactory.Flush ();
                var manualModification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                  .FindById (modificationId, machine);
                var reasonProposal = ModelDAOHelper.ModelFactory.CreateReasonProposal (manualModification, R (300, 3600));
                ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakePersistent (reasonProposal);
              }
              reasonSlot.CancelData ();
              ModelDAOHelper.DAOFactory.Flush ();

              reasonExtension.TryResetReason (ref reasonSlot);
              Assert.Multiple (() => {
                Assert.That (reasonSlot.Reason, Is.EqualTo (newReason1));
                Assert.That (reasonSlot.ReasonScore, Is.EqualTo (100));
                Assert.That (reasonSlot.DateTimeRange, Is.EqualTo (R (1, 300)));
                Assert.That (reasonSlot.Consolidated, Is.True);
                Assert.That (reasonSlot.ReasonDetails, Is.EqualTo ("details"));
              });

              var reasonSlot2 = ModelDAOHelper.DAOFactory.ReasonSlotDAO
                .FindAt (machine, T (300));
              Assert.Multiple (() => {
                Assert.That (reasonSlot2.DateTimeRange.Lower.Value, Is.EqualTo (T (300)));
                Assert.That (reasonSlot2.Reason.Id, Is.EqualTo ((int)ReasonId.Processing));
              });
              reasonExtension.TryResetReason (ref reasonSlot2);
              Assert.Multiple (() => {
                Assert.That (reasonSlot2.Reason, Is.EqualTo (newReason2));
                Assert.That (reasonSlot2.ReasonDetails, Is.EqualTo ("details"));
                Assert.That (reasonSlot2.Consolidated, Is.True);
                Assert.That (reasonSlot.Consolidated, Is.True);
                Assert.That (reasonSlot2.Reason, Is.Not.EqualTo (reasonSlot.Reason));
                Assert.That (reasonSlot.ReasonDetails, Is.EqualTo ("details"));
                Assert.That (reasonSlot.ReferenceDataEquals (reasonSlot2), Is.False);
              });
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }
  }
}
