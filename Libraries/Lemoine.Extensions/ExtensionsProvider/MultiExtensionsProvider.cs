// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Threading;

namespace Lemoine.Extensions.ExtensionsProvider
{
  /// <summary>
  /// MultiExtensionsProvider
  /// </summary>
  public class MultiExtensionsProvider : IExtensionsProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (MultiExtensionsProvider).FullName);

#if (NETSTANDARD2_1 || NETCOREAPP2_0)
    readonly ConcurrentQueue<IExtensionsProvider> m_extensionsProviders = new ConcurrentQueue<IExtensionsProvider> ();
#else
    ConcurrentQueue<IExtensionsProvider> m_extensionsProviders = new ConcurrentQueue<IExtensionsProvider> ();
    readonly SemaphoreSlim m_m_extensionsProvidersSemaphore = new SemaphoreSlim (1, 1);
#endif

    public bool Loaded => m_extensionsProviders.All (x => x.Loaded);

    public IEnumerable<PluginDllLoader> LoadErrorPlugins => m_extensionsProviders.SelectMany (x => x.LoadErrorPlugins);

    /// <summary>
    /// Constructor
    /// </summary>
    public MultiExtensionsProvider (params IExtensionsProvider[] extensionsProviders)
    {
      foreach (var extensionsProvider in extensionsProviders) {
        Add (extensionsProvider);
      }
    }

    public bool IsActive ()
    {
      return m_extensionsProviders.Any (x => x.IsActive ());
    }

    public void ActivateNHibernateExtensions (bool pluginUserDirectoryActive)
    {
      foreach (var extensionsProvider in m_extensionsProviders) {
        extensionsProvider.ActivateNHibernateExtensions (pluginUserDirectoryActive);
      }
    }

    public void Activate (bool pluginUserDirectoryActive = true)
    {
      foreach (var extensionsProvider in m_extensionsProviders) {
        extensionsProvider.Activate (pluginUserDirectoryActive);
      }
    }

    public void Deactivate ()
    {
      foreach (var extensionsProvider in m_extensionsProviders) {
        extensionsProvider.Deactivate ();
      }
    }

    public void ClearDeactivate ()
    {
      foreach (var extensionsProvider in m_extensionsProviders) {
        extensionsProvider.ClearDeactivate ();
      }
    }

    public IEnumerable<T> GetExtensions<T> (string packageIdentifier = "", IChecked checkedThread = null) where T : IExtension
    {
      return m_extensionsProviders.SelectMany (x => x.GetExtensions<T> (packageIdentifier, checkedThread));
    }

    public IEnumerable<T> LoadAndGetNHibernateExtensions<T> (IChecked checkedThread = null) where T : IExtension
    {
      return m_extensionsProviders.SelectMany (x => x.LoadAndGetNHibernateExtensions<T> (checkedThread));
    }

    public void Reload (IChecked checkedThread = null)
    {
      foreach (var extensionsProvider in m_extensionsProviders) {
        extensionsProvider.Reload (checkedThread);
      }
    }

    public void Reload (IPluginFilter pluginFilter)
    {
      foreach (var extensionsProvider in m_extensionsProviders) {
        extensionsProvider.Reload (pluginFilter);
      }
    }

    public void Load (IChecked checkedThread = null)
    {
      foreach (var extensionsProvider in m_extensionsProviders) {
        extensionsProvider.Load (checkedThread);
      }
    }

    public async Task LoadAsync (CancellationToken cancellationToken, IChecked checkedThread = null)
    {
      await Task.WhenAll (m_extensionsProviders.Select (x => new Task (() => x.LoadAsync (cancellationToken, checkedThread))));
    }

    public IPluginDll GetPlugin (string identifyingName)
    {
      return m_extensionsProviders
        .Select (x => x.GetPlugin (identifyingName))
        .FirstOrDefault (x => (null != x));
    }

    /// <summary>
    /// Add a config reader
    /// </summary>
    /// <param name="extensionsProvider"></param>
    public void Add (IExtensionsProvider extensionsProvider)
    {
      m_extensionsProviders.Enqueue (extensionsProvider);
    }

    /// <summary>
    /// Clear all the config readers
    /// </summary>
    public void Clear ()
    {
#if (NETSTANDARD2_1 || NETCOREAPP2_0)
      m_configReaders.Clear ();
#else
      using (var sephanoreHolder = SemaphoreSlimHolder.Create (m_m_extensionsProvidersSemaphore)) {
        m_extensionsProviders = new ConcurrentQueue<IExtensionsProvider> ();
      }
#endif // NETSTANDARD2_1 || NETCOREAPP2_0
    }

  }
}
