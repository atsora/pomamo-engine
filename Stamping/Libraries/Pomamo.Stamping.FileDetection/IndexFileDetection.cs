// Copyright (C) 2009-2023 Lemoine Automation Technologies 2023 Nicolas Relange
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.TargetSpecific.FileRepository;
using System.Collections.Generic;
using Lemoine.Collections;

namespace Pomamo.Stamping.FileDetection
{
  /// <summary>
  /// Class to manage the folder containing index files
  /// </summary>
  public class IndexFileDetection : ThreadClass, IThreadClass
  {
    /// <summary>
    /// Config set to be used when a list of directories is targeted
    /// </summary>
    static readonly string INDEX_FILES_DIRECTORIES_KEY = "StampFileWatch.IndexFilesDirectories";
    static readonly string INDEX_FILES_DIRECTORIES_DEFAULT = "";

    static readonly string INDEX_FILES_DIRECTORY_KEY = "StampFileWatch.IndexFilesDirectory";
    static readonly string INDEX_FILES_DIRECTORY_DEFAULT = "C:\\Users\\Public\\AppData\\Lemoine\\Pulse";

    static readonly string SERVICE_USE_CURRENT_USER_KEY = "StampFileWatch.ServiceUseCurrentUser";
    static readonly bool SERVICE_USE_CURRENT_USER_DEFAULT = false;

    static readonly string MAX_DELAY_FOR_ISO_FILE_CREATION_KEY = "StampFileWatch.MaxDelayForFileCreation";
    static readonly int MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT = 5;  // seconds

    static readonly string DELAY_BEFORE_STAMPING_KEY = "StampFileWatch.DelayBeforeStamping";
    static readonly int DELAY_BEFORE_STAMPING_DEFAULT = 0;  // seconds    

    readonly IEnumerable<string> m_indexFilesDirectories;
    readonly bool m_useCurrentUser = false;
    readonly int m_daxDelayForISOFileCreation;
    readonly int m_delayBeforeStamping;
    DateTime m_lastReadTime = DateTime.MinValue;

    static readonly ILog log = LogManager.GetLogger (typeof (IndexFileDetection).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public IndexFileDetection ()
    {
      var indexFilesDirectoriesConfig = Lemoine.Info.ConfigSet
        .LoadAndGet<string> (INDEX_FILES_DIRECTORIES_KEY, INDEX_FILES_DIRECTORIES_DEFAULT);
      if (!string.IsNullOrWhiteSpace (indexFilesDirectoriesConfig)) {
        m_indexFilesDirectories = EnumerableString.ParseListString (indexFilesDirectoriesConfig);
      }
      else {
        var indexFilesDirectory = Lemoine.Info.ConfigSet.LoadAndGet<string> (INDEX_FILES_DIRECTORY_KEY, INDEX_FILES_DIRECTORY_DEFAULT);
        m_indexFilesDirectories = new string[] { indexFilesDirectory };
      }
      m_useCurrentUser = Lemoine.Info.ConfigSet.LoadAndGet<bool> (SERVICE_USE_CURRENT_USER_KEY, SERVICE_USE_CURRENT_USER_DEFAULT);
      m_daxDelayForISOFileCreation = Lemoine.Info.ConfigSet.LoadAndGet<int> (MAX_DELAY_FOR_ISO_FILE_CREATION_KEY, MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT);
      m_delayBeforeStamping = Lemoine.Info.ConfigSet.LoadAndGet<int> (DELAY_BEFORE_STAMPING_KEY, DELAY_BEFORE_STAMPING_DEFAULT);
    }

    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// Run the directory monitoring
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      foreach (var indexFilesDirectory in m_indexFilesDirectories) {
        log.Info ($"Run: initializing {indexFilesDirectory}");
        // check directory exists
        if (!Directory.Exists (indexFilesDirectory)) {
          log.Info ($"Run: Directory {indexFilesDirectory} does not exists, try to create it");
          try {
            log.Info ($"Run: create folder {indexFilesDirectory}");
            Directory.CreateDirectory (indexFilesDirectory);
            log.Info ($"Run: Directory {indexFilesDirectory} created");
          }
          catch (Exception ex) {
            log.Fatal ($"Run: unable to create folder {indexFilesDirectory}", ex);
            log.Fatal ($"Run: Directory {indexFilesDirectory} does not exists, exiting");
            this.SetExitRequested ();
            return;
          }
        }

        // create directory watcher
        try {
          var directoryWatcher = new FileSystemWatcher (indexFilesDirectory);

          directoryWatcher.Changed += OnChanged;
          directoryWatcher.Created += OnCreated;
          directoryWatcher.Renamed += OnRenamed;
          directoryWatcher.Deleted += OnDeleted;

          // start monitoring directory
          directoryWatcher.EnableRaisingEvents = true;
        }
        catch (Exception ex) {
          log.Error ("Run: exception", ex);
          this.SetExitRequested ();
          return;
        }
      }

      try {
        // to keep thread alive
        while (!cancellationToken.IsCancellationRequested && !this.ExitRequested) {
          SetActive ();
          this.Sleep (TimeSpan.FromSeconds (10), cancellationToken, () => this.ExitRequested);
        }
      }
      catch (Exception ex) {
        log.Error ("Run: exception", ex);
        this.SetExitRequested ();
        return;
      }
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    void OnChanged (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      if (m_useCurrentUser) {
        using (ImpersonationUtils.ImpersonateCurrentUser ()) {
          ProcessOnChanged (sender, fileSystemEventArgs);
        }
      }
      else {
        ProcessOnChanged (sender, fileSystemEventArgs);
        ;
      }
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    void OnCreated (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"OnCreated {fileSystemEventArgs.FullPath}");
      if (m_useCurrentUser) {
        using (ImpersonationUtils.ImpersonateCurrentUser ()) {
          ProcessOnChanged (sender, fileSystemEventArgs);
        }
      }
      else {
        ProcessOnChanged (sender, fileSystemEventArgs);
      }
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    void OnDeleted (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"OnDeleted {fileSystemEventArgs.FullPath}");
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    void OnError (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"OnError {fileSystemEventArgs.FullPath}");
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    void OnRenamed (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"OnRenamed {fileSystemEventArgs.FullPath}");
    }



    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    void ProcessOnChanged (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      // wait to ensure file is created properly after OnCreated event.
      Thread.Sleep (500);
      var watcherChangeType = fileSystemEventArgs.ChangeType;
      string fileChangedFullPath = fileSystemEventArgs.FullPath;
      // ignore duplicate event and event where file no longer exists
      try {
        if (File.Exists (fileChangedFullPath)) {
          DateTime lastWriteTime = File.GetLastWriteTime (fileChangedFullPath);
          log.Info ($"ProcessOnChanged: info: File={fileChangedFullPath}, {watcherChangeType}, {lastWriteTime}");
          if (lastWriteTime != m_lastReadTime) {
            m_lastReadTime = lastWriteTime;
            var isoFilePath = GetIsoFilePathFromIndexFile (fileChangedFullPath);

            // test file exists. ISO file may be generated AFTER index file. Wait for it.
            bool isoFileExists = false;
            int fileExistsCheckRetries = 0;
            while (!isoFileExists && fileExistsCheckRetries <= m_daxDelayForISOFileCreation) {
              isoFileExists = File.Exists (isoFilePath);
              if (!isoFileExists) {
                log.Info ($"ProcessOnChanged: info: wait for file {isoFilePath}");
                Thread.Sleep (1000);
                fileExistsCheckRetries++;
              }
            }

            if (isoFileExists) {
              if (fileExistsCheckRetries > 0) {
                log.Info ($"ProcessOnChanged: ISO file {isoFilePath} created after {fileExistsCheckRetries}s");
              }
              // wait before stamping
              if (m_delayBeforeStamping > 0) {
                log.Info ($"ProcessOnChanged: info: wait {m_delayBeforeStamping} seconds before stamping");
                Thread.Sleep (1000 * m_delayBeforeStamping);
              }
              try {
                var fileManager = new PprFileStamper (isoFilePath);
                fileManager.RunDirectly ();
              }
              finally {
                // remove index file
                log.Info ($"ProcessOnChanged: info: delete file {fileChangedFullPath}");
                TryRemoveFile (fileChangedFullPath);
              }
            }
            else {
              log.Error ($"ProcessOnChanged: ISO file {isoFilePath} does not exist");
            }
          }
          else {
            log.Info ($"ProcessOnChanged: info: ignore duplicate event File={fileChangedFullPath}");
          }
        }
        else {
          log.Info ($"ProcessOnChanged: info: ignore duplicate event File={fileChangedFullPath} no longer exists");
        }
      }
      catch (Exception ex) {
        log.Error ("ProcessOnChanged: exception", ex);
      }

      SetActive ();
    }

    /// <summary>
    /// Get ISO file path from index file
    /// </summary>
    string GetIsoFilePathFromIndexFile (string indexFile)
    {
      // read only the first line of file and ignore others
      log.Info ($"GetIsoFilePathFromIndexFile: index File={indexFile}");
      try {
        using (StreamReader reader = new StreamReader (indexFile)) {
          return reader.ReadLine ().Trim ();
        }
      }
      catch (Exception ex) {
        log.Error ($"GetIsoFilePathFromIndexFile: failed to read file: {indexFile}", ex);
        throw;
      }
    }

    /// <summary>
    /// remove  ISO file
    /// </summary>
    void TryRemoveFile (string file)
    {
      try {
        log.Info ($"TryRemoveFile: delete file {file}");
        File.Delete (file);
        return;
      }
      catch (Exception ex) {
        log.Error ($"TryRemoveFile: unable to delete file {file}", ex);
        return;
      }
    }
  }
} 
