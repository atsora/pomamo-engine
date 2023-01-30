// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ReasonSlotDAO.
  /// </summary>
  [TestFixture]
  public class ReasonSlotDAO_UnitTest
    : Lemoine.UnitTests.WithDayTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateSlot_UnitTest).FullName);

    public ReasonSlotDAO_UnitTest ()
      : base (new DateTime (2011, 01, 31, 05, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestFindById ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          {
            var reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindById (240450, machine);
            Assert.IsNotNull (reasonSlot);
          }
          {
            var reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindById (240450, machine);
            Assert.IsNotNull (reasonSlot);
          }
          {
            var reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindById (99999999, machine);
            Assert.IsNull (reasonSlot);
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test FindOverlapsRangeDescending
    /// </summary>
    [Test]
    public void TestOverlapsRangeDescending ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Database.SlotDAO.FindOverlapsRangeStep.LowerLimit",
                                             new LowerBound<DateTime> (null));

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (5);

          var slots0 = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, R (-30, 0)).Reverse ().ToList ();
          var slots1 = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRangeDescending (machine, R (-30, 0), TimeSpan.FromDays (5.5)).ToList ();
          var slots2 = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRangeDescending (machine, R (-30, 0), TimeSpan.FromDays (1)).ToList ();

          Assert.AreEqual (slots0.Count, slots1.Count);
          Assert.AreEqual (slots0.Count, slots2.Count);
          for (int i = 0; i < slots0.Count; ++i) {
            Assert.AreEqual (slots0[i].Id, slots1[i].Id);
            Assert.AreEqual (slots0[i].Id, slots2[i].Id);
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    /// <summary>
    /// Test FindOverlapsRangeDescending
    /// </summary>
    [Test]
    public void TestOverlapsRangeAscending ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Database.SlotDAO.FindOverlapsRangeStep.LowerLimit",
                                             new LowerBound<DateTime> (null));

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (5);

          var slots0 = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, R (-30, 0)).ToList ();
          var slots1 = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRangeAscending (machine, R (-30, 0), TimeSpan.FromDays (5.5)).ToList ();
          var slots2 = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRangeAscending (machine, R (-30, 0), TimeSpan.FromDays (1)).ToList ();

          Assert.AreEqual (slots0.Count, slots1.Count);
          Assert.AreEqual (slots0.Count, slots2.Count);
          for (int i = 0; i < slots0.Count; ++i) {
            Assert.AreEqual (slots0[i].Id, slots1[i].Id);
            Assert.AreEqual (slots0[i].Id, slots2[i].Id);
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }
  }
}
