// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.StateMachine;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lemoine.Extensions.Analysis.StateMachine
{
  /// <summary>
  /// Global analysis interface
  /// </summary>
  public interface IGlobalAnalysis : IContext<IGlobalAnalysis>
  {
    /// <summary>
    /// Is the global analysis run for the first time ?
    /// </summary>
    bool FirstRun { get; set; }

    /// <summary>
    /// Set exit is requested
    /// </summary>
    void SetExitRequested ();

    /// <summary>
    /// Run all the states
    /// </summary>
    /// <param name="cancellationToken"></param>
    void MakeAnalysis (CancellationToken cancellationToken);

    /// <summary>
    /// Initialize the class
    /// 
    /// Return false if it was interrupted, so if it is not fully initialized (may be retried the next time)
    /// </summary>
    /// <returns>Initialization completed</returns>
    bool Initialize ();

    /// <summary>
    /// Reset the present value
    /// </summary>
    /// <returns>if false, interrupt the state machine</returns>
    bool ResetPresent ();

    /// <summary>
    /// Manage the day templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool ManageDayTemplates (CancellationToken cancellationToken);

    /// <summary>
    /// Manage the day templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool ManageDayTemplates (CancellationToken cancellationToken, TimeSpan maxTime);

    /// <summary>
    /// Manage the week numbers in the day slots
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    bool ManageWeekNumbers (CancellationToken cancellationToken);

    /// <summary>
    /// Manage the shift templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool ManageShiftTemplates (CancellationToken cancellationToken);

    /// <summary>
    /// Manage the shift templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool ManageShiftTemplates (CancellationToken cancellationToken, TimeSpan maxTime);

    /// <summary>
    /// Run the analysis of the pending modifications
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="minPastPriority">Minimum priority of the past pending modifications to process</param>
    /// <param name="minPresentPriority">Minimum priority of the present pending modifications to process. Must be greater or equal minPastPriority</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunPendingModificationsAnalysis (CancellationToken cancellationToken, int minPastPriority, int minPresentPriority);

    /// <summary>
    /// Run the analysis of the pending modifications
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <param name="minPastPriority">Minimum priority of the past pending modifications to process</param>
    /// <param name="minPresentPriority">Minimum priority of the present pending modifications to process. Must be greater or equal minPastPriority</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunPendingModificationsAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, int minPastPriority, int minPresentPriority);

    /// <summary>
    /// Check if cleaning the flagged modifications is required
    /// </summary>
    /// <returns></returns>
    bool IsCleanFlaggedModificationsRequired ();

    /// <summary>
    /// Clean the flagged modifications
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    bool CleanFlaggedModifications (CancellationToken cancellationToken);

    /// <summary>
    /// Clean the flagged modifications
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool CleanFlaggedModifications (CancellationToken cancellationToken, TimeSpan maxTime);
  }
}
