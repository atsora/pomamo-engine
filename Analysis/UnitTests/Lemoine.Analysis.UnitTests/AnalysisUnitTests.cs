// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using System.Linq;
using Lemoine.Analysis;
using Lemoine.GDBPersistentClasses;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// Utility methods to check the Analysis
  /// </summary>
  public class AnalysisUnitTests
  {
    /// <summary>
    /// Check the number of analysis logs
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expected"></param>
    internal static void CheckNumberOfAnalysisLogs (ISession session, int expected)
    {
      CheckNumberOfAnalysisLogs (expected);
    }

    /// <summary>
    /// Check the number of analysis logs
    /// </summary>
    /// <param name="expected"></param>
    internal static void CheckNumberOfAnalysisLogs (int expected)
    {
      IList<IGlobalModificationLog> globalModificationLogs =
        ModelDAOHelper.DAOFactory.GlobalModificationLogDAO
        .FindAll ();
      IList<IMachineModificationLog> machineModificationLogs =
        ModelDAOHelper.DAOFactory.MachineModificationLogDAO
        .FindAll ();
      Assert.That (globalModificationLogs.Count + machineModificationLogs.Count, Is.EqualTo (expected));
    }

    /// <summary>
    /// In case there was only a single modification,
    /// check it is done
    /// </summary>
    /// <param name="session"></param>
    internal static void CheckSingleModificationDone<TModification> (ISession session)
      where TModification : Modification
    {
      IList<TModification> modifications =
        session.CreateCriteria<TModification> ()
        .AddOrder (Order.Asc ("DateTime"))
        .List<TModification> ();
      Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
      Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
    }

    /// <summary>
    /// Check all the modification are done
    /// </summary>
    /// <param name="session"></param>
    /// <param name="expectedNumber">Expected number of modifications</param>
    internal static void CheckAllModificationDone<TModification> (ISession session, int expectedNumber)
      where TModification : Modification
    {
      IList<TModification> modifications =
        session.CreateCriteria<TModification> ()
        .AddOrder (Order.Asc ("DateTime"))
        .List<TModification> ();
      Assert.That (modifications, Has.Count.EqualTo (expectedNumber), "Number of modifications");
      foreach (TModification modification in modifications) {
        Assert.That (modification.AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "modification status");
      }
    }

    /// <summary>
    /// Run MakeAnalysis for the current session
    /// </summary>
    internal static void RunMakeAnalysis<TModification> ()
      where TModification : Modification
    {
      RunMakeAnalysis ();
    }

    /// <summary>
    /// Run the MakeAnalysis method for all the modifications
    /// of the given type
    /// </summary>
    /// <param name="session">not used</param>
    internal static void RunMakeAnalysis<TModification> (ISession session)
      where TModification : Modification
    {
      RunMakeAnalysis<TModification> ();
    }

    /// <summary>
    /// Run the MakeAnalysis method for all the modifications
    /// </summary>
    internal static void RunMakeAnalysis ()
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

    internal static void RunProcessingReasonSlotsAnalysis ()
    {
      var monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
        .FindAll ();
      foreach (var monitoredMachine in monitoredMachines) {
        RunProcessingReasonSlotsAnalysis (monitoredMachine);
      }
    }

    internal static void RunProcessingReasonSlotsAnalysis (IMonitoredMachine machine)
    {
      var analysis = new ProcessingReasonSlotsAnalysis (machine, null);
      analysis.RunOnce (System.Threading.CancellationToken.None, null);
    }

    /// <summary>
    /// Run the MakeAnalysis method on the first pending modification
    /// </summary>
    internal static void RunFirst ()
    {
      {
        IGlobalModification modification = ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .GetFirstPendingModification ();
        if (null != modification) {
          ProcessModification (modification);
          return;
        }
      }
      IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll ();
      foreach (IMachine machine in machines) {
        IMachineModification modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .GetFirstPendingModification (machine);
        if (null != modification) {
          ProcessModification (modification);
        }
      }
    }

    static void ProcessModification (IModification modification)
    {
      if (!modification.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)
        && !modification.AnalysisStatus.IsCompletedSuccessfully ()) {
        ((Modification)modification).MakeAnalysis ();
      }
      if (modification.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)) {
        foreach (IModification subModification in modification.SubModifications) {
          ProcessModification (subModification);
        }
        if (modification.SubModifications.All (m => m.AnalysisStatus.IsCompletedSuccessfully ())) {
          modification.MarkAllSubModificationsCompleted ();
        }
        ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (modification);
      }
    }

    internal static void RunMachineStateTemplateAnalysis (IMachine machine)
    {
      var impactedSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindWithNoMachineObservationState (machine, DateTime.UtcNow, 100000);
      foreach (IObservationStateSlot impactedSlot in impactedSlots) {
        ((ObservationStateSlot)impactedSlot).ProcessTemplate (System.Threading.CancellationToken.None, new UtcDateTimeRange (new LowerBound<DateTime> (null), DateTime.UtcNow),
                                                               null, true, null, null);
      }
    }
  }
}
