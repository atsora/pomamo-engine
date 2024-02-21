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
using Lemoine.Extensions.Plugin;
using Lemoine.Threading;

namespace Lemoine.Extensions.ExtensionsProvider
{
  /// <summary>
  /// AdditionalExtensionsOnlyProvider
  /// </summary>
  public class AdditionalExtensionsOnlyProvider : IExtensionsProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (AdditionalExtensionsOnlyProvider).FullName);

    readonly IList<Type> m_additionalExtensionTypes = new List<Type> ();
    volatile bool m_active = true; // Active by default

    /// <summary>
    /// Constructor
    /// </summary>
    public AdditionalExtensionsOnlyProvider ()
    {
    }

    #region IExtensionsProvider
    public bool Loaded => true;

    public IEnumerable<IPluginDllLoader> LoadErrorPlugins => new List<IPluginDllLoader> ();

    public void Activate (bool pluginUserDirectoryActive = true)
    {
      m_active = true;
    }

    public void ActivateNHibernateExtensions (bool pluginUserDirectoryActive)
    {
    }

    public void ClearDeactivate ()
    {
      Clear ();
      Deactivate ();
    }

    public void Deactivate ()
    {
      m_active = false;
    }

    public IEnumerable<T> GetExtensions<T> (string packageIdentifier = "", IChecked checkedThread = null) where T : IExtension
    {
      IList<T> extensions = new List<T> ();

      if (!m_active) {
        log.Debug ("GetExtensions: extensions are not active => return an empty list");
        return extensions;
      }

      if (!string.IsNullOrEmpty (packageIdentifier)) {
        log.Info ($"GetExtensions: packageIdentifier is set => return an empty list");
        return extensions;
      }

      foreach (var additionalExtensionType in m_additionalExtensionTypes
        .Where (type => typeof (T).IsAssignableFrom (type))
        .Distinct ()) {
        IExtension extension = (IExtension)Activator.CreateInstance (additionalExtensionType);
        extensions.Add ((T)extension);
        checkedThread?.SetActive ();
      }
      return extensions;
    }

    public IPluginDll GetPlugin (string identifyingName)
    {
      return null;
    }

    public bool IsActive ()
    {
      return m_active && m_additionalExtensionTypes.Any ();
    }

    public void Load (IChecked checkedThread = null)
    {
    }

    public IEnumerable<T> LoadAndGetNHibernateExtensions<T> (IChecked checkedThread = null) where T : IExtension
    {
      return GetExtensions<T> ();
    }

    public async Task LoadAsync (CancellationToken cancellationToken, IChecked checkedThread = null)
    {
      await Task.Delay (0);
    }

    public void Reload (IChecked checkedThread = null)
    {
    }

    public void Reload (IPluginFilter pluginFilter)
    {
    }
    #endregion // IExtensionsProvider

    /// <summary>
    /// Add an extension manually, for example for the tests or for specific applications
    /// </summary>
    /// <param name="extensionType"></param>
    public void Add (Type extensionType)
    {
      if (typeof (IExtension).IsAssignableFrom (extensionType)) {
        m_additionalExtensionTypes.Add (extensionType);
      }
      else {
        log.Error ($"Add: extensionType {extensionType} is not of type IExtension, skip it");
      }
    }

    /// <summary>
    /// Clear all the additional extensions (used for the tests for example)
    /// </summary>
    public void Clear ()
    {
      m_additionalExtensionTypes.Clear ();
    }
  }
}
