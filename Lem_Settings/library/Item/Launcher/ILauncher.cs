// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of ILauncher.
  /// </summary>
  public interface ILauncher : IItem
  {
    /// <summary>
    /// Path of the software to open
    /// </summary>
    String SoftwarePath { get; }
    
    /// <summary>
    /// Argument to send, may be null
    /// </summary>
    IList<String> Arguments { get; }
  }
}
