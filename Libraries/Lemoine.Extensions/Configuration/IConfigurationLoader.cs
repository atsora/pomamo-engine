// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration
{
  /// <summary>
  /// Configuration loader
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public interface IConfigurationLoader<TConfiguration>
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration
  {
    /// <summary>
    /// Load the parameters to build a Configuration object
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    TConfiguration LoadConfiguration (string parameters);

    /// <summary>
    /// Get the properties to save from the configuration
    /// </summary>
    /// <returns></returns>
    string GetProperties (TConfiguration configuration);
  }
}
