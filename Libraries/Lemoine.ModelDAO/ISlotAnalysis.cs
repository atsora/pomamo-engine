// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the slot analysis
  /// 
  /// It is used when a global slot must be refined by machine / machine module
  /// </summary>
  public interface ISlotAnalysis
  {
    /// <summary>
    /// Status of the analysis
    /// </summary>
    AnalysisStatus AnalysisStatus { get; }
    
    /// <summary>
    /// Date/time
    /// <item>on which the analysis applies</item>
    /// <item>or until when the analysis is done</item>
    /// </summary>
    DateTime? AnalysisAppliedDateTime { get; set; }
    
    /// <summary>
    /// Analysis step span
    /// </summary>
    TimeSpan? AnalysisStepSpan { get; set; }
    
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
    /// Total process time for this slot analysis
    /// </summary>
    TimeSpan AnalysisTotalDuration { get; set; }
    
    /// <summary>
    /// Process time of the last iteration
    /// </summary>
    TimeSpan? AnalysisLastDuration { get; set; }
    
    /// <summary>
    /// Analyze
    /// </summary>
    void Analyze ();
    
    /// <summary>
    /// Reset the analysis status:
    /// the slot must be analyzed again
    /// </summary>
    void ResetAnalysisStatus ();
    
    /// <summary>
    /// Mark the slot analysis with a Timeout
    /// </summary>
    void MarkAsTimeout ();
    
    /// <summary>
    /// Mark the slot analysis with a StepTimeout (to be retried with a shorter step span)
    /// </summary>
    void MarkAsStepTimeout ();

    /// <summary>
    /// Mark the modification with ConstraintIntegrityViolation error
    /// </summary>
    void MarkAsConstraintIntegrityViolation ();
  }
}
