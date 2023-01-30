// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Lemoine.CorbaHelper;
using Lemoine.Core.Log;

namespace Lemoine.FileRepository.Corba
{
  /// <summary>
  /// Description of FileRepoClientCorba.
  /// </summary>
  public class FileRepoClientCorba : IFileRepoClient
  {
    #region Members
    static readonly int MAX_ATTEMPT_NB = 1;
    PULSE.FileRepository m_fileRepository = null;
    object m_fileRepositoryLock = new object ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientCorba).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public FileRepoClientCorba ()
    {
    }
    #endregion // Constructors

    #region IFileRepoClient implementation
    void InitializeFileRepository ()
    {
      lock (m_fileRepositoryLock) {
        if (null == m_fileRepository) {
          m_fileRepository = (PULSE.FileRepository)CorbaClientConnection.GetObject ("PULSE", "PulseFileRepository");
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient"/>
    /// </summary>
    /// <returns></returns>
    public bool Test ()
    {
      if (null != m_fileRepository) {
        return true;
      }
      else {
        try {
          InitializeFileRepository ();
        }
        catch (Exception ex) {
          log.Error ("Test: InitializeFileRepository failed", ex);
          return false;
        }
        return true;
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
      if (null == m_fileRepository) {
        InitializeFileRepository ();
      }

      ICollection<string> result = new List<string> ();

      try {
        PULSE.DirEntry[] subDirectoryEntries;
        PULSE.FileEntry[] fileEntries;
        m_fileRepository.listDirectory (nspace, path, out subDirectoryEntries, out fileEntries);

        foreach (PULSE.FileEntry fileEntry in fileEntries) {
          result.Add (fileEntry.name);
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("ListFilesInDirectory: got exception {0}", ex);
        throw;
      }

      return result;
    }

    /// <summary>
    /// List the directories in a FileRepository directory
    /// </summary>
    /// <param name="nspace">FileRepository namespace (root directory)</param>
    /// <param name="path">Directory path in FileRepository</param>
    /// <returns></returns>
    public ICollection<string> ListDirectoriesInDirectory (string nspace, string path)
    {
      if (null == m_fileRepository) {
        InitializeFileRepository ();
      }

      ICollection<string> result = new List<string> ();

      try {
        PULSE.DirEntry[] subDirectoryEntries;
        PULSE.FileEntry[] fileEntries;
        m_fileRepository.listDirectory (nspace, path, out subDirectoryEntries, out fileEntries);

        foreach (var dirEntry in subDirectoryEntries) {
          result.Add (dirEntry.name);
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("ListDirectoriesInDirectory: got exception {0}", ex);
        throw;
      }

      return result;
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
      if (null == m_fileRepository) {
        InitializeFileRepository ();
      }

      PULSE.FileIterator fileIterator = null;
      try {
        fileIterator = m_fileRepository.openFile (nspace, path);
        if (null == fileIterator) {
          log.ErrorFormat ("GetFile: fileRepository.openFile nspace={0} path={1} failed",
                           nspace, path);
          throw new Exception ("fileRepository.openFile failed");
        }

        var localDirectory = Path.GetDirectoryName (localPath);
        if (!Directory.Exists (localDirectory)) {
          Directory.CreateDirectory (localDirectory);
        }
        using (FileStream fileStream = File.OpenWrite (localPath)) {
          byte[] oseq;
          bool eof = false;
          while (!eof) {
            fileIterator.getData (32768, out oseq, out eof);
            if (0 == oseq.Length) {
              break;
            }
            fileStream.Write (oseq, 0, oseq.Length);
          }
        }
      }
      catch (PULSE.BadPath ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetFile: BadPath returned", ex);
          log.DebugFormat ("GetFile: BadPath returned for nspace={0} path={1}", nspace, path);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        log.ErrorFormat ("GetFile: nspace={0} path={1}, got exception {2}", nspace, path, ex);
        throw;
      }
      finally {
        if (null != fileIterator) {
          fileIterator.shutdown ();
        }
      }
    }

    /// <summary>
    /// Get a file in FileRepository in format string.
    /// In case of error, an exception is thrown.
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns>File in string format</returns>
    public string GetString (string nspace, string path, bool optional = false)
    {
      return GetString (nspace, path, 0, optional);
    }

    /// <summary>
    /// Get the content of a binary file in FileRepository
    /// In case of error, an exception is thrown.
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns>File in binary format</returns>
    public byte[] GetBinary (string nspace, string path)
    {
      return GetBinary (nspace, path, 0);
    }

    /// <summary>
    /// Return the date of the latest modification of a distant file
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public DateTime GetLastModifiedDate (string nspace, string path)
    {
      if (null == m_fileRepository) {
        InitializeFileRepository ();
      }

      var date = new DateTime ();

      try {
        // Distant file characteristics
        Int32 lowDateTimeWrite, highDateTimeWrite, lowSize, highSize;
        m_fileRepository.getFileInfo (nspace, path, out lowDateTimeWrite, out highDateTimeWrite,
                                     out lowSize, out highSize);
        //Int64 distantLength = ToLong(highSize, lowSize); // Inversion needed here! Not used
        Int64 distantDateTime = ToLong (lowDateTimeWrite, highDateTimeWrite);
        date = DateTime.FromFileTimeUtc (distantDateTime);
      }
      catch (Exception ex) {
        log.ErrorFormat ("GetLastModifiedDate, Corba: got exception {0}", ex);
      }

      return date;
    }
    #endregion // IFileRepoClient implementation

    #region Private methods
    static Int64 ToLong (Int32 low, Int32 high)
    {
      UInt32 uHigh = unchecked((UInt32)high);
      UInt32 uLow = unchecked((UInt32)low);

      UInt64 tmp = uHigh;
      tmp = unchecked(tmp << 32 | uLow);

      Int64 result = unchecked((Int64)tmp);
      return result < 0 ? 0 : result;
    }

    /// <summary>
    /// Get a file in FileRepository in format string.
    /// In case of error, an exception is thrown.
    /// In case of error, if attempt == 0, we try again
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="attempt">Attempt number</param>
    /// <param name="optional"></param>
    /// <returns>File in string format</returns>
    string GetString (string nspace, string path, int attempt, bool optional = false)
    {
      if (null == m_fileRepository) {
        InitializeFileRepository ();
      }

      string result = "";
      PULSE.FileIterator fileIterator = null;

      try {
        fileIterator = m_fileRepository.openFile (nspace, path);
        if (null == fileIterator) {
          log.ErrorFormat ("GetString: fileRepository.openFile nspace={0} path={1} failed",
                           nspace, path);
          throw new Exception ("fileRepository.openFile failed");
        }

        byte[] oseq;
        bool eof = false;
        while (!eof) {
          fileIterator.getData (32768, out oseq, out eof);
          if (0 == oseq.Length) {
            break;
          }
          foreach (char c in oseq) {
            result += c;
          }
        }
      }
      catch (PULSE.BadPath ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetFile: BadPath returned for {nspace}/{path}, optional={optional}", ex);
        }
        if (optional) {
          return "";
        }
        else {
          throw new MissingFileException (nspace, path, ex);
        }
      }
      catch (Exception ex) {
        if (attempt < MAX_ATTEMPT_NB) {
          log.WarnFormat ("GetString: nspace={0} path={1} got exception {2} but make a new attempt " +
                          "after resetting the CORBA connection", nspace, path, ex);
          if (null != fileIterator) {
            fileIterator.shutdown ();
            fileIterator = null;
          }
          CorbaClientConnection.ResetChannel ();
          return GetString (nspace, path, attempt + 1, optional);
        }
        else {
          log.ErrorFormat ("GetString: nspace={0} path={1} got exception {2} give up", nspace, path, ex);
          throw;
        }
      }
      finally {
        if (null != fileIterator) {
          fileIterator.shutdown ();
        }
      }

      return result;
    }

    /// <summary>
    /// Get the binary content of a file in FileRepository
    /// In case of error, an exception is thrown.
    /// In case of error, if attempt == 0, we try again
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="attempt">Attempt number</param>
    /// <returns>File in string format</returns>
    byte[] GetBinary (string nspace, string path, int attempt)
    {
      if (null == m_fileRepository) {
        InitializeFileRepository ();
      }

      PULSE.FileIterator fileIterator = null;

      try {
        fileIterator = m_fileRepository.openFile (nspace, path);
        if (null == fileIterator) {
          log.ErrorFormat ("GetBinary: fileRepository.openFile nspace={0} path={1} failed",
                           nspace, path);
          throw new Exception ("fileRepository.openFile failed");
        }

        byte[] oseq;
        bool eof = false;
        using (var stream = new MemoryStream ()) {
          while (!eof) {
            fileIterator.getData (32768, out oseq, out eof);
            if (0 == oseq.Length) {
              break;
            }
            stream.Write (oseq, 0, oseq.Length);
          }
          return stream.ToArray ();
        }
      }
      catch (PULSE.BadPath ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetFile: BadPath returned", ex);
          log.DebugFormat ("GetFile: BadPath returned for nspace={0} path={1}", nspace, path);
        }
        throw new MissingFileException (nspace, path, ex);
      }
      catch (Exception ex) {
        if (attempt < MAX_ATTEMPT_NB) {
          log.WarnFormat ("GetFile: got exception {0} but make a new attempt " +
                          "after resetting the CORBA connection", ex);
          if (null != fileIterator) {
            fileIterator.shutdown ();
            fileIterator = null;
          }
          CorbaClientConnection.ResetChannel ();
          return GetBinary (nspace, path, attempt + 1);
        }
        else {
          log.ErrorFormat ("GetFile: got exception {0} give up", ex);
          throw;
        }
      }
      finally {
        if (null != fileIterator) {
          fileIterator.shutdown ();
        }
      }
    }
    #endregion // Private methods
  }
}
