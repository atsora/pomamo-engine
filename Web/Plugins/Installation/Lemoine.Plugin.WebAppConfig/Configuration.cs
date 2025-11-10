// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lemoine.Plugin.WebAppConfig
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Relative file path to the root directory of the web app
    /// </summary>
    [PluginConf ("Text", "File path", Description = "Relative file path to the root directory of the web app. Default: scripts/config_custom.js", Multiple = false)]
    [DefaultValue ("scripts/config_custom.js")]
    public string RelativePath
    {
      get; set;
    } = "scripts/config_custom.js";

    /// <summary>
    /// Instruction line to set in the path
    /// </summary>
    [PluginConf ("Text", "Instruction line", Description = "Instruction line to set in the path", Optional = false, Multiple = false)]
    [DefaultValue ("")]
    public string InstructionLine
    {
      get; set;
    } = "";

    /// <summary>
    /// Comment to append to the instruction line
    /// </summary>
    [PluginConf ("Text", "Comment", Description = "Comment to append to the instruction line. Default: Set by the WebAppConfig plugin", Optional = true, Multiple = false)]
    [DefaultValue ("Set by the WebAppConfig plugin")]
    public string Comment
    {
      get; set;
    } = "Set by the WebAppConfig plugin";

    /// <summary>
    /// * or a list (, separated) of root directory names to match
    /// </summary>
    [PluginConf ("Text", "Root directory names", Description = "* or a list (, separated) of root directory names to match. Default: *")]
    [DefaultValue ("*")]
    public string RootDirectoryNames
    {
      get; set;
    } = "*";

    /// <summary>
    /// Html pattern
    /// </summary>
    [PluginConf ("Text", "Html pattern", Description = "Pattern to search for html files in the root directory. Default: *.html", Multiple = false)]
    [DefaultValue ("*.html")]
    public string HtmlPattern
    {
      get; set;
    } = "*.html";

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
      if (string.IsNullOrEmpty (this.InstructionLine)) {
        errorList.Add ("Instruction line is empty");
      }
      errors = errorList;
      return true;
    }
  }
}
