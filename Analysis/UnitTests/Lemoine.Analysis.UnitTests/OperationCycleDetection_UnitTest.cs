// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions.Analysis;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Plugin.CycleCountSummary;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Analysis.Detection;
using Lemoine.Business.Config;
using System.Linq;
using Lemoine.Plugin.CycleDurationSummary;
using Lemoine.Plugin.IntermediateWorkPieceSummary;
using Lemoine.Database.Persistent;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// Unit tests for the class OperationCycleDetection.
  /// </summary>
  [TestFixture]
  public class OperationCycleDetection_UnitTest : WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationCycleDetection_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationCycleDetection_UnitTest ()
      : base (UtcDateTime.From (2012, 06, 01, 12, 00, 0))
    {
    }

    [SetUp]
    public void Setup ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.OperationSlotCycles.OperationSlotCyclesAccumulatorExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CyclesWithRealEndFull.OperationCycleFullExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CycleCountSummary.AccumulatorExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.IntermediateWorkPieceSummary.IntermediateWorkPieceByMachineSummaryAccumulatorExtension));
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();

      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
    }

    /// <summary>
    /// Test the StartCycle method
    /// in case there is no operation slot to associate
    /// </summary>
    [Test]
    public void TestStartCycle1NoOperationSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 19));

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (1, operationCycles.Count);
        int i = 0;
        IOperationCycle operationCycle = operationCycles[i];
        Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.Begin);
        Assert.IsNull (operationCycle.End);
        Assert.AreEqual (machine, operationCycle.Machine);
        Assert.IsNull (operationCycle.OperationSlot);

        IList<IBetweenCycles> betweenCycless =
          ModelDAOHelper.DAOFactory.BetweenCyclesDAO
          .FindAll ();
        Assert.AreEqual (0, betweenCycless.Count);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StartCycle method
    /// in case there is one operation slot to associate to it
    /// </summary>
    [Test]
    public void TestStartCycle2WithOperationSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        {
          IList<ICycleCountSummary> summaries =
            new CycleCountSummaryDAO ()
            .FindAll ();
          Assert.AreEqual (0, summaries.Count);
        }

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 19));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.Begin);
          Assert.IsNull (operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot, operationCycle.OperationSlot);

          Assert.AreEqual (1, operationSlot.PartialCycles);
          Assert.AreEqual (0, operationSlot.TotalCycles);
          Assert.IsNull (operationSlot.AverageCycleTime);
          CheckSummaries (operationSlot);
        }

        IList<IBetweenCycles> betweenCycless =
          ModelDAOHelper.DAOFactory.BetweenCyclesDAO
          .FindAll ();
        Assert.AreEqual (0, betweenCycless.Count);

        {
          IList<ICycleCountSummary> summaries =
            new CycleCountSummaryDAO ()
            .FindAll ();
          Assert.AreEqual (1, summaries.Count);
          int i = 0;
          Assert.AreEqual (operationSlot.Operation, summaries[i].Operation);
          Assert.AreEqual (operationSlot.Component, summaries[i].Component);
          Assert.AreEqual (operationSlot.WorkOrder, summaries[i].WorkOrder);
          Assert.AreEqual (0, summaries[i].Full);
          Assert.AreEqual (1, summaries[i].Partial);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StartCycle method
    /// in case the given date/time is invalid
    /// </summary>
    [Test]
    public void TestStartCycle3InvalidDateTime ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 19));
        StartCycle (machine,
                    UtcDateTime.From (2011, 06, 19));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.Begin);
          Assert.IsNull (operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.IsNull (operationCycle.OperationSlot);
        }

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (0, betweenCycless.Count);
        }

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ();
          Assert.AreEqual (1, detectionAnalysisLogs.Count);
          int i = 0;
          IDetectionAnalysisLog detectionAnalysisLog = detectionAnalysisLogs[i];
          Assert.AreEqual (machine, detectionAnalysisLog.Machine);
          Assert.AreEqual (LogLevel.ERROR, detectionAnalysisLog.Level);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is no Begin Cycle before.
    /// There is no operation slot to associate with.
    /// </summary>
    [Test]
    public void TestStopCycle1NoBeginNoSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 19));

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (1, operationCycles.Count);
        int i = 0;
        IOperationCycle operationCycle = operationCycles[i];
        Assert.IsNull (operationCycle.Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.End);
        Assert.AreEqual (machine, operationCycle.Machine);
        Assert.IsNull (operationCycle.OperationSlot);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (0, betweenCycless.Count);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is only one full cycle before.
    /// There is one operation slot to associate with.
    /// </summary>
    [Test]
    public void TestStopCycle2NoBeginWithSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IWorkOrder workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO
          .FindById (2);
        Assert.NotNull (workOrder2);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00), UtcDateTime.From (2012, 06, 20)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);
        {
          IOperationSlot operationSlot2 =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  null, null, workOrder2, null, null, null, null,
                                  new UtcDateTimeRange (UtcDateTime.From (2012, 06, 20)));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot2);
          operationSlot2.ConsolidateRunTime ();
        }

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 19));
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 20));

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (operationSlot.BeginDateTime, operationCycle.Begin);
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle.Status);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot, operationCycle.OperationSlot);
          ++i;
          operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.Begin);
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle.Status);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 20), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot, operationCycle.OperationSlot);

          Assert.AreEqual (0, operationSlot.PartialCycles);
          Assert.AreEqual (2, operationSlot.TotalCycles);
          Assert.AreEqual (TimeSpan.FromDays (1), operationSlot.AverageCycleTime);
          CheckSummaries (operationSlot);
        }

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (0, betweenCycless.Count);
          // No gap between the two operation cycles
        }

        // Extend the operation slot
        AddOperation (machine, operation1, UtcDateTime.From (2012, 06, 20), UtcDateTime.From (2012, 06, 22));
        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (operationSlot.BeginDateTime, operationCycle.Begin);
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle.Status);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot, operationCycle.OperationSlot);
          ++i;
          operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.Begin);
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle.Status);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 20), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot, operationCycle.OperationSlot);

          Assert.AreEqual (0, operationSlot.PartialCycles);
          Assert.AreEqual (2, operationSlot.TotalCycles);
          Assert.AreEqual (TimeSpan.FromDays (1), operationSlot.AverageCycleTime);
          CheckSummaries (operationSlot);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is only one full cycle before.
    /// There is one operation slot to associate with.
    /// </summary>
    [Test]
    public void TestStopCycle2NoBeginWithSlot2 ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IWorkOrder workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO
          .FindById (2);
        Assert.NotNull (workOrder2);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                R (0, 1));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);
        {
          IOperationSlot operationSlot2 =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  null, null, workOrder2, null, null, null, null,
                                  R (1, null));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot2);
          operationSlot2.ConsolidateRunTime ();
        }

        for (int i = 2; i < 32; i++) {
          AddOperation (machine, operation1, T (i - 1), T (i));
          StopCycle (machine, T (i));
        }

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (30, operationCycles.Count);
          {
            IOperationCycle operationCycle = operationCycles[0];
            Assert.AreEqual (T (0), operationCycle.Begin);
            Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle.Status);
            Assert.AreEqual (T (2), operationCycle.End);
            Assert.AreEqual (machine, operationCycle.Machine);
            Assert.AreEqual (operationSlot, operationCycle.OperationSlot);
          }
          for (int i = 1; i < 30; i++) {
            IOperationCycle operationCycle = operationCycles[i];
            Assert.AreEqual (T (i + 1), operationCycle.Begin);
            Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle.Status);
            Assert.AreEqual (T (i + 2), operationCycle.End);
            Assert.AreEqual (machine, operationCycle.Machine);
            Assert.AreEqual (operationSlot, operationCycle.OperationSlot);
          }

          {
            IList<IBetweenCycles> betweenCycless =
              ModelDAOHelper.DAOFactory.BetweenCyclesDAO
              .FindAll ();
            Assert.AreEqual (0, betweenCycless.Count);
            // No gap between the operation cycles
          }

          Assert.AreEqual (0, operationSlot.PartialCycles);
          Assert.AreEqual (30, operationSlot.TotalCycles);
          Assert.AreEqual (TimeSpan.FromSeconds (1), operationSlot.AverageCycleTime);
          CheckSummaries (operationSlot);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is one previous begin cycle
    /// that is on the same operation slot.
    /// </summary>
    [Test]
    public void TestStopCycle3AfterBeginSameSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 02));

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 04));
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 05));

        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (3, operationCycles.Count);
        int i = 1;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].End);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 04), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 05), operationCycles[i].End);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (1, betweenCycless.Count);
          int j = 0;
          IBetweenCycles betweenCycles = betweenCycless[j];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), betweenCycles.Begin);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 04), betweenCycles.End);
          Assert.AreEqual (machine, betweenCycles.Machine);
          Assert.AreEqual (operationCycles[1], betweenCycles.PreviousCycle);
          Assert.AreEqual (operationCycles[2], betweenCycles.NextCycle);
        }

        Assert.AreEqual (1, operationSlot.PartialCycles);
        Assert.AreEqual (2, operationSlot.TotalCycles);
        Assert.AreEqual (TimeSpan.FromDays (2), operationSlot.AverageCycleTime);
        CheckSummaries (operationSlot);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test partial cycles estimated times on a sequence of start with no stop
    /// average cycle time remains null (no full cycle)
    /// </summary>
    [Test]
    public void TestPartialCyclesNoAverage ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 02));

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 03));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (3, operationCycles.Count);
        int i = 0;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].Begin);
        Assert.AreEqual (null, operationCycles[i].End);
        Assert.AreEqual (new OperationCycleStatus (), operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (0, betweenCycless.Count);
          // No gap between the cycles
        }

        Assert.AreEqual (3, operationSlot.PartialCycles);
        Assert.AreEqual (0, operationSlot.TotalCycles);
        // only partial cycles => no average cycle time
        Assert.AreEqual (null, operationSlot.AverageCycleTime);
        CheckSummaries (operationSlot);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test whether operationslot splitting also splits operationcycle correctly
    /// </summary>
    [Test]
    public void TestSlotSplit ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        // single cycle in slot
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();


        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (1, operationCycles.Count);
        int i = 0;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].End);
        Assert.AreEqual (new OperationCycleStatus (), operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        Assert.AreEqual (0, operationSlot.PartialCycles);
        Assert.AreEqual (1, operationSlot.TotalCycles);
        CheckSummaries (operationSlot);

        // cut slot in middle
        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation2, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2012, 06, 02, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot2);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        operationSlot.EndDateTime = UtcDateTime.From (2012, 06, 02, 00, 00, 00);
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        (operationSlot as OperationSlot)
          .Consolidate (null);

        (operationSlot2 as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        // slot split => cycle split in two cyles (one partial, one full)
        Assert.AreEqual (1, operationSlot.PartialCycles);
        Assert.AreEqual (0, operationSlot.TotalCycles);
        CheckSummaries (operationSlot);
        Assert.AreEqual (0, operationSlot2.PartialCycles);
        Assert.AreEqual (1, operationSlot2.TotalCycles);
        CheckSummaries (operationSlot2);

        IList<IOperationCycle> operationCycles1 =
          ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllWithOperationSlot (operationSlot);

        IList<IOperationCycle> operationCycles2 =
          ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllWithOperationSlot (operationSlot2);

        Assert.AreEqual (1, operationCycles1.Count);
        Assert.AreEqual (1, operationCycles2.Count);

        IOperationCycle operationCycle1 = operationCycles1[0];
        IOperationCycle operationCycle2 = operationCycles2[0];

        // first cycle in first slot and with an estimated end
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycle1.Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycle1.End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycle1.Status);
        Assert.AreEqual (machine, operationCycle1.Machine);
        Assert.AreEqual (operationSlot, operationCycle1.OperationSlot);

        // second cycle in second slot and with an estimated begin
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycle2.Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle2.End);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle2.Status);
        Assert.AreEqual (machine, operationCycle2.Machine);
        Assert.AreEqual (operationSlot2, operationCycle2.OperationSlot);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test estimated begin/end w.r.t operation slot begin/end
    /// </summary>
    [Test]
    public void TestEstimatedFromSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00),
                                                      UtcDateTime.From (2012, 06, 02, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        // start cycle (a bit later than first slot's end otherwise
        // last partial cycle will have end in second slot and be put into it)
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));

        // consolidate cycles
        (operationSlot as OperationSlot)
          .Consolidate (null);

        // other slot
        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation2,
                                null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2012, 06, 02, 00, 00, 01)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot2);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        // stop cycle
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        // consolidate cycles
        (operationSlot as OperationSlot)
          .Consolidate (null); // Here nothing is done because the slot is already consolidated

        (operationSlot2 as OperationSlot)
          .Consolidate (null);

        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();

        IList<IOperationCycle> operationCycleList1 =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAllWithOperationSlot (operationSlot);

        IList<IOperationCycle> operationCycleList2 =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAllWithOperationSlot (operationSlot2);

        Assert.AreEqual (1, operationCycleList1.Count, "1.not 1");
        Assert.AreEqual (1, operationCycleList2.Count, "2.not 1");

        IOperationCycle operationCycle1 = operationCycleList1[0];
        IOperationCycle operationCycle2 = operationCycleList2[0];

        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycle1.Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycle1.End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycle1.Status);
        Assert.AreEqual (machine, operationCycle1.Machine);
        Assert.AreEqual (operationSlot, operationCycle1.OperationSlot);

        Assert.AreEqual (UtcDateTime.From (2012, 06, 02, 00, 00, 01), operationCycle2.Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle2.End);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle2.Status);
        Assert.AreEqual (machine, operationCycle2.Machine);
        Assert.AreEqual (operationSlot2, operationCycle2.OperationSlot);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test full cycles with estimated times on a sequence of stop with no start
    /// average cycle time computed from estimated begin times
    /// </summary>
    [Test]
    public void TestFullCyclesEstimatedBeginAverage ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1,
                                null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 01));
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 02));

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (3, operationCycles.Count);
        int i = 0;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycles[i].End);
        Assert.AreEqual (operationSlot.BeginDateTime, operationCycles[i].Begin);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        Assert.AreEqual (0, operationSlot.PartialCycles);
        Assert.AreEqual (3, operationSlot.TotalCycles);
        // only full cycles (all with estimated begin) => there is an average cycle time
        Assert.AreEqual (TimeSpan.FromDays (1), operationSlot.AverageCycleTime);
        CheckSummaries (operationSlot);

        transaction.Rollback ();
      }
    }


    /// <summary>
    /// Test average cycle time on a partial / full / full / full / partial scenario
    /// </summary>
    [Test]
    public void TestAverageCycleTimePartialThen3FullThenPartial ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        // one partial
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));

        // followed by 3 full cycles
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 02));

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 03));

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 04));

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 05));

        // followed by one partial
        StartCycle (machine,
                   UtcDateTime.From (2012, 06, 05));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (5, operationCycles.Count);
        Assert.AreEqual (2, operationSlot.PartialCycles);
        Assert.AreEqual (3, operationSlot.TotalCycles);
        CheckSummaries (operationSlot);

        int i = 0;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].End);
        Assert.AreEqual (0, (int)operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 04), operationCycles[i].End);
        Assert.AreEqual (0, (int)operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 04), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 05), operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 05), operationCycles[i].Begin);
        Assert.AreEqual (null, operationCycles[i].End);
        Assert.AreEqual (0, (int)operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (2, betweenCycless.Count);
          int j = 0;
          IBetweenCycles betweenCycles = betweenCycless[j];
          Assert.AreEqual (machine, betweenCycles.Machine);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), betweenCycles.Begin);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), betweenCycles.End);
          Assert.AreEqual (null, betweenCycles.OffsetDuration);
          ++j;
          betweenCycles = betweenCycless[j];
          Assert.AreEqual (machine, betweenCycles.Machine);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 05), betweenCycles.Begin);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 05), betweenCycles.End);
          Assert.AreEqual (null, betweenCycles.OffsetDuration);
        }

        // 3 full cycles on a duration of 3 days (from the 2nd to the 5th)
        Assert.AreEqual (TimeSpan.FromDays (1), operationSlot.AverageCycleTime);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the creation of an operation cycle with both
    /// estimated begin and estimated end
    /// </summary>
    [Test]
    public void TestCycleBothEstimated ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        // one full cycle with estimated begin = start of slot
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 01));

        // one full cycle with estimated begin = end of last cycle
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 02));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();
        NHibernateHelper.GetCurrentSession ().Flush ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (2, operationCycles.Count);
        Assert.AreEqual (0, operationSlot.PartialCycles);
        Assert.AreEqual (2, operationSlot.TotalCycles);
        CheckSummaries (operationSlot);

        int i = 0;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycles[i].End);
        Assert.AreEqual (operationSlot.BeginDateTime, operationCycles[i].Begin);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (0, betweenCycless.Count);
          // No gap between the cycles
        }


        // cut operation slot in middle of second cycle
        DateTime cutTime = UtcDateTime.From (2012, 06, 01, 08, 00, 00);
        operationSlot.EndDateTime = cutTime;

        // and consolidate it
        (operationSlot as OperationSlot)
          .Consolidate (null);

        DAOFactory.EmptyAccumulators ();

        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAllWithOperationSlot (operationSlot);

        Assert.AreEqual (1, operationCycles.Count);
        // first full cycle remains associated to the slot
        // second full cycle not associated to the slot anymore
        Assert.AreEqual (0, operationSlot.PartialCycles);
        Assert.AreEqual (1, operationSlot.TotalCycles);
        CheckSummaries (operationSlot);

        // first cycle is full, with an estimated begin set to start of slot (a while back)
        i = 0;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycles[i].End);
        Assert.AreEqual (true, operationCycles[i].Full);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
        Assert.AreEqual (UtcDateTime.From (2008, 01, 16, 00, 00, 00), operationCycles[i].Begin);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        // second cycle not associated to slot and with estimated begin to end of slot
        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();

        Assert.AreEqual (2, operationCycles.Count);
        Assert.IsTrue (operationCycles[0].End.HasValue && operationCycles[1].End.HasValue);
        IOperationCycle secondCycle = operationCycles[0].End.Value < operationCycles[1].End.Value ?
          operationCycles[1] : operationCycles[0];
        Assert.AreEqual (null, secondCycle.OperationSlot);
        Assert.IsTrue (secondCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated));
        Assert.IsNull (secondCycle.Begin);
        transaction.Rollback ();
      }
    }



    /// <summary>
    /// Test operation cycle end "just after" operation slot end
    /// </summary>
    [Test]
    public void TestCycleJustAfterSlot1 ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        DateTime T0 = UtcDateTime.From (2012, 01, 01, 00, 00, 00);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (T0));


        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        DateTime T1 = T0.AddHours (1);
        StartCycle (machine, T1);
        DateTime T2 = T1.AddHours (1);
        operationSlot.EndDateTime = T2;

        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        DateTime T3 = T2.AddSeconds (10);
        StopCycle (machine, T3);

        (operationSlot as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (1, operationCycles.Count, "slot has not 1 cycle");
        Assert.AreEqual (0, operationSlot.PartialCycles, "slot has not 0 partial cycle");
        Assert.AreEqual (1, operationSlot.TotalCycles, "slot has not 1 total cycle");

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test operation cycle end "just after" operation slot end
    /// (with another trailing operation slot)
    /// </summary>
    [Test]
    public void TestCycleJustAfterSlot2 ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        DateTime T0 = UtcDateTime.From (2012, 01, 01, 00, 00, 00);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (T0));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        DateTime T1 = T0.AddHours (1);
        StartCycle (machine, T1);
        DateTime T2 = T1.AddHours (1);
        operationSlot.EndDateTime = T2;

        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        DateTime T4 = T2.AddSeconds (15);

        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (T4));

        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot2);

        DateTime T3 = T2.AddSeconds (10);
        StopCycle (machine, T3);

        (operationSlot as OperationSlot)
          .Consolidate (null);

        (operationSlot2 as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (1, operationCycles.Count, "slot has not 1 cycle");
        Assert.AreEqual (0, operationSlot.PartialCycles, "slot has not 0 partial cycle");
        Assert.AreEqual (1, operationSlot.TotalCycles, "slot has not 1 total cycle");
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is one previous begin cycle.
    /// There is no operation slot.
    /// </summary>
    [Test]
    public void TestStopCycle4AfterBeginNoSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 02));
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (2, operationCycles.Count);
        int i = 1;
        IOperationCycle operationCycle = operationCycles[i];
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycle.Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle.End);
        Assert.AreEqual (machine, operationCycle.Machine);
        Assert.IsNull (operationCycle.OperationSlot);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is one previous begin cycle
    /// but that is on a different slot
    /// and that is far before the new operation slot
    /// </summary>
    [Test]
    public void TestStopCycle5AfterBeginDifferentSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        IOperationSlot operationSlot1 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00),
                                                      UtcDateTime.From (2012, 06, 01)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot1);
        operationSlot1.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot1);
        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation2, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2012, 06, 01, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot2);
        operationSlot2.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot2);

        StartCycle (machine,
                    UtcDateTime.From (2010, 06, 02));
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2010, 06, 02), operationCycle.Begin);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot1, operationCycle.OperationSlot);
          Assert.AreEqual (operationSlot1.EndDateTime, operationCycle.End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycle.Status);
          ++i;
          operationCycle = operationCycles[i];
          Assert.AreEqual (operationSlot2.BeginDateTime, operationCycle.Begin);
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle.Status);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot2, operationCycle.OperationSlot);
        }

        Assert.AreEqual (1, operationSlot1.PartialCycles);
        Assert.AreEqual (0, operationSlot1.TotalCycles);
        Assert.IsNull (operationSlot1.AverageCycleTime);
        CheckSummaries (operationSlot1);
        Assert.AreEqual (1, operationSlot2.TotalCycles);
        Assert.AreEqual (0, operationSlot2.PartialCycles);
        Assert.IsNull (operationSlot2.AverageCycleTime);
        CheckSummaries (operationSlot2);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is one previous begin cycle
    /// that is on a different slot,
    /// but that is close enough the new operation slot
    /// </summary>
    [Test]
    public void TestStopCycle6AfterBeginDifferentSlotButClose ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        IOperationSlot operationSlot1 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00),
                                                      UtcDateTime.From (2012, 06, 01, 00, 00, 02)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot1);
        operationSlot1.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot1);
        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation2, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2012, 06, 01, 00, 00, 02)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot2);
        operationSlot2.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot2);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycle.Begin);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot2, operationCycle.OperationSlot);
        }

        Assert.AreEqual (0, operationSlot1.PartialCycles);
        Assert.AreEqual (0, operationSlot1.TotalCycles);
        Assert.IsNull (operationSlot1.AverageCycleTime);
        CheckSummaries (operationSlot1);
        Assert.AreEqual (1, operationSlot2.TotalCycles);
        Assert.AreEqual (0, operationSlot2.PartialCycles);
        Assert.IsNull (operationSlot2.AverageCycleTime);
        CheckSummaries (operationSlot2);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is one invalid date/time
    /// </summary>
    [Test]
    public void TestStopCycle7InvalidDateTime1 ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 19));
        StopCycle (machine,
                   UtcDateTime.From (2011, 06, 19));

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.Begin);
          Assert.IsNull (operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.IsNull (operationCycle.OperationSlot);
        }

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ();
          Assert.AreEqual (2, detectionAnalysisLogs.Count);
          int i = 0;
          // 1st log is on ExtendOperation
          Assert.AreEqual (machine, detectionAnalysisLogs[i].Machine);
          Assert.AreEqual (LogLevel.WARN, detectionAnalysisLogs[i].Level);
          ++i;
          // 2nd log is on the date/time problem
          Assert.AreEqual (machine, detectionAnalysisLogs[i].Machine);
          Assert.AreEqual (LogLevel.ERROR, detectionAnalysisLogs[i].Level);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// There is one invalid date/time
    /// </summary>
    [Test]
    public void TestStopCycle8InvalidDateTime2 ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 19));
        StopCycle (machine,
                   UtcDateTime.From (2011, 06, 19));

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.IsNull (operationCycle.Begin);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 19), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.IsNull (operationCycle.OperationSlot);
        }

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ()
            .OrderBy (x => x.Id)
            .ToList ();
          Assert.AreEqual (3, detectionAnalysisLogs.Count);
          int i = 0;
          // 1st 2 logs are on ExtendOperation
          Assert.AreEqual (machine, detectionAnalysisLogs[i].Machine);
          Assert.AreEqual (LogLevel.WARN, detectionAnalysisLogs[i].Level);
          ++i;
          Assert.AreEqual (machine, detectionAnalysisLogs[i].Machine);
          Assert.AreEqual (LogLevel.WARN, detectionAnalysisLogs[i].Level);
          ++i;
          // 3rd log is on the date/time problem
          Assert.AreEqual (machine, detectionAnalysisLogs[i].Machine);
          Assert.AreEqual (LogLevel.ERROR, detectionAnalysisLogs[i].Level);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// Check the error PreviousCycleNotMatchingSlot is correctly raised.
    /// </summary>
    [Test]
    public void TestStopCycleAPreviousCycleNotMatchingSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ((OperationSlot)operationSlot).TotalCycles = 0;
        operationSlot.AverageCycleTime = null;
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        StartStopCycle (machine,
                        UtcDateTime.From (2012, 06, 01),
                        UtcDateTime.From (2012, 06, 02));

        DAOFactory.EmptyAccumulators ();

        Assert.AreEqual (1, operationSlot.TotalCycles);
        CheckSummaries (operationSlot);
        ((OperationSlot)operationSlot).TotalCycles = 0;

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          int i = 1;
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        }

        Assert.AreEqual (0, operationSlot.PartialCycles);
        Assert.AreEqual (1, operationSlot.TotalCycles);
        Assert.IsNull (operationSlot.AverageCycleTime);
        CheckSummaries (operationSlot);

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ();
          Assert.AreEqual (1, detectionAnalysisLogs.Count);
          int i = 0;
          IDetectionAnalysisLog detectionAnalysisLog = detectionAnalysisLogs[i];
          Assert.AreEqual (machine, detectionAnalysisLog.Machine);
          Assert.AreEqual (LogLevel.ERROR, detectionAnalysisLog.Level);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test how offset duration is computed on start/stop
    /// </summary>
    [Test]
    public void TestOffsetDurationStartStop ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.OperationCycleOffsetDurationAccumulatorExtension));
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.CycleDurationSummaryAccumulatorExtension));

          // Reference data
          IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
          Assert.NotNull (machine);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);
          operation1.MachiningDuration = TimeSpan.FromSeconds (30);

          DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
          DateTime T1 = T0.AddMinutes (1);
          DateTime T2 = T1.AddSeconds (45);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  new UtcDateTimeRange (T0));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);

          StartCycle (machine,
                      T1);

          StopCycle (machine,
                     T2);

          DAOFactory.EmptyAccumulators ();

          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.AreEqual (1, operationCycles.Count);
            int i = 0;
            Assert.AreEqual (T1, operationCycles[i].Begin);
            Assert.AreEqual (T2, operationCycles[i].End);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
            Assert.AreEqual ((100 * (45 - 30)) / 30, operationCycles[i].OffsetDuration);
          }

          {
            IList<ICycleDurationSummary> summaries =
              new CycleDurationSummaryDAO ()
              .FindAll ();
            Assert.AreEqual (1, summaries.Count);
            int i = 0;
            Assert.AreEqual (operationSlot.Operation, summaries[i].Operation);
            Assert.AreEqual (operationSlot.Component, summaries[i].Component);
            Assert.AreEqual (operationSlot.WorkOrder, summaries[i].WorkOrder);
            Assert.AreEqual (50, summaries[i].Offset);
            Assert.AreEqual (1, summaries[i].Number);
          }

        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test how offset duration is computed on start/start
    /// </summary>
    [Test]
    public void TestOffsetDurationStartStart ()
    {
      try {
        ModelDAOHelper.ModelFactory =
          new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          try {
            Lemoine.Extensions.ExtensionManager
              .Add (typeof (Lemoine.Plugin.CycleDurationSummary.NHibernateExtension));
            Lemoine.Extensions.ExtensionManager
              .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));
            Lemoine.Extensions.ExtensionManager
              .Add (typeof (Lemoine.Plugin.CycleDurationSummary.OperationCycleOffsetDurationAccumulatorExtension));
            Lemoine.Extensions.ExtensionManager
              .Add (typeof (Lemoine.Plugin.CycleDurationSummary.CycleDurationSummaryAccumulatorExtension));

            // Reference data
            IMonitoredMachine machine =
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
            Assert.NotNull (machine);
            IOperation operation1 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.NotNull (operation1);
            operation1.MachiningDuration = TimeSpan.FromSeconds (30);

            DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
            DateTime T1 = T0.AddMinutes (1);
            DateTime T2 = T1.AddSeconds (45);

            IOperationSlot operationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1, null, null, null, null, null, null,
                                    new UtcDateTimeRange (T0));
            ModelDAOHelper.DAOFactory.OperationSlotDAO
              .MakePersistent (operationSlot);

            StartCycle (machine,
                        T1);

            StartCycle (machine,
                        T2);

            DAOFactory.EmptyAccumulators ();

            {
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (2, operationCycles.Count);
              int i = 0;
              Assert.AreEqual (T1, operationCycles[i].Begin);
              Assert.AreEqual (T2, operationCycles[i].End);
              Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
              Assert.AreEqual ((100 * (45 - 30)) / 30, operationCycles[i].OffsetDuration);
              ++i;
              Assert.AreEqual (T2, operationCycles[i].Begin);
              Assert.AreEqual (null, operationCycles[i].End);
              Assert.AreEqual (new OperationCycleStatus (), operationCycles[i].Status);
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
              Assert.AreEqual (null, operationCycles[i].OffsetDuration);
            }

            {
              IList<ICycleDurationSummary> summaries =
                new CycleDurationSummaryDAO ()
                .FindAll ();
              Assert.AreEqual (1, summaries.Count);
              int i = 0;
              Assert.AreEqual (operationSlot.Operation, summaries[i].Operation);
              Assert.AreEqual (operationSlot.Component, summaries[i].Component);
              Assert.AreEqual (operationSlot.WorkOrder, summaries[i].WorkOrder);
              Assert.AreEqual (50, summaries[i].Offset);
              Assert.AreEqual (0, summaries[i].Number);
              Assert.AreEqual (1, summaries[i].Partial);
            }

            {
              IList<ICycleCountSummary> summaries =
                new CycleCountSummaryDAO ()
                .FindAll ();
              Assert.AreEqual (1, summaries.Count);
              int i = 0;
              Assert.AreEqual (operationSlot.Operation, summaries[i].Operation);
              Assert.AreEqual (operationSlot.Component, summaries[i].Component);
              Assert.AreEqual (operationSlot.WorkOrder, summaries[i].WorkOrder);
              Assert.AreEqual (0, summaries[i].Full);
              Assert.AreEqual (2, summaries[i].Partial);
            }

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
      finally {
      }
    }

    /// <summary>
    /// Test how offset duration is computed on stop/stop
    /// </summary>
    [Test]
    public void TestOffsetDurationStopStop ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.OperationCycleOffsetDurationAccumulatorExtension));
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.CycleDurationSummaryAccumulatorExtension));

          // Reference data
          IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
          Assert.NotNull (machine);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);
          operation1.MachiningDuration = TimeSpan.FromSeconds (30);

          DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
          DateTime T1 = T0.AddMinutes (1);
          DateTime T2 = T1.AddSeconds (45);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  new UtcDateTimeRange (T0));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);

          StopCycle (machine,
                     T1);

          StopCycle (machine,
                     T2);

          DAOFactory.EmptyAccumulators ();

          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.AreEqual (2, operationCycles.Count);
            int i = 0;
            Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
            Assert.AreEqual (operationSlot.BeginDateTime, operationCycles[i].Begin);
            Assert.AreEqual (T1, operationCycles[i].End);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
            Assert.AreEqual (100, operationCycles[i].OffsetDuration); // 1 min instead of 30 s
            i++;
            Assert.AreEqual (T1, operationCycles[i].Begin);
            Assert.AreEqual (T2, operationCycles[i].End);
            Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
            Assert.AreEqual ((100 * (45 - 30)) / 30, operationCycles[i].OffsetDuration);
          }

          {
            IList<ICycleDurationSummary> summaries =
              new CycleDurationSummaryDAO ()
              .FindAll ();
            Assert.AreEqual (2, summaries.Count);
            int i = 0;
            Assert.AreEqual (operationSlot.Operation, summaries[i].Operation);
            Assert.AreEqual (operationSlot.Component, summaries[i].Component);
            Assert.AreEqual (operationSlot.WorkOrder, summaries[i].WorkOrder);
            Assert.AreEqual (100, summaries[i].Offset);
            Assert.AreEqual (1, summaries[i].Number);
            Assert.AreEqual (0, summaries[i].Partial);
            ++i;
            Assert.AreEqual (operationSlot.Operation, summaries[i].Operation);
            Assert.AreEqual (operationSlot.Component, summaries[i].Component);
            Assert.AreEqual (operationSlot.WorkOrder, summaries[i].WorkOrder);
            Assert.AreEqual (50, summaries[i].Offset);
            Assert.AreEqual (1, summaries[i].Number);
            Assert.AreEqual (0, summaries[i].Partial);
          }

          {
            IList<ICycleCountSummary> summaries =
              new CycleCountSummaryDAO ()
              .FindAll ();
            Assert.AreEqual (1, summaries.Count);
            int i = 0;
            Assert.AreEqual (operationSlot.Operation, summaries[i].Operation);
            Assert.AreEqual (operationSlot.Component, summaries[i].Component);
            Assert.AreEqual (operationSlot.WorkOrder, summaries[i].WorkOrder);
            Assert.AreEqual (2, summaries[i].Full);
            Assert.AreEqual (0, summaries[i].Partial);
          }

        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// Check the error IncoherentTotalCycles is correctly raised.
    /// </summary>
    [Test]
    public void TestStopCycleBIncoherentTotalCycles ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ((OperationSlot)operationSlot).TotalCycles = 1;
        operationSlot.AverageCycleTime = null;
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (operationSlot.BeginDateTime, operationCycles[i].Begin);
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[i].Status);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        }

        Assert.AreEqual (0, operationSlot.PartialCycles);
        Assert.AreEqual (1, operationSlot.TotalCycles);
        Assert.IsNull (operationSlot.AverageCycleTime);
        CheckSummaries (operationSlot);

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ();
          Assert.AreEqual (1, detectionAnalysisLogs.Count);
          int i = 0;
          IDetectionAnalysisLog detectionAnalysisLog = detectionAnalysisLogs[i];
          Assert.AreEqual (machine, detectionAnalysisLog.Machine);
          Assert.AreEqual (LogLevel.ERROR, detectionAnalysisLog.Level);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StartStopCycle method
    /// 
    /// Both start and stop are on the same operation slot.
    /// </summary>
    [Test]
    public void TestStartStopCycle1SameSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));
        StartStopCycle (machine,
                        UtcDateTime.From (2012, 06, 02),
                        UtcDateTime.From (2012, 06, 03));
        StartStopCycle (machine,
                        UtcDateTime.From (2012, 06, 04),
                        UtcDateTime.From (2012, 06, 05));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (3, operationCycles.Count);
        int i = 1;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycles[i].End);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2012, 06, 04), operationCycles[i].Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 05), operationCycles[i].End);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (1, betweenCycless.Count);
          int j = 0;
          IBetweenCycles betweenCycles = betweenCycless[j];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), betweenCycles.Begin);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 04), betweenCycles.End);
        }

        Assert.AreEqual (1, operationSlot.PartialCycles);
        Assert.AreEqual (2, operationSlot.TotalCycles);
        Assert.AreEqual (TimeSpan.FromDays (2), operationSlot.AverageCycleTime);
        CheckSummaries (operationSlot);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StartStopCycle method
    /// 
    /// There is no operation slot.
    /// </summary>
    [Test]
    public void TestStartStopCycle2NoSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));
        StartStopCycle (machine,
                        UtcDateTime.From (2012, 06, 02),
                        UtcDateTime.From (2012, 06, 03));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (2, operationCycles.Count);
        int i = 1;
        IOperationCycle operationCycle = operationCycles[i];
        Assert.AreEqual (UtcDateTime.From (2012, 06, 02), operationCycle.Begin);
        Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle.End);
        Assert.AreEqual (machine, operationCycle.Machine);
        Assert.IsNull (operationCycle.OperationSlot);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.AreEqual (0, betweenCycless.Count);
          // No gap between the cycles
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StartStopCycle method
    /// 
    /// The Start and Stop are on different slots
    /// and Start is far before the other operation slot
    /// </summary>
    [Test]
    public void TestStartStopCycle3DifferentSlotsFarBefore ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        IOperationSlot operationSlot1 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00),
                                                      UtcDateTime.From (2012, 06, 01)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot1);
        operationSlot1.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot1);
        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation2, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2012, 06, 01, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot2);
        operationSlot2.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot2);

        StartStopCycle (machine,
                        UtcDateTime.From (2010, 06, 02),
                        UtcDateTime.From (2012, 06, 03));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();

          Assert.AreEqual (2, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2010, 06, 02), operationCycle.Begin);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot1, operationCycle.OperationSlot);
          Assert.AreEqual (operationSlot1.EndDateTime, operationCycle.End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycle.Status);
          ++i;
          operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycle.Begin);
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycle.Status);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot2, operationCycle.OperationSlot);
        }

        Assert.AreEqual (1, operationSlot1.PartialCycles);
        Assert.AreEqual (0, operationSlot1.TotalCycles);
        Assert.IsNull (operationSlot1.AverageCycleTime);
        CheckSummaries (operationSlot1);
        Assert.AreEqual (1, operationSlot2.TotalCycles);
        Assert.AreEqual (0, operationSlot2.PartialCycles);
        Assert.IsNull (operationSlot2.AverageCycleTime);
        CheckSummaries (operationSlot2);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StartStopCycle method
    /// 
    /// The Start and Stop are on different slots but with the same operation
    /// and Start is far before the other operation slot
    /// </summary>
    [Test]
    public void TestStartStopCycleDifferentSlotsFarBeforeSameOperation ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                           (int)(OperationSlotSplitOption.ByDay | OperationSlotSplitOption.ByGlobalShift));

        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation);
        IShift shift1 = ModelDAOHelper.ModelFactory.CreateShiftFromName ("1");
        ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift1);
        IShift shift2 = ModelDAOHelper.ModelFactory.CreateShiftFromName ("2");
        ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift2);

        IOperationSlot operationSlot1 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation, null, null, null, null, UtcDateTime.From (2008, 01, 16), shift1,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00),
                                                      UtcDateTime.From (2012, 06, 01)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot1);
        operationSlot1.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot1);
        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation, null, null, null, null, UtcDateTime.From (2012, 06, 01), shift2,
                                new UtcDateTimeRange (UtcDateTime.From (2012, 06, 01, 00, 00, 00)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot2);
        operationSlot2.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot2);

        StartStopCycle (machine,
                        UtcDateTime.From (2010, 06, 02),
                        UtcDateTime.From (2012, 06, 03));

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2010, 06, 02), operationCycle.Begin);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot2, operationCycle.OperationSlot);
          Assert.AreEqual (new OperationCycleStatus (), operationCycle.Status);
        }

        Assert.AreEqual (0, operationSlot1.PartialCycles);
        Assert.AreEqual (0, operationSlot1.TotalCycles);
        Assert.IsNull (operationSlot1.AverageCycleTime);
        CheckSummaries (operationSlot1);
        Assert.AreEqual (1, operationSlot2.TotalCycles);
        Assert.AreEqual (0, operationSlot2.PartialCycles);
        Assert.IsNull (operationSlot2.AverageCycleTime);
        CheckSummaries (operationSlot2);

        // Reset the config
        Lemoine.Info.ConfigSet.ResetForceValues ();

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method
    /// 
    /// The Start and Stop are on different slots but with the same operation
    /// and Start is far before the other operation slot
    /// </summary>
    [Test]
    public void TestStopCycleDifferentSlotsFarBeforeSameOperation ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T (0)));

        using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          try {
            // Config
            Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                               (int)(OperationSlotSplitOption.ByDay | OperationSlotSplitOption.ByGlobalShift));

            // Reference data
            IMonitoredMachine machine =
              ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindById (1);
            Assert.NotNull (machine);
            IOperation operation =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.NotNull (operation);
            IShift shift1 = ModelDAOHelper.ModelFactory.CreateShiftFromName ("1");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift1);
            IShift shift2 = ModelDAOHelper.ModelFactory.CreateShiftFromName ("2");
            ModelDAOHelper.DAOFactory.ShiftDAO.MakePersistent (shift2);

            IOperationSlot operationSlot1 =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation, null, null, null, null, UtcDateTime.From (2008, 01, 16), shift1,
                                    new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00),
                                                          UtcDateTime.From (2012, 06, 01)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO
              .MakePersistent (operationSlot1);
            operationSlot1.ConsolidateRunTime ();
            UpdateIntermediateWorkPieceSummary (operationSlot1);
            IOperationSlot operationSlot2 =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation, null, null, null, null, UtcDateTime.From (2012, 06, 01), shift2,
                                    new UtcDateTimeRange (UtcDateTime.From (2012, 06, 01, 00, 00, 00)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO
              .MakePersistent (operationSlot2);
            operationSlot2.ConsolidateRunTime ();
            UpdateIntermediateWorkPieceSummary (operationSlot2);

            StartCycle (machine,
                        UtcDateTime.From (2010, 06, 02));
            StopCycle (machine,
                       UtcDateTime.From (2012, 06, 03));

            ModelDAOHelper.DAOFactory.FlushData ();
            DAOFactory.EmptyAccumulators ();

            {
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (1, operationCycles.Count);
              int i = 0;
              IOperationCycle operationCycle = operationCycles[i];
              Assert.AreEqual (UtcDateTime.From (2010, 06, 02), operationCycle.Begin);
              Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle.End);
              Assert.AreEqual (machine, operationCycle.Machine);
              Assert.AreEqual (operationSlot2, operationCycle.OperationSlot);
              Assert.AreEqual (new OperationCycleStatus (), operationCycle.Status);
            }

            Assert.AreEqual (0, operationSlot1.PartialCycles);
            Assert.AreEqual (0, operationSlot1.TotalCycles);
            Assert.IsNull (operationSlot1.AverageCycleTime);
            CheckSummaries (operationSlot1);
            Assert.AreEqual (1, operationSlot2.TotalCycles);
            Assert.AreEqual (0, operationSlot2.PartialCycles);
            Assert.IsNull (operationSlot2.AverageCycleTime);
            CheckSummaries (operationSlot2);

            // Reset the config
            Lemoine.Info.ConfigSet.ResetForceValues ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test the StartStopCycle method
    /// 
    /// Start and Stop are on different slots,
    /// but Start is close enough the other operation slot
    /// </summary>
    [Test]
    public void TestStartStopCycle4DifferentSlotsButClose ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        IOperationSlot operationSlot1 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00),
                                                      UtcDateTime.From (2012, 06, 01, 00, 00, 02)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot1);
        operationSlot1.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot1);
        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation2, null, null, null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2012, 06, 01, 00, 00, 02)));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot2);
        operationSlot2.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot2);

        StartStopCycle (machine,
                        UtcDateTime.From (2012, 06, 01),
                        UtcDateTime.From (2012, 06, 03));

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.AreEqual (UtcDateTime.From (2012, 06, 01), operationCycle.Begin);
          Assert.AreEqual (UtcDateTime.From (2012, 06, 03), operationCycle.End);
          Assert.AreEqual (machine, operationCycle.Machine);
          Assert.AreEqual (operationSlot2, operationCycle.OperationSlot);
        }

        Assert.AreEqual (0, operationSlot1.PartialCycles);
        Assert.AreEqual (0, operationSlot1.TotalCycles);
        Assert.IsNull (operationSlot1.AverageCycleTime);
        CheckSummaries (operationSlot1);
        Assert.AreEqual (1, operationSlot2.TotalCycles);
        Assert.AreEqual (0, operationSlot2.PartialCycles);
        Assert.IsNull (operationSlot2.AverageCycleTime);
        CheckSummaries (operationSlot2);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test cycle stops just after slot (cycles should become full)
    /// </summary>
    [Test]
    public void TestPartialCyclesBecomeFull ()
    {
      DateTime T0 = UtcDateTime.From (2012, 06, 01);

      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T0));

        ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          // Reference data
          IMonitoredMachine machine =
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          Assert.NotNull (machine);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  new UtcDateTimeRange (T0));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          operationSlot.ConsolidateRunTime ();
          UpdateIntermediateWorkPieceSummary (operationSlot);

          DateTime T1 = T0.AddMinutes (1);

          StartCycle (machine,
                      T1);

          DateTime T2 = T1.AddMinutes (1);

          StartCycle (machine,
                      T2);

          DateTime T3 = T2.AddMinutes (1);

          StartCycle (machine,
                      T3);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          // to invalidate operation cycle consolidation ...
          DateTime T4 = T3.AddMinutes (10);
          operationSlot.EndDateTime = T4;

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();

          Assert.AreEqual (3, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (T1, operationCycles[i].Begin);
          Assert.AreEqual (T2, operationCycles[i].End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
          ++i;
          Assert.AreEqual (T2, operationCycles[i].Begin);
          Assert.AreEqual (T3, operationCycles[i].End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
          ++i;
          Assert.AreEqual (T3, operationCycles[i].Begin);
          Assert.AreEqual (T4, operationCycles[i].End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);


          Assert.AreEqual (3, operationSlot.PartialCycles);
          Assert.AreEqual (0, operationSlot.TotalCycles);

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test partial cycles
    /// </summary>
    [Test]
    public void TestPartialCyclesEndEstimated ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        DateTime T0 = UtcDateTime.From (2012, 06, 01);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (T0));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        DateTime T1 = T0.AddMinutes (1);

        StartCycle (machine,
                    T1);

        DateTime T2 = T1.AddMinutes (1);

        StartCycle (machine,
                    T2);

        DateTime T3 = T2.AddMinutes (1);

        StartCycle (machine,
                    T3);

        (operationSlot as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        // to invalidate operation cycle consolidation ...
        DateTime T4 = T3.AddMinutes (10);
        operationSlot.EndDateTime = T4;

        (operationSlot as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();

        Assert.AreEqual (3, operationCycles.Count);
        int i = 0;
        Assert.AreEqual (T1, operationCycles[i].Begin);
        Assert.AreEqual (T2, operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (T2, operationCycles[i].Begin);
        Assert.AreEqual (T3, operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
        ++i;
        Assert.AreEqual (T3, operationCycles[i].Begin);
        Assert.AreEqual (T4, operationCycles[i].End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);


        Assert.AreEqual (3, operationSlot.PartialCycles);
        Assert.AreEqual (0, operationSlot.TotalCycles);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test partial cycles all at with same begin
    /// </summary>
    [Test]
    public void TestPartialCyclesSameBegin ()
    {
      DateTime sameDate = UtcDateTime.From (2012, 06, 01);

      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));

        ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          // Reference data
          IMonitoredMachine machine =
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          Assert.NotNull (machine);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);

          StartCycle (machine,
                      sameDate);

          StartCycle (machine,
                      sameDate);

          StartCycle (machine,
                      sameDate);

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();

          // reorder ...
          if (operationCycles[0].Id > operationCycles[1].Id) {
            IOperationCycle tmpCycle = operationCycles[1];
            operationCycles[1] = operationCycles[0];
            operationCycles[0] = tmpCycle;
          }

          if (operationCycles[1].Id > operationCycles[2].Id) {
            IOperationCycle tmpCycle = operationCycles[2];
            operationCycles[2] = operationCycles[1];
            operationCycles[1] = tmpCycle;
          }

          if (operationCycles[0].Id > operationCycles[1].Id) {
            IOperationCycle tmpCycle = operationCycles[1];
            operationCycles[1] = operationCycles[0];
            operationCycles[0] = tmpCycle;
          }
          // end reorder

          Assert.AreEqual (3, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (sameDate, operationCycles[i].Begin);
          Assert.AreEqual (sameDate, operationCycles[i].End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (null, operationCycles[i].OperationSlot);
          ++i;
          Assert.AreEqual (sameDate, operationCycles[i].Begin);
          Assert.AreEqual (sameDate, operationCycles[i].End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (null, operationCycles[i].OperationSlot);
          ++i;
          Assert.AreEqual (sameDate, operationCycles[i].Begin);
          Assert.AreEqual (null, operationCycles[i].End);
          Assert.AreEqual (new OperationCycleStatus (), operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (null, operationCycles[i].OperationSlot);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          operationSlot.ConsolidateRunTime ();
          UpdateIntermediateWorkPieceSummary (operationSlot);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAllWithOperationSlot (operationSlot);

          // reorder ...
          if (operationCycles[0].Id > operationCycles[1].Id) {
            IOperationCycle tmpCycle = operationCycles[1];
            operationCycles[1] = operationCycles[0];
            operationCycles[0] = tmpCycle;
          }

          if (operationCycles[1].Id > operationCycles[2].Id) {
            IOperationCycle tmpCycle = operationCycles[2];
            operationCycles[2] = operationCycles[1];
            operationCycles[1] = tmpCycle;
          }

          if (operationCycles[0].Id > operationCycles[1].Id) {
            IOperationCycle tmpCycle = operationCycles[1];
            operationCycles[1] = operationCycles[0];
            operationCycles[0] = tmpCycle;
          }
          // end reorder

          Assert.AreEqual (3, operationCycles.Count);
          i = 0;
          Assert.AreEqual (sameDate, operationCycles[i].Begin);
          Assert.AreEqual (sameDate, operationCycles[i].End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
          ++i;
          Assert.AreEqual (sameDate, operationCycles[i].Begin);
          Assert.AreEqual (sameDate, operationCycles[i].End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
          ++i;
          Assert.AreEqual (sameDate, operationCycles[i].Begin);
          Assert.AreEqual (null, operationCycles[i].End);
          Assert.AreEqual (new OperationCycleStatus (), operationCycles[i].Status);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

          Assert.AreEqual (3, operationSlot.PartialCycles);
          Assert.AreEqual (0, operationSlot.TotalCycles);
          // only partial cycles => no average cycle time
          Assert.AreEqual (null, operationSlot.AverageCycleTime);
          CheckSummaries (operationSlot);

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test whether for a cycle start < (auto) sequence < cycle end
    /// we associate the cycle to the matching operation slot
    /// even if the bounds are slightly off
    /// </summary>
    [Test]
    public void TestCycleWrappingSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          IMachineModule machineModule =
            daoFactory.MachineModuleDAO.FindById (1);
          Assert.NotNull (machineModule);
          IOperation operation1 =
            daoFactory.OperationDAO.FindById (1);
          Assert.NotNull (operation1);
          ISequence sequence1 =
            daoFactory.SequenceDAO.FindById (1);
          Assert.NotNull (sequence1);
          sequence1.AutoOnly = true; // test pass only if set to false, otherwise cycle not associated

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          stamp1.OperationCycleBegin = true;
          daoFactory.StampDAO.MakePersistent (stamp1);

          Stamp stamp2 = new Stamp ();
          stamp2.OperationCycleEnd = true;
          daoFactory.StampDAO.MakePersistent (stamp2);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> (), null);
          OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> (), null);
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable, null);

          // Dates
          DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
          DateTime T1 = T0.AddSeconds (5);
          DateTime T2 = T1.AddSeconds (30);
          DateTime T3 = T2.AddSeconds (5);

          {
            stampDetection.StartStamp (stamp1, null, T0);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T1, T2);
            stampDetection.StartStamp (stamp2, null, T3);
            IList<IOperationSlot> operationSlots =
              daoFactory.OperationSlotDAO.FindAll ();

            ModelDAOHelper.DAOFactory.FlushData ();
            DAOFactory.EmptyAccumulators ();

            Assert.AreEqual (1, operationSlots.Count, "Number of operation slots after 1.");
            Assert.AreEqual (1, operationSlots[0].TotalCycles, "Number of cycles is not 1.");

            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAllWithOperationSlot (operationSlots[0]);

            Assert.AreEqual (1, operationCycles.Count);
            Assert.AreEqual (T0, operationCycles[0].Begin);
            Assert.AreEqual (T3, operationCycles[0].End);

          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    void CheckSummaries (IOperationSlot operationSlot)
    {
      {
        if (operationSlot.Day.HasValue) {
          var summary = new CycleCountSummaryDAO ()
            .FindByKey (operationSlot.Machine,
            operationSlot.Day.Value,
            operationSlot.Shift,
            operationSlot.WorkOrder,
            operationSlot.Line,
            operationSlot.Task,
            operationSlot.Component,
            operationSlot.Operation);
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindByDayShift (operationSlot.Machine, operationSlot.Day.Value, operationSlot.Shift)
            .Where (s => WorkOrder.Equals (operationSlot.WorkOrder, s.WorkOrder))
            .Where (s => Line.Equals (operationSlot.Line, s.Line))
            .Where (s => Task.Equals (operationSlot.Task, s.Task))
            .Where (s => Component.Equals (operationSlot.Component, s.Component))
            .Where (s => Operation.Equals (operationSlot.Operation, s.Operation));
          var total = operationSlots
            .Sum (s => s.TotalCycles);
          var partial = operationSlots
            .Sum (s => s.PartialCycles);
          if (null == summary) {
            Assert.AreEqual (0, total);
            Assert.AreEqual (0, partial);
          }
          else {
            Assert.AreEqual (summary.Full, total);
            Assert.AreEqual (summary.Partial, partial);
          }
        }
      }

      {
        int counted1 = 0;
        int corrected1 = 0;
        int counted2 = 0;
        int corrected2 = 0;
        foreach (IIntermediateWorkPiece intermediateWorkPiece in operationSlot.Operation.IntermediateWorkPieces) {
          {
            var summary =
              new IntermediateWorkPieceByMachineSummaryDAO ()
              .FindByKey (operationSlot.Machine,
                          intermediateWorkPiece,
                          operationSlot.Component,
                          operationSlot.WorkOrder,
                          operationSlot.Line,
                          operationSlot.Task,
                          operationSlot.Day,
                          operationSlot.Shift);
            if (null != summary) {
              counted1 += summary.Counted / intermediateWorkPiece.OperationQuantity;
              corrected1 += summary.Corrected / intermediateWorkPiece.OperationQuantity;
            }
          }
          {
            IIntermediateWorkPieceSummary summary =
              new IntermediateWorkPieceSummaryDAO ()
              .FindByKey (intermediateWorkPiece,
                          operationSlot.Component,
                          operationSlot.WorkOrder,
                          operationSlot.Line,
                          operationSlot.Day,
                          operationSlot.Shift);
            if (null != summary) {
              counted2 += summary.Counted / intermediateWorkPiece.OperationQuantity;
              corrected2 += summary.Corrected / intermediateWorkPiece.OperationQuantity;
            }
          }
        }
        // Because there may be other operaiton slots that target the same summary,
        // there is a LessOrEqual
        Assert.LessOrEqual (operationSlot.TotalCycles, counted1);
        Assert.LessOrEqual (operationSlot.TotalCycles, corrected1);
        Assert.LessOrEqual (operationSlot.TotalCycles, counted2);
        Assert.LessOrEqual (operationSlot.TotalCycles, corrected2);
      }
    }

    /// <summary>
    /// StopCycle after a slot (FEMA occurrence)
    /// 
    /// </summary>
    [Test]
    public void TestFema ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        DateTime T0 = UtcDateTime.From (2000, 1, 1);
        DateTime T1 = T0.AddMinutes (10);
        DateTime T2 = T0.AddMinutes (20);
        DateTime T3 = T0.AddMinutes (21);


        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (T0, T2));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        StartCycle (machine,
                    T1);

        StopCycle (machine,
                   T3);

        (operationSlot as OperationSlot)
          .Consolidate (null);

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (1, operationCycles.Count);
        IOperationCycle operationCycle = operationCycles[0];
        Assert.AreEqual (T1, operationCycle.Begin, "begin cycle1 not T1");
        Assert.AreEqual (T3, operationCycle.End, "end cycle1 not T2");
        Assert.IsTrue (!operationCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated)
                       && !operationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated));

        transaction.Rollback ();
      }
    }


    /// <summary>
    /// StartCycle / StopCycle just around a slot
    /// 
    /// </summary>
    [Test]
    public void TestAcme ()
    {
      ModelDAOHelper.ModelFactory =
      new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
          Assert.NotNull (machine);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);

          DateTime T0 = UtcDateTime.From (2000, 1, 1);
          DateTime T1 = T0.AddMinutes (10);
          DateTime T2 = T0.AddMinutes (20);
          DateTime T3 = T0.AddMinutes (21);


          IOperationSlot operationSlot1 =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  null, null, null, null, null, null, null,
                                  new UtcDateTimeRange (T0));

          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot1);

          StartCycle (machine,
                      T1);

          IOperationSlot operationSlot2 =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  new UtcDateTimeRange (T1.AddSeconds (2), T2));
          operationSlot1.EndDateTime = T1.AddSeconds (2);

          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot1);
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot2);

          (operationSlot1 as OperationSlot)
            .Consolidate (null);
          (operationSlot2 as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.FlushData ();

          StopCycle (machine,
                     T2.AddSeconds (2));

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();

          Assert.AreEqual (1, operationCycles.Count);
          IOperationCycle operationCycle = operationCycles[0];

          {
            IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAll (machine);
            Assert.AreEqual (2, operationSlots.Count);
            Assert.AreEqual (T2.AddSeconds (2), operationSlots[1].EndDateTime.Value);
            Assert.IsNotNull (operationCycle.Begin);
            Assert.IsNotNull (operationCycle.End);
            Assert.AreEqual (T1, operationCycle.Begin);
            Assert.AreEqual (T2.AddSeconds (2), operationCycle.End);
            Assert.AreEqual (operationSlots[1], operationCycle.OperationSlot);
            Assert.AreEqual (0, operationSlots[0].TotalCycles);
            Assert.AreEqual (1, operationSlots[1].TotalCycles);
          }

          /*
          Assert.AreEqual (T1, operationCycle1.Begin, "begin cycle1 not T1");
          Assert.AreEqual (T2, operationCycle1.End, "end cycle1 not T2");
          Assert.IsTrue (operationCycle1.Status.HasValue);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycle1.Status.Value, "status cycle 1 not endestimated");
          Assert.IsTrue(operationCycle2.Begin == null, "begin cycle2 not null");
          Assert.AreEqual (T3, operationCycle2.End, "end cycle2 not T3");
          Assert.IsTrue (operationCycle2.Status == null, "status cycle2 not null");
           */
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test operation slot split in between two cycles
    /// </summary>
    [Test]
    public void TestOperationSlotSplitBetweenTwoCycles ()
    {
      DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);

      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T0));
        Lemoine.Info.ConfigSet.ForceValue<DateTime> ("Database.OperationCycleDAO.MinDateTime", T0);

        ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          // Reference data
          IMonitoredMachine machine =
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          Assert.NotNull (machine);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);
          operation1.MachiningDuration = TimeSpan.FromSeconds (30);
          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.NotNull (operation2);

          DateTime T1 = T0.AddMinutes (1);
          DateTime T2 = T0.AddMinutes (2);
          DateTime T3 = T0.AddMinutes (3);
          DateTime T4 = T0.AddMinutes (4);
          DateTime T5 = T0.AddMinutes (5);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  new UtcDateTimeRange (T0));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);

          StartCycle (machine,
                      T1);
          StopCycle (machine,
                     T2);
          StartCycle (machine,
                      T4);
          StopCycle (machine,
                     T5);
          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          Assert.AreEqual (operationSlot, operationCycles[0].OperationSlot);
          Assert.AreEqual (operationSlot, operationCycles[1].OperationSlot);
          Assert.AreEqual (2, operationSlot.TotalCycles);
          Assert.AreEqual (0, operationSlot.PartialCycles);

          IOperationMachineAssociation association =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T3, null, true);
          association.Operation = operation2;
          // association.End = null;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          association.Apply ();

          IList<IOperationSlot> operationSlots =
            ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();

          Assert.AreEqual (2, operationSlots.Count, "Not 2 operation slots");

          foreach (IOperationSlot opSlot in operationSlots) {
            (opSlot as OperationSlot)
              .Consolidate (null);
          }

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          for (int i = 0; i < 2; i++) {
            Assert.AreEqual (1, operationSlots[i].TotalCycles, "not 1 total cycle");
            Assert.AreEqual (0, operationSlots[i].PartialCycles, "not 0 partial cycle");
          }

          operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          Assert.AreEqual (operationSlots[0], operationCycles[0].OperationSlot);
          Assert.AreEqual (operationSlots[1], operationCycles[1].OperationSlot);

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test operation slot split during a partial cycle
    /// </summary>
    [Test]
    public void TestOperationSlotSplitDuringAPartialCycle ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        operation1.MachiningDuration = TimeSpan.FromSeconds (30);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
        DateTime T1 = T0.AddMinutes (1);
        DateTime T2 = T0.AddMinutes (2);
        DateTime T3 = T0.AddMinutes (3);
        DateTime T4 = T0.AddMinutes (4);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (T0));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        StartCycle (machine,
                    T1);

        StopCycle (machine,
                   T2);

        StartCycle (machine,
                    T3);


        (operationSlot as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (2, operationCycles.Count);
        Assert.AreEqual (operationSlot, operationCycles[0].OperationSlot);
        Assert.AreEqual (operationSlot, operationCycles[1].OperationSlot);
        Assert.AreEqual (1, operationSlot.TotalCycles);
        Assert.AreEqual (1, operationSlot.PartialCycles);



        IOperationMachineAssociation association =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T4, null, true);
        association.Operation = operation2;
        // association.End = null;
        ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
        association.Apply (); // will also consolidate the slots

        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();

        Assert.AreEqual (2, operationSlots.Count, "Not 2 operation slots");

        IOperationSlot operationSlot1 = operationSlots[0];
        IOperationSlot operationSlot2 = operationSlots[1];

        if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
          IOperationSlot tmpSlot = operationSlot1;
          operationSlot1 = operationSlot2;
          operationSlot2 = tmpSlot;
        }

        Assert.AreEqual (T0, operationSlot1.BeginDateTime.Value);
        Assert.IsTrue (operationSlot1.EndDateTime.HasValue);
        Assert.AreEqual (T4, operationSlot1.EndDateTime.Value);
        Assert.AreEqual (T4, operationSlot2.BeginDateTime.Value);

        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (2, operationCycles.Count);
        Assert.AreEqual (operationSlot1, operationCycles[0].OperationSlot);
        Assert.AreEqual (operationSlot1, operationCycles[1].OperationSlot);

        Assert.AreEqual (1, operationSlot1.TotalCycles, "not 1 total cycle");
        Assert.AreEqual (1, operationSlot1.PartialCycles, "not 1 partial cycle");

        Assert.AreEqual (0, operationSlot2.TotalCycles, "not 0 total cycle");
        Assert.AreEqual (0, operationSlot2.PartialCycles, "not 0 partial cycle");

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test operation slot split during a total cycle
    /// </summary>
    [Test]
    public void TestOperationSlotSplitDuringATotalCycle ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        operation1.MachiningDuration = TimeSpan.FromSeconds (30);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
        DateTime T1 = T0.AddMinutes (1);
        DateTime T2 = T0.AddMinutes (2);
        DateTime T3 = T0.AddMinutes (3);
        DateTime T4 = T0.AddMinutes (4);
        DateTime T5 = T0.AddMinutes (5);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (T0));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        StartCycle (machine,
                    T1);

        StopCycle (machine,
                   T2);

        StartCycle (machine,
                    T3);

        StopCycle (machine,
                   T5);

        (operationSlot as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (2, operationCycles.Count);
        Assert.AreEqual (operationSlot, operationCycles[0].OperationSlot);
        Assert.AreEqual (operationSlot, operationCycles[1].OperationSlot);
        Assert.AreEqual (2, operationSlot.TotalCycles);
        Assert.AreEqual (0, operationSlot.PartialCycles);

        IOperationMachineAssociation association =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T4, null, true);
        association.Operation = operation2;
        // association.End = null;
        ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
        association.Apply (); // will also consolidate the slots

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();

        Assert.AreEqual (2, operationSlots.Count, "Not 2 operation slots");

        IOperationSlot operationSlot1 = operationSlots[0];
        IOperationSlot operationSlot2 = operationSlots[1];

        if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
          IOperationSlot tmpSlot = operationSlot1;
          operationSlot1 = operationSlot2;
          operationSlot2 = tmpSlot;
        }

        Assert.AreEqual (T0, operationSlot1.BeginDateTime.Value);
        Assert.IsTrue (operationSlot1.EndDateTime.HasValue);
        Assert.AreEqual (T4, operationSlot1.EndDateTime.Value);
        Assert.AreEqual (T4, operationSlot2.BeginDateTime.Value);

        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (3, operationCycles.Count, "not 3 cycles");
        Assert.AreEqual (operationSlot1, operationCycles[0].OperationSlot);
        Assert.AreEqual (operationSlot1, operationCycles[1].OperationSlot);
        Assert.AreEqual (operationSlot2, operationCycles[2].OperationSlot);

        Assert.AreEqual (1, operationSlot1.TotalCycles, "not 1 total cycle");
        Assert.AreEqual (1, operationSlot1.PartialCycles, "not 1 partial cycle");

        Assert.AreEqual (1, operationSlot2.TotalCycles, "not 1 total cycle");
        Assert.AreEqual (0, operationSlot2.PartialCycles, "not 0 partial cycle");

        Assert.IsTrue (operationCycles[1].End.HasValue);
        Assert.AreEqual (T4, operationCycles[1].End);
        Assert.IsTrue (operationCycles[1].Status.Equals (OperationCycleStatus.EndEstimated));

        Assert.AreEqual (T4, operationCycles[2].Begin);
        Assert.IsTrue (operationCycles[2].Status.Equals (OperationCycleStatus.BeginEstimated));
        Assert.AreEqual (T5, operationCycles[2].End);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test operation slot split during a total cycle but close to begin
    /// </summary>
    [Test]
    public void TestOperationSlotSplitDuringATotalCycleCloseToBegin ()
    {
      DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T0));

        ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          // Reference data
          IMonitoredMachine machine =
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          Assert.NotNull (machine);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);
          operation1.MachiningDuration = TimeSpan.FromSeconds (30);
          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.NotNull (operation2);

          DateTime T1 = T0.AddMinutes (1);
          DateTime T2 = T0.AddMinutes (2);
          DateTime T3 = T0.AddMinutes (3);
          DateTime T4 = T0.AddMinutes (3).AddSeconds (20); // close to T3 ( <= AnalysisConfigHelper.OperationCycleAssociationMargin)
          DateTime T5 = T0.AddMinutes (5);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  new UtcDateTimeRange (T0));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);

          StartCycle (machine,
                      T1);

          StopCycle (machine,
                     T2);

          StartCycle (machine,
                      T3);

          StopCycle (machine,
                     T5);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          Assert.AreEqual (operationSlot, operationCycles[0].OperationSlot);
          Assert.AreEqual (operationSlot, operationCycles[1].OperationSlot);
          Assert.AreEqual (2, operationSlot.TotalCycles);
          Assert.AreEqual (0, operationSlot.PartialCycles);

          IOperationMachineAssociation association =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T4, null, true);
          association.Operation = operation2;
          // association.End = null;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          association.Apply (); // will also consolidate the slots

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          IList<IOperationSlot> operationSlots =
            ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();

          Assert.AreEqual (2, operationSlots.Count, "Not 2 operation slots");

          IOperationSlot operationSlot1 = operationSlots[0];
          IOperationSlot operationSlot2 = operationSlots[1];

          if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
            IOperationSlot tmpSlot = operationSlot1;
            operationSlot1 = operationSlot2;
            operationSlot2 = tmpSlot;
          }

          Assert.AreEqual (T0, operationSlot1.BeginDateTime.Value);
          Assert.IsTrue (operationSlot1.EndDateTime.HasValue);
          Assert.AreEqual (T4, operationSlot1.EndDateTime.Value);
          Assert.AreEqual (T4, operationSlot2.BeginDateTime.Value);

          operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count, "not 2 cycles");
          Assert.AreEqual (operationSlot1, operationCycles[0].OperationSlot);
          Assert.AreEqual (operationSlot2, operationCycles[1].OperationSlot);

          Assert.AreEqual (1, operationSlot1.TotalCycles, "not 1 total cycle");
          Assert.AreEqual (0, operationSlot1.PartialCycles, "not 0 partial cycle");

          Assert.AreEqual (1, operationSlot2.TotalCycles, "not 1 total cycle");
          Assert.AreEqual (0, operationSlot2.PartialCycles, "not 0 partial cycle");

          Assert.AreEqual (T3, operationCycles[1].Begin);
          Assert.IsTrue (operationCycles[1].End.HasValue);
          Assert.AreEqual (T5, operationCycles[1].End);
          Assert.AreEqual (new OperationCycleStatus (), operationCycles[1].Status);


          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test operation slot split during a total cycle but close to end
    /// </summary>
    [Test]
    public void TestOperationSlotSplitDuringATotalCycleCloseToEnd ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        operation1.MachiningDuration = TimeSpan.FromSeconds (30);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation2);

        DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
        DateTime T1 = T0.AddMinutes (1);
        DateTime T2 = T0.AddMinutes (2);
        DateTime T3 = T0.AddMinutes (3);
        DateTime T4 = T0.AddMinutes (4).AddSeconds (40); // close to T5
        DateTime T5 = T0.AddMinutes (5);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                new UtcDateTimeRange (T0));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);

        StartCycle (machine,
                    T1);

        StopCycle (machine,
                   T2);

        StartCycle (machine,
                    T3);

        StopCycle (machine,
                   T5);

        (operationSlot as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (2, operationCycles.Count);
        Assert.AreEqual (operationSlot, operationCycles[0].OperationSlot);
        Assert.AreEqual (operationSlot, operationCycles[1].OperationSlot);
        Assert.AreEqual (2, operationSlot.TotalCycles);
        Assert.AreEqual (0, operationSlot.PartialCycles);

        IOperationMachineAssociation association =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T4, null, true);
        association.Operation = operation2;
        // association.End = null;
        ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
        association.Apply (); // will also consolidate the slots

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();

        Assert.AreEqual (2, operationSlots.Count, "Not 2 operation slots");

        IOperationSlot operationSlot1 = operationSlots[0];
        IOperationSlot operationSlot2 = operationSlots[1];

        if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
          IOperationSlot tmpSlot = operationSlot1;
          operationSlot1 = operationSlot2;
          operationSlot2 = tmpSlot;
        }

        Assert.AreEqual (T0, operationSlot1.BeginDateTime.Value);
        Assert.IsTrue (operationSlot1.EndDateTime.HasValue);
        Assert.AreEqual (T4, operationSlot1.EndDateTime.Value);
        Assert.AreEqual (T4, operationSlot2.BeginDateTime.Value);

        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (3, operationCycles.Count, "not 3 cycles");
        Assert.AreEqual (operationSlot1, operationCycles[0].OperationSlot);
        Assert.AreEqual (operationSlot1, operationCycles[1].OperationSlot);
        Assert.AreEqual (operationSlot2, operationCycles[2].OperationSlot);

        Assert.AreEqual (1, operationSlot1.TotalCycles, "not 1 total cycle");
        Assert.AreEqual (1, operationSlot1.PartialCycles, "not 1 partial cycle");

        Assert.AreEqual (1, operationSlot2.TotalCycles, "not 1 total cycle");
        Assert.AreEqual (0, operationSlot2.PartialCycles, "not 0 partial cycle");

        Assert.AreEqual (T1, operationCycles[0].Begin);
        Assert.IsTrue (operationCycles[0].End.HasValue);
        Assert.AreEqual (T2, operationCycles[0].End);
        Assert.AreEqual (new OperationCycleStatus (), operationCycles[0].Status);
        Assert.AreEqual (T3, operationCycles[1].Begin);
        Assert.IsTrue (operationCycles[1].End.HasValue);
        Assert.AreEqual (T4, operationCycles[1].End);
        Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[1].Status);
        Assert.AreEqual (T4, operationCycles[2].Begin);
        Assert.IsTrue (operationCycles[2].End.HasValue);
        Assert.AreEqual (T5, operationCycles[2].End.Value);
        Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles[2].Status);
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test operation slot split on a cycle
    /// </summary>
    [Test]
    public void TestOperationSlotSplitOnCycle ()
    {
      DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);

      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T0));

        ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          // Reference data
          IMonitoredMachine machine =
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          Assert.NotNull (machine);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);
          operation1.MachiningDuration = TimeSpan.FromSeconds (30);
          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.NotNull (operation2);

          DateTime T1 = T0.AddMinutes (1);
          DateTime T2 = T0.AddMinutes (2);
          DateTime T3 = T0.AddMinutes (3);
          DateTime T4 = T0.AddMinutes (4);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  new UtcDateTimeRange (T0));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);

          StartCycle (machine,
                      T1);

          StopCycle (machine,
                     T2);

          StartCycle (machine,
                      T3);

          StopCycle (machine,
                     T4);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          Assert.AreEqual (operationSlot, operationCycles[0].OperationSlot);
          Assert.AreEqual (operationSlot, operationCycles[1].OperationSlot);
          Assert.AreEqual (2, operationSlot.TotalCycles);
          Assert.AreEqual (0, operationSlot.PartialCycles);

          {
            IList<IOperationSlot> operationSlots =
              ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();
            Assert.AreEqual (1, operationSlots.Count);
          }

          IWorkOrder workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          IWorkOrderMachineAssociation association =
            ModelDAOHelper.ModelFactory.CreateWorkOrderMachineAssociation (machine, workOrder, new UtcDateTimeRange (T3));
          ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (association);
          association.Apply ();

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          {
            IList<IOperationSlot> operationSlots =
              ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();
            Assert.AreEqual (2, operationSlots.Count, "Not 2 operation slots");
            IOperationSlot operationSlot1 = operationSlots[0];
            IOperationSlot operationSlot2 = operationSlots[1];

            if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
              IOperationSlot tmpSlot = operationSlot1;
              operationSlot1 = operationSlot2;
              operationSlot2 = tmpSlot;
            }

            Assert.AreEqual (T0, operationSlot1.BeginDateTime.Value);
            Assert.IsTrue (operationSlot1.EndDateTime.HasValue);
            Assert.AreEqual (T3, operationSlot1.EndDateTime.Value);
            Assert.AreEqual (T3, operationSlot2.BeginDateTime.Value);

            operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.AreEqual (2, operationCycles.Count, "not 2 cycles");
            Assert.AreEqual (operationSlot1, operationCycles[0].OperationSlot);
            Assert.AreEqual (operationSlot2, operationCycles[1].OperationSlot);
          }
          /*
          Assert.AreEqual(2, operationSlot1.TotalCycles, "not 2 total cycle");
          Assert.AreEqual(0, operationSlot1.PartialCycles, "not 0 partial cycle");

          Assert.AreEqual(0, operationSlot2.TotalCycles, "not 0 total cycle");
          Assert.AreEqual(0, operationSlot2.PartialCycles, "not 0 partial cycle");

          Assert.AreEqual(T3, operationCycles[1].Begin);
          Assert.IsTrue(operationCycles[1].End.HasValue);
          Assert.AreEqual(T5, operationCycles[1].End);
          Assert.AreEqual(null, operationCycles[1].Status);
           */

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test the operation slot is extended correctly
    /// </summary>
    [Test]
    public void TestExtendOperationSlot ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);


        OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
        OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
        MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                               operationDetection, operationCycleDetection);

        // Start cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleBegin = true;
          masterDetection.StartStamp (stamp, null,
                                      T (0));
        }

        AddOperation (machine, operation1, T (1), T (2));

        // Stop cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleEnd = true;
          masterDetection.StartStamp (stamp, null,
                                      T (3));
        }

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (T (0), operationCycles[i].Begin);
          Assert.AreEqual (T (3), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (T (0), operationCycles[i].OperationSlot.BeginDateTime.Value);
          Assert.AreEqual (T (3), operationCycles[i].OperationSlot.EndDateTime.Value);
          Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
          CheckSummaries (operationCycles[i].OperationSlot);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the operation slot is extended correctly
    /// when no operation is specified with the cycle stop
    /// </summary>
    [Test]
    public void TestExtendOperationSlotNoOperation ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
        OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
        MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                               operationDetection, operationCycleDetection);

        // Start cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleBegin = true;
          masterDetection.StartStamp (stamp, null,
                                      T (0));
        }

        AddOperation (machine, operation1, T (1), T (2));

        // Stop cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.OperationCycleEnd = true;
          masterDetection.StartStamp (stamp, null,
                                      T (3));
        }

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (T (0), operationCycles[i].Begin);
          Assert.AreEqual (T (3), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (T (0), operationCycles[i].OperationSlot.BeginDateTime.Value);
          Assert.AreEqual (T (3), operationCycles[i].OperationSlot.EndDateTime.Value);
          Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
          CheckSummaries (operationCycles[i].OperationSlot);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the operation slot is extended correctly with a work order ended in oo
    /// </summary>
    [Test]
    public void TestExtendOperationSlotWorkOrder ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IWorkOrder workOrder =
          ModelDAOHelper.DAOFactory.WorkOrderDAO
          .FindById (1);

        OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
        OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
        MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                               operationDetection, operationCycleDetection);

        // Start cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleBegin = true;
          masterDetection.StartStamp (stamp, null,
                                      T (0));
        }

        AddWorkOrder (machine, workOrder, T (1), null);
        AddOperation (machine, operation1, T (1), T (2));

        // Stop cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleEnd = true;
          masterDetection.StartStamp (stamp, null,
                                      T (3));
        }

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationSlot> operationSlots =
            ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindAll (machine);
          Assert.AreEqual (2, operationSlots.Count);
          int i = 0;
          Assert.AreEqual (machine, operationSlots[i].Machine);
          Assert.AreEqual (T (0), operationSlots[i].BeginDateTime.Value);
          Assert.AreEqual (T (3), operationSlots[i].EndDateTime.Value);
          Assert.AreEqual (operation1, operationSlots[i].Operation);
          ++i;
          Assert.AreEqual (machine, operationSlots[i].Machine);
          Assert.AreEqual (T (3), operationSlots[i].BeginDateTime.Value);
          Assert.IsFalse (operationSlots[i].EndDateTime.HasValue);
          Assert.AreEqual (null, operationSlots[i].Operation);
          Assert.AreEqual (workOrder, operationSlots[i].WorkOrder);
        }
        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (T (0), operationCycles[i].Begin);
          Assert.AreEqual (T (3), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (T (0), operationCycles[i].OperationSlot.BeginDateTime.Value);
          Assert.AreEqual (T (3), operationCycles[i].OperationSlot.EndDateTime.Value);
          Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
          CheckSummaries (operationCycles[i].OperationSlot);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the operation slot is extended correctly with a work order ended in oo
    /// and when no operation is specified with the operation cycle stop
    /// </summary>
    [Test]
    public void TestExtendOperationSlotWorkOrderNoOperation ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        IWorkOrder workOrder =
          ModelDAOHelper.DAOFactory.WorkOrderDAO
          .FindById (1);

        OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
        OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
        MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                               operationDetection, operationCycleDetection);

        // Start cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleBegin = true;
          masterDetection.StartStamp (stamp, null,
                                      T (0));
        }

        AddWorkOrder (machine, workOrder, T (1), null);
        AddOperation (machine, operation1, T (1), T (2));

        // Stop cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.OperationCycleEnd = true;
          masterDetection.StartStamp (stamp, null,
                                      T (3));
        }

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationSlot> operationSlots =
            ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindAll (machine);
          Assert.AreEqual (2, operationSlots.Count);
          int i = 0;
          Assert.AreEqual (machine, operationSlots[i].Machine);
          Assert.AreEqual (T (0), operationSlots[i].BeginDateTime.Value);
          Assert.AreEqual (T (3), operationSlots[i].EndDateTime.Value);
          Assert.AreEqual (operation1, operationSlots[i].Operation);
          ++i;
          Assert.AreEqual (machine, operationSlots[i].Machine);
          Assert.AreEqual (T (3), operationSlots[i].BeginDateTime.Value);
          Assert.IsFalse (operationSlots[i].EndDateTime.HasValue);
          Assert.AreEqual (null, operationSlots[i].Operation);
          Assert.AreEqual (workOrder, operationSlots[i].WorkOrder);
        }
        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (T (0), operationCycles[i].Begin);
          Assert.AreEqual (T (3), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (T (0), operationCycles[i].OperationSlot.BeginDateTime.Value);
          Assert.AreEqual (T (3), operationCycles[i].OperationSlot.EndDateTime.Value);
          Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
          CheckSummaries (operationCycles[i].OperationSlot);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the operation slot is extended correctly on limits
    /// </summary>
    [Test]
    public void TestExtendOperationSlotLimits ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
        OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
        MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                               operationDetection, operationCycleDetection);

        // Start cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleBegin = true;
          masterDetection.StartStamp (stamp, null,
                                      T (0));
        }

        AddOperation (machine, operation1, T (0), T (3));

        // Stop cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleEnd = true;
          masterDetection.StartStamp (stamp, null,
                                      T (3));
        }

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (T (0), operationCycles[i].Begin);
          Assert.AreEqual (T (3), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (T (0), operationCycles[i].OperationSlot.BeginDateTime.Value);
          Assert.AreEqual (T (3), operationCycles[i].OperationSlot.EndDateTime.Value);
          Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
          CheckSummaries (operationCycles[i].OperationSlot);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the operation slot is extended correctly
    /// 
    /// with only a start and a stop cycle
    /// </summary>
    [Test]
    public void TestExtendOperationSlot2 ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
        OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
        MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                               operationDetection, operationCycleDetection);

        // Start cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleBegin = true;
          masterDetection.StartStamp (stamp, null,
                                      T (0));
        }

        // Stop cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleEnd = true;
          masterDetection.StartStamp (stamp, null,
                                      T (3));
        }

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (1, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (T (0), operationCycles[i].Begin);
          Assert.AreEqual (T (3), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (T (0), operationCycles[i].OperationSlot.BeginDateTime.Value);
          Assert.AreEqual (T (3), operationCycles[i].OperationSlot.EndDateTime.Value);
          Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
          CheckSummaries (operationCycles[i].OperationSlot);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the 'Extend OperationSlot' process with different operations
    /// </summary>
    [Test]
    public void TestExtendOperationSlotDifferentOperations ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.NotNull (operation1);

        OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
        OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
        MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                               operationDetection, operationCycleDetection);

        // Start cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleBegin = true;
          masterDetection.StartStamp (stamp, null,
                                      T (0));
        }

        AddOperation (machine, operation2, T (3), T (4));

        // Stop cycle
        {
          IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp.Operation = operation1;
          stamp.OperationCycleEnd = true;
          masterDetection.StartStamp (stamp, null,
                                      T (5));
        }

        ModelDAOHelper.DAOFactory.FlushData ();
        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationSlot> operationSlots =
            ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindAll ();
          Assert.AreEqual (2, operationSlots.Count);
          int i = 0;
          Assert.AreEqual (machine, operationSlots[i].Machine);
          Assert.AreEqual (T (0), operationSlots[i].BeginDateTime.Value);
          Assert.AreEqual (T (0).AddSeconds (1), operationSlots[i].EndDateTime.Value);
          Assert.AreEqual (operation1, operationSlots[i].Operation);
          CheckSummaries (operationSlots[i]);
          ++i;
          Assert.AreEqual (machine, operationSlots[i].Machine);
          Assert.AreEqual (T (3), operationSlots[i].BeginDateTime.Value);
          Assert.AreEqual (T (4), operationSlots[i].EndDateTime.Value);
          Assert.AreEqual (operation2, operationSlots[i].Operation);
          CheckSummaries (operationSlots[i]);
        }

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.AreEqual (2, operationCycles.Count);
          int i = 0;
          Assert.AreEqual (T (0), operationCycles[i].Begin);
          Assert.AreEqual (T (0).AddSeconds (1), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (T (0), operationCycles[i].OperationSlot.BeginDateTime.Value);
          Assert.AreEqual (T (0).AddSeconds (1), operationCycles[i].OperationSlot.EndDateTime.Value);
          Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles[i].Status);
          ++i;
          Assert.AreEqual (null, operationCycles[i].Begin);
          Assert.AreEqual (T (5), operationCycles[i].End);
          Assert.AreEqual (machine, operationCycles[i].Machine);
          Assert.AreEqual (null, operationCycles[i].OperationSlot);
          Assert.AreEqual (new OperationCycleStatus (), operationCycles[i].Status);
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the StopCycle method after a complete full cycle
    /// when the option Extend the full cycle when a new cycle end is detected is on
    /// </summary>
    [Test]
    public void TestStopCycleAfterFullCycleAndExtend ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);

        // Option
        Lemoine.Info.ConfigSet.ForceValue<bool> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ExtendFullCycleWhenNewCycleEnd),
                                                 true);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1, null, null, null, null, null, null,
                                R (0, null));
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        operationSlot.ConsolidateRunTime ();
        UpdateIntermediateWorkPieceSummary (operationSlot);

        StartCycle (machine,
                    T (1));
        StopCycle (machine,
                   T (2));
        StopCycle (machine,
                   T (3));
        StartCycle (machine,
                    T (4));
        StopCycle (machine,
                   T (5));
        StopCycle (machine,
                   T (6));

        DAOFactory.EmptyAccumulators ();

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.AreEqual (2, operationCycles.Count);
        int i = 0;
        Assert.AreEqual (T (1), operationCycles[i].Begin);
        Assert.AreEqual (T (3), operationCycles[i].End);
        Assert.AreEqual (machine, operationCycles[i].Machine);
        Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

        Assert.AreEqual (0, operationSlot.PartialCycles);
        Assert.AreEqual (2, operationSlot.TotalCycles);
        Assert.AreEqual (TimeSpan.FromSeconds (3), operationSlot.AverageCycleTime);
        CheckSummaries (operationSlot);

        Lemoine.Info.ConfigSet.ResetForceValues ();

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the insertion of a betweencycles row
    /// </summary>
    [Test]
    public void TestBetweenCyclesPalletChangingTime ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));

          // Reference data
          IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
          Assert.NotNull (machine);
          machine.PalletChangingDuration = TimeSpan.FromSeconds (2);

          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  R (0, null));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          operationSlot.ConsolidateRunTime ();
          UpdateIntermediateWorkPieceSummary (operationSlot);

          StopCycle (machine,
                     T (1));
          StartCycle (machine,
                      T (2));

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.AreEqual (2, operationCycles.Count);
            int i = 0;
            Assert.AreEqual (T (1), operationCycles[i].End);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
            ++i;
            Assert.AreEqual (T (2), operationCycles[i].Begin);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

            {
              IList<IBetweenCycles> betweenCycless =
                ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindAll ();
              Assert.AreEqual (1, betweenCycless.Count);
              int j = 0;
              IBetweenCycles betweenCycles = betweenCycless[j];
              Assert.AreEqual (T (1), betweenCycles.Begin);
              Assert.AreEqual (T (2), betweenCycles.End);
              Assert.AreEqual (machine, betweenCycles.Machine);
              Assert.AreEqual (operationCycles[0], betweenCycles.PreviousCycle);
              Assert.AreEqual (operationCycles[1], betweenCycles.NextCycle);
              Assert.AreEqual (50, betweenCycles.OffsetDuration);
            }
          }

          Assert.AreEqual (1, operationSlot.PartialCycles);
          Assert.AreEqual (1, operationSlot.TotalCycles);
          CheckSummaries (operationSlot);

        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the insertion of a betweencycles row with an offset that
    /// is computed from the loading / unloading time
    /// </summary>
    [Test]
    public void TestBetweenCyclesLoadingUnloadingTime ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));

          // Reference data
          IMonitoredMachine machine =
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          Assert.NotNull (machine);

          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);
          operation1.UnloadingDuration = TimeSpan.FromSeconds (3);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.NotNull (operation2);
          operation2.LoadingDuration = TimeSpan.FromSeconds (7);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation2);

          IOperationSlot operationSlot1 =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  R (0, 4));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot1);
          operationSlot1.ConsolidateRunTime ();
          UpdateIntermediateWorkPieceSummary (operationSlot1);
          IOperationSlot operationSlot2 =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation2, null, null, null, null, null, null,
                                  R (4, null));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot2);
          operationSlot2.ConsolidateRunTime ();
          UpdateIntermediateWorkPieceSummary (operationSlot2);

          StopCycle (machine,
                     T (1));
          StartCycle (machine,
                      T (21));

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          Assert.AreEqual (null, machine.PalletChangingDuration);
          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.AreEqual (2, operationCycles.Count);
            int i = 0;
            Assert.AreEqual (T (1), operationCycles[i].End);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot1, operationCycles[i].OperationSlot);
            ++i;
            Assert.AreEqual (T (21), operationCycles[i].Begin);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot2, operationCycles[i].OperationSlot);

            {
              IList<IBetweenCycles> betweenCycless =
                ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindAll ();
              Assert.AreEqual (1, betweenCycless.Count);
              int j = 0;
              IBetweenCycles betweenCycles = betweenCycless[j];
              Assert.AreEqual (T (1), betweenCycles.Begin);
              Assert.AreEqual (T (21), betweenCycles.End);
              Assert.AreEqual (machine, betweenCycles.Machine);
              Assert.AreEqual (operationCycles[0], betweenCycles.PreviousCycle);
              Assert.AreEqual (operationCycles[1], betweenCycles.NextCycle);
              Assert.AreEqual (operationSlot2, betweenCycles.NextCycle.OperationSlot);
              Assert.AreEqual (200, betweenCycles.OffsetDuration); // 100 * 20 / 10
            }
          }

          Assert.AreEqual (0, operationSlot1.PartialCycles);
          Assert.AreEqual (1, operationSlot1.TotalCycles);
          CheckSummaries (operationSlot1);
          Assert.AreEqual (1, operationSlot2.PartialCycles);
          Assert.AreEqual (0, operationSlot2.TotalCycles);
          CheckSummaries (operationSlot2);

        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the offset duration of a betweencycles row
    /// is still correct after a change of operation slots
    /// </summary>
    [Test]
    public void TestBetweenCyclesOperationCycleChange ()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T (0)));

          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));
          Lemoine.Extensions.ExtensionManager
            .Add (typeof (Lemoine.Plugin.CycleDurationSummary.OperationCycleOffsetDurationAccumulatorExtension));

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          Assert.NotNull (machine);

          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.NotNull (operation1);
          operation1.UnloadingDuration = TimeSpan.FromSeconds (10);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.NotNull (operation2);
          operation2.LoadingDuration = TimeSpan.FromSeconds (20);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation2);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1, null, null, null, null, null, null,
                                  R (0, null));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          operationSlot.ConsolidateRunTime ();
          UpdateIntermediateWorkPieceSummary (operationSlot);

          StopCycle (machine,
                     T (1));
          StartCycle (machine,
                      T (21));

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          Assert.AreEqual (null, machine.PalletChangingDuration);
          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.AreEqual (2, operationCycles.Count);
            int i = 0;
            Assert.AreEqual (T (1), operationCycles[i].End);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
            ++i;
            Assert.AreEqual (T (21), operationCycles[i].Begin);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

            {
              IList<IBetweenCycles> betweenCycless =
                ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindAll ();
              Assert.AreEqual (1, betweenCycless.Count);
              int j = 0;
              IBetweenCycles betweenCycles = betweenCycless[j];
              Assert.AreEqual (T (1), betweenCycles.Begin);
              Assert.AreEqual (T (21), betweenCycles.End);
              Assert.AreEqual (machine, betweenCycles.Machine);
              Assert.AreEqual (operationCycles[0], betweenCycles.PreviousCycle);
              Assert.AreEqual (operationCycles[1], betweenCycles.NextCycle);
              Assert.AreEqual (operationSlot, betweenCycles.NextCycle.OperationSlot);
              Assert.AreEqual (200, betweenCycles.OffsetDuration); // 100 * 20 / 10
            }
          }

          Assert.AreEqual (1, operationSlot.PartialCycles);
          Assert.AreEqual (1, operationSlot.TotalCycles);
          CheckSummaries (operationSlot);

          // Change the operation of operation slot
          IOperationMachineAssociation association =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T (0), null, true);
          association.Operation = operation2;
          association.Apply ();
          DAOFactory.EmptyAccumulators ();

          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.AreEqual (2, operationCycles.Count);
            int i = 0;
            Assert.AreEqual (T (1), operationCycles[i].End);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operation2, operationCycles[i].OperationSlot.Operation);
            ++i;
            Assert.AreEqual (T (21), operationCycles[i].Begin);
            Assert.AreEqual (machine, operationCycles[i].Machine);
            Assert.AreEqual (operation2, operationCycles[i].OperationSlot.Operation);

            {
              IList<IBetweenCycles> betweenCycless =
                ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindAll ();
              Assert.AreEqual (1, betweenCycless.Count);
              int j = 0;
              IBetweenCycles betweenCycles = betweenCycless[j];
              Assert.AreEqual (T (1), betweenCycles.Begin);
              Assert.AreEqual (T (21), betweenCycles.End);
              Assert.AreEqual (machine, betweenCycles.Machine);
              Assert.AreEqual (operationCycles[0], betweenCycles.PreviousCycle);
              Assert.AreEqual (operationCycles[1], betweenCycles.NextCycle);
              Assert.AreEqual (100, betweenCycles.OffsetDuration); // 100 * 20 / 20
            }
          }

        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    /// <summary>
    /// Test the offset duration of a betweencycles row
    /// is still correct after a split in an operation cycle
    /// </summary>
    [Test]
    public void TestBetweenCyclesSplitOperationCycle ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T (0)));

        ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          try {
            Lemoine.Extensions.ExtensionManager
              .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));

            // Config
            Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)OperationSlotSplitOption.None);

            // Reference data
            IMonitoredMachine machine =
              ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindById (1);
            Assert.NotNull (machine);

            IOperation operation1 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.NotNull (operation1);
            operation1.UnloadingDuration = TimeSpan.FromSeconds (10);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

            IOperation operation2 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (2);
            Assert.NotNull (operation2);
            operation2.LoadingDuration = TimeSpan.FromSeconds (20);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation2);

            IOperationSlot operationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1, null, null, null, null, null, null,
                                    R (0, null));
            ModelDAOHelper.DAOFactory.OperationSlotDAO
              .MakePersistent (operationSlot);
            operationSlot.ConsolidateRunTime ();
            UpdateIntermediateWorkPieceSummary (operationSlot);

            DateTime split = T (10000);
            DateTime stop2 = T (20000);

            StopCycle (machine,
                       T (1));
            StartStopCycle (machine,
                            T (21),
                            stop2);

            ModelDAOHelper.DAOFactory.FlushData ();
            DAOFactory.EmptyAccumulators ();

            Assert.AreEqual (null, machine.PalletChangingDuration);
            {
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (2, operationCycles.Count);
              int i = 0;
              Assert.AreEqual (T (1), operationCycles[i].End);
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);
              ++i;
              Assert.AreEqual (T (21), operationCycles[i].Begin);
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operationSlot, operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.AreEqual (1, betweenCycless.Count);
                int j = 0;
                IBetweenCycles betweenCycles = betweenCycless[j];
                Assert.AreEqual (T (1), betweenCycles.Begin);
                Assert.AreEqual (T (21), betweenCycles.End);
                Assert.AreEqual (machine, betweenCycles.Machine);
                Assert.AreEqual (operationCycles[0], betweenCycles.PreviousCycle);
                Assert.AreEqual (operationCycles[1], betweenCycles.NextCycle);
                Assert.AreEqual (operationSlot, betweenCycles.NextCycle.OperationSlot);
                Assert.AreEqual (200, betweenCycles.OffsetDuration); // 100 * 20 / 10
              }
            }

            Assert.AreEqual (0, operationSlot.PartialCycles);
            Assert.AreEqual (2, operationSlot.TotalCycles);
            CheckSummaries (operationSlot);

            // Split
            {
              IOperationMachineAssociation association =
                ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, split, null, true);
              association.Operation = operation2;
              association.Apply ();
            }

            DAOFactory.EmptyAccumulators ();

            {
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (3, operationCycles.Count);
              int i = 0;
              Assert.AreEqual (T (1), operationCycles[i].End);
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
              ++i;
              Assert.AreEqual (T (21), operationCycles[i].Begin);
              Assert.AreEqual (split, operationCycles[i].End);
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
              Assert.IsFalse (operationCycles[i].Full);
              ++i;
              Assert.AreEqual (split, operationCycles[i].Begin);
              Assert.AreEqual (stop2, operationCycles[i].End);
              Assert.AreEqual (operation2, operationCycles[i].OperationSlot.Operation);
              Assert.IsTrue (operationCycles[i].Full);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.AreEqual (1, betweenCycless.Count);
                int j = 0;
                IBetweenCycles betweenCycles = betweenCycless[j];
                Assert.AreEqual (T (1), betweenCycles.Begin);
                Assert.AreEqual (T (21), betweenCycles.End);
                Assert.AreEqual (machine, betweenCycles.Machine);
                Assert.AreEqual (operationCycles[0], betweenCycles.PreviousCycle);
                Assert.AreEqual (operationCycles[1], betweenCycles.NextCycle);
                Assert.AreEqual (200, betweenCycles.OffsetDuration);
              }
            }

            // Merge then
            {
              IOperationMachineAssociation association =
                ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, split, null, true);
              association.Operation = operation1;
              association.Apply ();
            }

            DAOFactory.EmptyAccumulators ();

            {
              IList<IOperationSlot> operationSlots =
                ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindAll (machine);
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (2, operationCycles.Count);
              int i = 0;
              Assert.AreEqual (T (1), operationCycles[i].End);
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
              CheckSummaries (operationCycles[i].OperationSlot);
              ++i;
              Assert.AreEqual (T (21), operationCycles[i].Begin);
              Assert.AreEqual (stop2, operationCycles[i].End);
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
              Assert.IsTrue (operationCycles[i].Full);
              CheckSummaries (operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.AreEqual (1, betweenCycless.Count);
                int j = 0;
                IBetweenCycles betweenCycles = betweenCycless[j];
                Assert.AreEqual (T (1), betweenCycles.Begin);
                Assert.AreEqual (T (21), betweenCycles.End);
                Assert.AreEqual (machine, betweenCycles.Machine);
                Assert.AreEqual (operationCycles[0], betweenCycles.PreviousCycle);
                Assert.AreEqual (operationCycles[1], betweenCycles.NextCycle);
                Assert.AreEqual (200, betweenCycles.OffsetDuration);
              }
            }

            ModelDAOHelper.DAOFactory.FlushData ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test a partial cycle is re-attached to a full cycle
    /// </summary>
    [Test]
    public void TestReattachPartialCloseToSlot ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T (0)));
        Lemoine.Info.ConfigSet.ForceValue<TimeSpan> ("Analysis.OperationCycleAssociationMargin", TimeSpan.FromSeconds (2));

        ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          try {
            Lemoine.Extensions.ExtensionManager
              .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));

            // Config
            Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)OperationSlotSplitOption.None);

            // Reference data
            IMonitoredMachine machine =
              ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindById (1);
            Assert.NotNull (machine);

            IOperation operation1 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.NotNull (operation1);
            operation1.UnloadingDuration = TimeSpan.FromSeconds (10);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

            IOperation operation2 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (2);
            Assert.NotNull (operation2);
            operation2.LoadingDuration = TimeSpan.FromSeconds (20);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation2);

            var component1 = ModelDAOHelper.DAOFactory.ComponentDAO
              .FindById (1);
            Assert.NotNull (component1);

            var workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO
              .FindById (1);
            Assert.NotNull (workOrder1);

            IOperationSlot operationSlot1 =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1, null, null, null, null, null, null,
                                    R (0, 3));
            ModelDAOHelper.DAOFactory.OperationSlotDAO
              .MakePersistent (operationSlot1);
            operationSlot1.ConsolidateRunTime ();
            UpdateIntermediateWorkPieceSummary (operationSlot1);

            IOperationSlot operationSlot2 =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation2, component1, workOrder1, null, null, null, null,
                                    R (3, null));
            ModelDAOHelper.DAOFactory.OperationSlotDAO
              .MakePersistent (operationSlot2);
            operationSlot2.ConsolidateRunTime ();
            UpdateIntermediateWorkPieceSummary (operationSlot2);

            StartCycle (machine, T (1));
            StopCycle (machine, T (4));

            ModelDAOHelper.DAOFactory.FlushData ();
            DAOFactory.EmptyAccumulators ();

            {
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (2, operationCycles.Count);
              int i = 0;
              Assert.AreEqual (T (1), operationCycles[i].Begin);
              Assert.AreEqual (T (3), operationCycles[i].End);
              Assert.IsTrue (operationCycles[i].HasRealBegin ());
              Assert.IsTrue (!operationCycles[i].HasRealEnd ());
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operationSlot1, operationCycles[i].OperationSlot);
              ++i;
              Assert.AreEqual (T (3), operationCycles[i].Begin);
              Assert.AreEqual (T (4), operationCycles[i].End);
              Assert.IsTrue (!operationCycles[i].HasRealBegin ());
              Assert.IsTrue (operationCycles[i].HasRealEnd ());
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operationSlot2, operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.AreEqual (0, betweenCycless.Count);
              }
            }

            Assert.AreEqual (1, operationSlot1.PartialCycles);
            Assert.AreEqual (0, operationSlot1.TotalCycles);
            Assert.AreEqual (0, operationSlot2.PartialCycles);
            Assert.AreEqual (1, operationSlot2.TotalCycles);
            CheckSummaries (operationSlot1);
            CheckSummaries (operationSlot2);

            // Move operationSlot2
            {
              IOperationMachineAssociation association =
                ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T (2), null, true);
              association.Operation = operation2;
              association.Apply ();
            }

            DAOFactory.EmptyAccumulators ();
            ModelDAOHelper.DAOFactory.FlushData ();

            {
              IList<IOperationSlot> operationSlots =
                ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindAll (machine);
              Assert.AreEqual (2, operationSlots.Count);
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (1, operationCycles.Count);
              int i = 0;
              Assert.AreEqual (T (1), operationCycles[i].Begin);
              Assert.AreEqual (T (4), operationCycles[i].End);
              Assert.IsTrue (operationCycles[i].HasRealBegin ());
              Assert.IsTrue (operationCycles[i].HasRealEnd ());
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operation2, operationCycles[i].OperationSlot.Operation);
              CheckSummaries (operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.AreEqual (0, betweenCycless.Count);
              }

              Assert.AreEqual (0, operationSlot1.PartialCycles);
              Assert.AreEqual (0, operationSlot1.TotalCycles);
              Assert.AreEqual (0, operationCycles[i].OperationSlot.PartialCycles);
              Assert.AreEqual (1, operationCycles[i].OperationSlot.TotalCycles);
            }

            ModelDAOHelper.DAOFactory.FlushData ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test a new shift with a cycle between the two shifts
    /// </summary>
    //    [Test] // Note: this test does not work with nunit-console, but it works with VisualStudio
    // This generates this following error
    /*
    System.Net.Sockets.SocketException : An existing connection was forcibly closed by the remote host

    --SocketException
    An existing connection was forcibly closed by the remote host
       at NUnit.Engine.Communication.Transports.Tcp.TestAgentTcpProxy.SendCommandMessage(String command, Object[] arguments)
       at NUnit.Engine.Runners.ProcessRunner.Dispose(Boolean disposing)
       at NUnit.Engine.Runners.AbstractTestRunner.Dispose()
       at NUnit.Engine.Runners.MasterTestRunner.Dispose(Boolean disposing)
       at NUnit.Engine.Runners.MasterTestRunner.Dispose()
       at NUnit.ConsoleRunner.ConsoleRunner.RunTests(TestPackage package, TestFilter filter)
       at NUnit.ConsoleRunner.Program.Main(String[] args)
     */
    public void TestNewShift ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue<LowerBound<DateTime>> ("Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit", new LowerBound<DateTime> (T (0)));
        Lemoine.Info.ConfigSet.ForceValue<TimeSpan> ("Analysis.OperationCycleAssociationMargin", TimeSpan.FromSeconds (2));

        ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
        IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          try {
            Lemoine.Extensions.ExtensionManager
              .Add (typeof (Lemoine.Plugin.CycleDurationSummary.BetweenCyclesOffsetDurationExtension));

            // Config
            Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)OperationSlotSplitOption.None);

            // Reference data
            IMonitoredMachine machine =
              ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindById (1);
            Assert.NotNull (machine);

            IOperation operation1 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.NotNull (operation1);
            operation1.UnloadingDuration = TimeSpan.FromSeconds (10);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

            var component1 = ModelDAOHelper.DAOFactory.ComponentDAO
              .FindById (1);
            Assert.NotNull (component1);

            var workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO
              .FindById (1);
            Assert.NotNull (workOrder1);

            var shift1 = ModelDAOHelper.DAOFactory.ShiftDAO
              .FindById (1);
            Assert.NotNull (shift1);

            var shift2 = ModelDAOHelper.DAOFactory.ShiftDAO
              .FindById (2);
            Assert.NotNull (shift2);

            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
              .FindById (1);
            Assert.NotNull (attended);

            IOperationSlot operationSlot1 =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1, null, null, null, null, T (0).Date, shift1,
                                    R (0, 2));
            ModelDAOHelper.DAOFactory.OperationSlotDAO
              .MakePersistent (operationSlot1);
            operationSlot1.ConsolidateRunTime ();
            UpdateIntermediateWorkPieceSummary (operationSlot1);

            IOperationSlot operationSlot2 =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1, null, null, null, null, T (0).Date, shift2,
                                    R (2, 3));
            ModelDAOHelper.DAOFactory.OperationSlotDAO
              .MakePersistent (operationSlot2);
            operationSlot2.ConsolidateRunTime ();
            UpdateIntermediateWorkPieceSummary (operationSlot2);

            {
              var association = ModelDAOHelper.ModelFactory
                .CreateMachineObservationStateAssociation (machine, attended, R (2));
              association.Shift = shift2;
              association.Apply ();
            }

            StartCycle (machine, T (1));
            {
              IList<IOperationSlot> operationSlots =
                ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindAll (machine);
            }
            StopCycleWithoutExtendingOperation (machine, T (4));

            ModelDAOHelper.DAOFactory.FlushData ();
            DAOFactory.EmptyAccumulators ();

            {
              IList<IOperationSlot> operationSlots =
                ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindAll (machine);
            }
            {
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (2, operationCycles.Count);
              int i = 0;
              Assert.AreEqual (T (1), operationCycles[i].Begin);
              Assert.AreEqual (T (2), operationCycles[i].End);
              Assert.IsTrue (operationCycles[i].HasRealBegin ());
              Assert.IsTrue (!operationCycles[i].HasRealEnd ());
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operationSlot1, operationCycles[i].OperationSlot);
              ++i;
              Assert.AreEqual (null, operationCycles[i].Begin);
              Assert.AreEqual (T (4), operationCycles[i].End);
              Assert.IsTrue (!operationCycles[i].HasRealBegin ());
              Assert.IsTrue (operationCycles[i].HasRealEnd ());
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (null, operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.AreEqual (0, betweenCycless.Count);
              }
            }

            Assert.AreEqual (1, operationSlot1.PartialCycles);
            Assert.AreEqual (0, operationSlot1.TotalCycles);
            Assert.AreEqual (0, operationSlot2.PartialCycles);
            Assert.AreEqual (0, operationSlot2.TotalCycles);
            CheckSummaries (operationSlot1);
            CheckSummaries (operationSlot2);

            // Move operationSlot2
            ExtendOperation (machine, T (5));

            DAOFactory.EmptyAccumulators ();
            ModelDAOHelper.DAOFactory.FlushData ();

            {
              IList<IOperationSlot> operationSlots =
                ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindAll (machine);
              Assert.AreEqual (2, operationSlots.Count);
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.AreEqual (1, operationCycles.Count);
              int i = 0;
              Assert.AreEqual (T (1), operationCycles[i].Begin);
              Assert.AreEqual (T (4), operationCycles[i].End);
              Assert.IsTrue (operationCycles[i].HasRealBegin ());
              Assert.IsTrue (operationCycles[i].HasRealEnd ());
              Assert.AreEqual (machine, operationCycles[i].Machine);
              Assert.AreEqual (operation1, operationCycles[i].OperationSlot.Operation);
              CheckSummaries (operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.AreEqual (0, betweenCycless.Count);
              }

              Assert.AreEqual (0, operationSlot1.PartialCycles);
              Assert.AreEqual (0, operationSlot1.TotalCycles);
              Assert.AreEqual (0, operationCycles[i].OperationSlot.PartialCycles);
              Assert.AreEqual (1, operationCycles[i].OperationSlot.TotalCycles);
            }

            ModelDAOHelper.DAOFactory.FlushData ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }


    void UpdateIntermediateWorkPieceSummary (IOperationSlot operationSlot)
    {
      foreach (IIntermediateWorkPiece intermediateWorkPiece in operationSlot.Operation.IntermediateWorkPieces) {
        {
          IIntermediateWorkPieceByMachineSummary summary =
            new IntermediateWorkPieceByMachineSummaryDAO ()
            .FindByKey (operationSlot.Machine,
                        intermediateWorkPiece,
                        operationSlot.Component,
                        operationSlot.WorkOrder,
                        operationSlot.Line,
                        operationSlot.Task,
                        operationSlot.Day,
                        operationSlot.Shift);
          if (null == summary) {
            summary = new IntermediateWorkPieceByMachineSummary (operationSlot.Machine,
                                                            intermediateWorkPiece,
                                                            operationSlot.Component,
                                                            operationSlot.WorkOrder,
                                                            operationSlot.Line,
                                                            operationSlot.Task,
                                                            operationSlot.Day,
                                                            operationSlot.Shift);
          }
          summary.Counted += operationSlot.TotalCycles * intermediateWorkPiece.OperationQuantity;
          summary.Corrected += operationSlot.TotalCycles * intermediateWorkPiece.OperationQuantity;
          new IntermediateWorkPieceByMachineSummaryDAO ()
            .MakePersistent (summary);
        }
      }
      NHibernateHelper.GetCurrentSession ().Flush ();
    }

    void StartCycle (IMonitoredMachine machine, DateTime dateTime)
    {
      OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
      OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
      MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                             operationDetection, operationCycleDetection);

      IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
      stamp.OperationCycleBegin = true;
      masterDetection.StartStamp (stamp, null,
                                  dateTime);
    }

    void StopCycle (IMonitoredMachine machine, DateTime dateTime)
    {
      OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
      OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
      MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                             operationDetection, operationCycleDetection);

      IStamp stamp = ModelDAOHelper.ModelFactory.CreateStamp ();
      stamp.OperationCycleEnd = true;
      masterDetection.StartStamp (stamp, null,
                                  dateTime);
    }

    void StopCycleWithoutExtendingOperation (IMonitoredMachine machine, DateTime dateTime)
    {
      OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
      operationCycleDetection.StopCycle (null, dateTime);
    }

    void ExtendOperation (IMonitoredMachine machine, DateTime dateTime)
    {
      OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
      operationDetection.ExtendOperation (null, dateTime);
    }

    void StartStopCycle (IMonitoredMachine machine,
                         DateTime startDateTime,
                         DateTime stopDateTime)
    {
      OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machine, new List<IDetectionExtension> ());
      OperationDetection operationDetection = new OperationDetection (machine, new List<IOperationDetectionExtension> ());
      MasterDetection masterDetection = new MasterDetection (machine.MainMachineModule, TransactionLevel.Serializable,
                                                             operationDetection, operationCycleDetection);

      masterDetection.StartStopCycle (startDateTime, stopDateTime);
    }

    void AddOperation (IMachine machine, IOperation operation, DateTime begin, DateTime? end)
    {
      AddOperation (machine, operation, new UtcDateTimeRange (begin, new UpperBound<DateTime> (end)));
    }

    void AddOperation (IMachine machine, IOperation operation, UtcDateTimeRange range)
    {
      IOperationMachineAssociation association =
        ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, range, null, true);
      association.Operation = operation;
      association.Apply ();
    }

    void AddWorkOrder (IMachine machine, IWorkOrder workOrder, DateTime begin, DateTime? end)
    {
      AddWorkOrder (machine, workOrder, new UtcDateTimeRange (begin, new UpperBound<DateTime> (end)));
    }

    void AddWorkOrder (IMachine machine, IWorkOrder workOrder, UtcDateTimeRange range)
    {
      IWorkOrderMachineAssociation association =
        ModelDAOHelper.ModelFactory.CreateWorkOrderMachineAssociation (machine, workOrder, range);
      association.Apply ();
    }
  }
}
