// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Cnc module interface
  /// </summary>
  public interface ICncModule: Pomamo.CncModule.ICncModule, IChecked, IDisposable
  {    
    /// <summary>
    /// Set the data handler to warn the data handler the thread is still active
    /// </summary>
    /// <param name="dataHandler"></param>
    void SetDataHandler (IChecked dataHandler);
  }

  public static class CncModuleExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (CncModuleExtensions));

    /// <summary>
    /// Get a path to a CNC Resource (in pfrdata and synchronized)
    /// </summary>
    /// <returns></returns>
    public static string GetCncResourcePath (this ICncModule module, string name)
    {
      if (Path.IsPathRooted (name)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCncResourcePath: path rooted, return {name}");
        }
        return name;
      }
      else {
        string p;
        p = Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, "cnc_resources", "shared", name);
        if (File.Exists (p)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCncResourcePath: consider {p} with program name");
          }
          return p;
        }
        p = Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, "cnc_resources", Lemoine.Info.ProgramInfo.Name, name);
        if (File.Exists (p)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCncResourcePath: consider {p} with program name");
          }
          return p;
        }
        p = Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, "cnc_resources", $"acquisition-{module.CncAcquisitionId}", name);
        if (File.Exists (p)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCncResourcePath: consider {p} with acquisition id");
          }
          return p;
        }
        p = Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, "cnc_resources", name);
        if (File.Exists (p)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCncResourcePath: consider {p} in local cnc_resources");
          }
          return p;
        }
        // TODO: check in FileRepo as well ?
        if (log.IsWarnEnabled && !File.Exists (Path.Combine (System.Environment.CurrentDirectory, name))) {
          log.Warn ($"GetCncResourcePath: {name} does not exist in current directory {System.Environment.CurrentDirectory}");
        }
        return name;
      }
    }
  }
}
