// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Lemoine.Core.Log;
using Lemoine.FileRepository;

namespace Lemoine.Core.TargetSpecific.FileRepository
{
  /// <summary>
  /// Description of FileRepoClientSharedDir.
  /// </summary>
  public class FileRepoClientImpersonateSharedDir: IFileRepoClient
  {
    #region Members
    readonly string m_rootPath = "";
    readonly bool m_local = false;
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof(FileRepoClientImpersonateSharedDir).FullName);

    #region Constructors
    /// <summary>
    /// Root path of all files that are going to be retrieved
    /// </summary>
    /// <param name="rootPath"></param>
    public FileRepoClientImpersonateSharedDir(string rootPath)
      : this (rootPath, false)
    {
    }

    /// <summary>
    /// Root path of all files that are going to be retrieved
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="local">does it correspond to a local path ?</param>
    public FileRepoClientImpersonateSharedDir(string rootPath, bool local)
    {
      if (String.IsNullOrEmpty(rootPath)) {
        throw new ArgumentNullException("rootPath", "the string cannot be null or empty");
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
    public bool Test()
    {
      try {
        using (ImpersonationUtils.ImpersonateCurrentUser(!m_local))
        {
          if (Directory.Exists(m_rootPath)) {
            return true;
          }
          else {
            log.ErrorFormat("Test: " +
                            "Root directory {0} cannot be found",
                            m_rootPath);
            return false;
          }
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
    public ICollection<string> ListFilesInDirectory(string nspace, string path)
    {
      string dir = GetPath(nspace, path);
      IList<string> files = new List<string>();
      
      try {
        ICollection<string> filesFull = null;
        using (ImpersonationUtils.ImpersonateCurrentUser(!m_local)) {
          if (Directory.Exists(dir)) {
            filesFull = Directory.GetFiles(dir);
          }
          else {
            log.ErrorFormat("Directory {0} cannot be found", dir);
          }
          
          if (filesFull != null) {
            foreach (string fileFull in filesFull) {
              var fi = new FileInfo(fileFull);
              files.Add(fi.Name);
            }
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
      string dir = GetPath(nspace, path);
      ICollection<string> directories = new List<string>();
      
      try {
        ICollection<string> directoriesFull = null;
        using (ImpersonationUtils.ImpersonateCurrentUser(!m_local))
        {
          if (Directory.Exists(dir)) {
            directoriesFull = Directory.GetDirectories(dir);
          }
          else {
            log.ErrorFormat("Directory {0} cannot be found", dir);
          }

          if (directoriesFull != null) {
            foreach (string dirFull in directoriesFull) {
              var di = new DirectoryInfo(dirFull);
              directories.Add(di.Name);
            }
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat("ListDirectoriesInDirectory: " +
                        "got exception {0}", ex);
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
    public void GetFile(string nspace, string path, string localPath)
    {
      try {
        // Do it in two steps:
        // - once you read the file with ImpersonateCurrentUser
        //   just because you may need some shared network access
        // - once you write a file without ImpersonateCurrentUser
        //   because you may need to write in a folder that is controlled by the system user
        using (ImpersonationUtils.ImpersonateCurrentUser(!m_local))
        {
          File.Copy (GetPath (nspace, path), localPath);
        }
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetString: FileNotFound exception", ex);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        log.ErrorFormat ("GetFile:" +
                         "nspace={0} path={1} localPath={2} " +
                         "got exception {3}",
                         nspace, path, localPath,
                         ex);
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
    public string GetString(string nspace, string path, bool optional = false)
    {
      try {
        using (ImpersonationUtils.ImpersonateCurrentUser (!m_local)) {
          return File.ReadAllText (GetPath (nspace, path));
        }
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetString: FileNotFound exception for {nspace}/{path}, optional={optional}", ex);
        }
        if (optional) {
          return "";
        }
        else {
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
    public byte[] GetBinary(string nspace, string path)
    {
      try {
        using (ImpersonationUtils.ImpersonateCurrentUser(!m_local))
        {
          return File.ReadAllBytes (GetPath (nspace, path));
        }
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
    public DateTime GetLastModifiedDate(string nspace, string path)
    {
      try {
        var fileInfo = new FileInfo(GetPath(nspace, path));
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
    string GetPath(string nspace, string path)
    {
      return Path.Combine(Path.Combine(m_rootPath, nspace), path);
    }
    #endregion // Private methods
  }
}
