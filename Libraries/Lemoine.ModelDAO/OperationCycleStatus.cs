// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// OperationCycleStatus: bound estimates
  /// </summary>
  [Flags]
  public enum OperationCycleStatus
  {
    /// <summary>
    /// begin of cycle is estimated
    /// </summary>
    BeginEstimated = 1,
    /// <summary>
    /// end of cycle is estimated
    /// </summary>
    EndEstimated = 2, // 1 << 1
  };
  
  /// <summary>
  /// Extensions to OperationCycleStatus
  /// </summary>
  public static class OperationCycleStatusMethodExtensions
  {
    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this OperationCycleStatus t, OperationCycleStatus other)
    {
      return other == (t & other);
    }
    
    /// <summary>
    /// Add a flag to another one (which may be null)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static OperationCycleStatus Add (this OperationCycleStatus t, OperationCycleStatus? other)
    {
      if (other.HasValue) {
        return t | other.Value;
      }
      else {
        return t;
      }
    }
    
    /// <summary>
    /// Remove a flag
    /// </summary>
    /// <param name="t"></param>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public static OperationCycleStatus Remove (this OperationCycleStatus t, OperationCycleStatus toRemove)
    {
      return t & ~toRemove;
    }
  }
}
