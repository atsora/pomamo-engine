// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Threading;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// DummyPluginsLoader: IPluginsLoader that does nothing (that does not load any plugin and does not return any)
  /// </summary>
  public class DummyPluginsLoader: IPluginsLoader, INHibernatePluginsLoader, IMainPluginsLoader
  {
    readonly ILog log = LogManager.GetLogger (typeof (DummyPluginsLoader).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DummyPluginsLoader ()
    {
    }

    /// <summary>
    /// <see cref="IPluginsLoader"/>
    /// </summary>
    public bool Loaded => true;

    /// <summary>
    /// <see cref="IPluginsLoader"/>
    /// </summary>
    /// <param name="checkedThread"></param>
    public void Clear (IChecked checkedThread)
    {
    }

    /// <summary>
    /// <see cref="IPluginsLoader"/>
    /// </summary>
    /// <returns></returns>
    public IList<IPluginDll> GetActivePlugins ()
    {
      return new List<IPluginDll> ();
    }

    /// <summary>
    /// <see cref="IPluginsLoader"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IPluginDllLoader> GetLoadErrorPlugins ()
    {
      return new List <IPluginDllLoader> ();
    }

    /// <summary>
    /// <see cref="IPluginsLoader"/>
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    public void Load (bool pluginUserDirectoryActive, IPluginFilter pluginFilter, IChecked checkedThread)
    {
    }

    /// <summary>
    /// <see cref="IPluginsLoader"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    /// <returns></returns>
    public async Task LoadAsync (CancellationToken cancellationToken, bool pluginUserDirectoryActive, IPluginFilter pluginFilter, IChecked checkedThread)
    {
      await Task.Delay (0);
    }

    /// <summary>
    /// <see cref="IPluginsLoader"/>
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    public void Reload (bool pluginUserDirectoryActive, IPluginFilter pluginFilter, IChecked checkedThread)
    {
      return;
    }
  }
}
