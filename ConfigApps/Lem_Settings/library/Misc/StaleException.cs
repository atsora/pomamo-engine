// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of StaleException.
  /// </summary>
  public class StaleException: Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public StaleException (string message)
      : base (message)
    { }
  }
}
