// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.StateMachine;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lemoine.Extensions.Analysis.StateMachine
{
  /// <summary>
  /// Machine activity analysis interface
  /// </summary>
  public interface IMachineActivityAnalysis: IContext<IMachineActivityAnalysis>
  {
    /// <summary>
    /// Reference to the machine
    /// </summary>
    IMachine Machine { get; }

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
    /// Manage the machine state templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool ManageMachineStateTemplates (CancellationToken cancellationToken);

    /// <summary>
    /// Manage the machine state templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <param name="minTime">Minimum time before checking the maximum time is reached</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool ManageMachineStateTemplates (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime);

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
    /// <param name="minTime">Minimum time before checking the maximum time is reached</param>
    /// <param name="minPastPriority">Minimum priority of the past pending modifications to process</param>
    /// <param name="minPresentPriority">Minimum priority of the present pending modifications to process. Must be greater or equal minPastPriority</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunPendingModificationsAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, int minPastPriority, int minPresentPriority);

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
    /// <param name="minTime">Minimum time before checking the maximum time</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool CleanFlaggedModifications (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime);

    /// <summary>
    /// Run the production analysis
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunProductionAnalysis (CancellationToken cancellationToken);

    /// <summary>
    /// Run the production analysis
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <param name="minTime">Minimum time before checking the maximum time</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunProductionAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunOperationSlotSplitAnalysis (CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime"></param>
    /// <param name="minTime">Minimum time before checking the maximum time</param>
    /// <param name="period">Period to analyze. If null, consider a configuration key</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunOperationSlotSplitAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, TimeSpan? period = null);

    /// <summary>
    /// Get the states that are associated to the analysis extensions
    /// </summary>
    /// <returns></returns>
    IEnumerable<IState<T>> GetExtensionAnalysisStates<T> ()
      where T: IMachineActivityAnalysis, IContext<T>;
  }
}
