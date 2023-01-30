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
  /// DAO interface for IModification.
  /// </summary>
  public interface IModificationDAO
  {
    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="entity"></param>
    void Lock (IModification entity);

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// 
    /// Do not use it in a serializable transaction
    /// </summary>
    /// <param name="entity"></param>
    void UpgradeLock (IModification entity);

    /// <summary>
    /// Make persistent the modification
    /// </summary>
    /// <param name="modification"></param>
    void MakePersistent (IModification modification);
    
    /// <summary>
    /// Find all the modifications (global and machine)
    /// </summary>
    /// <returns></returns>
    IEnumerable<IModification> FindAll ();

    /// <summary>
    /// Find by ID
    /// 
    /// This method is inefficient. Please prefer the specific method of GlobalModificationDAO or MachineModificationDAO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IModification FindById (long id);
    
    /// <summary>
    /// Returns the number of remaining modifications to process
    /// before a specified modification is completed
    /// </summary>
    /// <param name="modificationId"></param>
    /// <param name="createNewAnalysisStatusBefore"></param>
    /// <returns></returns>
    double GetNumberOfRemainingModifications (long modificationId, bool createNewAnalysisStatusBefore = true);

    /// <summary>
    /// Get the completion of a modification
    /// 
    /// <item>0: not yet started</item>
    /// <item>1: completed</item>
    /// <item>1: in case the analysis status is pending (arbitrary value)</item>
    /// </summary>
    /// <param name="modification">not null</param>
    /// <returns></returns>
    double GetCompletion (IModification modification);

    /// <summary>
    /// Get the next completion order
    /// </summary>
    /// <returns></returns>
    long GetNextCompletionOrder ();

    /// <summary>
    /// Get a number of modifications with a specified analysis status
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    int GetNumber (AnalysisStatus analysisStatus);
    
    /// <summary>
    /// Get all modifications not completed that have a revision
    /// </summary>
    /// <param name="application">Name of the application that created the revision (can be null or empty)</param>
    /// <param name="minId">Minimum id to browse (strict)</param>
    /// <returns></returns>
    IEnumerable<IModification> FindNotCompletedWithRevision(string application, long minId);
    
    /// <summary>
    /// Get all modifications related to a revision
    /// </summary>
    /// <param name="revision">Cannot be null</param>
    /// <param name="minId">Minimum ID of the modifications returned (strict)</param>
    /// <returns></returns>
    IEnumerable<IModification> FindByRevision(IRevision revision, long minId);

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
  }
}
