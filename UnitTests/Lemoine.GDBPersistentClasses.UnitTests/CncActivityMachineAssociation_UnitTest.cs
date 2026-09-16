// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class CncActivityMachineAssociation
  /// </summary>
  [TestFixture]
  public class CncActivityMachineAssociation_UnitTest : WithMinuteTimeStamp
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public CncActivityMachineAssociation_UnitTest ()
      : base (new DateTime (2015, 01, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// The last reason slot must not be extended (fast process) over a period with another machine observation state,
    /// even if the machine status is not consistent with the last reason slot and does not show any change,
    /// else the reason is not reset on that period
    /// </summary>
    [Test]
    public void TestNoFastProcessWhenTheLastReasonSlotHasAnotherMachineObservationState ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonProcessing = daoFactory.ReasonDAO.FindById ((int)ReasonId.Processing);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);

          // Existing reason slot 0 -> 10, unattended
          {
            var existingSlot = new ReasonSlot (machine, R (0, 10));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = unattended;
            existingSlot.SetDefaultReason (reasonUnattended, 10.0, false, true);
            existingSlot.Consolidate (null, null);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }

          // Machine status that is not consistent with the last reason slot: attended instead of unattended
          var machineStatus = new MachineStatus (machine);
          machineStatus.CncMachineMode = inactive;
          machineStatus.MachineMode = inactive;
          machineStatus.MachineObservationState = attended;
          machineStatus.ManualActivity = false;
          machineStatus.Reason = reasonUnattended;
          machineStatus.ReasonSlotEnd = T (10);
          daoFactory.MachineStatusDAO.MakePersistent (machineStatus);

          // New attended activity 10 -> 20
          {
            var association = new CncActivityMachineAssociation (machine, inactive, null, attended, R (10, 20));
            Assert.That (association.ProcessAssociation (machineStatus), Is.True);
          }
          {
            IList<IReasonSlot> reasonSlots = daoFactory.ReasonSlotDAO
              .FindOverlapsRange (machine, R (0, null));
            Assert.That (reasonSlots, Has.Count.EqualTo (2), "Number of reason slots");
            Assert.Multiple (() => {
              Assert.That (reasonSlots[0].EndDateTime.Value, Is.EqualTo (T (10)));
              Assert.That (reasonSlots[0].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[0].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (reasonSlots[1].BeginDateTime.Value, Is.EqualTo (T (10)));
              Assert.That (reasonSlots[1].EndDateTime.Value, Is.EqualTo (T (20)));
              Assert.That (reasonSlots[1].MachineObservationState, Is.EqualTo (attended));
              Assert.That (reasonSlots[1].Reason, Is.EqualTo (reasonProcessing));
            });
          }

          // New attended activity 20 -> 30: the machine status is now consistent, the fast process applies
          {
            var association = new CncActivityMachineAssociation (machine, inactive, null, attended, R (20, 30));
            Assert.That (association.ProcessAssociation (machineStatus), Is.True);
          }
          {
            IList<IReasonSlot> reasonSlots = daoFactory.ReasonSlotDAO
              .FindOverlapsRange (machine, R (0, null));
            Assert.That (reasonSlots, Has.Count.EqualTo (2), "Number of reason slots");
            Assert.Multiple (() => {
              Assert.That (reasonSlots[1].BeginDateTime.Value, Is.EqualTo (T (10)));
              Assert.That (reasonSlots[1].EndDateTime.Value, Is.EqualTo (T (30)));
              Assert.That (reasonSlots[1].MachineObservationState, Is.EqualTo (attended));
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
