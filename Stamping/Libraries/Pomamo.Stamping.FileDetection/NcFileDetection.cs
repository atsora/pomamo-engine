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
using System.Threading.Tasks;
using System.Linq;

namespace Pomamo.Stamping.FileDetection
{
  /// <summary>
  /// Class to manage the folder containing index files
  /// </summary>
  public sealed class NcFileDetection : ThreadClass, IThreadClass, IDisposable
  {
    static readonly int MAX_RESET_ATTEMPT = 20;

    /// <summary>
    /// Config set to be used when a list of directories is targeted
    /// </summary>
    static readonly string NC_DIRECTORIES_KEY = "Stamping.NcFileDetection.Directories";
    static readonly string NC_DIRECTORIES_DEFAULT = "";

    static readonly string NC_DIRECTORY_KEY = "Stamping.NcFileDetection.Directory";
    static readonly string NC_DIRECTORY_DEFAULT = "";

    /// <summary>
    /// NC program extensions to keep. If empty, consider all the extensions
    /// </summary>
    static readonly string NC_EXTENSIONS_KEY = "Stamping.NcFileDetection.Extensions";
    static readonly string NC_EXTENSIONS_DEFAULT = "";

    /// <summary>
    /// File extensions to exclude from the search. If empty, do not exclude any specific file
    /// </summary>
    static readonly string EXCLUDE_EXTENSIONS_KEY = "Stamping.NcFileDetection.ExcludeExtensions";
    static readonly string EXCLUDE_EXTENSIONS_DEFAULT = ",.mce,.mcc,.stamp_in_progress,.error";

    /// <summary>
    /// Search recursively in the sub-directories
    /// </summary>
    static readonly string RECURSIVE_KEY = "Stamping.NcFileDetection.Recursive";
    static readonly string RECURSIVE_DEFAULT = ""; // Boolean: empty string means keep the default (true by default)

    static readonly string SERVICE_USE_CURRENT_USER_KEY = "Stamping.NcFileDetection.ServiceUseCurrentUser";
    static readonly bool SERVICE_USE_CURRENT_USER_DEFAULT = false;

    /// <summary>
    /// Try to run the stamper as the owner of the file (only works with a domain user)
    /// </summary>
    static readonly string TRY_FILE_OWNER_KEY = "Stamping.NcFileDetection.TryFileOwner";
    static readonly bool TRY_FILE_OWNER_DEFAULT = false;

    static readonly string MAX_DELAY_FOR_ISO_FILE_CREATION_KEY = "Stamping.NcFileDetection.MaxDelayForFileCreation";
    static readonly TimeSpan MAX_DELAY_FOR_ISO_FILE_CREATION_DEFAULT = TimeSpan.FromSeconds (12);

    static readonly string DELAY_BEFORE_STAMPING_KEY = "Stamping.NcFileDetection.DelayBeforeStamping";
    static readonly TimeSpan DELAY_BEFORE_STAMPING_DEFAULT = TimeSpan.FromSeconds (0.5);

    readonly IEnumerable<string> m_directories;
    readonly bool m_recursive = true; // Recursive search
    readonly IEnumerable<string> m_ncExtensions;
    readonly IEnumerable<string> m_excludeExtensions;
    readonly bool m_useCurrentUser = false;
    readonly bool m_tryFileOwner = false;
    readonly TimeSpan m_maxDelayForIsoFileCreation;
    readonly TimeSpan m_delayBeforeStamping;
    readonly ConcurrentDictionary<string, DateTime> m_processingPaths = new ConcurrentDictionary<string, DateTime> ();
    readonly ConcurrentDictionary<string, FileSystemWatcher> m_watchers = new ConcurrentDictionary<string, FileSystemWatcher> ();
    CancellationToken m_cancellationToken = CancellationToken.None;
    bool m_disposed = false;

    static readonly ILog log = LogManager.GetLogger (typeof (NcFileDetection).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NcFileDetection ()
    {
      var directoriesConfig = Lemoine.Info.ConfigSet
        .LoadAndGet<string> (NC_DIRECTORIES_KEY, NC_DIRECTORIES_DEFAULT);
      if (!string.IsNullOrWhiteSpace (directoriesConfig)) {
        m_directories = EnumerableString.ParseListString (directoriesConfig);
      }
      else {
        var indexFilesDirectory = Lemoine.Info.ConfigSet.LoadAndGet<string> (NC_DIRECTORY_KEY, NC_DIRECTORY_DEFAULT);
        m_directories = new string[] { indexFilesDirectory };
      }
      var ncExtensionsConfig = Lemoine.Info.ConfigSet
        .LoadAndGet (NC_EXTENSIONS_KEY, NC_EXTENSIONS_DEFAULT);
      if (string.IsNullOrEmpty (ncExtensionsConfig)) {
        m_ncExtensions = new string[] { };
      }
      else {
        m_ncExtensions = EnumerableString.ParseListString (ncExtensionsConfig);
      }
      var excludeExtensionsConfig = Lemoine.Info.ConfigSet
        .LoadAndGet (EXCLUDE_EXTENSIONS_KEY, EXCLUDE_EXTENSIONS_DEFAULT);
      if (string.IsNullOrEmpty (excludeExtensionsConfig)) {
        m_excludeExtensions = new string[] { };
      }
      else {
        m_excludeExtensions = EnumerableString.ParseListString (excludeExtensionsConfig);
      }
      var recursiveConfiguration = Lemoine.Info.ConfigSet.LoadAndGet<string> (RECURSIVE_KEY, RECURSIVE_DEFAULT);
      if (!string.IsNullOrEmpty (recursiveConfiguration)) {
        if (bool.TryParse (recursiveConfiguration, out var recursive)) {
          m_recursive = recursive;
        }
      }
      m_useCurrentUser = Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (SERVICE_USE_CURRENT_USER_KEY, SERVICE_USE_CURRENT_USER_DEFAULT);
      m_tryFileOwner = Lemoine.Info.ConfigSet
        .LoadAndGet (TRY_FILE_OWNER_KEY, TRY_FILE_OWNER_DEFAULT);
      m_maxDelayForIsoFileCreation = Lemoine.Info.ConfigSet
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
    protected override async void Run (CancellationToken cancellationToken)
    {
      m_cancellationToken = cancellationToken;

      try {
        foreach (var directory in m_directories.Distinct ()) {
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
        await VisitDirectoryAsync (directory, cancellationToken);
      }
      catch (Exception ex) {
        log.Error ("StartWatchDirectoryAsync: exception", ex);
        this.SetExitRequested ();
        throw;
      }
    }

    async Task VisitDirectoryAsync (string directory, CancellationToken cancellationToken)
    {
      foreach (var filePath in Directory.GetFiles (directory, "*", m_recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {
        cancellationToken.ThrowIfCancellationRequested ();
        await ProcessFileImpersonatedAsync (filePath, cancellationToken);
      }
    }

    void CreateWatcher (string directory, CancellationToken cancellationToken)
    {
      var watcher = new FileSystemWatcher (directory) {
        IncludeSubdirectories = m_recursive,
        InternalBufferSize = 32768, // 32 KB instead of default 8 KB (Max is 64 KB)
        NotifyFilter = NotifyFilters.CreationTime
      | NotifyFilters.FileName
      | NotifyFilters.LastWrite
      | NotifyFilters.Size
      };

      watcher.Changed += OnChanged;
      watcher.Created += OnCreated;
      watcher.Renamed += OnRenamed;
      watcher.Deleted += OnDeleted;
      watcher.Error += OnError;

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
    async void OnChanged (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      await ProcessOnChangedAsync (sender, fileSystemEventArgs);
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    async void OnCreated (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"OnCreated {fileSystemEventArgs.FullPath}");
      await ProcessOnChangedAsync (sender, fileSystemEventArgs);
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
    void OnRenamed (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      log.Info ($"OnRenamed {fileSystemEventArgs.FullPath}");
    }

    /// <summary>
    /// Directory monitoring error event watcher
    /// </summary>
    async void OnError (object sender, ErrorEventArgs e)
    {
      log.Error ($"OnError: exception", e.GetException ());
      try {
        var watcher = (FileSystemWatcher)sender;
        var path = watcher.Path;
        await ResetWatcherAsync (path);
      }
      catch (Exception ex) {
        log.Error ($"OnError: ResetWatcher failed", ex);
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

      await VisitDirectoryAsync (path, CancellationToken.None);
    }

    /// <summary>
    /// Directory monitoring change event watcher
    /// </summary>
    async Task ProcessOnChangedAsync (object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      var path = fileSystemEventArgs.FullPath;
      await ProcessFileImpersonatedAsync (path, m_cancellationToken);
    }

    async Task ProcessFileImpersonatedAsync (string path, CancellationToken cancellationToken)
    {
      if (m_tryFileOwner) {
        try {
          await Lemoine.Core.Security.Identity.RunImpersonatedAsFileOwnerAsync (path, async () => await ProcessFileAsync (path, cancellationToken));
          return;
        }
        catch (Exception ex) {
          log.Debug ($"ProcessFileImpersonatedAsync: with file owner, exception", ex);
        }
      }

      if (m_useCurrentUser) {
        await Lemoine.Core.Security.Identity.RunImpersonatedAsExplorerUserAsync (async () => await ProcessFileAsync (path, cancellationToken));
      }
      else {
        await ProcessFileAsync (path, cancellationToken);
      }
    }

    async Task ProcessFileAsync (string path, CancellationToken cancellationToken)
    {
      if (!File.Exists (path)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessFileAsync: file {path} does not exist any more or is not a valid file, skip it");
        }
        return;
      }
      cancellationToken.ThrowIfCancellationRequested ();

      if (m_ncExtensions.Any ()) {
        var fileExtension = Path.GetExtension (path);
        if (!m_ncExtensions.Any (x => fileExtension.Equals (x, StringComparison.InvariantCultureIgnoreCase))) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessFileAsync: file {path} with extension {fileExtension} is not accepted, allowed extensions are {m_ncExtensions}");
          }
          return;
        }
      }
      if (m_excludeExtensions.Any ()) {
        var fileExtension = Path.GetExtension (path);
        if (m_excludeExtensions.Any (x => fileExtension.Equals (x, StringComparison.InvariantCultureIgnoreCase))) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessFileAsync: file {path} with extension {fileExtension} is not excluded, exclude extensions are {m_excludeExtensions}");
          }
          return;
        }
      }
      cancellationToken.ThrowIfCancellationRequested ();

      var detectionDateTime = DateTime.UtcNow;

      try {
        if (!m_processingPaths.TryAdd (path, detectionDateTime)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessFileAsync: {path} already processing");
          }
          m_processingPaths.AddOrUpdate (path, detectionDateTime, (s, d) => d < detectionDateTime ? detectionDateTime : d);
          return;
        }
      }
      catch (Exception ex) {
        log.Error ($"ProcessOnChanged: exception in TryAdd", ex);
        throw;
      }
      cancellationToken.ThrowIfCancellationRequested ();

      try {
        // Check the file is not being written
        var previousLastWriteTime = File.GetLastWriteTime (path);
        long? previousLength = null;
        try {
          previousLength = new FileInfo (path).Length;
        }
        catch (Exception ex) {
          log.Debug ($"ProcessFileAsync: exception for FileInfo.Length", ex);
        }

        while (DateTime.UtcNow <= detectionDateTime.Add (m_maxDelayForIsoFileCreation)) {
          if (cancellationToken.IsCancellationRequested) {
            log.Warn ($"ProcessFileAsync: cancellation requested for path={path}");
            return;
          }
          SetActive ();
          this.Sleep (TimeSpan.FromSeconds (1), cancellationToken);
          if (cancellationToken.IsCancellationRequested) {
            log.Warn ($"ProcessFileAsync: cancellation requested for path={path}");
            return;
          }

          var oldDetectionDateTime = detectionDateTime;
          if (!m_processingPaths.TryGetValue (path, out detectionDateTime)) {
            log.Fatal ($"ProcessFileAsync: {path} not found in processing paths, unexpected");
          }
          else if (oldDetectionDateTime < detectionDateTime) {
            if (log.IsDebugEnabled) {
              log.Debug ($"ProcessFileAsync: new detectionDateTime {oldDetectionDateTime}=>{detectionDateTime} => continue");
            }
            continue;
          }
          else if (oldDetectionDateTime > detectionDateTime) {
            log.Fatal ($"ProcessFileAsync: wrong detectionDateTime old={oldDetectionDateTime} > {detectionDateTime}");
          }
          else if (log.IsDebugEnabled) {
            log.Debug ($"ProcessFileAsync: unchanged detectionDateTime {oldDetectionDateTime} => {detectionDateTime}");
          }

          var lastWriteTime = File.GetLastWriteTime (path);
          if (lastWriteTime.Equals (previousLastWriteTime)) {
            try {
              var length = new FileInfo (path).Length;
              if (length == previousLength) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ProcessFileAsync: no writetime change and no length change");
                }
                break;
              }
              else {
                previousLength = length;
              }
            }
            catch (Exception ex) {
              log.Debug ($"ProcessFileAsync: exception for FileInfo.Length", ex);
              if (!previousLength.HasValue) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ProcessFileAsync: exception for FileInfo.Length, but it was unknown before", ex);
                }
                break;
              }
            }
          }
          else {
            previousLastWriteTime = lastWriteTime;
          }
        }

        this.Sleep (m_delayBeforeStamping, cancellationToken);
        if (cancellationToken.IsCancellationRequested) {
          log.Warn ($"ProcessFileAsync: cancellation requested for path={path}");
          return;
        }

        try {
          var fileStamper = new PprFileStamper (path);
          await fileStamper.RunAsync (cancellationToken);
        }
        catch (Exception ex) {
          log.Error ($"ProcessFileAsync: exception in PprFileStamper", ex);
        }
      }
      catch (Exception ex) {
        log.Error ($"ProcessFileAsync: exception in PprFileStamper", ex);
      }
      finally {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessFileAsync: stamping of {path} completed");
          this.Sleep (TimeSpan.FromSeconds (2), cancellationToken);
          if (!m_processingPaths.TryRemove (path, out var _)) {
            log.Error ($"ProcessFileAsync: TryRemove returned false for path={path}");
          }
        }
      }

    }

    protected override void Dispose (bool disposing)
    {
      if (!m_disposed) {
        if (disposing) {
          foreach (var watcher in m_watchers.Values) {
            try {
              watcher.EnableRaisingEvents = false;
            }
            finally {
              watcher.Dispose ();
            }
          }
        }

        m_disposed = true;
      }

      base.Dispose (disposing);
    }
  }
}
