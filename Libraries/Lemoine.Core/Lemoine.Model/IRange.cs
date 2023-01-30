// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// 
  /// </summary>
  public interface IRange<T>
    where T : struct, IComparable<T>, IComparable, IEquatable<T>
  {
    /// <summary>
    /// Lower bound of range
    /// </summary>
    LowerBound<T> Lower { get; }

    /// <summary>
    /// Is lower bound inclusive ?
    /// </summary>
    bool LowerInclusive { get; }

    /// <summary>
    /// Upper bound of range
    /// </summary>
    UpperBound<T> Upper { get; }

    /// <summary>
    /// Is upper bound inclusive ?
    /// </summary>
    bool UpperInclusive { get; }

    /// <summary>
    /// Check if the range is empty
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
  }
}
