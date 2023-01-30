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
  public abstract class ConfigurationLoaderWithXml<TConfiguration>
    : ConfigurationLoader<TConfiguration>
    , IConfigurationLoader<TConfiguration>
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigurationLoaderWithXml<TConfiguration>).FullName);

    /// <summary>
    /// Load the parameters to build a Configuration object
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public override TConfiguration LoadConfiguration (string parameters)
    {
      var trimmed = parameters.Trim ();

      if (trimmed.StartsWith ("<", StringComparison.InvariantCulture)) {
        return LoadXmlConfiguration (parameters);
      }

      if (trimmed.StartsWith ("{", StringComparison.InvariantCulture)
          || trimmed.StartsWith ("[", StringComparison.InvariantCulture)) {
        return base.LoadConfiguration (parameters);
      }

      throw new ArgumentException ("Invalid format");
    }

    /// <summary>
    /// Load a configuration from an XML string. To override if needed
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    protected abstract TConfiguration LoadXmlConfiguration (string xml);
  }
}
