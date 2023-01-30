// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Analysis status
  /// </summary>
  public enum AnalysisStatus
  {
    /// <summary>
    /// New
    /// </summary>
    New = 0,
    /// <summary>
    /// Pending
    /// </summary>
    Pending = 1,
    /// <summary>
    /// Done
    /// </summary>
    Done = 3,
    /// <summary>
    /// In error
    /// </summary>
    Error = 4,
    /// <summary>
    /// Obsolete
    /// 
    /// The data could not be processed previously,
    /// and is now considered too old
    /// </summary>
    Obsolete = 5,
    /// <summary>
    /// Delete
    /// 
    /// Delete the modification when you get the possibility to do it
    /// </summary>
    Delete = 6,
    /// <summary>
    /// Timeout
    /// 
    /// The request to the database ended with a timeout error (temporary)
    /// </summary>
    Timeout = 7,
    /// <summary>
    /// Constraint integrity violation
    /// 
    /// The request to the database ended with a constraint integrity violation
    /// </summary>
    ConstraintIntegrityViolation = 8,
    /// <summary>
    /// In progress
    /// 
    /// To split the modifications in several transactions, but the modification is not completed yet
    /// </summary>
    InProgress = 9,
    /// <summary>
    /// Sub-modifications
    /// 
    /// This modification contains some sub-modifications that are not completed yet although the main modification is completed
    /// </summary>
    PendingSubModifications = 10,
    /// <summary>
    /// Step time out
    /// 
    /// The analysis may retried after shorten the step process duration
    /// </summary>
    StepTimeout = 11,
    /// <summary>
    /// Cancel
    /// 
    /// The analysis was manually cancelled
    /// </summary>
    Cancel = 12,
    /// <summary>
    /// The modification is done, but purge it after some time, most of the time because it is an automatic modification
    /// </summary>
    DonePurge = 13,
    /// <summary>
    /// The modification is not applicable (for example the dynamic time can't be applied)
    /// </summary>
    NotApplicable = 14,
    /// <summary>
    /// This sub-modification triggers the ancestor modification must be set to 
    /// NotApplicable
    /// </summary>
    AncestorNotApplicable = 15,
    /// <summary>
    /// This sub-modification triggers the ancestor modification must be set to 
    /// Error
    /// </summary>
    AncestorError = 16,
    /// <summary>
    /// Cancelled because of a timeout error
    /// </summary>
    TimeoutCanceled = 17,
    /// <summary>
    /// A child modification is cancelled because its parent ended in error
    /// </summary>
    ParentInError = 18,
    /// <summary>
    /// A modification was completed but one of its requested child modification is in error
    /// </summary>
    ChildInError = 19,
    /// <summary>
    /// A database in timeout was reached
    /// </summary>
    DatabaseTimeout = 20,
    /// <summary>
    /// Cancelled because of a database timeout error
    /// </summary>
    DatabaseTimeoutCanceled = 21,
  };

  /// <summary>
  /// Extensions to AnalysisStatus
  /// </summary>
  public static class AnalysisStatusExtensions
  {
    /// <summary>
    /// Is the analysis in progress ?
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public static bool IsInProgress (this AnalysisStatus analysisStatus)
    {
      return analysisStatus.Equals (AnalysisStatus.InProgress)
        || analysisStatus.Equals (AnalysisStatus.PendingSubModifications)
        || analysisStatus.Equals (AnalysisStatus.StepTimeout)
        || analysisStatus.Equals (AnalysisStatus.Timeout)
        || analysisStatus.Equals (AnalysisStatus.DatabaseTimeout);
    }

    /// <summary>
    /// Is the analysis new ?
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public static bool IsNew (this AnalysisStatus analysisStatus)
    {
      return analysisStatus.Equals (AnalysisStatus.New);
    }

    /// <summary>
    /// Return true if the modification is in progress or new
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public static bool IsNotCompleted (this AnalysisStatus analysisStatus)
    {
      return analysisStatus.IsInProgress () || analysisStatus.IsNew ();
    }

    /// <summary>
    /// Has the analysis been completed successfully ?
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public static bool IsCompletedSuccessfully (this AnalysisStatus analysisStatus)
    {
      return analysisStatus.Equals (AnalysisStatus.Done)
        || analysisStatus.Equals (AnalysisStatus.DonePurge)
        || analysisStatus.Equals (AnalysisStatus.NotApplicable)
        || analysisStatus.Equals (AnalysisStatus.AncestorNotApplicable);
      ;
    }

    /// <summary>
    /// Is the modification completed but in error ?
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public static bool IsInError (this AnalysisStatus analysisStatus)
    {
      switch (analysisStatus) {
      case AnalysisStatus.Error:
      case AnalysisStatus.ConstraintIntegrityViolation:
      case AnalysisStatus.AncestorError:
      case AnalysisStatus.TimeoutCanceled:
      case AnalysisStatus.ParentInError:
      case AnalysisStatus.ChildInError:
      case AnalysisStatus.DatabaseTimeoutCanceled:
        return true;
      default:
        return false;
      }
    }

    /// <summary>
    /// Textual description that can be displayed to the user or in the log
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public static string GetDescription (this AnalysisStatus analysisStatus)
    {
      var message = "";

      switch (analysisStatus) {
      case AnalysisStatus.New:
        message = "modification not processed yet";
        break;
      case AnalysisStatus.Pending:
        message = "modification processed, waiting conditions to be applied";
        break;
      case AnalysisStatus.Done:
        message = "modification done";
        break;
      case AnalysisStatus.Error:
        message = "modification stopped: in error";
        break;
      case AnalysisStatus.Obsolete:
        message = "modification stopped: obsolete data";
        break;
      case AnalysisStatus.Delete:
        message = "modification to delete";
        break;
      case AnalysisStatus.Timeout:
        message = "timeout in modification analysis: retry";
        break;
      case AnalysisStatus.ConstraintIntegrityViolation:
        message = "modification stopped: constraint integrity violation in the database";
        break;
      case AnalysisStatus.InProgress:
        message = "modification waiting for child modifications to be processed";
        break;
      case AnalysisStatus.PendingSubModifications:
        message = "modification processed with child modifications not completed yet";
        break;
      case AnalysisStatus.StepTimeout:
        message = "heavy modification, will be split";
        break;
      case AnalysisStatus.Cancel:
        message = "modification canceled";
        break;
      case AnalysisStatus.DonePurge:
        message = "modification done, purge needed";
        break;
      case AnalysisStatus.NotApplicable:
        message = "modification not applicable";
        break;
      case AnalysisStatus.AncestorNotApplicable:
        message = "sub-modification that must trigger the ancestor to be in NotApplicable status";
        break;
      case AnalysisStatus.AncestorError:
        message = "sub-modification that must trigger the ancestor to be in Error status";
        break;
      case AnalysisStatus.TimeoutCanceled:
        message = "canceled because of a time out";
        break;
      case AnalysisStatus.ParentInError:
        message = "canceled because its parent was in error";
        break;
      case AnalysisStatus.ChildInError:
        message = "initially successfully completed but one of the requested child modification is in error";
        break;
      case AnalysisStatus.DatabaseTimeout:
        message = "a timeout in database was reached";
        break;
      case AnalysisStatus.DatabaseTimeoutCanceled:
        message = "canceled because of a database time out";
        break;
      }

      return message;
    }
  }
}
