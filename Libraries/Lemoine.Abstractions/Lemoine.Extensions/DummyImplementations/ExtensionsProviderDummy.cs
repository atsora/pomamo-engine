// Copyright (c) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Interfaces;
using Lemoine.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.Extensions.DummyImplementations
{
  /// <summary>
  /// Dummy implementation (no extension implementation is provided) of <see cref="IExtensionsProvider"/>
  /// </summary>
  public class ExtensionsProviderDummy: IExtensionsProvider
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public ExtensionsProviderDummy ()
    {
    }

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    public bool Loaded => true;

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    public IEnumerable<IPluginDllLoader> LoadErrorPlugins => new List<IPluginDllLoader> ();

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    public void Activate (bool pluginUserDirectoryActive = true)
    {
    }

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    public void ActivateNHibernateExtensions (bool pluginUserDirectoryActive)
    {
    }

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    public void ClearDeactivate ()
    {
    }

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    public void Deactivate ()
    {
    }

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="packageIdentifier"></param>
    /// <param name="checkedThread"></param>
    /// <returns></returns>
    public IEnumerable<T> GetExtensions<T> (string packageIdentifier = "", IChecked checkedThread = null) where T : IExtension => new List<T> ();

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IPluginDll GetPlugin (string identifyingName) => throw new NotImplementedException ();

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <returns></returns>
    public bool IsActive () => true;

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <param name="checkedThread"></param>
    public void Load (IChecked checkedThread = null)
    {
    }

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="checkedThread"></param>
    /// <returns></returns>
    public IEnumerable<T> LoadAndGetNHibernateExtensions<T> (IChecked checkedThread = null) where T : IExtension => new List<T> ();

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="checkedThread"></param>
    /// <returns></returns>
#if NET40
    public Task LoadAsync (CancellationToken cancellationToken, IChecked checkedThread = null) => new Task (DoNothing);
    void DoNothing () { }
#else // !NET40
    public Task LoadAsync (CancellationToken cancellationToken, IChecked checkedThread = null) => Task.CompletedTask;
#endif // NET40

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <param name="checkedThread"></param>
    public void Reload (IChecked checkedThread = null)
    {
    }

    /// <summary>
    /// <see cref="IExtensionsProvider"/>
    /// </summary>
    /// <param name="pluginFilter"></param>
    public void Reload (IPluginFilter pluginFilter)
    {
    }
  }
}
