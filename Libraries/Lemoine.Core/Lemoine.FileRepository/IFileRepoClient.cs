// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Lemoine.Core.Log;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Description of IFileRepoClient.
  /// </summary>
  public interface IFileRepoClient
  {
    /// <summary>
    /// Test if the implementation is correctly configured and the network active
    /// 
    /// The implementation should catch the exceptions and false returned 
    /// in case of a temporary problem
    /// </summary>
    /// <returns></returns>
    bool Test ();
    
    /// <summary>
    /// List the files in a FileRepository directory
    /// </summary>
    /// <param name="nspace">FileRepository namespace (root directory)</param>
    /// <param name="path">Directory path in FileRepository</param>
    /// <returns></returns>
    ICollection<string> ListFilesInDirectory (string nspace, string path);
    
    /// <summary>
    /// List the directories in a FileRepository directory
    /// </summary>
    /// <param name="nspace">FileRepository namespace (root directory)</param>
    /// <param name="path">Directory path in FileRepository</param>
    /// <returns></returns>
    ICollection<string> ListDirectoriesInDirectory (string nspace, string path);

    /// <summary>
    /// Copy the file found in the file repository on the name
    /// nspace/path to localPath
    /// </summary>
    /// <param name="nspace">Root directory where the file is</param>
    /// <param name="path">name of the file to be copied from the server</param>
    /// <param name="localPath">Complete path with name of the destination file</param>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    void GetFile (string nspace, string path, string localPath);

    /// <summary>
    /// Get a file in FileRepository in format string.
    /// In case of error, an exception is thrown.
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="optional">if optional is true and the resource is not found, then an empty string is returned</param>
    /// <returns>File in string format</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    string GetString (string nspace, string path, bool optional = false);

    /// <summary>
    /// Get the binary content of a file.
    /// In case of error, an exception is thrown.
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns>binary content of the file</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    byte[] GetBinary (string nspace, string path);

    /// <summary>
    /// Return the date of the latest modification of a distant file
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    DateTime GetLastModifiedDate (string nspace, string path);
  }

  /// <summary>
  /// Extensions to interface <see cref="IFileRepoClient" />
  /// </summary>
  public static class FileRepoClientExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientExtensions).FullName);

    /// <summary>
    /// Like GetFile, but do not get download again the file if the local copy is already the most recent one
    /// </summary>
    /// <param name="fileRepoClient"></param>
    /// <param name="distantDirectory">Directory of the file to be copied from the server</param>
    /// <param name="distantFileName">Name of the file to be copied from the server</param>
    /// <param name="localPath">Complete path with name of the destination file</param>
    /// <returns>false if an old version of the file is used</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public static bool SynchronizeFile (this IFileRepoClient fileRepoClient, string distantDirectory, string distantFileName, string localPath)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("SynchronizeFile: {0}/{1} => {2}", distantDirectory, distantFileName, localPath);
      }

      using (var synchronizerFile = new SynchronizerFile ()) {
        if (File.Exists (localPath) && !IsUpdateNeeded (fileRepoClient, synchronizerFile, distantDirectory, distantFileName, localPath)) {
          log.DebugFormat ("SynchronizeFile: {0} is already the latest version, do nothing",
            localPath);
          return true;
        }
        else { // Download requested
          if (log.IsDebugEnabled) {
            log.DebugFormat ($"SynchronizeFile: download of {distantDirectory}/{distantFileName} to {localPath} is requested");
          }
          string backupPath = null;
          if (File.Exists (localPath)) {
            backupPath = localPath + ".synchroBackup";
            if (File.Exists (backupPath)) {
              File.Delete (backupPath);
            }
            File.Move (localPath, backupPath);
          }
          try {
            fileRepoClient.GetFile (distantDirectory, distantFileName, localPath);
            try {
              synchronizerFile.SetDate (localPath,
                fileRepoClient.GetLastModifiedDate (distantDirectory, distantFileName));
              if (null != backupPath) {
                File.Delete (backupPath);
              }
            }
            catch (Exception ex1) {
              if (log.IsErrorEnabled) {
                log.Error ("SynchronizeFile: exception in post-process phase", ex1);
              }
            }
            return true;
          }
          catch (Exception ex) {
            if (log.IsErrorEnabled) {
              log.Error ("SynchronizeFile: GetFile exception", ex);
            }
            if (null != backupPath) {
              if (File.Exists (localPath)) {
                File.Delete (localPath);
              }
              File.Move (backupPath, localPath);
              return false;
            }
            else {
              throw;
            }
          }
        }
      }
    }

    static bool IsUpdateNeeded (IFileRepoClient fileRepoClient, SynchronizerFile synchronizerFile, string distantDirectory, string distantFileName, string localPath)
    {
      // Last modified date of the distant file
      DateTime distantDate = fileRepoClient.GetLastModifiedDate (distantDirectory, distantFileName);

      // Value registered locally
      DateTime? localDate = synchronizerFile.GetDate (localPath);
      if (localDate.HasValue) {
        double distance = distantDate.Subtract (localDate.Value).TotalSeconds;
        return distance > 1 || distance < -1;
      }
      return true;
    }
  }
}
