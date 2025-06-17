// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Migration;
using Lemoine.Database.Persistent;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;

namespace Pulse.Database.ConnectionInitializer
{
  /// <summary>
  /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
  /// </summary>
  public class ConnectionInitializer : Lemoine.ModelDAO.Interfaces.IConnectionInitializer, Lemoine.ModelDAO.IDatabaseConnectionStatus
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConnectionInitializer).FullName);

    readonly string m_applicationName;
    readonly IMigrationHelper m_migrationHelper;

    /// <summary>
    /// Kill orphaned connections first
    /// </summary>
    public bool KillOrphanedConnectionsFirst { get; set; } = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public ConnectionInitializer (string applicationName, IMigrationHelper migrationHelper)
    {
      m_applicationName = applicationName;
      m_migrationHelper = migrationHelper;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ConnectionInitializer (IMigrationHelper migrationHelper)
      : this (System.Reflection.Assembly.GetCallingAssembly ().GetName ().Name, migrationHelper)
    {
      m_migrationHelper = migrationHelper;
    }

    /// <summary>
    /// <see cref="IDatabaseConnectionStatus"/>
    /// </summary>
    public bool IsDatabaseConnectionUp => Lemoine.ModelDAO.ModelDAOHelper.DAOFactory?.IsDatabaseConnectionUp ?? false;

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public async Task CreateAndInitializeConnectionAsync (CancellationToken? cancellationToken = null)
    {
      // TODO: difference between initializing dao factory and the connection

      if (Lemoine.ModelDAO.ModelDAOHelper.IsInitialized ()) {
        log.Warn ("CreateAndInitializeConnectionAsync: a DAO Factory is already set");
      }

      NHibernateHelper.ApplicationName = m_applicationName;
      NHibernateHelper.KillOrphanedConnectionsFirst = this.KillOrphanedConnectionsFirst;
      NHibernateHelper.MigrationHelper = m_migrationHelper;
      log.Info ("CreateAndInitializeConnectionAsync: create the model factory and initialize the connection");
      try {
        await Lemoine.GDBPersistentClasses.GDBPersistentClassFactory
          .CreateModelFactoryAndInitializeConnectionAsync (cancellationToken: cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"CreateAndInitializeConnectionAsync: CreateModelFactoryAndInitializeConnectionAsync in exception", ex);
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

      NHibernateHelper.ApplicationName = m_applicationName;
      NHibernateHelper.KillOrphanedConnectionsFirst = this.KillOrphanedConnectionsFirst;
      NHibernateHelper.MigrationHelper = m_migrationHelper;
      log.Info ("CreateAndInitializeConnectionAsync: create the model factory and initialize the connection");
      try {
        await Lemoine.GDBPersistentClasses.GDBPersistentClassFactory
          .CreateModelFactoryAndInitializeConnectionAsync (maxNbAttempt, cancellationToken: cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"CreateAndInitializeConnectionAsync: CreateModelFactoryAndInitializeConnectionAsync in exception", ex);
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
        log.Warn ("CreateAndInitializeConnection: a DAO Factory is already set");
      }

      NHibernateHelper.ApplicationName = m_applicationName;
      NHibernateHelper.KillOrphanedConnectionsFirst = this.KillOrphanedConnectionsFirst;
      NHibernateHelper.MigrationHelper = m_migrationHelper;
      log.Info ("CreateAndInitializeConnection: create the model factory and initialize the connection");
      Lemoine.GDBPersistentClasses.GDBPersistentClassFactory
        .CreateModelFactoryAndInitializeConnection (cancellationToken: cancellationToken);
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="cancellationToken"></param>
    public void CreateAndInitializeConnection (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.IsInitialized ()) {
        log.Warn ("CreateAndInitializeConnection: a DAO Factory is already set");
      }

      NHibernateHelper.ApplicationName = m_applicationName;
      NHibernateHelper.KillOrphanedConnectionsFirst = this.KillOrphanedConnectionsFirst;
      NHibernateHelper.MigrationHelper = m_migrationHelper;
      log.Info ("CreateAndInitializeConnection: create the model factory and initialize the connection");
      Lemoine.GDBPersistentClasses.GDBPersistentClassFactory
        .CreateModelFactoryAndInitializeConnection (maxNbAttempt, cancellationToken: cancellationToken);
    }
  }
}
