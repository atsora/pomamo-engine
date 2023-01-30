// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;

namespace Lemoine.Extensions.Configuration
{
  /// <summary>
  /// Basic interface for the configuration interfaces
  /// </summary>
  public interface IConfiguration
  {
    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    bool IsValid (out IEnumerable<string> errors);
  }
}
