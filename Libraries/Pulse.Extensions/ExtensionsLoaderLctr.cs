// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Model;
using Lemoine.ModelDAO.Interfaces;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Simple <see cref="IExtensionsLoader"/> implementation that works only on lctr
  /// </summary>
  public class ExtensionsLoaderLctr : IExtensionsLoader
  {
    readonly ILog log = LogManager.GetLogger (typeof (ExtensionsLoaderLctr).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (ExtensionsLoaderLctr).FullName);

    readonly IExtensionsProvider m_extensionsProvider;

    /// <summary>
    /// Force the load of the exension provider
    /// </summary>
    public bool ForceNewExtensionsProvider { get; set; } = false;

    /// <summary>
    /// Associated extensions provider
    /// 
    /// <see cref="IExtensionsLoader"/>
    /// </summary>
    public IExtensionsProvider ExtensionsProvider => m_extensionsProvider;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extensionsProvider">not null</param>
    public ExtensionsLoaderLctr (IExtensionsProvider extensionsProvider)
    {
      Debug.Assert (null != extensionsProvider);

      m_extensionsProvider = extensionsProvider;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IExtensionsLoader"/>
    /// </summary>
    /// <returns></returns>
    public async Task LoadExtensionsAsync (CancellationToken? cancellationToken = null)
    {
      Lemoine.Extensions.ExtensionManager.Initialize (m_extensionsProvider, force: this.ForceNewExtensionsProvider);

      if (m_extensionsProvider.Loaded) {
        if (log.IsDebugEnabled) {
          log.Debug ("LoadExtensionsAsync: already loaded");
        }
        return;
      }

      var token = cancellationToken ?? CancellationToken.None;

      try {
        log.Info ("LoadExtensionsAsync: load the extensions");
        m_extensionsProvider.Activate (false);
        await m_extensionsProvider.LoadAsync (token);
      }
      catch (Exception ex) {
        log.Error ("LoadExtensionsAsync: error while loading the extensions but continue", ex);
      }
    }

    /// <summary>
    /// <see cref="IExtensionsLoader"/>
    /// </summary>
    /// <returns></returns>
    public void LoadExtensions ()
    {
      Lemoine.Extensions.ExtensionManager.Initialize (m_extensionsProvider, force: this.ForceNewExtensionsProvider);

      if (m_extensionsProvider.Loaded) {
        if (log.IsDebugEnabled) {
          log.Debug ("LoadExtensionsAsync: already loaded");
        }
        return;
      }

      try {
        log.Info ("LoadExtensionsAsync: load the extensions");
        m_extensionsProvider.Activate (false);
        m_extensionsProvider.Load ();
      }
      catch (Exception ex) {
        log.Error ("LoadExtensionsAsync: error while loading the extensions but continue", ex);
      }
    }

  }
}
