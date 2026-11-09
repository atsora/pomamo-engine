// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// Unit tests for the class PendingMachineModificationAnalysis
  /// </summary>
  [TestFixture]
  public class PendingMachineModificationAnalysis_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PendingMachineModificationAnalysis_UnitTest).FullName);

    /// <summary>
    /// A modification with a lower id that is committed late (its analysis status row is created
    /// after the one of a modification with a greater id) gets its analysis status row
    /// only thanks to the periodic full scan of FillAnalysisStatus
    /// </summary>
    /// <param name="fullScan">activate the full scan at each call</param>
    /// <param name="expectedStatusRow">the status row of the late modification is expected to be created</param>
    [TestCase (true, true)]
    [TestCase (false, false)] // No full scan: the late modification is not considered
    public void TestFillAnalysisStatusLateCommit (bool fullScan, bool expectedStatusRow)
    {
      try {
        ForceConfig (fullScan ? TimeSpan.FromTicks (1) : TimeSpan.FromDays (1), TimeSpan.FromDays (1));

        var daoFactory = ModelDAOHelper.DAOFactory;
        using (var daoSession = daoFactory.OpenSession ())
        using (var transaction = daoSession.BeginTransaction ()) {
          var machine = daoFactory.MonitoredMachineDAO.FindById (3);
          var lateId = CreateModification (machine, 0);
          var otherId = CreateModification (machine, 1);
          Assert.That (lateId, Is.LessThan (otherId));

          var analysis = new PendingMachineModificationAnalysis (machine, false);

          // 1. FillAnalysisStatus creates the status rows: its cursor moves to otherId
          DeleteStatusRow (machine, lateId);
          DeleteStatusRow (machine, otherId);
          analysis.FillAnalysisStatus (CancellationToken.None, 100);
          Assert.That (HasStatusRow (machine, otherId), Is.True, "Status row of the other modification");

          // 2. The late modification is committed only now (no status row yet)
          DeleteStatusRow (machine, lateId);
          Assert.That (HasStatusRow (machine, lateId), Is.False);
          analysis.FillAnalysisStatus (CancellationToken.None, 100);
          Assert.That (HasStatusRow (machine, lateId), Is.EqualTo (expectedStatusRow), "Status row of the late modification");

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// The analysis status rows of the modifications that are inserted during a pass
    /// are created again periodically during the pass
    /// </summary>
    /// <param name="refill">activate the refill at each check</param>
    /// <param name="expectedStatusRow">the status row of the new modification is expected to be created</param>
    [TestCase (true, true)]
    [TestCase (false, false)] // Former behavior: the status rows are created only once per pass
    public void TestFillAnalysisStatusRefillDuringPass (bool refill, bool expectedStatusRow)
    {
      try {
        ForceConfig (TimeSpan.FromDays (1), refill ? TimeSpan.FromTicks (1) : TimeSpan.FromDays (1));

        var daoFactory = ModelDAOHelper.DAOFactory;
        using (var daoSession = daoFactory.OpenSession ())
        using (var transaction = daoSession.BeginTransaction ()) {
          var machine = daoFactory.MonitoredMachineDAO.FindById (3);
          var analysis = new PendingMachineModificationAnalysis (machine, false);

          // 1. Start of a pass: the status rows are created
          var firstId = CreateModification (machine, 0);
          DeleteStatusRow (machine, firstId);
          analysis.CheckNewAnalysisStatus (CancellationToken.None);
          Assert.That (HasStatusRow (machine, firstId), Is.True, "Status row of the first modification");

          // 2. A new modification is inserted during the pass
          var newId = CreateModification (machine, 1);
          DeleteStatusRow (machine, newId);
          analysis.CheckNewAnalysisStatus (CancellationToken.None);
          Assert.That (HasStatusRow (machine, newId), Is.EqualTo (expectedStatusRow), "Status row of the new modification");

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    static void ForceConfig (TimeSpan fullScanFrequency, TimeSpan refillFrequency)
    {
      Lemoine.Info.ConfigSet.ForceValue ("Analysis.Modification.FillAnalysisStatusSerializable", false); // Nested transaction
      Lemoine.Info.ConfigSet.ForceValue ("Analysis.Modification.LockTableToPartition.FillAnalysisStatus", false);
      Lemoine.Info.ConfigSet.ForceValue ("Analysis.Modification.FillAnalysisStatus.FullScanFrequency", fullScanFrequency);
      Lemoine.Info.ConfigSet.ForceValue ("Analysis.Modification.FillAnalysisStatus.RefillFrequency", refillFrequency);
    }

    static long CreateModification (IMachine machine, int hours)
    {
      var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
        .FindById ((int)MachineObservationStateId.Attended);
      var dateTime = new DateTime (2008, 01, 01, 12, 00, 00, DateTimeKind.Utc).AddHours (hours);
      var modification = ModelDAOHelper.ModelFactory
        .CreateMachineObservationStateAssociation (machine, attended, dateTime);
      ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO.MakePersistent (modification);
      ModelDAOHelper.DAOFactory.FlushData ();
      return ((IDataWithId<long>)modification).Id;
    }

    static void DeleteStatusRow (IMachine machine, long modificationId)
    {
      NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery ("DELETE FROM machinemodificationstatus WHERE modificationid=:id AND machinemodificationstatusmachineid=:machineid")
        .SetInt64 ("id", modificationId)
        .SetInt32 ("machineid", machine.Id)
        .ExecuteUpdate ();
    }

    static bool HasStatusRow (IMachine machine, long modificationId)
    {
      return 0 < NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery ("SELECT COUNT(*) AS nb FROM machinemodificationstatus WHERE modificationid=:id AND machinemodificationstatusmachineid=:machineid")
        .AddScalar ("nb", NHibernate.NHibernateUtil.Int64)
        .SetInt64 ("id", modificationId)
        .SetInt32 ("machineid", machine.Id)
        .UniqueResult<long> ();
    }
  }
}
