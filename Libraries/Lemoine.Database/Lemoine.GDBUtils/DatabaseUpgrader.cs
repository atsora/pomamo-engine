// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using NHibernate.Cfg;
using System.Reflection;
using Lemoine.Info;
using Lemoine.Database.Persistent;
using Lemoine.ModelDAO;
using Lemoine.Database.Migration;
using Lemoine.ModelDAO.Interfaces;

namespace Lemoine.GDBUtils
{
  /// <summary>
  /// DatabaseUpgrader
  /// </summary>
  public class DatabaseUpgrader
  {
    /// <summary>
    /// Npgsql configuration key to use to set a command timeout
    /// 
    /// In the documentation, it is referenced Command Timeout
    /// The code property is named CommandTimeout
    /// 
    /// With Npgsql 4, both work
    /// </summary>
    static readonly string NPGSQL_COMMAND_TIMEOUT_KEY_KEY = "Database.Npgsql.CommandTimeout.Key";
    static readonly string NPGSQL_COMMAND_TIMEOUT_KEY_DEFAULT = "Command Timeout";

    readonly IConnectionInitializer m_connectionInitializer;
    readonly IDefaultValuesFactory m_defaultValuesFactory;
    readonly Assembly m_assembly;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DatabaseUpgrader (IConnectionInitializer connectionInitializer, IDefaultValuesFactory defaultValuesFactory, Assembly assembly)
    {
      Debug.Assert (null != connectionInitializer);
      Debug.Assert (null != defaultValuesFactory);
      Debug.Assert (null != assembly);

      m_connectionInitializer = connectionInitializer;
      m_defaultValuesFactory = defaultValuesFactory;
      m_assembly = assembly;
    }
    #endregion // Constructors

    /// <summary>
    /// Alternative to the constructor, create a database upgrader from a specific type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connectionInitializer"></param>
    /// <param name="defaultValuesFactory"></param>
    /// <returns></returns>
    public static DatabaseUpgrader Create<T> (IConnectionInitializer connectionInitializer, IDefaultValuesFactory defaultValuesFactory)
    {
      return new DatabaseUpgrader (connectionInitializer, defaultValuesFactory, typeof (T).Assembly);
    }

    /// <summary>
    /// Try to upgrade the database and log the results in logger
    /// </summary>
    /// <param name="applicationName"></param>
    /// <param name="log"></param>
    /// <returns>Success</returns>
    public bool Upgrade (string applicationName, ILog log)
    {
      try {
        string connectionString = GDBConnectionParameters.GetGDBConnectionString (applicationName);
        log.Info ($"Connection string={connectionString}");
        var nhibernateConfigManager = NHibernateConfigManager
          .CreateNoCacheNoExtensions (m_assembly);
        nhibernateConfigManager.Configure (connectionString);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("NHibernate successfully configured");
        }
        var configuration = nhibernateConfigManager.Configuration;

        var success = TryMigrateBuildSessionDefaultValues (configuration, log);
        if (success) {
          log.Info ("Upgrade success");
        }
        else {
          log.Error ("Upgrade not completed");
        }
        return success;
      }
      catch (Exception ex) {
        log.Error ("Upgrade failed", ex);
        return false;
      }
    }

    bool TryMigrateBuildSessionDefaultValues (Configuration configuration, ILog log)
    {
      try {
        bool migrationSuccess = false;
        bool insertDefaultValuesCompleted = false;
        int nbMigrationAttemptsWithInsertDefaultValuesCompleted = 0;
        for (int i = 1; (i <= 30) && !migrationSuccess && (0 == nbMigrationAttemptsWithInsertDefaultValuesCompleted); ++i) { // Try several times (30x max) migration/default values
          if (log.IsDebugEnabled) {
            log.Debug ($"Migrate/Default values attempt #{i}");
          }

          // Migrate the database if needed
          if (insertDefaultValuesCompleted) {
            ++nbMigrationAttemptsWithInsertDefaultValuesCompleted;
          }
          migrationSuccess = TryMigrate (configuration, log);

          var sessionFactory = configuration.BuildSessionFactory ();
          if (null == sessionFactory) {
            log.Fatal ("NHibernate session factory is null");
          }
          SessionFactoryInitializer.ForceSessionFactory (sessionFactory);

          m_connectionInitializer.CreateAndInitializeConnection ();

          // Reload types if needed (required if the extension citext was created)
          using (var session = new DAOSession (sessionFactory)) {
            var connection = NHibernateHelper.GetCurrentSession ().Connection;
            using (var command = connection.CreateCommand ()) {
              command.CommandText = $"CREATE EXTENSION IF NOT EXISTS citext;";
              command.ExecuteNonQuery ();
            }

            var npgsqlConnection = connection as Npgsql.NpgsqlConnection;
            npgsqlConnection?.ReloadTypes ();
          }

          // Add default values
          try {
            AddDefaultValues (sessionFactory);
            log.Info ("Default values were successfully inserted");
            insertDefaultValuesCompleted = true;
          }
          catch (Exception ex) {
            insertDefaultValuesCompleted = false;
            if (migrationSuccess) {
              log.Error ("Default values insertion failure", ex);
              throw;
            }
            else { // This is may be because the migration was not completed => try again
              log.Warn ("Default values insertion not completed while the migration failed, retry");
            }
          } // Add default values
        } // Attempt loop

        if (!migrationSuccess) {
          log.Error ("Migration error");
        }
        else if (log.IsDebugEnabled) {
          log.Debug ("Migration successful");
        }
        if (!insertDefaultValuesCompleted) {
          log.Error ("Default values not completed");
        }
        else if (log.IsDebugEnabled) {
          log.Debug ("Default values completed");
        }

        return migrationSuccess && insertDefaultValuesCompleted;
      }
      finally {
        SessionFactoryInitializer.ClearSessionFactory ();
      }
    }

    bool AddDefaultValues (NHibernate.ISessionFactory sessionFactory)
    {
      var defaultValues = m_defaultValuesFactory.CreateNocache (sessionFactory);
      return defaultValues.InsertDefaultValues ();
    }

    bool TryMigrate (Configuration configuration, ILog log)
    {
      Migrator.Migrator migrator = null;
      try {
        string connectionString = configuration.GetProperty ("connection.connection_string");
        // If not set, set CommandTimeout to 0 (no command time out)
        var connectionStringWithCommandTimeout = connectionString;
        var npgsqlCommandTimeoutKey = Lemoine.Info.ConfigSet
          .LoadAndGet (NPGSQL_COMMAND_TIMEOUT_KEY_KEY, NPGSQL_COMMAND_TIMEOUT_KEY_DEFAULT);
        if (!connectionStringWithCommandTimeout.Contains (npgsqlCommandTimeoutKey)) {
          connectionStringWithCommandTimeout += $"{npgsqlCommandTimeoutKey}=0;";
        }
        migrator = new Migrator.Migrator ("Postgre",
                                          connectionStringWithCommandTimeout,
                                          m_assembly);
        migrator.MigrateToLastVersion ();
        log.Info ($"Database migration successful. Last migration # is {migrator.GetLatestAppliedMigration (log)}");
        return true;
      }
      catch (Exception ex) {
        log.Error ("Database migration error", ex);
        if (null != migrator) {
          log.InfoFormat ("Database migration progress: {0}/{1}",
            migrator.GetLatestAppliedMigration (log),
            migrator.GetLastMigrationNumber (log));
        }
        return false;
      }
    }
  }
}
