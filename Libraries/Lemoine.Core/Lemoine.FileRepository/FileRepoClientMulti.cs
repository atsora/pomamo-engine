// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using System.IO;
using System.Threading;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// FileRepoClient with multiple implementations
  /// </summary>
  public sealed class FileRepoClientMulti : IFileRepoClient
  {
    readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientMulti).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (FileRepoClientMulti).FullName);

    readonly IList<IFileRepoClient> m_clients = new List<IFileRepoClient> ();

    #region Getters / Setters
    /// <summary>
    /// Number of clients
    /// </summary>
    public int Count
    {
      get { return m_clients.Count; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoClientMulti ()
    {
    }

    /// <summary>
    /// Create a new FileRepoClientMulti from a FileRepoClientSharedDirectory and a FileRepoClientWeb
    /// </summary>
    /// <returns></returns>
    public static FileRepoClientMulti CreateFromSharedDirectoryWeb (CancellationToken cancellationToken = default
#if NETSTANDARD
      , System.Net.Http.HttpClient httpClient = null
#endif // NETSTANDARD
      )
    {
      var multiClient = new Lemoine.FileRepository.FileRepoClientMulti ();
      { // PfrDataDir
        var pfrDataDir = Lemoine.Info.PulseInfo.PfrDataDir;
        if (IsPfrdataDirectoryValid (pfrDataDir)) {
          if (slog.IsDebugEnabled) {
            slog.Debug ($"CreateFromSharedDirectoryWeb: add FileRepoClient with PFR data directory {pfrDataDir}");
          }
          multiClient.Add (new Lemoine.FileRepository.FileRepoClientDirectory (pfrDataDir));
        }
      }
      cancellationToken.ThrowIfCancellationRequested ();
      { // FileRepoWeb with the web service URL
        var webServiceUrl = Lemoine.Info.PulseInfo.WebServiceUrl;
        if (!string.IsNullOrEmpty (webServiceUrl)) {
          if (slog.IsDebugEnabled) {
            slog.Debug ($"CreateFromSharedDirectoryWeb: add WebFileRepo {webServiceUrl}");
          }
#if NETSTANDARD
          var validHttpClient = httpClient ?? new System.Net.Http.HttpClient ();
          multiClient.Add (new Lemoine.FileRepository.FileRepoClientWeb (validHttpClient, webServiceUrl));
#else // !NETSTANDARD
          multiClient.Add (new Lemoine.FileRepository.FileRepoClientWeb (webServiceUrl));
#endif // !NETSTANDARD
          cancellationToken.ThrowIfCancellationRequested ();
        }
      }
      cancellationToken.ThrowIfCancellationRequested ();
      if (Lemoine.Info.PulseInfo.UseSharedDirectory) { // FileRepoSharedDir
        var sharedDirectoryPath = Lemoine.Info.PulseInfo.SharedDirectoryPath;
        if (!string.IsNullOrEmpty (sharedDirectoryPath)) {
          if (sharedDirectoryPath.StartsWith (@"\\")) {
            if (slog.IsDebugEnabled) {
              slog.DebugFormat ("CreateFromSharedDirectoryWeb: add FileRepoClient with shared network directory {0}", sharedDirectoryPath);
            }
            multiClient.Add (new Lemoine.FileRepository.FileRepoClientSharedDir (sharedDirectoryPath));
          }
          else { // !StartsWith (@"\\")
            if (slog.IsDebugEnabled) {
              slog.DebugFormat ("CreateFromSharedDirectoryWeb: add FileRepoClient with shared (possibly local) directory {0}", sharedDirectoryPath);
            }
            multiClient.Add (new Lemoine.FileRepository.FileRepoClientDirectory (sharedDirectoryPath));
            multiClient.Add (new Lemoine.FileRepository.FileRepoClientSharedDir (sharedDirectoryPath));
          }
        }
      }

      cancellationToken.ThrowIfCancellationRequested ();
      if (0 == multiClient.m_clients.Count) {
        if (slog.IsErrorEnabled) {
          slog.Error ("CreateFromSharedDirectoryWeb: FileRepoClientMulti initialized with no client");
          slog.ErrorFormat ("CreateFromSharedDirectoryWeb: no client set with Pfr={0} shared={1} web={2}",
            Lemoine.Info.PulseInfo.PfrDataDir,
            Lemoine.Info.PulseInfo.SharedDirectoryPath,
            Lemoine.Info.PulseInfo.WebServiceUrl);
        }
      }

      return multiClient;
    }
    #endregion // Constructors

    static bool IsPfrdataDirectoryValid (string pfrDataDirectory)
    {
      if (string.IsNullOrEmpty (pfrDataDirectory)) {
        if (slog.IsDebugEnabled) {
          slog.DebugFormat ("IsPfrdataDirectoryValid: empty or null");
        }
        return false;
      }
      if (!Directory.Exists (pfrDataDirectory)) {
        slog.DebugFormat ("IsPfrdataDirectoryValid: directory {0} does not exist => return false", pfrDataDirectory);
        return false;
      }
      else { // Check there is at least one CNC configuration file in it
        var cncConfigsDirectory = Path.Combine (pfrDataDirectory, "cncconfigs");
        if (!Directory.Exists (cncConfigsDirectory)) {
          slog.DebugFormat ("IsPfrdataDirectoryValid: directory {0} does not exist => return false", cncConfigsDirectory);
          return false;
        }
        else {
          return true;
        }
      }
    }

    /// <summary>
    /// Add a file repo client
    /// </summary>
    /// <param name="client"></param>
    public void Add (IFileRepoClient client)
    {
      m_clients.Add (client);
    }

    #region IFileRepoClient implementation
    /// <summary>
    /// <see cref="IFileRepoClient"/>
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public byte[] GetBinary (string nspace, string path)
    {
      if (0 == m_clients.Count) {
        log.Fatal ("GetBinary: no client implementation was added to the Multi File Repo client");
        throw new Exception ("Missing MultiFileRepo implementation");
      }

      for (int i = 0; i < m_clients.Count; ++i) {
        var client = m_clients[i];
        try {
          return client.GetBinary (nspace, path);
        }
        catch (MissingFileException) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetBinary: MissingFile exception, no need to visit another client");
          }
          throw;
        }
        catch (Exception ex) {
          if ((m_clients.Count - 1) == i) {
            log.ErrorFormat ("GetBinary: error with last client {0}", client);
            log.Error ("GetBinary: error with last client", ex);
            throw;
          }
          else {
            log.WarnFormat ("GetBinary: error with client #{0} {1}", i, client);
            log.Warn ("GetBinary: error", ex);
          }
        }
      }

      log.Fatal ("GetBinary: unexpected");
      throw new InvalidOperationException ("Unexpected call");
    }

    /// <summary>
    /// <see cref="IFileRepoClient"/>
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="localPath"></param>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public void GetFile (string nspace, string path, string localPath)
    {
      if (0 == m_clients.Count) {
        log.Fatal ("GetFile: no client implementation was added to the Multi File Repo client");
        throw new Exception ("Missing MultiFileRepo implementation");
      }

      for (int i = 0; i < m_clients.Count; ++i) {
        var client = m_clients[i];
        try {
          client.GetFile (nspace, path, localPath);
          return;
        }
        catch (MissingFileException) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetFile: MissingFile exception, no need to visit another client");
          }
          throw;
        }
        catch (Exception ex) {
          if ((m_clients.Count - 1) == i) {
            log.ErrorFormat ("GetFile: error with last client {0}", client);
            log.Error ("GetFile: error with last client", ex);
            throw;
          }
          else {
            log.WarnFormat ("GetFile: error with client #{0} {1}", i, client);
            log.Warn ("GetFile: error", ex);
          }
        }
      }

      log.Fatal ("GetFile: unexpected");
      throw new InvalidOperationException ("Unexpected call");
    }

    /// <summary>
    /// <see cref="IFileRepoClient"/>
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public DateTime GetLastModifiedDate (string nspace, string path)
    {
      if (0 == m_clients.Count) {
        log.Fatal ("GetLastModifiedDate: no client implementation was added to the Multi File Repo client");
        throw new Exception ("Missing MultiFileRepo implementation");
      }

      for (int i = 0; i < m_clients.Count; ++i) {
        var client = m_clients[i];
        try {
          return client.GetLastModifiedDate (nspace, path);
        }
        catch (MissingFileException) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetLastModifiedDate: MissingFile exception, no need to visit another client");
          }
          throw;
        }
        catch (Exception ex) {
          if ((m_clients.Count - 1) == i) {
            log.ErrorFormat ("GetLastModifiedDate: error with last client {0}", client);
            log.Error ("GetLastModifiedDate: error with last client", ex);
            throw;
          }
          else {
            log.WarnFormat ("GetLastModifiedDate: error with client #{0} {1}", i, client);
            log.Warn ("GetLastModifiedDate: error", ex);
          }
        }
      }

      log.Fatal ("GetLastModifiedDate: unexpected");
      throw new InvalidOperationException ("Unexpected call");
    }

    /// <summary>
    /// <see cref="IFileRepoClient"/>
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="optional"></param>
    /// <returns></returns>
    public string GetString (string nspace, string path, bool optional = false)
    {
      if (0 == m_clients.Count) {
        log.Fatal ("GetString: no client implementation was added to the Multi File Repo client");
        throw new Exception ("Missing MultiFileRepo implementation");
      }

      for (int i = 0; i < m_clients.Count; ++i) {
        var client = m_clients[i];
        try {
          return client.GetString (nspace, path, optional);
        }
        catch (MissingFileException) {
          if (log.IsDebugEnabled) {
            log.Debug ("GetString: MissingFile exception, no need to visit another client");
          }
          throw;
        }
        catch (Exception ex) {
          if ((m_clients.Count - 1) == i) {
            log.ErrorFormat ("GetString: error with last client {0}", client);
            log.Error ("GetString: error with last client", ex);
            throw;
          }
          else {
            log.WarnFormat ("GetString: error with client #{0} {1}", i, client);
            log.Warn ("GetString: error", ex);
          }
        }
      }

      log.Fatal ("GetString: unexpected");
      throw new InvalidOperationException ("Unexpected call");
    }

    /// <summary>
    /// <see cref="IFileRepoClient"/>
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public ICollection<string> ListDirectoriesInDirectory (string nspace, string path)
    {
      if (0 == m_clients.Count) {
        log.Fatal ("ListDirectoriesInDirectory: no client implementation was added to the Multi File Repo client");
        throw new Exception ("Missing MultiFileRepo implementation");
      }

      for (int i = 0; i < m_clients.Count; ++i) {
        var client = m_clients[i];
        try {
          return client.ListDirectoriesInDirectory (nspace, path);
        }
        catch (Exception ex) {
          if ((m_clients.Count - 1) == i) {
            log.ErrorFormat ("ListDirectoriesInDirectory: error with last client {0}", client);
            log.Error ("ListDirectoriesInDirectory: error with last client", ex);
            throw;
          }
          else {
            log.WarnFormat ("ListDirectoriesInDirectory: error with client #{0} {1}", i, client);
            log.Warn ("ListDirectoriesInDirectory: error", ex);
          }
        }
      }

      log.Fatal ("ListDirectoriesInDirectory: unexpected");
      throw new InvalidOperationException ("Unexpected call");
    }

    /// <summary>
    /// <see cref="IFileRepoClient"/>
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public ICollection<string> ListFilesInDirectory (string nspace, string path)
    {
      if (0 == m_clients.Count) {
        log.Fatal ("ListFilesInDirectory: no client implementation was added to the Multi File Repo client");
        throw new Exception ("Missing MultiFileRepo implementation");
      }

      for (int i = 0; i < m_clients.Count; ++i) {
        var client = m_clients[i];
        try {
          return client.ListFilesInDirectory (nspace, path);
        }
        catch (Exception ex) {
          if ((m_clients.Count - 1) == i) {
            log.ErrorFormat ("ListFilesInDirectory: error with last client {0}", client);
            log.Error ("ListFilesInDirectory: error with last client", ex);
            throw;
          }
          else {
            log.WarnFormat ("ListFilesInDirectory: error with client #{0} {1}", i, client);
            log.Warn ("ListFilesInDirectory: error", ex);
          }
        }
      }

      log.Fatal ("ListFilesInDirectory: unexpected");
      throw new InvalidOperationException ("Unexpected call");
    }

    /// <summary>
    /// <see cref="IFileRepoClient"/>
    /// </summary>
    /// <returns></returns>
    public bool Test ()
    {
      if (0 == m_clients.Count) {
        log.Fatal ("Test: no client implementation was added to the Multi File Repo client");
        throw new Exception ("Missing MultiFileRepo implementation");
      }

      for (int i = 0; i < m_clients.Count; ++i) {
        var client = m_clients[i];
        try {
          var result = client.Test ();
          if (result) {
            return true;
          }
        }
        catch (Exception ex) {
          if ((m_clients.Count - 1) == i) {
            log.ErrorFormat ("Test: error with last client {0}", client);
            log.Error ("Test: error with last client", ex);
            throw;
          }
          else {
            log.WarnFormat ("Test: error with client #{0} {1}", i, client);
            log.Warn ("Test: error", ex);
          }
        }
      }

      log.Error ("Test: no client returned true");
      return false;
    }
    #endregion // IFileRepoClient implementation
  }
}
