// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Common interface to CycleProgressResponse and OperationProgressResponse
  /// </summary>
  public interface IProgressResponse
  {
    /// <summary>
    /// Current operation
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Progress by machine module
    /// 
    /// nullable
    /// </summary>
    IDictionary<IMachineModule, IProgressByMachineModuleResponse> MachineModuleProgress { get; }

    /// <summary>
    /// Start date/time, if applicable
    /// </summary>
    DateTime? StartDateTime { get; }

    /// <summary>
    /// Estimated date/time of the next stop, if any
    /// </summary>
    DateTime? EstimatedNextStopDateTime { get; }

    /// <summary>
    /// Estimated date/time of the next pallet change, if any
    /// </summary>
    DateTime? EstimatedAutoPalletChangeDateTime { get; }

    /// <summary>
    /// Estimated time of the operation or cycle end (in the future)
    /// </summary>
    DateTime? EstimatedEndDateTime { get; }

    /// <summary>
    /// Completion % (between 0 and 1) of the cycle considering in priority the operation duration
    /// </summary>
    double? Completion { get; }

    /// <summary>
    /// Completion % (between 0 and 1) of the cycle considering in priority the machining periods
    /// </summary>
    double? MachiningCompletion { get; }

    /// <summary>
    /// Date/time of the latest cycle completion if there is currently no active cycle
    /// </summary>
    DateTime? CompletionDateTime { get; }

    /// <summary>
    /// Warnings
    /// </summary>
    IList<string> Warnings { get; }

    /// <summary>
    /// Errors
    /// </summary>
    IList<string> Errors { get; }

    /// <summary>
    /// Notices on the data
    /// </summary>
    IList<string> Notices { get; }
  }

  /// <summary>
  /// Machine module part of the response
  /// </summary>
  public interface IProgressByMachineModuleResponse
  {
    /// <summary>
    /// Current sequence
    /// </summary>
    ISequence CurrentSequence { get; }

    /// <summary>
    /// Duration of the current sequence
    /// </summary>
    TimeSpan? CurrentSequenceStandardTime { get; }

    /// <summary>
    /// Begin date/time of the current sequence
    /// </summary>
    DateTime? CurrentSequenceBeginDateTime { get; }

    /// <summary>
    /// Already elapsed time in the current sequence
    /// </summary>
    TimeSpan? CurrentSequenceElapsed { get; }

    /// <summary>
    /// List of sequences to process for this machine module
    /// </summary>
    IEnumerable<ISequence> Sequences { get; }

    /// <summary>
    /// Estimated date/time of the next stop, if any
    /// </summary>
    DateTime? EstimatedNextStopDateTime { get; }

    /// <summary>
    /// Name of the next stop, if any, else null
    /// </summary>
    ISequence NextStop { get; }

    /// <summary>
    /// Estimated date/time of the next pallet change, if any
    /// </summary>
    DateTime? EstimatedAutoPalletChangeDateTime { get; }

    /// <summary>
    /// Estimated time of the next cycle end (in the future)
    /// </summary>
    DateTime? EstimatedEndDateTime { get; }

    /// <summary>
    /// Machining duration for this machine module considering the sum of the sequence duration
    /// </summary>
    TimeSpan? MachiningDuration { get; }

    /// <summary>
    /// Completion % (between 0 and 1) of the cycle
    /// </summary>
    double? Completion { get; }
  }
}
