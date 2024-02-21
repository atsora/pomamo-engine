// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Model;
using Lemoine.ModelDAO.Interfaces;

namespace Pulse.Extensions
{
  /// <summary>
  /// ExtensionsLoader
  /// </summary>
  public class ExtensionsLoader : IExtensionsLoader
  {
    readonly ILog log = LogManager.GetLogger (typeof (ExtensionsLoader).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (ExtensionsLoader).FullName);

    readonly Lemoine.FileRepository.IFileRepoClientFactory m_fileRepoClientFactory;
    readonly IExtensionsProvider m_extensionsProvider;
    readonly IConnectionInitializer m_connectionInitializer;
    readonly ILctrChecker m_lctrChecker;

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
    /// <param name="fileRepoClientFactory">not null</param>
    /// <param name="extensionsProvider">not null</param>
    /// <param name="lctrChecker">not null</param>
    /// <param name="connectionInitializer">nullable</param>
    public ExtensionsLoader (Lemoine.FileRepository.IFileRepoClientFactory fileRepoClientFactory, IExtensionsProvider extensionsProvider, ILctrChecker lctrChecker, IConnectionInitializer connectionInitializer)
    {
      Debug.Assert (null != fileRepoClientFactory);
      Debug.Assert (null != extensionsProvider);
      Debug.Assert (null != lctrChecker);

      m_fileRepoClientFactory = fileRepoClientFactory;
      m_extensionsProvider = extensionsProvider;
      m_lctrChecker = lctrChecker;
      m_connectionInitializer = connectionInitializer;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IExtensionsLoader"/>
    /// </summary>
    /// <returns></returns>
    public async Task LoadExtensionsAsync (CancellationToken? cancellationToken = null)
    {
      Lemoine.Extensions.ExtensionManager.Initialize (m_extensionsProvider);

      if (m_extensionsProvider.Loaded) {
        if (log.IsDebugEnabled) {
          log.Debug ("LoadExtensionsAsync: already loaded");
        }
        return;
      }

      var token = cancellationToken ?? CancellationToken.None;

      try {
        if (null != m_connectionInitializer) {
          await m_connectionInitializer.InitializeAsync ();
        }
        var isLctr = m_lctrChecker.IsLctr ();
        if (!isLctr) {
          m_fileRepoClientFactory.InitializeFileRepoClient (); // Required for the plugins synchronization
        }

        log.Info ("LoadExtensionsAsync: load the extensions");
        m_extensionsProvider.Activate (!isLctr);
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
      Lemoine.Extensions.ExtensionManager.Initialize (m_extensionsProvider);

      if (m_extensionsProvider.Loaded) {
        if (log.IsDebugEnabled) {
          log.Debug ("LoadExtensions: already loaded");
        }
        return;
      }

      try {
        m_connectionInitializer?.Initialize ();
        var isLctr = m_lctrChecker.IsLctr ();
        if (!isLctr) {
          m_fileRepoClientFactory.InitializeFileRepoClient (); // Required for the plugins synchronization
        }

        log.Info ("LoadExtensions: load the extensions");
        m_extensionsProvider.Activate (!isLctr);
        m_extensionsProvider.Load ();
      }
      catch (Exception ex) {
        log.Error ("LoadExtensions: error while loading the extensions but continue", ex);
      }
    }
  }
}
