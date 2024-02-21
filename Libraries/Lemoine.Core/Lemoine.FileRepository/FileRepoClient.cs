// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using System.Diagnostics;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Status of a synchronization
  /// </summary>
  public enum SynchronizationStatus
  {
    /// <summary>
    /// No synchronization necessary, already synchronized
    /// </summary>
    ALREADY_SYNCHRONIZED = 1,

    /// <summary>
    /// Synchronization needed and successfull
    /// </summary>
    SYNCHRONIZATION_DONE = 2,

    /// <summary>
    /// Synchronization needed but failed
    /// </summary>
    SYNCHRONIZATION_FAILED = 4,
  }

  /// <summary>
  /// Advanced status of the synchronization
  /// </summary>
  [Flags]
  public enum SynchronizationAdvancedStatus
  {
    /// <summary>
    /// No flag set
    /// </summary>
    None = 0,
    /// <summary>
    /// The synchronization was successfully completed
    /// </summary>
    SuccessfullyCompleted = 1,
    /// <summary>
    /// The synchronized directory may be valid even if the synchronization failed
    /// </summary>
    PossiblyValid = 2,
    /// <summary>
    /// The synchronized directory is for sure not valid
    /// </summary>
    NotValid = 4,
    /// <summary>
    /// New files were synchronized
    /// </summary>
    NewFiles = 8,
    /// <summary>
    /// Files were updated
    /// </summary>
    UpdatedFiles = 16,
    /// <summary>
    /// Files or directories were deleted
    /// </summary>
    DeletedExtra = 32,
    /// <summary>
    /// Warnings were raised
    /// </summary>
    Warning = 64,
    /// <summary>
    /// Errors were raised
    /// </summary>
    Error = 128,
    /// <summary>
    /// No FileRepoClient implementation
    /// </summary>
    NoImplementation = 256,
  }

  /// <summary>
  /// Extensions to DetectionMethod
  /// </summary>
  public static class SynchronizationAdvancedStatusExtensions
  {
    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this SynchronizationAdvancedStatus t, SynchronizationAdvancedStatus other)
    {
      return other == (t & other);
    }

  }

  /// <summary>
  /// FileRepository client
  /// </summary>
  public class FileRepoClient
  {
    const string SYNCHRONIZATION_ERROR_FILE_NAME = "synchronization.error";
    const string SYNCHRONIZATION_WARN_FILE_NAME = "synchronization.warn";
    const string SYNCHRONIZATION_COMPLETION_FILE_NAME = "synchronization.completion";

    #region Members
    IFileRepoClient m_implementation = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoClient).FullName);

    #region Constructors
    /// <summary>
    /// Private constructor => singleton
    /// </summary>
    FileRepoClient ()
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Implementation
    /// </summary>
    public static IFileRepoClient Implementation
    {
      get { return Instance.m_implementation; }
      set { Instance.m_implementation = value; }
    }
    #endregion // Getters / Setters

    #region Methods
    public static bool Test ()
    {
      if (null == Instance.m_implementation) {
        log.Error ("Test: no implementation was set => return false");
        return false;
      }

      var result = Instance.m_implementation.Test ();
      log.Info ($"Test: implementation returned {result}");
      return result;
    }

    /// <summary>
    /// List the files in a FileRepository directory
    /// </summary>
    /// <param name="distantDirectory">Directory path in FileRepository</param>
    /// <returns></returns>
    public static ICollection<string> ListFilesInDirectory (string distantDirectory)
    {
      if (null == Instance.m_implementation) {
        log.Error ("ListFilesInDirectory: no implementation was set => return an empty list");
        return new List<string> ();
      }

      var list = Instance.m_implementation.ListFilesInDirectory (distantDirectory, "");
      log.InfoFormat ("ListFilesInDirectory: found files {0} in directory={1}",
                     String.Join (", ", list.ToArray ()),
                     distantDirectory);
      return list;
    }

    /// <summary>
    /// List the directories in a FileRepository directory
    /// </summary>
    /// <param name="distantDirectory">Directory path in FileRepository</param>
    /// <returns></returns>
    public static ICollection<string> ListDirectoriesInDirectory (string distantDirectory)
    {
      if (null == Instance.m_implementation) {
        log.Error ("ListDirectoriesInDirectory: no implementation was set => return an empty list");
        return new List<string> ();
      }

      var list = Instance.m_implementation.ListDirectoriesInDirectory (distantDirectory, "");
      log.InfoFormat ("ListDirectoriesInDirectory: found directories {0} in directory={1}",
                     String.Join (", ", list.ToArray ()),
                     distantDirectory);
      return list;
    }

    /// <summary>
    /// Copy a file from the FileRepository to the local computer
    /// </summary>
    /// <param name="distantDirectory">Directory of the file to be copied from the server</param>
    /// <param name="distantFileName">Name of the file to be copied from the server</param>
    /// <param name="localPath">Complete path with name of the destination file</param>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public static void GetFile (string distantDirectory, string distantFileName, string localPath)
    {
      if (null == Instance.m_implementation) {
        log.Error ("GetFile: no implementation was set => return an exception");
        throw new NotImplementedException ("Implementation not set");
      }

      Instance.m_implementation.GetFile (distantDirectory, distantFileName, localPath);
    }

    /// <summary>
    /// Like GetFile, but do not get download again the file if the local copy is already the most recent one
    /// </summary>
    /// <param name="distantDirectory">Directory of the file to be copied from the server</param>
    /// <param name="distantFileName">Name of the file to be copied from the server</param>
    /// <param name="localPath">Complete path with name of the destination file</param>
    /// <returns>false if an old version of the file is used</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public static bool SynchronizeFile (string distantDirectory, string distantFileName, string localPath)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"SynchronizeFile: {distantDirectory}/{distantFileName} => {localPath}");
      }

      using (var synchronizerFile = new SynchronizerFile ()) {
        if (File.Exists (localPath) && !IsUpdateNeeded (synchronizerFile, distantDirectory, distantFileName, localPath)) {
          log.Debug ($"SynchronizeFile: {localPath} is already the latest version, do nothing");
          return true;
        }
        else { // Download requested
          if (log.IsDebugEnabled) {
            log.Debug ($"SynchronizeFile: download of {distantDirectory}/{distantFileName} to {localPath} is requested");
          }
          string backupPath = null;
          if (File.Exists (localPath)) {
            backupPath = localPath + ".synchroBackup";
            try {
              if (File.Exists (backupPath)) {
                File.Delete (backupPath);
              }
              File.Move (localPath, backupPath);
            }
            catch (Exception ex) {
              log.Error ($"SynchronizeFile: backup of {localPath} to {backupPath} failed", ex);
              backupPath = null;
            }
          }
          try {
            GetFile (distantDirectory, distantFileName, localPath);
            try {
              synchronizerFile.SetDate (localPath,
                GetLastModifiedDate (distantDirectory, distantFileName));
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
              log.Error ($"SynchronizeFile: GetFile exception for {distantDirectory}/{distantFileName}", ex);
            }
            if (null != backupPath) {
              try {
                log.Info ($"SynchronizeFile: restore {backupPath} into {localPath} after an error");
                if (File.Exists (localPath)) {
                  File.Delete (localPath);
                }
                File.Move (backupPath, localPath);
                return false;
              }
              catch (Exception ex1) {
                log.Error ($"SynchronizeFile: restore of {backupPath} to {localPath} failed", ex1);
                throw;
              }
            }
            else {
              throw;
            }
          }
        }
      }
    }

    /// <summary>
    /// Get a file in FileRepository in format string.
    /// In case of error, an exception is thrown.
    /// </summary>
    /// <param name="distantDirectory">Directory of the file to be copied from the server</param>
    /// <param name="distantFileName">Name of the file to be copied from the server</param>
    /// <param name="optional">If optional is true and there is not the specified resource, then an empty string is returned</param>
    /// <returns>File in string format</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public static string GetString (string distantDirectory, string distantFileName, bool optional = false)
    {
      if (null == Instance.m_implementation) {
        log.Error ("GetString: no implementation was set => return an exception");
        throw new NotImplementedException ("Implementation not set");
      }

      return Instance.m_implementation.GetString (distantDirectory, distantFileName, optional);
    }

    /// <summary>
    /// Get the binary content of a file.
    /// In case of error, an exception is thrown.
    /// </summary>
    /// <param name="distantDirectory">Directory of the file to be copied from the server</param>
    /// <param name="distantFileName">Name of the file to be copied from the server</param>
    /// <returns>File in string format</returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public static byte[] GetBinary (string distantDirectory, string distantFileName)
    {
      if (null == Instance.m_implementation) {
        log.Error ("GetBinary: no implementation was set => return an exception");
        throw new NotImplementedException ("Implementation not set");
      }

      return Instance.m_implementation.GetBinary (distantDirectory, distantFileName);
    }

    /// <summary>
    /// Return the date of the latest modification of a distant file
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="MissingFileException">the source file does not exist</exception>
    public static DateTime GetLastModifiedDate (string nspace, string path)
    {
      if (null == Instance.m_implementation) {
        log.Error ("GetLastModifiedDate: no implementation was set => return an error");
        throw new NotImplementedException ("Implementation not set");
      }

      var result = Instance.m_implementation.GetLastModifiedDate (nspace, path);
      log.Info ($"GetLastModifiedDate: last modified date of {nspace}/{path} is {result}");
      return result;
    }

    /// <summary>
    /// Synchronize only the files
    /// </summary>
    /// <param name="distantDirectory"></param>
    /// <param name="localDirectory"></param>
    /// <param name="filter">if not null, filter the files to synchronize</param>
    /// <param name="errors"></param>
    /// <param name="warnings"></param>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    static SynchronizationAdvancedStatus SynchronizeFilesOnly (string distantDirectory, string localDirectory, Func<IEnumerable<string>, string, bool> filter, ref IList<string> errors, ref IList<string> warnings, CancellationToken? cancellationToken = null)
    {
      SynchronizationAdvancedStatus result = SynchronizationAdvancedStatus.None;
      var token = cancellationToken ?? CancellationToken.None;

      using (var synchronizerFile = new SynchronizerFile ()) {
        IEnumerable<string> filteredFiles;
        { // Files in repository
          IEnumerable<string> srcFiles;
          try {
            srcFiles = ListFilesInDirectory (distantDirectory);
          }
          catch (Exception ex) {
            log.Error ($"SynchronizeFilesOnly: files could not be listed in remote directory {distantDirectory}", ex);
            errors.Add ("Files could not be listed in remote directory " + distantDirectory);
            result |= SynchronizationAdvancedStatus.Error;
            return result;
          }

          // Filter
          if (null != filter) {
            filteredFiles = srcFiles
              .Where (f => filter (srcFiles, f))
              .ToList (); // To apply the filter only once...
          }
          else {
            filteredFiles = srcFiles;
          }
        }
        token.ThrowIfCancellationRequested ();

        // Files in local directory (full path and name with extension)
        IEnumerable<string> destFiles;
        try {
          destFiles = Directory.GetFiles (localDirectory)
            .Select (f => new FileInfo (f).Name);
        }
        catch (Exception ex) {
          log.Error ($"SynchronizeFilesOnly: couldn't list files in local directory {localDirectory}", ex);
          errors.Add ("Files could not be listed in local directory " + localDirectory);
          result |= SynchronizationAdvancedStatus.Error;
          return result;
        }
        token.ThrowIfCancellationRequested ();

        // Delete extra files
        var extraFiles = destFiles
          .Where (f => !filteredFiles.Contains (f));
        foreach (var extraFile in extraFiles) {
          try {
            var localPath = Path.Combine (localDirectory, extraFile);
            File.Delete (localPath);
            synchronizerFile.SetDate (localPath, null);
            log.Info ($"SynchronizeFilesOnly: file {localPath} deleted");
            result |= SynchronizationAdvancedStatus.DeletedExtra;
          }
          catch (Exception ex) {
            log.Error ($"SynchronizeFilesOnly: extra file {extraFile} could not be removed in directory {localDirectory}", ex);
            warnings.Add ("File " + localDirectory + "/" + extraFile + " could not be deleted");
            result |= SynchronizationAdvancedStatus.Warning;
          }
        }
        token.ThrowIfCancellationRequested ();

        // Download missing files
        var missingFiles = filteredFiles
          .Where (f => !destFiles.Contains (f));
        foreach (var missingFile in missingFiles) {
          try {
            var localPath = Path.Combine (localDirectory, missingFile);
            GetFile (distantDirectory, missingFile, localPath);
            synchronizerFile.SetDate (localPath,
              GetLastModifiedDate (distantDirectory, missingFile));
            log.Info ($"SynchronizeFilesOnly: File {localPath} downloaded");
            result |= SynchronizationAdvancedStatus.NewFiles;
          }
          catch (Exception ex) {
            log.Error ($"SynchronizeFilesOnly: missing file {missingFile} in directory {distantDirectory} could not be downloaded", ex);
            errors.Add ("File " + distantDirectory + "/" + missingFile + " could not be downloaded");
            result |= SynchronizationAdvancedStatus.NotValid;
            result |= SynchronizationAdvancedStatus.Error;
          }
        }
        token.ThrowIfCancellationRequested ();

        // Possibly update existing files
        var toUpdateFiles = filteredFiles
          .Where (f => destFiles.Contains (f))
          .Where (f => IsUpdateNeeded (synchronizerFile, distantDirectory, f, Path.Combine (localDirectory, f)));
        foreach (var toUpdateFile in toUpdateFiles) {
          token.ThrowIfCancellationRequested ();
          try {
            if (log.IsDebugEnabled) {
              log.Debug ($"SynchronizeFilesOnly: update {toUpdateFile} in local directory {localDirectory}");
            }
            var localPath = Path.Combine (localDirectory, toUpdateFile);
            var backupPath = localPath + ".synchroBackup";
            if (File.Exists (backupPath)) {
              File.Delete (backupPath);
            }
            File.Move (localPath, backupPath);
            bool downloadOk = false;
            try {
              GetFile (distantDirectory, toUpdateFile, localPath);
              downloadOk = true;
            }
            catch (Exception ex) {
              log.Error ($"SynchronizeFilesOnly: file {toUpdateFile} in directory {localDirectory} could not be downloaded", ex);
              warnings.Add ("File " + distantDirectory + "/" + toUpdateFile + " could not be downloaded, keep the previous one");
              if (File.Exists (localPath)) {
                File.Delete (localPath);
              }
              File.Move (backupPath, localPath);
              result |= SynchronizationAdvancedStatus.Warning;
            }
            if (downloadOk) {
              synchronizerFile.SetDate (localPath, GetLastModifiedDate (distantDirectory, toUpdateFile));
              File.Delete (backupPath);
              log.Info ($"SynchronizeFilesOnly: File {localPath} updated");
              result |= SynchronizationAdvancedStatus.UpdatedFiles;
            }
            else if (File.Exists (backupPath)) {
              File.Delete (backupPath);
            }
          }
          catch (Exception ex) {
            log.Error ($"SynchronizeFilesOnly: file {toUpdateFile} in directory {localDirectory} could not be updated", ex);
            errors.Add ("File " + distantDirectory + "/" + toUpdateFile + " could not be updated");
            result |= SynchronizationAdvancedStatus.NotValid;
            result |= SynchronizationAdvancedStatus.Error;
          }
        }
      }
      token.ThrowIfCancellationRequested ();

      return result;
    }

    /// <summary>
    /// Synchronize only the directories
    /// </summary>
    /// <param name="distantDirectory"></param>
    /// <param name="localDirectory"></param>
    /// <param name="filter"></param>
    /// <param name="errors"></param>
    /// <param name="warnings"></param>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    static SynchronizationAdvancedStatus SynchronizeDirectoriesOnly (string distantDirectory, string localDirectory, Func<IEnumerable<string>, string, bool> filter, ref IList<string> errors, ref IList<string> warnings, CancellationToken? cancellationToken = null)
    {
      var token = cancellationToken ?? CancellationToken.None;
      SynchronizationAdvancedStatus result = SynchronizationAdvancedStatus.None;

      // Directories in repository
      IEnumerable<string> srcDirectories;
      try {
        srcDirectories = ListDirectoriesInDirectory (distantDirectory);
      }
      catch (Exception ex) {
        log.Error ($"SynchronizeDirectoriesOnly: directories could not be listed in remote directory {distantDirectory}", ex);
        errors.Add ("Directories could not be listed in remote directory " + distantDirectory);
        result |= SynchronizationAdvancedStatus.Error;
        return result;
      }
      token.ThrowIfCancellationRequested ();

      // Directories in local directory
      IEnumerable<string> destDirectories;
      try {
        destDirectories = Directory.GetDirectories (localDirectory)
          .Select (d => new DirectoryInfo (d).Name);
      }
      catch (Exception ex) {
        log.Error ($"SynchronizeDirectoriesOnly: couldn't list directories in local directory {localDirectory}", ex);
        errors.Add ("Directories could not be listed in local directory " + localDirectory);
        result |= SynchronizationAdvancedStatus.Error;
        return result;
      }
      token.ThrowIfCancellationRequested ();

      // Delete extra directories
      var extraDirectories = destDirectories
        .Where (f => !srcDirectories.Contains (f));
      foreach (var extraDirectory in extraDirectories) {
        try {
          var localPath = Path.Combine (localDirectory, extraDirectory);
          log.Info ($"SynchronizeDirectoriesOnly: clean directory {localPath}");
          IList<string> cleanDirectoryErrors = new List<string> ();
          CleanDirectory (localPath, ref cleanDirectoryErrors);
          if (cleanDirectoryErrors.Any ()) {
            result |= SynchronizationAdvancedStatus.Warning;
            foreach (var cleanDirectoryError in cleanDirectoryErrors) {
              warnings.Add (cleanDirectoryError);
            }
          }
          result |= SynchronizationAdvancedStatus.DeletedExtra;
        }
        catch (Exception ex) {
          log.Error ($"SynchronizeDirectoriesOnly: extra directory {extraDirectory} could not be removed in directory {localDirectory}", ex);
          warnings.Add ("Directory " + localDirectory + "/" + extraDirectory + " could not be deleted");
          result |= SynchronizationAdvancedStatus.Warning;
        }
      }
      token.ThrowIfCancellationRequested ();

      // Synchronize sub-directories
      var toSynchronizeDirectories = destDirectories
        .Where (d => srcDirectories.Contains (d));
      foreach (var toSynchronizeDirectory in toSynchronizeDirectories) {
        token.ThrowIfCancellationRequested ();
        try {
          var localDirectoryPath = Path.Combine (localDirectory, toSynchronizeDirectory);
          if (!Directory.Exists (localDirectoryPath)) {
            Directory.CreateDirectory (localDirectoryPath);
            log.Info ($"SynchronizeDirectoriesOnly: directory {localDirectoryPath} created");
          }
          var distantDirectoryPath = FileRepoPath.Combine (distantDirectory, toSynchronizeDirectory);
          result |= SynchronizeDirectory (distantDirectoryPath, localDirectoryPath, filter, ref errors, ref warnings);
        }
        catch (Exception ex) {
          log.Error ($"SynchronizeDirectoriesOnly: directory {toSynchronizeDirectory} could not be synchronized in directory {localDirectory}", ex);
          errors.Add ("Directory " + localDirectory + "/" + toSynchronizeDirectory + " could not be synchronized");
          result |= SynchronizationAdvancedStatus.Error;
          result |= SynchronizationAdvancedStatus.NotValid;
        }
      }
      token.ThrowIfCancellationRequested ();

      return result;
    }

    /// <summary>
    /// Synchronize a directory
    /// </summary>
    /// <param name="distantDirectory"></param>
    /// <param name="localDirectory"></param>
    /// <param name="filter">If not null, filter the files to synchronize</param>
    /// <param name="errors"></param>
    /// <param name="warnings"></param>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    static SynchronizationAdvancedStatus SynchronizeDirectory (string distantDirectory, string localDirectory, Func<IEnumerable<string>, string, bool> filter, ref IList<string> errors, ref IList<string> warnings, CancellationToken? cancellationToken = null)
    {
      log.Info ($"SynchronizeDirectory: synchronization of distantDirectory={distantDirectory} to localDirectory={localDirectory}");
      SynchronizationAdvancedStatus result = SynchronizationAdvancedStatus.None;

      if (!Directory.Exists (localDirectory)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SynchronizeDirectory: create directory {localDirectory}");
        }
        Directory.CreateDirectory (localDirectory);
      }

      // Files in repository
      result |= SynchronizeFilesOnly (distantDirectory, localDirectory, filter, ref errors, ref warnings, cancellationToken: cancellationToken);

      // Directories in repository
      result |= SynchronizeDirectoriesOnly (distantDirectory, localDirectory, filter, ref errors, ref warnings, cancellationToken: cancellationToken);

      return result;
    }

    /// <summary>
    /// Synchronize a directory (recursively)
    /// </summary>
    /// <param name="distantDirectory"></param>
    /// <param name="localDirectory"></param>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    public static SynchronizationAdvancedStatus TrySynchronize (string distantDirectory, string localDirectory, CancellationToken? cancellationToken = null)
    {
      return TrySynchronize (distantDirectory, localDirectory, null, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Synchronize a directory (recursively)
    /// </summary>
    /// <param name="distantDirectory"></param>
    /// <param name="localDirectory"></param>
    /// <param name="filter">If not null, filter the files to synchronize</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static SynchronizationAdvancedStatus TrySynchronize (string distantDirectory, string localDirectory, Func<IEnumerable<string>, string, bool> filter, CancellationToken? cancellationToken = null)
    {
      var token = cancellationToken ?? CancellationToken.None;

      if (null == Instance.m_implementation) {
        log.Error ("TrySynchronize: no implementation was set => return FAILURE");
        return SynchronizationAdvancedStatus.NoImplementation | SynchronizationAdvancedStatus.Error;
      }

      log.Info ($"TrySynchronize: synchronization of distantDirectory={distantDirectory} to localDirectory={localDirectory}");
      IList<string> errors = new List<string> ();
      IList<string> warnings = new List<string> ();

      string completionFilePath = Path.Combine (localDirectory, SYNCHRONIZATION_COMPLETION_FILE_NAME);
      bool previousCompletion = File.Exists (completionFilePath);
      try {
        if (File.Exists (completionFilePath)) {
          File.Delete (completionFilePath);
        }
      }
      catch (Exception ex) {
        log.Error ($"TrySynchronize: completion file {completionFilePath} could not be removed", ex);
      }
      token.ThrowIfCancellationRequested ();
      string warnFilePath = null;
      try {
        warnFilePath = Path.Combine (localDirectory, SYNCHRONIZATION_WARN_FILE_NAME);
        File.Delete (warnFilePath);
      }
      catch (Exception ex) {
        log.Error ($"TrySynchronize: warn file {warnFilePath} could not be removed", ex);
      }
      token.ThrowIfCancellationRequested ();
      string errorFilePath = null;
      try {
        errorFilePath = Path.Combine (localDirectory, SYNCHRONIZATION_ERROR_FILE_NAME);
        File.Delete (errorFilePath);
      }
      catch (Exception ex) {
        log.Error ($"TrySynchronize: error file {errorFilePath} could not be removed", ex);
      }

      token.ThrowIfCancellationRequested ();
      var advancedStatus = SynchronizeDirectory (distantDirectory, localDirectory, filter, ref errors, ref warnings);
      if (!advancedStatus.HasFlag (SynchronizationAdvancedStatus.Error)
        || (previousCompletion && !advancedStatus.HasFlag (SynchronizationAdvancedStatus.NotValid))) {
        advancedStatus |= SynchronizationAdvancedStatus.PossiblyValid;
      }
      if (advancedStatus.HasFlag (SynchronizationAdvancedStatus.PossiblyValid)) {
        try {
          File.WriteAllText (completionFilePath, "");
        }
        catch (Exception ex) {
          log.Error ($"TrySynchronize: completion file {completionFilePath} could not be written", ex);
        }
      }
      if (warnings.Any ()) {
        log.Warn ($"TrySynchronize: there is at least one warning in the synchronization of distantDirectory={distantDirectory} to localDirectory={localDirectory}");
        try {
          File.WriteAllLines (warnFilePath, warnings.ToArray ());
        }
        catch (Exception ex) {
          log.Error ($"TrySynchronize: warn file {warnFilePath} could not be written", ex);
        }
      }
      if (errors.Any ()) {
        log.Error ($"TrySynchronize: there is at least one error in the synchronization of distantDirectory={distantDirectory} to localDirectory={localDirectory}, return SYNCHRONIZATION_FAILED");
        try {
          File.WriteAllLines (errorFilePath, errors.ToArray ());
        }
        catch (Exception ex) {
          log.Error ($"TrySynchronize: error file {errorFilePath} could not be written", ex);
        }
        return advancedStatus;
      }
      return advancedStatus;
    }

    /// <summary>
    /// Force the synchronization of a directory (recursively)
    /// </summary>
    /// <param name="distantDirectory"></param>
    /// <param name="localDirectory"></param>
    /// <param name="checkedThread"></param>
    /// <param name="timeout">Optional (default: null)</param>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    public static SynchronizationStatus ForceSynchronize (string distantDirectory, string localDirectory, Lemoine.Threading.IChecked checkedThread = null, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
    {
      return ForceSynchronize (distantDirectory, localDirectory, null, checkedThread, timeout, cancellationToken);
    }

    /// <summary>
    /// Force the synchronization of a directory (recursively)
    /// </summary>
    /// <param name="distantDirectory"></param>
    /// <param name="localDirectory"></param>
    /// <param name="filter">If not null, filter the files to synchronize</param>
    /// <param name="checkedThread"></param>
    /// <param name="timeout">Optional (default is null)</param>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    public static SynchronizationStatus ForceSynchronize (string distantDirectory, string localDirectory, Func<IEnumerable<string>, string, bool> filter, Lemoine.Threading.IChecked checkedThread = null, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
    {
      var token = cancellationToken ?? CancellationToken.None;
      var startDateTime = DateTime.UtcNow;
      DateTime? timeoutDateTime = null;
      if (timeout.HasValue) {
        timeoutDateTime = startDateTime.Add (timeout.Value);
      }
      var fatalLogSet = false;
      while (!token.IsCancellationRequested && (!timeoutDateTime.HasValue || (DateTime.UtcNow < timeoutDateTime.Value))) {
        checkedThread?.SetActive ();
        var advancedStatus = TrySynchronize (distantDirectory, localDirectory, filter);
        if (advancedStatus.HasFlag (SynchronizationAdvancedStatus.NoImplementation)) {
          log.Fatal ("ForceSynchronize: no FileRepo implementation was set => give up");
#if NETSTANDARD
          Debug.Assert (false);
#endif // NETSTANDARD
          throw new NotImplementedException ("No FileRepo implementation");
        }
        else if (advancedStatus.HasFlag (SynchronizationAdvancedStatus.PossiblyValid)) {
          if (advancedStatus.HasFlag (SynchronizationAdvancedStatus.Error)) {
            return SynchronizationStatus.SYNCHRONIZATION_FAILED;
          }
          else if (advancedStatus.HasFlag (SynchronizationAdvancedStatus.UpdatedFiles)
            || advancedStatus.HasFlag (SynchronizationAdvancedStatus.NewFiles)) {
            log.Info ($"ForceSynchronize: in the synchronization of distantDirectory={distantDirectory} to localDirectory={localDirectory} at least one file was updated or created");
            return SynchronizationStatus.SYNCHRONIZATION_DONE;
          }
          else {
            return SynchronizationStatus.ALREADY_SYNCHRONIZED;
          }
        }
        else { // Not possibly valid, retry...
          if (!fatalLogSet && (startDateTime.AddMinutes (1) < DateTime.UtcNow)) {
            log.Fatal ("ForceSynchronize: please check the FileRepo is accessible");
            fatalLogSet = true;
          }
          log.Error ("ForceSynchronize: synchronization could not get a possibly valid directory, retry in 1s");
          checkedThread?.SetActive ();
          token.WaitHandle.WaitOne (1000); // 1s
        }
      }

      log.Error ($"ForceSynchronize: timeout {timeout} reached");
      return SynchronizationStatus.SYNCHRONIZATION_FAILED;
    }

    static void CleanDirectory (string targetDirectory, ref IList<string> errors)
    {
      IEnumerable<string> files;
      try {
        files = Directory.GetFiles (targetDirectory);
      }
      catch (Exception ex) {
        log.Error ($"CleanDirectory: files could not be listed in local directory {targetDirectory}", ex);
        errors.Add ("Files could not be listed in local directory " + targetDirectory + " to remove");
        files = null;
      }
      if (null != files) {
        foreach (var file in files) {
          try {
            File.SetAttributes (file, FileAttributes.Normal);
            File.Delete (file);
          }
          catch (Exception ex) {
            log.Error ($"CleanDirectory: file {file} could not be remove in local directory {targetDirectory}", ex);
            errors.Add ("File " + file + " could not be removed in local directory " + targetDirectory);
          }
        }
      }

      IEnumerable<string> subDirectories;
      try {
        subDirectories = Directory.GetDirectories (targetDirectory);
      }
      catch (Exception ex) {
        log.Error ($"CleanDirectory: sub-directories could not be listed in local directory {targetDirectory}", ex);
        errors.Add ("Sub-directories could not be listed in local directory " + targetDirectory + " to remove");
        subDirectories = null;
      }
      if (null != subDirectories) {
        foreach (var subDirectory in subDirectories) {
          try {
            CleanDirectory (subDirectory, ref errors);
          }
          catch (Exception ex) {
            log.Error ($"CleanDirectory: sub-directory {subDirectory} could not be remove in local directory {targetDirectory}", ex);
            errors.Add ("Sub-directory " + subDirectory + " could not be removed in local directory " + targetDirectory);
          }
        }
      }

      Directory.Delete (targetDirectory, false);
    }

    static bool IsUpdateNeeded (SynchronizerFile synchronizerFile, string distantDirectory, string distantFileName, string localPath)
    {
      // Last modified date of the distant file
      DateTime distantDate = GetLastModifiedDate (distantDirectory, distantFileName);

      // Value registered locally
      DateTime? localDate = synchronizerFile.GetDate (localPath);
      if (localDate.HasValue) {
        double distance = distantDate.Subtract (localDate.Value).TotalSeconds;
        return distance > 1 || distance < -1;
      }
      return true;
    }
    #endregion // Methods

    #region Instance
    static FileRepoClient Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
      static Nested () { }
      internal static readonly FileRepoClient instance = new FileRepoClient ();
    }
    #endregion
  }
}
