// Copyright (C) 2009-2023 Lemoine Automation Technologies 2023 Nicolas Relange
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;
using System.Collections.Generic;
using Lemoine.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

namespace Pomamo.Stamping.FileDetection
{
  /// <summary>
  /// Class to manage the folder containing index files
  /// </summary>
  public class IndexFileDetection : ThreadClass, IThreadClass
  {
    static readonly int MAX_RESET_ATTEMPT = 20;

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
    static readonly double MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT = 5;  // seconds

    static readonly string DELAY_BEFORE_STAMPING_KEY = "StampFileWatch.DelayBeforeStamping";
    static readonly double DELAY_BEFORE_STAMPING_DEFAULT = 0;  // seconds    

    readonly IEnumerable<string> m_indexFilesDirectories;
    readonly bool m_useCurrentUser = false;
    readonly TimeSpan m_daxDelayForISOFileCreation;
    readonly TimeSpan m_delayBeforeStamping;
    readonly ConcurrentDictionary<string, bool> m_processingPaths = new ConcurrentDictionary<string, bool> ();
    readonly ConcurrentDictionary<string, FileSystemWatcher> m_watchers = new ConcurrentDictionary<string, FileSystemWatcher> ();
    CancellationToken m_cancellationToken = CancellationToken.None;

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
      m_daxDelayForISOFileCreation = TimeSpan.FromSeconds (Lemoine.Info.ConfigSet
        .LoadAndGet (MAX_DELAY_FOR_ISO_FILE_CREATION_KEY, MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT));
      m_delayBeforeStamping = TimeSpan.FromSeconds (Lemoine.Info.ConfigSet
        .LoadAndGet (DELAY_BEFORE_STAMPING_KEY, DELAY_BEFORE_STAMPING_DEFAULT));
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
    protected override async void Run (CancellationToken cancellationToken)
    {
      m_cancellationToken = cancellationToken;

      try {
        foreach (var directory in m_indexFilesDirectories.Distinct ()) {
          log.Info ($"Run: initializing {directory}");
          cancellationToken.ThrowIfCancellationRequested ();
          await StartWatchDirectoryAsync (directory, cancellationToken);
        }

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

    async Task StartWatchDirectoryAsync (string directory, CancellationToken cancellationToken)
    {
      // check directory exists
      if (!Directory.Exists (directory)) {
        log.Info ($"StartWatchDirectoryAsync: Directory {directory} does not exists, try to create it");
        try {
          log.Info ($"StartWatchDirectoryAsync: create folder {directory}");
          Directory.CreateDirectory (directory);
          log.Info ($"StartWatchDirectoryAsync: Directory {directory} created");
        }
        catch (Exception ex) {
          log.Fatal ($"StartWatchDirectoryAsync: unable to create folder {directory}", ex);
          log.Fatal ($"StartWatchDirectoryAsync: Directory {directory} does not exists, exiting");
          this.SetExitRequested ();
          throw;
        }
      }
      cancellationToken.ThrowIfCancellationRequested ();

      // create directory watcher
      try {
        CreateWatcher (directory, cancellationToken);

        // Parse the existing files
        await ProcessDirectoryAsync (directory, cancellationToken);
      }
      catch (Exception ex) {
        log.Error ("StartWatchDirectoryAsync: exception", ex);
        this.SetExitRequested ();
        throw;
      }
    }

    async Task ProcessDirectoryAsync (string indexFilesDirectory, CancellationToken cancellationToken)
    {
      // Parse the existing files
      foreach (var indexFilePath in Directory.GetFiles (indexFilesDirectory)) {
        cancellationToken.ThrowIfCancellationRequested ();
        await ProcessIndexFileAsync (indexFilePath);
      }
    }

    void CreateWatcher (string directory, CancellationToken cancellationToken)
    {
      var watcher = new FileSystemWatcher (directory) {
        IncludeSubdirectories = false,
        InternalBufferSize = 32768, // 32 KB instead of default 8 KB (Max is 64 KB)
        NotifyFilter = NotifyFilters.CreationTime
      | NotifyFilters.FileName
      | NotifyFilters.LastWrite
      | NotifyFilters.Size
      };

      watcher.Changed += async (object sender, FileSystemEventArgs fileSystemEventArgs) =>
        await ProcessOnChangedAsync (sender, fileSystemEventArgs); ;
      watcher.Created += async (object sender, FileSystemEventArgs fileSystemEventArgs) =>
        await ProcessOnChangedAsync (sender, fileSystemEventArgs);
      watcher.Renamed += (object sender, RenamedEventArgs fileSystemEventArgs) =>
        log.Info ($"OnRenamed {fileSystemEventArgs.FullPath}");
      watcher.Deleted += (object sender, FileSystemEventArgs fileSystemEventArgs) =>
        log.Info ($"OnDeleted {fileSystemEventArgs.FullPath}");
      watcher.Error += async (object sender, ErrorEventArgs e) =>
        await ProcessOnErrorAsync (sender, e);

      // start monitoring directory
      while (!m_watchers.TryAdd (directory, watcher) && !cancellationToken.IsCancellationRequested) {
        log.Error ($"CreateWatcher: try add watcher for {directory} in dictionary failed");
        this.Sleep (TimeSpan.FromSeconds (2), cancellationToken);
      }
      watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    async Task ProcessOnChangedAsync (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      var watcherChangeType = fileSystemEventArgs.ChangeType;
      var indexFilePath = fileSystemEventArgs.FullPath;
      if (log.IsDebugEnabled) {
        log.Debug ($"ProcessOnChangedAsync: File={indexFilePath}, {watcherChangeType}");
      }
      await ProcessIndexFileImpersonatedAsync (indexFilePath);
    }

    /// <summary>
    /// Directory monitoring error event watcher
    /// </summary>
    async Task ProcessOnErrorAsync (object sender, ErrorEventArgs e)
    {
      log.Error ($"ProcessOnErrorAsync: exception", e.GetException ());
      try {
        var watcher = (FileSystemWatcher)sender;
        var path = watcher.Path;
        await ResetWatcherAsync (path);
      }
      catch (Exception ex) {
        log.Error ($"ProcessOnErrorAsync: ResetWatcherAsync failed", ex);
      }
    }

    async Task ResetWatcherAsync (string path, int attempt = 0)
    {
      if (attempt > MAX_RESET_ATTEMPT) {
        log.Error ($"ResetWatcherAsync: max attempt for reset reached for {path}");
        this.SetExitRequested ();
        throw new Exception ("Max attempt for reset reached");
      }

      try {
        if (m_watchers.TryGetValue (path, out var oldWatcher)) {
          oldWatcher.EnableRaisingEvents = false;
          while (!m_watchers.TryRemove (path, out _)) {
            log.Error ($"ResetWatcher: TryRemove failed");
            this.Sleep (TimeSpan.FromSeconds (2));
          }
          oldWatcher.Dispose ();
        }
        CreateWatcher (path, CancellationToken.None);
      }
      catch (Exception ex) {
        log.Error ($"ResetWatcher: exception => try again in 2 seconds", ex);
        this.Sleep (TimeSpan.FromSeconds (2));
        await ResetWatcherAsync (path, attempt++);
        return;
      }

      await ProcessDirectoryAsync (path, CancellationToken.None);
    }

    async Task ProcessIndexFileImpersonatedAsync (string indexFilePath)
    {
      if (m_useCurrentUser) {
        await Lemoine.Core.Security.Identity.RunImpersonatedAsExplorerUserAsync (async () => await ProcessIndexFileAsync (indexFilePath));
      }
      else {
        await ProcessIndexFileAsync (indexFilePath);
      }
    }

    async Task ProcessIndexFileAsync (string indexFilePath)
    {
      var token = m_cancellationToken;

      // wait to ensure file is created properly after OnCreated event.
      this.Sleep (TimeSpan.FromSeconds (0.5), token);
      if (token.IsCancellationRequested) {
        log.Warn ($"ProcessIndexFileAsync: cancellation requested for index path={indexFilePath}");
        return;
      }

      if (!File.Exists (indexFilePath)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessIndexFileAsync: index file {indexFilePath} does not exist any more or is not a valid file, skip it");
        }
        return;
      }

      try {
        if (!m_processingPaths.TryAdd (indexFilePath, true)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessIndexFileAsync: {indexFilePath} already processing");
          }
          return;
        }
      }
      catch (Exception ex) {
        log.Error ($"ProcessOnChanged: exception in TryAdd", ex);
        throw;
      }

      // ignore duplicate event and event where file no longer exists
      try {
        var ncProgramPath = GetIsoFilePathFromIndexFile (indexFilePath);

        this.Sleep (m_daxDelayForISOFileCreation, token, () => File.Exists (ncProgramPath), TimeSpan.FromSeconds (1));
        if (token.IsCancellationRequested) {
          log.Warn ($"ProcessIndexFileAsync: cancellation requested for index path={indexFilePath}");
          return;
        }

        if (!File.Exists (ncProgramPath)) {
          log.Error ($"ProcessIndexFileAsync: NC program {ncProgramPath} still does not exist after {m_daxDelayForISOFileCreation}");
        }
        else { // File.Exists (ncProgramPath)
          if (log.IsDebugEnabled && 0 < m_delayBeforeStamping.Ticks) {
            log.Debug ($"ProcessIndexFileAsync: wait {m_delayBeforeStamping} before stamping for {ncProgramPath}");
          }
          this.Sleep (m_delayBeforeStamping, token);
          if (token.IsCancellationRequested) {
            log.Warn ($"ProcessIndexFileAsync: cancellation requested for index path={indexFilePath}");
            return;
          }

          try {
            var fileManager = new PprFileStamper (ncProgramPath);
            await fileManager.RunAsync (token);
          }
          finally {
            // remove index file
            log.Info ($"ProcessIndexFileAsync: delete index file {indexFilePath}");
            TryRemoveFile (indexFilePath);
          }
        }
      }
      catch (Exception ex) {
        log.Error ("ProcessIndexFileAsync: exception", ex);
      }
      finally {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessIndexFileAsync: stamping for index {indexFilePath} completed");
          this.Sleep (TimeSpan.FromSeconds (2), token);
          if (!m_processingPaths.TryRemove (indexFilePath, out var _)) {
            log.Error ($"ProcessIndexFileAsync: TryRemove returned false for index path={indexFilePath}");
          }
        }
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
          return reader.ReadLine ()?.Trim () ?? "";
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
