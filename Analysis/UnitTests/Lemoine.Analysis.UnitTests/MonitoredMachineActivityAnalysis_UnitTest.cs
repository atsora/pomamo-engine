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
            Assert.That (machine, Is.Not.Null);
            MachineMode autoMode = session.Get<MachineMode> (3);
            Assert.That (autoMode, Is.Not.Null);
            MachineMode inactiveMode = session.Get<MachineMode> (1);
            Assert.That (inactiveMode, Is.Not.Null);
            MachineMode noDataMode = session.Get<MachineMode> (8);
            Assert.That (noDataMode, Is.Not.Null);
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
              Assert.That (operationSlots, Has.Count.EqualTo (2));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationSlots[i].Machine.Id, Is.EqualTo (machine.Id));
                Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromMinutes (12)));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (operationSlots[i].Machine.Id, Is.EqualTo (machine.Id));
                Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromMinutes (7)));
              });
            }
            {
              // ActivitySummaries
              IList<MachineActivitySummary> activitySummaries =
                session.CreateCriteria<MachineActivitySummary> ()
                .Add (Restrictions.Eq ("Machine", machine))
                .AddOrder (Order.Asc ("Day"))
                .AddOrder (Order.Desc ("MachineMode"))
                .List<MachineActivitySummary> ();
              Assert.That (activitySummaries, Has.Count.EqualTo (1101));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (activitySummaries[i].Machine.Id, Is.EqualTo (machine.Id));
                Assert.That (activitySummaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 16)));
                Assert.That (activitySummaries[i].MachineObservationState.Id, Is.EqualTo (2));
                Assert.That (activitySummaries[i].MachineMode, Is.EqualTo (noDataMode));
                Assert.That (activitySummaries[i].Time.TotalSeconds, Is.EqualTo (33840));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (activitySummaries[i].Machine.Id, Is.EqualTo (machine.Id));
                Assert.That (activitySummaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 16)));
                Assert.That (activitySummaries[i].MachineObservationState.Id, Is.EqualTo (2));
                Assert.That (activitySummaries[i].MachineMode, Is.EqualTo (autoMode));
                Assert.That (activitySummaries[i].Time.TotalSeconds, Is.EqualTo (3000));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (activitySummaries[i].Machine.Id, Is.EqualTo (machine.Id));
                Assert.That (activitySummaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 16)));
                Assert.That (activitySummaries[i].MachineObservationState.Id, Is.EqualTo (2));
                Assert.That (activitySummaries[i].MachineMode, Is.EqualTo (inactiveMode));
                Assert.That (activitySummaries[i].Time.TotalSeconds, Is.EqualTo (2760));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (activitySummaries[i].Machine.Id, Is.EqualTo (machine.Id));
                Assert.That (activitySummaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 17)));
                Assert.That (activitySummaries[i].MachineObservationState.Id, Is.EqualTo (2));
                Assert.That (activitySummaries[i].MachineMode, Is.EqualTo (noDataMode));
                Assert.That (activitySummaries[i].Time.TotalSeconds, Is.EqualTo (86400));
              });
              i = 1101 - 2;
              Assert.Multiple (() => {
                Assert.That (activitySummaries[i].Machine.Id, Is.EqualTo (machine.Id));
                Assert.That (activitySummaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 01, 16)));
                Assert.That (activitySummaries[i].MachineObservationState.Id, Is.EqualTo (2));
                Assert.That (activitySummaries[i].MachineMode, Is.EqualTo (autoMode));
                Assert.That (activitySummaries[i].Time.TotalSeconds, Is.EqualTo (3000));
              });
              i = 1101 - 1;
              Assert.Multiple (() => {
                Assert.That (activitySummaries[i].Machine.Id, Is.EqualTo (machine.Id));
                Assert.That (activitySummaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 01, 16)));
                Assert.That (activitySummaries[i].MachineObservationState.Id, Is.EqualTo (2));
                Assert.That (activitySummaries[i].MachineMode, Is.EqualTo (inactiveMode));
                Assert.That (activitySummaries[i].Time.TotalSeconds, Is.EqualTo (2760));
              });
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
          Assert.That (machine, Is.Not.Null);
          MachineModule machineModule = session.Get<MachineModule> (3);
          Assert.That (machineModule, Is.Not.Null);
          MachineMode autoMode = session.Get<MachineMode> (3);
          Assert.That (autoMode, Is.Not.Null);
          MachineMode idleMode = session.Get<MachineMode> (1);
          Assert.That (idleMode, Is.Not.Null);
          MachineMode noDataMode = session.Get<MachineMode> (8);
          Assert.That (noDataMode, Is.Not.Null);
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
            Assert.That (reasonSlots, Has.Count.GreaterThanOrEqualTo (6), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 26, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 31, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (autoMode));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 31, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 36, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (idleMode));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 36, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 16, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 16, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 17, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 17, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (idleMode));
            });
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
            Assert.That (machine, Is.Not.Null);
            MachineModule machineModule = session.Get<MachineModule> (3);
            Assert.That (machineModule, Is.Not.Null);
            MachineMode autoMode = session.Get<MachineMode> (3);
            Assert.That (autoMode, Is.Not.Null);
            MachineMode idleMode = session.Get<MachineMode> (1);
            Assert.That (idleMode, Is.Not.Null);
            MachineMode noDataMode = session.Get<MachineMode> (8);
            Assert.That (noDataMode, Is.Not.Null);
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
              Assert.That (existingReasonSlot.Reason, Is.EqualTo (reasonUnattended));
              var productionStates1 = ModelDAOHelper.DAOFactory.ProductionStateDAO.FindAll ();
              var productionStateNoProduction1 = productionStates1.Single (p => p.TranslationKey.Equals ("ProductionStateNoProduction"));

              Assert.That (existingReasonSlot.ProductionState, Is.EqualTo (productionStateNoProduction1));
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
                .InsertManualReason (machine, range, reasonTV, 100.0, "", null);
              AnalysisUnitTests.RunMakeAnalysis ();
            }

            // Check the MachineStatus
            {
              MachineStatus machineStatus =
                session.Get<MachineStatus> (machine.Id);
              Assert.Multiple (() => {
                Assert.That (machineStatus.Reason, Is.EqualTo (reasonTV));
                Assert.That (machineStatus.ReasonSlotEnd, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 32, 00)));
                Assert.That (machineStatus.ReasonSource, Is.EqualTo (ReasonSource.Manual));
                Assert.That (machineStatus.ConsolidationLimit.HasValue, Is.EqualTo (true));
                Assert.That (machineStatus.ConsolidationLimit.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 36, 00)));
              });
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
              Assert.That (reasonSlots, Has.Count.GreaterThanOrEqualTo (5), "Number of reason slots");
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 31, 00)));
                Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 36, 00)));
                Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
                Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (idleMode));
                Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonTV));
                Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (false));
                Assert.That (reasonSlots[i].ProductionState, Is.EqualTo (productionStateNoProduction));
              });

              ++i;
              Assert.Multiple (() => {
                Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 36, 00)));
                Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 16, 00, 00)));
                Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
                Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
                Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
                Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
                Assert.That (reasonSlots[i].ProductionState, Is.EqualTo (null));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 16, 00, 00)));
                Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 17, 00, 00)));
                Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (attended));
                Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
                Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnanswered));
                Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
                Assert.That (reasonSlots[i].ProductionState, Is.EqualTo (null));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 17, 00, 00)));
                Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 00, 00)));
                Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
                Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
                Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
                Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
                Assert.That (reasonSlots[i].ProductionState, Is.EqualTo (null));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 00, 00)));
                Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
                Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
                Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (idleMode));
                Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
                Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
                Assert.That (reasonSlots[i].ProductionState, Is.EqualTo (productionStateNoProduction));
              });
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
          Assert.That (machine, Is.Not.Null);
          MachineModule machineModule = session.Get<MachineModule> (3);
          Assert.That (machineModule, Is.Not.Null);
          MachineMode autoMode = session.Get<MachineMode> (3);
          Assert.That (autoMode, Is.Not.Null);
          MachineMode idleMode = session.Get<MachineMode> (1);
          Assert.That (idleMode, Is.Not.Null);
          IMachineStateTemplate mstUnattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (2);
          Assert.That (mstUnattended, Is.Not.Null);
          IMachineStateTemplate mstAttended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (1);
          Assert.That (mstAttended, Is.Not.Null);
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
            Assert.That (existingReasonSlot.Reason, Is.EqualTo (reasonUnattended));
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
            Assert.Multiple (() => {
              Assert.That (machineStatus.Reason, Is.EqualTo (reasonUnattended));
              Assert.That (machineStatus.ReasonSlotEnd, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
              Assert.That (machineStatus.ReasonSource, Is.EqualTo (ReasonSource.DefaultAuto));
              Assert.That (machineStatus.ConsolidationLimit, Is.EqualTo (new UpperBound<DateTime> ()));
              Assert.That (machineStatus.MachineStateTemplate, Is.EqualTo (mstUnattended));
              Assert.That (machineStatus.MachineObservationState, Is.EqualTo (unattended));
            });
          }

          // But the observation state slots were updated
          {
            IList<IObservationStateSlot> stateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (machine, new UtcDateTimeRange (UtcDateTime.From (0)));
            Assert.That (stateSlots, Has.Count.EqualTo (2));
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
            Assert.Multiple (() => {
              Assert.That (machineStatus.Reason, Is.EqualTo (reasonMotion));
              Assert.That (machineStatus.ReasonSlotEnd, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 17, 00, 00)));
              Assert.That (machineStatus.ReasonSource, Is.EqualTo (ReasonSource.DefaultAuto | ReasonSource.DefaultIsAuto));
              Assert.That (machineStatus.ConsolidationLimit, Is.EqualTo (new UpperBound<DateTime> ()));
              Assert.That (machineStatus.MachineStateTemplate, Is.EqualTo (mstAttended));
              Assert.That (machineStatus.MachineObservationState, Is.EqualTo (attended));
            });
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
            Assert.That (reasonSlots, Has.Count.EqualTo (2));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 12, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (idleMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 17, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (autoMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonMotion));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
            });
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
          Assert.That (machine, Is.Not.Null);
          IMachineModule machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindById (3);
          Assert.That (machineModule, Is.Not.Null);
          IMachineMode idleMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (1);
          Assert.That (idleMode, Is.Not.Null);
          IMachineStateTemplate mstAttended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (1);
          Assert.That (mstAttended, Is.Not.Null);
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
            Assert.That (summaries, Is.Empty);
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
            Assert.That (existingReasonSlot.Reason, Is.EqualTo (reasonShort));
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
            Assert.Multiple (() => {
              Assert.That (machineStatus.Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (machineStatus.ReasonSlotEnd, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 17, 00, 00)));
              Assert.That (machineStatus.ReasonSource, Is.EqualTo (ReasonSource.Default));
            });
            Assert.That (machineStatus.ConsolidationLimit.HasValue, Is.False);
            Assert.Multiple (() => {
              Assert.That (machineStatus.MachineStateTemplate, Is.EqualTo (null));
              Assert.That (machineStatus.MachineObservationState, Is.EqualTo (attended));
            });
          }

          // Note: cut-off time is at 21:00 UTC

          // Check the reason slots
          {
            IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindOverlapsRange (machine, new UtcDateTimeRange (new DateTime (2008, 01, 01, 12, 00, 00),
                                                                 new DateTime (2008, 01, 01, 17, 00, 00)));
            Assert.That (reasonSlots, Has.Count.EqualTo (1));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 12, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 17, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (idleMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
            });
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
            Assert.That (summaries, Has.Count.EqualTo (1));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (5)));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
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
          Assert.That (machine, Is.Not.Null);
          MachineModule machineModule = session.Get<MachineModule> (3);
          Assert.That (machineModule, Is.Not.Null);
          MachineMode autoMode = session.Get<MachineMode> (3);
          Assert.That (autoMode, Is.Not.Null);
          MachineMode idleMode = session.Get<MachineMode> (1);
          Assert.That (idleMode, Is.Not.Null);
          MachineMode noDataMode = session.Get<MachineMode> (8);
          Assert.That (noDataMode, Is.Not.Null);
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
            Assert.That (existingReasonSlot.Reason, Is.EqualTo (reasonUnattended));
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
            Assert.Multiple (() => {
              Assert.That (machineStatus.Reason, Is.EqualTo (reasonUnattended));
              Assert.That (machineStatus.ReasonSlotEnd, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
              Assert.That (machineStatus.ReasonSource, Is.EqualTo (ReasonSource.DefaultAuto | ReasonSource.DefaultIsAuto));
              Assert.That (machineStatus.ConsolidationLimit, Is.EqualTo (new UpperBound<DateTime> ()));
              Assert.That (machineStatus.MachineObservationState, Is.EqualTo (unattended));
            });
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
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 12, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 21, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 21, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 23, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 23, 00, 00)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 00, 00)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
            });
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
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (9)));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (2)));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (22)));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
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
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (9)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (2)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (22)));
            });
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
          Assert.That (machine, Is.Not.Null);
          MachineModule machineModule = session.Get<MachineModule> (3);
          Assert.That (machineModule, Is.Not.Null);
          MachineMode autoMode = session.Get<MachineMode> (3);
          Assert.That (autoMode, Is.Not.Null);
          MachineMode idleMode = session.Get<MachineMode> (1);
          Assert.That (idleMode, Is.Not.Null);
          MachineMode noDataMode = session.Get<MachineMode> (8);
          Assert.That (noDataMode, Is.Not.Null);
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
            Assert.That (existingReasonSlot.Reason, Is.EqualTo (reasonUnattended));
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
            Assert.Multiple (() => {
              Assert.That (machineStatus.Reason, Is.EqualTo (reasonUnattended));
              Assert.That (machineStatus.ReasonSlotEnd, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
              Assert.That (machineStatus.ReasonSource, Is.EqualTo (ReasonSource.DefaultAuto));
              Assert.That (machineStatus.ConsolidationLimit, Is.EqualTo (new UpperBound<DateTime> ()));
              Assert.That (machineStatus.MachineObservationState, Is.EqualTo (unattended));
            });
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
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 12, 00, 00)));
              Assert.That (reasonSlots[i].BeginDay.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
              Assert.That (reasonSlots[i].EndDay.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (reasonSlots[i].Shift, Is.EqualTo (shift1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 16, 00, 00)));
              Assert.That (reasonSlots[i].BeginDay.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 21, 00, 00)));
              Assert.That (reasonSlots[i].EndDay.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (reasonSlots[i].Shift, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 21, 00, 00)));
              Assert.That (reasonSlots[i].BeginDay.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 23, 00, 00)));
              Assert.That (reasonSlots[i].EndDay.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (reasonSlots[i].Shift, Is.EqualTo (shift2));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (reasonSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 01, 23, 00, 00)));
              Assert.That (reasonSlots[i].BeginDay.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (reasonSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 00, 00)));
              Assert.That (reasonSlots[i].EndDay.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16)));
              Assert.That (reasonSlots[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (reasonSlots[i].MachineMode, Is.EqualTo (noDataMode));
              Assert.That (reasonSlots[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (reasonSlots[i].DefaultReason, Is.EqualTo (true));
              Assert.That (reasonSlots[i].Shift, Is.EqualTo (null));
            });
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
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (summaries[i].Shift, Is.EqualTo (shift1));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (4)));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (summaries[i].Shift, Is.EqualTo (null));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (5)));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (2)));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Reason, Is.EqualTo (reasonUnattended));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (22)));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
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
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Shift, Is.EqualTo (shift1));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (4)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 01)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Shift, Is.EqualTo (null));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (5)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (summaries[i].Shift, Is.EqualTo (shift2));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (2)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Machine, Is.EqualTo (machine));
              Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2008, 01, 02)));
              Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattended));
              Assert.That (summaries[i].Shift, Is.EqualTo (null));
              Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (22)));
            });
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
          Assert.That (machine, Is.Not.Null);

          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          Assert.That (attended, Is.Not.Null);

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

          Assert.That (((Lemoine.Collections.IDataWithId<long>)currentModification).Id, Is.EqualTo (((Lemoine.Collections.IDataWithId<long>)machineObsAssoc3).Id),
                          "Modification with highest priority should be treated first");

          lastModificationId = ((Lemoine.Collections.IDataWithId<long>)currentModification).Id;
          lastModificationDateTime = currentModification.DateTime;
          lastPriority = currentModification.StatusPriority;

          currentModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetFirstPendingModification (machine,
                                         lastModificationId,
                                         lastPriority, 0);

          Assert.That (((Lemoine.Collections.IDataWithId<long>)currentModification).Id, Is.EqualTo (((Lemoine.Collections.IDataWithId<long>)machineObsAssoc2).Id),
                          "Modification with second highest priority should be treated second");

          lastModificationId = ((Lemoine.Collections.IDataWithId<long>)currentModification).Id;
          lastModificationDateTime = currentModification.DateTime;
          lastPriority = currentModification.StatusPriority;

          currentModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetFirstPendingModification (machine,
                                         lastModificationId,
                                         lastPriority, 0);

          Assert.Multiple (() => {
            Assert.That (machineObsAssoc1, Is.Not.Null);
            Assert.That (currentModification, Is.Not.Null);
            Assert.That (((Lemoine.Collections.IDataWithId<long>)currentModification).Id, Is.EqualTo (((Lemoine.Collections.IDataWithId<long>)machineObsAssoc1).Id),
                            "Modification with lowest priority should be treated last");
          });
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
          Assert.That (machine, Is.Not.Null);

          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          Assert.That (attended, Is.Not.Null);

          IMachineObservationStateAssociation[] machineObsAssocArray
            = new IMachineObservationStateAssociation[10];

          for (int i = 0; i < 10; i++) {
            machineObsAssocArray[i] =
              ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended,
                                                        currentDate);
            Assert.That (machineObsAssocArray[i], Is.Not.EqualTo (null));
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
          int[] oracleId = [5, 8, 0, 1, 4, 6, 9, 3, 7, 2];

          long lastModificationId = 0;
          int lastPriority = Int32.MaxValue;

          PendingMachineModificationAnalysis pendingModificationAnalysis =
            new PendingMachineModificationAnalysis (machine, false);
          for (int i = 0; i < 10; i++) {
            IModification currentModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
              .GetFirstPendingModification (machine,
                                           lastModificationId,
                                           lastPriority, 0);

            Assert.Multiple (() => {
              Assert.That (currentModification, Is.Not.EqualTo (null));
              Assert.That (((Lemoine.Collections.IDataWithId<long>)currentModification).Id, Is.EqualTo (((Lemoine.Collections.IDataWithId<long>)machineObsAssocArray[oracleId[i]]).Id),
                              String.Format ("Error at index {0}", i));
            });

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
        Assert.That (trimmed.Any (), Is.False);
      }
      {
        IList<IAutoSequencePeriod> periods = new List<IAutoSequencePeriod> ();
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (2), inactive));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (3), T (4), inactive));
        IEnumerable<IAutoSequencePeriod> trimmed =
          AutoSequenceAnalysis.Trim (periods);
        Assert.That (trimmed.Any (), Is.False);
      }
      {
        IList<IAutoSequencePeriod> periods = new List<IAutoSequencePeriod> ();
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (2), active));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (3), T (4), active));
        IEnumerable<IAutoSequencePeriod> trimmed =
          AutoSequenceAnalysis.Trim (periods);
        Assert.That (trimmed.Count (), Is.EqualTo (2));
      }
      {
        IList<IAutoSequencePeriod> periods = new List<IAutoSequencePeriod> ();
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (1), T (2), inactive));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (2), T (4), active));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (4), T (5), active));
        periods.Add (ModelDAOHelper.ModelFactory.CreateFact (machine, T (7), T (8), inactive));
        IEnumerable<IAutoSequencePeriod> trimmed =
          AutoSequenceAnalysis.Trim (periods);
        Assert.That (trimmed.Count (), Is.EqualTo (2));
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
      Assert.That (result, Is.True);
      Assert.Multiple (() => {
        Assert.That (currentPeriods.Count (), Is.EqualTo (1));
        Assert.That (currentPeriods.First ().Begin, Is.EqualTo (T (1.1)));
        Assert.That (currentPeriods.Last ().End, Is.EqualTo (T (1.2)));
      });
      Assert.Multiple (() => {
        Assert.That (currentPeriods.First ().AutoSequence, Is.False);
        Assert.That (matchingAutoSequences.Count (), Is.EqualTo (0));
      });

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.That (result, Is.True);
      Assert.Multiple (() => {
        Assert.That (currentPeriods.Count (), Is.EqualTo (1));
        Assert.That (currentPeriods.First ().Begin, Is.EqualTo (T (2)));
        Assert.That (currentPeriods.Last ().End, Is.EqualTo (T (4)));
      });
      Assert.That (currentPeriods.First ().AutoSequence, Is.True);
      Assert.Multiple (() => {
        Assert.That (matchingAutoSequences.Count (), Is.EqualTo (1));
        Assert.That (matchingAutoSequences.First ().Begin, Is.EqualTo (T (1)));
        Assert.That (matchingAutoSequences.Last ().End.Value, Is.EqualTo (T (3)));
      });

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.That (result, Is.True);
      Assert.Multiple (() => {
        Assert.That (currentPeriods.Count (), Is.EqualTo (1));
        Assert.That (currentPeriods.First ().Begin, Is.EqualTo (T (4)));
        Assert.That (currentPeriods.Last ().End, Is.EqualTo (T (5)));
      });
      Assert.Multiple (() => {
        Assert.That (currentPeriods.First ().AutoSequence, Is.False);
        Assert.That (matchingAutoSequences.Count (), Is.EqualTo (0));
      });

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.Multiple (() => {
        Assert.That (result, Is.True);
        Assert.That (currentPeriods.Count (), Is.EqualTo (2));
        Assert.That (currentPeriods.First ().Begin, Is.EqualTo (T (5)));
        Assert.That (currentPeriods.Last ().End, Is.EqualTo (T (8)));
        Assert.That (currentPeriods.First ().AutoSequence, Is.True);
        Assert.That (matchingAutoSequences.Count (), Is.EqualTo (0));
      });

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.That (result, Is.True);
      Assert.Multiple (() => {
        Assert.That (currentPeriods.Count (), Is.EqualTo (2));
        Assert.That (currentPeriods.First ().Begin, Is.EqualTo (T (8)));
        Assert.That (currentPeriods.Last ().End, Is.EqualTo (T (9)));
      });
      Assert.Multiple (() => {
        Assert.That (currentPeriods.First ().AutoSequence, Is.False);
        Assert.That (matchingAutoSequences.Count (), Is.EqualTo (0));
      });

      result = autoSequenceAnalysis.GetNextAutoSequencePeriods (machineModule, R (2, 19), ref periods,
                                                                ref autoSequences,
                                                                out currentPeriods,
                                                                out matchingAutoSequences);
      Assert.That (result, Is.True);
      Assert.Multiple (() => {
        Assert.That (currentPeriods.Count (), Is.EqualTo (3));
        Assert.That (currentPeriods.First ().Begin, Is.EqualTo (T (9)));
        Assert.That (currentPeriods.Last ().End, Is.EqualTo (T (19)));
      });
      Assert.That (currentPeriods.First ().AutoSequence, Is.True);
      Assert.Multiple (() => {
        Assert.That (matchingAutoSequences.Count (), Is.EqualTo (3));
        Assert.That (matchingAutoSequences.First ().Begin, Is.EqualTo (T (10)));
        Assert.That (matchingAutoSequences.Last ().End.Value, Is.EqualTo (T (20)));
      });
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
