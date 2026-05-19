// Copyright (C) 2026 Atsora Solutions
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
  /// IFileRepoClient implementation using file paths (absolute or relative to the file directory)
  /// without a root path
  /// 
  /// nspace is completely omitted here
  /// </summary>
  public class FileRepoClientFilePath : IFileRepoClient
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientFilePath).FullName);

    /// <summary>
    /// Root path of all files that are going to be retrieved
    /// </summary>
    /// <param name="rootPath">not empty or null</param>
    public FileRepoClientFilePath ()
    {
    }

    #region IFileRepoClient implementation
    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <returns></returns>
    public bool Test () => true;

    /// <summary>
    /// List the files in a FileRepository directory
    /// </summary>
    /// <param name="nspace">omitted</param>
    /// <param name="path">Directory path</param>
    /// <returns></returns>
    public ICollection<string> ListFilesInDirectory (string nspace, string path)
    {
      try {
        if (!Directory.Exists (path)) {
          log.Error ($"ListFilesInDirectory: directory {path} cannot be found");
          return new List<string> ();
        }

        return Directory
          .GetFiles (path)
          .Select (x => new FileInfo (x).Name)
          .ToList ();
      }
      catch (Exception ex) {
        log.Error ($"ListFilesInDirectory: got exception for path={path}", ex);
        throw;
      }
    }

    /// <summary>
    /// List the directories in a FileRepository directory
    /// </summary>
    /// <param name="nspace">omitted</param>
    /// <param name="path">Directory path</param>
    /// <returns></returns>
    public ICollection<string> ListDirectoriesInDirectory (string nspace, string path)
    {
      try {
        if (!Directory.Exists (path)) {
          log.Error ($"ListDirectoriesInDirectory: directory {path} cannot be found");
          return new List<string> ();
        }

        return Directory
          .GetDirectories (path)
          .Select (x => new DirectoryInfo (x).Name)
          .ToList ();
      }
      catch (Exception ex) {
        log.Error ($"ListDirectoriesInDirectory: got exception for path={path}", ex);
        throw;
      }
    }

    /// <summary>
    /// Copy the file found in the file repository on the name
    /// nspace/path to localPath
    /// </summary>
    /// <param name="nspace">omitted</param>
    /// <param name="path">name of the file to be copied from the server</param>
    /// <param name="localPath">Complete path with name of the destination file</param>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public void GetFile (string nspace, string path, string localPath)
    {
      try {
        File.Copy (path, localPath);
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
    /// <param name="nspace">omitted</param>
    /// <param name="path"></param>
    /// <param name="optional"></param>
    /// <returns>File in string format</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public string GetString (string nspace, string path, bool optional = false)
    {
      try {
        return File.ReadAllText (path);
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
    /// <param name="nspace">omitted</param>
    /// <param name="path"></param>
    /// <returns>File in string format</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public byte[] GetBinary (string nspace, string path)
    {
      try {
        return File.ReadAllBytes (path);
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetBinary: FileNotFound exception for path={path}", ex);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        log.Error ($"GetBinary: got exception for path={path}", ex);
        throw;
      }
    }

    /// <summary>
    /// Return the date of the latest modification of a distant file
    /// </summary>
    /// <param name="nspace">omitted</param>
    /// <param name="path"></param>
    /// <returns></returns>
    public DateTime GetLastModifiedDate (string nspace, string path)
    {
      try {
        var fileInfo = new FileInfo (path);
        return fileInfo.LastWriteTimeUtc;
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetLastModifiedDate: FileNotFound exception for path={path}", ex);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        log.Error ($"GetLastModifiedDate: got exception for path={path}", ex);
        throw;
      }
    }
    #endregion // IFileRepoClient implementation
  }
}
