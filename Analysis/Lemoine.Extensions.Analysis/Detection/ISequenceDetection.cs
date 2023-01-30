// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis.Detection
{
  /// <summary>
  /// Interface of SequenceDetection process class
  /// </summary>
  public interface ISequenceDetection
  {
    /// <summary>
    /// Associated machine module
    /// </summary>
    IMachineModule MachineModule { get; }

    /// <summary>
    /// Start an auto-only operation
    /// 
    /// It must be run only at start of an operation cycle (not in a middle of an operation cycle execution)
    /// 
    /// Update the autosequence or sequenceslot table accordingly
    /// 
    /// It contains a top transaction and must not be run inside a transaction
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="dateTime"></param>
    void StartAutoOnlyOperation (IOperation operation, DateTime dateTime);

    /// <summary>
    /// Start a new sequence
    /// 
    /// Update the autosequence or sequenceslot table accordingly
    /// 
    /// This method contains top transactions and must not be run inside a transaction
    /// </summary>
    /// <param name="sequence">not null</param>
    /// <param name="dateTime"></param>
    void StartSequence (ISequence sequence, DateTime dateTime);

    /// <summary>
    /// Stop an sequence, e.g. because the end of an ISO file was reached
    /// 
    /// Update the autosequence and sequenceslot tables accordingly
    /// </summary>
    /// <param name="dateTime"></param>
    void StopSequence (DateTime dateTime);
  }
}
