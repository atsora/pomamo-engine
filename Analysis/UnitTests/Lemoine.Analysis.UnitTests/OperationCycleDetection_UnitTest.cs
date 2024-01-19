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
        Assert.That (machine, Is.Not.Null);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 19));

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.That (operationCycles, Has.Count.EqualTo (1));
        int i = 0;
        IOperationCycle operationCycle = operationCycles[i];
        Assert.Multiple (() => {
          Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
          Assert.That (operationCycle.End, Is.Null);
          Assert.That (operationCycle.Machine, Is.EqualTo (machine));
          Assert.That (operationCycle.OperationSlot, Is.Null);
        });

        IList<IBetweenCycles> betweenCycless =
          ModelDAOHelper.DAOFactory.BetweenCyclesDAO
          .FindAll ();
        Assert.That (betweenCycless, Is.Empty);

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

        {
          IList<ICycleCountSummary> summaries =
            new CycleCountSummaryDAO ()
            .FindAll ();
          Assert.That (summaries, Is.Empty);
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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
            Assert.That (operationCycle.End, Is.Null);
          });
          Assert.Multiple (() => {
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot));

            Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (0));
          });
          Assert.That (operationSlot.AverageCycleTime, Is.Null);
          CheckSummaries (operationSlot);
        }

        IList<IBetweenCycles> betweenCycless =
          ModelDAOHelper.DAOFactory.BetweenCyclesDAO
          .FindAll ();
        Assert.That (betweenCycless, Is.Empty);

        {
          IList<ICycleCountSummary> summaries =
            new CycleCountSummaryDAO ()
            .FindAll ();
          Assert.That (summaries, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (summaries[i].Operation, Is.EqualTo (operationSlot.Operation));
            Assert.That (summaries[i].Component, Is.EqualTo (operationSlot.Component));
            Assert.That (summaries[i].WorkOrder, Is.EqualTo (operationSlot.WorkOrder));
            Assert.That (summaries[i].Full, Is.EqualTo (0));
            Assert.That (summaries[i].Partial, Is.EqualTo (1));
          });
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
        Assert.That (machine, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
            Assert.That (operationCycle.End, Is.Null);
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.Null);
          });
        }

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Is.Empty);
        }

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ();
          Assert.That (detectionAnalysisLogs, Has.Count.EqualTo (1));
          int i = 0;
          IDetectionAnalysisLog detectionAnalysisLog = detectionAnalysisLogs[i];
          Assert.Multiple (() => {
            Assert.That (detectionAnalysisLog.Machine, Is.EqualTo (machine));
            Assert.That (detectionAnalysisLog.Level, Is.EqualTo (LogLevel.ERROR));
          });
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
        Assert.That (machine, Is.Not.Null);

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 19));

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.That (operationCycles, Has.Count.EqualTo (1));
        int i = 0;
        IOperationCycle operationCycle = operationCycles[i];
        Assert.That (operationCycle.Begin, Is.Null);
        Assert.Multiple (() => {
          Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
          Assert.That (operationCycle.Machine, Is.EqualTo (machine));
        });
        Assert.That (operationCycle.OperationSlot, Is.Null);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Is.Empty);
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IWorkOrder workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO
          .FindById (2);
        Assert.That (workOrder2, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (operationSlot.BeginDateTime.NullableValue));
            Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
            Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 20)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot));

            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
            Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromDays (1)));
          });
          CheckSummaries (operationSlot);
        }

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Is.Empty);
          // No gap between the two operation cycles
        }

        // Extend the operation slot
        AddOperation (machine, operation1, UtcDateTime.From (2012, 06, 20), UtcDateTime.From (2012, 06, 22));
        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (operationSlot.BeginDateTime.NullableValue));
            Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
            Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 20)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot));

            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
            Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromDays (1)));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IWorkOrder workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO
          .FindById (2);
        Assert.That (workOrder2, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (30));
          {
            IOperationCycle operationCycle = operationCycles[0];
            Assert.Multiple (() => {
              Assert.That (operationCycle.Begin, Is.EqualTo (T (0)));
              Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
              Assert.That (operationCycle.End, Is.EqualTo (T (2)));
              Assert.That (operationCycle.Machine, Is.EqualTo (machine));
              Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot));
            });
          }
          for (int i = 1; i < 30; i++) {
            IOperationCycle operationCycle = operationCycles[i];
            Assert.Multiple (() => {
              Assert.That (operationCycle.Begin, Is.EqualTo (T (i + 1)));
              Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
              Assert.That (operationCycle.End, Is.EqualTo (T (i + 2)));
              Assert.That (operationCycle.Machine, Is.EqualTo (machine));
              Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot));
            });
          }

          {
            IList<IBetweenCycles> betweenCycless =
              ModelDAOHelper.DAOFactory.BetweenCyclesDAO
              .FindAll ();
            Assert.That (betweenCycless, Is.Empty);
            // No gap between the operation cycles
          }

          Assert.Multiple (() => {
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (30));
            Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromSeconds (1)));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (3));
        int i = 1;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 04)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 05)));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Has.Count.EqualTo (1));
          int j = 0;
          IBetweenCycles betweenCycles = betweenCycless[j];
          Assert.Multiple (() => {
            Assert.That (betweenCycles.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (betweenCycles.End, Is.EqualTo (UtcDateTime.From (2012, 06, 04)));
            Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
            Assert.That (betweenCycles.PreviousCycle, Is.EqualTo (operationCycles[1]));
            Assert.That (betweenCycles.NextCycle, Is.EqualTo (operationCycles[2]));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
          Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromDays (2)));
        });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (3));
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycles[i].End, Is.EqualTo (null));
          Assert.That (operationCycles[i].Status, Is.EqualTo (new OperationCycleStatus ()));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Is.Empty);
          // No gap between the cycles
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (3));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (0));
          // only partial cycles => no average cycle time
          Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (null));
        });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (1));
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycles[i].Status, Is.EqualTo (new OperationCycleStatus ()));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));

          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
        });
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

        Assert.Multiple (() => {
          // slot split => cycle split in two cyles (one partial, one full)
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (0));
        });
        CheckSummaries (operationSlot);
        Assert.Multiple (() => {
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
        });
        CheckSummaries (operationSlot2);

        IList<IOperationCycle> operationCycles1 =
          ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllWithOperationSlot (operationSlot);

        IList<IOperationCycle> operationCycles2 =
          ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllWithOperationSlot (operationSlot2);

        Assert.Multiple (() => {
          Assert.That (operationCycles1, Has.Count.EqualTo (1));
          Assert.That (operationCycles2, Has.Count.EqualTo (1));
        });

        IOperationCycle operationCycle1 = operationCycles1[0];
        IOperationCycle operationCycle2 = operationCycles2[0];

        Assert.Multiple (() => {
          // first cycle in first slot and with an estimated end
          Assert.That (operationCycle1.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycle1.End, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycle1.Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycle1.Machine, Is.EqualTo (machine));
          Assert.That (operationCycle1.OperationSlot, Is.EqualTo (operationSlot));

          // second cycle in second slot and with an estimated begin
          Assert.That (operationCycle2.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycle2.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycle2.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycle2.Machine, Is.EqualTo (machine));
          Assert.That (operationCycle2.OperationSlot, Is.EqualTo (operationSlot2));
        });

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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

        Assert.Multiple (() => {
          Assert.That (operationCycleList1, Has.Count.EqualTo (1), "1.not 1");
          Assert.That (operationCycleList2, Has.Count.EqualTo (1), "2.not 1");
        });

        IOperationCycle operationCycle1 = operationCycleList1[0];
        IOperationCycle operationCycle2 = operationCycleList2[0];

        Assert.Multiple (() => {
          Assert.That (operationCycle1.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycle1.End, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycle1.Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycle1.Machine, Is.EqualTo (machine));
          Assert.That (operationCycle1.OperationSlot, Is.EqualTo (operationSlot));

          Assert.That (operationCycle2.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02, 00, 00, 01)));
          Assert.That (operationCycle2.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycle2.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycle2.Machine, Is.EqualTo (machine));
          Assert.That (operationCycle2.OperationSlot, Is.EqualTo (operationSlot2));
        });

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (3));
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycles[i].Begin, Is.EqualTo (operationSlot.BeginDateTime.NullableValue));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));

          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (3));
          // only full cycles (all with estimated begin) => there is an average cycle time
          Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromDays (1)));
        });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
        Assert.Multiple (() => {
          Assert.That (operationCycles, Has.Count.EqualTo (5));
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (2));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (3));
        });
        CheckSummaries (operationSlot);

        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That ((int)operationCycles[i].Status, Is.EqualTo (0));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 04)));
          Assert.That ((int)operationCycles[i].Status, Is.EqualTo (0));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 04)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 05)));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 05)));
          Assert.That (operationCycles[i].End, Is.EqualTo (null));
          Assert.That ((int)operationCycles[i].Status, Is.EqualTo (0));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Has.Count.EqualTo (2));
          int j = 0;
          IBetweenCycles betweenCycles = betweenCycless[j];
          Assert.Multiple (() => {
            Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
            Assert.That (betweenCycles.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (betweenCycles.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (null));
          });
          ++j;
          betweenCycles = betweenCycless[j];
          Assert.Multiple (() => {
            Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
            Assert.That (betweenCycles.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 05)));
            Assert.That (betweenCycles.End, Is.EqualTo (UtcDateTime.From (2012, 06, 05)));
            Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (null));
          });
        }

        // 3 full cycles on a duration of 3 days (from the 2nd to the 5th)
        Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromDays (1)));

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
        Assert.Multiple (() => {
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
        });
        CheckSummaries (operationSlot);

        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycles[i].Begin, Is.EqualTo (operationSlot.BeginDateTime.NullableValue));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Is.Empty);
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

        Assert.Multiple (() => {
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          // first full cycle remains associated to the slot
          // second full cycle not associated to the slot anymore
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
        });
        CheckSummaries (operationSlot);

        // first cycle is full, with an estimated begin set to start of slot (a while back)
        i = 0;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
          Assert.That (operationCycles[i].Full, Is.EqualTo (true));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });

        // second cycle not associated to slot and with estimated begin to end of slot
        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();

        Assert.That (operationCycles, Has.Count.EqualTo (2));
        Assert.That (operationCycles[0].End.HasValue && operationCycles[1].End.HasValue, Is.True);
        IOperationCycle secondCycle = operationCycles[0].End.Value < operationCycles[1].End.Value ?
          operationCycles[1] : operationCycles[0];
        Assert.Multiple (() => {
          Assert.That (secondCycle.OperationSlot, Is.EqualTo (null));
          Assert.That (secondCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated), Is.True);
          Assert.That (secondCycle.Begin, Is.Null);
        });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
        Assert.Multiple (() => {
          Assert.That (operationCycles, Has.Count.EqualTo (1), "slot has not 1 cycle");
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0), "slot has not 0 partial cycle");
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (1), "slot has not 1 total cycle");
        });

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
        Assert.Multiple (() => {
          Assert.That (operationCycles, Has.Count.EqualTo (1), "slot has not 1 cycle");
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0), "slot has not 0 partial cycle");
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (1), "slot has not 1 total cycle");
        });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 01));
        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 02));
        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        IList<IOperationCycle> operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.That (operationCycles, Has.Count.EqualTo (2));
        int i = 1;
        IOperationCycle operationCycle = operationCycles[i];
        Assert.Multiple (() => {
          Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycle.Machine, Is.EqualTo (machine));
        });
        Assert.That (operationCycle.OperationSlot, Is.Null);

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2010, 06, 02)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot1));
            Assert.That (operationCycle.End, Is.EqualTo (operationSlot1.EndDateTime.NullableValue));
            Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          });
          ++i;
          operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (operationSlot2.BeginDateTime.NullableValue));
            Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot2));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot1.PartialCycles, Is.EqualTo (1));
          Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot1.AverageCycleTime, Is.Null);
        CheckSummaries (operationSlot1);
        Assert.Multiple (() => {
          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot2.AverageCycleTime, Is.Null);
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot2));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot1.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot1.AverageCycleTime, Is.Null);
        CheckSummaries (operationSlot1);
        Assert.Multiple (() => {
          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot2.AverageCycleTime, Is.Null);
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
        Assert.That (machine, Is.Not.Null);

        StartCycle (machine,
                    UtcDateTime.From (2012, 06, 19));
        StopCycle (machine,
                   UtcDateTime.From (2011, 06, 19));

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
            Assert.That (operationCycle.End, Is.Null);
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.Null);
          });
        }

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ();
          Assert.That (detectionAnalysisLogs, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            // 1st log is on ExtendOperation
            Assert.That (detectionAnalysisLogs[i].Machine, Is.EqualTo (machine));
            Assert.That (detectionAnalysisLogs[i].Level, Is.EqualTo (LogLevel.WARN));
          });
          ++i;
          Assert.Multiple (() => {
            // 2nd log is on the date/time problem
            Assert.That (detectionAnalysisLogs[i].Machine, Is.EqualTo (machine));
            Assert.That (detectionAnalysisLogs[i].Level, Is.EqualTo (LogLevel.ERROR));
          });
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
        Assert.That (machine, Is.Not.Null);

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 19));
        StopCycle (machine,
                   UtcDateTime.From (2011, 06, 19));

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.That (operationCycle.Begin, Is.Null);
          Assert.Multiple (() => {
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 19)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
          });
          Assert.That (operationCycle.OperationSlot, Is.Null);
        }

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ()
            .OrderBy (x => x.Id)
            .ToList ();
          Assert.That (detectionAnalysisLogs, Has.Count.EqualTo (3));
          int i = 0;
          Assert.Multiple (() => {
            // 1st 2 logs are on ExtendOperation
            Assert.That (detectionAnalysisLogs[i].Machine, Is.EqualTo (machine));
            Assert.That (detectionAnalysisLogs[i].Level, Is.EqualTo (LogLevel.WARN));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (detectionAnalysisLogs[i].Machine, Is.EqualTo (machine));
            Assert.That (detectionAnalysisLogs[i].Level, Is.EqualTo (LogLevel.WARN));
          });
          ++i;
          Assert.Multiple (() => {
            // 3rd log is on the date/time problem
            Assert.That (detectionAnalysisLogs[i].Machine, Is.EqualTo (machine));
            Assert.That (detectionAnalysisLogs[i].Level, Is.EqualTo (LogLevel.ERROR));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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

        Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
        CheckSummaries (operationSlot);
        ((OperationSlot)operationSlot).TotalCycles = 0;

        StopCycle (machine,
                   UtcDateTime.From (2012, 06, 03));

        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          int i = 1;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
        });
        Assert.That (operationSlot.AverageCycleTime, Is.Null);
        CheckSummaries (operationSlot);

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ();
          Assert.That (detectionAnalysisLogs, Has.Count.EqualTo (1));
          int i = 0;
          IDetectionAnalysisLog detectionAnalysisLog = detectionAnalysisLogs[i];
          Assert.Multiple (() => {
            Assert.That (detectionAnalysisLog.Machine, Is.EqualTo (machine));
            Assert.That (detectionAnalysisLog.Level, Is.EqualTo (LogLevel.ERROR));
          });
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
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);
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
            Assert.That (operationCycles, Has.Count.EqualTo (1));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].Begin, Is.EqualTo (T1));
              Assert.That (operationCycles[i].End, Is.EqualTo (T2));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
              Assert.That (operationCycles[i].OffsetDuration, Is.EqualTo ((100 * (45 - 30)) / 30));
            });
          }

          {
            IList<ICycleDurationSummary> summaries =
              new CycleDurationSummaryDAO ()
              .FindAll ();
            Assert.That (summaries, Has.Count.EqualTo (1));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Operation, Is.EqualTo (operationSlot.Operation));
              Assert.That (summaries[i].Component, Is.EqualTo (operationSlot.Component));
              Assert.That (summaries[i].WorkOrder, Is.EqualTo (operationSlot.WorkOrder));
              Assert.That (summaries[i].Offset, Is.EqualTo (50));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
            });
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
            Assert.That (machine, Is.Not.Null);
            IOperation operation1 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.That (operation1, Is.Not.Null);
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
              Assert.That (operationCycles, Has.Count.EqualTo (2));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T1));
                Assert.That (operationCycles[i].End, Is.EqualTo (T2));
                Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
                Assert.That (operationCycles[i].OffsetDuration, Is.EqualTo ((100 * (45 - 30)) / 30));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T2));
                Assert.That (operationCycles[i].End, Is.EqualTo (null));
                Assert.That (operationCycles[i].Status, Is.EqualTo (new OperationCycleStatus ()));
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
                Assert.That (operationCycles[i].OffsetDuration, Is.EqualTo (null));
              });
            }

            {
              IList<ICycleDurationSummary> summaries =
                new CycleDurationSummaryDAO ()
                .FindAll ();
              Assert.That (summaries, Has.Count.EqualTo (1));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (summaries[i].Operation, Is.EqualTo (operationSlot.Operation));
                Assert.That (summaries[i].Component, Is.EqualTo (operationSlot.Component));
                Assert.That (summaries[i].WorkOrder, Is.EqualTo (operationSlot.WorkOrder));
                Assert.That (summaries[i].Offset, Is.EqualTo (50));
                Assert.That (summaries[i].Number, Is.EqualTo (0));
                Assert.That (summaries[i].Partial, Is.EqualTo (1));
              });
            }

            {
              IList<ICycleCountSummary> summaries =
                new CycleCountSummaryDAO ()
                .FindAll ();
              Assert.That (summaries, Has.Count.EqualTo (1));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (summaries[i].Operation, Is.EqualTo (operationSlot.Operation));
                Assert.That (summaries[i].Component, Is.EqualTo (operationSlot.Component));
                Assert.That (summaries[i].WorkOrder, Is.EqualTo (operationSlot.WorkOrder));
                Assert.That (summaries[i].Full, Is.EqualTo (0));
                Assert.That (summaries[i].Partial, Is.EqualTo (2));
              });
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
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);
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
            Assert.That (operationCycles, Has.Count.EqualTo (2));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
              Assert.That (operationCycles[i].Begin, Is.EqualTo (operationSlot.BeginDateTime.NullableValue));
              Assert.That (operationCycles[i].End, Is.EqualTo (T1));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
              Assert.That (operationCycles[i].OffsetDuration, Is.EqualTo (100)); // 1 min instead of 30 s
            });
            i++;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].Begin, Is.EqualTo (T1));
              Assert.That (operationCycles[i].End, Is.EqualTo (T2));
              Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
              Assert.That (operationCycles[i].OffsetDuration, Is.EqualTo ((100 * (45 - 30)) / 30));
            });
          }

          {
            IList<ICycleDurationSummary> summaries =
              new CycleDurationSummaryDAO ()
              .FindAll ();
            Assert.That (summaries, Has.Count.EqualTo (2));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Operation, Is.EqualTo (operationSlot.Operation));
              Assert.That (summaries[i].Component, Is.EqualTo (operationSlot.Component));
              Assert.That (summaries[i].WorkOrder, Is.EqualTo (operationSlot.WorkOrder));
              Assert.That (summaries[i].Offset, Is.EqualTo (100));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
              Assert.That (summaries[i].Partial, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Operation, Is.EqualTo (operationSlot.Operation));
              Assert.That (summaries[i].Component, Is.EqualTo (operationSlot.Component));
              Assert.That (summaries[i].WorkOrder, Is.EqualTo (operationSlot.WorkOrder));
              Assert.That (summaries[i].Offset, Is.EqualTo (50));
              Assert.That (summaries[i].Number, Is.EqualTo (1));
              Assert.That (summaries[i].Partial, Is.EqualTo (0));
            });
          }

          {
            IList<ICycleCountSummary> summaries =
              new CycleCountSummaryDAO ()
              .FindAll ();
            Assert.That (summaries, Has.Count.EqualTo (1));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (summaries[i].Operation, Is.EqualTo (operationSlot.Operation));
              Assert.That (summaries[i].Component, Is.EqualTo (operationSlot.Component));
              Assert.That (summaries[i].WorkOrder, Is.EqualTo (operationSlot.WorkOrder));
              Assert.That (summaries[i].Full, Is.EqualTo (2));
              Assert.That (summaries[i].Partial, Is.EqualTo (0));
            });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (operationSlot.BeginDateTime.NullableValue));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
        });
        Assert.That (operationSlot.AverageCycleTime, Is.Null);
        CheckSummaries (operationSlot);

        {
          IList<IDetectionAnalysisLog> detectionAnalysisLogs =
            ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
            .FindAll ();
          Assert.That (detectionAnalysisLogs, Has.Count.EqualTo (1));
          int i = 0;
          IDetectionAnalysisLog detectionAnalysisLog = detectionAnalysisLogs[i];
          Assert.Multiple (() => {
            Assert.That (detectionAnalysisLog.Machine, Is.EqualTo (machine));
            Assert.That (detectionAnalysisLog.Level, Is.EqualTo (LogLevel.ERROR));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (3));
        int i = 1;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 04)));
          Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 05)));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Has.Count.EqualTo (1));
          int j = 0;
          IBetweenCycles betweenCycles = betweenCycless[j];
          Assert.Multiple (() => {
            Assert.That (betweenCycles.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (betweenCycles.End, Is.EqualTo (UtcDateTime.From (2012, 06, 04)));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
          Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromDays (2)));
        });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (2));
        int i = 1;
        IOperationCycle operationCycle = operationCycles[i];
        Assert.Multiple (() => {
          Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
          Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
          Assert.That (operationCycle.Machine, Is.EqualTo (machine));
        });
        Assert.That (operationCycle.OperationSlot, Is.Null);

        {
          IList<IBetweenCycles> betweenCycless =
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindAll ();
          Assert.That (betweenCycless, Is.Empty);
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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

          Assert.That (operationCycles, Has.Count.EqualTo (2));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2010, 06, 02)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot1));
            Assert.That (operationCycle.End, Is.EqualTo (operationSlot1.EndDateTime.NullableValue));
            Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          });
          ++i;
          operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
            Assert.That (operationCycle.Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot2));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot1.PartialCycles, Is.EqualTo (1));
          Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot1.AverageCycleTime, Is.Null);
        CheckSummaries (operationSlot1);
        Assert.Multiple (() => {
          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot2.AverageCycleTime, Is.Null);
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation, Is.Not.Null);
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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2010, 06, 02)));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot2));
            Assert.That (operationCycle.Status, Is.EqualTo (new OperationCycleStatus ()));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot1.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot1.AverageCycleTime, Is.Null);
        CheckSummaries (operationSlot1);
        Assert.Multiple (() => {
          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot2.AverageCycleTime, Is.Null);
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
            Assert.That (machine, Is.Not.Null);
            IOperation operation =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.That (operation, Is.Not.Null);
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
              Assert.That (operationCycles, Has.Count.EqualTo (1));
              int i = 0;
              IOperationCycle operationCycle = operationCycles[i];
              Assert.Multiple (() => {
                Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2010, 06, 02)));
                Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
                Assert.That (operationCycle.Machine, Is.EqualTo (machine));
                Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot2));
                Assert.That (operationCycle.Status, Is.EqualTo (new OperationCycleStatus ()));
              });
            }

            Assert.Multiple (() => {
              Assert.That (operationSlot1.PartialCycles, Is.EqualTo (0));
              Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
            });
            Assert.That (operationSlot1.AverageCycleTime, Is.Null);
            CheckSummaries (operationSlot1);
            Assert.Multiple (() => {
              Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
              Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
            });
            Assert.That (operationSlot2.AverageCycleTime, Is.Null);
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          IOperationCycle operationCycle = operationCycles[i];
          Assert.Multiple (() => {
            Assert.That (operationCycle.Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
            Assert.That (operationCycle.End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycle.Machine, Is.EqualTo (machine));
            Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlot2));
          });
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot1.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot1.AverageCycleTime, Is.Null);
        CheckSummaries (operationSlot1);
        Assert.Multiple (() => {
          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
        });
        Assert.That (operationSlot2.AverageCycleTime, Is.Null);
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
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);

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

          Assert.That (operationCycles, Has.Count.EqualTo (3));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T1));
            Assert.That (operationCycles[i].End, Is.EqualTo (T2));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T2));
            Assert.That (operationCycles[i].End, Is.EqualTo (T3));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T3));
            Assert.That (operationCycles[i].End, Is.EqualTo (T4));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));


            Assert.That (operationSlot.PartialCycles, Is.EqualTo (3));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (0));
          });

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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

        Assert.That (operationCycles, Has.Count.EqualTo (3));
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (T1));
          Assert.That (operationCycles[i].End, Is.EqualTo (T2));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (T2));
          Assert.That (operationCycles[i].End, Is.EqualTo (T3));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (T3));
          Assert.That (operationCycles[i].End, Is.EqualTo (T4));
          Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));


          Assert.That (operationSlot.PartialCycles, Is.EqualTo (3));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (0));
        });

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
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);

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
            (operationCycles[0], operationCycles[1]) = (operationCycles[1], operationCycles[0]);
          }

          if (operationCycles[1].Id > operationCycles[2].Id) {
            (operationCycles[1], operationCycles[2]) = (operationCycles[2], operationCycles[1]);
          }

          if (operationCycles[0].Id > operationCycles[1].Id) {
            (operationCycles[0], operationCycles[1]) = (operationCycles[1], operationCycles[0]);
          }
          // end reorder

          Assert.That (operationCycles, Has.Count.EqualTo (3));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].End, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (null));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].End, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (null));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].End, Is.EqualTo (null));
            Assert.That (operationCycles[i].Status, Is.EqualTo (new OperationCycleStatus ()));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (null));
          });

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
            (operationCycles[0], operationCycles[1]) = (operationCycles[1], operationCycles[0]);
          }

          if (operationCycles[1].Id > operationCycles[2].Id) {
            (operationCycles[1], operationCycles[2]) = (operationCycles[2], operationCycles[1]);
          }

          if (operationCycles[0].Id > operationCycles[1].Id) {
            (operationCycles[0], operationCycles[1]) = (operationCycles[1], operationCycles[0]);
          }
          // end reorder

          Assert.That (operationCycles, Has.Count.EqualTo (3));
          i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].End, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].End, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (sameDate));
            Assert.That (operationCycles[i].End, Is.EqualTo (null));
            Assert.That (operationCycles[i].Status, Is.EqualTo (new OperationCycleStatus ()));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));

            Assert.That (operationSlot.PartialCycles, Is.EqualTo (3));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (0));
            // only partial cycles => no average cycle time
            Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (null));
          });
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
          Assert.That (machine, Is.Not.Null);
          IMachineModule machineModule =
            daoFactory.MachineModuleDAO.FindById (1);
          Assert.That (machineModule, Is.Not.Null);
          IOperation operation1 =
            daoFactory.OperationDAO.FindById (1);
          Assert.That (operation1, Is.Not.Null);
          ISequence sequence1 =
            daoFactory.SequenceDAO.FindById (1);
          Assert.That (sequence1, Is.Not.Null);
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

            Assert.That (operationSlots, Has.Count.EqualTo (1), "Number of operation slots after 1.");
            Assert.That (operationSlots[0].TotalCycles, Is.EqualTo (1), "Number of cycles is not 1.");

            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAllWithOperationSlot (operationSlots[0]);

            Assert.That (operationCycles, Has.Count.EqualTo (1));
            Assert.Multiple (() => {
              Assert.That (operationCycles[0].Begin, Is.EqualTo (T0));
              Assert.That (operationCycles[0].End, Is.EqualTo (T3));
            });

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
            Assert.Multiple (() => {
              Assert.That (total, Is.EqualTo (0));
              Assert.That (partial, Is.EqualTo (0));
            });
          }
          else {
            Assert.Multiple (() => {
              Assert.That (total, Is.EqualTo (summary.Full));
              Assert.That (partial, Is.EqualTo (summary.Partial));
            });
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
        Assert.That (operationSlot.TotalCycles, Is.LessThanOrEqualTo (counted1));
        Assert.That (operationSlot.TotalCycles, Is.LessThanOrEqualTo (corrected1));
        Assert.That (operationSlot.TotalCycles, Is.LessThanOrEqualTo (counted2));
        Assert.That (operationSlot.TotalCycles, Is.LessThanOrEqualTo (corrected2));
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (1));
        IOperationCycle operationCycle = operationCycles[0];
        Assert.Multiple (() => {
          Assert.That (operationCycle.Begin, Is.EqualTo (T1), "begin cycle1 not T1");
          Assert.That (operationCycle.End, Is.EqualTo (T3), "end cycle1 not T2");
        });
        Assert.That (!operationCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated)
                       && !operationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated), Is.True);

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
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);

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

          Assert.That (operationCycles, Has.Count.EqualTo (1));
          IOperationCycle operationCycle = operationCycles[0];

          {
            IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAll (machine);
            Assert.That (operationSlots, Has.Count.EqualTo (2));
            Assert.Multiple (() => {
              Assert.That (operationSlots[1].EndDateTime.Value, Is.EqualTo (T2.AddSeconds (2)));
              Assert.That (operationCycle.Begin, Is.Not.Null);
              Assert.That (operationCycle.End, Is.Not.Null);
            });
            Assert.Multiple (() => {
              Assert.That (operationCycle.Begin, Is.EqualTo (T1));
              Assert.That (operationCycle.End, Is.EqualTo (T2.AddSeconds (2)));
              Assert.That (operationCycle.OperationSlot, Is.EqualTo (operationSlots[1]));
              Assert.That (operationSlots[0].TotalCycles, Is.EqualTo (0));
              Assert.That (operationSlots[1].TotalCycles, Is.EqualTo (1));
            });
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
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);
          operation1.MachiningDuration = TimeSpan.FromSeconds (30);
          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.That (operation2, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          Assert.Multiple (() => {
            Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot));
            Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          });

          IOperationMachineAssociation association =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T3, null, true);
          association.Operation = operation2;
          // association.End = null;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          association.Apply ();

          IList<IOperationSlot> operationSlots =
            ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();

          Assert.That (operationSlots, Has.Count.EqualTo (2), "Not 2 operation slots");

          foreach (IOperationSlot opSlot in operationSlots) {
            (opSlot as OperationSlot)
              .Consolidate (null);
          }

          ModelDAOHelper.DAOFactory.FlushData ();
          DAOFactory.EmptyAccumulators ();

          for (int i = 0; i < 2; i++) {
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].TotalCycles, Is.EqualTo (1), "not 1 total cycle");
              Assert.That (operationSlots[i].PartialCycles, Is.EqualTo (0), "not 0 partial cycle");
            });
          }

          operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          Assert.Multiple (() => {
            Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlots[0]));
            Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlots[1]));
          });

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        operation1.MachiningDuration = TimeSpan.FromSeconds (30);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (2));
        Assert.Multiple (() => {
          Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot));
          Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
        });



        IOperationMachineAssociation association =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, T4, null, true);
        association.Operation = operation2;
        // association.End = null;
        ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
        association.Apply (); // will also consolidate the slots

        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();

        Assert.That (operationSlots, Has.Count.EqualTo (2), "Not 2 operation slots");

        IOperationSlot operationSlot1 = operationSlots[0];
        IOperationSlot operationSlot2 = operationSlots[1];

        if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
          (operationSlot2, operationSlot1) = (operationSlot1, operationSlot2);
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot1.BeginDateTime.Value, Is.EqualTo (T0));
          Assert.That (operationSlot1.EndDateTime.HasValue, Is.True);
          Assert.That (operationSlot1.EndDateTime.Value, Is.EqualTo (T4));
          Assert.That (operationSlot2.BeginDateTime.Value, Is.EqualTo (T4));
        });

        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.That (operationCycles, Has.Count.EqualTo (2));
        Assert.Multiple (() => {
          Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot1));
          Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot1));

          Assert.That (operationSlot1.TotalCycles, Is.EqualTo (1), "not 1 total cycle");
          Assert.That (operationSlot1.PartialCycles, Is.EqualTo (1), "not 1 partial cycle");

          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (0), "not 0 total cycle");
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0), "not 0 partial cycle");
        });

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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        operation1.MachiningDuration = TimeSpan.FromSeconds (30);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (2));
        Assert.Multiple (() => {
          Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot));
          Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
        });

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

        Assert.That (operationSlots, Has.Count.EqualTo (2), "Not 2 operation slots");

        IOperationSlot operationSlot1 = operationSlots[0];
        IOperationSlot operationSlot2 = operationSlots[1];

        if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
          (operationSlot2, operationSlot1) = (operationSlot1, operationSlot2);
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot1.BeginDateTime.Value, Is.EqualTo (T0));
          Assert.That (operationSlot1.EndDateTime.HasValue, Is.True);
          Assert.That (operationSlot1.EndDateTime.Value, Is.EqualTo (T4));
          Assert.That (operationSlot2.BeginDateTime.Value, Is.EqualTo (T4));
        });

        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.That (operationCycles, Has.Count.EqualTo (3), "not 3 cycles");
        Assert.Multiple (() => {
          Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot1));
          Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot1));
          Assert.That (operationCycles[2].OperationSlot, Is.EqualTo (operationSlot2));

          Assert.That (operationSlot1.TotalCycles, Is.EqualTo (1), "not 1 total cycle");
          Assert.That (operationSlot1.PartialCycles, Is.EqualTo (1), "not 1 partial cycle");

          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1), "not 1 total cycle");
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0), "not 0 partial cycle");

          Assert.That (operationCycles[1].End.HasValue, Is.True);
          Assert.That (operationCycles[1].End, Is.EqualTo (T4));
          Assert.That (operationCycles[1].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));

          Assert.That (operationCycles[2].Begin, Is.EqualTo (T4));
          Assert.That (operationCycles[2].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
          Assert.That (operationCycles[2].End, Is.EqualTo (T5));
        });

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
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);
          operation1.MachiningDuration = TimeSpan.FromSeconds (30);
          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.That (operation2, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          Assert.Multiple (() => {
            Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot));
            Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          });

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

          Assert.That (operationSlots, Has.Count.EqualTo (2), "Not 2 operation slots");

          IOperationSlot operationSlot1 = operationSlots[0];
          IOperationSlot operationSlot2 = operationSlots[1];

          if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
            (operationSlot2, operationSlot1) = (operationSlot1, operationSlot2);
          }

          Assert.Multiple (() => {
            Assert.That (operationSlot1.BeginDateTime.Value, Is.EqualTo (T0));
            Assert.That (operationSlot1.EndDateTime.HasValue, Is.True);
          });
          Assert.Multiple (() => {
            Assert.That (operationSlot1.EndDateTime.Value, Is.EqualTo (T4));
            Assert.That (operationSlot2.BeginDateTime.Value, Is.EqualTo (T4));
          });

          operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (2), "not 2 cycles");
          Assert.Multiple (() => {
            Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot1));
            Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot2));

            Assert.That (operationSlot1.TotalCycles, Is.EqualTo (1), "not 1 total cycle");
            Assert.That (operationSlot1.PartialCycles, Is.EqualTo (0), "not 0 partial cycle");

            Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1), "not 1 total cycle");
            Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0), "not 0 partial cycle");

            Assert.That (operationCycles[1].Begin, Is.EqualTo (T3));
          });
          Assert.That (operationCycles[1].End.HasValue, Is.True);
          Assert.Multiple (() => {
            Assert.That (operationCycles[1].End, Is.EqualTo (T5));
            Assert.That (operationCycles[1].Status, Is.EqualTo (new OperationCycleStatus ()));
          });


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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
        operation1.MachiningDuration = TimeSpan.FromSeconds (30);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation2, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (2));
        Assert.Multiple (() => {
          Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot));
          Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
        });

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

        Assert.That (operationSlots, Has.Count.EqualTo (2), "Not 2 operation slots");

        IOperationSlot operationSlot1 = operationSlots[0];
        IOperationSlot operationSlot2 = operationSlots[1];

        if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
          (operationSlot2, operationSlot1) = (operationSlot1, operationSlot2);
        }

        Assert.Multiple (() => {
          Assert.That (operationSlot1.BeginDateTime.Value, Is.EqualTo (T0));
          Assert.That (operationSlot1.EndDateTime.HasValue, Is.True);
          Assert.That (operationSlot1.EndDateTime.Value, Is.EqualTo (T4));
          Assert.That (operationSlot2.BeginDateTime.Value, Is.EqualTo (T4));
        });

        operationCycles =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAll ();
        Assert.That (operationCycles, Has.Count.EqualTo (3), "not 3 cycles");
        Assert.Multiple (() => {
          Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot1));
          Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot1));
          Assert.That (operationCycles[2].OperationSlot, Is.EqualTo (operationSlot2));

          Assert.That (operationSlot1.TotalCycles, Is.EqualTo (1), "not 1 total cycle");
          Assert.That (operationSlot1.PartialCycles, Is.EqualTo (1), "not 1 partial cycle");

          Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1), "not 1 total cycle");
          Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0), "not 0 partial cycle");

          Assert.That (operationCycles[0].Begin, Is.EqualTo (T1));
          Assert.That (operationCycles[0].End.HasValue, Is.True);
          Assert.That (operationCycles[0].End, Is.EqualTo (T2));
          Assert.That (operationCycles[0].Status, Is.EqualTo (new OperationCycleStatus ()));
          Assert.That (operationCycles[1].Begin, Is.EqualTo (T3));
          Assert.That (operationCycles[1].End.HasValue, Is.True);
          Assert.That (operationCycles[1].End, Is.EqualTo (T4));
          Assert.That (operationCycles[1].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          Assert.That (operationCycles[2].Begin, Is.EqualTo (T4));
          Assert.That (operationCycles[2].End.HasValue, Is.True);
          Assert.That (operationCycles[2].End.Value, Is.EqualTo (T5));
          Assert.That (operationCycles[2].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
        });
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
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);
          operation1.MachiningDuration = TimeSpan.FromSeconds (30);
          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.That (operation2, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          Assert.Multiple (() => {
            Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot));
            Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          });

          {
            IList<IOperationSlot> operationSlots =
              ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll ();
            Assert.That (operationSlots, Has.Count.EqualTo (1));
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
            Assert.That (operationSlots, Has.Count.EqualTo (2), "Not 2 operation slots");
            IOperationSlot operationSlot1 = operationSlots[0];
            IOperationSlot operationSlot2 = operationSlots[1];

            if (operationSlot1.BeginDateTime > operationSlot2.BeginDateTime) {
              (operationSlot2, operationSlot1) = (operationSlot1, operationSlot2);
            }

            Assert.Multiple (() => {
              Assert.That (operationSlot1.BeginDateTime.Value, Is.EqualTo (T0));
              Assert.That (operationSlot1.EndDateTime.HasValue, Is.True);
            });
            Assert.Multiple (() => {
              Assert.That (operationSlot1.EndDateTime.Value, Is.EqualTo (T3));
              Assert.That (operationSlot2.BeginDateTime.Value, Is.EqualTo (T3));
            });

            operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.That (operationCycles, Has.Count.EqualTo (2), "not 2 cycles");
            Assert.Multiple (() => {
              Assert.That (operationCycles[0].OperationSlot, Is.EqualTo (operationSlot1));
              Assert.That (operationCycles[1].OperationSlot, Is.EqualTo (operationSlot2));
            });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);


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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].End, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot.BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].OperationSlot.EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].End, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot.BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].OperationSlot.EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
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
          Assert.That (operationSlots, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (3)));
          });
          Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder));
          });
        }
        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].End, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot.BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].OperationSlot.EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);
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
          Assert.That (operationSlots, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (3)));
          });
          Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder));
          });
        }
        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].End, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot.BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].OperationSlot.EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].End, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot.BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].OperationSlot.EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].End, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot.BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].OperationSlot.EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        Assert.That (operation1, Is.Not.Null);

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
          Assert.That (operationSlots, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (0).AddSeconds (1)));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
          });
          CheckSummaries (operationSlots[i]);
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (4)));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
          });
          CheckSummaries (operationSlots[i]);
        }

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].End, Is.EqualTo (T (0).AddSeconds (1)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot.BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationCycles[i].OperationSlot.EndDateTime.Value, Is.EqualTo (T (0).AddSeconds (1)));
            Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (null));
            Assert.That (operationCycles[i].End, Is.EqualTo (T (5)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (null));
            Assert.That (operationCycles[i].Status, Is.EqualTo (new OperationCycleStatus ()));
          });
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
        Assert.That (machine, Is.Not.Null);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.That (operation1, Is.Not.Null);

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
        Assert.That (operationCycles, Has.Count.EqualTo (2));
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationCycles[i].Begin, Is.EqualTo (T (1)));
          Assert.That (operationCycles[i].End, Is.EqualTo (T (3)));
          Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));

          Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
          Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromSeconds (3)));
        });
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
          Assert.That (machine, Is.Not.Null);
          machine.PalletChangingDuration = TimeSpan.FromSeconds (2);

          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);

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
            Assert.That (operationCycles, Has.Count.EqualTo (2));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].End, Is.EqualTo (T (1)));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].Begin, Is.EqualTo (T (2)));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
            });

            {
              IList<IBetweenCycles> betweenCycless =
                ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindAll ();
              Assert.That (betweenCycless, Has.Count.EqualTo (1));
              int j = 0;
              IBetweenCycles betweenCycles = betweenCycless[j];
              Assert.Multiple (() => {
                Assert.That (betweenCycles.Begin, Is.EqualTo (T (1)));
                Assert.That (betweenCycles.End, Is.EqualTo (T (2)));
                Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
                Assert.That (betweenCycles.PreviousCycle, Is.EqualTo (operationCycles[0]));
                Assert.That (betweenCycles.NextCycle, Is.EqualTo (operationCycles[1]));
                Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (50));
              });
            }
          }

          Assert.Multiple (() => {
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
          });
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
          Assert.That (machine, Is.Not.Null);

          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);
          operation1.UnloadingDuration = TimeSpan.FromSeconds (3);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.That (operation2, Is.Not.Null);
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

          Assert.That (machine.PalletChangingDuration, Is.EqualTo (null));
          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.That (operationCycles, Has.Count.EqualTo (2));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].End, Is.EqualTo (T (1)));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].Begin, Is.EqualTo (T (21)));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot2));
            });

            {
              IList<IBetweenCycles> betweenCycless =
                ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindAll ();
              Assert.That (betweenCycless, Has.Count.EqualTo (1));
              int j = 0;
              IBetweenCycles betweenCycles = betweenCycless[j];
              Assert.Multiple (() => {
                Assert.That (betweenCycles.Begin, Is.EqualTo (T (1)));
                Assert.That (betweenCycles.End, Is.EqualTo (T (21)));
                Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
                Assert.That (betweenCycles.PreviousCycle, Is.EqualTo (operationCycles[0]));
                Assert.That (betweenCycles.NextCycle, Is.EqualTo (operationCycles[1]));
              });
              Assert.Multiple (() => {
                Assert.That (betweenCycles.NextCycle.OperationSlot, Is.EqualTo (operationSlot2));
                Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (200)); // 100 * 20 / 10
              });
            }
          }

          Assert.Multiple (() => {
            Assert.That (operationSlot1.PartialCycles, Is.EqualTo (0));
            Assert.That (operationSlot1.TotalCycles, Is.EqualTo (1));
          });
          CheckSummaries (operationSlot1);
          Assert.Multiple (() => {
            Assert.That (operationSlot2.PartialCycles, Is.EqualTo (1));
            Assert.That (operationSlot2.TotalCycles, Is.EqualTo (0));
          });
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
          Assert.That (machine, Is.Not.Null);

          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);
          operation1.UnloadingDuration = TimeSpan.FromSeconds (10);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

          IOperation operation2 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          Assert.That (operation2, Is.Not.Null);
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

          Assert.That (machine.PalletChangingDuration, Is.EqualTo (null));
          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindAll ();
            Assert.That (operationCycles, Has.Count.EqualTo (2));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].End, Is.EqualTo (T (1)));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].Begin, Is.EqualTo (T (21)));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
            });

            {
              IList<IBetweenCycles> betweenCycless =
                ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindAll ();
              Assert.That (betweenCycless, Has.Count.EqualTo (1));
              int j = 0;
              IBetweenCycles betweenCycles = betweenCycless[j];
              Assert.Multiple (() => {
                Assert.That (betweenCycles.Begin, Is.EqualTo (T (1)));
                Assert.That (betweenCycles.End, Is.EqualTo (T (21)));
                Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
                Assert.That (betweenCycles.PreviousCycle, Is.EqualTo (operationCycles[0]));
                Assert.That (betweenCycles.NextCycle, Is.EqualTo (operationCycles[1]));
              });
              Assert.Multiple (() => {
                Assert.That (betweenCycles.NextCycle.OperationSlot, Is.EqualTo (operationSlot));
                Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (200)); // 100 * 20 / 10
              });
            }
          }

          Assert.Multiple (() => {
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
          });
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
            Assert.That (operationCycles, Has.Count.EqualTo (2));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].End, Is.EqualTo (T (1)));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation2));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationCycles[i].Begin, Is.EqualTo (T (21)));
              Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
              Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation2));
            });

            {
              IList<IBetweenCycles> betweenCycless =
                ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindAll ();
              Assert.That (betweenCycless, Has.Count.EqualTo (1));
              int j = 0;
              IBetweenCycles betweenCycles = betweenCycless[j];
              Assert.Multiple (() => {
                Assert.That (betweenCycles.Begin, Is.EqualTo (T (1)));
                Assert.That (betweenCycles.End, Is.EqualTo (T (21)));
                Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
                Assert.That (betweenCycles.PreviousCycle, Is.EqualTo (operationCycles[0]));
                Assert.That (betweenCycles.NextCycle, Is.EqualTo (operationCycles[1]));
                Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (100)); // 100 * 20 / 20
              });
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
            Assert.That (machine, Is.Not.Null);

            IOperation operation1 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.That (operation1, Is.Not.Null);
            operation1.UnloadingDuration = TimeSpan.FromSeconds (10);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

            IOperation operation2 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (2);
            Assert.That (operation2, Is.Not.Null);
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

            Assert.That (machine.PalletChangingDuration, Is.EqualTo (null));
            {
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.That (operationCycles, Has.Count.EqualTo (2));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].End, Is.EqualTo (T (1)));
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T (21)));
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
              });

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.That (betweenCycless, Has.Count.EqualTo (1));
                int j = 0;
                IBetweenCycles betweenCycles = betweenCycless[j];
                Assert.Multiple (() => {
                  Assert.That (betweenCycles.Begin, Is.EqualTo (T (1)));
                  Assert.That (betweenCycles.End, Is.EqualTo (T (21)));
                  Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
                  Assert.That (betweenCycles.PreviousCycle, Is.EqualTo (operationCycles[0]));
                  Assert.That (betweenCycles.NextCycle, Is.EqualTo (operationCycles[1]));
                });
                Assert.Multiple (() => {
                  Assert.That (betweenCycles.NextCycle.OperationSlot, Is.EqualTo (operationSlot));
                  Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (200)); // 100 * 20 / 10
                });
              }
            }

            Assert.Multiple (() => {
              Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
              Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
            });
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
              Assert.That (operationCycles, Has.Count.EqualTo (3));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].End, Is.EqualTo (T (1)));
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T (21)));
                Assert.That (operationCycles[i].End, Is.EqualTo (split));
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
              });
              Assert.That (operationCycles[i].Full, Is.False);
              ++i;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (split));
                Assert.That (operationCycles[i].End, Is.EqualTo (stop2));
                Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation2));
              });
              Assert.That (operationCycles[i].Full, Is.True);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.That (betweenCycless, Has.Count.EqualTo (1));
                int j = 0;
                IBetweenCycles betweenCycles = betweenCycless[j];
                Assert.Multiple (() => {
                  Assert.That (betweenCycles.Begin, Is.EqualTo (T (1)));
                  Assert.That (betweenCycles.End, Is.EqualTo (T (21)));
                  Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
                  Assert.That (betweenCycles.PreviousCycle, Is.EqualTo (operationCycles[0]));
                  Assert.That (betweenCycles.NextCycle, Is.EqualTo (operationCycles[1]));
                  Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (200));
                });
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
              Assert.That (operationCycles, Has.Count.EqualTo (2));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].End, Is.EqualTo (T (1)));
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
              });
              CheckSummaries (operationCycles[i].OperationSlot);
              ++i;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T (21)));
                Assert.That (operationCycles[i].End, Is.EqualTo (stop2));
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
              });
              Assert.That (operationCycles[i].Full, Is.True);
              CheckSummaries (operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.That (betweenCycless, Has.Count.EqualTo (1));
                int j = 0;
                IBetweenCycles betweenCycles = betweenCycless[j];
                Assert.Multiple (() => {
                  Assert.That (betweenCycles.Begin, Is.EqualTo (T (1)));
                  Assert.That (betweenCycles.End, Is.EqualTo (T (21)));
                  Assert.That (betweenCycles.Machine, Is.EqualTo (machine));
                  Assert.That (betweenCycles.PreviousCycle, Is.EqualTo (operationCycles[0]));
                  Assert.That (betweenCycles.NextCycle, Is.EqualTo (operationCycles[1]));
                  Assert.That (betweenCycles.OffsetDuration, Is.EqualTo (200));
                });
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
            Assert.That (machine, Is.Not.Null);

            IOperation operation1 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.That (operation1, Is.Not.Null);
            operation1.UnloadingDuration = TimeSpan.FromSeconds (10);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

            IOperation operation2 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (2);
            Assert.That (operation2, Is.Not.Null);
            operation2.LoadingDuration = TimeSpan.FromSeconds (20);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation2);

            var component1 = ModelDAOHelper.DAOFactory.ComponentDAO
              .FindById (1);
            Assert.That (component1, Is.Not.Null);

            var workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO
              .FindById (1);
            Assert.That (workOrder1, Is.Not.Null);

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
              Assert.That (operationCycles, Has.Count.EqualTo (2));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T (1)));
                Assert.That (operationCycles[i].End, Is.EqualTo (T (3)));
              });
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].HasRealBegin (), Is.True);
                Assert.That (!operationCycles[i].HasRealEnd (), Is.True);
              });
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot1));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T (3)));
                Assert.That (operationCycles[i].End, Is.EqualTo (T (4)));
              });
              Assert.Multiple (() => {
                Assert.That (!operationCycles[i].HasRealBegin (), Is.True);
                Assert.That (operationCycles[i].HasRealEnd (), Is.True);
              });
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot2));
              });

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.That (betweenCycless, Is.Empty);
              }
            }

            Assert.Multiple (() => {
              Assert.That (operationSlot1.PartialCycles, Is.EqualTo (1));
              Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
              Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
              Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
            });
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
              Assert.That (operationSlots, Has.Count.EqualTo (2));
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.That (operationCycles, Has.Count.EqualTo (1));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T (1)));
                Assert.That (operationCycles[i].End, Is.EqualTo (T (4)));
              });
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].HasRealBegin (), Is.True);
                Assert.That (operationCycles[i].HasRealEnd (), Is.True);
              });
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation2));
              });
              CheckSummaries (operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.That (betweenCycless, Is.Empty);
              }

              Assert.Multiple (() => {
                Assert.That (operationSlot1.PartialCycles, Is.EqualTo (0));
                Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
                Assert.That (operationCycles[i].OperationSlot.PartialCycles, Is.EqualTo (0));
                Assert.That (operationCycles[i].OperationSlot.TotalCycles, Is.EqualTo (1));
              });
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
    private void TestNewShift ()
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
            Assert.That (machine, Is.Not.Null);

            IOperation operation1 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            Assert.That (operation1, Is.Not.Null);
            operation1.UnloadingDuration = TimeSpan.FromSeconds (10);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation1);

            var component1 = ModelDAOHelper.DAOFactory.ComponentDAO
              .FindById (1);
            Assert.That (component1, Is.Not.Null);

            var workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO
              .FindById (1);
            Assert.That (workOrder1, Is.Not.Null);

            var shift1 = ModelDAOHelper.DAOFactory.ShiftDAO
              .FindById (1);
            Assert.That (shift1, Is.Not.Null);

            var shift2 = ModelDAOHelper.DAOFactory.ShiftDAO
              .FindById (2);
            Assert.That (shift2, Is.Not.Null);

            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
              .FindById (1);
            Assert.That (attended, Is.Not.Null);

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
              Assert.That (operationCycles, Has.Count.EqualTo (2));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T (1)));
                Assert.That (operationCycles[i].End, Is.EqualTo (T (2)));
              });
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].HasRealBegin (), Is.True);
                Assert.That (!operationCycles[i].HasRealEnd (), Is.True);
              });
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot1));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (null));
                Assert.That (operationCycles[i].End, Is.EqualTo (T (4)));
              });
              Assert.Multiple (() => {
                Assert.That (!operationCycles[i].HasRealBegin (), Is.True);
                Assert.That (operationCycles[i].HasRealEnd (), Is.True);
              });
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (null));
              });

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.That (betweenCycless, Is.Empty);
              }
            }

            Assert.Multiple (() => {
              Assert.That (operationSlot1.PartialCycles, Is.EqualTo (1));
              Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
              Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
              Assert.That (operationSlot2.TotalCycles, Is.EqualTo (0));
            });
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
              Assert.That (operationSlots, Has.Count.EqualTo (2));
              IList<IOperationCycle> operationCycles =
                ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindAll ();
              Assert.That (operationCycles, Has.Count.EqualTo (1));
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (operationCycles[i].Begin, Is.EqualTo (T (1)));
                Assert.That (operationCycles[i].End, Is.EqualTo (T (4)));
                Assert.That (operationCycles[i].HasRealBegin (), Is.True);
                Assert.That (operationCycles[i].HasRealEnd (), Is.True);
                Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
                Assert.That (operationCycles[i].OperationSlot.Operation, Is.EqualTo (operation1));
              });
              CheckSummaries (operationCycles[i].OperationSlot);

              {
                IList<IBetweenCycles> betweenCycless =
                  ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                  .FindAll ();
                Assert.That (betweenCycless, Is.Empty);
              }

              Assert.Multiple (() => {
                Assert.That (operationSlot1.PartialCycles, Is.EqualTo (0));
                Assert.That (operationSlot1.TotalCycles, Is.EqualTo (0));
                Assert.That (operationCycles[i].OperationSlot.PartialCycles, Is.EqualTo (0));
                Assert.That (operationCycles[i].OperationSlot.TotalCycles, Is.EqualTo (1));
              });
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
