// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.TargetSpecific.FileRepository;

namespace Lemoine.Stamping.Lem_StampFileWatch
{
  /// <summary>
  /// Class to manage the folder containing index files
  /// </summary>
  public class DirectoryManager : ThreadClass, IThreadClass
  {
    static readonly string INDEX_FILES_DIRECTORY_KEY = "StampFileWatch.IndexFilesDirectory";
    static readonly string INDEX_FILES_DIRECTORY_DEFAULT = "C:\\Users\\Public\\AppData\\Lemoine\\Pulse";

    static readonly string SERVICE_USE_CURRENT_USER_KEY = "StampFileWatch.ServiceUseCurrentUser";
    static readonly bool SERVICE_USE_CURRENT_USER_DEFAULT = false;

    static readonly string MAX_DELAY_FOR_ISO_FILE_CREATION_KEY = "StampFileWatch.MaxDelayForFileCreation";
    static readonly int MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT = 5;  // seconds

    static readonly string DELAY_BEFORE_STAMPING_KEY = "StampFileWatch.DelayBeforeStamping";
    static readonly int DELAY_BEFORE_STAMPING_DEFAULT = 0;  // seconds    

    readonly string m_indexFilesDirectory;
    readonly bool m_useCurrentUser = false;
    DateTime m_lastReadTime = DateTime.MinValue;
    readonly int m_daxDelayForISOFileCreation;
    readonly int m_delayBeforeStamping;

    static readonly ILog log = LogManager.GetLogger (typeof (DirectoryManager).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DirectoryManager ()
    {
      m_indexFilesDirectory = Lemoine.Info.ConfigSet.LoadAndGet<string> (INDEX_FILES_DIRECTORY_KEY, INDEX_FILES_DIRECTORY_DEFAULT);
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
      log.Info ($"Run: watching {m_indexFilesDirectory}");
      // check directory exists
      if (!Directory.Exists (m_indexFilesDirectory)) {
        log.Info ($"Run: Directory {m_indexFilesDirectory} does not exists, try to create it");
        try {
          log.Info ($"Run: create folder {m_indexFilesDirectory}");
          Directory.CreateDirectory (m_indexFilesDirectory);
          log.Info ($"Run: Directory {m_indexFilesDirectory} created");
        }
        catch (Exception ex) {
          log.Fatal ($"Run: unable to create folder {m_indexFilesDirectory}", ex);
          log.Fatal ($"Run: Directory {m_indexFilesDirectory} does not exists, exiting");
          this.SetExitRequested ();
          return;
        }
      }
    
      // create directory watcher
      try {
        FileSystemWatcher directoryWatcher = new FileSystemWatcher (m_indexFilesDirectory);
        
        directoryWatcher.Changed += OnChanged;
        directoryWatcher.Created += OnCreated;
        directoryWatcher.Renamed += OnRenamed;
        directoryWatcher.Deleted += OnDeleted;


        // start monitoring directory
        directoryWatcher.EnableRaisingEvents = true;

        // to keep thread alive
        while (!cancellationToken.IsCancellationRequested && !this.ExitRequested) {
          SetActive ();
          this.Sleep (TimeSpan.FromSeconds (10), cancellationToken, () => this.ExitRequested);
        }
      }
      catch (Exception ex) {
        log.Error ("DirectoryManager: exception", ex);
        log.Error ("DirectoryManager: do not exit!!!");
        //this.SetExitRequested ();
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
      log.Info ($"DirectoryManager: OnCreated {fileSystemEventArgs.FullPath}");
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
    private static void OnDeleted (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"DirectoryManager: OnDeleted {fileSystemEventArgs.FullPath}");
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    private static void OnError (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"DirectoryManager: OnError {fileSystemEventArgs.FullPath}");
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    private static void OnRenamed (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"DirectoryManager: OnRenamed {fileSystemEventArgs.FullPath}");
    }



    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    void ProcessOnChanged (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      // wait to ensure file is created properly after OnCreated event.
      Thread.Sleep (500);
      WatcherChangeTypes watcherChangeType = fileSystemEventArgs.ChangeType;
      string fileChangedFullPath = fileSystemEventArgs.FullPath;
      // ignore duplicate event and event where file no longer exists
      try {
        if (File.Exists (fileChangedFullPath)) {
          DateTime lastWriteTime = File.GetLastWriteTime (fileChangedFullPath);
          log.Info ($"OnChanged: info: File={fileChangedFullPath}, {watcherChangeType}, {lastWriteTime}");
          if (lastWriteTime != m_lastReadTime) {
            m_lastReadTime = lastWriteTime;
            var isoFilePath = GetISOFilePathFromIndexFile (fileChangedFullPath);

            // test file exists. ISO file may be generated AFTER index file. Wait for it.
            bool isoFileExists = false;
            int fileExistsCheckRetries = 0;
            while (!isoFileExists && fileExistsCheckRetries <= m_daxDelayForISOFileCreation) {
              isoFileExists = File.Exists (isoFilePath);
              if (!isoFileExists) {
                log.Info ("OnChanged: info: wait for file {isoFilePath}");
                Thread.Sleep (1000);
                fileExistsCheckRetries++;
              }
            }

            if (isoFileExists) {
              if (fileExistsCheckRetries > 0) {
                log.Info ($"OnChanged: ISO file {isoFilePath} created after {fileExistsCheckRetries}s");
              }
              // wait before stamping
              if (m_delayBeforeStamping > 0) {
                log.Info ($"OnChanged: info: wait {m_delayBeforeStamping} seconds before stamping");
                Thread.Sleep (1000 * m_delayBeforeStamping);
              }
              var fileManager = new ISOFileManager (isoFilePath);
              fileManager.RunDirectly ();

              // remove index file
              log.Info ($"OnChanged: info: delete file {fileChangedFullPath}");
              RemoveFile (fileChangedFullPath);
            }
            else {
              log.Error ($"OnChanged: ISO file {isoFilePath} does not exist");
            }
          }
          else {
            log.Info ($"OnChanged: info: ignore duplicate event File={fileChangedFullPath}");
          }
        }
        else {
          log.Info ($"OnChanged: info: ignore duplicate event File={fileChangedFullPath} no longer exists");
        }
      }
      catch (Exception ex) {
        log.Error ("OnChanged: exception", ex);
      }

      SetActive ();
    }

    /// <summary>
    /// Get ISO file path from index file
    /// </summary>
    private string GetISOFilePathFromIndexFile (string indexFile)
    {
      // read only the first line of file and ignore others
      log.Info ($"GetISOFilePathFromIndexFile: index File={indexFile}");
      try {
        using (StreamReader reader = new StreamReader (indexFile)) {
          return reader.ReadLine ().Trim ();
        }
      }
      catch (Exception ex) {
        log.Error ($"GetISOFilePathFromIndexFile: failed to read file: {indexFile}", ex);
        throw;
      }
    }

    /// <summary>
    /// remove  ISO file
    /// </summary>
    private static void RemoveFile (string file)
    {
      try {
        log.Info ($"RemoveFile: delete file {file}");
        File.Delete (file);
        return;
      }
      catch (Exception ex) {
        log.Error ($"RemoveFile: unable to delete file {file}", ex);
        return;
      }
    }
  }
} 
