// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.AlertConfigTemplate
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Directory
    /// </summary>
    [PluginConf ("Text", "Directory", Description = "Directory where the template file is located. If empty, consider the confiuration key Alert.ConfigDirectory whose default value is alert.d")]
    [DefaultValue ("")]
    public string Directory
    {
      get; set;
    } = "";

    /// <summary>
    /// File name
    /// </summary>
    [PluginConf ("Text", "TemplateFileName", Description = "Template file name to activate. Mandatory")]
    public string TemplateFileName
    {
      get; set;
    }

    /// <summary>
    /// File name
    /// </summary>
    [PluginConf ("Text", "ConfigFileName", Description = "Destination file name. Mandatory")]
    public string ConfigFileName
    {
      get; set;
    }
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
      var directoryPath = new Lemoine.Extensions.Alert.AlertConfigDirectory (this.Directory).AbsolutePath;
      if (!System.IO.Directory.Exists (directoryPath)) {
        log.Error ($"IsValid: directory {directoryPath} does not exist");
        errorList.Add ($"Directory does not exist");
      }
      else {
        var filePath = System.IO.Path.Combine (directoryPath, this.TemplateFileName);
        if (!System.IO.File.Exists (filePath)) {
          log.Error ($"IsValid: template {filePath} does not exist");
          errorList.Add ($"Template does not exist");
        }
      }
      errors = errorList;
      return !errors.Any ();
    }
    #endregion // Constructors
  }
}
