// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ShiftMachineAssociation
  /// </summary>
  [TestFixture]
  public class ShiftMachineAssociation_UnitTest : WithHourTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftMachineAssociation_UnitTest).FullName);

    /// <summary>
    /// Constructor
    ///
    /// In the unit test database, the processed day slots are:
    /// <item>2017-01-18: T(-2) - T(+22)</item>
    /// <item>2017-01-19: T(+22) - T(+46)</item>
    /// </summary>
    public ShiftMachineAssociation_UnitTest ()
      : base (UtcDateTime.From (2017, 01, 17, 23, 00, 00)) // local: 2017-01-18 0:00:00
    {
    }

    /// <summary>
    /// A ShiftMachineAssociation with no day and no shift
    /// (for example created by ShiftByMachineAccumulator when the machine shift becomes null)
    /// must keep the day of the operation slots
    /// </summary>
    [Test]
    public void TestNoDayNoShiftKeepsDay ()
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
          Assert.That (machine, Is.Not.Null);
          var operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (13157);
          Assert.That (operation1, Is.Not.Null);
          var shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
          Assert.That (shift2, Is.Not.Null);
          var day18 = new DateTime (2017, 01, 18);

          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)(OperationSlotSplitOption.ByDay | OperationSlotSplitOption.ByMachineShift));
          {
            var operationSlotSplit = ModelDAOHelper.ModelFactory.CreateOperationSlotSplit (machine);
            operationSlotSplit.End = T (+46);
            ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
          }
          Assert.That (ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (-2, +46)), Is.Empty);

          { // Existing operation slot with a day and a shift
            var operationSlot = ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine, operation1, null, null, null, null, day18, shift2, R (+6, +14));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
          }

          { // The machine shift becomes null between T+10 and T+14
            var association = new ShiftMachineAssociation (machine, null, null, R (+10, +14));
            association.Auto = true;
            (new ShiftMachineAssociationDAO ()).MakePersistent (association);
            AnalysisUnitTests.RunMakeAnalysis ();
          }

          {
            var slots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (-2, +46));
            Assert.That (slots, Has.Count.EqualTo (2));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].DateTimeRange, Is.EqualTo (R (+6, +10)));
              Assert.That (slots[i].Operation, Is.EqualTo (operation1));
              Assert.That (slots[i].Day, Is.EqualTo (day18));
              Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].DateTimeRange, Is.EqualTo (R (+10, +14)));
              Assert.That (slots[i].Operation, Is.EqualTo (operation1));
              Assert.That (slots[i].Day, Is.EqualTo (day18), "the day must be kept");
              Assert.That (slots[i].Shift, Is.Null);
            });
          }
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Same as <see cref="TestNoDayNoShiftKeepsDay"/> but on a period that spans two days:
    /// each part must keep its own day
    /// </summary>
    [Test]
    public void TestNoDayNoShiftOnTwoDays ()
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
          Assert.That (machine, Is.Not.Null);
          var operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (13157);
          Assert.That (operation1, Is.Not.Null);
          var shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
          Assert.That (shift2, Is.Not.Null);
          var day18 = new DateTime (2017, 01, 18);
          var day19 = new DateTime (2017, 01, 19);

          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)(OperationSlotSplitOption.ByDay | OperationSlotSplitOption.ByMachineShift));
          {
            var operationSlotSplit = ModelDAOHelper.ModelFactory.CreateOperationSlotSplit (machine);
            operationSlotSplit.End = T (+46);
            ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
          }
          Assert.That (ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (-2, +46)), Is.Empty);

          { // Existing operation slots on both sides of the day change at T+22
            var operationSlot1 = ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine, operation1, null, null, null, null, day18, shift2, R (+18, +22));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot1);
            var operationSlot2 = ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine, operation1, null, null, null, null, day19, shift2, R (+22, +26));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot2);
          }

          { // The machine shift becomes null between T+20 and T+24
            var association = new ShiftMachineAssociation (machine, null, null, R (+20, +24));
            association.Auto = true;
            (new ShiftMachineAssociationDAO ()).MakePersistent (association);
            AnalysisUnitTests.RunMakeAnalysis ();
          }

          {
            var slots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R (-2, +46));
            Assert.That (slots, Has.Count.EqualTo (4));
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].DateTimeRange, Is.EqualTo (R (+18, +20)));
              Assert.That (slots[i].Day, Is.EqualTo (day18));
              Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].DateTimeRange, Is.EqualTo (R (+20, +22)));
              Assert.That (slots[i].Day, Is.EqualTo (day18), "the day must be kept");
              Assert.That (slots[i].Shift, Is.Null);
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].DateTimeRange, Is.EqualTo (R (+22, +24)));
              Assert.That (slots[i].Day, Is.EqualTo (day19), "the day must be kept");
              Assert.That (slots[i].Shift, Is.Null);
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].DateTimeRange, Is.EqualTo (R (+24, +26)));
              Assert.That (slots[i].Day, Is.EqualTo (day19));
              Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            });
          }
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }
  }
}
