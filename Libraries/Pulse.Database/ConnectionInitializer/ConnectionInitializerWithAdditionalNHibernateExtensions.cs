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
using Lemoine.Database.Persistent;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.ModelDAO.Interfaces;

namespace Pulse.Database.ConnectionInitializer
{
  /// <summary>
  /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
  /// </summary>
  public class ConnectionInitializerWithAdditionalNHibernateExtensions : Lemoine.ModelDAO.Interfaces.IConnectionInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConnectionInitializerWithAdditionalNHibernateExtensions).FullName);

    readonly IConnectionInitializer m_connectionInitializer;
    readonly IExtensionsProvider m_extensionsProvider;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ConnectionInitializerWithAdditionalNHibernateExtensions (IConnectionInitializer connectionInitializer, IExtensionsProvider extensionsProvider)
    {
      m_connectionInitializer = connectionInitializer;
      m_extensionsProvider = extensionsProvider;
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

      Lemoine.Extensions.ExtensionManager
        .Initialize (m_extensionsProvider);
      Lemoine.Extensions.ExtensionManager
        .ActivateNHibernateExtensions (false);

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

      Lemoine.Extensions.ExtensionManager
        .Initialize (m_extensionsProvider);
      Lemoine.Extensions.ExtensionManager
        .ActivateNHibernateExtensions (false);

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

      Lemoine.Extensions.ExtensionManager
        .Initialize (m_extensionsProvider);
      Lemoine.Extensions.ExtensionManager
        .ActivateNHibernateExtensions (false);

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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public void CreateAndInitializeConnection (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.IsInitialized ()) {
        log.Warn ("CreateAndInitializeConnection: a DAO Factory is already set");
      }

      Lemoine.Extensions.ExtensionManager
        .Initialize (m_extensionsProvider);
      Lemoine.Extensions.ExtensionManager
        .ActivateNHibernateExtensions (false);

      NHibernateHelper.ExtensionsProvider = m_extensionsProvider;
      try {
        m_connectionInitializer.CreateAndInitializeConnection (maxNbAttempt, cancellationToken: cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"CreateAndInitializeConnection: CreateAndInitializeConnection in exception", ex);
        throw;
      }
    }

  }
}
