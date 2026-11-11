// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// Shared named value
  /// </summary>
  internal interface ISharedValue<T>
  {
    /// <summary>
    /// Associated value
    /// </summary>
    T Value { get; set; }
  }
}
