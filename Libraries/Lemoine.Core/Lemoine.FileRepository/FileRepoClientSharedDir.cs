// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Lemoine.Core.Log;
using static Lemoine.FileRepository.FileRepoPath;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Description of FileRepoClientSharedDir.
  /// </summary>
  public class FileRepoClientSharedDir : IFileRepoClient
  {
    #region Members
    readonly string m_rootPath = "";
    readonly bool m_local = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientSharedDir).FullName);

    #region Constructors
    /// <summary>
    /// Root path of all files that are going to be retrieved
    /// </summary>
    /// <param name="rootPath"></param>
    public FileRepoClientSharedDir (string rootPath)
      : this (rootPath, false)
    {
    }

    /// <summary>
    /// Root path of all files that are going to be retrieved
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="local">does it correspond to a local path ?</param>
    public FileRepoClientSharedDir (string rootPath, bool local)
    {
      if (String.IsNullOrEmpty (rootPath)) {
        throw new ArgumentNullException ("rootPath", "the string cannot be null or empty");
      }

      m_rootPath = rootPath;
      m_local = local;
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
          log.ErrorFormat ("Test: " +
                          "Root directory {0} cannot be found",
                          m_rootPath);
          return false;
        }
      }
      catch (Exception ex) {
        log.Error ($"Test: got exception", ex);
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
      string dir = GetOsPath (nspace, path);
      IList<string> files = new List<string> ();

      try {
        ICollection<string> filesFull = null;
        if (Directory.Exists (dir)) {
          filesFull = Directory.GetFiles (dir);
        }
        else {
          log.ErrorFormat ("Directory {0} cannot be found", dir);
        }

        if (filesFull != null) {
          foreach (string fileFull in filesFull) {
            var fi = new FileInfo (fileFull);
            files.Add (fi.Name);
          }
        }
      }
      catch (Exception ex) {
        log.Error ("ListFilesInDirectory: got exception", ex);
        throw;
      }

      return files;
    }

    /// <summary>
    /// List the directories in a FileRepository directory
    /// </summary>
    /// <param name="nspace">FileRepository namespace (root directory)</param>
    /// <param name="path">Directory path in FileRepository</param>
    /// <returns></returns>
    public ICollection<string> ListDirectoriesInDirectory (string nspace, string path)
    {
      string dir = GetOsPath (nspace, path);
      ICollection<string> directories = new List<string> ();

      try {
        ICollection<string> directoriesFull = null;
        if (Directory.Exists (dir)) {
          directoriesFull = Directory.GetDirectories (dir);
        }
        else {
          log.ErrorFormat ("Directory {0} cannot be found", dir);
        }

        if (directoriesFull != null) {
          foreach (string dirFull in directoriesFull) {
            var di = new DirectoryInfo (dirFull);
            directories.Add (di.Name);
          }
        }
      }
      catch (Exception ex) {
        log.Error ("ListDirectoriesInDirectory: got exception", ex);
        throw;
      }

      return directories;
    }

    /// <summary>
    /// Copy the file found in the file repository on the name
    /// nspace/path to localPath
    /// </summary>
    /// <param name="nspace">Root directory where the file is</param>
    /// <param name="path">name of the file to be copied from the server</param>
    /// <param name="localPath">Complete path with name of the destination file</param>
    /// <returns></returns>
    public void GetFile (string nspace, string path, string localPath)
    {
      try {
        // Do it in two steps:
        // - once you read the file with ImpersonateCurrentUser
        //   just because you may need some shared network access
        // - once you write a file without ImpersonateCurrentUser
        //   because you may need to write in a folder that is controlled by the system user
        File.Copy (GetOsPath (nspace, path), localPath);
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetString: FileNotFound exception", ex);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        log.Error ($"GetFile:nspace={nspace} path={path} localPath={localPath} got exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Get a file in FileRepository in format string.
    /// In case of error, an exception is thrown.
    /// 
    /// If the file does not exist, an empty string is returned (optional)
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="optional"></param>
    /// <returns>File in string format</returns>
    public string GetString (string nspace, string path, bool optional = false)
    {
      try {
        return File.ReadAllText (GetOsPath (nspace, path));
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
        log.Error ("GetString: exception", ex);
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
        log.Error ("GetBinary: exception", ex);
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
        log.Error ("GetLastModifiedDate: exception", ex);
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
