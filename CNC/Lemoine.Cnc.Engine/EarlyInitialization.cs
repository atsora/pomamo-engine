// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// EarlyInitialization
  /// </summary>
  public static class EarlyInitialization
  {
    static readonly ILog log = LogManager.GetLogger (typeof (EarlyInitialization).FullName);

    static readonly string EARLY_INITIALIZATION_CNC_MODULES_KEY = "Cnc.EarlyInitialization.Modules";
    static readonly string EARLY_INITIALIZATION_CNC_MODULES_DEFAULT = "";

    /// <summary>
    /// Run the early initialization methods (methods that must be run first in the main thread)
    /// </summary>
    /// <param name="cancellationToken"></param>
    public static void RunEarlyInitialization (System.Threading.CancellationToken cancellationToken)
    {
      var earlyInitializationCncModulesString = Lemoine.Info.ConfigSet
        .LoadAndGet (EARLY_INITIALIZATION_CNC_MODULES_KEY, EARLY_INITIALIZATION_CNC_MODULES_DEFAULT);
      if (!string.IsNullOrEmpty (earlyInitializationCncModulesString)) {
        var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
        var earlyInitializationCncModules = Lemoine.Collections.EnumerableString.ParseListString (earlyInitializationCncModulesString);
        foreach (var earlyInitializationCncModule in earlyInitializationCncModules) {
          try {
            Lemoine.Core.Plugin.Reflection.InvokeStaticMethod (assemblyLoader, earlyInitializationCncModule, "Initialize", System.Threading.CancellationToken.None);
          }
          catch (Exception ex) {
            log.Error ($"RunEarlyInitialization: Initialize of {earlyInitializationCncModule} failed", ex);
          }
        }
      }
    }
  }
}
