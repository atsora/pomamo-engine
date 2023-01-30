// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.ModelDAO.Interfaces;

namespace Pulse.Database.ConnectionInitializer
{
  /// <summary>
  /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
  /// </summary>
  public class ConnectionInitializerWithNHibernateExtensions : Lemoine.ModelDAO.Interfaces.IConnectionInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConnectionInitializerWithNHibernateExtensions).FullName);

    readonly IConnectionInitializer m_connectionInitializer;
    readonly IExtensionsProvider m_extensionsProvider;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly ILctrChecker m_lctrChecker;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ConnectionInitializerWithNHibernateExtensions (IConnectionInitializer connectionInitializer, IExtensionsProvider extensionsProvider, IFileRepoClientFactory fileRepoClientFactory, ILctrChecker lctrChecker)
    {
      Debug.Assert (null != lctrChecker);

      m_connectionInitializer = connectionInitializer;
      m_extensionsProvider = extensionsProvider;
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_lctrChecker = lctrChecker;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public async Task CreateAndInitializeConnectionAsync (CancellationToken? cancellationToken = null)
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.IsInitialized ()) {
        log.Warn ("CreateAndInitializeConnectionAsync: a DAO Factory is already set");
      }

      bool isLctrNoDatabase = IsLctr ();
      if (!isLctrNoDatabase) {
        m_fileRepoClientFactory.InitializeFileRepoClient ();
      }
      Lemoine.Extensions.ExtensionManager
        .Initialize (m_extensionsProvider);
      Lemoine.Extensions.ExtensionManager
        .ActivateNHibernateExtensions (!isLctrNoDatabase);

      NHibernateHelper.ExtensionsProvider = m_extensionsProvider;
      try {
        await m_connectionInitializer.CreateAndInitializeConnectionAsync (cancellationToken: cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"CreateAndInitializeConnectionAsync: CreateAndInitializeConnectionAsync in exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public async Task CreateAndInitializeConnectionAsync (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.IsInitialized ()) {
        log.Warn ("CreateAndInitializeConnectionAsync: a DAO Factory is already set");
      }

      bool isLctrNoDatabase = IsLctr ();
      if (!isLctrNoDatabase) {
        m_fileRepoClientFactory.InitializeFileRepoClient ();
      }
      Lemoine.Extensions.ExtensionManager
        .Initialize (m_extensionsProvider);
      Lemoine.Extensions.ExtensionManager
        .ActivateNHibernateExtensions (!isLctrNoDatabase);

      NHibernateHelper.ExtensionsProvider = m_extensionsProvider;
      try {
        await m_connectionInitializer.CreateAndInitializeConnectionAsync (maxNbAttempt, cancellationToken: cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"CreateAndInitializeConnectionAsync: CreateAndInitializeConnectionAsync in exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public void CreateAndInitializeConnection (CancellationToken? cancellationToken = null)
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.IsInitialized ()) {
        log.Warn ("CreateAndInitializeConnectionAsync: a DAO Factory is already set");
      }

      bool isLctrNoDatabase = IsLctr ();
      if (!isLctrNoDatabase) {
        m_fileRepoClientFactory.InitializeFileRepoClient ();
      }
      Lemoine.Extensions.ExtensionManager
        .Initialize (m_extensionsProvider);
      Lemoine.Extensions.ExtensionManager
        .ActivateNHibernateExtensions (!isLctrNoDatabase);

      NHibernateHelper.ExtensionsProvider = m_extensionsProvider;
      try {
        m_connectionInitializer.CreateAndInitializeConnection (cancellationToken: cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"CreateAndInitializeConnection: CreateAndInitializeConnection in exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public void CreateAndInitializeConnection (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.IsInitialized ()) {
        log.Warn ("CreateAndInitializeConnection: a DAO Factory is already set");
      }

      bool isLctrNoDatabase = IsLctr ();
      if (!isLctrNoDatabase) {
        m_fileRepoClientFactory.InitializeFileRepoClient ();
      }
      Lemoine.Extensions.ExtensionManager
        .Initialize (m_extensionsProvider);
      Lemoine.Extensions.ExtensionManager
        .ActivateNHibernateExtensions (!isLctrNoDatabase);

      NHibernateHelper.ExtensionsProvider = m_extensionsProvider;
      try {
        m_connectionInitializer.CreateAndInitializeConnection (maxNbAttempt, cancellationToken: cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"CreateAndInitializeConnection: CreateAndInitializeConnection in exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Method to test if this computer is lctr without using a database access
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsLctr ()
    {
      try {
        return m_lctrChecker.IsLctr ();
      }
      catch (Exception ex) {
        log.Error ("IsLctr: IsLctr test failed => fallback to true", ex);
        return true;
      }
    }
  }
}
