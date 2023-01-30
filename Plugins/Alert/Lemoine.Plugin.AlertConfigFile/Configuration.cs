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

namespace Lemoine.Plugin.AlertConfigFile
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
    , IEquatable<Configuration>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Directory
    /// </summary>
    [PluginConf ("Text", "Directory", Description = "Directory where the file is located. If empty, consider the confiuration key Alert.ConfigDirectory whose default value is alert.d")]
    [DefaultValue ("")]
    public string Directory
    {
      get; set;
    } = "";

    /// <summary>
    /// File name
    /// </summary>
    [PluginConf ("Text", "File", Description = "File name to import. Mandatory")]
    public string File
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
      if (!string.IsNullOrEmpty (this.Directory)) {
        var pathRootedDirectoryPath = new AlertConfigDirectory (this.Directory).AbsolutePath;
        if (!System.IO.Directory.Exists (pathRootedDirectoryPath)) {
          log.Error ($"IsValid: directory {pathRootedDirectoryPath} does not exist");
          errorList.Add ($"Directory does not exist");
        }
        else {
          var filePath = System.IO.Path.Combine (pathRootedDirectoryPath, this.File);
          if (!System.IO.File.Exists (filePath)) {
            log.Error ($"IsValid: file {filePath} does not exist");
            errorList.Add ($"File does not exist");
          }
        }
      }
      errors = errorList;
      return !errors.Any ();
    }

    public bool Equals (Configuration other)
    {
      if (Object.ReferenceEquals (other, null)) {
        return false;
      }
      if (Object.ReferenceEquals (this, other)) {
        return true;
      }
      return this.Directory.Equals (other.Directory) && this.File.Equals (other.File);
    }

    public override int GetHashCode ()
    {
      unchecked {
        return 17 * this.Directory.GetHashCode () + 23 * this.File.GetHashCode ();
      }
    }
    #endregion // Constructors
  }
}
