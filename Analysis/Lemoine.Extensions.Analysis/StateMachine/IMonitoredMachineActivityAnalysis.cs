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
  /// Monitored machine activity analysis interface
  /// </summary>
  public interface IMonitoredMachineActivityAnalysis
    : IContext<IMonitoredMachineActivityAnalysis>
    , IMachineActivityAnalysis
  {
    /// <summary>
    /// Reference to the monitored machine
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; }

    /// <summary>
    /// Run the analysis of this MachineActivityAnalysis class
    /// without initializing the properties of a new analysis
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunMachineActivityAnalysis (CancellationToken cancellationToken);

    /// <summary>
    /// Facts are retrieved for the activity analysis
    /// </summary>
    IList<IFact> Facts { get; }

    /// <summary>
    /// Run the analysis of the new activity periods
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxNumberOfFacts">Max number of facts or consider a configuration key</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunActivityAnalysis (CancellationToken cancellationToken, int? maxNumberOfFacts = null);

    /// <summary>
    /// Run the analysis of the processing reason slots
    /// 
    /// Method with the default max time
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="range">Optional range</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunProcessingReasonSlotsAnalysis (CancellationToken cancellationToken, UtcDateTimeRange range = null);

    /// <summary>
    /// Run the analysis of the processing reason slots
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime">Max time</param>
    /// <param name="minTime">Minimum time before checking the maximum time</param>
    /// <param name="range">Optional range</param>
    /// <param name="maxLoopNumber">Maximum number of times the loop is run. If null, configuration key is considered</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunProcessingReasonSlotsAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, UtcDateTimeRange range = null, int? maxLoopNumber = null);

    /// <summary>
    /// Run the analysis of the processing reason slots for the last period only
    /// 
    /// Method with the default max time
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="lastPeriod"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunProcessingReasonSlotsAnalysisLastPeriod (CancellationToken cancellationToken, TimeSpan lastPeriod);

    /// <summary>
    /// Run the analysis of the processing reason slots for the last period only
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime">Max time</param>
    /// <param name="minTime">Minimum time before checking the maximum time</param>
    /// <param name="lastPeriod"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunProcessingReasonSlotsAnalysisLastPeriod (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, TimeSpan lastPeriod);

    /// <summary>
    /// Run the detection analysis
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunDetectionAnalysis (CancellationToken cancellationToken);

    /// <summary>
    /// Run the detection analysis
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime">Max time</param>
    /// <param name="minTime">Minimum time before checking the maximum time</param>
    /// <param name="numberOfItems">Number of items ot process. If null, consider a configuration key</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunDetectionAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, int? numberOfItems = null);

    /// <summary>
    /// Process the auto-sequence periods
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunAutoSequenceAnalysis (CancellationToken cancellationToken);

    /// <summary>
    /// Process the auto-sequence periods
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxTime">Max time</param>
    /// <param name="minTime">Minimum time before checking the maximum time</param>
    /// <param name="numberOfItems">Number of items to process, else consider a configuration key</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunAutoSequenceAnalysis (CancellationToken cancellationToken, TimeSpan maxTime, TimeSpan minTime, int? numberOfItems = null);
  }
}
