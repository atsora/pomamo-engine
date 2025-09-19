// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using Lemoine.Extensions.Database;
using Lemoine.GDBUtils;
using Lemoine.Info;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Proxy;
using Lemoine.Database.Migration;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// NHibernate connection status
  /// </summary>
  public enum NHibernateStatus
  {
    /// <summary>
    /// Not initialized yet
    /// </summary>
    NotInitialized = 0,
    /// <summary>
    /// Start initializing
    /// </summary>
    StartInitializing = 1,
    /// <summary>
    /// Orphaned connections were killed but it is not configured yet
    /// </summary>
    NotConfigured = 2,
    /// <summary>
    /// Configured
    /// </summary>
    Configured = 3,
    /// <summary>
    /// Initialized
    /// </summary>
    Initialized = 4,
    /// <summary>
    /// Configuration error
    /// </summary>
    ConfigurationError = 5,
    /// <summary>
    /// Migration or default values or build session error
    /// </summary>
    MigrateDefaultError = 6,
    /// <summary>
    /// Build session error
    /// </summary>
    BuildSessionError = 7,
  }

  /// <summary>
  /// Utilities for NHibernate
  /// </summary>
  public static class NHibernateHelper
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NHibernateHelper).FullName);

    static ISession s_uniqueSession = null;

    /// <summary>
    /// Initializing status
    /// </summary>
    public static NHibernateStatus Status => SessionFactoryInitializer.Status;

    /// <summary>
    /// Get the sessionFactory for this application (singleton object)
    /// </summary>
    public static ISessionFactory SessionFactory => SessionFactoryInitializer.GetSessionFactory ();

    /// <summary>
    /// Registered application name
    /// 
    /// The application name may be used in the connection string
    /// </summary>
    public static string ApplicationName
    {
      get {
        return SessionFactoryInitializer.ApplicationName;
      }
      set {
        SessionFactoryInitializer.ApplicationName = value;
      }
    }

    /// <summary>
    /// Kill orhpaned connections before the first connection
    /// </summary>
    public static bool KillOrphanedConnectionsFirst
    {
      get {
        return SessionFactoryInitializer.KillOrphanedConnectionsFirst;
      }
      set {
        SessionFactoryInitializer.KillOrphanedConnectionsFirst = value;
      }
    }

    /// <summary>
    /// Associated migration helper
    /// </summary>
    public static IMigrationHelper MigrationHelper
    {
      get { return SessionFactoryInitializer.MigrationHelper; }
      set { SessionFactoryInitializer.MigrationHelper = value; }
    }

    /// <summary>
    /// Associated extensions provider
    /// </summary>
    public static IExtensionsProvider ExtensionsProvider
    {
      get { return SessionFactoryInitializer.ExtensionsProvider; }
      set { SessionFactoryInitializer.ExtensionsProvider = value; }
    }

    /// <summary>
    /// Add persistent class models to analyze for persistent classes
    /// 
    /// This must be done before the first connection
    /// </summary>
    /// <param name="persistentClassModels"></param>
    public static void AddPersistentClassModel (params IPersistentClassModel[] persistentClassModels)
    {
      SessionFactoryInitializer.AddPersistentClassModel (persistentClassModels);
    }

    /// <summary>
    /// Associated persistent class models
    /// </summary>
    public static IEnumerable<IPersistentClassModel> PersistentClassModels => SessionFactoryInitializer.PersistentClassModels;

    /// <summary>
    /// Associated session accumulators
    /// </summary>
    public static IEnumerable<ISessionAccumulator> SessionAccumulators = SessionFactoryInitializer.SessionAccumulators;

    /// <summary>
    /// Get the sessionFactory for this application (singleton object)
    /// </summary>
    public static async System.Threading.Tasks.Task<ISessionFactory> GetSessionFactoryAsync ()
    {
      return await SessionFactoryInitializer.GetSessionFactoryAsync ();
    }

    /// <summary>
    /// Get the current NHibernate session
    /// </summary>
    /// <returns></returns>
    public static ISession GetCurrentSession ()
    {
      if (null != s_uniqueSession) {
        log.Debug ("GetCurrentSession: return the unique session");
        return s_uniqueSession;
      }
      return NHibernateHelper.SessionFactory.GetCurrentSession ();
    }

    /// <summary>
    /// Get the associated NHibernate session of a IDAOSession
    /// </summary>
    /// <param name="daoSession"></param>
    /// <returns></returns>
    public static ISession GetSession (Lemoine.ModelDAO.IDAOSession daoSession)
    {
      return ((DAOSession)daoSession).Session;
    }

    /// <summary>
    /// Open a new session
    /// </summary>
    /// <returns>Session</returns>
    public static ISession OpenSession ()
    {
      return SessionFactory.OpenSession ();
    }

    /// <summary>
    /// Unproxy an element of an NHibernate entity
    /// 
    /// This must be run in a session
    /// </summary>
    /// <param name="obj"></param>
    public static void Unproxy<T> (ref T obj) where T : Lemoine.Model.ISerializableModel
    {
      if (null != obj) {
        obj.Unproxy ();
        if (obj is INHibernateProxy) {
          obj = (T)GetCurrentSession ().GetSessionImplementation ().PersistenceContext.Unproxy (obj);
        }
      }
    }

    /// <summary>
    /// Apply the equals function only if the two objects are not null, else return true only if they are both null
    /// </summary>
    /// <returns></returns>
    public static bool EqualsNullable<T, U> (T a, U b, Func<T, U, bool> equals)
    {
      return Lemoine.Model.Comparison.EqualsNullable<T, U> (a, b, equals);
    }
  }
}
