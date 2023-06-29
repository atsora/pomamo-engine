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
  /// GenericConfigReader using the key values in the registry
  /// 
  /// Thread safe
  /// </summary>
  public class RegistryConfigReader
    : ExternalAssemblyConfigReader
    , IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RegistryConfigReader).FullName);

    /// <summary>
    /// Constructor considering the default key HKLM\SOFTWARE\Lemoine\PULSE
    /// </summary>
    public RegistryConfigReader (bool lazy = false)
      : base ("Lemoine.Core.TargetSpecific", "Lemoine.Info.ConfigReader.TargetSpecific.RegistryConfigReader", new object[] { lazy })
    {
    }
  }
}
