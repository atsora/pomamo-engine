// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.AlertConfigDirectory
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// My parameter
    /// </summary>
    [PluginConf ("Text", "Directory", Description = "Directory to import. If empty, consider the confiuration key Alert.ConfigDirectory whose default value is alert.d")]
    [DefaultValue ("")]
    public string Directory
    {
      get; set;
    } = "";
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();
      string pathRootedDirectoryPath = new Lemoine.Extensions.Alert.AlertConfigDirectory (this.Directory).AbsolutePath;
      if (!System.IO.Directory.Exists (pathRootedDirectoryPath)) {
        log.Error ($"IsValid: {pathRootedDirectoryPath} does not exist");
        errorList.Add ($"Directory does not exist");
      }
      errors = errorList;
      return !errors.Any ();
    }
    #endregion // Constructors
  }
}
