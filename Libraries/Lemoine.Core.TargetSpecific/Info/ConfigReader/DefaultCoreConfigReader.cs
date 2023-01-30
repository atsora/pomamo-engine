// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader.TargetSpecific
{
  /// <summary>
  /// Default configurations in case .NET Core is used
  /// </summary>
  public sealed class DefaultCoreConfigReader : DefaultGenericConfigReader, IGenericConfigReader
  {
    readonly ILog log = LogManager.GetLogger (typeof (DefaultCoreConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultCoreConfigReader ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="DefaultGenericConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected override object Get (string key)
    {
      switch (key) {
#if NETCOREAPP
      // See NHibernateConfigCache: the serialization of the configuration does not work in .NET Core
      case "Database.UseMappingCache":
      case "Database.StoreMappingCache":
        return false;
#endif // NETCOREAPP
      default:
        throw new ConfigKeyNotFoundException (key);
      }
    }
  }
}
