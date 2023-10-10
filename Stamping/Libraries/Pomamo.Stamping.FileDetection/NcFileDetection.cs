// Copyright (C) 2023 Atsora Solutions

using System;
using System.IO;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.TargetSpecific.FileRepository;
using System.Collections.Generic;
using Lemoine.Collections;
using System.Collections.Concurrent;

namespace Pomamo.Stamping.FileDetection
{
  /// <summary>
  /// Class to manage the folder containing index files
  /// </summary>
  public class NcFileDetection : ThreadClass, IThreadClass
  {
    /// <summary>
    /// Config set to be used when a list of directories is targeted
    /// </summary>
    static readonly string NC_DIRECTORIES_KEY = "NcFileDetection.Directories";
    static readonly string NC_DIRECTORIES_DEFAULT = "";

    static readonly string NC_DIRECTORY_KEY = "NcFileDetection.Directory";
    static readonly string NC_DIRECTORY_DEFAULT = "";

    static readonly string SERVICE_USE_CURRENT_USER_KEY = "NcFileDetection.ServiceUseCurrentUser";
    static readonly bool SERVICE_USE_CURRENT_USER_DEFAULT = false;

    static readonly string MAX_DELAY_FOR_ISO_FILE_CREATION_KEY = "NcFileDetection.MaxDelayForFileCreation";
    static readonly TimeSpan MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly string DELAY_BEFORE_STAMPING_KEY = "NcFileDetection.DelayBeforeStamping";
    static readonly TimeSpan DELAY_BEFORE_STAMPING_DEFAULT = TimeSpan.FromSeconds (0.5);

    readonly IEnumerable<string> m_indexFilesDirectories;
    readonly bool m_useCurrentUser = false;
    readonly TimeSpan m_daxDelayForIsoFileCreation;
    readonly TimeSpan m_delayBeforeStamping;
    readonly ConcurrentDictionary<string, DateTime> m_processingPaths = new ConcurrentDictionary<string, DateTime> ();

    static readonly ILog log = LogManager.GetLogger (typeof (NcFileDetection).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NcFileDetection ()
    {
      var indexFilesDirectoriesConfig = Lemoine.Info.ConfigSet
        .LoadAndGet<string> (NC_DIRECTORIES_KEY, NC_DIRECTORIES_DEFAULT);
      if (!string.IsNullOrWhiteSpace (indexFilesDirectoriesConfig)) {
        m_indexFilesDirectories = EnumerableString.ParseListString (indexFilesDirectoriesConfig);
      }
      else {
        var indexFilesDirectory = Lemoine.Info.ConfigSet.LoadAndGet<string> (NC_DIRECTORY_KEY, NC_DIRECTORY_DEFAULT);
        m_indexFilesDirectories = new string[] { indexFilesDirectory };
      }
      m_useCurrentUser = Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (SERVICE_USE_CURRENT_USER_KEY, SERVICE_USE_CURRENT_USER_DEFAULT);
      m_daxDelayForIsoFileCreation = Lemoine.Info.ConfigSet
        .LoadAndGet (MAX_DELAY_FOR_ISO_FILE_CREATION_KEY, MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT);
      m_delayBeforeStamping = Lemoine.Info.ConfigSet
        .LoadAndGet (DELAY_BEFORE_STAMPING_KEY, DELAY_BEFORE_STAMPING_DEFAULT);
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
      var path = fileSystemEventArgs.FullPath;
      var detectionDateTime = DateTime.UtcNow;

      try {
        if (!m_processingPaths.TryAdd (path, detectionDateTime)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessOnChanged: {path} already processing");
          }
          m_processingPaths.AddOrUpdate (path, detectionDateTime, (s, d) => d < detectionDateTime ? detectionDateTime : d);
          return;
        }
      }
      catch (Exception ex) {
        log.Error ($"ProcessOnChanged: exception in TryAdd", ex);
        throw;
      }

      try {
        // Check the file is not being written
        var previousLastWriteTime = File.GetLastWriteTime (path);
        long? previousLength = null;
        try {
          previousLength = new FileInfo (path).Length;
        }
        catch (Exception ex) {
          log.Debug ($"ProcessOnChanged: exception for FileInfo.Length", ex);
        }

        while (DateTime.UtcNow <= detectionDateTime.Add (m_daxDelayForIsoFileCreation)) {
          Thread.Sleep (1000);

          var oldDetectionDateTime = detectionDateTime;
          if (!m_processingPaths.TryGetValue (path, out detectionDateTime)) {
            log.Fatal ($"ProcessOnChanged: {path} not found in processing paths, unexpected");
          }
          else if (oldDetectionDateTime < detectionDateTime) {
            if (log.IsDebugEnabled) {
              log.Debug ($"ProcessOnChanged: new detectionDateTime {oldDetectionDateTime}=>{detectionDateTime} => continue");
            }
            continue;
          }
          else if (oldDetectionDateTime > detectionDateTime) {
            log.Fatal ($"ProcessOnChanged: wrong detectionDateTime old={oldDetectionDateTime} > {detectionDateTime}");
          }
          else if (log.IsDebugEnabled) {
            log.Debug ($"ProcessOnChanged: unchanged detectionDateTime {oldDetectionDateTime} => {detectionDateTime}");
          }

          var lastWriteTime = File.GetLastWriteTime (path);
          if (lastWriteTime.Equals (previousLastWriteTime)) {
            try {
              var length = new FileInfo (path).Length;
              if (length == previousLength) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ProcessOnChanged: no writetime change and no length change");
                }
                break;
              }
              else {
                previousLength = length;
              }
            }
            catch (Exception ex) {
              log.Debug ($"ProcessOnChanged: exception for FileInfo.Length", ex);
              if (!previousLength.HasValue) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ProcessOnChanged: exception for FileInfo.Length, but it was unknown before", ex);
                }
                break;
              }
            }
          }
          else {
            previousLastWriteTime = lastWriteTime;
          }
        }

        Thread.Sleep (m_delayBeforeStamping);

        try {
          var fileStamper = new PprFileStamper (path);
          fileStamper.RunDirectly ();
        }
        catch (Exception ex) {
          log.Error ($"ProcessOnChanged: exception in PprFileStamper", ex);
        }
      }
      catch (Exception ex) {
        log.Error ($"ProcessOnChanged: exception", ex);
        throw;
      }
      finally {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessOnChanged: stamping of {path} completed");
          if (!m_processingPaths.TryRemove (path, out var _)) {
            log.Error ($"ProcessOnChanged: TryRemove returned false for path={path}");
          }
        }
      }

    }
  }
}
