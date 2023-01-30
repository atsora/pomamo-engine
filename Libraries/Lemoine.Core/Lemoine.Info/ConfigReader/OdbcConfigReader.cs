// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;
using Microsoft.Win32;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Read the ODBC configuration and set them in the config key DbConnection[.dsnname]
  /// 
  /// Thread safe
  /// </summary>
  public class OdbcConfigReader
    : ExternalAssemblyConfigReader
    , IGenericConfigReader
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OdbcConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public OdbcConfigReader ()
      : base ("Lemoine.Core.TargetSpecific", "Lemoine.Info.ConfigReader.TargetSpecific.OdbcConfigReader", new object[] { })
    {
    }
    #endregion // Constructors
  }
}
