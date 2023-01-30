// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.Threading;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Analysis.StateMachine
{
  /// <summary>
  /// Analysis with an internal state machine
  /// </summary>
  public interface IStateMachineAnalysis
    : IChecked
  {
    /// <summary>
    /// Performance suffix
    /// </summary>
    string PerfSuffix { get; }

    /// <summary>
    /// Check if a pause in the analysis is requested
    /// </summary>
    /// <returns></returns>
    bool IsPauseRequested ();

    /// <summary>
    /// Get the Id of the modification that triggered the pause
    /// 
    /// If no pause is requested, 0 is returned
    /// </summary>
    /// <returns></returns>
    Int64 GetPauseTriggeringModificationId ();

    /// <summary>
    /// Set the exit is requested
    /// </summary>
    void SetExitRequested ();

    /// <summary>
    /// Exit requested
    /// </summary>
    bool ExitRequested { get; }

    /// <summary>
    /// Check if an exception requires to exit
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    bool IsExitRequired (Exception ex);

    /// <summary>
    /// Add an exception from one of the state
    /// </summary>
    /// <param name="ex"></param>
    void AddStateException (Exception ex);

    /// <summary>
    /// Get the state exceptions
    /// </summary>
    IEnumerable<Exception> StateExceptions { get; }
  }
}
