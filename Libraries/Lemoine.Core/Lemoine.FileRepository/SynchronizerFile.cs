// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Lemoine.Info;
using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Class to record the time of the synchronization
  /// </summary>
  internal class SynchronizerFile : IDisposable
  {
    static readonly string ADD_PROGRAM_NAME_TO_SYNCHRONIZATION_PATH_KEY = "FileRepository.Synchronization.AddProgramName";
    static readonly bool ADD_PROGRAM_NAME_TO_SYNCHRONIZATION_PATH_DEFAULT = false;

    static readonly string FILE_ACCESS_ATTEMPT_NB_KEY = "FileRepository.Synchronization.FileAccessAttemptNumber";
    static readonly int FILE_ACCESS_ATTEMPT_NB_DEFAULT = 20;

    static readonly string FILE_ACCESS_ATTEMPT_SLEEP_KEY = "FileRepository.Syncronization.FileAccessSleep";
    static readonly TimeSpan FILE_ACCESS_ATTEMPT_SLEEP_DEFAULT = TimeSpan.FromMilliseconds (100);

    #region Members
    static readonly string SYNCHRONIZATION_FILE_NAME = "synchronization.txt";
    static readonly string DATETIME_FORMAT = @"yyyy'/'MM'/'dd HH':'mm':'ss.fff";
    static readonly char SEPARATOR = '>';
    readonly string m_path = "";
    readonly IDictionary<string, DateTime> m_dates = new Dictionary<string, DateTime> ();
    readonly IDictionary<string, DateTime?> m_updatedDates = new Dictionary<string, DateTime?> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SynchronizerFile).FullName);

    #region Constructors
    /// <summary>
    /// Constructor with a default path
    /// </summary>
    public SynchronizerFile ()
    {
      try {
        m_path = GetSynchronizerFilePath ();
      }
      catch (Exception ex) {
        log.Error ("SynchronizerFile: exception in GetSynchronizerFilePath", ex);
        m_path = "";
        return;
      }

      if (File.Exists (m_path)) {
        try {
          Read ();
        }
        catch (Exception ex) {
          log.Error ($"SynchronizerFile: error reading {m_path}", ex);
          try {
            File.Delete (m_path);
          }
          catch (Exception ex1) {
            log.Error ($"SynchronizerFile: delete of {m_path} failed => reset m_path", ex1);
            m_path = "";
          }
        }
      }
    }
    #endregion // Constructors

    #region Public static methods
    static string GetSynchronizerFilePath ()
    {
      var directoryPath = PulseInfo.LocalConfigurationDirectory;
      if (Lemoine.Info.ConfigSet.LoadAndGet (ADD_PROGRAM_NAME_TO_SYNCHRONIZATION_PATH_KEY, ADD_PROGRAM_NAME_TO_SYNCHRONIZATION_PATH_DEFAULT)) {
        var programName = Lemoine.Info.ProgramInfo.Name;
        if (!string.IsNullOrEmpty (programName)) {
          directoryPath = Path.Combine (directoryPath, programName);
        }
      }
      if (!Directory.Exists (directoryPath)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetSynchronizedFilePath: create directory {directoryPath}");
        }
        try {
          Directory.CreateDirectory (directoryPath);
        }
        catch (Exception ex) {
          if (log.IsErrorEnabled) {
            log.Error ($"GetSynchronizerFilePath: CreateDirectory {directoryPath} failed", ex);
          }
          throw;
        }
      }
      return Path.Combine (directoryPath, SYNCHRONIZATION_FILE_NAME);
    }
    #endregion // Public static methods

    #region Other methods
    /// <summary>
    /// Get the date of a file
    /// Can be null if the date is not known yet
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal DateTime? GetDate (string path)
    {
      if (m_dates.ContainsKey (path)) {
        return m_dates[path];
      }
      return null;
    }

    /// <summary>
    /// Set the date of a file
    /// A null date will remove the information
    /// </summary>
    /// <param name="path"></param>
    /// <param name="date"></param>
    internal void SetDate (string path, DateTime? date)
    {
      var currentDate = GetDate (path);
      if (object.Equals (currentDate, date)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("SetDatePrivate: no change for {0}>{1} => return", path, date);
        }
        return;
      }
      m_updatedDates[path] = date;
    }

    void Read ()
    {
      var fileAccessAttemptNumber = Lemoine.Info.ConfigSet
        .LoadAndGet (FILE_ACCESS_ATTEMPT_NB_KEY, FILE_ACCESS_ATTEMPT_NB_DEFAULT);
      var fileAccessSleep = Lemoine.Info.ConfigSet
        .LoadAndGet (FILE_ACCESS_ATTEMPT_SLEEP_KEY, FILE_ACCESS_ATTEMPT_SLEEP_DEFAULT);
      IOException lastIoException = null;
      for (int i = 1; i <= fileAccessAttemptNumber; ++i) {
        lastIoException = null;
        try {
          EffectiveRead ();
          return;
        }
        catch (IOException ex) {
          lastIoException = ex;
          if (i == fileAccessAttemptNumber) {
            if (log.IsErrorEnabled) {
              log.Error ($"Read: IO exception for path {m_path} attempt {i} => give up", ex);
            }
            return;
          }
          if (log.IsWarnEnabled) {
            log.Warn ($"Read: IO exception for path {m_path} attempt {i} => sleep {fileAccessSleep}", ex);
          }
          System.Threading.Thread.Sleep (fileAccessSleep);
        }
      }
      if (log.IsFatalEnabled) {
        log.Fatal ("Read: Unexpected code reached after the loop");
      }
      if (null != lastIoException) {
        throw new InvalidOperationException ("Unexpected code reached", lastIoException);
      }
      else {
        throw new InvalidOperationException ("Unexpected code reached");
      }
    }

    void EffectiveRead ()
    {
      if (string.IsNullOrEmpty (m_path)) {
        log.Warn ($"EffectiveRead: path is empty or null (because of an error in the constructors?)");
      }
      else {
        using (var sr = new StreamReader (m_path)) {
          Read (sr);
        }
      }
    }

    void Read (TextReader sr)
    {
      string line;
      while ((line = sr.ReadLine ()) != null) {
        if (string.IsNullOrWhiteSpace (line)) {
          continue;
        }
        var parts = line.Split (SEPARATOR);
        if (parts.Length != 2) {
          log.Error ($"Read: invalid line {line}, skip it");
          continue;
        }
        try {
          m_dates[parts[0]] = DateTime.ParseExact (
            parts[1], DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex) {
          if (log.IsErrorEnabled) {
            log.Error ($"Read: exception in line {line}", ex);
          }
        }
      }
    }

    void Write ()
    {
      if (!m_updatedDates.Any ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("Write: nothing to write => return");
        }
        return;
      }

      var fileAccessAttemptNumber = Lemoine.Info.ConfigSet
        .LoadAndGet (FILE_ACCESS_ATTEMPT_NB_KEY, FILE_ACCESS_ATTEMPT_NB_DEFAULT);
      var fileAccessSleep = Lemoine.Info.ConfigSet
        .LoadAndGet (FILE_ACCESS_ATTEMPT_SLEEP_KEY, FILE_ACCESS_ATTEMPT_SLEEP_DEFAULT);
      IOException lastIoException = null;
      for (int i = 1; i <= fileAccessAttemptNumber; ++i) {
        lastIoException = null;
        try {
          EffectiveWrite ();
          m_updatedDates.Clear ();
          return;
        }
        catch (IOException ex) {
          lastIoException = ex;
          if (i == fileAccessAttemptNumber) {
            if (log.IsErrorEnabled) {
              log.Error ($"Write: IO exception for path {m_path} attempt {i} => give up", ex);
            }
            return;
          }
          if (log.IsWarnEnabled) {
            log.Warn ($"Write: IO exception for path {m_path} attempt {i} => sleep {fileAccessSleep}", ex);
          }
          System.Threading.Thread.Sleep (fileAccessSleep);
        }
      }
      if (log.IsFatalEnabled) {
        log.Fatal ("Write: Unexpected code reached after the loop");
      }
      if (null != lastIoException) {
        throw new InvalidOperationException ("Unexpected code reached", lastIoException);
      }
      else {
        throw new InvalidOperationException ("Unexpected code reached");
      }
    }

    void EffectiveWrite ()
    {
      if (string.IsNullOrEmpty (m_path)) {
        log.Warn ("EffectiveWrite: m_path is null or empty (because of an error in the constructor?)");
      }
      else {
        using (var fs = File.Open (m_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
          using (var sr = new StreamReader (fs)) {
            Read (sr);
            fs.Seek (0, SeekOrigin.Begin);
            using (var sw = new StreamWriter (fs)) {
              foreach (var item in m_dates.Where (x => !m_updatedDates.ContainsKey (x.Key))) {
                sw.WriteLine (item.Key + SEPARATOR + item.Value.ToString (DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture));
              }
              foreach (var item in m_updatedDates.Where (x => x.Value.HasValue)) {
                sw.WriteLine (item.Key + SEPARATOR + item.Value.Value.ToString (DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture));
              }
            }
          }
        }
      }
    }

    public void Dispose ()
    {
      try {
        Write ();
      }
      catch (Exception ex) {
        log.Fatal ("Dispose: write failed", ex);
      }
    }
    #endregion // Other methods
  }
}
