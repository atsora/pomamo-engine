// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Lemoine.Cnc.Data;
using Lemoine.Collections;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;
using Lemoine.Threading;
using Lemoine.Core.Log;
using System.Text.Json;
using MessagePack;
using MessagePack.Resolvers;

namespace Lemoine.Cnc.SQLiteQueue
{
  /// <summary>
  /// Description of SQLiteCncDataQueue.
  /// </summary>
  public sealed class SQLiteCncDataQueue
    : ICncDataQueue
    , IConfigurable
    , IDisposable
    , IChecked
    , ICheckedCaller
    , INamedCollection
  {
    static readonly string ENQUEUE_ATTEMPTS_KEY = "SQLiteQueue.Enqueue.Attempts";
    static readonly int ENQUEUE_ATTEMPTS_DEFAULT = 5;

    static readonly string ENQUEUE_ATTEMPT_SLEEP_KEY = "SQLiteQueue.Enqueue.AttemptSleep";
    static readonly TimeSpan ENQUEUE_ATTEMPT_SLEEP_DEFAULT = TimeSpan.FromMilliseconds (100);

    static readonly string SERIALIZER_KEY = "SQLiteQueue.Serializer";
    static readonly string SERIALIZER_MESSAGEPACK = "MessagePack";
    static readonly string SERIALIZER_DEFAULT = SERIALIZER_MESSAGEPACK; // MessagePack / BinaryFormatter

    readonly string MACHINE_ID_KEY = "MachineId";
    readonly string MACHINE_MODULE_ID_KEY = "MachineModuleId";
    static readonly string DIRECTORY_KEY = "Directory";
    static readonly string DATABASE_PREFIX_KEY = "Prefix";
    static readonly string DATABASE_PREFIX_DEFAULT = "CncDataFifoDb-"; // When the database name is determined by the machine
    static readonly string DATABASE_SUFFIX_KEY = "Suffix";
    static readonly string DATABASE_SUFFIX_DEFAULT = "";
    static readonly string TABLE_NAME_KEY = "SQLite.TableName";
    static readonly string TABLE_NAME_DEFAULT = "CncData"; // When the table name is not determined by the machine
    static readonly string DATETIME_FORMAT = "O";
    static readonly string NULL_TYPE_KEYWORD = "null";
    static readonly long MAX_FILESIZE_BEFORE_VACUUM = 1024 * 1024 * 10; // vacuum when file size reaches 10 Mo
    // TO CHANGE!
    static readonly TimeSpan MIN_DELAY_BETWEEN_VACUUM = TimeSpan.FromHours (1); // at least 1 hour between vacuums; BEWARE: not too short (1 minute = kills performance)
    static readonly TimeSpan MIN_DELAY_BETWEEN_DELETE = TimeSpan.FromSeconds (30); // at least 30 seconds between delete on enqueues

    static readonly int MAX_NB_ATTEMPT = 2;

    #region Members
    bool m_initialized = false;
    IGenericConfigReader m_configReader = null;
    MemoryConfigReader m_defaultConfigReader = new MemoryConfigReader ();
    SQLiteConnection m_connection;
    string m_fileName = null;
    string m_filePath;
    string m_tableName;
    SQLiteSynchroMode m_synchroMode = SQLiteSynchroMode.Normal;
    bool m_checkIntegrity = true;
    DateTime m_lastVacuum = DateTime.UtcNow;
    DateTime m_lastDelete = DateTime.UtcNow;
    IChecked m_checkedCaller = null;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (SQLiteCncDataQueue).FullName);

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int MachineId
    {
      get {
        try {
          var machineId = m_configReader.Get<int> (MACHINE_ID_KEY);
          if (0 == machineId) {
            log.Error ($"MachineId.get: returned machine id {machineId} is 0");
          }
          return machineId;
        }
        catch (Exception ex) {
          log.Error ($"MachineId.get: exception", ex);
          throw;
        }
      }
      set {
        if (value < 0) {
          log.ErrorFormat ("MachineId.set: " +
                           "negative value {0}, a positive value is expected",
                           value);
          throw new ArgumentOutOfRangeException ("value", "Not a positive value");
        }
        m_defaultConfigReader.Add (MACHINE_ID_KEY, value, true);
      }
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int MachineModuleId
    {
      get {
        try {
          var machineModuleId = m_configReader.Get<int> (MACHINE_MODULE_ID_KEY);
          if (0 == machineModuleId) {
            log.Error ($"MachineModuleId.get: returned machine module id {machineModuleId} is 0");
          }
          return machineModuleId;
        }
        catch (Exception ex) {
          log.Error ($"MachineModuleId.get: exception", ex);
          throw;
        }
      }
      set {
        if (value < 0) {
          log.ErrorFormat ("MachineModuleId.set: " +
                           "negative value {0}, a positive value is expected",
                           value);
          throw new ArgumentOutOfRangeException ("value", "Not a positive value");
        }
        m_defaultConfigReader.Add (MACHINE_MODULE_ID_KEY, value, true);
      }
    }

    /// <summary>
    /// <see cref="INamedCollection" />
    /// </summary>
    public string Name
    {
      get {
        Initialize ();

        return (null == m_fileName) ? null : "SQLiteQueue" + m_fileName;
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SQLiteCncDataQueue ()
    {
      m_defaultConfigReader.Add (DATABASE_PREFIX_KEY, DATABASE_PREFIX_DEFAULT);
      m_defaultConfigReader.Add (DATABASE_SUFFIX_KEY, DATABASE_SUFFIX_DEFAULT);
      m_defaultConfigReader.Add (TABLE_NAME_KEY, TABLE_NAME_DEFAULT);
      m_defaultConfigReader.Add ("SynchroMode", "Normal");
      m_defaultConfigReader.Add ("CheckIntegrity", false);

      string directory = Path.Combine (PulseInfo.LocalConfigurationDirectory, "Cnc");
      if (!Directory.Exists (directory)) {
        directory = PulseInfo.LocalConfigurationDirectory;
      }
      m_defaultConfigReader.Add (DIRECTORY_KEY, directory);
    }

    #region IConfigurable implementation
    /// <summary>
    /// <see cref="IConfigurable" />
    /// </summary>
    /// <param name="configReader"></param>
    public void SetConfigReader (IGenericConfigReader configReader)
    {
      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (configReader);
      multiConfigReader.Add (m_defaultConfigReader);
      m_configReader = multiConfigReader;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Initialize the queue based on configurations
    /// </summary>
    void Initialize ()
    {
      if (m_initialized) {
        return;
      }

      Debug.Assert (null != m_configReader);

      try {
        switch (m_configReader.Get<string> ("SynchroMode")) {
          case "Full":
            m_synchroMode = SQLiteSynchroMode.Full;
            break;
          case "Off":
            m_synchroMode = SQLiteSynchroMode.Off;
            break;
          case "Normal":
          default:
            m_synchroMode = SQLiteSynchroMode.Normal;
            break;
        }
      }
      catch (Exception ex) {
        log.Error ($"Initialize: SynchroMode key is not defined, use the default {m_synchroMode}", ex);
      }
      try {
        m_checkIntegrity = m_configReader.Get<bool> ("CheckIntegrity");
      }
      catch (Exception ex) {
        log.Error ($"Initialize: CheckIntegrity is not defined, use default {m_checkIntegrity}", ex);
      }

      // Directory
      string directory;
      try {
        directory = m_configReader.Get<string> (DIRECTORY_KEY);
      }
      catch (Exception ex) {
        log.Fatal ($"Initialize: directory key {DIRECTORY_KEY} is not defined", ex);
        throw;
      }

      // Logger name and filename
      string loggerName = typeof (SQLiteCncDataQueue).FullName +
        "." + MachineId +
        "." + MachineModuleId;
      m_fileName = null;
      try {
        m_fileName = m_configReader.Get<string> ("FileName");
      }
      // disable once EmptyGeneralCatchClause
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: FileName is not defined", ex);
        }
      }

      string prefix;
      try {
        prefix = m_configReader.Get<string> (DATABASE_PREFIX_KEY);
      }
      catch (Exception ex) {
        log.Fatal ($"Initialize: prefix key {DATABASE_PREFIX_KEY} is not defined", ex);
        throw;
      }

      string suffix;
      try {
        suffix = m_configReader.Get<string> (DATABASE_SUFFIX_KEY);
      }
      catch (Exception ex) {
        log.Fatal ($"Initialize: suffix key {DATABASE_SUFFIX_KEY} is not defined", ex);
        throw;
      }

      if (String.IsNullOrEmpty (m_fileName)) {
        m_fileName = prefix +
          MachineId +
          "-" + MachineModuleId;
      }
      else {
        loggerName += "." + m_fileName;
      }
      if (!string.IsNullOrEmpty (suffix)) {
        m_fileName += suffix;
        loggerName += suffix;
      }

      log = LogManager.GetLogger (loggerName);
      Initialize (directory, m_fileName, TABLE_NAME_DEFAULT);

      m_initialized = true;
    }

    void Initialize (string directory, string fileName, string tableName)
    {
      m_tableName = tableName;

      string filePath = Path.Combine (directory, fileName);
      var connBuilder = new SQLiteConnectionStringBuilder ();
      connBuilder.DataSource = filePath;
      connBuilder.JournalMode = SQLiteJournalModeEnum.Default; // for WAL mode, use SQLiteJournalModeEnum.Wal
      // connBuilder.Pooling = true; // problem with unit test TestMalformedDiskImage (corrupt file remains locked despite connection failed)

      SetActive ();

      switch (m_synchroMode) {
        case SQLiteSynchroMode.Full:
          connBuilder.SyncMode = SynchronizationModes.Full;
          break;
        case SQLiteSynchroMode.Normal:
          connBuilder.SyncMode = SynchronizationModes.Normal;
          break;
        case SQLiteSynchroMode.Off:
          connBuilder.SyncMode = SynchronizationModes.Off;
          break;
      }
      string connString = connBuilder.ToString ();

      SetActive ();
      m_connection = new SQLiteConnection (connBuilder.ToString ());
      OpenAndCreate (filePath);
    }

    /// <summary>
    /// Set a caller
    /// </summary>
    /// <param name="caller"></param>
    public void SetCheckedCaller (IChecked caller)
    {
      m_checkedCaller = caller;
    }

    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void SetActive ()
    {
      if (null != m_checkedCaller) {
        m_checkedCaller.SetActive ();
      }
    }

    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_checkedCaller) {
        m_checkedCaller.PauseCheck ();
      }
    }

    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_checkedCaller) {
        m_checkedCaller.ResumeCheck ();
      }
    }

    void OpenAndCreate (string filePath)
    {
      OpenAndCreate (filePath, 1);
    }

    void OpenAndCreate (string filePath, int nbAttempt)
    {
      SetActive ();

      if (MAX_NB_ATTEMPT < nbAttempt) {
        log.FatalFormat ("OpenAndCreate: " +
                         "the maximum number of connection attempts was reached {0} " +
                         "=> give up",
                         MAX_NB_ATTEMPT);
        throw new SQLiteException ("Database file is corrupted again");
      }

      try {
        m_connection.Open ();
        m_filePath = filePath;
      }
      catch (SQLiteException ex) {
        log.Error ($"OpenAndCreate: Connection to {filePath} failed with a SQLiteException", ex);
        if (SQLiteErrorCode.NoMem == ex.ResultCode) {
          log.Fatal ($"OpenAndCreate: out of memory code={ex.ResultCode} => exit", ex);
          Lemoine.Core.Environment.LogAndForceExit (ex, log);
        }
        else if (SQLiteErrorCode.Corrupt == ex.ResultCode) {
          // The database disk image is malformed: delete it
          log.Error ($"OpenAndCreate: the database disk image {filePath} is malformed => delete it");
          try {
            File.Delete (filePath + ".corrupted");
            File.Move (filePath, filePath + ".corrupted");
            log.Debug ($"OpenAndCreate: Malformed database {filePath} has been successfully deleted");
          }
          catch (Exception deleteException) {
            log.Error ($"OpenAndCreate: malformed database {filePath} could not be deleted, throw", deleteException);
            throw;
          }
          // Next attempt
          OpenAndCreate (filePath, nbAttempt + 1);
          return;
        }
        else {
          log.Error ($"OpenAndCreate: another SQLiteException", ex);
          throw;
        }
      }
      catch (Exception ex) {
        log.Error ($"OpenAndCreate: Connection to {filePath} failed", ex);
        throw;
      }

      SetActive ();

      if (m_checkIntegrity) {
        log.DebugFormat ("OpenAndCreate: " +
                        "Checking integrity of SQLite file {0}",
                        filePath);
        if (!InternalCheckIntegrity ()) {
          try {
            log.DebugFormat ("OpenAndCreate: " +
                            "Trying to delete a corrupt database file {0}",
                            filePath);
            m_connection.Close ();
            // despite connection close, current process seems to lock file,
            // thus preventing deletion.
            // Calling m_connection.Dispose() is not enough
            // but this GC hack seems to do the job.
            System.GC.Collect ();
            System.GC.WaitForPendingFinalizers ();
            File.Delete (filePath + ".corrupted");
            File.Move (filePath, filePath + ".corrupted");
            log.DebugFormat ("OpenAndCreate: " +
                            "Corrupt database file {0} has been successfully deleted",
                            filePath);
          }
          catch (Exception ex) {
            log.Error ($"OpenAndCreate: SQLite database file {filePath} is corrupt and could not be deleted: Abort", ex);
            throw;
          }

          // Next attempt
          OpenAndCreate (filePath, nbAttempt + 1);
          return;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"OpenAndCreate: CheckIntegrity success on {filePath}");
          }
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.DebugFormat ($"OpenAndCreate: NOT checking integrity of SQLite file {filePath}");
        }
      }

      SetActive ();

      // Check the table exists
      // If not create it
      try {
        {
          DbCommand command = m_connection.CreateCommand ();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = string.Format (@"
CREATE TABLE IF NOT EXISTS {0}
(Id INTEGER PRIMARY KEY AUTOINCREMENT,
 MachineId INTEGER NOT NULL,
 MachineModuleId INTEGER,
 DateTime TEXT NOT NULL,
 Command TEXT NOT NULL,
 Key TEXT NOT NULL,
 ValueType TEXT NOT NULL,
 ValueString TEXT,
 ValueInt INTEGER,
 ValueDouble REAL,
 ValueBinary BLOB)",
                                               m_tableName);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          command.ExecuteNonQuery ();
        }
        SetActive ();
        {
          DbCommand command = m_connection.CreateCommand ();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = string.Format (@"
CREATE TABLE IF NOT EXISTS {0}FirstId
(FirstId INTEGER NOT NULL)",
                                               m_tableName);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          command.ExecuteNonQuery ();
        }
        SetActive ();
        {
          DbCommand command = m_connection.CreateCommand ();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = string.Format (@"
INSERT INTO {0}FirstId (FirstId)
SELECT 0
WHERE NOT EXISTS (SELECT 1 FROM {0}FirstId)",
                                               m_tableName);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          command.ExecuteNonQuery ();
        }
      }
      catch (Exception ex) {
        log.Error ($"Initialize: table creation {m_tableName} failed", ex);
        throw;
      }

      SetActive ();
    }

    /// <summary>
    /// IDisposable implementation
    /// </summary>
    public void Dispose ()
    {
      if (null != m_connection) {
        m_connection.Close ();
      }

      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// 
    /// This implementation is very very slow. Be careful if you intend to use it
    /// </summary>
    public int Count
    {
      get {
        Initialize ();

        DbDataReader reader = null;

        try {
          using (DbCommand command = m_connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            command.CommandText = string.Format (@"
SELECT COUNT (*)
FROM {0}
WHERE Id>=(SELECT FirstId FROM {0}FirstId)",
                                                 m_tableName);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

            try {
              reader = command.ExecuteReader ();
            }
            catch (Exception ex) {
              log.Error ($"Count.get: SELECT FROM {m_tableName} WHERE... failed", ex);
              throw;
            }
          }

          SetActive ();

          if (false == reader.Read ()) {
            log.Error ($"Count.get: SELECT COUNT failed on table {m_tableName}");
            throw new Exception ("SELECT COUNT failed");
          }

          int result = Convert.ToInt32 (reader[0]);
          Debug.Assert (false == reader.Read ()); // Only one row
          return result;
        }
        finally {
          if (null != reader) {
            reader.Close ();
            reader.Dispose ();
          }
        }
      }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    public void Clear ()
    {
      Initialize ();

      using (DbCommand command = m_connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        command.CommandText = string.Format (@"
UPDATE {0}FirstId
SET FirstId= COALESCE ((SELECT MAX(Id) from {0})+1, 0)", m_tableName);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

        try {
          command.ExecuteNonQuery ();
        }
        catch (Exception ex) {
          log.Error ($"Clear: UPDATE {m_tableName}FirstId... failed", ex);
          throw;
        }
      }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public ExchangeData Dequeue ()
    {
      Initialize ();

      using (DbTransaction transaction = m_connection.BeginTransaction ()) {
        ExchangeData data = Peek ();

        using (DbCommand command = m_connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = string.Format (@"
UPDATE {0}FirstId
SET FirstId=(SELECT MIN(Id)
             FROM {0}
             WHERE Id>=(SELECT FirstId FROM {0}FirstId))+1",
                                               m_tableName);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          try {
            command.ExecuteNonQuery ();
          }
          catch (Exception ex) {
            log.Error ($"Peek: DELETE FROM {m_tableName} failed", ex);
            transaction.Rollback ();
            throw;
          }
        }

        transaction.Commit ();

        return data;
      }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public void UnsafeDequeue ()
    {
      UnsafeDequeue (1);
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public void UnsafeDequeue (int n)
    {
      Initialize ();

      DateTime refTime = DateTime.UtcNow;

      using (DbTransaction transaction = m_connection.BeginTransaction ())
      using (DbCommand command = m_connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        command.CommandText = string.Format (@"
UPDATE {0}FirstId
SET FirstId=(SELECT MIN(Id)
             FROM {0}
             WHERE Id>=(SELECT FirstId FROM {0}FirstId))+{1}",
                                             m_tableName,
                                             n);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

        try {
          command.ExecuteNonQuery ();
        }
        catch (Exception ex) {
          log.Error ($"UnsafeDequeue: UPDATE {m_tableName}FirstId failed", ex);
          transaction.Rollback ();
          throw;
        }

        transaction.Commit ();

        return;
      }
    }

    /// <summary>
    /// Escape the single quotes for SQLite
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    string Escape (string s)
    {
      return s.Replace ("'", "''");
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <param name="data"></param>
    public void Enqueue (ExchangeData data)
    {
      var maxAttempts = Lemoine.Info.ConfigSet.LoadAndGet (ENQUEUE_ATTEMPTS_KEY, ENQUEUE_ATTEMPTS_DEFAULT);

      for (int i = 1; i <= maxAttempts; ++i) {
        try {
          TryEnqueue (data, i);
          return;
        }
        catch (SQLiteException ex) {
          if (i <= maxAttempts) {
            log.Error ($"Enqueue: maxAttempts {maxAttempts} reached, throw", ex);
            throw;
          }
          if (ex.ResultCode == SQLiteErrorCode.Locked) {
            var sleepTime = Lemoine.Info.ConfigSet.LoadAndGet (ENQUEUE_ATTEMPT_SLEEP_KEY, ENQUEUE_ATTEMPT_SLEEP_DEFAULT);
            if (log.IsWarnEnabled) {
              log.Warn ($"Enqueue: SQLite exception Database is locked: try once again in {sleepTime}", ex);
            }
            SetActive ();
            this.Sleep (sleepTime);
            SetActive ();
            continue;
          }
          else {
            log.Error ($"Enqueue: another SQLite exception", ex);
            throw;
          }
        }
        catch (Exception ex) {
          log.Error ($"Enqueue: not a SQLiteException, throw", ex);
          throw;
        }
      }

      log.Fatal ($"Enqueue: unexpected program call");
      throw new InvalidOperationException ("Unexpected program call");
    }

    void TryEnqueue (ExchangeData data, int attempt)
    {
      Initialize ();

      DateTime refTime = DateTime.UtcNow;

      if (refTime.Subtract (m_lastDelete) > MIN_DELAY_BETWEEN_DELETE) {
        using (DbCommand command = m_connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = string.Format (@"
DELETE FROM {0}
WHERE (SELECT FirstId FROM {0}FirstId) >= (SELECT COALESCE (MIN(Id), 0) FROM {0}) + 200
  AND Id<(SELECT FirstId FROM {0}FirstId)",
                                               m_tableName);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          SetActive ();
          try {
            log.Debug ("TryEnqueue: about to clean the data");
            command.ExecuteNonQuery ();
            m_lastDelete = refTime;
            log.Debug ("TryEnqueue: data cleaned");
          }
          catch (Exception ex) {
            log.Warn ($"TryEnqueue: DELETE FROM {m_tableName} ... failed, but try to continue", ex);
          }
        }
      }

      SetActive ();

      using (DbCommand command = m_connection.CreateCommand ()) {
        try {
          {
            string dateTimeString = data.DateTime.ToString (DATETIME_FORMAT);
            if (null == data.Value) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
              command.CommandText = string.Format (@"
INSERT INTO {0} (MachineId, MachineModuleId, DateTime, Command, Key, ValueType)
VALUES ({1}, {2}, '{3}', '{4}', '{5}', '{6}')",
                                                   m_tableName,
                                                   data.MachineId,
                                                   data.MachineModuleId,
                                                   dateTimeString,
                                                   data.Command.ToString (),
                                                   data.Key,
                                                   NULL_TYPE_KEYWORD);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            }
            else { // null != data.Value
              Type valueType = data.Value.GetType ();
              if (valueType.Equals (typeof (int))
                  || valueType.Equals (typeof (Int64))
                  || valueType.Equals (typeof (Int16))) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = string.Format (@"
INSERT INTO {0} (MachineId, MachineModuleId, DateTime, Command, Key, ValueType, ValueInt)
VALUES ({1}, {2}, '{3}', '{4}', '{5}', '{6}', {7})",
                                                     m_tableName,
                                                     data.MachineId,
                                                     data.MachineModuleId,
                                                     dateTimeString,
                                                     data.Command.ToString (),
                                                     data.Key,
                                                     valueType.ToString (),
                                                     data.Value);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
              }
              else if (valueType.Equals (typeof (bool))) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = string.Format (@"
INSERT INTO {0} (MachineId, MachineModuleId, DateTime, Command, Key, ValueType, ValueInt)
VALUES ({1}, {2}, '{3}', '{4}', '{5}', '{6}', {7})",
                                                     m_tableName,
                                                     data.MachineId,
                                                     data.MachineModuleId,
                                                     dateTimeString,
                                                     data.Command.ToString (),
                                                     data.Key,
                                                     valueType.ToString (),
                                                     ((bool)data.Value ? 1 : 0));
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
              }
              else if (valueType.Equals (typeof (double))) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = string.Format (@"
INSERT INTO {0} (MachineId, MachineModuleId, DateTime, Command, Key, ValueType, ValueDouble)
VALUES ({1}, {2}, '{3}', '{4}', '{5}', '{6}', {7})",
                                                     m_tableName,
                                                     data.MachineId,
                                                     data.MachineModuleId,
                                                     dateTimeString,
                                                     data.Command.ToString (),
                                                     data.Key,
                                                     valueType.ToString (),
                                                     ((double)data.Value).ToString (CultureInfo.InvariantCulture));
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
              }
              else if (valueType.Equals (typeof (string))) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = string.Format (@"
INSERT INTO {0} (MachineId, MachineModuleId, DateTime, Command, Key, ValueType, ValueString)
VALUES ({1}, {2}, '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                     m_tableName,
                                                     data.MachineId,
                                                     data.MachineModuleId,
                                                     dateTimeString,
                                                     data.Command.ToString (),
                                                     data.Key,
                                                     valueType.ToString (),
                                                     Escape (data.Value.ToString ()));
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
              }
              else {
                var typeElements = valueType.AssemblyQualifiedName.Split ([", "], StringSplitOptions.None);
                string qualifiedType;
                if (typeElements[1].Equals ("mscorlib")) {
                  qualifiedType = typeElements[0];
                }
                else {
                  qualifiedType = $"{typeElements[0]}, {typeElements[1]}";
                }
                try {
                  // Try Json
                  JsonSerializerOptions jsonSerializerOptions = new (JsonSerializerOptions.Default) {
                    WriteIndented = false
                  };
                  var json = JsonSerializer.Serialize (data.Value, jsonSerializerOptions);
                  if (log.IsDebugEnabled) {
                    log.Debug ($"TryEnqueue: json is {json}");
                  }
                  command.CommandText = $"""
                  INSERT INTO {m_tableName} (MachineId, MachineModuleId, DateTime, Command, Key, ValueType, ValueString)
                  VALUES ({data.MachineId}, {data.MachineModuleId}, '{dateTimeString}', '{data.Command}', '{data.Key}', '{qualifiedType}', '{json}') 
                  """;
                }
                catch (Exception ex) {
                  log.Error ($"TryEnqueue: could not serialize {data.Value} in Json => try binary instead", ex);
                  string hex;
                  using (MemoryStream stream = new MemoryStream ()) {
                    var serializer = Lemoine.Info.ConfigSet.LoadAndGet (SERIALIZER_KEY, SERIALIZER_DEFAULT);
                    if (serializer.Equals (SERIALIZER_MESSAGEPACK)) {
                      MessagePackSerializer.Serialize (stream, data.Value, ContractlessStandardResolver.Options);
                    }
                    else {
                      IFormatter formatter = new BinaryFormatter ();
                      formatter.Serialize (stream, data.Value);
                    }
                    hex = BitConverter.ToString (stream.ToArray ());
                  }
                  if (log.IsDebugEnabled) {
                    log.Debug ($"TryEnqueue: binary serialization instead");
                  }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                  command.CommandText = string.Format (@"
INSERT INTO {0} (MachineId, MachineModuleId, DateTime, Command, Key, ValueType, ValueBinary)
VALUES ({1}, {2}, '{3}', '{4}', '{5}', '{6}', x'{7:x2}')",
                                                     m_tableName,
                                                     data.MachineId,
                                                     data.MachineModuleId,
                                                     dateTimeString,
                                                     data.Command.ToString (),
                                                     data.Key,
                                                     qualifiedType,
                                                     hex.Replace ("-", ""));
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                }
              }
            }
            command.ExecuteNonQuery ();
          }
        }
        catch (Exception ex) {
          log.Error ($"TryEnqueue: INSERT INTO {m_tableName} {data} failed", ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Vacuums db if needed
    /// </summary>
    /// <returns></returns>
    public bool VacuumIfNeeded ()
    {
      Initialize ();

      long initDbSize = GetDbSize ();
      SetActive ();
      if ((initDbSize > MAX_FILESIZE_BEFORE_VACUUM) &&
          (DateTime.UtcNow.Subtract (m_lastVacuum) > MIN_DELAY_BETWEEN_VACUUM)) {
        m_lastVacuum = DateTime.UtcNow;
        log.DebugFormat ("Start Vacuum of {0} (size {1})", m_filePath, initDbSize);
        Vacuum ();
        log.DebugFormat ("End of Vacuum of {0} (size {1})", m_filePath, GetDbSize ());
        return true;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Vacuum queue/db
    /// </summary>
    void Vacuum ()
    {
      using (DbCommand command = m_connection.CreateCommand ()) {
        command.CommandText = "Vacuum";
        try {
          command.ExecuteNonQuery ();
        }
        catch (Exception ex) {
          log.Error ("Vacuum failed", ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Return size of file/db
    /// </summary>
    /// <returns></returns>
    long GetDbSize ()
    {
      if (m_filePath == null) {
        return 0;
      }
      FileInfo finfo = new FileInfo (m_filePath);
      return finfo.Length;
    }

    /// <summary>
    /// Check integrity of the database file
    /// </summary>
    /// <returns></returns>
    public bool CheckIntegrity ()
    {
      Initialize ();

      return InternalCheckIntegrity ();
    }

    bool InternalCheckIntegrity ()
    {
      DbCommand command = m_connection.CreateCommand ();
      command.CommandText = "PRAGMA integrity_check(1)";
      try {
        Object obj = command.ExecuteScalar ();
        return ((obj.GetType () == typeof (string)) && (((string)obj) == "ok"));
      }
      catch (Exception ex) {
        log.Error ("CheckIntegrity: failed with exception", ex);
        return false;
      }
      finally {
        command.Dispose ();
      }
    }


    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public ExchangeData Peek ()
    {
      Initialize ();

      IList<ExchangeData> datas = Peek (1);
      if (0 == datas.Count) {
        log.Info ("Peek: the queue is empty => throw InvalidOperationException");
        throw new InvalidOperationException ();
      }
      else {
        return datas[0];
      }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public IList<ExchangeData> Peek (int nbElements)
    {
      Initialize ();

      log.Debug ("Peek /B");

      IList<ExchangeData> datas = new List<ExchangeData> ();
      DbDataReader reader = null;

      try {
        using (DbCommand command = m_connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = string.Format (@"
SELECT MachineId, MachineModuleId, DateTime, Command, Key, ValueType, ValueString, ValueInt, ValueDouble, ValueBinary
FROM {0}
WHERE Id>=(SELECT FirstId FROM {0}FirstId)
ORDER BY Id Asc
LIMIT {1}",
                                               m_tableName,
                                               nbElements);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

          try {
            reader = command.ExecuteReader ();
          }
          catch (Exception ex) {
            log.Error ($"Peek: SELECT FROM {m_tableName} failed", ex);
            throw;
          }
        }

        while (reader.Read ()) {
          SetActive ();
          try {
            int machineId = Convert.ToInt32 (reader[0]); // reader [0] is an Int64
            int machineModuleId = (null != reader[1]) ? Convert.ToInt32 (reader[1]) : 0;
            DateTime dateTime = DateTime.Parse ((string)reader[2], null, DateTimeStyles.RoundtripKind);
            ExchangeDataCommand dataCommand = (ExchangeDataCommand)Enum.Parse (typeof (ExchangeDataCommand), (string)reader[3]);
            string key = (string)reader[4];
            string valueType = (string)reader[5];
            object val = null;

            if (valueType.Equals (NULL_TYPE_KEYWORD)) {
              val = null;
            }
            else if (valueType.Equals (typeof (int).ToString ())) {
              val = Convert.ToInt32 (reader[7]);
            }
            else if (valueType.Equals (typeof (Int16).ToString ())) {
              val = Convert.ToInt16 (reader[7]);
            }
            else if (valueType.Equals (typeof (Int64).ToString ())) {
              val = Convert.ToInt64 (reader[7]);
            }
            else if (valueType.Equals (typeof (bool).ToString ())) {
              val = (0 != Convert.ToInt32 (reader[7]));
            }
            else if (valueType.Equals (typeof (double).ToString ())) {
              val = (double)reader[8];
            }
            else if (valueType.Equals (typeof (string).ToString ())) {
              val = (string)reader[6];
            }
            else {
              var json = reader[6]?.ToString ();
              if (!string.IsNullOrEmpty (json)) { // Json
                var t = Type.GetType (valueType);
                if (t is null && log.IsErrorEnabled) {
                  log.Error ($"Peek: type {valueType} is unknown");
                }
                val = JsonSerializer.Deserialize (json, t);
              }
              else {
                try {
                  using (MemoryStream stream = new MemoryStream ((byte[])reader[9])) {
                    val = MessagePackSerializer.Deserialize (Type.GetType (valueType), stream, ContractlessStandardResolver.Options);
                  }
                }
                catch (Exception ex) {
                  if (log.IsInfoEnabled) {
                    log.Info ($"Peek: MessagePack deserialize failed with exception => switch to BinaryFormatter", ex);
                  }
                  BinaryFormatter formatter = new BinaryFormatter ();
                  using (MemoryStream stream = new MemoryStream ((byte[])reader[9])) {
                    val = formatter.Deserialize (stream);
                  }
                }
              }
            }

            ExchangeData data = ExchangeData.BuildExchangeData (machineId, machineModuleId,
                                                               dateTime, dataCommand, key, val);

            datas.Add (data);
          }
          catch (Exception ex) {
            log.Error ("Peek: error while getting the data ", ex);
            throw;
          }
        }
      }
      finally { // Close the reader ! Else a lock may be hold a longer time than necessary
        if (null != reader) {
          reader.Close ();
          reader.Dispose ();
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Peek: return {datas.Count} elements from the {nbElements} asked elements");
      }
      return datas;
    }

    /// <summary>
    /// Close the queue
    /// </summary>
    public void Close ()
    {
      if (null != m_connection) {
        m_connection.Close ();
        GC.Collect ();   // yes, really release the db
      }
    }

    /// <summary>
    /// Delete the queue
    /// </summary>
    public void Delete ()
    {
      Initialize ();

      // http://stackoverflow.com/questions/8511901/system-data-sqlite-close-not-releasing-database-file
      if (File.Exists (m_filePath)) {
        Close ();

        for (var i = 1; ; ++i) {
          try {
            this.Sleep (TimeSpan.FromMilliseconds (i * 100));
            if (File.Exists (m_filePath)) {
              File.Delete (m_filePath);
            }
            return;
          }
          catch (IOException ex) {  // delete only throws this on locking
            if (log.IsInfoEnabled) {
              log.Info ($"Delete: attempt {i} to close file {m_filePath} failed");
            }
            if (3 < i) {
              log.Error ($"Delete: last attempt to close file {m_filePath}, throw", ex);
              throw;
            }
          }
        }

        throw new IOException ("Unable to close file " + m_filePath);
      }
    }
    #endregion // Methods
  }
}

