// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.TargetSpecific.FileRepository;

namespace Lemoine.Stamping.Lem_StampFileWatchService
{
  /// <summary>
  /// Class to manage the folder containing index files
  /// </summary>
  public class DirectoryManager : ThreadClass, IThreadClass
  {
    #region Members
    string m_indexFilesDirectory = null;
    static DirectoryManager m_directoryManager;
    static readonly string INDEX_FILES_DIRECTORY_KEY = "StampFileWatchService.IndexFilesDirectory";
    static readonly string INDEX_FILES_DIRECTORY_DEFAULT = "C:\\Users\\Public\\AppData\\Lemoine\\Pulse";
    static readonly string SERVICE_USE_CURRENT_USER_KEY = "StampFileWatchService.ServiceUseCurrentUser";
    static readonly bool SERVICE_USE_CURRENT_USER_DEFAULT = false;
    static readonly string MAX_DELAY_FOR_ISO_FILE_CREATION_KEY = "StampFileWatchService.MaxDelayForFileCreation";
    static readonly int MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT = 5;  // seconds
    static readonly string DELAY_BEFORE_STAMPING_KEY = "StampFileWatchService.DelayBeforeStamping";
    static readonly int DELAY_BEFORE_STAMPING_DEFAULT = 0;  // seconds    
    static bool m_useCurrentUser = false;
    static DateTime m_lastReadTime = DateTime.MinValue;
    static int m_maxDelayForISOFileCreation;
    static int m_delayBeforeStamping;

    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (DirectoryManager).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DirectoryManager ()
    {
      m_indexFilesDirectory = Lemoine.Info.ConfigSet.LoadAndGet<string> (INDEX_FILES_DIRECTORY_KEY, INDEX_FILES_DIRECTORY_DEFAULT);
      m_directoryManager = this;
      m_useCurrentUser = Lemoine.Info.ConfigSet.LoadAndGet<bool> (SERVICE_USE_CURRENT_USER_KEY, SERVICE_USE_CURRENT_USER_DEFAULT);
      m_maxDelayForISOFileCreation = Lemoine.Info.ConfigSet.LoadAndGet<int> (MAX_DELAY_FOR_ISO_FILE_CREATION_KEY, MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT);
      m_delayBeforeStamping = Lemoine.Info.ConfigSet.LoadAndGet<int> (DELAY_BEFORE_STAMPING_KEY, DELAY_BEFORE_STAMPING_DEFAULT);
    }
    #endregion // Constructors

    #region Methods
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
      log.ErrorFormat ("DirectoryManager: info: watching {0}", m_indexFilesDirectory);
      // check directory exists
      if (!Directory.Exists (m_indexFilesDirectory)) {
        log.ErrorFormat ("DirectoryManager: Directory {0} does not exists, exiting", m_indexFilesDirectory);
        this.SetExitRequested ();
        return;
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
        this.SetExitRequested ();
        return;
      }
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    private static void OnChanged (object sender, FileSystemEventArgs fileSystemEventArgs)
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
    private static void OnCreated (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.ErrorFormat ("DirectoryManager: info: OnCreated ", fileSystemEventArgs.FullPath);
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
    private static void OnDeleted (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.ErrorFormat ("DirectoryManager: info: OnDeleted ", fileSystemEventArgs.FullPath);
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    private static void OnError (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.ErrorFormat ("DirectoryManager: info: OnError ", fileSystemEventArgs.FullPath);
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    private static void OnRenamed (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.ErrorFormat ("DirectoryManager: info: OnRenamed ", fileSystemEventArgs.FullPath);
    }



    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    private static void ProcessOnChanged (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      string isoFilePath = null;

      // wait to ensure file is created properly after OnCreated event.
      Thread.Sleep (500);
      WatcherChangeTypes watcherChangeType = fileSystemEventArgs.ChangeType;
      string fileChangedFullPath = fileSystemEventArgs.FullPath;
      log.Error ($"OnChanged: info: file path={fileChangedFullPath}");
      // ignore duplicate event and event where file no longer exists
      //using (ImpersonationUtils.ImpersonateCurrentUser ()) {
      if (File.Exists (fileChangedFullPath)) {
        DateTime lastWriteTime = File.GetLastWriteTime (fileChangedFullPath);
        log.ErrorFormat ("OnChanged: info: File={0}, {1}, {2}", fileChangedFullPath, watcherChangeType.ToString (), lastWriteTime);
        if (lastWriteTime != m_lastReadTime) {
          m_lastReadTime = lastWriteTime;
          isoFilePath = m_directoryManager.GetISOFilePathFromIndexFile (fileChangedFullPath);

          // test file exists. ISO file may be generated AFTER index file. Wait for it.
          bool isoFileExists = false;
          int fileExistsCheckRetries = 0;
          while(!isoFileExists && fileExistsCheckRetries <= m_maxDelayForISOFileCreation) {
            isoFileExists = File.Exists (isoFilePath);
            if(!isoFileExists) {
              log.ErrorFormat ("OnChanged: info: wait for file {0}", isoFilePath);
              Thread.Sleep (1000);
              fileExistsCheckRetries++;
            }
          }

          if (isoFileExists) {
            if (fileExistsCheckRetries > 0) {
              log.ErrorFormat ("OnChanged: info: ISO file {0} created after {1}s", isoFilePath, fileExistsCheckRetries);
            }
            // wait before stamping
            if (m_delayBeforeStamping > 0) {
              log.Error ($"OnChanged: info: wait {m_delayBeforeStamping} seconds before stamping");
              Thread.Sleep (1000 * m_delayBeforeStamping);
            }
            
            ISOFileManager fileManager = new ISOFileManager (fileChangedFullPath, isoFilePath, m_useCurrentUser);
            fileManager.RunDirectly ();
            // remove index file
            log.ErrorFormat ("OnChanged: info: delete file {0}", fileChangedFullPath);
            RemoveFile (fileChangedFullPath);
          }
          else {
            log.ErrorFormat ("OnChanged: ISO file {0} does not exist", isoFilePath);
          }
        }
        else {
          log.ErrorFormat ("OnChanged: info: ignore duplicate event File={0}", fileChangedFullPath);
        }
      }
      else {
        log.ErrorFormat ("OnChanged: info: ignore duplicate event File={0} no longer exists", fileChangedFullPath);
      }
      //}

      m_directoryManager.SetActive ();
    }

    /// <summary>
    /// Get ISO file path from index file
    /// </summary>
    private string GetISOFilePathFromIndexFile (string indexFile)
    {
      // read only the first line of file and ignore others
      string filePath = null;
      log.ErrorFormat ("getISOFilePathFromIndexFile: info: index File={0}", indexFile);
      try {
        using (StreamReader reader = new StreamReader (indexFile)) {
          filePath = reader.ReadLine ().Trim();
        }
      }
      catch (Exception e) {
        log.ErrorFormat ("getISOFilePathFromIndexFile: failed to read file: {0}, {1}", indexFile, e);
      }
      return filePath;
    }

    /// <summary>
    /// remove  ISO file
    /// </summary>
    private static void RemoveFile (string file)
    {
      try {
        log.ErrorFormat ("removeFile: delete file {0}", file);
        File.Delete (file);
        return;
      }
      catch (Exception e) {
        log.ErrorFormat ("removeFile: unable to delete file {0} {1}", file, e);
        return;
      }
    }

    #endregion Methods  
  }
} 
