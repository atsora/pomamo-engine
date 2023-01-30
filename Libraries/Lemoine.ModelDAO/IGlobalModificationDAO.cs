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
  /// DAO interface for IGlobalModification.
  /// </summary>
  public interface IGlobalModificationDAO: IGenericDAO<IGlobalModification, long>
  {
    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// </summary>
    /// <param name="entity"></param>
    void UpgradeLock (IGlobalModification entity);

    /// <summary>
    /// Returns the number of remaining modifications to process
    /// before a specified modification is completed
    /// </summary>
    /// <param name="globalModification"></param>
    /// <param name="createNewAnalysisStatusBefore">not used here</param>
    /// <returns></returns>
    double GetNumberOfRemainingModifications (IGlobalModification globalModification, bool createNewAnalysisStatusBefore = true);
    
    /// <summary>
    /// Returns the n first old pending modifications before a specified date/time
    /// that have either no analysis status, or a new/pending analysis status
    /// and that are "greater than" (lastModificationId, lastPriority)
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="before"></param>
    /// <param name="maxResults"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    IEnumerable<IGlobalModification> GetPastPendingModifications (long lastModificationId,
                                                                  int lastPriority,
                                                                  DateTime before,
                                                                  int maxResults,
                                                                  int minPriority);
    
    /// <summary>
    /// Returns the first modification
    /// which has either no analysis status, or a new/pending analysis status
    /// </summary>
    /// <returns></returns>
    IGlobalModification GetFirstPendingModification();

    /// <summary>
    /// Returns the first modification "greater than"
    /// (lastModificationId, lastPriority)
    /// which has either no analysis status, or a new/pending analysis status
    /// "greather than" the specified:
    /// <item>lastModificationId</item>
    /// <item>lastPriority</item>
    /// 
    /// This must be used in a PendingGlobalModificationAnalysis
    /// where the machine modifications are processed separately
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    IGlobalModification GetFirstPendingModification(long lastModificationId,
                                                    int lastPriority,
                                                    int minPriority);

    /// <summary>
    /// Returns the first modification "greater than"
    /// (lastModificationId, lastPriority)
    /// which has either no analysis status, or a new/pending analysis status
    /// "greather than" the specified:
    /// <item>lastModificationId</item>
    /// <item>lastPriority</item>
    /// 
    /// This must be used in a PendingGlobalMachineModificationAnalysis
    /// where the global and machine modifications are processed simultaneously
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    IGlobalModification GetFirstPendingGlobalMachineModification(long lastModificationId,
                                                                 int lastPriority,
                                                                 int minPriority);
    
    /// <summary>
    /// Get the sub-modifications of the specified modification
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    IEnumerable<IModification> GetNotCompletedSubModifications (IGlobalModification modification);
    
    /// <summary>
    /// Get the sub-globalmodifications of the specified modification
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    IEnumerable<IGlobalModification> GetNotCompletedSubGlobalModifications (IGlobalModification modification);
    
    /// <summary>
    /// Get the sub-machinemodifications of the specified modification
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    IEnumerable<IMachineModification> GetNotCompletedSubMachineModifications (IGlobalModification modification);

    /// <summary>
    /// Check if there are some sub-machine modifications
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    bool HasNotCompletedSubMachineModifications (IGlobalModification modification);
    
    /// <summary>
    /// Check if there are some sub-machine modifications
    /// that are not completed yet for a specific machine
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="machine"></param>
    /// <param name="createNewAnalysisStatus">Create the 'New' analysis status first. This requires to have a read-write transaction</param>
    /// <returns></returns>
    bool HasNotCompletedSubMachineModifications (IGlobalModification modification, IMachine machine,
                                                 bool createNewAnalysisStatus);
    
    /// <summary>
    /// Delete the modifications of the specified analysis status
    /// </summary>
    /// <param name="analysisStatus"></param>
    void Delete (AnalysisStatus analysisStatus);
    
    /// <summary>
    /// Delete the modifications of the specified analysis status that are completed before a specific date/time
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <param name="maxCompletionDateTime"></param>
    void Delete (AnalysisStatus analysisStatus, DateTime maxCompletionDateTime);

    /// <summary>
    /// Get the modifications in error strictly after the specified completion order
    /// </summary>
    /// <param name="completionOrder"></param>
    /// <returns></returns>
    IList<IGlobalModification> GetInErrorStrictlyAfter (int completionOrder);
    
    /// <summary>
    /// Get a number of modifications with a specified analysis status
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    int GetNumber (AnalysisStatus analysisStatus);
    
    /// <summary>
    /// Get the maximum modificationId
    /// </summary>
    /// <returns></returns>
    long? GetMaxModificationId ();

    /// <summary>
    /// Get the maximum modificationId asynchronously
    /// </summary>
    /// <returns></returns>
    Task<long?> GetMaxModificationIdAsync ();

    /// <summary>
    /// Get all global modifications not completed that have a revision
    /// </summary>
    /// <param name="application">Name of the application that created the revision (can be null or empty)</param>
    /// <param name="minId">Minimum id to browse (strict)</param>
    /// <returns></returns>
    IEnumerable<IGlobalModification> FindNotCompletedWithRevision(string application, long minId);
    
    /// <summary>
    /// Get all global modifications related to a revision
    /// </summary>
    /// <param name="revision">Cannot be null</param>
    /// <param name="minId">Minimum ID of the modifications returned (strict)</param>
    /// <returns></returns>
    IList<IGlobalModification> FindByRevision(IRevision revision, long minId);
  }
}
