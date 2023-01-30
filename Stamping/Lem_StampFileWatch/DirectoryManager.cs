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
    #region Members
    string m_indexFilesDirectory = null;
    static DirectoryManager m_directoryManager;
    static readonly string INDEX_FILES_DIRECTORY_KEY = "StampFileWatch.IndexFilesDirectory";
    static readonly string INDEX_FILES_DIRECTORY_DEFAULT = "C:\\Users\\Public\\AppData\\Lemoine\\Pulse";
    static readonly string SERVICE_USE_CURRENT_USER_KEY = "StampFileWatch.ServiceUseCurrentUser";
    static readonly bool SERVICE_USE_CURRENT_USER_DEFAULT = false;
    static readonly string MAX_DELAY_FOR_ISO_FILE_CREATION_KEY = "StampFileWatch.MaxDelayForFileCreation";
    static readonly int MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT = 5;  // seconds
    static readonly string DELAY_BEFORE_STAMPING_KEY = "StampFileWatchService.DelayBeforeStamping";
    static readonly int DELAY_BEFORE_STAMPING_DEFAULT = 0;  // seconds    
    static readonly string USE_OLD_LEMSTAMP_KEY = "StampFileWatch.UseOldLemStamp";
    static readonly bool USE_OLD_LEMSTAMP_DEFAULT = false;
    static bool m_useCurrentUser = false;
    static DateTime m_lastReadTime = DateTime.MinValue;
    static int m_daxDelayForISOFileCreation;
    static int m_delayBeforeStamping;
    static bool m_useOldLemStamp = false;
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
      m_daxDelayForISOFileCreation = Lemoine.Info.ConfigSet.LoadAndGet<int> (MAX_DELAY_FOR_ISO_FILE_CREATION_KEY, MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT);
      m_delayBeforeStamping = Lemoine.Info.ConfigSet.LoadAndGet<int> (DELAY_BEFORE_STAMPING_KEY, DELAY_BEFORE_STAMPING_DEFAULT);
      m_useOldLemStamp = Lemoine.Info.ConfigSet.LoadAndGet<bool> (USE_OLD_LEMSTAMP_KEY, USE_OLD_LEMSTAMP_DEFAULT);
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
      log.Info ($"DirectoryManager: info: watching {m_indexFilesDirectory}");
      // check directory exists
      if (!Directory.Exists (m_indexFilesDirectory)) {
        log.Info ($"DirectoryManager: Directory {m_indexFilesDirectory} does not exists, try to create it");
        try {
          log.Info ($"DirectoryManager: create folder {m_indexFilesDirectory}");
          Directory.CreateDirectory (m_indexFilesDirectory);
          log.Info ($"DirectoryManager: Directory {m_indexFilesDirectory} created");
        }
        catch (Exception e) {
          log.Fatal ($"DirectoryManager: unable to create folder {m_indexFilesDirectory} {e}");
          log.Fatal ($"DirectoryManager: Directory {m_indexFilesDirectory} does not exists, exiting");
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
      log.Info ($"DirectoryManager: OnCreated {fileSystemEventArgs.FullPath}");
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
    private static void ProcessOnChanged (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      string isoFilePath = null;

      // wait to ensure file is created properly after OnCreated event.
      Thread.Sleep (500);
      WatcherChangeTypes watcherChangeType = fileSystemEventArgs.ChangeType;
      string fileChangedFullPath = fileSystemEventArgs.FullPath;
      // ignore duplicate event and event where file no longer exists
      try {
        if (File.Exists (fileChangedFullPath)) {
          DateTime lastWriteTime = File.GetLastWriteTime (fileChangedFullPath);
          log.Info ($"OnChanged: info: File={fileChangedFullPath}, {watcherChangeType.ToString ()}, {lastWriteTime}");
          if (lastWriteTime != m_lastReadTime) {
            m_lastReadTime = lastWriteTime;
            isoFilePath = m_directoryManager.GetISOFilePathFromIndexFile (fileChangedFullPath);

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
              log.Info ($"OnChanged: info: use old Lem_Stamp = {m_delayBeforeStamping}");
              if (m_useOldLemStamp) {
                ISOFileManagerLemStamp fileManager = new ISOFileManagerLemStamp (fileChangedFullPath, isoFilePath, m_useCurrentUser);
                fileManager.RunDirectly ();
              }
              else {
                ISOFileManager fileManager = new ISOFileManager (fileChangedFullPath, isoFilePath, m_useCurrentUser);
                fileManager.RunDirectly ();
              }

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
        log.Error ("OnChanged: do not exit!!!");
      }

      m_directoryManager.SetActive ();
    }

    /// <summary>
    /// Get ISO file path from index file
    /// </summary>
    private string GetISOFilePathFromIndexFile (string indexFile)
    {
      // read only the first line of file and ignore others
      string filePath = null;
      log.Info ($"getISOFilePathFromIndexFile: info: index File={indexFile}");
      try {
        using (StreamReader reader = new StreamReader (indexFile)) {
          filePath = reader.ReadLine ().Trim();
        }
      }
      catch (Exception e) {
        log.Error ($"getISOFilePathFromIndexFile: info: failed to read file: {indexFile}, {e}");
      }
      return filePath;
    }

    /// <summary>
    /// remove  ISO file
    /// </summary>
    private static void RemoveFile (string file)
    {
      try {
        log.Info ($"removeFile: info: delete file {file}");
        File.Delete (file);
        return;
      }
      catch (Exception e) {
        log.Error ($"removeFile: unable to delete file {file} {e}");
        return;
      }
    }

    #endregion Methods  
  }
} 
