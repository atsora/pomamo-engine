// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Hosting;

namespace Lemoine.Core.Performance
{
  /// <summary>
  /// Performance record interface
  /// </summary>
  public interface IPerfRecorder: IApplicationInitializer
  {
    /// <summary>
    /// Record a duration for a specific key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="duration"></param>
    void Record (string key, TimeSpan duration);
  }
}
