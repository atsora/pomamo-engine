// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Reflection;
using System.Linq;

using Lemoine.GDBUtils;
using Lemoine.Info;
using Lemoine.Core.Log;
using Migrator;
using Migrator.Framework;
using Lemoine.Database.Migration;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Class to help updating / downgrading GDB database
  /// </summary>
  public class MigrationHelper: IMigrationHelper
  {
    #region Members
    string m_connectionString;
    bool m_checkCache = true;
    Migrator.Migrator m_migrator = null; // To acess it, use the property Migrator instead
    string m_cacheFilePath = CACHE_FILE;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (MigrationHelper).FullName);

    static readonly string CACHE_FILE = "GDBMigrationCache";

    /// <summary>
    /// Npgsql configuration key to use to set a command timeout
    /// 
    /// In the documentation, it is referenced Command Timeout
    /// The code property is named CommandTimeout
    /// 
    /// With Npgsql 4, both work    /// </summary>
    static readonly string NPGSQL_COMMAND_TIMEOUT_KEY_KEY = "Database.Npgsql.CommandTimeout.Key";
    static readonly string NPGSQL_COMMAND_TIMEOUT_KEY_DEFAULT = "Command Timeout";

    #region Getters / Setters
    /// <summary>
    /// Connection string during the migration
    /// </summary>
    public string ConnectionString {
      get { return m_connectionString; }
      set { m_connectionString = value; }
    }
    
    /// <summary>
    /// Short connection string with only the first three elements:
    /// the server, the port and the database
    /// </summary>
    public string ShortConnectionString {
      get
      {
        return string.Join (";", m_connectionString.Split (new char[] {';'}).Take (3).ToArray ());
      }
    }
    
    /// <summary>
    /// Try to use the file CACHE_FILE to make the process faster
    /// Default is true.
    /// </summary>
    public bool CheckCache {
      get { return m_checkCache; }
      set { m_checkCache = value; }
    }
    
    /// <summary>
    /// Simulation mode.
    /// Default is false.
    /// </summary>
    public bool Simulate {
      get { return Migrator.DryRun; }
      set { Migrator.DryRun = value; }
    }
    
    /// <summary>
    /// Migrator
    /// </summary>
    public Migrator.Migrator Migrator {
      get {
        if (m_migrator == null) {
          // If not set, set Command Timeout to 0 (no command time out)
          var connectionStringWithCommandTimeout = m_connectionString;
          var npgsqlCommandTimeoutKey = Lemoine.Info.ConfigSet
            .LoadAndGet (NPGSQL_COMMAND_TIMEOUT_KEY_KEY, NPGSQL_COMMAND_TIMEOUT_KEY_DEFAULT);
          if (!connectionStringWithCommandTimeout.Contains (npgsqlCommandTimeoutKey)) {
            connectionStringWithCommandTimeout += $"{npgsqlCommandTimeoutKey}=0;";
          }
          m_migrator = new Migrator.Migrator ("Postgre",
                                              connectionStringWithCommandTimeout,
                                              Assembly.GetExecutingAssembly ());
        }
        return m_migrator;
      }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Default constructor.
    /// Get the connection string with the help of the
    /// <see cref="GDBConnectionParameters">GDBUtils.GDBConnectionParameters class</see>.
    /// </summary>
    public MigrationHelper ()
    {
      ConnectionString = GDBConnectionParameters.GDBConnectionString;
      InitializeCacheFilePath ();
    }

    /// <summary>
    /// Constructor with a connection string
    /// </summary>
    /// <param name="connectionStringArg"></param>
    public MigrationHelper(string connectionStringArg)
    {
      ConnectionString = connectionStringArg;
      InitializeCacheFilePath ();
    }

    /// <summary>
    /// Get the connection string with the help of the
    /// <see cref="GDBConnectionParameters">GDBUtils.GDBConnectionParameters class</see>.
    /// </summary>
    /// <param name="useCache">Try to use the cache file to make the process faster</param>
    public MigrationHelper (bool useCache)
    {
      ConnectionString = GDBConnectionParameters.GDBConnectionString;
      m_checkCache = useCache;
      InitializeCacheFilePath ();
    }

    /// <summary>
    /// Constructor with a connection string
    /// </summary>
    /// <param name="connectionStringArg"></param>
    /// <param name="useCache">Try to use the cache file to make the process faster</param>
    public MigrationHelper(string connectionStringArg, bool useCache)
    {
      ConnectionString = connectionStringArg;
      m_checkCache = useCache;
      InitializeCacheFilePath ();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Migrate to the given version
    /// 
    /// The file CACHE_FILE may prevent the database from migrating
    /// in some rare cases.
    /// In this time delete the CACHE_FILE manually first.
    /// </summary>
    /// <param name="toVersion">-1 for the latest</param>
    public void Migrate (int toVersion)
    {
      if (IsRequired () // To make the process faster
          || (toVersion != -1)) { // except when toVersion != -1
        if (toVersion == -1) {
          Migrator.MigrateToLastVersion ();
        }
        else {
          Migrator.MigrateTo (toVersion);
        }
        File.WriteAllText (this.m_cacheFilePath, this.ShortConnectionString);
      }
    }
    
    /// <summary>
    /// Migrate to the latest version
    /// </summary>
    public void Migrate ()
    {
      Migrate (-1);
    }
    
    /// <summary>
    /// Try to get from a cache file whether you should migrate or not ?
    /// </summary>
    /// <returns></returns>
    private bool IsRequired ()
    {
      if (false == m_checkCache) {
        return true;
      }
      
      if (false == File.Exists (this.m_cacheFilePath)) {
        return true;
      }
      
      Assembly ass = Assembly.GetCallingAssembly();
      if (ass.Location == null) {
        log.ErrorFormat ("MigrateFromCache: " +
                         "unable to get the location of the calling assembly");
        return true;
      }
      FileInfo configInfo = new FileInfo(this.m_cacheFilePath);
      FileInfo assInfo = new FileInfo(ass.Location);
      if (configInfo.LastWriteTime < assInfo.LastWriteTime) {
        log.InfoFormat ("MigrateFromCache: " +
                        "cache file is too old");
        return true;
      }
      
      return File.ReadAllText (this.m_cacheFilePath) != this.ShortConnectionString;
    }
    
    /// <summary>
    /// Initialize the cache file path
    /// 
    /// The cache file is searched in the installation directory.
    /// </summary>
    void InitializeCacheFilePath ()
    {
      m_cacheFilePath = Path.Combine (PulseInfo.LocalConfigurationDirectory, CACHE_FILE);
    }
    #endregion
  }
}
