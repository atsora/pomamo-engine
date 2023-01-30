// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.FileRepository;
using Lemoine.Info;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Plugin.PluginDirectories
{
  /// <summary>
  /// Class to synchronize and load the plugin directories only once (eager load)
  /// 
  /// Threadsafe
  /// </summary>
  public class PluginDirectories
    : Directories
    , IPluginDirectories
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PluginDirectories).FullName);
    static readonly ILog pluginStatusLog = LogManager.GetLogger ("Lemoine.Extensions.PluginStatus");

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="assemblyLoader">not null</param>
    public PluginDirectories (IAssemblyLoader assemblyLoader)
      : base (assemblyLoader)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// To override to get a specific directory
    /// </summary>
    /// <returns></returns>
    protected override string GetSpecificDirectoryName ()
    {
      return "plugins";
    }
    #endregion // Methods
  }
}
