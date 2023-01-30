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
  /// Class to synchronize and load the NHibernateExtension plugin directories only once (eager load)
  /// 
  /// Threadsafe
  /// </summary>
  public class NHibernatePluginDirectories
    : Directories
    , IPluginDirectories
  {
    static readonly string SYNC_PLUGINS_DIRECTORY_APPLICATION_NAME_KEY = "Extensions.SyncNHibernatePluginsDirectoryWithApplicationName";
    static readonly bool SYNC_PLUGINS_DIRECTORY_APPLICATION_NAME_DEFAULT = true;

    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (NHibernatePluginDirectories).FullName);
    static readonly ILog pluginStatusLog = LogManager.GetLogger ("Lemoine.Extensions.PluginStatus");

    #region Constructors
    /// <summary>
    /// Contructor
    /// </summary>
    /// <param name="assemblyLoader">not null</param>
    public NHibernatePluginDirectories (IAssemblyLoader assemblyLoader)
      : base (assemblyLoader)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="Directories"/>
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginNames"></param>
    /// <param name="checkedThread"></param>
    protected override void LoadAdditionalPluginDirectories (bool pluginUserDirectoryActive, IEnumerable<string> pluginNames, Lemoine.Threading.IChecked checkedThread)
    {
    }

    /// <summary>
    /// <see cref="Directories"/>
    /// </summary>
    /// <returns></returns>
    protected override bool IncludeApplicationName ()
    {
      return Lemoine.Info.ConfigSet
  .LoadAndGet<bool> (SYNC_PLUGINS_DIRECTORY_APPLICATION_NAME_KEY,
  SYNC_PLUGINS_DIRECTORY_APPLICATION_NAME_DEFAULT);
    }

    /// <summary>
    /// To override to get a specific directory
    /// </summary>
    /// <returns></returns>
    protected override string GetSpecificDirectoryName ()
    {
      return "nhibernateextensions";
    }

    /// <summary>
    /// <see cref="Directories"/>
    /// </summary>
    /// <param name="fileNames"></param>
    /// <param name="fileName"></param>
    /// <param name="pluginNames"></param>
    /// <returns></returns>
    protected override bool Filter (IEnumerable<string> fileNames, string fileName, IEnumerable<string> pluginNames)
    {
      var extension = Path.GetExtension (fileName);
      if (extension.Equals (".nhibernateextension")) {
        return true;
      }
      else if (extension.Equals (".dll")) {
        if (fileName.StartsWith ("Lemoine.Plugin.")) {
          var pluginName = Path.GetFileNameWithoutExtension (fileName)
            .Substring ("Lemoine.Plugin.".Length);
          return fileNames.Any (f => f.Equals (pluginName + ".nhibernateextension"));
        }
        else {
          return false;
        }
      }
      else {
        return false;
      }
    }
    #endregion // Methods
  }
}
