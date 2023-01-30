// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD || NET48 || NETCOREAPP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Microsoft.Extensions.Configuration;

namespace Lemoine.Info.ConfigReader.TargetSpecific
{
  /// <summary>
  /// CoreConfigReader
  /// </summary>
  public class CoreConfigReader: IGenericConfigReader
  {
    readonly ILog log = LogManager.GetLogger (typeof (CoreConfigReader).FullName);

    readonly IConfiguration m_configuration;

#region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CoreConfigReader (IConfiguration configuration)
    {
      m_configuration = configuration;
    }

    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      var v = m_configuration.GetValue<T> (key);
      if (object.Equals (default (T), v)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: config for {key} is the default one => return ConfigKeyNotFound");
        }
        throw new ConfigKeyNotFoundException (key);
      }
      else {
        return v;
      }
    }
#endregion // Constructors

  }
}

#endif // NETSTANDARD || NET48 || NETCOREAPP
