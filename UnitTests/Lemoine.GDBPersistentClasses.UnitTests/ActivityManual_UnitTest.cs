// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Business.Config;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ManualActivity
  /// </summary>
  [TestFixture]
  public class ActivityManual_UnitTest : WithMinuteTimeStamp
  {
    string previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (ActivityManual_UnitTest).FullName);

    public ActivityManual_UnitTest ()
      : base (new DateTime (2011, 08, 01, 12, 00, 00, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestMakeAnalysis ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonSetup = daoFactory.ReasonDAO.FindById (16);
          IReason reasonProcessing = daoFactory.ReasonDAO.FindById ((int)ReasonId.Processing);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              R (0, 10));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              R (10, 20));
            existingSlot.MachineMode = active;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonMotion, 10.0, false, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              R (20, 30));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 01),
                                                                                attended, inactive);
            summary.Time = TimeSpan.FromMinutes (20);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 01),
                                                                                attended, active);
            summary.Time = TimeSpan.FromMinutes (10);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 01), null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromMinutes (20);
            summary.Number = 2;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 01), null,
                                          attended, reasonMotion);
            summary.Time = TimeSpan.FromMinutes (10);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (30);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New association 10 -> 20 inactive
          {
            var association =
              ModelDAOHelper.ModelFactory.CreateActivityManual (machine1, inactive, R (10, 20));
            association.DateTime = UtcDateTime.From (2011, 08, 01, 12, 15, 00);
            daoFactory.ActivityManualDAO.MakePersistent (association);
          }

          {
            AnalysisUnitTests.RunMakeAnalysis ();
            DAOFactory.EmptyAccumulators ();
            ModelDAOHelper.DAOFactory.FlushData ();
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None);
          }

          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (1, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonUnanswered, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (30), slots[i].EndDateTime.Value);
          }
          // - AnalysisLogs
          AnalysisUnitTests.CheckNumberOfAnalysisLogs (session, 0); // Warning on reason slot motion

        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();

    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
