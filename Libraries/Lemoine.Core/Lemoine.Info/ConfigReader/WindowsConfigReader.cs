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
  /// GenericConfigReader using the .exe.config file
  /// 
  /// Thread safe
  /// </summary>
  public class WindowsConfigReader
    : ExternalAssemblyConfigReader
    , IGenericConfigReader
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (WindowsConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public WindowsConfigReader ()
      : base ("Lemoine.Core.TargetSpecific", "Lemoine.Info.ConfigReader.TargetSpecific.WindowsConfigReader", new object[] { })
    {
    }
    #endregion // Constructors
  }
}
