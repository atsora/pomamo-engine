// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineModification.
  /// </summary>
  public interface IMachineModificationDAO : IGenericByMachineDAO<IMachineModification, long>
  {
    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// </summary>
    /// <param name="entity"></param>
    void UpgradeLock (IMachineModification entity);

    /// <summary>
    /// Returns the number of remaining modifications to process
    /// before a specified modification is completed
    /// </summary>
    /// <param name="machineModification"></param>
    /// <param name="createNewAnalysisStatusBefore"></param>
    /// <returns></returns>
    double GetNumberOfRemainingModifications (IMachineModification machineModification, bool createNewAnalysisStatusBefore = true);

    /// <summary>
    /// Returns the n first old pending modifications before a specified date/time
    /// that have either no analysis status, or a new/pending analysis status
    /// and that are "greater than" (lastModificationId, lastPriority)
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="before"></param>
    /// <param name="maxResults"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    IEnumerable<IMachineModification> GetPastPendingModifications (IMachine machine,
                                                                   long lastModificationId,
                                                                   int lastPriority,
                                                                   DateTime before,
                                                                   int maxResults,
                                                                   int minPriority);

    /// <summary>
    /// Returns the first modification
    /// which has either no analysis status, or a new/pending analysis status
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IMachineModification GetFirstPendingModification (IMachine machine);

    /// <summary>
    /// Returns the first modification "greater than"
    /// (lastModificationId, lastPriority)
    /// which has either no analysis status, or a new/pending analysis status
    /// "greather than" the specified:
    /// <item>machine</item>
    /// <item>lastModificationId</item>
    /// <item>lastPriority</item>
    /// 
    /// This must be used in a PendingMachineModificationAnalysis
    /// where the global modifications are processed separately
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    IMachineModification GetFirstPendingModification (IMachine machine,
                                                      long lastModificationId,
                                                      int lastPriority,
                                                      int minPriority);

    /// <summary>
    /// Returns the first modification "greater than"
    /// (lastModificationId, lastPriority)
    /// which has either no analysis status, or a new/pending analysis status
    /// "greather than" the specified:
    /// <item>machine</item>
    /// <item>lastModificationId</item>
    /// <item>lastPriority</item>
    /// 
    /// This must be used in a PendingGlobalMachineModificationAnalysis
    /// where the global and machine modifications are processed simulataneously
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    IMachineModification GetFirstPendingGlobalMachineModification (IMachine machine,
                                                                   long lastModificationId,
                                                                   int lastPriority,
                                                                   int minPriority);

    /// <summary>
    /// Get the sub-modifications of the specified modification
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    IEnumerable<IModification> GetNotCompletedSubModifications (IMachineModification modification);

    /// <summary>
    /// Get the sub-globalmodifications of the specified modification
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    IEnumerable<IGlobalModification> GetNotCompletedSubGlobalModifications (IMachineModification modification);

    /// <summary>
    /// Get the sub-machinemodifications of the specified modification
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    IEnumerable<IMachineModification> GetNotCompletedSubMachineModifications (IMachineModification modification);

    /// <summary>
    /// Get the sub-machinemodifications of the specified modification
    /// that are not completed yet and for the specified machine Id
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="modification"></param>
    /// <returns></returns>
    IEnumerable<IMachineModification> GetNotCompletedSubMachineModifications (IMachine machine,
                                                                              IMachineModification modification);

    /// <summary>
    /// Check if there are some sub-machine modifications
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    bool HasNotCompletedSubMachineModifications (IMachineModification modification);

    /// <summary>
    /// Check if there are some sub-machine modifications
    /// that are not completed yet for a specific machine
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="machine"></param>
    /// <param name="createNewAnalysisStatus">Create the 'New' analysis status first. This requires to have a read-write transaction</param>
    /// <returns></returns>
    bool HasNotCompletedSubMachineModifications (IMachineModification modification, IMachine machine,
                                                 bool createNewAnalysisStatus);

    /// <summary>
    /// Check if there are some sub-global modifications
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    bool HasNotCompletedGlobalModifications (IMachineModification modification);

    /// <summary>
    /// Delete the modifications of the specified analysis status
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="analysisStatus"></param>
    /// <param name="checkedThread">not null (except in unit tests)</param>
    /// <param name="itemNumberByStep"></param>
    /// <param name="maxNumberOfModifications">approximative...</param>
    /// <param name="maxRunningDateTime">optional</param>
    /// <returns>Completed</returns>
    bool Delete (IMachine machine, AnalysisStatus analysisStatus, Lemoine.Threading.IChecked checkedThread, int itemNumberByStep, int maxNumberOfModifications, DateTime? maxRunningDateTime);

    /// <summary>
    /// Delete the modifications of the specified analysis status that were completed before a specified date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="analysisStatus"></param>
    /// <param name="maxCompletionDateTime"></param>
    /// <param name="checkedThread">not null</param>
    /// <param name="itemNumberByStep"></param>
    /// <param name="maxNumberOfModifications">approximative...</param>
    /// <param name="maxRunningDateTime">optional</param>
    /// <returns>Completed</returns>
    bool Delete (IMachine machine, AnalysisStatus analysisStatus, DateTime maxCompletionDateTime, Lemoine.Threading.IChecked checkedThread, int itemNumberByStep, int maxNumberOfModifications, DateTime? maxRunningDateTime);

    /// <summary>
    /// Get the modifications in error strictly after the specified completion order
    /// </summary>
    /// <param name="completionOrder"></param>
    /// <returns></returns>
    IList<IMachineModification> GetInErrorStrictlyAfter (int completionOrder);

    /// <summary>
    /// Get a number of modifications with a specified analysis status
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    int GetNumber (AnalysisStatus analysisStatus);

    /// <summary>
    /// Get the maximum modificationId from the specified machine
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    long? GetMaxModificationId (IMachine machine);

    /// <summary>
    /// Get the maximum modificationId from the specified machine asynchronously
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    Task<long?> GetMaxModificationIdAsync (IMachine machine);

    /// <summary>
    /// Create the analysis status row for all the new modifications associated to the specified machine
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="serializable">is the transaction serializable ? Set false if unsafe. False may be set if the returned value is not checked</param>
    /// <param name="minModificationId">minimum modification id to consider (for optimization)</param>
    /// <param name="limit">Maximum number of items to insert</param>
    /// <param name="limitReached">The limit was reached</param>
    /// <returns>maximum modification id that was inserted (or minModificationId)</returns>
    long CreateNewAnalysisStatus (IMachine machine, bool serializable, long minModificationId, long limit, out bool limitReached);

    /// <summary>
    /// Create rows in analysisstatus tables for the new modifications that don't have any yet.
    /// 
    /// No limit version
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="serializable">is the transaction serializable ? Set false if unsafe. False may be set if the returned value is not checked</param>
    /// <param name="minModificationId">minimum modification id to consider (for optimization)</param>
    /// <returns>Maximum modification id that was inserted</returns>
    long CreateNewAnalysisStatusNoLimit (IMachine machine, bool serializable, long minModificationId);

    /// <summary>
    /// Get all machine modifications not completed that have a revision
    /// </summary>
    /// <param name="application">Name of the application that created the revision (can be null or empty)</param>
    /// <param name="minId">Minimum id to browse (strict)</param>
    /// <returns></returns>
    IEnumerable<IMachineModification> FindNotCompletedWithRevision (string application, long minId);

    /// <summary>
    /// Get all machine modifications related to a revision
    /// </summary>
    /// <param name="revision">Cannot be null</param>
    /// <param name="minId">Minimum ID of the modifications returned (strict)</param>
    /// <returns></returns>
    IList<IMachineModification> FindByRevision (IRevision revision, long minId);
  }
}
