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
using System.Linq;
using Lemoine.Plugin.CycleCountSummary;
using Lemoine.Plugin.IntermediateWorkPieceSummary;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class OperationSlot
  /// </summary>
  [TestFixture]
  public class OperationSlot_UnitTest : WithDayTimeStamp
  {
    string previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (OperationSlot_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationSlot_UnitTest ()
      : base (UtcDateTime.From (2011, 07, 31))
    {
    }

    /// <summary>
    /// Test Save / Delete / Save
    /// </summary>
    [Test]
    public void TestSaveDeleteSave ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        IMachine machine = daoFactory.MachineDAO.FindById (1);
        IOperation operation1 = daoFactory.OperationDAO.FindById (1);
        IOperation operation2 = daoFactory.OperationDAO.FindById (2);
        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation1,
                                                           null,
                                                           null,
                                                           null, null, null, null,
                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 23, 23, 00, 00)));
        daoFactory.OperationSlotDAO.MakePersistent (operationSlot);
        IOperationSlot operationSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           null,
                                                           null,
                                                           null, null, null, null,
                                                           new UtcDateTimeRange (operationSlot.BeginDateTime));
        daoFactory.OperationSlotDAO.MakeTransient (operationSlot);
        daoFactory.OperationSlotDAO.MakePersistent (operationSlot2);
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the Consolidate method
    /// </summary>
    [Test]
    public void TestConsolidate ()
    {
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

        // full cycle ending at 2006.3
        IOperationCycle operationCycle0 =
          ModelDAOHelper.ModelFactory
          .CreateOperationCycle (machine);
        operationCycle0.SetRealEnd (UtcDateTime.From (2006, 06, 03));
        ModelDAOHelper.DAOFactory.OperationCycleDAO
          .MakePersistent (operationCycle0);

        // partial cycle starting at 2012.1
        IOperationCycle operationCycle1 =
          ModelDAOHelper.ModelFactory
          .CreateOperationCycle (machine);
        operationCycle1.Begin = UtcDateTime.From (2012, 06, 01);
        ModelDAOHelper.DAOFactory.OperationCycleDAO
          .MakePersistent (operationCycle1);

        // full cycle from 2012.2 to 2012.3
        IOperationCycle operationCycle2 =
          ModelDAOHelper.ModelFactory
          .CreateOperationCycle (machine);
        operationCycle2.Begin = UtcDateTime.From (2012, 06, 02);
        operationCycle2.SetRealEnd (UtcDateTime.From (2012, 06, 03));
        ModelDAOHelper.DAOFactory.OperationCycleDAO
          .MakePersistent (operationCycle2);

        // full cycle ending at 2012.5
        IOperationCycle operationCycle3 =
          ModelDAOHelper.ModelFactory
          .CreateOperationCycle (machine);
        operationCycle3.SetRealEnd (UtcDateTime.From (2012, 06, 05));
        ModelDAOHelper.DAOFactory.OperationCycleDAO
          .MakePersistent (operationCycle3);

        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory
          .CreateOperationSlot (machine,
                                operation1,
                                null,
                                null,
                                null, null, null, null,
                                new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
        operationSlot.AverageCycleTime = null;
        ModelDAOHelper.DAOFactory.OperationSlotDAO
          .MakePersistent (operationSlot);
        UpdateSummaries (operationSlot);

        (operationSlot as OperationSlot)
          .Consolidate (null);

        ModelDAOHelper.DAOFactory.Flush ();
        DAOFactory.EmptyAccumulators ();

        {
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (4));
          int i = 0;
          // first cycle removed from slot
          Assert.That (operationCycles[i].Begin, Is.Null);
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2006, 06, 03)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          });
          Assert.That (operationCycles[i].OperationSlot, Is.Null);
          ++i;
          Assert.Multiple (() => {
            // second cycle in slot, partial, estimated end equal to next cycle begin
            Assert.That (!operationCycles[i].Full, Is.True);
            Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
            Assert.That (operationCycles[i].Status.HasFlag (OperationCycleStatus.EndEstimated), Is.True);
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          Assert.Multiple (() => {
            // third cycle in slot, full, no estimation
            Assert.That (operationCycles[i].Full, Is.True);
            Assert.That (operationCycles[i].Status, Is.EqualTo (new OperationCycleStatus ()));
            Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          Assert.Multiple (() => {
            // fourth cycle in slot, full, no estimation
            Assert.That (operationCycles[i].Full, Is.True);
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 05)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
        }
        CheckSummaries (operationSlot);

        Assert.Multiple (() => {
          Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
          Assert.That (operationSlot.TotalCycles, Is.EqualTo (2));
          Assert.That (operationSlot.AverageCycleTime, Is.EqualTo (TimeSpan.FromDays (2)));
        });

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the Consolidate method (2)
    /// </summary>
    [Test]
    public void TestConsolidate2 ()
    {
      try {
        Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CyclesWithRealEndFull.OperationCycleFullExtension));

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

          IOperationCycle operationCycle0 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle0.Begin = UtcDateTime.From (2008, 01, 10);
          operationCycle0.SetRealEnd (UtcDateTime.From (2008, 01, 25));
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle0);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1,
                                  null, null, null, null, null, null,
                                  new UtcDateTimeRange (UtcDateTime.From (2008, 01, 05)));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          operationSlot.ConsolidateRunTime ();
          UpdateSummaries (operationSlot);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
          });

          // shorten operation slot
          operationSlot.EndDateTime = UtcDateTime.From (2008, 01, 20);
          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          // get operation cycles in order (FindAll does not ensure that)
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAllInRange (machine, new UtcDateTimeRange (UtcDateTime.From (2008, 01, 01)));

          Assert.That (operationCycles, Has.Count.EqualTo (2));

          Assert.That (operationCycles[0].End <= operationCycles[1].End, Is.True);
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Full, Is.EqualTo (false));
            Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2008, 01, 10)));
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2008, 01, 20)));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });


          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Full, Is.EqualTo (true));
            Assert.That (operationCycles[i].Begin.HasValue, Is.False);
            Assert.That (operationCycles[i].Status.HasFlag (OperationCycleStatus.BeginEstimated), Is.True);
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2008, 01, 25)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (null));

            Assert.That (operationSlot.TotalCycles, Is.EqualTo (0));
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
          });
          CheckSummaries (operationSlot);

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
      }
    }

    /// <summary>
    /// Test the Consolidate method (3)
    /// </summary>
    [Test]
    public void TestConsolidate3 ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.OperationSlotCycles.OperationSlotCyclesAccumulatorExtension));
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.IntermediateWorkPieceSummary.IntermediateWorkPieceByMachineSummaryAccumulatorExtension));

          // Reference data
          IMonitoredMachine machine =
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);

          IOperationCycle operationCycle0 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle0.Begin = T (10);
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle0);

          IOperationCycle operationCycle1 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle1.Begin = T (20);
          operationCycle1.SetRealEnd (T (25));
          operationCycle1.OperationSlot = null;
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle1);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1,
                                  null, null, null, null, null, null,
                                  R (5, 20));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          UpdateSummaries (operationSlot);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (0));
          });

          operationSlot.EndDateTime = T (25);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
          });

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAllInRange (machine, new UtcDateTimeRange (T (1)));
          Assert.That (operationCycles, Has.Count.EqualTo (2));

          Assert.Multiple (() => {
            Assert.That (operationCycles[0].End.HasValue, Is.True);
            Assert.That (operationCycles[1].Begin, Is.EqualTo (operationCycles[0].End.Value));
          });
          CheckSummaries (operationSlot);
          /*
          Assert.AreEqual (0, operationSlot.PartialCycles);

          // shorten operation slot
          operationSlot.EndDateTime = UtcDateTime.From (2008, 01, 20);
          (operationSlot as OperationSlot)
            .Consolidate ();

          // get operation cycles in order (FindAll does not ensure that)
          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAllInRange(machine, UtcDateTime.From(2008, 01, 01), null);

          Assert.AreEqual (2, operationCycles.Count);

          Assert.IsTrue (operationCycles [0].End <= operationCycles[1].Begin);
          int i = 0;
          Assert.AreEqual (true, operationCycles [i].IsPartial());
          Assert.AreEqual (UtcDateTime.From(2008, 01, 10), operationCycles [i].Begin);
          Assert.AreEqual (UtcDateTime.From(2008, 01, 20), operationCycles [i].End);
          Assert.AreEqual (OperationCycleStatus.EndEstimated, operationCycles [i].Status);
          Assert.AreEqual (machine, operationCycles [i].Machine);
          Assert.AreEqual (operationSlot, operationCycles [i].OperationSlot);


          ++i;
          Assert.AreEqual (true, operationCycles [i].IsFull());
          Assert.AreEqual (UtcDateTime.From(2008, 01, 20), operationCycles [i].Begin);
          Assert.AreEqual (OperationCycleStatus.BeginEstimated, operationCycles [i].Status);
          Assert.AreEqual (UtcDateTime.From (2008, 01, 25), operationCycles [i].End);
          Assert.AreEqual (machine, operationCycles [i].Machine);
          Assert.AreEqual (null, operationCycles [i].OperationSlot);

          Assert.AreEqual (0, operationSlot.TotalCycles);
          Assert.AreEqual (1, operationSlot.PartialCycles);
           */
        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test the Consolidate method
    /// for OperationCycle in case a new operation slot is coming
    /// </summary>
    [Test]
    public void TestConsolidateOperationCycleNewOperationSlot ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.OperationSlotCycles.OperationSlotCyclesAccumulatorExtension));
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.IntermediateWorkPieceSummary.IntermediateWorkPieceByMachineSummaryAccumulatorExtension));

          // Reference data
          IMachine machine =
          ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);

          IOperationCycle operationCycle0 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle ((IMonitoredMachine)machine);
          operationCycle0.SetRealEnd (UtcDateTime.From (2006, 06, 03));
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle0);
          IOperationCycle operationCycle1 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle ((IMonitoredMachine)machine);
          operationCycle1.Begin = UtcDateTime.From (2012, 06, 01);
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle1);
          IOperationCycle operationCycle2 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle ((IMonitoredMachine)machine);
          operationCycle2.Begin = UtcDateTime.From (2012, 06, 02);
          operationCycle2.SetRealEnd (UtcDateTime.From (2012, 06, 03));
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle2);
          IOperationCycle operationCycle3 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle ((IMonitoredMachine)machine);
          operationCycle3.SetRealEnd (UtcDateTime.From (2012, 06, 05));
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle3);

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1,
                                  null, null, null, null, null, null,
                                  new UtcDateTimeRange (UtcDateTime.From (2008, 01, 16, 00, 00, 00)));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          UpdateSummaries (operationSlot);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          {
            IOperation operation2 =
              ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (2);
            Assert.That (operation2, Is.Not.Null);
            IOperationMachineAssociation operationMachineAssociation =
              ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                             UtcDateTime.From (2012, 06, 03, 01, 00, 00));
            operationMachineAssociation.Operation = operation2;
            ((OperationMachineAssociation)operationMachineAssociation).Apply ();
          }
          IOperationSlot operationSlot2 =
            ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAt (machine,
                                                               UtcDateTime.From (2012, 06, 03, 01, 00, 00));
          Assert.That (operationSlot2, Is.Not.Null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (4));
          int i = 0;
          Assert.That (operationCycles[i].Begin, Is.Null);
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2006, 06, 03)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
          });
          Assert.That (operationCycles[i].OperationSlot, Is.Null);
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Full, Is.False);
            Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 01)));
            // glueing generated partial cycle with an estimated end
            Assert.That (operationCycles[i].Status.HasFlag (OperationCycleStatus.EndEstimated), Is.True);
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (UtcDateTime.From (2012, 06, 02)));
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 03)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationCycles[i].Begin, Is.EqualTo (operationSlot2.BeginDateTime.NullableValue));
            Assert.That (operationCycles[i].Status, Is.EqualTo (OperationCycleStatus.BeginEstimated));
            Assert.That (operationCycles[i].End, Is.EqualTo (UtcDateTime.From (2012, 06, 05)));
            Assert.That (operationCycles[i].Machine, Is.EqualTo (machine));
            Assert.That (operationCycles[i].OperationSlot, Is.EqualTo (operationSlot2));

            Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (1));
          });
          CheckSummaries (operationSlot);
          Assert.Multiple (() => {
            Assert.That (operationSlot2.PartialCycles, Is.EqualTo (0));
            Assert.That (operationSlot2.TotalCycles, Is.EqualTo (1));
          });
          CheckSummaries (operationSlot2);

        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test the Consolidate method
    /// OperationSlot grows: last partial cycle estimated end should also grow
    /// </summary>
    [Test]
    public void TestConsolidateOperationSlotGrowsLastPartialCycleGrows ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.OperationSlotCycles.OperationSlotCyclesAccumulatorExtension));

          // Reference data
          IMachine machine =
          ModelDAOHelper.DAOFactory.MachineDAO
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
                                  new UtcDateTimeRange (UtcDateTime.From (2006, 01, 16, 00, 00, 00)));
          UpdateSummaries (operationSlot);

          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);

          IOperationCycle operationCycle0 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle ((IMonitoredMachine)machine);
          operationCycle0.Begin = UtcDateTime.From (2006, 06, 03);
          operationCycle0.OperationSlot = operationSlot;
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle0);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          IList<IOperationCycle> operationCycles =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAll ();
          Assert.That (operationCycles, Has.Count.EqualTo (1));
          IOperationCycle operationCycle1 = operationCycles[0];

          Assert.Multiple (() => {
            Assert.That (operationCycle1.Begin, Is.EqualTo (UtcDateTime.From (2006, 06, 03)));
            Assert.That (operationCycle1.End, Is.Null);
            Assert.That (operationCycle1.OperationSlot, Is.EqualTo (operationSlot));
          });

          operationSlot.EndDateTime = UtcDateTime.From (2006, 06, 04);
          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (operationCycle1.Begin, Is.EqualTo (UtcDateTime.From (2006, 06, 03)));
            Assert.That (operationCycle1.End, Is.EqualTo (UtcDateTime.From (2006, 06, 04)));
            Assert.That (operationCycle1.Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycle1.OperationSlot, Is.EqualTo (operationSlot));
          });

          operationSlot.EndDateTime = UtcDateTime.From (2006, 06, 05);
          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (operationCycle1.Begin, Is.EqualTo (UtcDateTime.From (2006, 06, 03)));
            Assert.That (operationCycle1.End, Is.EqualTo (UtcDateTime.From (2006, 06, 05)));
            Assert.That (operationCycle1.Status, Is.EqualTo (OperationCycleStatus.EndEstimated));
            Assert.That (operationCycle1.OperationSlot, Is.EqualTo (operationSlot));
          });
          CheckSummaries (operationSlot);
        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test the optimizations in Consolidate method
    /// </summary>
    [Test]
    public void TestOptimizeConsolidate ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.OperationSlotCycles.OperationSlotCyclesAccumulatorExtension));
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.IntermediateWorkPieceSummary.IntermediateWorkPieceByMachineSummaryAccumulatorExtension));

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
                                  null,
                                  null,
                                  null, null, new DateTime (2012, 06, 01), null,
                                  new UtcDateTimeRange (DT (10), DT (35)));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          (operationSlot as OperationSlot)
            .Consolidate (null);

          { // full cycle ending at t(2)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (2));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
            var cycleOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAt (machine, DT (2));
            operationCycle.OperationSlot = cycleOperationSlot;
          }

          for (int i = 0; i < 20; ++i) {
            // full cycle ending at T(12+i)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (12 + i));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
            var cycleOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAt (machine, DT (12 + i));
            operationCycle.OperationSlot = cycleOperationSlot;
          }

          { // partial cycle starting at T(39)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.Begin = DT (39);
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
            var cycleOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAt (machine, DT (39));
            operationCycle.OperationSlot = cycleOperationSlot;
          }

          { // full cycle from T(40) to T(42)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.Begin = DT (40);
            operationCycle.SetRealEnd (DT (42));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
            var cycleOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAt (machine, DT (42));
            operationCycle.OperationSlot = cycleOperationSlot;
          }

          { // full cycle ending at T(50)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (50));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
            var cycleOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAt (machine, DT (50));
            operationCycle.OperationSlot = cycleOperationSlot;
          }

          { // full cycle ending at T(5)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (5));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
            var cycleOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAt (machine, DT (5));
            operationCycle.OperationSlot = cycleOperationSlot;
          }

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (20));
          });
          { // Extend operation slot
            IOperationMachineAssociation association =
              ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, DT (1), null, false);
            association.End = DT (59);
            association.Operation = operation1;
            association.Apply ();
          }

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          { // Test the operation slots
            IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAll (machine);
            Assert.That (operationSlots, Has.Count.EqualTo (1));
            IOperationSlot operationSloti = operationSlots[0];
            Assert.Multiple (() => {
              Assert.That (operationSloti.PartialCycles, Is.EqualTo (1));
              Assert.That (operationSloti.TotalCycles, Is.EqualTo (24));
              Assert.That (operationSloti.AverageCycleTime.HasValue, Is.True);
              Assert.That (operationSloti.AverageCycleTime.Value.Seconds, Is.EqualTo (2));
            });
            CheckSummaries (operationSloti);
          }

        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test the optimizations in Consolidate method
    /// </summary>
    [Test]
    public void TestOptimizeConsolidate2 ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.OperationSlotCycles.OperationSlotCyclesAccumulatorExtension));

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

          { // full cycle ending at t(2)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (2));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          for (int i = 0; i < 20; ++i) {
            // full cycle ending at T(12+i)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (12 + i));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          { // partial cycle starting at T(39)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.Begin = DT (39);
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          { // full cycle from T(40) to T(42)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.Begin = DT (40);
            operationCycle.SetRealEnd (DT (42));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          { // full cycle ending at T(50)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (50));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          { // full cycle ending at T(5)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (5));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1,
                                  null,
                                  null,
                                  null, null, null, null,
                                  new UtcDateTimeRange (DT (10), DT (55)));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          UpdateSummaries (operationSlot);
          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (1));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (22));
          });

          { // Create a new operation slot at T(54)
            IOperationMachineAssociation association =
              ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, DT (54), null, false);
            association.End = DT (59);
            association.Operation = operation2;
            association.Apply ();
          }

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          { // Test the operation slots
            IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAll (machine);
            Assert.That (operationSlots, Has.Count.EqualTo (2));
            int i = -1;
            IOperationSlot operationSloti;
            operationSloti = operationSlots[++i];
            Assert.Multiple (() => {
              Assert.That (operationSloti.PartialCycles, Is.EqualTo (1));
              Assert.That (operationSloti.TotalCycles, Is.EqualTo (22));
              Assert.That (operationSloti.AverageCycleTime.HasValue, Is.True);
              Assert.That (operationSloti.AverageCycleTime.Value.Seconds, Is.EqualTo (1));
            });
            operationSloti = operationSlots[++i];
            CheckSummaries (operationSloti);
          }

        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    /// <summary>
    /// Test the optimizations in Consolidate method
    /// </summary>
    [Test]
    public void TestOptimizeConsolidate3 ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.OperationSlotCycles.OperationSlotCyclesAccumulatorExtension));

          // Reference data
          IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
          Assert.That (machine, Is.Not.Null);
          IOperation operation1 =
            ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          Assert.That (operation1, Is.Not.Null);

          { // full cycle ending at t(2)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (2));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          for (int i = 0; i < 20; ++i) {
            // full cycle ending at T(12+i)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (12 + i));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          { // partial cycle starting at T(39)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.Begin = DT (39);
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          { // full cycle from T(40) to T(42)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.Begin = DT (40);
            operationCycle.SetRealEnd (DT (42));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          { // full cycle ending at T(50)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (50));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          { // full cycle ending at T(5)
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory
              .CreateOperationCycle (machine);
            operationCycle.SetRealEnd (DT (5));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }

          IOperationSlot operationSlot =
            ModelDAOHelper.ModelFactory
            .CreateOperationSlot (machine,
                                  operation1,
                                  null,
                                  null,
                                  null, null, null, null,
                                  new UtcDateTimeRange (DT (10), DT (35)));
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (operationSlot);
          UpdateSummaries (operationSlot);

          (operationSlot as OperationSlot)
            .Consolidate (null);

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (operationSlot.PartialCycles, Is.EqualTo (0));
            Assert.That (operationSlot.TotalCycles, Is.EqualTo (20));
          });

          { // Extend operation slot
            IOperationMachineAssociation association =
              ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, DT (10), null, false);
            association.End = DT (59);
            association.Operation = operation1;
            association.Apply ();
          }

          ModelDAOHelper.DAOFactory.Flush ();
          DAOFactory.EmptyAccumulators ();

          { // Test the operation slots
            IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAll (machine);
            Assert.That (operationSlots, Has.Count.EqualTo (1));
            IOperationSlot operationSloti = operationSlots[0];
            Assert.Multiple (() => {
              Assert.That (operationSloti.PartialCycles, Is.EqualTo (1));
              Assert.That (operationSloti.TotalCycles, Is.EqualTo (22));
              Assert.That (operationSloti.AverageCycleTime.HasValue, Is.True);
              Assert.That (operationSloti.AverageCycleTime.Value.Seconds, Is.EqualTo (1));
            });
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    void UpdateSummaries (IOperationSlot operationSlot)
    {
      if (operationSlot.Day.HasValue) {
        UpdateCycleCountSummary (operationSlot);
        UpdateIntermediateWorkPieceSummary (operationSlot);
        ModelDAOHelper.DAOFactory.Flush ();
      }
    }

    void UpdateCycleCountSummary (IOperationSlot operationSlot)
    {
      if (operationSlot.Day.HasValue) {
        var summary = new CycleCountSummaryDAO ()
          .FindByKey (operationSlot.Machine,
          operationSlot.Day.Value,
          operationSlot.Shift,
          operationSlot.WorkOrder,
          operationSlot.Line,
          operationSlot.ManufacturingOrder,
          operationSlot.Component,
          operationSlot.Operation);
        if (null == summary) {
          summary = new CycleCountSummary (operationSlot.Machine,
            operationSlot.Day.Value,
            operationSlot.Shift,
            operationSlot.WorkOrder,
            operationSlot.Line,
            operationSlot.ManufacturingOrder,
            operationSlot.Component,
            operationSlot.Operation);
        }
      ((CycleCountSummary)summary).Full = operationSlot.TotalCycles;
        ((CycleCountSummary)summary).Partial = operationSlot.PartialCycles;
        new CycleCountSummaryDAO ().MakePersistent (summary);
        ModelDAOHelper.DAOFactory.Flush ();
      }
      else {
        // TODO: ... ?
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
                        operationSlot.ManufacturingOrder,
                        operationSlot.Day,
                        operationSlot.Shift);
          if (null == summary) {
            summary = new IntermediateWorkPieceByMachineSummary (operationSlot.Machine,
                                                            intermediateWorkPiece,
                                                            operationSlot.Component,
                                                            operationSlot.WorkOrder,
                                                            operationSlot.Line,
                                                            operationSlot.ManufacturingOrder,
                                                            operationSlot.Day,
                                                            operationSlot.Shift);
          }
          summary.Counted += operationSlot.TotalCycles * intermediateWorkPiece.OperationQuantity;
          summary.Corrected += operationSlot.TotalCycles * intermediateWorkPiece.OperationQuantity;
          new IntermediateWorkPieceByMachineSummaryDAO ()
            .MakePersistent (summary);
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
            operationSlot.ManufacturingOrder,
            operationSlot.Component,
            operationSlot.Operation);
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindByDayShift (operationSlot.Machine, operationSlot.Day.Value, operationSlot.Shift)
            .Where (s => WorkOrder.Equals (operationSlot.WorkOrder, s.WorkOrder))
            .Where (s => Line.Equals (operationSlot.Line, s.Line))
            .Where (s => ManufacturingOrder.Equals (operationSlot.ManufacturingOrder, s.ManufacturingOrder))
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
            IIntermediateWorkPieceByMachineSummary summary =
              new IntermediateWorkPieceByMachineSummaryDAO ()
              .FindByKey (operationSlot.Machine,
                          intermediateWorkPiece,
                          operationSlot.Component,
                          operationSlot.WorkOrder,
                          operationSlot.Line,
                          operationSlot.ManufacturingOrder,
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
    /// Return different date/times for the tests
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    DateTime DT (int i)
    {
      return UtcDateTime.From (2012, 06, 01, 12, 00, i);
    }

    [OneTimeSetUp]
    public void Init ()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();

      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.OperationSlotCycles.OperationSlotCyclesAccumulatorExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.IntermediateWorkPieceSummary.IntermediateWorkPieceByMachineSummaryAccumulatorExtension));
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();

      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
