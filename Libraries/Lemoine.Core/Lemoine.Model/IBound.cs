// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Bound interface
  /// </summary>
  public interface IBound<T>
  {
    /// <summary>
    /// Is the value null ? Does it correspond to infinity ?
    /// </summary>
    bool HasValue { get; }

    /// <summary>
    /// Return the value if it is not null
    /// 
    /// Else an exception is raised
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Bound type
    /// <item>Lower: null corresponds to -oo</item>
    /// <item>Upper: null corresponds to +oo</item>
    /// </summary>
    BoundType BoundType { get; }
  }
}
