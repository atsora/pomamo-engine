// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Alert
{
  /// <summary>
  /// AlertConfigDirectory
  /// </summary>
  public class AlertConfigDirectory
  {
    static readonly string CONFIG_DIRECTORY_KEY = "Alert.ConfigDirectory";
    static readonly string CONFIG_DIRECTORY_DEFAULT = "alert.d";
    // Note: Os specific default values for Alert.ConfigDirectory
    //       are defined in Lemoine.Core.TargetSpecific

    readonly ILog log = LogManager.GetLogger (typeof (AlertConfigDirectory).FullName);

    readonly string m_absolutePath;

    /// <summary>
    /// Absolute path (rooted) of the specified directory
    /// </summary>
    public string AbsolutePath => m_absolutePath;

    #region Constructors
    /// <summary>
    /// Constructor using the default alert config from configuration key Alert.ConfigDirectory
    /// </summary>
    public AlertConfigDirectory ()
      : this ("")
    {
    }

    /// <summary>
    /// Constructor specifying a directory name or path
    /// </summary>
    /// <param name="directory"></param>
    public AlertConfigDirectory (string directory)
    {
      string effectiveDirectory;
      if (string.IsNullOrEmpty (directory)) {
        effectiveDirectory = Lemoine.Info.ConfigSet.LoadAndGet (CONFIG_DIRECTORY_KEY, CONFIG_DIRECTORY_DEFAULT);
      }
      else {
        effectiveDirectory = directory;
      }
      m_absolutePath = GetPathRootedConfigDirectory (effectiveDirectory);
      if (log.IsDebugEnabled) {
        log.Debug ($"AlertConfigDirectory: {directory} => {effectiveDirectory} => {m_absolutePath}");
      }
    }
    #endregion // Constructors

    /// <summary>
    /// If not rooted, get the first existing directory made of:
    /// <item>common configuration directory</item>
    /// <item>program directory</item>
    /// <item>local configuration directory</item>
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    string GetPathRootedConfigDirectory (string relativePath)
    {
      if (Path.IsPathRooted (relativePath)) {
        return relativePath;
      }
      else {
        var commonConfigurationDirectory = Lemoine.Info.PulseInfo.CommonConfigurationDirectory;
        if (string.IsNullOrEmpty (commonConfigurationDirectory)
          || !Path.IsPathRooted (commonConfigurationDirectory)) {
          log.Fatal ($"GetCorrectedConfigDirectoryPath: common configuration directory {commonConfigurationDirectory} is not valid");
        }
        else {
          var pathWithCommonConfigurationDirectory = Path.Combine (commonConfigurationDirectory, relativePath);
          if (Directory.Exists (pathWithCommonConfigurationDirectory)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetPathRootedConfigDirectory: with common configuration directory, {relativePath} => {pathWithCommonConfigurationDirectory}");
            }
            return pathWithCommonConfigurationDirectory;
          }
        }
        var callingAssemblyDirectory = Lemoine.Info.ProgramInfo.AbsoluteDirectory;
        if (null != callingAssemblyDirectory) {
          var correctedDirectory = Path.Combine (callingAssemblyDirectory, relativePath);
          if (Directory.Exists (correctedDirectory)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetPathRootedConfigDirectory: from calling assembly directory, {relativePath} => {correctedDirectory}");
            }
            return correctedDirectory;
          }
        }
        var defaultResult = Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, relativePath);
        if (log.IsDebugEnabled) {
          log.Debug ($"GetPathRootedConfigDirectory: with local configuration directory (default), {relativePath} => {defaultResult}");
        }
        return defaultResult;
      }
    }

  }
}
