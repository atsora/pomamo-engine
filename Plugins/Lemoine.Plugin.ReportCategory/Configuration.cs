// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.ReportCategory
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Name of the file to configure
    /// </summary>
    [PluginConf ("Text", "File to configure", Description = "Name of the file to configure. Default: report_categories_custom.txt", Optional = true, Multiple = false)]
    [DefaultValue ("report_categories_custom.txt")]
    public string ReportCategoriesFileName
    {
      get; set;
    } = "report_categories_custom.txt";

    /// <summary>
    /// Report name
    /// </summary>
    [PluginConf ("Text", "Report name", Description = "Name of the report without .rptdesign", Optional = false, Multiple = false)]
    [DefaultValue ("")]
    public string ReportName
    {
      get; set;
    } = "";

    /// <summary>
    /// Category
    /// </summary>
    [PluginConf ("Text", "Category", Description = "Category to set to the report", Optional = false, Multiple = false)]
    [DefaultValue ("")]
    public string Category
    {
      get; set;
    } = "";

    /// <summary>
    /// Comment to append to the instruction line
    /// </summary>
    [PluginConf ("Text", "Comment", Description = "Comment to append to the instruction line. Default: Set by the ReportCategory plugin", Optional = true, Multiple = false)]
    [DefaultValue ("Set by the ReportCategory plugin")]
    public string Comment
    {
      get; set;
    } = "set by the ReportCategory plugin";

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("Bool", "Unique category", Description = "Should a unique category be considered for this report ? If true any other associated category is removed. Default: false", Optional = true, Multiple = false)]
    [DefaultValue(false)]
    public bool UniqueCategory
    {
      get; set;
    } = false;

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
      if (string.IsNullOrEmpty (this.ReportName)) {
        errorList.Add ("Report name is empty");
      }
      if (string.IsNullOrEmpty (this.Category)) {
        errorList.Add ("Category is empty");
      }
      if (string.IsNullOrEmpty (this.ReportCategoriesFileName)) {
        errorList.Add ("Report categories file name is empty");
      }
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
