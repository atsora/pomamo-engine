// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Lemoine.Core.Log;
using static Lemoine.FileRepository.FileRepoPath;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// IFileRepoClient implementation from a local directory
  /// </summary>
  public class FileRepoClientDirectory : IFileRepoClient
  {
    #region Members
    readonly string m_rootPath = "";
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientSharedDir).FullName);

    #region Constructors
    /// <summary>
    /// Root path of all files that are going to be retrieved
    /// </summary>
    /// <param name="rootPath">not empty or null</param>
    public FileRepoClientDirectory (string rootPath)
    {
      Debug.Assert (!string.IsNullOrEmpty (rootPath));

      if (string.IsNullOrEmpty (rootPath)) {
        log.Error ($"FileRepoClientDirectory: empty root path {rootPath}");
        throw new ArgumentNullException (nameof (rootPath), "the root path must be defined (not null or empty)");
      }
      m_rootPath = rootPath;
    }
    #endregion // Constructors

    #region IFileRepoClient implementation
    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <returns></returns>
    public bool Test ()
    {
      try {
        if (Directory.Exists (m_rootPath)) {
          return true;
        }
        else {
          log.Error ($"Test: Root directory {m_rootPath} cannot be found");
          return false;
        }
      }
      catch (Exception ex) {
        log.Error ("Test: got exception", ex);
        return false;
      }
    }

    /// <summary>
    /// List the files in a FileRepository directory
    /// </summary>
    /// <param name="nspace">FileRepository namespace (root directory)</param>
    /// <param name="path">Directory path in FileRepository</param>
    /// <returns></returns>
    public ICollection<string> ListFilesInDirectory (string nspace, string path)
    {
      var sourceDirectoryPath = GetOsPath (nspace, path);

      try {
        if (!Directory.Exists (sourceDirectoryPath)) {
          log.Error ($"ListFilesInDirectory: directory {sourceDirectoryPath} cannot be found");
          return new List<string> ();
        }

        return Directory
          .GetFiles (sourceDirectoryPath)
          .Select (x => new FileInfo (x).Name)
          .ToList ();
      }
      catch (Exception ex) {
        log.Error ("ListFilesInDirectory: got exception", ex);
        throw;
      }
    }

    /// <summary>
    /// List the directories in a FileRepository directory
    /// </summary>
    /// <param name="nspace">FileRepository namespace (root directory)</param>
    /// <param name="path">Directory path in FileRepository</param>
    /// <returns></returns>
    public ICollection<string> ListDirectoriesInDirectory (string nspace, string path)
    {
      string sourceDirectoryPath = GetOsPath (nspace, path);

      try {
        if (!Directory.Exists (sourceDirectoryPath)) {
          log.Error ($"ListDirectoriesInDirectory: directory {sourceDirectoryPath} cannot be found");
          return new List<string> ();
        }

        return Directory
          .GetDirectories (sourceDirectoryPath)
          .Select (x => new DirectoryInfo (x).Name)
          .ToList ();
      }
      catch (Exception ex) {
        log.Error ("ListDirectoriesInDirectory: got exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Copy the file found in the file repository on the name
    /// nspace/path to localPath
    /// </summary>
    /// <param name="nspace">Root directory where the file is</param>
    /// <param name="path">name of the file to be copied from the server</param>
    /// <param name="localPath">Complete path with name of the destination file</param>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public void GetFile (string nspace, string path, string localPath)
    {
      try {
        File.Copy (GetOsPath (nspace, path), localPath);
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetFile: FileNotFound exception", ex);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        log.Error ($"GetFile: nspace={nspace} path={path} localPath={localPath} got exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Get a file in FileRepository in format string.
    /// In case of error, an exception is thrown.
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="optional"></param>
    /// <returns>File in string format</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public string GetString (string nspace, string path, bool optional = false)
    {
      try {
        var ospath = GetOsPath (nspace, path);
        return File.ReadAllText (ospath);
      }
      catch (FileNotFoundException ex) {
        if (optional) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetString: FileNotFound but optional => return an empty string", ex);
          }
          return "";
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ("GetString: FileNotFound exception", ex);
          }
          throw new MissingFileException (nspace, path, ex);
        }
      }
      catch (Exception ex) {
        log.Error ("GetString: got exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Get the binary content of a file.
    /// In case of error, an exception is thrown.
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns>File in string format</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public byte[] GetBinary (string nspace, string path)
    {
      try {
        return File.ReadAllBytes (GetOsPath (nspace, path));
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetBinary: FileNotFound exception", ex);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        log.Error ("GetBinary: got exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Return the date of the latest modification of a distant file
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public DateTime GetLastModifiedDate (string nspace, string path)
    {
      try {
        var fileInfo = new FileInfo (GetOsPath (nspace, path));
        return fileInfo.LastWriteTimeUtc;
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetLastModifiedDate: FileNotFound exception", ex);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        log.Error ("GetLastModifiedDate: got exception", ex);
        throw;
      }
    }
    #endregion // IFileRepoClient implementation

    #region Private methods
    string GetOsPath (string nspace, string path)
    {
      return Path.Combine (m_rootPath, ConvertToOsPath (nspace), ConvertToOsPath (path));
    }
    #endregion // Private methods
  }
}
