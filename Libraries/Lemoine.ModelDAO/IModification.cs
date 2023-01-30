// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Modification
  /// </summary>
  public interface IModification: Lemoine.Collections.IDataWithId<long>, ISerializableModel
  {
    /// <summary>
    /// Refers to the modificationreferencedtable
    /// </summary>
    string ModificationType { get; }

    /// <summary>
    /// Associated revision
    /// </summary>
    IRevision Revision { get; set; }

    /// <summary>
    /// Parent modification (global or machine) when applicable
    /// </summary>
    IModification Parent { get; set; }
    
    /// <summary>
    /// Parent global modification when applicable
    /// </summary>
    IGlobalModification ParentGlobal { get; set; }
    
    /// <summary>
    /// Parent global modification when applicable
    /// </summary>
    IMachineModification ParentMachine { get; set; }

    /// <summary>
    /// Should the modification run by the Activity analysis thread ?
    /// </summary>
    bool ActivityAnalysis { get; set; }
    
    /// <summary>
    /// Sub-globalmodifications when applicable
    /// </summary>
    ICollection<IGlobalModification> SubGlobalModifications { get; }
    
    /// <summary>
    /// Sub-machinemodifications when applicable
    /// </summary>
    ICollection<IMachineModification> SubMachineModifications { get; }
    
    /// <summary>
    /// Sub-modifications when applicable (Global or Machine)
    /// </summary>
    IEnumerable<IModification> SubModifications { get; }
    
    /// <summary>
    /// Date/time of the modification
    /// </summary>
    DateTime DateTime { get; set; }
    
    /// <summary>
    /// Initial priority of the modification (w.r.t. other modifications)
    /// high value = high priority
    /// </summary>
    int Priority { get; set; }

    /// <summary>
    /// Priority of the modification (w.r.t. other modifications) in the modificationstatus table
    /// high value = high priority
    /// </summary>
    int StatusPriority { get; set; }

    /// <summary>
    /// Status of the analysis
    /// </summary>
    AnalysisStatus AnalysisStatus { get; }

    /// <summary>
    /// In case of a parent modification, status to take once all the children are completed
    /// </summary>
    AnalysisStatus? NextAnalysisStatus { get; }

    /// <summary>
    /// Date/time
    /// <item>on which the analysis applies</item>
    /// <item>or until when the analysis is done</item>
    /// </summary>
    DateTime? AnalysisAppliedDateTime { get; set; }
    
    /// <summary>
    /// Auto modification
    /// 
    /// Delete the modification when done. Do not use it for auto-reasons
    /// </summary>
    bool Auto { get; set; }

    /// <summary>
    /// Begin date/time of the first analysis
    /// </summary>
    DateTime? AnalysisBegin { get; set; }
    
    /// <summary>
    /// End date/time of the last processed analysis
    /// </summary>
    DateTime? AnalysisEnd { get; set; }
    
    /// <summary>
    /// Begin date/time of the last analysis
    /// </summary>
    DateTime LastAnalysisBegin { get; }
    
    /// <summary>
    /// Number of analysis iterations
    /// </summary>
    int AnalysisIterations { get; set; }
    
    /// <summary>
    /// Total process time for this modification
    /// </summary>
    TimeSpan AnalysisTotalDuration { get; set; }
    
    /// <summary>
    /// Process time of the last iteration
    /// </summary>
    TimeSpan? AnalysisLastDuration { get; set; }
    
    /// <summary>
    /// Analysis completion order
    /// </summary>
    long? AnalysisCompletionOrder { get; set; }
    
    /// <summary>
    /// Analysis step span
    /// </summary>
    TimeSpan? AnalysisStepSpan { get; set; }
    
    /// <summary>
    /// Is there any sub-modifications ? Global or Machine
    /// </summary>
    bool AnalysisSubModifications { get; }
    
    /// <summary>
    /// Is there any sub-globalmodifications ?
    /// </summary>
    bool AnalysisSubGlobalModifications { get; }

    /// <summary>
    /// Is there any sub-globalmodifications ?
    /// </summary>
    bool AnalysisSubMachineModifications { get; }

    /// <summary>
    /// Cancelling the modification is required.
    /// 
    /// After an error, cancel any data that could have been set by the modification or its parent.
    /// The modification will be in error afterwards.
    /// </summary>
    void Cancel ();

    /// <summary>
    /// Apply directly the modification,
    /// while keeping the transient status of the modification
    /// </summary>
    void Apply ();
    
    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// 
    /// null if there is no impacted activity analysis
    /// </summary>
    /// <returns></returns>
    IList<IMachine> GetImpactedActivityAnalysis ();
    
    /// <summary>
    /// Mark the modification with a Timeout
    /// </summary>
    /// <param name="startDateTime">Start date/time of the modification in time out</param>
    void MarkAsTimeout (DateTime startDateTime);
    
    /// <summary>
    /// Mark the modification with a StepTimeout (to be retried with a shorter step span)
    /// </summary>
    void MarkAsStepTimeout ();

    /// <summary>
    /// Mark the modification with ConstraintIntegrityViolation error
    /// </summary>
    void MarkAsConstraintIntegrityViolation ();

    /// <summary>
    /// Mark the modification with DatabaseTimeout error
    /// </summary>
    void MarkAsDatabaseTimeout ();

    /// <summary>
    /// Mark the modification with the status ParentInError.
    /// </summary>
    /// <param name="cancel">Cancel the modification first</param>
    void MarkAsParentInError (bool cancel);

    /// <summary>
    /// Mark all the sub-modifications were completed
    /// 
    /// The status is going from PendingSubModifications to Done
    /// </summary>
    void MarkAllSubModificationsCompleted ();

    /// <summary>
    /// It the main modification transient ?
    /// 
    /// If false, then the analysis status may be updated and sub-modifications created.
    /// If true, the modifications must be processed directly.
    /// </summary>
    bool IsMainModificationTransient ();
    
    /// <summary>
    /// Check if the step timeout is reached.
    /// 
    /// If it is, raise the StepTimeoutException
    /// 
    /// It must be called by the main modification
    /// </summary>
    /// <returns></returns>
    void CheckStepTimeout ();

    /// <summary>
    /// Get a transaction name suffix
    /// </summary>
    /// <returns></returns>
    string GetTransactionNameSuffix ();
  }
}
