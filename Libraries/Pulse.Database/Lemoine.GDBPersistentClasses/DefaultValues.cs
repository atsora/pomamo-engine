// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.Linq;
using System.Reflection;
using Lemoine.Info;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Collections.Generic;
using System;
using Lemoine.Database.Persistent;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Class to help inserting the default values in GDB database
  /// </summary>
  internal class DefaultValues: IDefaultValues
  {
    static readonly string CHECK_CACHE_KEY = "DefaultValues.CheckCache";
    static readonly bool CHECK_CACHE_DEFAULT = true;

    static readonly string MAX_NUMBER_EXCEPTIONS_KEY = "DefaultValues.MaxNumberExceptions";
    static readonly int MAX_NUMBER_EXCEPTIONS_DEFAULT = 10;

    #region Members
    int m_numberOfInsertDefaultValuesException = 0;
    ISessionFactory m_sessionFactory;
    string m_shortConnectionString;
    bool m_checkCache = true;
    string m_cacheFilePath = CACHE_FILE;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (DefaultValues).FullName);

    static readonly string CACHE_FILE = "GDBDefaultValuesCache";

    #region Getters / Setters
    /// <summary>
    /// Try to use the file CACHE_FILE to make the process faster
    /// Default is true.
    /// </summary>
    public bool CheckCache
    {
      get { return m_checkCache; }
      set { m_checkCache = value; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Default constructor when the cache is not used
    /// </summary>
    /// <param name="sessionFactory"></param>
    DefaultValues (ISessionFactory sessionFactory)
    {
      m_checkCache = false;
      m_sessionFactory = sessionFactory;
      m_shortConnectionString = null;
    }

    /// <summary>
    /// Create it when the cache is not used
    /// </summary>
    /// <param name="sessionFactory"></param>
    /// <returns></returns>
    internal static DefaultValues CreateNoCache (ISessionFactory sessionFactory)
    {
      return new DefaultValues (sessionFactory);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="sessionFactory"></param>
    /// <param name="connectionString"></param>
    public DefaultValues (ISessionFactory sessionFactory, string connectionString)
    {
      m_checkCache = Lemoine.Info.ConfigSet.LoadAndGet<bool> (CHECK_CACHE_KEY, CHECK_CACHE_DEFAULT);

      m_sessionFactory = sessionFactory;
      // Keep only the first three elements of the connection string:
      // the server, the port and the database name
      m_shortConnectionString = string.Join (";", connectionString.Split (new char[] { ';' }).Take (3).ToArray ());
      InitializeCacheFilePath ();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Insert the default values if they have not been set before
    /// 
    /// The file CACHE_FILE may prevent the database from migrating
    /// in some rare cases.
    /// In this time delete the CACHE_FILE manually first.
    /// </summary>
    /// <param name="writeCacheFile">Write a small cache file not to try to insert the default values a second time</param>
    /// <returns>false if only partially completed</returns>
    public bool ConnectAndInsertDefaultValues (bool writeCacheFile)
    {
      if (IsRequired ()) { // To make the process faster
        try {
          if (log.IsDebugEnabled) {
            log.Debug ("ConnectAndInsertDefaultValues: run InsertDefaultValues");
          }
          if (InsertDefaultValues ()) {
            if (writeCacheFile && (null != m_shortConnectionString)) {
              log.InfoFormat ("ConnectAndInsertDefaultValues: " +
                              "success, write the result it in {0}",
                              this.m_cacheFilePath);
              File.WriteAllText (this.m_cacheFilePath, m_shortConnectionString);
            }
            return true;
          }
          else {
            log.InfoFormat ("ConnectAndInsertDefaultValues: not completed");
            return false;
          }
        }
        catch (Exception ex) {
          log.Fatal ("ConnectAndInsertDefaultValues: InsertDefaultValues crashed", ex);
          var maxNumberOfInsertDefaultValuesExceptions = Lemoine.Info.ConfigSet
            .LoadAndGet<int> (MAX_NUMBER_EXCEPTIONS_KEY, MAX_NUMBER_EXCEPTIONS_DEFAULT);
          if (++m_numberOfInsertDefaultValuesException <= maxNumberOfInsertDefaultValuesExceptions) {
            log.InfoFormat ("ConnectAndInsertDefaultValues: attempt {0} VS {1} => try again",
              m_numberOfInsertDefaultValuesException, maxNumberOfInsertDefaultValuesExceptions);
            return false;
          }
          else {
            log.FatalFormat ("ConnectAndInsertDefaultValues: attempt {0} VS {1} => stop",
              m_numberOfInsertDefaultValuesException, maxNumberOfInsertDefaultValuesExceptions);
            return true;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Insert the default values if they have not been set before
    /// 
    /// The file CACHE_FILE may prevent the database from migrating
    /// in some rare cases.
    /// In this time delete the CACHE_FILE manually first.
    /// </summary>
    /// <param name="writeCacheFile">Write a small cache file not to try to insert the default values a second time</param>
    /// <returns>false if only partially completed</returns>
    public async System.Threading.Tasks.Task<bool> ConnectAndInsertDefaultValuesAsync (bool writeCacheFile)
    {
      if (await IsRequiredAsync ()) { // To make the process faster
        try {
          if (log.IsDebugEnabled) {
            log.Debug ("ConnectAndInsertDefaultValues: run InsertDefaultValues");
          }
          if (await InsertDefaultValuesAsync ()) {
            if (writeCacheFile && (null != m_shortConnectionString)) {
              log.InfoFormat ("ConnectAndInsertDefaultValues: " +
                              "success, write the result it in {0}",
                              this.m_cacheFilePath);
              // Note: File.WriteAllTextAsync is not available in .NET Standard, only in .NET Core
#if NETCOREAPP
              await File.WriteAllTextAsync (this.m_cacheFilePath, m_shortConnectionString);
#else // !NETCOREAPP
              using (var sw = new StreamWriter (m_cacheFilePath)) {
                await sw.WriteAsync (m_shortConnectionString);
              }
#endif // NETCOREAPP
            }
            return true;
          }
          else {
            log.InfoFormat ("ConnectAndInsertDefaultValues: not completed");
            return false;
          }
        }
        catch (Exception ex) {
          log.Fatal ("ConnectAndInsertDefaultValues: InsertDefaultValues crashed", ex);
          var maxNumberOfInsertDefaultValuesExceptions = Lemoine.Info.ConfigSet
            .LoadAndGet<int> (MAX_NUMBER_EXCEPTIONS_KEY, MAX_NUMBER_EXCEPTIONS_DEFAULT);
          if (++m_numberOfInsertDefaultValuesException <= maxNumberOfInsertDefaultValuesExceptions) {
            log.InfoFormat ("ConnectAndInsertDefaultValues: attempt {0} VS {1} => try again",
              m_numberOfInsertDefaultValuesException, maxNumberOfInsertDefaultValuesExceptions);
            return false;
          }
          else {
            log.FatalFormat ("ConnectAndInsertDefaultValues: attempt {0} VS {1} => stop",
              m_numberOfInsertDefaultValuesException, maxNumberOfInsertDefaultValuesExceptions);
            return true;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Complete this method with all the default values
    /// </summary>
    /// <returns>false if only partially completed</returns>
    public bool InsertDefaultValues ()
    {
      bool completed = true;
      using (IDAOSession session = new DAOSession (m_sessionFactory)) {
        // Note: use here auto-commit, so that in case of error the previous default values are inserted
        IList<long> migrations = new List<long> ();
        using (var command = m_sessionFactory.GetCurrentSession ().Connection.CreateCommand ()) {
          command.CommandText = @"SELECT version FROM schemainfo";
          using (var reader = command.ExecuteReader ()) {
            while (reader.Read ()) {
              migrations.Add ((long)reader[0]);
            }
          }
        }

        if (!(new DisplayDAO ()).InsertDefaultValues (migrations)) { // Do Display before Config because Config is using indirectly some Display settings
          log.Info ("InsertDefaultValues: false will be returned because InsertDefaultValues of DisplayDAO returned false");
          completed = false;
        }
        if (!(new ConfigDAO ()).InsertDefaultValues (migrations)) {
          log.Info ($"InsertDefaultValues: false will be returned because InsertDefaultValues of ConfigDAO retuned false");
          completed = false;
        }
        (new TranslationDAO ()).InsertDefaultValues ();
        if (migrations.Contains (267)) { // goaltype
          (new GoalTypeDAO ()).InsertDefaultValues ();
        }
        else {
          log.Info ("InsertDefaultValues: false will be returned because the migration 267 is not completed");
          completed = false;
        }
        (new MachineMonitoringTypeDAO ()).InsertDefaultValues ();
        (new MachineModeDAO ()).InsertDefaultValues ();
        (new MachineObservationStateDAO ()).InsertDefaultValues ();
        (new MachineStateTemplateDAO ()).InsertDefaultValues ();
        (new ReasonGroupDAO ()).InsertDefaultValues ();
        (new ReasonDAO ()).InsertDefaultValues ();
        if (migrations.Contains (540)) { // machinemodedefaultreasonauto
          (new MachineModeDefaultReasonDAO ()).InsertDefaultValues ();
          (new ReasonSelectionDAO ()).InsertDefaultValues (); // Must be after MachineModeDefaultReasonDAO...
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 540 is not completed");
          completed = false;
        }
        if (migrations.Contains (1005)) {
          (new ProductionStateDAO ()).InsertDefaultValues ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 1005 is not completed");
          completed = false;
        }
        if (migrations.Contains (30)) {
          (new FieldDAO ()).InsertDefaultValues ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 30 is not completed");
          completed = false;
        }
        (new FieldLegendDAO ()).InsertDefaultValues ();
        (new RoleDAO ()).InsertDefaultValues ();
        (new UnitDAO ()).InsertDefaultValues ();
        (new EventLevelDAO ()).InsertDefaultValues ();
        if (migrations.Contains (128)) {
          (new ApplicationStateDAO ()).InsertDefaultValues ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 128 is not completed");
          completed = false;
        }
        if (migrations.Contains (516)) {
          (new CncAlarmSeverityDAO ()).InsertDefaultValues ();
          ModelDAOHelper.DAOFactory.FlushData ();
          (new CncAlarmSeverityPatternDAO ()).InsertDefaultValues (); // Must be after CncAlarmSeverityDAO
          ModelDAOHelper.DAOFactory.FlushData ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 516 is not completed");
          completed = false;
        }
      }
      return completed;
    }

    /// <summary>
    /// Complete this method with all the default values
    /// </summary>
    /// <returns>false if only partially completed</returns>
    public async System.Threading.Tasks.Task<bool> InsertDefaultValuesAsync ()
    {
      bool completed = true;
      using (IDAOSession session = new DAOSession (m_sessionFactory)) {
        // Note: use here auto-commit, so that in case of error the previous default values are inserted
        IList<long> migrations = new List<long> ();
        using (var command = m_sessionFactory.GetCurrentSession ().Connection.CreateCommand ()) {
          command.CommandText = @"SELECT version FROM schemainfo";
          using (var reader = await command.ExecuteReaderAsync ()) {
            while (await reader.ReadAsync ()) {
              migrations.Add ((long)reader[0]);
            }
          }
        }

        // TODO: All InsertDefaultValues async

        if (!(new DisplayDAO ()).InsertDefaultValues (migrations)) { // Do Display before Config because Config is using indirectly some Display settings
          log.InfoFormat ("InsertDefaultValues: false will be returned because InsertDefaultValues of DisplayDAO returned false");
          completed = false;
        }
        if (!(new ConfigDAO ()).InsertDefaultValues (migrations)) {
          log.Info ($"InsertDefaultValues: false will be returned because InsertDefaultValues of ConfigDAO retuned false");
          completed = false;
        }
        (new TranslationDAO ()).InsertDefaultValues ();
        if (migrations.Contains (267)) { // goaltype
          (new GoalTypeDAO ()).InsertDefaultValues ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 267 is not completed");
          completed = false;
        }
        (new MachineMonitoringTypeDAO ()).InsertDefaultValues ();
        (new MachineModeDAO ()).InsertDefaultValues ();
        (new MachineObservationStateDAO ()).InsertDefaultValues ();
        (new MachineStateTemplateDAO ()).InsertDefaultValues ();
        (new ReasonGroupDAO ()).InsertDefaultValues ();
        (new ReasonDAO ()).InsertDefaultValues ();
        if (migrations.Contains (540)) { // machinemodedefaultreasonauto
          (new MachineModeDefaultReasonDAO ()).InsertDefaultValues ();
          (new ReasonSelectionDAO ()).InsertDefaultValues (); // Must be after MachineModeDefaultReasonDAO...
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 540 is not completed");
          completed = false;
        }
        if (migrations.Contains (1005)) {
          (new ProductionStateDAO ()).InsertDefaultValues ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 1005 is not completed");
          completed = false;
        }
        if (migrations.Contains (30)) {
          (new FieldDAO ()).InsertDefaultValues ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 30 is not completed");
          completed = false;
        }
        (new FieldLegendDAO ()).InsertDefaultValues ();
        (new RoleDAO ()).InsertDefaultValues ();
        (new UnitDAO ()).InsertDefaultValues ();
        (new EventLevelDAO ()).InsertDefaultValues ();
        if (migrations.Contains (128)) {
          (new ApplicationStateDAO ()).InsertDefaultValues ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 128 is not completed");
          completed = false;
        }
        if (migrations.Contains (516)) {
          (new CncAlarmSeverityDAO ()).InsertDefaultValues ();
          ModelDAOHelper.DAOFactory.FlushData ();
          (new CncAlarmSeverityPatternDAO ()).InsertDefaultValues (); // Must be after CncAlarmSeverityDAO
          ModelDAOHelper.DAOFactory.FlushData ();
        }
        else {
          log.InfoFormat ("InsertDefaultValues: false will be returned because the migration 516 is not completed");
          completed = false;
        }
      }
      return completed;
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

      if (null == m_shortConnectionString) {
        return true;
      }

      if (false == File.Exists (this.m_cacheFilePath)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsRequired: {0} does not exist => return true", m_cacheFilePath);
        }
        return true;
      }
      if (log.IsDebugEnabled) {
        log.DebugFormat ("IsRequired: {0} exists, check it", m_cacheFilePath);
      }

      Assembly ass;
      try {
        ass = Assembly.GetCallingAssembly ();
        if (ass.Location == null) {
          log.ErrorFormat ("IsRequired: " +
                           "unable to get the location of the calling assembly");
          return true;
        }
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsRequired: calling assembly is {0}", ass.Location);
        }
      }
      catch (Exception ex) {
        log.Fatal ("IsRequired: exception while getting the calling assembly", ex);
        return true;
      }

      try {
        FileInfo configInfo = new FileInfo (this.m_cacheFilePath);
        FileInfo assInfo = new FileInfo (ass.Location);
        if (configInfo.LastWriteTime < assInfo.LastWriteTime) {
          log.InfoFormat ("IsRequired: " +
                          "cache file is too old");
          return true;
        }
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsRequired: cache file {0} is recent enough {1} VS {2}",
            m_cacheFilePath, configInfo.LastWriteTime, assInfo.LastWriteTime);
        }
      }
      catch (Exception ex) {
        log.Fatal ("IsRequired: exception while checking the times", ex);
        return true;
      }

      string fileContent = null;
      try {
        fileContent = File.ReadAllText (m_cacheFilePath);
        var result = !string.Equals (fileContent, m_shortConnectionString, StringComparison.InvariantCultureIgnoreCase);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsRequired: final result is {0}", result);
        }
        return result;
      }
      catch (Exception ex) {
        log.Fatal ("IsRequired: exception while checking the file content", ex);
        log.FatalFormat ("IsRequired: fileContent={0} VS connectionString={1}", fileContent, m_shortConnectionString);
        return true;
      }
    }

    /// <summary>
    /// Try to get from a cache file whether you should migrate or not ?
    /// </summary>
    /// <returns></returns>
    private async System.Threading.Tasks.Task<bool> IsRequiredAsync ()
    {
      if (false == m_checkCache) {
        return true;
      }

      if (null == m_shortConnectionString) {
        return true;
      }

      if (false == File.Exists (this.m_cacheFilePath)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsRequired: {0} does not exist => return true", m_cacheFilePath);
        }
        return true;
      }
      if (log.IsDebugEnabled) {
        log.DebugFormat ("IsRequired: {0} exists, check it", m_cacheFilePath);
      }

      Assembly ass;
      try {
        ass = Assembly.GetCallingAssembly ();
        if (ass.Location == null) {
          log.ErrorFormat ("IsRequired: " +
                           "unable to get the location of the calling assembly");
          return true;
        }
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsRequired: calling assembly is {0}", ass.Location);
        }
      }
      catch (Exception ex) {
        log.Fatal ("IsRequired: exception while getting the calling assembly", ex);
        return true;
      }

      try {
        FileInfo configInfo = new FileInfo (this.m_cacheFilePath);
        FileInfo assInfo = new FileInfo (ass.Location);
        if (configInfo.LastWriteTime < assInfo.LastWriteTime) {
          log.InfoFormat ("IsRequired: " +
                          "cache file is too old");
          return true;
        }
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsRequired: cache file {0} is recent enough {1} VS {2}",
            m_cacheFilePath, configInfo.LastWriteTime, assInfo.LastWriteTime);
        }
      }
      catch (Exception ex) {
        log.Fatal ("IsRequired: exception while checking the times", ex);
        return true;
      }

      string fileContent = null;
      try {
        // Note: File.ReadAllTextAsync is only available in .NET Core, not .NET Standard
#if NETCOREAPP
        fileContent = await File.ReadAllTextAsync (m_cacheFilePath);
#else // !NETCOREAPP
        using (var sr = new StreamReader (m_cacheFilePath)) {
          fileContent = await sr.ReadToEndAsync ();
        }
#endif // NETCOREAPP
        var result = !string.Equals (fileContent, m_shortConnectionString, StringComparison.InvariantCultureIgnoreCase);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsRequired: final result is {0}", result);
        }
        return result;
      }
      catch (Exception ex) {
        log.Fatal ("IsRequired: exception while checking the file content", ex);
        log.FatalFormat ("IsRequired: fileContent={0} VS connectionString={1}", fileContent, m_shortConnectionString);
        return true;
      }
    }

    /// <summary>
    /// Initialize the cache file path
    /// 
    /// The cache file is searched in the installation directory.
    /// </summary>
    void InitializeCacheFilePath ()
    {
      m_cacheFilePath = System.IO.Path.Combine (PulseInfo.LocalConfigurationDirectory, CACHE_FILE);
    }
#endregion
  }
}
