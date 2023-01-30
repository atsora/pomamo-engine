// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Action to update an auto-reason state
  /// </summary>
  public interface IStateAction
  {
    /// <summary>
    /// Name of the action
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Apply the action (update an auto-reason state)
    /// 
    /// Done in a read/write transaction with a ReadCommitted transaction level
    /// </summary>
    void Run ();

    /// <summary>
    /// Reset some internal values in case of failure
    /// </summary>
    void Reset ();
  }
}
