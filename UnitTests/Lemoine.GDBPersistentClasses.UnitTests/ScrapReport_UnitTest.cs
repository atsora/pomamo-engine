// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ScrapReport and the scrap counters of the operation slots
  /// </summary>
  [TestFixture]
  public class ScrapReport_UnitTest : WithDayTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ScrapReport_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ScrapReport_UnitTest ()
      : base (UtcDateTime.From (2011, 07, 31))
    {
    }

    IScrapReport CreateScrapReport (IOperationSlot operationSlot, UtcDateTimeRange range, int valid, int scrap, IScrapReport reportUpdate = null)
    {
      var scrapReport = ModelDAOHelper.ModelFactory.CreateScrapReport (operationSlot, range);
      scrapReport.NbCycles = 10;
      scrapReport.NbParts = 20;
      scrapReport.NbValid = valid;
      scrapReport.NbSetup = 2;
      scrapReport.NbScrap = scrap;
      scrapReport.NbFixable = 1;
      scrapReport.ReportUpdate = reportUpdate;
      ModelDAOHelper.DAOFactory.ScrapReportDAO.MakePersistent (scrapReport);
      return scrapReport;
    }

    static System.Collections.Generic.IList<IMachineModificationLog> GetDiscardLogs (IScrapReport scrapReport) =>
      ModelDAOHelper.DAOFactory.MachineModificationLogDAO
        .FindAll ()
        .Where (l => ((l.Modification as Lemoine.Collections.IDataWithId<long>)?.Id ?? 0) == ((Lemoine.Collections.IDataWithId<long>)scrapReport).Id)
        .Where (l => l.Message.StartsWith ("Discard:"))
        .ToList ();

    static void AssertScrapCounters (IOperationSlot operationSlot, int reportedCycles, int reportedParts, int valid, int setup, int scrap, int fixable)
    {
      Assert.Multiple (() => {
        Assert.That (operationSlot.ReportedCycles, Is.EqualTo (reportedCycles), "reported cycles");
        Assert.That (operationSlot.ReportedParts, Is.EqualTo (reportedParts), "reported parts");
        Assert.That (operationSlot.ValidParts, Is.EqualTo (valid), "valid parts");
        Assert.That (operationSlot.SetupParts, Is.EqualTo (setup), "setup parts");
        Assert.That (operationSlot.ScrapParts, Is.EqualTo (scrap), "scrap parts");
        Assert.That (operationSlot.FixableParts, Is.EqualTo (fixable), "fixable parts");
      });
    }

    /// <summary>
    /// A scrap report is counted in its operation slot,
    /// and a report that updates it replaces its counters
    /// </summary>
    [Test]
    public void TestApplyAndUpdate ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
        var operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (13157);

        var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, operation1, null, null, null, null, null, null, R (1, 2));
        ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);

        var report1 = CreateScrapReport (operationSlot, R (1.25, 1.5), 15, 2);
        AnalysisUnitTests.RunMakeAnalysis<ScrapReport> ();
        AssertScrapCounters (operationSlot, 10, 20, 15, 2, 2, 1);

        var report2 = CreateScrapReport (operationSlot, R (1.25, 1.5), 16, 1, report1);
        AnalysisUnitTests.RunMakeAnalysis<ScrapReport> ();
        // Only the second report is counted
        AssertScrapCounters (operationSlot, 10, 20, 16, 2, 1, 1);

        // A new recompute gives the same result
        ((OperationSlot)operationSlot).ConsolidateScrap ();
        AssertScrapCounters (operationSlot, 10, 20, 16, 2, 1, 1);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// A recompute of the operation slot while the scrap report is not completed yet does not count it,
    /// so that it is counted only once by its analysis
    /// </summary>
    [Test]
    public void TestRecomputeBeforeAnalysis ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
        var operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (13157);

        var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, operation1, null, null, null, null, null, null, R (1, 2));
        ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);

        var report1 = CreateScrapReport (operationSlot, R (1.25, 1.5), 15, 2);
        ((OperationSlot)operationSlot).ConsolidateScrap (); // report1 is not completed yet
        AssertScrapCounters (operationSlot, 0, 0, 0, 0, 0, 0);
        AnalysisUnitTests.RunMakeAnalysis<ScrapReport> ();
        AssertScrapCounters (operationSlot, 10, 20, 15, 2, 2, 1);
        ((OperationSlot)operationSlot).ConsolidateScrap (); // report1 is completed now
        AssertScrapCounters (operationSlot, 10, 20, 15, 2, 2, 1);

        // A pending report that updates report1: report1 is still counted until its analysis
        var report2 = CreateScrapReport (operationSlot, R (1.25, 1.5), 16, 1, report1);
        ((OperationSlot)operationSlot).ConsolidateScrap ();
        AssertScrapCounters (operationSlot, 10, 20, 15, 2, 2, 1);
        AnalysisUnitTests.RunMakeAnalysis<ScrapReport> ();
        AssertScrapCounters (operationSlot, 10, 20, 16, 2, 1, 1);
        ((OperationSlot)operationSlot).ConsolidateScrap ();
        AssertScrapCounters (operationSlot, 10, 20, 16, 2, 1, 1);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// The incremental consolidation after a change of range gives the same result as a full consolidation
    /// </summary>
    [Test]
    public void TestIncrementalConsolidation ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
        var operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (13157);

        var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, operation1, null, null, null, null, null, null, R (1, 4));
        ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
        CreateScrapReport (operationSlot, R (1.25, 1.5), 15, 2);
        CreateScrapReport (operationSlot, R (2.25, 2.5), 16, 1);
        CreateScrapReport (operationSlot, R (3.25, 3.5), 17, 0);
        AnalysisUnitTests.RunMakeAnalysis<ScrapReport> ();
        AssertScrapCounters (operationSlot, 30, 60, 48, 6, 3, 3);

        var slot = (OperationSlot)operationSlot;

        // Shorten both bounds: only the first and the last reports are removed
        slot.UpdateDateTimeRange (R (2, 3));
        slot.ConsolidateScrap ();
        AssertScrapCounters (slot, 10, 20, 16, 2, 1, 1);

        // Shorten the start inside the remaining report: it is not included any more
        slot.UpdateDateTimeRange (R (2.4, 3));
        slot.ConsolidateScrap ();
        AssertScrapCounters (slot, 0, 0, 0, 0, 0, 0);

        // Extend the start again: the reports are counted again
        slot.UpdateDateTimeRange (R (1, 3));
        slot.ConsolidateScrap ();
        AssertScrapCounters (slot, 20, 40, 31, 4, 3, 2);

        // Same result as a full consolidation
        slot.Operation = null;
        slot.Operation = operation1;
        slot.ConsolidateScrap ();
        AssertScrapCounters (slot, 20, 40, 31, 4, 3, 2);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Several scrap reports in an operation slot are summed up.
    /// A scrap report that is not included in a single operation slot is not counted
    /// </summary>
    [Test]
    public void TestReportOverSeveralSlots ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
        var operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (13157);
        var operation2 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (2);

        var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, operation1, null, null, null, null, null, null, R (1, 3));
        ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);

        var report1 = CreateScrapReport (operationSlot, R (1.25, 1.5), 15, 2);
        var report2 = CreateScrapReport (operationSlot, R (1.75, 2.25), 16, 1);
        AnalysisUnitTests.RunMakeAnalysis<ScrapReport> ();
        AssertScrapCounters (operationSlot, 20, 40, 31, 4, 3, 2);

        { // Split the slot inside report2: report2 is not counted any more
          var association = ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, R (2, 3));
          association.Operation = operation2;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();

          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (1, 3));
          var slot1 = operationSlots.Single (s => s.DateTimeRange.ContainsElement (T (1.25)));
          var slot2 = operationSlots.Single (s => s.DateTimeRange.ContainsElement (T (2.25)));
          Assert.That (slot2.Operation, Is.EqualTo (operation2));
          AssertScrapCounters (slot1, 10, 20, 15, 2, 2, 1);
          AssertScrapCounters (slot2, 0, 0, 0, 0, 0, 0);
          Assert.Multiple (() => {
            Assert.That (GetDiscardLogs (report1), Is.Empty);
            Assert.That (GetDiscardLogs (report2), Has.Count.EqualTo (1), "discard of report2, once");
          });
        }

        { // Back to operation1: report2 is still over two operation slots, it is not counted
          var association = ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, R (2, 3));
          association.Operation = operation1;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();

          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (1, 3));
          Assert.That (operationSlots.All (s => object.Equals (s.Operation, operation1)), Is.True);
          var slot1 = operationSlots.Single (s => s.DateTimeRange.ContainsElement (T (1.25)));
          AssertScrapCounters (slot1, 10, 20, 15, 2, 2, 1);
          Assert.That (operationSlots.Sum (s => s.ReportedCycles), Is.EqualTo (10), "report2 counted nowhere");
          Assert.That (GetDiscardLogs (report2), Has.Count.EqualTo (1), "discard of report2, once");
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// The scrap counters follow the report when the operation slot is split,
    /// and the report is discarded when the operation of its period changes
    /// </summary>
    [Test]
    public void TestSplitAndOperationChange ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
        var operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (13157);
        var operation2 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (2);
        var operation3 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (11003);

        var operationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, operation1, null, null, null, null, null, null, R (1, 3));
        ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);

        var report = CreateScrapReport (operationSlot, R (2.25, 2.5), 15, 2);
        AnalysisUnitTests.RunMakeAnalysis<ScrapReport> ();
        AssertScrapCounters (operationSlot, 10, 20, 15, 2, 2, 1);

        { // Split the slot: the report ends in the part that keeps operation1
          var association = ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, R (1, 1.5));
          association.Operation = operation2;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();

          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (1, 3));
          var slot1 = operationSlots.Single (s => s.DateTimeRange.ContainsElement (T (1.25)));
          var slot2 = operationSlots.Single (s => s.DateTimeRange.ContainsElement (T (2.5)));
          Assert.Multiple (() => {
            Assert.That (slot1.Operation, Is.EqualTo (operation2));
            Assert.That (slot2.Operation, Is.EqualTo (operation1));
          });
          AssertScrapCounters (slot1, 0, 0, 0, 0, 0, 0);
          AssertScrapCounters (slot2, 10, 20, 15, 2, 2, 1);
        }

        { // Change the operation where the report ends: it is discarded
          var association = ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, R (2, 3));
          association.Operation = operation3;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();

          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (1, 3));
          foreach (var slot in operationSlots) {
            AssertScrapCounters (slot, 0, 0, 0, 0, 0, 0);
          }
          Assert.That (GetDiscardLogs (report), Has.Count.EqualTo (1), "analysis log of the discard");
        }

        { // Split again the slot of operation3: the report is still not counted, and the discard is not logged again
          var association = ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine, R (2, 2.1));
          association.Operation = operation2;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();

          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (1, 3));
          var slot = operationSlots.Single (s => s.DateTimeRange.ContainsElement (T (2.5)));
          Assert.That (slot.Operation, Is.EqualTo (operation3));
          AssertScrapCounters (slot, 0, 0, 0, 0, 0, 0);
          Assert.That (GetDiscardLogs (report), Has.Count.EqualTo (1), "the discard is logged once");
        }

        transaction.Rollback ();
      }
    }
  }
}
