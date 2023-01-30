// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NUnit.Framework;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ActivityReasonMachineAssociation
  /// </summary>
  [TestFixture]
  public class ActivityReasonMachineAssociation_UnitTest : WithSecondTimeStamp
  {
    string m_previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (ActivityReasonMachineAssociation_UnitTest).FullName);

    public ActivityReasonMachineAssociation_UnitTest ()
      : base (new DateTime (2015, 01, 01, 21, 00, 00, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// Test
    /// </summary>
    [Test]
    public void TestShortShortIsLong ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonSetup = daoFactory.ReasonDAO.FindById (16);
          var reasonProcessing = daoFactory.ReasonDAO.FindById ((int)ReasonId.Processing);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);
          IMachineMode nulloverride = daoFactory.MachineModeDAO.FindById ((int)MachineModeId.AutoNullOverride);

          { // For a test
            var machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO.FindAll ();
          }

          // New association 0 -> 8 inactive
          {
            var association = new Lemoine.GDBPersistentClasses.ActivityReasonMachineAssociation (machine1,
                                                                                                 R (0, 8 * 60));
            association.MachineObservationState = attended;
            association.MachineMode = inactive;
            association.ProcessAssociation ();
          }
          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunMakeAnalysis ();
          ModelDAOHelper.DAOFactory.Flush ();
          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (1, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonProcessing, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (8 * 60), slots[i].EndDateTime.Value);
            Assert.AreEqual (ReasonSource.Default | ReasonSource.UnsafeAutoReasonNumber | ReasonSource.UnsafeManualFlag, slots[i].ReasonSource);
          }

          // New association 8 -> 16 autoNullOverride
          {
            var association = new Lemoine.GDBPersistentClasses.ActivityReasonMachineAssociation (machine1,
                                                                                                 R (8 * 60, 16 * 60));
            association.MachineObservationState = attended;
            association.MachineMode = nulloverride;
            association.ProcessAssociation ();
          }
          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunMakeAnalysis ();
          ModelDAOHelper.DAOFactory.Flush ();
          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (2, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonProcessing, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (8 * 60), slots[i].EndDateTime.Value);
            Assert.AreEqual (ReasonSource.Default | ReasonSource.UnsafeAutoReasonNumber | ReasonSource.UnsafeManualFlag, slots[i].ReasonSource);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonProcessing, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (nulloverride, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (8 * 60), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (16 * 60), slots[i].EndDateTime.Value);
            Assert.AreEqual (ReasonSource.Default | ReasonSource.UnsafeAutoReasonNumber | ReasonSource.UnsafeManualFlag, slots[i].ReasonSource);
            ++i;
          }

          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (2, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonUnanswered, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (8 * 60), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonUnanswered, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (nulloverride, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (8 * 60), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (16 * 60), slots[i].EndDateTime.Value);
            ++i;
          }
          {
            DayRange dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO
              .ConvertToDayRange (R (0, null));
            IList<IReasonSummary> summaries = ModelDAOHelper.DAOFactory.ReasonSummaryDAO
              .FindInDayRangeWithReason (machine1, dayRange);
            Assert.AreEqual (1, summaries.Count);
            int i = 0;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (reasonUnanswered, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromMinutes (16), summaries[i].Time);
          }

        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    /// <summary>
    /// Test
    /// </summary>
    [Test]
    public void TestExtendShortToLong ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonSetup = daoFactory.ReasonDAO.FindById (16);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);
          IMachineMode nulloverride = daoFactory.MachineModeDAO.FindById ((int)MachineModeId.AutoNullOverride);

          { // For a test
            var machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO.FindAll ();
          }

          // New association 0 -> 8 inactive
          {
            var association = new Lemoine.GDBPersistentClasses.ActivityReasonMachineAssociation (machine1,
                                                                                                 R (0, 8 * 60));
            association.MachineObservationState = attended;
            association.MachineMode = inactive;
            association.ProcessAssociation ();
          }
          // New association 8 -> 16 inactive
          {
            var association = new Lemoine.GDBPersistentClasses.ActivityReasonMachineAssociation (machine1,
                                                                                                 R (8 * 60, 16 * 60));
            association.MachineObservationState = attended;
            association.MachineMode = inactive;
            association.ProcessAssociation ();
          }

          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunMakeAnalysis ();
          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

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
            Assert.AreEqual (T (16 * 60), slots[i].EndDateTime.Value);
            ++i;
          }
          {
            DayRange dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO
              .ConvertToDayRange (R (0, null));
            IList<IReasonSummary> summaries = ModelDAOHelper.DAOFactory.ReasonSummaryDAO
              .FindInDayRangeWithReason (machine1, dayRange);
            Assert.AreEqual (1, summaries.Count);
            int i = 0;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (reasonUnanswered, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromMinutes (16), summaries[i].Time);
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    abstract class DynamicEndExtension
      : Lemoine.UnitTests.WithSecondTimeStamp
      , Lemoine.Extensions.Business.DynamicTimes.IDynamicTimeExtension
    {
      static int s_step = 0;

      public abstract DateTime GetFinal ();
  
      public DynamicEndExtension ()
        : base (new DateTime (2015, 01, 01, 21, 00, 00, DateTimeKind.Utc))
      {
      }

      public bool Initialize (IMachine machine, string parameter)
      {
        this.Machine = machine;
        return true;
      }

      public IMachine Machine
      {
        get; set;
      }

      public string Name
      {
        get
        {
          return "Test";
        }
      }

      public bool UniqueInstance
      {
        get
        {
          return true;
        }
      }

      public bool IsApplicable ()
      {
        return true;
      }

      public DynamicTimeApplicableStatus IsApplicableAt (DateTime at)
      {
        throw new NotImplementedException ();
      }         

      public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
      {
        var step = s_step++;
        switch (step) {
          case 0:
            return this.CreatePending ();
          case 1:
          case 2:
            return this.CreateWithHint (R (2));
          default:
            return this.CreateFinal (GetFinal ());
        }
      }

      public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
      {
        return TimeSpan.FromTicks (0);
      }

      public static void Reset ()
      {
        s_step = 0;
      }
    }

    class DynamicEndExtension12 : DynamicEndExtension
    {
      public override DateTime GetFinal () => T (12);
    }

    class DynamicEndExtension16 : DynamicEndExtension
    {
      public override DateTime GetFinal () => T (16);
    }

    class AutoReasonCoffee : IAutoReasonExtension
    {
      public double? ManualScore
      {
        get
        {
          return 200;
        }
      }

      public bool UniqueInstance
      {
        get
        {
          return true;
        }
      }

      public IMonitoredMachine Machine => throw new NotImplementedException ();

      public IReason Reason => throw new NotImplementedException ();

      public double ReasonScore => throw new NotImplementedException ();

      public bool CanOverride (IReasonSlot reasonSlot)
      {
        return true;
      }

      public bool Initialize (IMonitoredMachine machine, Lemoine.Threading.IChecked caller)
      {
        return true;
      }

      public bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
      {
        return true;
      }

      public bool IsValidMatch (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double score)
      {
        return reason.Id == 28;
      }

      public void RunOnce ()
      {        
        throw new NotImplementedException ();
      }
    }

    /// <summary>
    /// Test
    /// </summary>
    [Test]
    public void TestWithAutoReason ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtension16));
      Lemoine.Extensions.ExtensionManager.Add (typeof (AutoReasonCoffee));

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonSetup = daoFactory.ReasonDAO.FindById (16);
          IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
          IReason reasonProcessing = daoFactory.ReasonDAO.FindById ((int)ReasonId.Processing);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);
          IMachineMode nulloverride = daoFactory.MachineModeDAO.FindById ((int)MachineModeId.AutoNullOverride);

          { // For a test
            var machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO.FindAll ();
          }

          // New association 0 -> 8 inactive
          {
            var association = new Lemoine.GDBPersistentClasses.ActivityReasonMachineAssociation (machine1,
                                                                                                 R (0, 8));
            association.MachineObservationState = attended;
            association.MachineMode = inactive;
            association.ProcessAssociation ();
          }
          // New association 0 -> 16: auto-reason
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (0, 16), reasonCoffee, 120, "", ",Test", false, null);
          }
          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunMakeAnalysis ();
          ModelDAOHelper.DAOFactory.Flush ();
          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (1, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonCoffee, slots[i].Reason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (8), slots[i].EndDateTime.Value);
            ++i;
          }
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();
          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (1, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonCoffee, slots[i].Reason);
            Assert.AreEqual (false, slots[i].DefaultReason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (8), slots[i].EndDateTime.Value);
            ++i;
          }

          // New association 8 -> 16 inactive
          {
            var association = new Lemoine.GDBPersistentClasses.ActivityReasonMachineAssociation (machine1,
                                                                                               R (8, 16));
            association.MachineObservationState = attended;
            association.MachineMode = inactive;
            association.ProcessAssociation ();
          }

          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunMakeAnalysis ();
          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (1, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonCoffee, slots[i].Reason);
            Assert.AreEqual (false, slots[i].DefaultReason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (16), slots[i].EndDateTime.Value);
            ++i;
          }
          {
            DayRange dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO
              .ConvertToDayRange (R (0, null));
            IList<IReasonSummary> summaries = ModelDAOHelper.DAOFactory.ReasonSummaryDAO
              .FindInDayRangeWithReason (machine1, dayRange);
            Assert.AreEqual (1, summaries.Count);
            int i = 0;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (reasonCoffee, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromSeconds (16), summaries[i].Time);
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          DynamicEndExtension.Reset ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    /// <summary>
    /// Test
    /// </summary>
    [Test]
    public void TestWithAutoReason2 ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtension12));
      Lemoine.Extensions.ExtensionManager.Add (typeof (AutoReasonCoffee));

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonSetup = daoFactory.ReasonDAO.FindById (16);
          IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
          IReason reasonProcessing = daoFactory.ReasonDAO.FindById ((int)ReasonId.Processing);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);
          IMachineMode nulloverride = daoFactory.MachineModeDAO.FindById ((int)MachineModeId.AutoNullOverride);

          { // For a test
            var machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO.FindAll ();
          }

          // New association 0 -> 8 inactive
          {
            var association = new Lemoine.GDBPersistentClasses.ActivityReasonMachineAssociation (machine1,
                                                                                                 R (0, 8));
            association.MachineObservationState = attended;
            association.MachineMode = inactive;
            association.ProcessAssociation ();
          }
          // New association 0 -> 12: auto-reason
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (0, 12), reasonCoffee, 120.0, "", ",Test", false, null);
          }
          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunMakeAnalysis ();
          ModelDAOHelper.DAOFactory.Flush ();
          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (1, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonCoffee, slots[i].Reason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (8), slots[i].EndDateTime.Value);
            ++i;
          }
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();
          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (1, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonCoffee, slots[i].Reason);
            Assert.AreEqual (false, slots[i].DefaultReason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (8), slots[i].EndDateTime.Value);
            ++i;
          }

          // New association 8 -> 16 inactive
          {
            var association = new Lemoine.GDBPersistentClasses.ActivityReasonMachineAssociation (machine1,
                                                                                               R (8, 16));
            association.MachineObservationState = attended;
            association.MachineMode = nulloverride;
            association.ProcessAssociation ();
          }

          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunMakeAnalysis ();
          ModelDAOHelper.DAOFactory.Flush ();
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.AreEqual (3, slots.Count, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonCoffee, slots[i].Reason);
            Assert.AreEqual (false, slots[i].DefaultReason);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (0), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (8), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonCoffee, slots[i].Reason);
            Assert.AreEqual (false, slots[i].DefaultReason);
            Assert.AreEqual (nulloverride, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (8), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (12), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (reasonShort, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (nulloverride, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (T (12), slots[i].BeginDateTime.Value);
            Assert.AreEqual (T (16), slots[i].EndDateTime.Value);
            ++i;
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          DynamicEndExtension.Reset ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    /// <summary>
    /// Day begin
    /// </summary>
    /// <param name="days"></param>
    /// <returns></returns>
    DateTime D (int days)
    {
      return new DateTime (2015, 01, 02).AddDays (days);
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();

      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Plugin.DefaultAccumulators.DefaultAccumulators.Install ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();

      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }
}
