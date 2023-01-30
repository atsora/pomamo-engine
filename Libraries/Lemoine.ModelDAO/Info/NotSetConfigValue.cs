// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.ModelDAO.Info
{
  /// <summary>
  /// Specific config values to use in database to tell a config is not set
  /// </summary>
  public static class NotSetConfigValue
  {
    /// <summary>
    /// Specific config value to tell a string value is not set
    /// </summary>
    public static string String {
      get { return "not set"; }
    }
  }
}
