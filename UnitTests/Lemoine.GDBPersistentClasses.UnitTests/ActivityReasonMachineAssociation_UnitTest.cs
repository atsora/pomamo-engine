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
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonProcessing));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (8 * 60)));
              Assert.That (slots[i].ReasonSource, Is.EqualTo (ReasonSource.Default | ReasonSource.UnsafeAutoReasonNumber | ReasonSource.UnsafeManualFlag));
            });
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
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonProcessing));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (8 * 60)));
              Assert.That (slots[i].ReasonSource, Is.EqualTo (ReasonSource.Default | ReasonSource.UnsafeAutoReasonNumber | ReasonSource.UnsafeManualFlag));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonProcessing));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (slots[i].MachineMode, Is.EqualTo (nulloverride));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (8 * 60)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (16 * 60)));
              Assert.That (slots[i].ReasonSource, Is.EqualTo (ReasonSource.Default | ReasonSource.UnsafeAutoReasonNumber | ReasonSource.UnsafeManualFlag));
            });
            ++i;
          }

          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (8 * 60)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (slots[i].MachineMode, Is.EqualTo (nulloverride));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (8 * 60)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (16 * 60)));
            });
            ++i;
          }
          {
            DayRange dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO
              .ConvertToDayRange (R (0, null));
            IList<IReasonSummary> summaries = ModelDAOHelper.DAOFactory.ReasonSummaryDAO
              .FindInDayRangeWithReason (machine1, dayRange);
            Assert.That (summaries, Has.Count.EqualTo (1));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromMinutes (16)));
            });
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
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (16 * 60)));
            });
            ++i;
          }
          {
            DayRange dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO
              .ConvertToDayRange (R (0, null));
            IList<IReasonSummary> summaries = ModelDAOHelper.DAOFactory.ReasonSummaryDAO
              .FindInDayRangeWithReason (machine1, dayRange);
            Assert.That (summaries, Has.Count.EqualTo (1));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromMinutes (16)));
            });
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
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (8)));
            });
            ++i;
          }
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();
          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (false));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (8)));
            });
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
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (false));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (16)));
            });
            ++i;
          }
          {
            DayRange dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO
              .ConvertToDayRange (R (0, null));
            IList<IReasonSummary> summaries = ModelDAOHelper.DAOFactory.ReasonSummaryDAO
              .FindInDayRangeWithReason (machine1, dayRange);
            Assert.That (summaries, Has.Count.EqualTo (1));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromSeconds (16)));
            });
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
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (8)));
            });
            ++i;
          }
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();
          // Check the values
          {
            IList<IReasonSlot> slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine1, R (0, null));
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (false));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (8)));
            });
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
            Assert.That (slots, Has.Count.EqualTo (3), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (false));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (8)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (false));
              Assert.That (slots[i].MachineMode, Is.EqualTo (nulloverride));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (8)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (12)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonShort));
              Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (slots[i].MachineMode, Is.EqualTo (nulloverride));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (12)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (16)));
            });
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
