// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Transaction level
  /// </summary>
  public enum TransactionLevel {
    /// <summary>
    /// Default
    /// </summary>
    Default,
    /// <summary>
    /// Serializable
    /// </summary>
    Serializable,
    /// <summary>
    /// Repeatable read
    /// </summary>
    RepeatableRead,
    /// <summary>
    /// Read committed
    /// </summary>
    ReadCommitted,
    /// <summary>
    /// Read uncommitted
    /// </summary>
    ReadUncommitted
  };
}
