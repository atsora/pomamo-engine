// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using System.Linq;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Business.Config;
using Lemoine.Plugin.SameMachineMode;
using System.Threading;
using Lemoine.Extensions.Database;
using Lemoine.Database.Persistent;
using Pulse.Extensions;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// Unit tests for the classes MonitoredMachineActivityAnalysis
  /// </summary>
  [TestFixture]
  public class MonitoredMachineActivityAnalysis_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MonitoredMachineActivityAnalysis_UnitTest).FullName);

    /// <summary>
    /// Test an stamp => auto-sequence => sequence slot => operation slot process
    /// </summary>
    [Test]
    public void TestAnalysis ()
    {
      Lemoine.Plugin.DefaultAccumulators.DefaultAccumulators.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MonitoredMachineActivityAnalysisStateMachineExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MachineActivityAnalysisStateMachineExtension));

      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          try {
            // Reference data
            IMonitoredMachine machine =
              daoFactory.MonitoredMachineDAO.FindById (3);
            Assert.NotNull (machine);
            MachineMode autoMode = session.Get<MachineMode> (3);
            Assert.NotNull (autoMode);
            MachineMode inactiveMode = session.Get<MachineMode> (1);
            Assert.NotNull (inactiveMode);
            MachineMode noDataMode = session.Get<MachineMode> (8);
            Assert.NotNull (noDataMode);
            MachineObservationState attended =
              session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
            MachineObservationState unattended =
              session.Get<MachineObservationState> ((int)MachineObservationStateId.Unattended);

            // Config
            Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                               (int)OperationSlotSplitOption.None);
            Lemoine.Info.ConfigSet
              .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

            // OperationSlot
            {
              IOperation operation1 =
                daoFactory.OperationDAO.FindById (1);
              IOperation operation2 =
                daoFactory.OperationDAO.FindById (2);
              IOperationSlot operationSlot1 =
                modelFactory.CreateOperationSlot (machine,
                                                  operation1,
                                                  null,
                                                  null,
                                                  null, null, null, null,
                                                  new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 09, 00, 00),
                                                                        UtcDateTime.From (2008, 01, 16, 10, 27, 00)));
              daoFactory.OperationSlotDAO.MakePersistent (operationSlot1);
              IOperationSlot operationSlot2 =
                modelFactory.CreateOperationSlot (machine,
                                                  operation2,
                                                  null,
                                                  null,
                                                  null, null, null, null,
                                                  new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 10, 28, 00),
                                                                        UtcDateTime.From (2008, 01, 16, 10, 36, 00)));
              daoFactory.OperationSlotDAO.MakePersistent (operationSlot2);
            }

            // MachineModuleStatus
            {
              IMonitoredMachineAnalysisStatus monitoredMachineAnalysisStatus =
                ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO.FindById (machine.Id);
              if (null == monitoredMachineAnalysisStatus) {
                monitoredMachineAnalysisStatus = ModelDAOHelper.ModelFactory
                  .CreateMonitoredMachineAnalysisStatus (machine);
              }
              monitoredMachineAnalysisStatus.ActivityAnalysisDateTime =
                UtcDateTime.From (2008, 01, 16, 00, 00, 00);
              ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
                .MakePersistent (monitoredMachineAnalysisStatus);
            }

            MonitoredMachineActivityAnalysis monitoredMachineActivityAnalysis =
              new MonitoredMachineActivityAnalysis (machine);
            {
              monitoredMachineActivityAnalysis.MakeAnalysis (CancellationToken.None);
            }
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
            DAOFactory.EmptyAccumulators ();

            {
              // OperationSlots
              IList<IOperationSlot> operationSlots =
                daoFactory.OperationSlotDAO.FindAll (machine);
              Assert.AreEqual (2, operationSlots.Count);
              int i = 0;
              Assert.AreEqual (machine.Id, operationSlots[i].Machine.Id);
              Assert.AreEqual (TimeSpan.FromMinutes (12), operationSlots[i].RunTime);
              ++i;
              Assert.AreEqual (machine.Id, operationSlots[i].Machine.Id);
              Assert.AreEqual (TimeSpan.FromMinutes (7), operationSlots[i].RunTime);
            }
            {
              // ActivitySummaries
              IList<MachineActivitySummary> activitySummaries =
                session.CreateCriteria<MachineActivitySummary> ()
                .Add (Restrictions.Eq ("Machine", machine))
                .AddOrder (Order.Asc ("Day"))
                .AddOrder (Order.Desc ("MachineMode"))
                .List<MachineActivitySummary> ();
              Assert.AreEqual (1101, activitySummaries.Count);
              int i = 0;
              Assert.AreEqual (machine.Id, activitySummaries[i].Machine.Id);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16), activitySummaries[i].Day);
              Assert.AreEqual (2, activitySummaries[i].MachineObservationState.Id);
              Assert.AreEqual (noDataMode, activitySummaries[i].MachineMode);
              Assert.AreEqual (33840, activitySummaries[i].Time.TotalSeconds);
              ++i;
              Assert.AreEqual (machine.Id, activitySummaries[i].Machine.Id);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16), activitySummaries[i].Day);
              Assert.AreEqual (2, activitySummaries[i].MachineObservationState.Id);
              Assert.AreEqual (autoMode, activitySummaries[i].MachineMode);
              Assert.AreEqual (3000, activitySummaries[i].Time.TotalSeconds);
              ++i;
              Assert.AreEqual (machine.Id, activitySummaries[i].Machine.Id);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16), activitySummaries[i].Day);
              Assert.AreEqual (2, activitySummaries[i].MachineObservationState.Id);
              Assert.AreEqual (inactiveMode, activitySummaries[i].MachineMode);
              Assert.AreEqual (2760, activitySummaries[i].Time.TotalSeconds);
              ++i;
              Assert.AreEqual (machine.Id, activitySummaries[i].Machine.Id);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 17), activitySummaries[i].Day);
              Assert.AreEqual (2, activitySummaries[i].MachineObservationState.Id);
              Assert.AreEqual (noDataMode, activitySummaries[i].MachineMode);
              Assert.AreEqual (86400, activitySummaries[i].Time.TotalSeconds);
              i = 1101 - 2;
              Assert.AreEqual (machine.Id, activitySummaries[i].Machine.Id);
              Assert.AreEqual (UtcDateTime.From (2011, 01, 16), activitySummaries[i].Day);
              Assert.AreEqual (2, activitySummaries[i].MachineObservationState.Id);
              Assert.AreEqual (autoMode, activitySummaries[i].MachineMode);
              Assert.AreEqual (3000, activitySummaries[i].Time.TotalSeconds);
              i = 1101 - 1;
              Assert.AreEqual (machine.Id, activitySummaries[i].Machine.Id);
              Assert.AreEqual (UtcDateTime.From (2011, 01, 16), activitySummaries[i].Day);
              Assert.AreEqual (2, activitySummaries[i].MachineObservationState.Id);
              Assert.AreEqual (inactiveMode, activitySummaries[i].MachineMode);
              Assert.AreEqual (2760, activitySummaries[i].Time.TotalSeconds);
            }

            // New ObservationStateSlot
            {
              IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
                .CreateMachineObservationStateAssociation (machine, attended, UtcDateTime.From (2008, 01, 16, 10, 20, 00));
              association.End = UtcDateTime.From (2008, 01, 16, 10, 22, 00);
              ((MachineObservationStateAssociation)association).Apply ();
            }
          }
          finally {
            transaction.Rollback ();
            Lemoine.Info.ConfigSet.ResetForceValues ();
            Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          }
        }
      }
    }

    /// <summary>
    /// Test a 'No Data' (in Fact) analysis
    /// </summary>
    [Test]
    public void TestNoDataAnalysis ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MonitoredMachineActivityAnalysisStateMachineExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MachineActivityAnalysisStateMachineExtension));

      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (3);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (3);
          Assert.NotNull (machineModule);
          MachineMode autoMode = session.Get<MachineMode> (3);
          Assert.NotNull (autoMode);
          MachineMode idleMode = session.Get<MachineMode> (1);
          Assert.NotNull (idleMode);
          MachineMode noDataMode = session.Get<MachineMode> (8);
          Assert.NotNull (noDataMode);
          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          MachineObservationState unattended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Unattended);

          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // MachineModuleStatus
          {
            MonitoredMachineAnalysisStatus analysisStatus =
              new MonitoredMachineAnalysisStatus (machine);
            analysisStatus.ActivityAnalysisDateTime =
              UtcDateTime.From (2008, 01, 16, 11, 26, 00);
            session.Save (analysisStatus);
          }

          // ObservationStateSlot
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended, UtcDateTime.From (2008, 01, 16, 16, 00, 00));
            association.End = UtcDateTime.From (2008, 01, 16, 17, 00, 00);
            ((MachineObservationStateAssociation)association).Apply ();
          }

          MonitoredMachineActivityAnalysis activityAnalysis =
            new MonitoredMachineActivityAnalysis (machine);

          {
            activityAnalysis.MakeAnalysis (CancellationToken.None);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
          }

          // Check the reason slots
          {
            IList<ReasonSlot> reasonSlots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.GreaterOrEqual (reasonSlots.Count, 6, "Number of reason slots");
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 26, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 31, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (autoMode, reasonSlots[i].MachineMode);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 31, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 36, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (idleMode, reasonSlots[i].MachineMode);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 36, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 16, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 16, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 17, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (attended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 17, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 01, 16, 10, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2011, 01, 16, 10, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 01, 16, 10, 05, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (idleMode, reasonSlots[i].MachineMode);
            ++i;
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test the current reason is correctly processed
    /// </summary>
    [Test]
    public void TestCurrentReason ()
    {
      try {
        Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
        Lemoine.Extensions.ExtensionManager.Add (typeof (NextMachineMode));
        Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MonitoredMachineActivityAnalysisStateMachineExtension));
        Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MachineActivityAnalysisStateMachineExtension));

        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          Lemoine.Info.ConfigSet.Load<LowerBound<DateTime>> ("Database.SlotDAO.FindOverlapsRangeStep.LowerLimit",
            new LowerBound<DateTime> (UtcDateTime.From (2008, 01, 01, 00, 00, 00)),
            true);

          ISession session = NHibernateHelper.GetCurrentSession ();
          try {
            // Reference data
            MonitoredMachine machine = session.Get<MonitoredMachine> (3);
            Assert.NotNull (machine);
            MachineModule machineModule = session.Get<MachineModule> (3);
            Assert.NotNull (machineModule);
            MachineMode autoMode = session.Get<MachineMode> (3);
            Assert.NotNull (autoMode);
            MachineMode idleMode = session.Get<MachineMode> (1);
            Assert.NotNull (idleMode);
            MachineMode noDataMode = session.Get<MachineMode> (8);
            Assert.NotNull (noDataMode);
            MachineObservationState attended =
              session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
            MachineObservationState unattended =
              session.Get<MachineObservationState> ((int)MachineObservationStateId.Unattended);
            Reason reasonMotion = session.Get<Reason> (2);
            Reason reasonShort = session.Get<Reason> (3);
            Reason reasonUnanswered = session.Get<Reason> (4);
            Reason reasonUnattended = session.Get<Reason> (5);
            Reason reasonTV = session.Get<Reason> (30);

            // Config
            Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                               (int)OperationSlotSplitOption.None);
            Lemoine.Info.ConfigSet
              .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

            // Production state
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""MonitoredMachineActivityAnalysis_TestCurrentReason_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""RunningMachineModeIsProduction"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""Score"": 10.0,
  ""ExcludeManual"": false,
  ""UnknownIsNotRunning"": false
          }
        }
      ]
    }
  ]
}
", true, true);
            var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
            var pluginsLoader = new Pulse.Extensions.PluginsLoader (assemblyLoader);
            var pluginFilter = new PluginFilterFromFlag (PluginFlag.Analysis);
            var extensionsProvider = new Lemoine.Extensions.ExtensionsProvider.ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, new Lemoine.Extensions.Plugin.DummyPluginsLoader ());
            Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            // Existing reason slots
            {
              var existingReasonSlot =
                new ReasonSlot (machine,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 11, 31, 00),
                                                      UtcDateTime.From (2008, 01, 16, 11, 32, 00)));
              existingReasonSlot.MachineMode = idleMode;
              existingReasonSlot.MachineObservationState = unattended;
              existingReasonSlot.Consolidate (null, null);
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (existingReasonSlot);
              AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
              Assert.AreEqual (reasonUnattended, existingReasonSlot.Reason);
              var productionStates1 = ModelDAOHelper.DAOFactory.ProductionStateDAO.FindAll ();
              var productionStateNoProduction1 = productionStates1.Single (p => p.TranslationKey.Equals ("ProductionStateNoProduction"));

              Assert.AreEqual (productionStateNoProduction1, existingReasonSlot.ProductionState);
            }

            var productionStates = ModelDAOHelper.DAOFactory.ProductionStateDAO.FindAll ();
            var productionStateProduction = productionStates.Single (p => p.TranslationKey.Equals ("ProductionStateProduction"));
            var productionStateNoProduction = productionStates.Single (p => p.TranslationKey.Equals ("ProductionStateNoProduction"));

            // MachineModuleStatus
            {
              IMonitoredMachineAnalysisStatus monitoredMachineAnalysisStatus = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
                .FindById (machine.Id);
              if (null == monitoredMachineAnalysisStatus) {
                monitoredMachineAnalysisStatus = ModelDAOHelper.ModelFactory
                  .CreateMonitoredMachineAnalysisStatus (machine);
              }
              monitoredMachineAnalysisStatus.ActivityAnalysisDateTime =
                UtcDateTime.From (2008, 01, 16, 11, 32, 00);
              ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO.MakePersistent (monitoredMachineAnalysisStatus);
            }

            // MachineStatus
            {
              MachineStatus machineStatus =
                new MachineStatus (machine);
              machineStatus.CncMachineMode = idleMode;
              machineStatus.MachineMode = idleMode;
              machineStatus.MachineObservationState = unattended;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonUnattended;
              machineStatus.ReasonSlotEnd =
                UtcDateTime.From (2008, 01, 16, 11, 32, 00);
              session.Save (machineStatus);
            }

            // ObservationStateSlot
            {
              IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
                .CreateMachineObservationStateAssociation (machine, attended,
                                                           new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 16, 00, 00),
                                                                                 UtcDateTime.From (2008, 01, 16, 17, 00, 00)));
              ((MachineObservationStateAssociation)association).Apply ();
            }

            // ReasonMachineAssociation
            {
              var range = new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 11, 31, 00));
              ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                .InsertManualReason (machine, range, reasonTV, 100.0, "");
              AnalysisUnitTests.RunMakeAnalysis ();
            }

            // Check the MachineStatus
            {
              MachineStatus machineStatus =
                session.Get<MachineStatus> (machine.Id);
              Assert.AreEqual (reasonTV, machineStatus.Reason);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 32, 00), machineStatus.ReasonSlotEnd);
              Assert.AreEqual (ReasonSource.Manual, machineStatus.ReasonSource);
              Assert.AreEqual (true, machineStatus.ConsolidationLimit.HasValue);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 36, 00), machineStatus.ConsolidationLimit.Value);
            }

            MonitoredMachineActivityAnalysis activityAnalysis =
              new MonitoredMachineActivityAnalysis (machine);

            {
              activityAnalysis.MakeAnalysis (CancellationToken.None);
            }
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);

            // Check the reason slots
            {
              IList<ReasonSlot> reasonSlots =
                session.CreateCriteria<ReasonSlot> ()
                .Add (Restrictions.Eq ("Machine", machine))
                .AddOrder (Order.Asc ("DateTimeRange"))
                .List<ReasonSlot> ();
              Assert.GreaterOrEqual (reasonSlots.Count, 5, "Number of reason slots");
              int i = 0;
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 31, 00), reasonSlots[i].BeginDateTime.Value);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 36, 00), reasonSlots[i].EndDateTime.Value);
              Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
              Assert.AreEqual (idleMode, reasonSlots[i].MachineMode);
              Assert.AreEqual (reasonTV, reasonSlots[i].Reason);
              Assert.AreEqual (false, reasonSlots[i].DefaultReason);
              Assert.AreEqual (productionStateNoProduction, reasonSlots[i].ProductionState);

              ++i;
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 11, 36, 00), reasonSlots[i].BeginDateTime.Value);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 16, 00, 00), reasonSlots[i].EndDateTime.Value);
              Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
              Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
              Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
              Assert.AreEqual (true, reasonSlots[i].DefaultReason);
              Assert.AreEqual (null, reasonSlots[i].ProductionState);
              ++i;
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 16, 00, 00), reasonSlots[i].BeginDateTime.Value);
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 17, 00, 00), reasonSlots[i].EndDateTime.Value);
              Assert.AreEqual (attended, reasonSlots[i].MachineObservationState);
              Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
              Assert.AreEqual (reasonUnanswered, reasonSlots[i].Reason);
              Assert.AreEqual (true, reasonSlots[i].DefaultReason);
              Assert.AreEqual (null, reasonSlots[i].ProductionState);
              ++i;
              Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 17, 00, 00), reasonSlots[i].BeginDateTime.Value);
              Assert.AreEqual (UtcDateTime.From (2011, 01, 16, 10, 00, 00), reasonSlots[i].EndDateTime.Value);
              Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
              Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
              Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
              Assert.AreEqual (true, reasonSlots[i].DefaultReason);
              Assert.AreEqual (null, reasonSlots[i].ProductionState);
              ++i;
              Assert.AreEqual (UtcDateTime.From (2011, 01, 16, 10, 00, 00), reasonSlots[i].BeginDateTime.Value);
              Assert.AreEqual (UtcDateTime.From (2011, 01, 16, 10, 05, 00), reasonSlots[i].EndDateTime.Value);
              Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
              Assert.AreEqual (idleMode, reasonSlots[i].MachineMode);
              Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
              Assert.AreEqual (true, reasonSlots[i].DefaultReason);
              Assert.AreEqual (productionStateNoProduction, reasonSlots[i].ProductionState);
              ++i;
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
      }
    }

    /* Note: to test the private method, you can use something like:
     * System.Reflection.MethodInfo methodInfo1 =
              machineModuleActivityAnalysis.GetType().GetMethod("PurgeAndGetAutoSequences",
                                                                System.Reflection.BindingFlags.NonPublic
                                                                | System.Reflection.BindingFlags.Instance);
            
            Assert.AreNotEqual(null, methodInfo1);
            
            IList<IAutoSequence> autoSequences =
              (IList<IAutoSequence>) methodInfo1.Invoke(machineModuleActivityAnalysis,
                                                        new object[] { myDaoSession });
     */

    /// <summary>
    /// Test the AutoMachineStateTemplate process
    /// </summary>
    [Test]
    public void TestAutoMachineStateTemplate ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MonitoredMachineActivityAnalysisStateMachineExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MachineActivityAnalysisStateMachineExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AutoMachineStateTemplateConfig.CncActivityExtension));

      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""TestAutoMachineStateTemplate"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""AutoMachineStateTemplateConfig"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
          }
        }
      ]
    }
  ]
}
", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          Lemoine.Info.ConfigSet.Load<TimeSpan> ("Analysis.Activity.MachineStateTemplates.MaxTime",
                                                 TimeSpan.FromMinutes (3),
                                                 true);
          Lemoine.Info.ConfigSet.Load<TimeSpan> ("MachineStateTemplate.Process.MaxRange",
                                                 TimeSpan.FromDays (3 * 365),
                                                 true);

          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (3);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (3);
          Assert.NotNull (machineModule);
          MachineMode autoMode = session.Get<MachineMode> (3);
          Assert.NotNull (autoMode);
          MachineMode idleMode = session.Get<MachineMode> (1);
          Assert.NotNull (idleMode);
          IMachineStateTemplate mstUnattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (2);
          Assert.NotNull (mstUnattended);
          IMachineStateTemplate mstAttended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (1);
          Assert.NotNull (mstAttended);
          IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);
          IMachineObservationState unattended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonUnattended = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (5);
          IReason reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (2);

          // Config
          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)OperationSlotSplitOption.None);
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // Configuration AutoMachineStateTemplate
          IAutoMachineStateTemplate autoMachineStateTemplate =
            ModelDAOHelper.ModelFactory.CreateAutoMachineStateTemplate (autoMode, mstAttended);
          autoMachineStateTemplate.Current = mstUnattended;
          ModelDAOHelper.DAOFactory.AutoMachineStateTemplateDAO.MakePersistent (autoMachineStateTemplate);

          // Existing reason slots
          {
            ReasonSlot existingReasonSlot =
              new ReasonSlot (machine,
                              new UtcDateTimeRange (UtcDateTime.From (2008, 01, 01, 12, 00, 00),
                                                    UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
            existingReasonSlot.MachineMode = idleMode;
            existingReasonSlot.MachineObservationState = unattended;
            existingReasonSlot.Consolidate (null, null);
            session.Save (existingReasonSlot);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
            Assert.AreEqual (reasonUnattended, existingReasonSlot.Reason);
          }
          {
            IMachineActivitySummary summary = ModelDAOHelper.ModelFactory
              .CreateMachineActivitySummary (machine, UtcDateTime.From (2008, 01, 01), unattended, idleMode);
            summary.Time = TimeSpan.FromHours (4);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          {
            ReasonSummary summary =
              new ReasonSummary (machine, UtcDateTime.From (2008, 01, 01), null, unattended, reasonUnattended);
            summary.Time = TimeSpan.FromHours (4);
            summary.Number = 1;
            session.Save (summary);
          }

          // MachineModuleStatus
          {
            IMonitoredMachineAnalysisStatus analysisStatus = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
              .FindById (machine.Id);
            if (null == analysisStatus) {
              analysisStatus = ModelDAOHelper.ModelFactory
                .CreateMonitoredMachineAnalysisStatus (machine);
            }
            analysisStatus.ActivityAnalysisDateTime =
              UtcDateTime.From (2008, 01, 01, 16, 00, 00);
            ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO.MakePersistent (analysisStatus);
          }

          // MachineStatus
          {
            MachineStatus machineStatus =
              new MachineStatus (machine);
            machineStatus.CncMachineMode = idleMode;
            machineStatus.MachineMode = idleMode;
            machineStatus.MachineStateTemplate = mstUnattended;
            machineStatus.MachineObservationState = unattended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnattended;
            machineStatus.ReasonSource = ReasonSource.DefaultAuto;
            machineStatus.AutoReasonNumber = 1;
            machineStatus.ReasonScore = 90;
            machineStatus.ReasonSlotEnd =
              UtcDateTime.From (2008, 01, 01, 16, 00, 00);
            session.Save (machineStatus);
          }

          // New IFact active...
          {
            IList<IFact> facts = ModelDAOHelper.DAOFactory.FactDAO
              .FindAllInUtcRange (machine, new UtcDateTimeRange (UtcDateTime.From (0)));
            foreach (IFact fact in facts) {
              ModelDAOHelper.DAOFactory.FactDAO.MakeTransient (fact);
            }
          }
          {
            IFact fact = ModelDAOHelper.ModelFactory.CreateFact (machine,
                                                                 UtcDateTime.From (2008, 01, 01, 16, 00, 00),
                                                                 UtcDateTime.From (2008, 01, 01, 17, 00, 00),
                                                                 autoMode);
            ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
          }

          // First analysis:
          // - machine status did not change but there is a new machine state template
          MonitoredMachineActivityAnalysis activityAnalysis =
            new MonitoredMachineActivityAnalysis (machine);
          {
            activityAnalysis.MakeAnalysis (CancellationToken.None);
          }
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);

          // Check the MachineStatus
          {
            IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
              .FindById (machine.Id);
            Assert.AreEqual (reasonUnattended, machineStatus.Reason);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 16, 00, 00), machineStatus.ReasonSlotEnd);
            Assert.AreEqual (ReasonSource.DefaultAuto, machineStatus.ReasonSource);
            Assert.AreEqual (new UpperBound<DateTime> (), machineStatus.ConsolidationLimit);
            Assert.AreEqual (mstUnattended, machineStatus.MachineStateTemplate);
            Assert.AreEqual (unattended, machineStatus.MachineObservationState);
          }

          // But the observation state slots were updated
          {
            IList<IObservationStateSlot> stateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (machine, new UtcDateTimeRange (UtcDateTime.From (0)));
            Assert.AreEqual (2, stateSlots.Count);
            // TODO: check them
          }

          // Second analysis
          {
            activityAnalysis.MakeAnalysis (CancellationToken.None);
          }
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);

          // Check the MachineStatus
          {
            IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
              .FindById (machine.Id);
            Assert.AreEqual (reasonMotion, machineStatus.Reason);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 17, 00, 00), machineStatus.ReasonSlotEnd);
            Assert.AreEqual (ReasonSource.DefaultAuto | ReasonSource.DefaultIsAuto, machineStatus.ReasonSource);
            Assert.AreEqual (new UpperBound<DateTime> (), machineStatus.ConsolidationLimit);
            Assert.AreEqual (mstAttended, machineStatus.MachineStateTemplate);
            Assert.AreEqual (attended, machineStatus.MachineObservationState);
          }

          DAOFactory.EmptyAccumulators ();

          // Note: cut-off time is at 21:00 UTC

          // Check the reason slots
          {
            IList<ReasonSlot> reasonSlots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.AreEqual (2, reasonSlots.Count);
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 12, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 16, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (idleMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 16, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 17, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (attended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (autoMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonMotion, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            ++i;
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test the reason is updated from short to unanswered
    /// </summary>
    [Test]
    public void TestFromShortToUnanswered ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Plugin.DefaultAccumulators.DefaultAccumulators.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MonitoredMachineActivityAnalysisStateMachineExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MachineActivityAnalysisStateMachineExtension));

      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (3);
          Assert.NotNull (machine);
          IMachineModule machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindById (3);
          Assert.NotNull (machineModule);
          IMachineMode idleMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (1);
          Assert.NotNull (idleMode);
          IMachineStateTemplate mstAttended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (1);
          Assert.NotNull (mstAttended);
          IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);
          IReason reasonShort = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (4);

          // Config
          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)OperationSlotSplitOption.None);
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // ObservationStateSlot
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended, UtcDateTime.From (2008, 01, 01, 12, 00, 00));
            association.End = UtcDateTime.From (2008, 01, 01, 17, 00, 00);
            ((MachineObservationStateAssociation)association).Apply ();
          }

          {
            // - ReasonSummary
            IList<IReasonSummary> summaries = NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<IReasonSummary> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("Day"))
              .AddOrder (Order.Asc ("Reason.Id"))
              .List<IReasonSummary> ();
            Assert.AreEqual (0, summaries.Count);
          }

          // Existing reason slots
          {
            IReasonSlot existingReasonSlot = ModelDAOHelper.ModelFactory
            .CreateReasonSlot (machine,
                               new UtcDateTimeRange (UtcDateTime.From (2008, 01, 01, 12, 00, 00),
                                                     UtcDateTime.From (2008, 01, 01, 12, 00, 05)));
            existingReasonSlot.MachineMode = idleMode;
            existingReasonSlot.MachineObservationState = attended;
            ((ReasonSlot)existingReasonSlot).Consolidate (null, null);
            ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (existingReasonSlot);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
            Assert.AreEqual (reasonShort, existingReasonSlot.Reason);
          }

          // MachineModuleStatus
          {
            IMonitoredMachineAnalysisStatus analysisStatus = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
              .FindById (machine.Id);
            if (null == analysisStatus) {
              analysisStatus = ModelDAOHelper.ModelFactory
                .CreateMonitoredMachineAnalysisStatus (machine);
            }
            analysisStatus.ActivityAnalysisDateTime =
              UtcDateTime.From (2008, 01, 01, 12, 00, 05);
            ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO.MakePersistent (analysisStatus);
          }

          // MachineStatus
          {
            IMachineStatus machineStatus = ModelDAOHelper.ModelFactory.CreateMachineStatus (machine);
            machineStatus.CncMachineMode = idleMode;
            machineStatus.MachineMode = idleMode;
            machineStatus.MachineStateTemplate = mstAttended;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonShort;
            machineStatus.ReasonSlotEnd =
              UtcDateTime.From (2008, 01, 01, 12, 00, 05);
            machineStatus.ConsolidationLimit = UtcDateTime.From (2008, 01, 01, 12, 01, 00);
            ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New IFact active...
          {
            IList<IFact> facts = ModelDAOHelper.DAOFactory.FactDAO
              .FindAllInUtcRange (machine, new UtcDateTimeRange (UtcDateTime.From (0)));
            foreach (IFact fact in facts) {
              ModelDAOHelper.DAOFactory.FactDAO.MakeTransient (fact);
            }
          }
          {
            IFact fact = ModelDAOHelper.ModelFactory.CreateFact (machine,
                                                                 UtcDateTime.From (2008, 01, 01, 12, 00, 05),
                                                                 UtcDateTime.From (2008, 01, 01, 17, 00, 00),
                                                                 idleMode);
            ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
          }

          // First analysis:
          // - machine status did not change but there is a new machine state template
          MonitoredMachineActivityAnalysis activityAnalysis =
            new MonitoredMachineActivityAnalysis (machine);
          activityAnalysis.ThreadExecution = false;

          {
            activityAnalysis.MakeAnalysis (CancellationToken.None);
          }

          // Second analysis
          {
            activityAnalysis.MakeAnalysis (CancellationToken.None);
          }

          // Modifications
          ModelDAOHelper.DAOFactory.FlushData ();
          RunMakeAnalysis ();
          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();
          ModelDAOHelper.DAOFactory.FlushData ();

          // Check the MachineStatus
          {
            IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
              .FindById (machine.Id);
            Assert.AreEqual (reasonUnanswered, machineStatus.Reason);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 17, 00, 00), machineStatus.ReasonSlotEnd);
            Assert.AreEqual (ReasonSource.Default, machineStatus.ReasonSource);
            Assert.IsFalse (machineStatus.ConsolidationLimit.HasValue);
            Assert.AreEqual (null, machineStatus.MachineStateTemplate);
            Assert.AreEqual (attended, machineStatus.MachineObservationState);
          }

          // Note: cut-off time is at 21:00 UTC

          // Check the reason slots
          {
            IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine, new UtcDateTimeRange (new DateTime (2008, 01, 01, 12, 00, 00),
                                                                 new DateTime (2008, 01, 01, 17, 00, 00)));
            Assert.AreEqual (1, reasonSlots.Count);
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 12, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 17, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (attended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (idleMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnanswered, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            ++i;
          }
          {
            // - ReasonSummary
            IList<IReasonSummary> summaries = NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<IReasonSummary> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("Day"))
              .AddOrder (Order.Asc ("Reason.Id"))
              .List<IReasonSummary> ();
            Assert.AreEqual (1, summaries.Count);
            int i = 0;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (5), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
            ++i;
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test with a new machine observation state
    /// </summary>
    [Test]
    public void TestNewMachineObservationState ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Plugin.DefaultAccumulators.DefaultAccumulators.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MonitoredMachineActivityAnalysisStateMachineExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MachineActivityAnalysisStateMachineExtension));

      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (3);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (3);
          Assert.NotNull (machineModule);
          MachineMode autoMode = session.Get<MachineMode> (3);
          Assert.NotNull (autoMode);
          MachineMode idleMode = session.Get<MachineMode> (1);
          Assert.NotNull (idleMode);
          MachineMode noDataMode = session.Get<MachineMode> (8);
          Assert.NotNull (noDataMode);
          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          MachineObservationState unattended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Unattended);
          Reason reasonUnanswered = session.Get<Reason> (4);
          Reason reasonUnattended = session.Get<Reason> (5);

          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // Existing reason slots
          {
            ReasonSlot existingReasonSlot =
              new ReasonSlot (machine,
                              new UtcDateTimeRange (UtcDateTime.From (2008, 01, 01, 12, 00, 00),
                                                    UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
            existingReasonSlot.MachineMode = noDataMode;
            existingReasonSlot.MachineObservationState = unattended;
            existingReasonSlot.Consolidate (null, null);
            session.Save (existingReasonSlot);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
            Assert.AreEqual (reasonUnattended, existingReasonSlot.Reason);
          }
          {
            var machineActivitySummary = ModelDAOHelper.ModelFactory
              .CreateMachineActivitySummary (machine, new DateTime (2008, 01, 01), unattended, noDataMode);
            machineActivitySummary.Time = TimeSpan.FromHours (4);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (machineActivitySummary);
          }

          // MachineModuleStatus
          {
            MonitoredMachineAnalysisStatus analysisStatus =
              new MonitoredMachineAnalysisStatus (machine);
            analysisStatus.ActivityAnalysisDateTime =
              UtcDateTime.From (2008, 01, 01, 16, 00, 00);
            session.Save (analysisStatus);
          }

          // MachineStatus
          {
            MachineStatus machineStatus =
              new MachineStatus (machine);
            machineStatus.CncMachineMode = noDataMode;
            machineStatus.MachineMode = noDataMode;
            machineStatus.MachineObservationState = unattended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnattended;
            machineStatus.ReasonSource = ReasonSource.DefaultAuto | ReasonSource.DefaultIsAuto;
            machineStatus.AutoReasonNumber = 1;
            machineStatus.ReasonScore = 90;
            machineStatus.ReasonSlotEnd =
              UtcDateTime.From (2008, 01, 01, 16, 00, 00);
            session.Save (machineStatus);
          }

          // ObservationStateSlot
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended, UtcDateTime.From (2008, 01, 01, 21, 00, 00));
            association.End = UtcDateTime.From (2008, 01, 01, 23, 00, 00);
            ((MachineObservationStateAssociation)association).Apply ();
          }

          // Check the MachineStatus
          {
            MachineStatus machineStatus =
              session.Get<MachineStatus> (machine.Id);
            Assert.AreEqual (reasonUnattended, machineStatus.Reason);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 16, 00, 00), machineStatus.ReasonSlotEnd);
            Assert.AreEqual (ReasonSource.DefaultAuto | ReasonSource.DefaultIsAuto, machineStatus.ReasonSource);
            Assert.AreEqual (new UpperBound<DateTime> (), machineStatus.ConsolidationLimit);
            Assert.AreEqual (unattended, machineStatus.MachineObservationState);
          }

          MonitoredMachineActivityAnalysis activityAnalysis =
            new MonitoredMachineActivityAnalysis (machine);

          {
            activityAnalysis.MakeAnalysis (CancellationToken.None);
          }
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
          DAOFactory.EmptyAccumulators ();

          // Note: cut-off time is at 21:00 UTC

          // Check the reason slots
          {
            IList<ReasonSlot> reasonSlots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 12, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 21, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 21, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 23, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (attended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnanswered, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 23, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 10, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
          }
          {
            // - ReasonSummary
            IList<IReasonSummary> summaries =
              session.CreateCriteria<IReasonSummary> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("Day"))
              .AddOrder (Order.Asc ("Reason.Id"))
              .List<IReasonSummary> ();
            int i = 0;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (9), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (2), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (22), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
          }
          {
            // - MachineActivitySummary
            IList<IMachineActivitySummary> summaries =
              session.CreateCriteria<IMachineActivitySummary> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("Day"))
              .AddOrder (Order.Asc ("MachineObservationState.Id"))
              .List<IMachineActivitySummary> ();
            int i = 0;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (TimeSpan.FromHours (9), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (TimeSpan.FromHours (2), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (TimeSpan.FromHours (22), summaries[i].Time);
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test with a new machine observation state with a shift
    /// </summary>
    [Test]
    public void TestNewMachineObservationStateShift ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Plugin.DefaultAccumulators.DefaultAccumulators.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MonitoredMachineActivityAnalysisStateMachineExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.AnalysisStateMachineProductionShop.MachineActivityAnalysisStateMachineExtension));

      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IShift shift1 =
            daoFactory.ShiftDAO.FindById (1);
          IShift shift2 =
            daoFactory.ShiftDAO.FindById (2);
          IShift shift3 =
            daoFactory.ShiftDAO.FindById (3);
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (3);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (3);
          Assert.NotNull (machineModule);
          MachineMode autoMode = session.Get<MachineMode> (3);
          Assert.NotNull (autoMode);
          MachineMode idleMode = session.Get<MachineMode> (1);
          Assert.NotNull (idleMode);
          MachineMode noDataMode = session.Get<MachineMode> (8);
          Assert.NotNull (noDataMode);
          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          MachineObservationState unattended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Unattended);
          Reason reasonUnanswered = session.Get<Reason> (4);
          Reason reasonUnattended = session.Get<Reason> (5);

          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // Existing reason slots
          {
            ReasonSlot existingReasonSlot =
              new ReasonSlot (machine,
                              new UtcDateTimeRange (UtcDateTime.From (2008, 01, 01, 12, 00, 00),
                                                    UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
            existingReasonSlot.MachineMode = noDataMode;
            existingReasonSlot.MachineObservationState = unattended;
            existingReasonSlot.Shift = shift1;
            existingReasonSlot.Consolidate (null, null);
            session.Save (existingReasonSlot);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
            Assert.AreEqual (reasonUnattended, existingReasonSlot.Reason);
          }
          {
            var machineActivitySummary = ModelDAOHelper.ModelFactory
              .CreateMachineActivitySummary (machine, new DateTime (2008, 01, 01), unattended, noDataMode, shift1);
            machineActivitySummary.Time = TimeSpan.FromHours (4);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (machineActivitySummary);
          }

          // MachineModuleStatus
          {
            MonitoredMachineAnalysisStatus analysisStatus =
              new MonitoredMachineAnalysisStatus (machine);
            analysisStatus.ActivityAnalysisDateTime =
              UtcDateTime.From (2008, 01, 01, 16, 00, 00);
            session.Save (analysisStatus);
          }

          // MachineStatus
          {
            MachineStatus machineStatus =
              new MachineStatus (machine);
            machineStatus.CncMachineMode = noDataMode;
            machineStatus.MachineMode = noDataMode;
            machineStatus.MachineObservationState = unattended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnattended;
            machineStatus.ReasonSource = ReasonSource.DefaultAuto;
            machineStatus.ReasonScore = 90;
            machineStatus.AutoReasonNumber = 1;
            machineStatus.Shift = shift1;
            machineStatus.ReasonSlotEnd =
              UtcDateTime.From (2008, 01, 01, 16, 00, 00);
            session.Save (machineStatus);
          }

          // ObservationStateSlot
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended, UtcDateTime.From (2008, 01, 01, 21, 00, 00));
            association.End = UtcDateTime.From (2008, 01, 01, 23, 00, 00);
            association.Shift = shift2;
            ((MachineObservationStateAssociation)association).Apply ();
          }

          // Check the MachineStatus
          {
            MachineStatus machineStatus =
              session.Get<MachineStatus> (machine.Id);
            Assert.AreEqual (reasonUnattended, machineStatus.Reason);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 16, 00, 00), machineStatus.ReasonSlotEnd);
            Assert.AreEqual (ReasonSource.DefaultAuto, machineStatus.ReasonSource);
            Assert.AreEqual (new UpperBound<DateTime> (), machineStatus.ConsolidationLimit);
            Assert.AreEqual (unattended, machineStatus.MachineObservationState);
          }

          MonitoredMachineActivityAnalysis activityAnalysis =
            new MonitoredMachineActivityAnalysis (machine);

          {
            activityAnalysis.MakeAnalysis (CancellationToken.None);
          }
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (machine);
          DAOFactory.EmptyAccumulators ();

          // Note: cut-off time is at 21:00 UTC

          // Check the reason slots
          {
            IList<ReasonSlot> reasonSlots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 12, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), reasonSlots[i].BeginDay.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 16, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), reasonSlots[i].EndDay.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            Assert.AreEqual (shift1, reasonSlots[i].Shift);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 16, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), reasonSlots[i].BeginDay.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 21, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), reasonSlots[i].EndDay.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            Assert.AreEqual (null, reasonSlots[i].Shift);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 21, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), reasonSlots[i].BeginDay.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 23, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), reasonSlots[i].EndDay.Value);
            Assert.AreEqual (attended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnanswered, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            Assert.AreEqual (shift2, reasonSlots[i].Shift);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01, 23, 00, 00), reasonSlots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), reasonSlots[i].BeginDay.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 10, 00, 00), reasonSlots[i].EndDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 16), reasonSlots[i].EndDay.Value);
            Assert.AreEqual (unattended, reasonSlots[i].MachineObservationState);
            Assert.AreEqual (noDataMode, reasonSlots[i].MachineMode);
            Assert.AreEqual (reasonUnattended, reasonSlots[i].Reason);
            Assert.AreEqual (true, reasonSlots[i].DefaultReason);
            Assert.AreEqual (null, reasonSlots[i].Shift);
          }
          {
            // - ReasonSummary
            IList<IReasonSummary> summaries =
              session.CreateCriteria<IReasonSummary> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("Day"))
              .AddOrder (Order.Asc ("Shift"))
              .AddOrder (Order.Asc ("Reason.Id"))
              .List<IReasonSummary> ();
            int i = 0;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, summaries[i].Reason);
            Assert.AreEqual (shift1, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (4), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, summaries[i].Reason);
            Assert.AreEqual (null, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (5), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (2), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (22), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
          }
          {
            // - MachineActivitySummary
            IList<IMachineActivitySummary> summaries =
              session.CreateCriteria<IMachineActivitySummary> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Asc ("Day"))
              .AddOrder (Order.Asc ("MachineObservationState.Id"))
              .List<IMachineActivitySummary> ();
            int i = 0;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (shift1, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (4), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 01), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (null, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (5), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (shift2, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (2), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2008, 01, 02), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (null, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (22), summaries[i].Time);
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test 1 for priority of modification for analysis
    /// </summary>
    [Test]
    public void TestModificationAnalysisPriority1 ()
    {
      for (int i = 1; i <= 6; i++) {
        for (int j = 1; j <= 6; j++) {
          TestModificationAnalysisPriority (i, j);
        }
      }
    }

    void TestModificationAnalysisPriority (int shuffleIdIndex, int shuffleDateIndex)
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {

          DateTime currentDate = UtcDateTime.From (2008, 01, 01, 12, 00, 00);

          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (3);
          Assert.NotNull (machine);

          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          Assert.NotNull (attended);

          DateTime date1, date2, date3;

          switch (shuffleDateIndex) {
            case 1:
              date1 = currentDate;
              date2 = currentDate.AddSeconds (1);
              date3 = currentDate.AddSeconds (2);
              break;
            case 2:
              date1 = currentDate.AddSeconds (1);
              date2 = currentDate;
              date3 = currentDate.AddSeconds (2);
              break;
            case 3:
              date1 = currentDate;
              date2 = currentDate;
              date3 = currentDate.AddSeconds (2);
              break;
            case 4:
              date1 = currentDate.AddSeconds (2);
              date2 = currentDate;
              date3 = currentDate.AddSeconds (1);
              break;
            case 5:
              date1 = currentDate;
              date2 = currentDate;
              date3 = currentDate;
              break;
            default:
              date1 = currentDate.AddSeconds (2);
              date2 = currentDate.AddSeconds (1);
              date3 = currentDate;
              break;
          }

          IMachineObservationStateAssociation machineObsAssoc1 =
            ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine, attended,
                                                      date1);
          machineObsAssoc1.Priority = 50;

          IMachineObservationStateAssociation machineObsAssoc2 =
            ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine, attended,
                                                      date2);

          machineObsAssoc2.Priority = 100;

          IMachineObservationStateAssociation machineObsAssoc3 =
            ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine, attended,
                                                      date3);

          machineObsAssoc3.Priority = 200;

          switch (shuffleIdIndex) {
            case 1:
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc1);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc2);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc3);
              break;
            case 2:
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc3);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc2);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc1);
              break;
            case 3:
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc1);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc3);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc2);
              break;
            case 4:
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc3);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc1);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc2);
              break;
            case 5:
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc2);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc3);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc1);
              break;
            default:
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc2);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc1);
              daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssoc3);
              break;
          }
          ModelDAOHelper.DAOFactory.FlushData ();

          // should process machineObsAssoc3 before machineObsAssoc2 before machineObsAssoc1

          currentDate = currentDate.AddSeconds (5);

          DateTime lastModificationDateTime = DateTime.MinValue;
          long lastModificationId = 0;
          int lastPriority = Int32.MaxValue;

          PendingMachineModificationAnalysis pendingModificationAnalysis =
            new PendingMachineModificationAnalysis (machine, false);
          IModification currentModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetFirstPendingModification (machine,
                                         lastModificationId,
                                         lastPriority, 0);

          Assert.AreEqual (((Lemoine.Collections.IDataWithId<long>)machineObsAssoc3).Id, ((Lemoine.Collections.IDataWithId<long>)currentModification).Id,
                          "Modification with highest priority should be treated first");

          lastModificationId = ((Lemoine.Collections.IDataWithId<long>)currentModification).Id;
          lastModificationDateTime = currentModification.DateTime;
          lastPriority = currentModification.StatusPriority;

          currentModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetFirstPendingModification (machine,
                                         lastModificationId,
                                         lastPriority, 0);

          Assert.AreEqual (((Lemoine.Collections.IDataWithId<long>)machineObsAssoc2).Id, ((Lemoine.Collections.IDataWithId<long>)currentModification).Id,
                          "Modification with second highest priority should be treated second");

          lastModificationId = ((Lemoine.Collections.IDataWithId<long>)currentModification).Id;
          lastModificationDateTime = currentModification.DateTime;
          lastPriority = currentModification.StatusPriority;

          currentModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetFirstPendingModification (machine,
                                         lastModificationId,
                                         lastPriority, 0);

          Assert.IsNotNull (machineObsAssoc1);
          Assert.IsNotNull (currentModification);
          Assert.AreEqual (((Lemoine.Collections.IDataWithId<long>)machineObsAssoc1).Id, ((Lemoine.Collections.IDataWithId<long>)currentModification).Id,
                          "Modification with lowest priority should be treated last");
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    /// <summary>
    /// Test 2 for priority of modification for analysis
    /// </summary>
    [Test]
    public void TestModificationAnalysisPriority2 ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          DateTime currentDate = UtcDateTime.From (2008, 01, 01, 12, 00, 00);

          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (3);
          Assert.NotNull (machine);

          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          Assert.NotNull (attended);

          IMachineObservationStateAssociation[] machineObsAssocArray
            = new IMachineObservationStateAssociation[10];

          for (int i = 0; i < 10; i++) {
            machineObsAssocArray[i] =
              ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended,
                                                        currentDate);
            Assert.AreNotEqual (null, machineObsAssocArray[i]);
            daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssocArray[i]);
          }

          machineObsAssocArray[5].StatusPriority = 1000;
          machineObsAssocArray[8].StatusPriority = 1000;
          machineObsAssocArray[3].StatusPriority = 50;
          machineObsAssocArray[2].StatusPriority = 10;
          machineObsAssocArray[7].StatusPriority = 50;
          for (int i = 0; i < 10; i++) {
            daoFactory.MachineObservationStateAssociationDAO.MakePersistent (machineObsAssocArray[i]);
          }
          ModelDAOHelper.DAOFactory.FlushData ();

          // order of analysis should be 5, 8, 0, 1, 4, 6, 9, 3, 7, 2
          int[] oracleId = new int[10] { 5, 8, 0, 1, 4, 6, 9, 3, 7, 2 };

          long lastModificationId = 0;
          int lastPriority = Int32.MaxValue;

          PendingMachineModificationAnalysis pendingModificationAnalysis =
            new PendingMachineModificationAnalysis (machine, false);
          for (int i = 0; i < 10; i++) {
            IModification currentModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
              .GetFirstPendingModification (machine,
                                           lastModificationId,
                                           lastPriority, 0);

            Assert.AreNotEqual (null, currentModification);
            Assert.AreEqual (((Lemoine.Collections.IDataWithId<long>)machineObsAssocArray[oracleId[i]]).Id,
                            ((Lemoine.Collections.IDataWithId<long>)currentModification).Id,
                            String.Format ("Error at index {0}", i));

            lastModificationId = ((Lemoine.Collections.IDataWithId<long>)currentModification).Id;
            lastPriority = currentModification.StatusPriority;
          }

        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    /// <summary>
    /// Test the trim of auto-sequence periods
    /// </summary>
    [Test]
    public void TestTrim ()
    {
      // Reference data
      IMonitoredMachine machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
      IMachineMode active = ModelDAOHelper.ModelFactory.CreateMachineModeFromName ("Active", true);
      active.AutoSequence = true;
      IMachineMode inactive = ModelDAOHelper.ModelFactory.CreateMachineModeFromName ("Inactive", false);
      inactive.AutoSequence = false;

      {
        IList<IAutoSequencePeriod> periods = new List<IAutoSequencePeriod> ();
        IEnumerable<IAutoSequencePeriod> trimmed =
          AutoSequenceAnalysis.Trim (periods);
        Assert.IsFalse (trimmed.Any ());
      }
      {
        IList<IAutoSequencePeriod> periods = new List<IAutoSequencePeriod> ();
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (2), inactive));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (3), T (4), inactive));
        IEnumerable<IAutoSequencePeriod> trimmed =
          AutoSequenceAnalysis.Trim (periods);
        Assert.IsFalse (trimmed.Any ());
      }
      {
        IList<IAutoSequencePeriod> periods = new List<IAutoSequencePeriod> ();
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (2), active));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (3), T (4), active));
        IEnumerable<IAutoSequencePeriod> trimmed =
          AutoSequenceAnalysis.Trim (periods);
        Assert.AreEqual (2, trimmed.Count ());
      }
      {
        IList<IAutoSequencePeriod> periods = new List<IAutoSequencePeriod> ();
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (2), inactive));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (2), T (4), active));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (4), T (5), active));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (8), inactive));
        IEnumerable<IAutoSequencePeriod> trimmed =
          AutoSequenceAnalysis.Trim (periods);
        Assert.AreEqual (2, trimmed.Count ());
      }
    }

    /// <summary>
    /// Get the next auto-sequence periods, test
    /// </summary>
    [Test]
    public void TestGetNextAutoSequencePeriods ()
    {
      // Reference data
      IMonitoredMachine machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
      IMachineMode active = ModelDAOHelper.ModelFactory.CreateMachineModeFromName ("Active", true);
      active.AutoSequence = true;
      IMachineMode inactive = ModelDAOHelper.ModelFactory.CreateMachineModeFromName ("Inactive", false);
      inactive.AutoSequence = false;
      IMachineModule machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (machine, "MachineModule");
      ISequence sequenceAuto = ModelDAOHelper.ModelFactory.CreateSequence ("auto");
      sequenceAuto.AutoOnly = true;

      IEnumerable<IAutoSequence> autoSequences;
      {
        IList<IAutoSequence> autoSequenceList = new List<IAutoSequence> ();
        IAutoSequence autoSequence;
        autoSequence = ModelDAOHelper.ModelFactory.CreateAutoSequence (machineModule, sequenceAuto, T (1));
        autoSequence.End = T (3);
        autoSequenceList.Add (autoSequence);
        autoSequence = ModelDAOHelper.ModelFactory.CreateAutoSequence (machineModule, sequenceAuto, T (10));
        autoSequence.End = T (15);
        autoSequenceList.Add (autoSequence);
        autoSequence = ModelDAOHelper.ModelFactory.CreateAutoSequence (machineModule, sequenceAuto, T (16));
        autoSequence.End = T (17);
        autoSequenceList.Add (autoSequence);
        autoSequence = ModelDAOHelper.ModelFactory.CreateAutoSequence (machineModule, sequenceAuto, T (18));
        autoSequence.End = T (20);
        autoSequenceList.Add (autoSequence);
        autoSequences = autoSequenceList;
      }

      IEnumerable<IAutoSequencePeriod> periods;
      {
        IList<IAutoSequencePeriod> periodList = new List<IAutoSequencePeriod> ();
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (1.1), T (1.2), inactive));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (2), T (4), active));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (4), T (5), inactive));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (5), T (6), active));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (8), active));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (8), T (8.5), inactive));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (8.5), T (9), inactive));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (9), T (11), active));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (12), T (13), active));
        periodList.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (14), T (19), active));
        periods = periodList;
      }

      IEnumerable<IAutoSequencePeriod> currentPeriods;
      IEnumerable<IAutoSequence> matchingAutoSequences;
      bool result;

      var autoSequenceAnalysis = new AutoSequenceAnalysis (new MonitoredMachineActivityAnalysis (machine));

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.IsTrue (result);
      Assert.AreEqual (1, currentPeriods.Count ());
      Assert.AreEqual (T (1.1), currentPeriods.First ().Begin);
      Assert.AreEqual (T (1.2), currentPeriods.Last ().End);
      Assert.IsFalse (currentPeriods.First ().AutoSequence);
      Assert.AreEqual (0, matchingAutoSequences.Count ());

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.IsTrue (result);
      Assert.AreEqual (1, currentPeriods.Count ());
      Assert.AreEqual (T (2), currentPeriods.First ().Begin);
      Assert.AreEqual (T (4), currentPeriods.Last ().End);
      Assert.IsTrue (currentPeriods.First ().AutoSequence);
      Assert.AreEqual (1, matchingAutoSequences.Count ());
      Assert.AreEqual (T (1), matchingAutoSequences.First ().Begin);
      Assert.AreEqual (T (3), matchingAutoSequences.Last ().End.Value);

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.IsTrue (result);
      Assert.AreEqual (1, currentPeriods.Count ());
      Assert.AreEqual (T (4), currentPeriods.First ().Begin);
      Assert.AreEqual (T (5), currentPeriods.Last ().End);
      Assert.IsFalse (currentPeriods.First ().AutoSequence);
      Assert.AreEqual (0, matchingAutoSequences.Count ());

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.IsTrue (result);
      Assert.AreEqual (2, currentPeriods.Count ());
      Assert.AreEqual (T (5), currentPeriods.First ().Begin);
      Assert.AreEqual (T (8), currentPeriods.Last ().End);
      Assert.IsTrue (currentPeriods.First ().AutoSequence);
      Assert.AreEqual (0, matchingAutoSequences.Count ());

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.IsTrue (result);
      Assert.AreEqual (2, currentPeriods.Count ());
      Assert.AreEqual (T (8), currentPeriods.First ().Begin);
      Assert.AreEqual (T (9), currentPeriods.Last ().End);
      Assert.IsFalse (currentPeriods.First ().AutoSequence);
      Assert.AreEqual (0, matchingAutoSequences.Count ());

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.IsTrue (result);
      Assert.AreEqual (3, currentPeriods.Count ());
      Assert.AreEqual (T (9), currentPeriods.First ().Begin);
      Assert.AreEqual (T (19), currentPeriods.Last ().End);
      Assert.IsTrue (currentPeriods.First ().AutoSequence);
      Assert.AreEqual (3, matchingAutoSequences.Count ());
      Assert.AreEqual (T (10), matchingAutoSequences.First ().Begin);
      Assert.AreEqual (T (20), matchingAutoSequences.Last ().End.Value);
    }

    /// <summary>
    /// Run the MakeAnalysis method for all the modifications
    /// </summary>
    static void RunMakeAnalysis ()
    {
      while (true) {
        IGlobalModification modification = ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .GetFirstPendingModification ();
        if (null == modification) {
          break;
        }
        else {
          ProcessModification (modification);
        }
      }
      IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll ();
      foreach (IMachine machine in machines) {
        while (true) {
          IMachineModification modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetFirstPendingModification (machine);
          if (null == modification) {
            break;
          }
          else {
            ProcessModification (modification);
          }
        }
      }
    }

    static void ProcessModification (IModification modification)
    {
      ((Modification)modification).MakeAnalysis ();
      if (modification.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)) {
        foreach (IModification subModification in modification.SubModifications) {
          ProcessModification (subModification);
        }
        modification.MarkAllSubModificationsCompleted ();
        ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (modification);
      }
    }

    DateTime T (double i)
    {
      return new DateTime (2015, 11, 13, 0, 00, 00, DateTimeKind.Utc)
        .AddHours (i);
    }

    UtcDateTimeRange R (double x, double y)
    {
      return new UtcDateTimeRange (T (x), T (y));
    }
  }
}
