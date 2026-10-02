// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Plugin.DefaultAccumulators;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ShiftByMachineAccumulator
  /// </summary>
  [TestFixture]
  public class ShiftByMachineAccumulator_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftByMachineAccumulator_UnitTest).FullName);

    /// <summary>
    /// A machine shift change in the future, before operationSlotSplit.End,
    /// must create a ShiftMachineAssociation,
    /// else the operation slots that already exist in the future are never updated
    /// </summary>
    [Test]
    public void TestFutureChangeBeforeOperationSlotSplitEnd ()
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
          Assert.That (machine, Is.Not.Null);
          var shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
          Assert.That (shift2, Is.Not.Null);

          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)(OperationSlotSplitOption.ByDay | OperationSlotSplitOption.ByMachineShift));
          var t = DateTime.UtcNow.Date.AddDays (2); // In the future only
          {
            var operationSlotSplit = ModelDAOHelper.ModelFactory.CreateOperationSlotSplit (machine);
            operationSlotSplit.End = t.AddDays (10);
            ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
          }

          // The machine shift is restored (null -> shift2) between t and t+8h,
          // and becomes null (shift2 -> null) between t+8h and t+12h
          var restoredRange = new UtcDateTimeRange (t, t.AddHours (8));
          var nullRange = new UtcDateTimeRange (t.AddHours (8), t.AddHours (12));
          var accumulator = new ShiftByMachineAccumulator ();
          accumulator.RemoveObservationStateSlotPeriod (CreateSlot (machine, restoredRange, null), restoredRange);
          accumulator.AddObservationStateSlotPeriod (CreateSlot (machine, restoredRange, shift2), restoredRange);
          accumulator.RemoveObservationStateSlotPeriod (CreateSlot (machine, nullRange, shift2), nullRange);
          accumulator.AddObservationStateSlotPeriod (CreateSlot (machine, nullRange, null), nullRange);
          accumulator.Store ("UnitTest");

          var associations = GetShiftMachineAssociations (machine, new UtcDateTimeRange (t, t.AddDays (10)));
          Assert.That (associations, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (associations[i].Range, Is.EqualTo (restoredRange));
            Assert.That (associations[i].Day, Is.Null);
            Assert.That (associations[i].Shift, Is.EqualTo (shift2));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (associations[i].Range, Is.EqualTo (nullRange));
            Assert.That (associations[i].Day, Is.Null);
            Assert.That (associations[i].Shift, Is.Null);
          });
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A machine shift change after operationSlotSplit.End must not create any ShiftMachineAssociation:
    /// OperationSlotSplitAnalysis processes this period later
    /// </summary>
    [Test]
    public void TestFutureChangeAfterOperationSlotSplitEnd ()
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (4);
          Assert.That (machine, Is.Not.Null);
          var shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
          Assert.That (shift2, Is.Not.Null);

          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)(OperationSlotSplitOption.ByDay | OperationSlotSplitOption.ByMachineShift));
          var t = DateTime.UtcNow.Date.AddDays (2); // In the future only
          {
            var operationSlotSplit = ModelDAOHelper.ModelFactory.CreateOperationSlotSplit (machine);
            operationSlotSplit.End = t;
            ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
          }

          var range = new UtcDateTimeRange (t.AddHours (1), t.AddHours (8));
          var accumulator = new ShiftByMachineAccumulator ();
          accumulator.RemoveObservationStateSlotPeriod (CreateSlot (machine, range, null), range);
          accumulator.AddObservationStateSlotPeriod (CreateSlot (machine, range, shift2), range);
          accumulator.Store ("UnitTest");

          var associations = GetShiftMachineAssociations (machine, new UtcDateTimeRange (t, t.AddDays (10)));
          Assert.That (associations, Is.Empty);
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    static IObservationStateSlot CreateSlot (IMachine machine, UtcDateTimeRange range, IShift shift)
    {
      var slot = new ObservationStateSlot (machine, range);
      slot.Shift = shift;
      return slot;
    }

    static IList<ShiftMachineAssociation> GetShiftMachineAssociations (IMachine machine, UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftMachineAssociation> ()
        .List<ShiftMachineAssociation> ()
        .Where (a => object.Equals (a.Machine, machine) && a.Range.Overlaps (range))
        .OrderBy (a => a.Range.Lower.Value)
        .ToList ();
    }
  }
}
