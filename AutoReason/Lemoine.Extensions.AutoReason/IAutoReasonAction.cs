// Copyright (C) 2025 Atsora Solutions

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Action to apply an auto-reason process
  /// 
  /// <see cref="IReasonAction"/>
  /// <see cref="IStateAction"/>
  /// </summary>
  public interface IAutoReasonAction
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
    /// Commit number if you want to apply the delayed actions in different transactions
    /// </summary>
    int CommitNumber { get; }
  }
}
