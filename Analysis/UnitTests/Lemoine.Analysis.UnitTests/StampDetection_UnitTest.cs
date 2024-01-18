// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Analysis.Detection;
using Lemoine.Database.Persistent;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Business.Config;
using Lemoine.Plugin.IntermediateWorkPieceSummary;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// Unit tests for the class StampDetection
  /// </summary>
  [TestFixture]
  public class StampDetection_UnitTest
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (StampDetection_UnitTest).FullName);

    /// <summary>
    /// Test an analysis process in case of sequence and IsoFileEnd detection
    /// </summary>
    [Test]
    public void TestAnalysisSequenceDetection()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        try {
          // Reference data
          IMachineModule machineModule =
            daoFactory.MachineModuleDAO
            .FindById (1);
          Assert.NotNull (machineModule);
          ISequence sequence1 =
            daoFactory.SequenceDAO
            .FindById (1);
          Assert.NotNull (sequence1);
          ISequence sequence2 =
            daoFactory.SequenceDAO
            .FindById (2);
          Assert.NotNull (sequence2);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          Stamp stamp0 = new Stamp ();
          stamp0.IsoFileEnd = true;
          daoFactory.StampDAO.MakePersistent (stamp0);
          
          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          
          // 1. Sequence 1 detection at 10:05:00
          stampDetection.StartStamp (stamp1, null,
                                     UtcDateTime.From (2008, 01, 16, 10, 05, 00));
          {
            // Check the auto-sequence
            IList<AutoSequence> autoSequences =
              NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            int i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
          }
          
          // 5. IsoFileEnd detection at 10:19:59
          stampDetection.StartStamp (stamp0, null,
                                     UtcDateTime.From (2008, 01, 16, 10, 19, 59));
          {
            IList<AutoSequence> autoSequences =
              NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 5.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 59)));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
          }

          // 8. Sequence 2 detection at 10:25:01
          stampDetection.StartStamp (stamp2, null,
                                     UtcDateTime.From (2008, 01, 16, 10, 25, 01));
          {
            IList<AutoSequence> autoSequences =
              NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (2), "Number of auto-sequences after 8.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 59)));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 01)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence2));
            });
          }

          // 9. Sequence 1 detection at 10:28:00
          stampDetection.StartStamp (stamp1, null,
                                     UtcDateTime.From (2008, 01, 16, 10, 28, 00));
          {
            IList<AutoSequence> autoSequences =
              NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (3), "Number of auto-sequences after 9.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 59)));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 01)));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence2));
            });
            ++i;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test an stamp => auto-sequence => sequence slot => operation slot process
    /// </summary>
    [Test]
    public void TestAnalysis()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          Operation operation3 = session.Get<Operation> (11003);
          Assert.NotNull (operation3);
          Operation operation4 = session.Get<Operation> (11004);
          Assert.NotNull (operation4);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);
          Sequence sequence2 = session.Get<Sequence> (2);
          Assert.NotNull (sequence2);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);

          // Config
          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int) OperationSlotSplitOption.None);
          
          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation3,
                                    null, null, null, null, null, null,
                                    new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00),
                                                          UtcDateTime.From (2008, 01, 16, 10, 12, 00)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
            UpdateIntermediateWorkPieceSummary (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation4,
                                    null, null, null, null, null, null,
                                    new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 10, 12, 00)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
            UpdateIntermediateWorkPieceSummary (existingOperationSlot);
          }

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // 1. Sequence 1 detection at 10:05:00
          {
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2008, 01, 16, 10, 05, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 05, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 10, 08, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 08, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 10, 10, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 15, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 10, 19, 00));
          }
          {
            // Check there is operation1 between 10:05:00 and 10:10:00
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo ((5 + 4) * 60));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            // Check the SequenceSlots
            IList<ISequenceSlot> sequenceSlots = ModelDAOHelper.DAOFactory.SequenceSlotDAO
              .FindAll ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (1), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
          }

          // 5. Sequence 0 detection at 10:19:59
          {
            stampDetection.StopIsoFile (UtcDateTime.From (2008, 01, 16, 10, 19, 59));
          }
          {
            // Check there is operation1 between 10:05:00 and 10:20:00
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots after 5.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (9 * 60));
            });
            // Check the auto-processes
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-processes after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 59)));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
          }

          // 8. Sequence 2 detection at 10:25:01
          {
            stampDetection.StartStamp (stamp2, null,
                                       UtcDateTime.From (2008, 01, 16, 10, 25, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 25, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 10, 28, 00));
          }
          {
            // Check there is operation1 between 10:05:00 and 10:20:00
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (3), "Number of operation slots after 5.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (9 * 60));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (3 * 60));
            });
            // Check the auto-processes
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 8.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 00)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence2));
            });
            // Check the SequenceSlots
            IList<ISequenceSlot> sequenceSlots = ModelDAOHelper.DAOFactory.SequenceSlotDAO
              .FindAll ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (2), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence2));
            });
          }

          // 9. Sequence 1 detection at 10:28:00
          {
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2008, 01, 16, 10, 28, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 28, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 10, 30, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 30, 30),
                                                                     UtcDateTime.From (2008, 01, 16, 10, 35, 30));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 40, 30),
                                                                     UtcDateTime.From (2008, 01, 16, 10, 45, 30));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 50, 30),
                                                                     UtcDateTime.From (2008, 01, 16, 10, 55, 30));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 10, 56, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 11, 01, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 11, 06, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 11, 11, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 11, 16, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 11, 21, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2008, 01, 16, 11, 26, 00),
                                                                     UtcDateTime.From (2008, 01, 16, 11, 31, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 05, 00),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 10, 00));
          }
          {
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (5), "Number of operation slots after 9.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (9 * 60));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (3 * 60));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 31, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (2400 - 180));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 10, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (5 * 60));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 9.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (4), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 05, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 19, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 25, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence2));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 10, 28, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2008, 01, 16, 11, 31, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 10, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test the process of 'not auto' sequences
    /// </summary>
    [Test]
    public void TestNotAutoSequence ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          Operation operation3 = session.Get<Operation> (12708);
          Assert.NotNull (operation3);
          Operation operation4 = session.Get<Operation> (12678);
          Assert.NotNull (operation4);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);
          Sequence sequence2 = session.Get<Sequence> (2);
          Assert.NotNull (sequence2);
          Sequence sequence3 = session.Get<Sequence> (3);
          Assert.NotNull (sequence3);
          Sequence sequence4 = session.Get<Sequence> (4);
          Assert.NotNull (sequence4);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          Stamp stamp3 = new Stamp ();
          stamp3.Sequence = sequence3;
          Stamp stamp4 = new Stamp ();
          stamp4.Sequence = sequence4;

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // Dates
          DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T11 = UtcDateTime.From (2008, 01, 16, 01, 01, 00);
          DateTime T12 = UtcDateTime.From (2008, 01, 16, 01, 02, 00);
          DateTime T2 = UtcDateTime.From (2008, 01, 16, 02, 00, 00);
          DateTime T3 = UtcDateTime.From (2008, 01, 16, 03, 00, 00);
          DateTime T4 = UtcDateTime.From (2008, 01, 16, 04, 00, 00);
          
          // Sequence 3 detection at T1, Auto-Sequence between T11 and T12,
          // Sequence 3 detection again at T2,
          // Sequence 4 detection at T3,
          // Sequence 1 detection at T1
          {
            stampDetection.StartStamp (stamp1, null, T0);
            stampDetection.StartStamp (stamp3, null, T1);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T11, T12);
            stampDetection.StartStamp (stamp3, null, T2);
            stampDetection.StartStamp (stamp4, null, T3);
            stampDetection.StartStamp (stamp1, null, T4);
          }
          {
            // Check there is:
            // - operation3 between T1 and T3
            // - operation4 between T3 and T4
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T3));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T3));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T4));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation4));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (T4));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (2), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T3));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence3));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T3));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T3));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T4));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence4));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the process a mix of 'auto' and 'not auto' sequences
    /// with the auto-sequence periods coming later
    /// </summary>
    [Test]
    public void TestMixAutoNotAutoSequences2 ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          Operation operation3 = session.Get<Operation> (12708);
          Assert.NotNull (operation3);
          Operation operation4 = session.Get<Operation> (12678);
          Assert.NotNull (operation4);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);
          Sequence sequence2 = session.Get<Sequence> (2);
          Assert.NotNull (sequence2);
          Sequence sequence3 = session.Get<Sequence> (3);
          Assert.NotNull (sequence3);
          Sequence sequence4 = session.Get<Sequence> (4);
          Assert.NotNull (sequence4);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          Stamp stamp3 = new Stamp ();
          stamp3.Sequence = sequence3;
          daoFactory.StampDAO.MakePersistent (stamp3);
          Stamp stamp4 = new Stamp ();
          stamp4.Sequence = sequence4;
          daoFactory.StampDAO.MakePersistent (stamp4);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // Dates
          DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
          DateTime T02 = UtcDateTime.From (2008, 01, 16, 00, 02, 00);
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T11 = UtcDateTime.From (2008, 01, 16, 01, 01, 00);
          DateTime T12 = UtcDateTime.From (2008, 01, 16, 01, 02, 00);
          DateTime T2 = UtcDateTime.From (2008, 01, 16, 02, 00, 00);
          DateTime T21 = UtcDateTime.From (2008, 01, 16, 02, 01, 00);
          DateTime T3 = UtcDateTime.From (2008, 01, 16, 03, 00, 00);
          DateTime T4 = UtcDateTime.From (2008, 01, 16, 04, 00, 00);
          
          // Sequence 3 detection at T1, Auto-Sequence between T11 and T12,
          // Sequence 3 detection again at T2,
          // Sequence 4 detection at T3,
          // Sequence 1 detection at T1
          {
            stampDetection.StartStamp (stamp1, null, T0);
            stampDetection.StartStamp (stamp3, null, T1);
            stampDetection.StartStamp (stamp2, null, T2);
            stampDetection.StartStamp (stamp4, null, T3);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T02, T11);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T12, T21);
          }
          {
            // Check there is:
            // - operation1 between T02 and T1
            // - operation3 between T1 and T2
            // - operation2 between T2 and T21
            // - operation4 between T3 and T4
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (4), "Number of operation slots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T02));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T1));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T2));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T2));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T21));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.That (operationSlots [i].BeginDateTime.Value, Is.EqualTo (T3));
            Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation4));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (6000));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (T2));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (T3));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence2));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (4), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T02));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T1));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T2));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence3));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T2));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T2));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T21));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence2));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T3));
            });
            ++i;
            Assert.That (sequenceSlots [i].BeginDateTime.Value, Is.EqualTo (T3));
            Assert.IsFalse (sequenceSlots [i].EndDateTime.HasValue);
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence4));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the process of 'not auto' sequences
    /// </summary>
    [Test]
    public void TestNotAutoSequence2 ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          Operation operation3 = session.Get<Operation> (12708);
          Assert.NotNull (operation3);
          Operation operation4 = session.Get<Operation> (12678);
          Assert.NotNull (operation4);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);
          Sequence sequence2 = session.Get<Sequence> (2);
          Assert.NotNull (sequence2);
          Sequence sequence3 = session.Get<Sequence> (3);
          Assert.NotNull (sequence3);
          Sequence sequence4 = session.Get<Sequence> (4);
          Assert.NotNull (sequence4);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          Stamp stamp3 = new Stamp ();
          stamp3.Sequence = sequence3;
          Stamp stamp4 = new Stamp ();
          stamp4.Sequence = sequence4;

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          {
            stampDetection.StartStamp (stamp1, null, T(1));
            stampDetection.StartStamp (stamp3, null, T(2));
            stampDetection.StopIsoFile (T(3));
            stampDetection.StartStamp (stamp2, null, T(10));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T(0), T(4));
            stampDetection.StartStamp (stamp4, null, T(12));
            stampDetection.StartStamp (stamp2, null, T(13));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T(11), T(15));
          }
          {
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (5), "Number of operation slots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (2)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (3)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (11)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (12)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (12)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (13)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation4));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (13)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (15)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (T (13)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence2));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (7), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T (1)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T (2)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T (2)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T (3)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T (3)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence3));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T (3)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T (4)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T (11)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T (11)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T (12)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T (12)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence2));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T (12)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T (13)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T (13)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence4));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T (13)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T (15)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence2));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the process of a mix of 'auto' and 'not auto' sequences
    /// </summary>
    [Test]
    public void TestMixAutoAndNotAutoSequence ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          Operation operation3 = session.Get<Operation> (12708);
          Assert.NotNull (operation3);
          Operation operation4 = session.Get<Operation> (12678);
          Assert.NotNull (operation4);
          // The two next are 'auto'
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);
          Sequence sequence2 = session.Get<Sequence> (2);
          Assert.NotNull (sequence2);
          // The two next are 'not auto'
          Sequence sequence3 = session.Get<Sequence> (3);
          Assert.NotNull (sequence3);
          Sequence sequence4 = session.Get<Sequence> (4);
          Assert.NotNull (sequence4);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          Stamp stamp3 = new Stamp ();
          stamp3.Sequence = sequence3;
          daoFactory.StampDAO.MakePersistent (stamp3);
          Stamp stamp4 = new Stamp ();
          stamp4.Sequence = sequence4;
          daoFactory.StampDAO.MakePersistent (stamp4);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          
          // Dates
          DateTime T0 = UtcDateTime.From (2008, 01, 16, 00, 00, 00);
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T2 = UtcDateTime.From (2008, 01, 16, 02, 00, 00);
          DateTime T3 = UtcDateTime.From (2008, 01, 16, 03, 00, 00);
          DateTime T4 = UtcDateTime.From (2008, 01, 16, 04, 00, 00);
          
          {
            stampDetection.StartStamp (stamp3, null, T0); // not-auto
            stampDetection.StartStamp (stamp1, null, T0); // auto
            stampDetection.StartStamp (stamp3, null, T1); // not-auto
            stampDetection.StartStamp (stamp3, null, T1); // not-auto
            stampDetection.StartStamp (stamp2, null, T2); // auto
            stampDetection.StartStamp (stamp4, null, T3); // not-auto
            stampDetection.StartStamp (stamp1, null, T4); // auto
          }
          {
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (3), "Number of auto-sequences after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (T0));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (T1));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (T2));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (T3));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence2));
            });
            ++i;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (T4));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
          }
          {
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (2), "Number of SequenceSlots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T2));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence3));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T3));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T3));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T4));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence4));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test the process of 'auto only' sequences,
    /// when the sequence is not extended because there is a 'no sequence' with activity period
    /// </summary>
    [Test]
    public void TestAutoOnlySequenceNotExtended ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // Dates
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T11 = UtcDateTime.From (2008, 01, 16, 01, 01, 00);
          DateTime T12 = UtcDateTime.From (2008, 01, 16, 01, 02, 00);
          DateTime T2 = UtcDateTime.From (2008, 01, 16, 02, 00, 00);
          DateTime T21 = UtcDateTime.From (2008, 01, 16, 02, 01, 00);
          DateTime T22 = UtcDateTime.From (2008, 01, 16, 02, 02, 00);
          DateTime T3 = UtcDateTime.From (2008, 01, 16, 03, 00, 00);
          DateTime T31 = UtcDateTime.From (2008, 01, 16, 03, 01, 00);
          DateTime T32 = UtcDateTime.From (2008, 01, 16, 03, 02, 00);
          
          {
            stampDetection.StartStamp (stamp1, null, T1);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T11, T12);
            stampDetection.StopIsoFile (T2);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T21, T22);
            stampDetection.StartStamp (stamp1, null, T3);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T31, T32);
          }
          {
            // Check there is:
            // - operation1 between T11 and T12
            // - operation1 between T31 and T32
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T11));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T12));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T31));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T32));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (T3));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (3), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T11));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T12));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T21));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T21));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T22));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (T31));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T31));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T32));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Stop a 'not auto' sequence
    /// </summary>
    [Test]
    public void TestStopNotAutoSequence ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation3 = session.Get<Operation> (12708);
          Assert.NotNull (operation3);
          Sequence sequence3 = session.Get<Sequence> (3);
          Assert.NotNull (sequence3);

          // Stamps
          Stamp stamp3 = new Stamp ();
          stamp3.Sequence = sequence3;

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          
          // Dates
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T2 = UtcDateTime.From (2008, 01, 16, 02, 00, 00);
          
          // Sequence 3 detection at T1
          // Stop the sequence 3 at T2
          {
            stampDetection.StartStamp (stamp3, null, T1);
            stampDetection.StopIsoFile (T2);
          }
          {
            // Check there is:
            // - operation3 between T1 and T2
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (1), "Number of operation slots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T2));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (0), "Number of auto-sequences after 1.");
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (1), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T2));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence3));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test AddAutoSequence with no auto only sequence detection
    /// </summary>
    [Test]
    public void TestAddAutoAutoSequenceNoAutoOnly ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // Dates
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T2 = UtcDateTime.From (2008, 01, 16, 02, 00, 00);
          
          {
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T1, T2);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T1, T2);
          }
          {
            // Check there is:
            // - no operation between T1 and T2
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (0), "Number of operation slots after 1.");
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (0), "Number of auto-sequences after 1.");
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (1), "Number of SequenceSlots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T2));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Start and Stop a 'auto only' sequence at the same date/time
    /// </summary>
    [Test]
    public void TestStartStopAutoOnlySequence ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          
          // Dates
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          
          // Start and stop Sequence 1 detection at T1
          {
            stampDetection.StartStamp (stamp1, null, T1);
            stampDetection.StopIsoFile (T1);
          }
          {
            // Check there is:
            // - no operation
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (0), "Number of operation slots after 1.");
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (0), "Number of auto-sequences after 1.");
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (0), "Number of SequenceSlots after 1.");
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Start twice a not auto-only sequence
    /// </summary>
    [Test]
    public void TestStartNotAutoOnlySequenceTwice ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Sequence sequence3 = session.Get<Sequence> (3);
          Assert.NotNull (sequence3);
          Sequence sequence4 = session.Get<Sequence> (4);
          Assert.NotNull (sequence4);

          // Stamps
          IStamp stamp3 = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp3.Sequence = sequence3;
          IStamp stamp4 = ModelDAOHelper.ModelFactory.CreateStamp ();
          stamp4.Sequence = sequence4;

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          
          // Dates
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T2 = UtcDateTime.From (2008, 01, 16, 01, 05, 00);
          
          // Start Sequence 3 detection at T1
          {
            stampDetection.StartStamp (stamp3, null, T1);
            stampDetection.StartStamp (stamp4, null, T2);
          }
          {
            // Check there is:
            // - two operations
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots");
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (0), "Number of auto-sequences");
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (2), "Number of SequenceSlots");
            int i = 0;
            ISequenceSlot sequenceSlot;
            sequenceSlot = sequenceSlots [i];
            Assert.Multiple (() => {
              Assert.That (sequenceSlot.BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (sequenceSlot.EndDateTime.Value, Is.EqualTo (T2));
              Assert.That (sequenceSlot.NextBegin, Is.EqualTo (T2));
            });
            ++i;
            sequenceSlot = sequenceSlots [i];
            Assert.That (sequenceSlot.BeginDateTime.Value, Is.EqualTo (T2));
            Assert.IsFalse (sequenceSlot.EndDateTime.HasValue);
            Assert.IsNull (sequenceSlot.NextBegin);
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test an activity with no sequence
    /// </summary>
    [Test]
    public void TestActivityWithNoSequence ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // Dates
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T2 = UtcDateTime.From (2008, 01, 16, 02, 00, 00);
          DateTime T3 = UtcDateTime.From (2008, 01, 16, 03, 00, 00);
          
          {
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T1, T2);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T2, T3);
          }
          {
            // Check there is no operation
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (0), "Number of operation slots after 1.");
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (0), "Number of auto-sequences after 1.");
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (1), "Number of SequenceSlots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T1));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T3));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test an activity with a sequence
    /// </summary>
    [Test]
    public void TestActivityWithSequence ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // Dates
          DateTime T1 = UtcDateTime.From (2008, 01, 16, 01, 00, 00);
          DateTime T11 = UtcDateTime.From (2008, 01, 16, 01, 01, 00);
          DateTime T12 = UtcDateTime.From (2008, 01, 16, 01, 02, 00);
          DateTime T13 = UtcDateTime.From (2008, 01, 16, 01, 03, 00);
          
          {
            stampDetection.StartStamp (stamp1, null, T1);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T11, T12);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T12, T13);
          }
          {
            // Check the operation slots
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (1), "Number of operation slots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T11));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T13));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (T1));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (1), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (T11));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (T13));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test some specific cases (with margin for example)
    /// </summary>
    [Test]
    public void TestAnalysis2()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          Operation operation3 = session.Get<Operation> (11003);
          Assert.NotNull (operation3);
          Operation operation4 = session.Get<Operation> (11004);
          Assert.NotNull (operation4);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);
          Sequence sequence2 = session.Get<Sequence> (2);
          Assert.NotNull (sequence2);

          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          
          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation3, null, null, null, null, null, null,
                                    new UtcDateTimeRange (UtcDateTime.From (2011, 01, 16, 00, 00, 00),
                                                          UtcDateTime.From (2011, 01, 16, 10, 12, 00)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
            UpdateIntermediateWorkPieceSummary (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation4, null, null, null, null, null, null,
                                    new UtcDateTimeRange (UtcDateTime.From (2011, 01, 16, 10, 12, 00)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
            UpdateIntermediateWorkPieceSummary (existingOperationSlot);
          }
          
          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // 1. Sequence 1 detection at 10:05:00
          {
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2011, 01, 16, 10, 05, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 05, 00),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 08, 00));
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2011, 01, 16, 10, 08, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 08, 00),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 10, 00));
          }
          {
            // Check operation3 is discontinued at 10:05:00
            // and operation4 is removed
            // Check there is operation1 between 10:05:00 and 10:10:00
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots after 2b.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 00, 00, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 10, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (5 * 60));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (1), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 10, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
          }

          // Test the margins
          {
            stampDetection.StopIsoFile (UtcDateTime.From (2011, 01, 16, 10, 10, 01));
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2011, 01, 16, 10, 15, 01));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 15, 00),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 15, 01));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 15, 01),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 20, 00));
            stampDetection.StopIsoFile (UtcDateTime.From (2011, 01, 16, 10, 22, 00));
          }
          {
            // Check operation3 is discontinued at 10:05:00
            // and operation4 is removed
            // Check there is operation1 between 10:05:00 and 10:10:00
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots after 2b.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 00, 00, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 20, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (10 * 60));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].Begin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 01)));
              Assert.That (autoSequences[i].End.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 22, 00)));
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (3), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 10, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 01)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 01)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 01)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 20, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
          }

          // Test the margins
          {
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2011, 01, 16, 10, 23, 00));
            stampDetection.StopIsoFile (UtcDateTime.From (2011, 01, 16, 10, 27, 00));
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2011, 01, 16, 10, 29, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 25, 00),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 30, 00));
          }
          {
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (3), "Number of operation slots after 2b.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 00, 00, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (0));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 27, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (12 * 60));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 29, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 30, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (60));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (1), "Number of auto-sequences after 1.");
            i = 0;
            Assert.That (autoSequences [i].Begin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 29, 00)));
            Assert.IsFalse (autoSequences [i].End.HasValue);
            Assert.Multiple (() => {
              Assert.That (autoSequences[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (autoSequences[i].Sequence, Is.EqualTo (sequence1));
            });
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (6), "Number of SequenceSlots after 1.");
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 10, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 01)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 01)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 01)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 20, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 25, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 25, 00))); // Because of the StopIsoFile at 10:22
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 27, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 27, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 27, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 29, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 29, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 29, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 30, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test an analysis process with:
    /// <item>sequence detection before auto-process period</item>
    /// <item>end of sequence detection in the middle of an auto-sequence period</item>
    /// </summary>
    [Test]
    public void TestAnalysis4()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          Operation operation3 = session.Get<Operation> (11003);
          Assert.NotNull (operation3);
          Operation operation4 = session.Get<Operation> (11004);
          Assert.NotNull (operation4);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);
          Sequence sequence2 = session.Get<Sequence> (2);
          Assert.NotNull (sequence2);
          
          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          
          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // 1. Sequence 1 detection at 10:04:52
          {
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2011, 01, 16, 10, 04, 52));
            stampDetection.StopIsoFile (UtcDateTime.From (2011, 01, 16, 10, 08, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 05, 00),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 10, 00));
          }
          {
            // Check there is operation1 between 10:05:00 and 10:08:00
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (1), "Number of operation slots after 1.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 05, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 08, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
              Assert.That (operationSlots[i].RunTime.Value.TotalSeconds, Is.EqualTo (3 * 60));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (0), "Number of auto-sequences after 1.");
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test an analysis process with:
    /// <item>an auto-sequence that was discontinued by a operation machine association</item>
    /// </summary>
    [Test]
    public void TestAnalysis5()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          Operation operation3 = session.Get<Operation> (11003);
          Assert.NotNull (operation3);
          Operation operation4 = session.Get<Operation> (11004);
          Assert.NotNull (operation4);
          Sequence sequence1 = session.Get<Sequence> (1);
          Assert.NotNull (sequence1);
          Sequence sequence2 = session.Get<Sequence> (2);
          Assert.NotNull (sequence2);
          
          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          
          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // 1. Auto-sequence 10:05:00 -> 10:10:00
          //    with no activity yet
          {
            stampDetection.StartStamp (stamp1, null,
                                       UtcDateTime.From (2011, 01, 16, 10, 05, 00));
            stampDetection.StopIsoFile (UtcDateTime.From (2011, 01, 16, 10, 12, 00));
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 08, 00),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 12, 00));
          }
          // 2. Operation machine association to discontinue the auto-sequence
          {
            IOperationMachineAssociation association =
              ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                             UtcDateTime.From (2011, 01, 16, 10, 10, 00));
            association.Operation = operation2;
            association.End = UtcDateTime.From (2011, 01, 16, 10, 15, 00);
            ((OperationMachineAssociation) association).Apply ();
          }
          // 3. Activities once again
          {
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (UtcDateTime.From (2011, 01, 16, 10, 12, 00),
                                                                     UtcDateTime.From (2011, 01, 16, 10, 20, 00));

          }
          {
            // Check there is:
            // - operation1 between 10:08 and 10:10
            // - operation2 between 10:10 and 10:15
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots after 2.");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 08, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 10, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 10, 00)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 15, 00)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
            });
            // Check the auto-sequences
            IList<AutoSequence> autoSequences =
              session.CreateCriteria<AutoSequence> ()
              .AddOrder (Order.Asc ("Begin"))
              .List<AutoSequence> ();
            Assert.That (autoSequences.Count, Is.EqualTo (0), "Number of auto-sequences after 1.");
            // Check the SequenceSlots
            IList<SequenceSlot> sequenceSlots =
              session.CreateCriteria<SequenceSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<SequenceSlot> ();
            Assert.That (sequenceSlots.Count, Is.EqualTo (2), "Number of SequenceSlots after 1.");
            // Note: here the used date/time are quite approximative. The sequence slots are not cut exactly at the time
            //       of the operation machine association. May be it is possible to do something better, but may be it
            //       is not so important too
            i = 0;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 08, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 12, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 12, 00)));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (sequence1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (sequenceSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 12, 00)));
              Assert.That (sequenceSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 20, 00)));
              Assert.That (sequenceSlots[i].NextBegin, Is.EqualTo (null));
              Assert.That (sequenceSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (sequenceSlots[i].Sequence, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    [Test]
    public void TestTrackerId37498937()
    {
      // should pass if 37498937 is corrected
      DateTime T0 =  UtcDateTime.From(2016, 01, 01).AddHours(7);
      DateTime T1 = T0.AddMinutes(1); // start seq1 and G
      DateTime T2 = T1.AddMinutes(1); // end  of G
      DateTime T3 = T2.AddMinutes(1);  // seq2  start (seq1 end)
      DateTime T31 = T3.AddSeconds(10); // G begin
      DateTime T32 = T3.AddSeconds(12); // seq1 start (end of seq2)
      DateTime T33 = T3.AddSeconds(15); // G end / G begin
      DateTime T34 = T3.AddSeconds(100); // G end
      
      
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          
          Operation operation1 = session.Get<Operation> (12666);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (1);
          Assert.NotNull (operation2);
          
          Sequence sequence1 = session.Get<Sequence> (8);
          Assert.NotNull (sequence1);
          Assert.That (sequence1.Operation, Is.EqualTo (operation1));
          Sequence sequence2 = session.Get<Sequence> (9);
          Assert.NotNull (sequence2);
          Assert.That (sequence2.Operation, Is.EqualTo (operation1));
          
          Sequence sequence3 = session.Get<Sequence> (10);
          Assert.NotNull (sequence3);
          Assert.That (sequence3.Operation, Is.EqualTo (operation1));
          
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          Stamp stamp3 = new Stamp ();
          stamp3.Sequence = sequence3;
          daoFactory.StampDAO.MakePersistent (stamp3);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          stampDetection.StartStamp (stamp1, null, T1);
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T1, T2);
          
          stampDetection.StartStamp (stamp2, null, T3);
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T31, T32);
          
          stampDetection.StartStamp (stamp1, null, T32);
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T33, T34);
          
          IOperationSlot initOperationSlot =
            daoFactory.OperationSlotDAO.FindAt(machine, T1);

          Assert.That (initOperationSlot, Is.Not.EqualTo (null));
          Assert.That (initOperationSlot.Operation, Is.EqualTo (operation1));
          // Assert.AreEqual(T34, initOperationSlot.EndDateTime.Value);

          // find last operation slot (w.r.t. begin date times)
          IOperationSlot operationSlot2 =
            NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<OperationSlot> ()
            .Add (Restrictions.Eq ("Machine", machine))
            .AddOrder (Order.Desc ("DateTimeRange"))
            .SetMaxResults (1)
            .UniqueResult<OperationSlot> ();

          Assert.That (operationSlot2, Is.EqualTo (initOperationSlot));
          
        } finally {
          transaction.Rollback ();
        }

      }
    }
    
    [Test]
    public void TestLongGap()
    {
      // pass if new operation slot created if more than 24 hours (configurable)
      // elapsed since last operation slot activity
      DateTime T0 =  UtcDateTime.From(2016, 01, 01).AddHours(7);
      DateTime T1 = T0.AddMinutes(1); // start seq
      DateTime T2 = T1.AddMinutes(2);
      
      // 24 hours since end of last operation slot = threshold
      // e.g. T2.AddHours(23).AddMinutes(59).AddSeconds(59) would be too short
      DateTime T3 = T2.AddHours(24);
      DateTime T4 = T3.AddMinutes(1);

      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          
          Operation operation1 = session.Get<Operation> (12666);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (1);
          Assert.NotNull (operation2);
          
          Sequence sequence1 = session.Get<Sequence> (8);
          Assert.NotNull (sequence1);
          Assert.That (sequence1.Operation, Is.EqualTo (operation1));
          Sequence sequence2 = session.Get<Sequence> (9);
          Assert.NotNull (sequence2);
          Assert.That (sequence2.Operation, Is.EqualTo (operation1));
          
          Sequence sequence3 = session.Get<Sequence> (10);
          Assert.NotNull (sequence3);
          Assert.That (sequence3.Operation, Is.EqualTo (operation1));
          
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          Stamp stamp3 = new Stamp ();
          stamp3.Sequence = sequence3;
          daoFactory.StampDAO.MakePersistent (stamp3);

          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          stampDetection.StartStamp (stamp1, null, T1);
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T1, T2);
          
          stampDetection.StartStamp (stamp2, null, T3);
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T3, T4);
          
          IOperationSlot initOperationSlot =
            daoFactory.OperationSlotDAO.FindAt(machine, T1);

          Assert.That (initOperationSlot, Is.Not.EqualTo (null));
          Assert.Multiple (() => {
            Assert.That (initOperationSlot.Operation, Is.EqualTo (operation1));
            Assert.That (initOperationSlot.EndDateTime.Value, Is.EqualTo (T2));
          });

          // find last operation slot (w.r.t. begin date times)
          IOperationSlot operationSlot2 =
            NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<OperationSlot> ()
            .Add (Restrictions.Eq ("Machine", machine))
            .AddOrder (Order.Desc ("DateTimeRange"))
            .SetMaxResults (1)
            .UniqueResult<OperationSlot> ();

          Assert.That (operationSlot2, Is.Not.EqualTo (initOperationSlot));
          Assert.Multiple (() => {
            Assert.That (operationSlot2.Operation, Is.EqualTo (operation1));
            Assert.That (operationSlot2.BeginDateTime.Value, Is.EqualTo (T3));
            Assert.That (operationSlot2.EndDateTime.Value, Is.EqualTo (T4));
          });
        } finally {
          transaction.Rollback ();
        }

      }
    }
    
    [Test]
    public void TestFactsRandom()
    {
      DateTime T0 =  UtcDateTime.From (2016, 01, 01);
      DateTime T1 =  T0.AddMinutes(1);
      DateTime T100 = T0.AddMinutes(100);
      
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      for(int seed = 0 ; seed < 1 /* make it bigger for more random tests */ ; seed++) {
        using (IDAOSession daoSession = daoFactory.OpenSession ())
          using (IDAOTransaction transaction = daoSession.BeginTransaction ())
        {
          ISession session = NHibernateHelper.GetCurrentSession ();
          try {
            // Reference data
            IMonitoredMachine machine =
              daoFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            MachineModule machineModule = session.Get<MachineModule> (1);
            Assert.NotNull (machineModule);
            
            Operation operation1 = session.Get<Operation> (12666);
            Assert.NotNull (operation1);
            Operation operation2 = session.Get<Operation> (1);
            Assert.NotNull (operation2);
            
            Sequence sequence1 = session.Get<Sequence> (8);
            Assert.NotNull (sequence1);
            Assert.That (sequence1.Operation, Is.EqualTo (operation1));
            Sequence sequence2 = session.Get<Sequence> (9);
            Assert.NotNull (sequence2);
            Assert.That (sequence2.Operation, Is.EqualTo (operation1));
            
            Sequence sequenceForOp2 = session.Get<Sequence> (1);
            Assert.NotNull (sequenceForOp2);
            Assert.That (sequenceForOp2.Operation, Is.EqualTo (operation2));
            
            Stamp stamp1 = new Stamp ();
            stamp1.Sequence = sequence1;
            daoFactory.StampDAO.MakePersistent (stamp1);
            Stamp stamp2 = new Stamp ();
            stamp2.Sequence = sequence2;
            daoFactory.StampDAO.MakePersistent (stamp2);
            Stamp stampForOp2 = new Stamp ();
            stampForOp2.Sequence = sequenceForOp2;
            daoFactory.StampDAO.MakePersistent (stampForOp2);
            
            OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
            OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
            MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                  operationDetection, operationCycleDetection);
            var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
            
            stampDetection.StartStamp (stamp1, null, T0);
            autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T0, T1);
            
            IOperationSlot initOperationSlot =
              daoFactory.OperationSlotDAO.FindAt(machine, T0);

            Assert.That (initOperationSlot, Is.Not.EqualTo (null));
            Assert.Multiple (() => {
              Assert.That (initOperationSlot.Operation, Is.EqualTo (operation1));
              Assert.That (initOperationSlot.EndDateTime.Value, Is.EqualTo (T1));
            });

            // create a succession of green then yellow, and stamps
            // does not account for merging successive corresponding facts etc

            Random randomGenerator = new Random(seed); // use controllable seed
            
            DateTime currentDateTime = T1;
            DateTime lastGreenEnd = T1;
            
            while(currentDateTime < T100) {

              int randomStampCase = randomGenerator.Next() % 2;
              int offsetInSeconds = 1 + randomGenerator.Next() % 10; // bad random scheme but well
              DateTime middleTime = currentDateTime.AddSeconds(offsetInSeconds / 2);
              DateTime nextDateTime = currentDateTime.AddSeconds(offsetInSeconds);
              
              switch(randomStampCase) {
                case 1:
                  if (randomGenerator.Next() % 2 == 0) {
                    autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (currentDateTime, nextDateTime);
                  }
                  break;
                case 2:
                  stampDetection.StartStamp((randomGenerator.Next() % 2 == 0) ? stamp1 : stamp2,
                                            null,
                                            middleTime);
                  break;
              }
              
              currentDateTime = nextDateTime;
            }
            
            IOperationSlot operationSlot =
              daoFactory.OperationSlotDAO.FindAt(machine, T0);

            Assert.That (operationSlot, Is.Not.EqualTo (null));
            Assert.That (operationSlot.Operation, Is.EqualTo (operation1));
            // Assert.AreEqual(lastGreenEnd, operationSlot.EndDateTime.Value);
            
            // find last operation slot (w.r.t. begin date times)
            IOperationSlot operationSlot2 =
              NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<OperationSlot> ()
              .Add (Restrictions.Eq ("Machine", machine))
              .AddOrder (Order.Desc ("DateTimeRange"))
              .SetMaxResults (1)
              .UniqueResult<OperationSlot> ();

            // never a new slot
            Assert.That (operationSlot2, Is.Not.EqualTo (null));
            Assert.That (operationSlot2, Is.EqualTo (operationSlot));
            
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }
    
    [Test]
    public void TestFacts()
    {
      
      DateTime T0 =  UtcDateTime.From (2016, 01, 01);
      DateTime T1 = T0.AddMinutes(1);
      DateTime T2 = T1.AddMinutes(1);
      DateTime T21 = T2.AddSeconds(10);
      DateTime T22 = T2.AddSeconds(20);
      DateTime T3 = T2.AddMinutes(1);
      DateTime T31 = T3.AddSeconds(20);
      DateTime T32 = T3.AddSeconds(24);
      DateTime T33 = T3.AddSeconds(26);
      DateTime T34 = T3.AddSeconds(30);
      DateTime T35 = T3.AddSeconds(34);
      DateTime T36 = T3.AddSeconds(40);
      DateTime T37 = T3.AddSeconds(45);
      DateTime T38 = T3.AddSeconds(50);
      DateTime T39 = T3.AddSeconds(55);
      DateTime T40 = T3.AddMinutes(1);
      DateTime T41 = T40.AddSeconds(10);
      
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMonitoredMachine machine =
            daoFactory.MonitoredMachineDAO.FindById (1);
          Assert.NotNull (machine);
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          
          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          var autoSequenceMachineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (operationDetection, machineModule, TransactionLevel.Serializable);
          
          // machine idle T0->T1
          
          // machine active T2->T21
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T2, T21);
          
          Operation operation1 = session.Get<Operation> (12666);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (1);
          Assert.NotNull (operation2);
          
          Sequence sequence1 = session.Get<Sequence> (8);
          Assert.NotNull (sequence1);
          Assert.That (sequence1.Operation, Is.EqualTo (operation1));
          Sequence sequence2 = session.Get<Sequence> (9);
          Assert.NotNull (sequence2);
          Assert.That (sequence2.Operation, Is.EqualTo (operation1));
          
          Sequence sequenceForOp2 = session.Get<Sequence> (1);
          Assert.NotNull (sequenceForOp2);
          Assert.That (sequenceForOp2.Operation, Is.EqualTo (operation2));
          
          Stamp stamp1 = new Stamp ();
          stamp1.Sequence = sequence1;
          daoFactory.StampDAO.MakePersistent (stamp1);
          Stamp stamp2 = new Stamp ();
          stamp2.Sequence = sequence2;
          daoFactory.StampDAO.MakePersistent (stamp2);
          Stamp stampForOp2 = new Stamp ();
          stampForOp2.Sequence = sequenceForOp2;
          daoFactory.StampDAO.MakePersistent (stampForOp2);
          
          // stamp for sequence 1 at T21
          stampDetection.StartStamp (stamp1, null, T21);
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T21, T3);
          
          IOperationSlot operationSlot =
            daoFactory.OperationSlotDAO.FindAt(machine, T0);

          Assert.That (operationSlot, Is.EqualTo (null));
          
          operationSlot =
            daoFactory.OperationSlotDAO.FindAt(machine, T21);

          Assert.That (operationSlot, Is.Not.EqualTo (null));
          Assert.Multiple (() => {
            Assert.That (operationSlot.Operation, Is.EqualTo (operation1));
            Assert.That (operationSlot.EndDateTime.Value, Is.EqualTo (T3));
          });

          // machine idle T3 -> T31

          // machine active T31 -> T32
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T31, T32);
          
          // stamp for seq2 at T33
          stampDetection.StartStamp (stamp2, null, T33);
          
          // machine idle T34 -> T35
          
          // operation slot should now have an extended end date time
          operationSlot =
            daoFactory.OperationSlotDAO.FindAt(machine, T21);

          Assert.That (operationSlot, Is.Not.EqualTo (null));
          Assert.Multiple (() => {
            Assert.That (operationSlot.Operation, Is.EqualTo (operation1));
            Assert.That (operationSlot.EndDateTime.Value, Is.EqualTo (T32));
          });

          // find last operation slot
          IOperationSlot operationSlot2 =
            NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<OperationSlot> ()
            .Add (Restrictions.Eq ("Machine", machine))
            .AddOrder (Order.Desc ("DateTimeRange"))
            .SetMaxResults (1)
            .UniqueResult<OperationSlot> ();

          // no new slot
          Assert.That (operationSlot2, Is.Not.EqualTo (null));
          Assert.That (operationSlot2, Is.EqualTo (operationSlot));

          // machine active from T36->T37
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T36, T37);
          
          // machine still active T37->T38
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T37, T38);
          
          // stamp for sequence of operation 2 this time at T39
          stampDetection.StartStamp(stampForOp2, null, T39);
          
          // thus no operation slot created
          operationSlot2 =
            NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<OperationSlot> ()
            .Add (Restrictions.Eq ("Machine", machine))
            .AddOrder (Order.Desc ("DateTimeRange"))
            .SetMaxResults (1)
            .UniqueResult<OperationSlot> ();

          Assert.That (operationSlot2, Is.Not.EqualTo (null));
          Assert.That (operationSlot2, Is.EqualTo (operationSlot));
          
          // now emit fact8 which is late enough to be assoc with last stamp (new op)
          autoSequenceMachineModuleAnalysis.AddAutoSequencePeriod (T40, T41);

          // this time it's ok
          operationSlot2 =
            NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<OperationSlot> ()
            .Add (Restrictions.Eq ("Machine", machine))
            .AddOrder (Order.Desc ("DateTimeRange"))
            .SetMaxResults (1)
            .UniqueResult<OperationSlot> ();

          Assert.That (operationSlot2, Is.Not.EqualTo (null));

          // success: good this time
          Assert.That (operationSlot2, Is.Not.EqualTo (operationSlot));
          
          IOperationSlot operationSlot3 =
            daoFactory.OperationSlotDAO.FindAt(machine, T40);

          // OK this time
          Assert.That (operationSlot3, Is.Not.EqualTo (null));
          Assert.That (operationSlot3.Operation, Is.EqualTo (operation2));
          
        } finally {
          transaction.Rollback ();
        }

      }
    }

    void UpdateIntermediateWorkPieceSummary (IOperationSlot operationSlot)
    {
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
    
    /// <summary>
    /// Test if isofile slots are correctly processed by stamp detection
    /// </summary>
    [Test]
    public void TestIsoFileSlots()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          MachineModule machineModule = session.Get<MachineModule> (1);
          Assert.NotNull (machineModule);
          IIsoFile isofile1 = session.Get<IsoFile> (1);
          Assert.NotNull(isofile1);
          IIsoFile isofile2 = session.Get<IsoFile> (2);
          Assert.NotNull(isofile2);
          
          // Stamps
          Stamp stamp1 = new Stamp ();
          stamp1.IsoFile = isofile1;
          Stamp stamp2 = new Stamp ();
          stamp2.IsoFile = isofile2;
          Stamp stamp3 = new Stamp ();
          stamp3.IsoFileEnd = true;
          
          OperationCycleDetection operationCycleDetection = new OperationCycleDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IDetectionExtension> ());
          OperationDetection operationDetection = new OperationDetection (machineModule.MonitoredMachine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
          MasterDetection stampDetection = new MasterDetection (machineModule, TransactionLevel.Serializable,
                                                                operationDetection, operationCycleDetection);
          
          // Create two isoFiles slots, the second has no end
          {
            stampDetection.StartStamp (stamp1, null, UtcDateTime.From (2011, 01, 16, 10, 04, 52));
            stampDetection.StartStamp (stamp2, null, UtcDateTime.From (2011, 01, 16, 10, 08, 59));
          }
          {
            IList<IsoFileSlot> isoFileSlots = session.CreateCriteria<IsoFileSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<IsoFileSlot>();
            Assert.That (isoFileSlots.Count, Is.EqualTo (2), "Number of isoFile slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (isoFileSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 04, 52)));
              Assert.That (isoFileSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 08, 59)));
              Assert.That (isoFileSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (isoFileSlots[i].IsoFile, Is.EqualTo (isofile1));
            });

            ++i;
            Assert.That (isoFileSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 08, 59)));
            Assert.IsFalse (isoFileSlots [i].EndDateTime.HasValue);
            Assert.Multiple (() => {
              Assert.That (isoFileSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (isoFileSlots[i].IsoFile, Is.EqualTo (isofile2));
            });
          }
          
          // End the second isoFileSlot
          {
            stampDetection.StartStamp (stamp3, null, UtcDateTime.From (2011, 01, 16, 12, 00, 00));
          }
          {
            IList<IsoFileSlot> isoFileSlots = session.CreateCriteria<IsoFileSlot> ()
              .AddOrder (Order.Asc ("BeginDateTime"))
              .List<IsoFileSlot>();
            Assert.That (isoFileSlots.Count, Is.EqualTo (2), "Number of isoFile slots");
            int i = 1;
            Assert.Multiple (() => {
              Assert.That (isoFileSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 10, 08, 59)));
              Assert.That (isoFileSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 01, 16, 12, 00, 00)));
              Assert.That (isoFileSlots[i].MachineModule, Is.EqualTo (machineModule));
              Assert.That (isoFileSlots[i].IsoFile, Is.EqualTo (isofile2));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Return different date/times for the tests
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    DateTime T (int i)
    {
      return UtcDateTime.From (2012, 06, 01, 12, 00, i);
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
