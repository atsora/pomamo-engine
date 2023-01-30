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

namespace Lemoine.Plugin.InstallationSqlRunner
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Priority
    /// 
    /// <see cref="Lemoine.Extensions.Business.Config.IInstallationExtension"/>
    /// </summary>
    [PluginConf ("DoubleAsNumericUpDown", "Priority", Description = "Priority in which the different plugins are run, in ascending order for CheckConfig and in descending order for RemoveConfig", Optional = true, Parameters = "9999.99")]
    [DefaultValue (0.0)]
    public double Priority
    {
      get; set;
    } = 0.0;

    [PluginConf ("Bool", "Missing file is ok", Description = "Do not log an error if the SQL file is missing", Optional = true)]
    [DefaultValue (false)]
    public bool MissingFileOk { get; set; } = false;

    [PluginConf ("Bool", "Sql error is ok", Description = "Do not log an error if the SQL requests ends in error (for example because the object already exists)", Optional = true)]
    [DefaultValue (false)]
    public bool SqlErrorOk { get; set; } = false;

    /// <summary>
    /// Sql file name without the .sql suffix in the SqlRequests directory
    /// </summary>
    [PluginConf ("Text", "Sql file name", Description = "Sql file name without the .sql suffix in the SqlRequests directory", Optional = false)]
    public string SqlFileName {
      get; set;
    }

    /// <summary>
    /// Separator between SQL requests. Default: ;
    /// </summary>
    [PluginConf ("Text", "Separator", Description = "Separator between SQL requests. Default: ;", Optional = true)]
    [DefaultValue (";")]
    public string Separator
    {
      get; set;
    } = ";";

    /// <summary>
    /// Minimum version of PostgreSQL. Default: no minimum
    /// 
    /// Ex
    /// </summary>
    [PluginConf ("IntAsText", "Min PostgreSQL version", Description = "Minimum version of PostgreSQL, e.g. 120000. Default: no minimum", Optional = true)]
    [DefaultValue (null)]

    public int? MinPostgreSQLVersion
    {
      get; set;
    } = null;

    /// <summary>
    /// Maximum version of PostgreSQL (excluded). Default: no maximum
    /// </summary>
    [PluginConf ("IntAsText", "Max PostgreSQL version", Description = "Maximum version of PostgreSQL (excluded), e.g. 150000. Default: no maximum", Optional = true)]
    [DefaultValue (null)]

    public int? MaxPostgreSQLVersion
    {
      get; set;
    } = null;

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

      if (string.IsNullOrEmpty (this.SqlFileName)) {
        errorList.Add ("SqlFileName is empty");
      }

      errors = errorList;
      return true;
    }
  }
}
