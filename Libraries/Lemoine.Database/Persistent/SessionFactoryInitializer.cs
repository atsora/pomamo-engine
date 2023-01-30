// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Database.Migration;
using Lemoine.Extensions.Database;
using Lemoine.Extensions.Interfaces;
using Lemoine.GDBUtils;
using Lemoine.Info;
using Lemoine.Threading;
using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Lemoine.Database.Persistent
{
  static class NHibernateStatusExtensions
  {
    /// <summary>
    /// Is the NHiberateStatus in one of the initializing status ?
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsInitializing (this NHibernateStatus s)
    {
      switch (s) {
      case NHibernateStatus.StartInitializing:
      case NHibernateStatus.NotConfigured:
      case NHibernateStatus.Configured:
        return true;
      default:
        return false;
      }
    }
  }

  /// <summary>
  /// Singleton class to initialize the SessionFactory
  /// </summary>
  internal class SessionFactoryInitializer
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SessionFactoryInitializer).FullName);

    #region Constructors
    /// <summary>
    /// Private constructor => singleton
    /// </summary>
    SessionFactoryInitializer ()
    {
    }
    #endregion // Constructors

    static readonly string INSERT_DEFAULT_VALUES_KEY = "Database.InsertDefaultValues";
    static readonly bool INSERT_DEFAULT_VALUES_DEFAULT = true;

    static readonly string MAX_WAIT_TIME_KEY = "Database.MaxWaitTime";
    static readonly TimeSpan MAX_WAIT_TIME_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string TRY_MIGRATE_AND_ADD_DEFAULT_KEY = "Database.TryMigrateAndAddDefault";
    static readonly bool TRY_MIGRATE_AND_ADD_DEFAULT_DEFAULT = false;

    #region Members
    IMigrationHelper m_migrationHelper;
    IExtensionsProvider m_extensionsProvider = null;
    volatile NHibernateStatus m_status = NHibernateStatus.NotInitialized;
    NHibernate.Cfg.Configuration m_configuration = null;
    volatile ISessionFactory m_sessionFactory = null;
    SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim (1, 1);
    string m_applicationName = null;
    bool m_killOrphanedConnectionsFirst = false;
    ConcurrentDictionary<Type, IPersistentClassModel> m_persistentClassModels = new ConcurrentDictionary<Type, IPersistentClassModel> ();
    #endregion

    /// <summary>
    /// Associated migration helper
    /// </summary>
    public static IMigrationHelper MigrationHelper
    {
      get {
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (Instance.m_semaphoreSlim)) // make it thread safe
        {
          return Instance.m_migrationHelper;
        }
      }
      set {
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (Instance.m_semaphoreSlim)) // make it thread safe
        {
          Instance.m_migrationHelper = value;
        }
      }
    }

    /// <summary>
    /// Associated extensions provider
    /// </summary>
    public static IExtensionsProvider ExtensionsProvider
    {
      get {
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (Instance.m_semaphoreSlim)) // make it thread safe
        {
          return Instance.m_extensionsProvider;
        }
      }
      set {
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (Instance.m_semaphoreSlim)) // make it thread safe
        {
          Instance.m_extensionsProvider = value;
        }
      }
    }

    /// <summary>
    /// Registered application name
    /// 
    /// The application name may be used in the connection string
    /// </summary>
    public static string ApplicationName
    {
      get {
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (Instance.m_semaphoreSlim)) // make it thread safe
        {
          return Instance.m_applicationName;
        }
      }
      set {
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (Instance.m_semaphoreSlim)) // make it thread safe
        {
          Instance.m_applicationName = value;
        }
      }
    }

    /// <summary>
    /// Kill orhpaned connections before the first connection
    /// </summary>
    public static bool KillOrphanedConnectionsFirst
    {
      get {
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (Instance.m_semaphoreSlim)) // make it thread safe
        {
          return Instance.m_killOrphanedConnectionsFirst;
        }
      }
      set {
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (Instance.m_semaphoreSlim)) // make it thread safe
        {
          Instance.m_killOrphanedConnectionsFirst = value;
        }
      }
    }

    /// <summary>
    /// Add persistent class models to analyze for persistent classes
    /// 
    /// This must be done before the first connection
    /// </summary>
    /// <param name="persistentClassModels"></param>
    public static void AddPersistentClassModel (params IPersistentClassModel[] persistentClassModels)
    {
      if (!Instance.m_status.Equals (NHibernateStatus.NotInitialized)) {
        log.Error ($"AddPersistentClassModel: the status is already {Instance.m_status}, the call to this method is too late");
      }

      foreach (var persistentClassModel in persistentClassModels) {
        if (!Instance.m_persistentClassModels.TryAdd (persistentClassModel.GetType (), persistentClassModel)) {
          log.Error ($"AddPersistentClassModel: {persistentClassModel.GetType ()} has already been added");
        }
      }
    }

    /// <summary>
    /// Associated persistent class models
    /// </summary>
    public static IEnumerable<IPersistentClassModel> PersistentClassModels => Instance.m_persistentClassModels.Values;

    /// <summary>
    /// Associated session accumulators
    /// </summary>
    public static IEnumerable<ISessionAccumulator> SessionAccumulators = PersistentClassModels.Where (x => null != x.SessionAccumulator).Select (x => x.SessionAccumulator);

    /// <summary>
    /// Initializing status
    /// </summary>
    public static NHibernateStatus Status
    {
      get {
        return Instance.m_status;
      }
    }

    /// <summary>
    /// Get the sessionFactory for this application (singleton object)
    /// </summary>
    public static ISessionFactory SessionFactory => GetSessionFactory ();

    /// <summary>
    /// Get the sessionFactory for this application (singleton object)
    /// </summary>
    public static ISessionFactory GetSessionFactory ()
    {
      return Instance.GetInstanceSessionFactory ();
    }

    /// <summary>
    /// Get the sessionFactory for this application (singleton object)
    /// </summary>
    public static async System.Threading.Tasks.Task<ISessionFactory> GetSessionFactoryAsync ()
    {
      return await Instance.GetInstanceSessionFactoryAsync ();
    }

    ISessionFactory GetInstanceSessionFactory ()
    {
      if (m_sessionFactory == null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetSessionFactory: session factory is null status={m_status}");
        }
        if (m_status.IsInitializing ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetSessionFactory: is initializing status={m_status}");
          }
          // Limit the time, return an exception after some time
          var sleepTimeMilliseconds = 10;
          var maxWaitTime = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (MAX_WAIT_TIME_KEY, MAX_WAIT_TIME_DEFAULT);
          var maxDateTime = DateTime.UtcNow.Add (maxWaitTime);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetSessionFactory: initializing maxDateTime={maxDateTime} status={m_status}");
          }
          while (m_status.IsInitializing ()) {
            if (maxDateTime < DateTime.UtcNow) {
              log.Error ($"GetSessionFactory: date/time={maxDateTime}, max wait time={maxWaitTime} reached");
              throw new Exception ("Max wait time reached");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetSessionFactory: about to sleep {sleepTimeMilliseconds} status={m_status}");
              }
              System.Threading.Thread.Sleep (sleepTimeMilliseconds);
              sleepTimeMilliseconds = 100; // Initial sleep time is 10ms, then it is 100ms
            }
          }
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"GetSessionFactory: about to get the connection string for {m_applicationName} before acquiring the semaphore");
        }
        // Note: because GetGDBConnectionString is using ConfigSet, which may use a database access, this must not be called inside the critical section
        // else the application may be locked
        string connectionString = GDBConnectionParameters.GetGDBConnectionString (m_applicationName);

        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (m_semaphoreSlim)) // make it thread safe
        {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetSessionFactory: semaphore acquired");
          }

          if (null == m_sessionFactory) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetSessionFactory: status={m_status} connectionString={connectionString}");
            }
            switch (m_status) {
            case NHibernateStatus.NotInitialized:
              if (log.IsDebugEnabled) {
                log.Debug ("GetSessionFactory: NotInitialized => StartInitializing");
              }
              m_status = NHibernateStatus.StartInitializing;
              goto case NHibernateStatus.StartInitializing;
            case NHibernateStatus.StartInitializing:
              if (m_killOrphanedConnectionsFirst) {
                try {
                  Lemoine.GDBUtils.ConnectionTools.KillOrphanedConnections (connectionString);
                  m_status = NHibernateStatus.NotConfigured;
                }
                catch (Exception ex) {
                  log.Error ("GetSessionFactory: KillOrphanedConnections failed but continue", ex);
                }
              }
              else {
                m_status = NHibernateStatus.NotConfigured;
              }
              if (log.IsDebugEnabled) {
                log.Debug ($"GetSessionFactory: StartInitializing => {m_status}");
              }
              goto case NHibernateStatus.NotConfigured;
            case NHibernateStatus.NotConfigured:
            case NHibernateStatus.ConfigurationError:
              try {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetSessionFactory: build configuration with {m_persistentClassModels.Count} persistent class models");
                }
                var assemblies = m_persistentClassModels.Values
                  .SelectMany (x => x.Assemblies)
                  .ToArray ();
                m_configuration = BuildConfiguration (connectionString, assemblies);
                m_status = NHibernateStatus.Configured;
              }
              catch (Exception ex) {
                log.Fatal ("GetSessionFactory: configuration exception", ex);
                m_status = NHibernateStatus.ConfigurationError;
                throw;
              }
              if (log.IsDebugEnabled) {
                log.Debug ($"GetSessionFactory: NotConfigured/ConfigurationError => {m_status}");
              }
              goto case NHibernateStatus.Configured;
            case NHibernateStatus.Configured:
            case NHibernateStatus.MigrateDefaultError:
            case NHibernateStatus.BuildSessionError:
              var tryMigrateAndAddDefault = Lemoine.Info.ConfigSet.LoadAndGet<bool> (TRY_MIGRATE_AND_ADD_DEFAULT_KEY,
                TRY_MIGRATE_AND_ADD_DEFAULT_DEFAULT);
              try {
                if (tryMigrateAndAddDefault) {
                  TryMigrateBuildSessionDefaultValues (m_configuration);
                }
                else {
                  TryBuildSessionOnly (m_configuration);
                }
                if (null == m_sessionFactory) {
                  log.Fatal ("GetSessionFactory: session factory is null after build session");
                  m_status = NHibernateStatus.BuildSessionError;
                }
                else {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"GetSessionFactory: session factory is ok with migrateAndAddDefault={tryMigrateAndAddDefault} => switch to Initialized");
                  }
                  m_configuration = null; // Not needed any more, it can be reset to save some memory
                  m_status = NHibernateStatus.Initialized;
                }
              }
              catch (Exception ex) {
                log.Fatal ($"GetSessionFactory: Try...BuildSession... exception, tryMigrateAndAddDefault={tryMigrateAndAddDefault}", ex);
                if (tryMigrateAndAddDefault) {
                  m_status = NHibernateStatus.MigrateDefaultError;
                }
                else {
                  m_status = NHibernateStatus.BuildSessionError;
                }
                throw;
              }
              if (log.IsDebugEnabled) {
                log.Debug ($"GetSessionFactory: Configured/MigrateDefaultError => {m_status}");
              }
              break;
            }
          }
        }
      }

      if ((null != m_sessionFactory) && m_sessionFactory.IsClosed) {
        log.FatalFormat ("GetSessionFactory: " +
                         "sessionFactory is closed although it should not be");
      }

      return m_sessionFactory;
    }

    async System.Threading.Tasks.Task<ISessionFactory> GetInstanceSessionFactoryAsync ()
    {
      if (m_sessionFactory == null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetInstanceSessionFactoryAsync: session factory is null status={m_status}");
        }
        if (m_status.IsInitializing ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetInstanceSessionFactoryAsync: is initializing status={m_status}");
          }
          // Limit the time, return an exception after some time
          var sleepTimeMilliseconds = 10;
          var maxWaitTime = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (MAX_WAIT_TIME_KEY, MAX_WAIT_TIME_DEFAULT);
          var maxDateTime = DateTime.UtcNow.Add (maxWaitTime);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetInstanceSessionFactoryAsync: initializing maxDateTime={maxDateTime} status={m_status}");
          }
          while (m_status.IsInitializing ()) {
            if (maxDateTime < DateTime.UtcNow) {
              log.Error ($"GetInstanceSessionFactoryAsync: date/time={maxDateTime}, max wait time={maxWaitTime} reached");
              throw new Exception ("Max wait time reached");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetInstanceSessionFactoryAsync: about to sleep {sleepTimeMilliseconds} status={m_status}");
              }
              await System.Threading.Tasks.Task.Delay (sleepTimeMilliseconds);
              sleepTimeMilliseconds = 100; // Initial sleep time is 10ms, then it is 100ms
            }
          }
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"GetInstanceSessionFactoryAsync: about to get the connection string for {m_applicationName} before acquiring the semaphore");
        }
        // Note: because GetGDBConnectionString is using ConfigSet, which may use a database access, this must not be called inside the critical section
        // else the application may be locked
        string connectionString = GDBConnectionParameters.GetGDBConnectionString (m_applicationName);

        using (var semaphoreSlimHolder = await SemaphoreSlimHolder.CreateAsync (m_semaphoreSlim)) // make it thread safe
        {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetInstanceSessionFactoryAsync: semaphore acquired");
          }

          if (null == m_sessionFactory) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetInstanceSessionFactoryAsync: status={m_status} connectionString={connectionString}");
            }
            switch (m_status) {
            case NHibernateStatus.NotInitialized:
              if (log.IsDebugEnabled) {
                log.Debug ("GetInstanceSessionFactoryAsync: NotInitialized => StartInitializing");
              }
              m_status = NHibernateStatus.StartInitializing;
              goto case NHibernateStatus.StartInitializing;
            case NHibernateStatus.StartInitializing:
              if (m_killOrphanedConnectionsFirst) {
                try {
                  await Lemoine.GDBUtils.ConnectionTools.KillOrphanedConnectionsAsync (connectionString);
                }
                catch (Exception ex) {
                  log.Error ("GetInstanceSessionFactoryAsync: KillOrphanedConnections failed but continue", ex);
                }
              }
              m_status = NHibernateStatus.NotConfigured;
              if (log.IsDebugEnabled) {
                log.Debug ($"GetInstanceSessionFactoryAsync: StartInitializing => {m_status}");
              }
              goto case NHibernateStatus.NotConfigured;
            case NHibernateStatus.NotConfigured:
            case NHibernateStatus.ConfigurationError:
              try {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetInstanceSessionFactoryAsync: build configuration with {m_persistentClassModels.Count} persitent class models");
                }
                var assemblies = m_persistentClassModels.Values
                  .SelectMany (x => x.Assemblies)
                  .ToArray ();
                m_configuration = await BuildConfigurationAsync (connectionString, assemblies);
                m_status = NHibernateStatus.Configured;
              }
              catch (Exception ex) {
                log.Fatal ("GetInstanceSessionFactoryAsync: configuration exception", ex);
                m_status = NHibernateStatus.ConfigurationError;
                throw;
              }
              if (log.IsDebugEnabled) {
                log.Debug ($"GetInstanceSessionFactoryAsync: NotConfigured/ConfigurationError => {m_status}");
              }
              goto case NHibernateStatus.Configured;
            case NHibernateStatus.Configured:
            case NHibernateStatus.MigrateDefaultError:
            case NHibernateStatus.BuildSessionError:
              var tryMigrateAndAddDefault = Lemoine.Info.ConfigSet.LoadAndGet<bool> (TRY_MIGRATE_AND_ADD_DEFAULT_KEY,
                TRY_MIGRATE_AND_ADD_DEFAULT_DEFAULT);
              try {
                if (tryMigrateAndAddDefault) {
                  await TryMigrateBuildSessionDefaultValuesAsync (m_configuration);
                }
                else {
                  await TryBuildSessionOnlyAsync (m_configuration);
                }
                if (null == m_sessionFactory) {
                  log.Fatal ("GetInstanceSessionFactoryAsync: session factory is null after build session");
                  m_status = NHibernateStatus.BuildSessionError;
                }
                else {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"GetInstanceSessionFactoryAsync: session factory is ok with tryMigrateAndAddDefault={tryMigrateAndAddDefault} => switch to Initialized");
                  }
                  m_configuration = null; // Not needed any more, it can be reset to save some memory
                  m_status = NHibernateStatus.Initialized;
                }
              }
              catch (Exception ex) {
                log.Fatal ($"GetInstanceSessionFactoryAsync: Try...BuildSession... exception, tryMigrateAndAddDefault={tryMigrateAndAddDefault}", ex);
                if (tryMigrateAndAddDefault) {
                  m_status = NHibernateStatus.MigrateDefaultError;
                }
                else {
                  m_status = NHibernateStatus.BuildSessionError;
                }
                throw;
              }
              if (log.IsDebugEnabled) {
                log.Debug ($"GetInstanceSessionFactoryAsync: Configured/MigrateDefaultError => {m_status}");
              }
              break;
            }
          }
        }
      }

      if ((null != m_sessionFactory) && m_sessionFactory.IsClosed) {
        log.Fatal ("GetInstanceSessionFactoryAsync: sessionFactory is closed although it should not be");
      }

      return m_sessionFactory;
    }

    /// <summary>
    /// Force a session factory
    /// </summary>
    /// <param name="sessionFactory"></param>
    internal static void ForceSessionFactory (ISessionFactory sessionFactory)
    {
      Debug.Assert (null != sessionFactory);

      Instance.m_sessionFactory = sessionFactory;
    }

    /// <summary>
    /// Clear the current session factory
    /// </summary>
    internal static void ClearSessionFactory ()
    {
      Instance.m_sessionFactory = null;
    }

    void TryBuildSessionOnly (Configuration configuration)
    {
      m_sessionFactory = configuration.BuildSessionFactory ();
      if (null == m_sessionFactory) {
        log.Fatal ("TryBuildSessionOnly: null sessionFactory");
      }
    }

    async System.Threading.Tasks.Task TryBuildSessionOnlyAsync (Configuration configuration)
    {
      m_sessionFactory = await System.Threading.Tasks.Task.Run<ISessionFactory> (() => configuration.BuildSessionFactory ());
      if (null == m_sessionFactory) {
        log.Fatal ("TryBuildSessionOnly: null sessionFactory");
      }
    }

    void TryMigrateBuildSessionDefaultValues (Configuration configuration)
    {
      bool migrationSuccess = false;
      bool insertDefaultValuesCompleted = false;
      int nbMigrationAttemptsWithInsertDefaultValuesCompleted = 0;
      for (int i = 0; (i < 10) && !migrationSuccess && (0 == nbMigrationAttemptsWithInsertDefaultValuesCompleted); ++i) { // Try several times (10x max) migration/default values
        log.Info ($"TryMigrateBuildSessionDefaultValues: migration+default values, attempt={i}");

        // Migrate the database if needed
        if (insertDefaultValuesCompleted) {
          ++nbMigrationAttemptsWithInsertDefaultValuesCompleted;
        }
        migrationSuccess = Migrate (configuration);

        m_sessionFactory = configuration.BuildSessionFactory ();
        if (null == m_sessionFactory) {
          log.Fatal ("TryMigrateBuildSessionDefaultValues: null sessionFactory");
        }

        // Add default values
        try {
          if (ConfigSet.LoadAndGet<bool> (INSERT_DEFAULT_VALUES_KEY, INSERT_DEFAULT_VALUES_DEFAULT)) {
            insertDefaultValuesCompleted = AddDefaultValues (configuration, migrationSuccess);
          }
          else {
            log.Warn ("TryMigrateBuildSessionDefaultValues: option not to insert the default values on");
            break; // In that case, this is useless to loop
          }
        }
        catch (Exception ex) {
          insertDefaultValuesCompleted = false;
          if (migrationSuccess) {
            log.Error ("TryMigrateBuildSessionDefaultValues: insert default values failed", ex);
            throw;
          }
          else { // This is may be because the migration was not completed => try again
            log.Warn ("TryMigrateBuildSessionDefaultValues: insert default values failed while the migration was not completed => try again", ex);
          }
        } // Add default values
      } // Attempt loop

      if (!migrationSuccess) {
        log.Error ("TryMigrateBuildSessionDefaultValues: migration not completed");
      }
      else if (log.IsDebugEnabled) {
        log.Debug ("TryMigrateBuildSessionDefaultValues: migration successful");
      }
      if (!insertDefaultValuesCompleted) {
        log.Error ("TryMigrateBuildSessionDefaultValues: default values not completed");
      }
      else if (log.IsDebugEnabled) {
        log.Debug ("TryMigrateBuildSessionDefaultValues: default values completed");
      }
    }

    async System.Threading.Tasks.Task TryMigrateBuildSessionDefaultValuesAsync (Configuration configuration)
    {
      bool migrationSuccess = false;
      bool insertDefaultValuesCompleted = false;
      int nbMigrationAttemptsWithInsertDefaultValuesCompleted = 0;
      for (int i = 0; (i < 10) && !migrationSuccess && (0 == nbMigrationAttemptsWithInsertDefaultValuesCompleted); ++i) { // Try several times (10x max) migration/default values
        log.Info ($"TryMigrateBuildSessionDefaultValues: migration+default values, attempt={i}");

        // Migrate the database if needed
        if (insertDefaultValuesCompleted) {
          ++nbMigrationAttemptsWithInsertDefaultValuesCompleted;
        }
        migrationSuccess = await MigrateAsync (configuration);

        m_sessionFactory = await System.Threading.Tasks.Task.Run<ISessionFactory> (() => configuration.BuildSessionFactory ());
        if (null == m_sessionFactory) {
          log.Fatal ("TryMigrateBuildSessionDefaultValues: null sessionFactory");
        }

        // Add default values
        try {
          if (ConfigSet.LoadAndGet<bool> (INSERT_DEFAULT_VALUES_KEY, INSERT_DEFAULT_VALUES_DEFAULT)) {
            insertDefaultValuesCompleted = await AddDefaultValuesAsync (configuration, migrationSuccess);
          }
          else {
            log.WarnFormat ("TryMigrateBuildSessionDefaultValues: option not to insert the default values on");
            break; // In that case, this is useless to loop
          }
        }
        catch (Exception ex) {
          insertDefaultValuesCompleted = false;
          if (migrationSuccess) {
            log.Error ("TryMigrateBuildSessionDefaultValues: insert default values failed", ex);
            throw;
          }
          else { // This is may be because the migration was not completed => try again
            log.Warn ("TryMigrateBuildSessionDefaultValues: insert default values failed while the migration was not completed => try again", ex);
          }
        } // Add default values
      } // Attempt loop

      if (!migrationSuccess) {
        log.Error ("TryMigrateBuildSessionDefaultValues: migration not completed");
      }
      else if (log.IsDebugEnabled) {
        log.Debug ("TryMigrateBuildSessionDefaultValues: migration successful");
      }
      if (!insertDefaultValuesCompleted) {
        log.Error ("TryMigrateBuildSessionDefaultValues: default values not completed");
      }
      else if (log.IsDebugEnabled) {
        log.Debug ("TryMigrateBuildSessionDefaultValues: default values completed");
      }
    }

    /// <summary>
    /// Try to migrate the database
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns>success</returns>
    bool Migrate (Configuration configuration)
    {
      // TODO: and what to do if m_migrationHelper is null

      if (m_migrationHelper is null) {
        log.Fatal ("MigrateAsync: no migration helper was set");
        Debug.Assert (false);
        return true;
      }

      try {
        m_migrationHelper.ConnectionString = configuration.GetProperty ("connection.connection_string");
        m_migrationHelper.Migrate ();
        return true;
      }
      catch (Exception ex) {
        log.Error ("Migrate: migration failed with error", ex);
        return false;
      }
    }

    /// <summary>
    /// Try to migrate the database
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns>success</returns>
    async System.Threading.Tasks.Task<bool> MigrateAsync (Configuration configuration)
    {
      // TODO: and what to do if m_migrationHelper is null

      if (m_migrationHelper is null) {
        log.Fatal ("MigrateAsync: no migration helper was set");
        Debug.Assert (false);
        return true;
      }

      try {
        m_migrationHelper.ConnectionString = configuration.GetProperty ("connection.connection_string");
        await System.Threading.Tasks.Task.Run (() => m_migrationHelper.Migrate ());
        return true;
      }
      catch (Exception ex) {
        log.Error ("Migrate: migration failed with error", ex);
        return false;
      }
    }

    bool AddDefaultValues (Configuration configuration, bool migrationSuccess)
    {
      var result = true;
      foreach (var persistentClassModel in m_persistentClassModels.Values) {
        result &= persistentClassModel.AddDefaultValues (m_sessionFactory, configuration, migrationSuccess);
      }
      return result;
    }

    async System.Threading.Tasks.Task<bool> AddDefaultValuesAsync (Configuration configuration, bool migrationSuccess)
    {
      var result = true;
      foreach (var persistentClassModel in m_persistentClassModels.Values) {
        result &= await persistentClassModel.AddDefaultValuesAsync (m_sessionFactory, configuration, migrationSuccess);
      }
      return result;
    }

    NHibernate.Cfg.Configuration BuildConfiguration (string connectionString, params Assembly[] assemblies)
    {
      log.Debug ("BuildConfiguration: initializing the NHibernate session factory");

      var nhibernateConfigManager = NHibernateConfigManager.Create (m_extensionsProvider, m_applicationName, assemblies);
      nhibernateConfigManager.Configure (connectionString);
      return nhibernateConfigManager.Configuration;
    }

    async System.Threading.Tasks.Task<NHibernate.Cfg.Configuration> BuildConfigurationAsync (string connectionString, params Assembly[] assemblies)
    {
      log.Debug ("BuildConfiguration: initializing the NHibernate session factory");

      var nhibernateConfigManager = await NHibernateConfigManager.CreateAsync (m_extensionsProvider, m_applicationName, assemblies);
      await nhibernateConfigManager.ConfigureAsync (connectionString);
      return nhibernateConfigManager.Configuration;
    }

    #region Instance
    static SessionFactoryInitializer Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
      static Nested () { }
      internal static readonly SessionFactoryInitializer instance = new SessionFactoryInitializer ();
    }
    #endregion // Instance
  }
}
