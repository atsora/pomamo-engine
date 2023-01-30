// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Extension
{
  /// <summary>
  /// Interface for the extensions that are configurable
  /// </summary>
  public interface IConfigurable
  {
    /// <summary>
    /// Add the context of a configuration, i.e. an association between a package and a plugin here
    /// </summary>
    /// <param name="pluginInstance"></param>
    void AddConfigurationContext (IPluginInstance pluginInstance);
  }
}
