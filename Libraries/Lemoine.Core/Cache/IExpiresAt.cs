// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// Interface a cache value must implement so that it expires at a specific date/time
  /// </summary>
  public interface IExpiresAt
  {
    /// <summary>
    /// The data expires at the specified date/time
    /// </summary>
    DateTime ExpiresAt { get; }
  }
}
