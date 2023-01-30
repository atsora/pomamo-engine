// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.TargetSpecific.FileRepository;

namespace Lemoine.Stamping.Lem_StampFileWatch
{
  /// <summary>
  /// Class to manage ISO file stamping
  /// </summary>
  public class ISOFileManagerLemStamp : ThreadClass, IThreadClass
  {
    #region Members
    string m_indexFileFullPath = null;
    string m_ISOFileFullPath = null;
    string m_ISOFileName = null;
    string m_renamedISOFileFullPath = null;
    string m_tempISOFileFullPath = null;
    string m_tempFolder = null;  // TODO
    string m_stampingProcess = null; // TODO
    string m_tempFileProgressExtension = null;
    string m_errorFileExtension = null;
    bool m_useCurrentUser = false;
    #endregion

    static readonly string ISO_FILE_PROGRESS_EXTENSION_KEY = "StampFileWatch.FileProgressExtension";
    static readonly string ISO_FILE_PROGRESS_EXTENSION_DEFAULT = ".stamp_in_progress";
    static readonly string ERROR_FILE_EXTENSION_KEY = "StampFileWatch.ErrorExtension";
    static readonly string ERROR_FILE_EXTENSION_DEFAULT = ".stamp_error";
    static readonly string TEMP_DIRECTORY_KEY = "StampFileWatch.StampingTempDirectory";
    static readonly string TEMP_DIRECTORY_DEFAULT = "C:\\temp";
    static readonly string LEM_STAMP_PROCESS_NAME_KEY = "StampFileWatch.LemStampProcess";
    static readonly string LEM_STAMP_PROCESS_NAME_DEFAULT = "";
    static readonly string PPR_TAG_PREFIX_KEY = "StampFileWatch.PPR.";
    static readonly ILog log = LogManager.GetLogger (typeof (DirectoryManager).FullName);
    static readonly string LEM_STAMP_ERROR_EXTENSION = ".mce";
    static readonly string PPR_TAG_KEYWORD = "[PPR]";
    static readonly Regex PPR_TAG_REGEX = new Regex ("\\[PPR\\] *\\= *(?<destination>[a-zA-Z0-9_\\-]+( *[a-zA-Z0-9_\\-]*)*)(\\)|\\]| |).*", RegexOptions.Compiled);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="indexFileFullPath"></param>
    /// <param name="isoFileFullPath"></param>
    /// <param name="useCurrentUser"></param>
    public ISOFileManagerLemStamp (string indexFileFullPath, string  isoFileFullPath, bool useCurrentUser)
    {
      m_indexFileFullPath = indexFileFullPath;
      m_ISOFileFullPath = isoFileFullPath;
      m_ISOFileName = Path.GetFileName (isoFileFullPath);
      m_stampingProcess = Lemoine.Info.ConfigSet.LoadAndGet<string> (LEM_STAMP_PROCESS_NAME_KEY, LEM_STAMP_PROCESS_NAME_DEFAULT);
      m_tempFolder = Lemoine.Info.ConfigSet.LoadAndGet<string> (TEMP_DIRECTORY_KEY, TEMP_DIRECTORY_DEFAULT);
      m_tempFileProgressExtension = Lemoine.Info.ConfigSet.LoadAndGet<string> (ISO_FILE_PROGRESS_EXTENSION_KEY, ISO_FILE_PROGRESS_EXTENSION_DEFAULT);
      m_errorFileExtension = Lemoine.Info.ConfigSet.LoadAndGet<string> (ERROR_FILE_EXTENSION_KEY, ERROR_FILE_EXTENSION_DEFAULT);
      m_useCurrentUser = useCurrentUser;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// process ISO file
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      m_renamedISOFileFullPath = string.Concat (m_ISOFileFullPath, m_tempFileProgressExtension);
      m_tempISOFileFullPath = Path.Combine (m_tempFolder, m_ISOFileName);

      // test temp folder exists and create if not
      if (!Directory.Exists (m_tempFolder)) {
        if (!CreateFolder (m_tempFolder)) {
          return;
        }
      }

      if ( m_useCurrentUser) {
        using (ImpersonationUtils.ImpersonateCurrentUser ()) {
          ProcessIsoFile ();
        }
      }
      else {
        ProcessIsoFile ();
      }
      return;
    }

    /// <summary>
    /// process ISO file
    /// </summary>
    public void ProcessIsoFile ()
    {
      string finalDestinationFullPath = null;
      string finalDestinationFolder = null;
      // rename file during stamping process
      if (!RenameFile (m_ISOFileFullPath, m_renamedISOFileFullPath)) {
        return;
      }
      // copy file to temporary folder
      if (!CopyFile (m_renamedISOFileFullPath, m_tempISOFileFullPath)) {
        // restore file
        RenameFile (m_renamedISOFileFullPath, m_ISOFileFullPath);
        return;
      }
      // get PPR from ISO file
      string programPPR = GetPPRFromISoFile (m_tempISOFileFullPath);
      // get destination from PPR. Default to orginal path
      finalDestinationFullPath = m_ISOFileFullPath;
      if (!string.IsNullOrEmpty (programPPR)) {
        finalDestinationFolder = GetDestinationFromPPR (programPPR);
        if (!string.IsNullOrEmpty (finalDestinationFolder)) {
          log.ErrorFormat ("ProcessIsoFile: programPPR {0} not configured in options file. Keep original folder", programPPR);
          finalDestinationFullPath = Path.Combine (finalDestinationFolder, m_ISOFileName);
        }
      }

      log.ErrorFormat ("ProcessIsoFile: read PPR from config: info: PPR={0} destination={1}", programPPR, finalDestinationFullPath);

      // call stamp Process
      int stampingResult = -1;
      if (m_useCurrentUser) {
        stampingResult = RunStampingProcessAsCurrentUser ();
      }
      else {
        stampingResult = RunStampingProcess ();
      }
      if (stampingResult != 0) {
        // restore file
        RenameFile (m_renamedISOFileFullPath, m_ISOFileFullPath);
        return;
      }
      // test destination folder exists and create if not
      if (!string.IsNullOrEmpty (finalDestinationFolder)) {
        if (!Directory.Exists (finalDestinationFolder)) {
          if (!CreateFolder (finalDestinationFolder)) {
            log.ErrorFormat ("ProcessIsoFile: unable to create destination folder, use original program one");
            finalDestinationFullPath = m_ISOFileFullPath;
          }
        }
      }
      // copy stamped file to destination
      if (!CopyFile (m_tempISOFileFullPath, finalDestinationFullPath)) {
        // if failed copy to original location
        if (!CopyFile (m_tempISOFileFullPath, m_ISOFileFullPath)) {
          // if failed restore original file
          RenameFile (m_renamedISOFileFullPath, m_ISOFileFullPath);
        }
        return;
      }
      // remove temp file
      RemoveFile (m_tempISOFileFullPath);
      // remove renamed
      RemoveFile (m_renamedISOFileFullPath);
    }

    /// <summary>
    /// rename ISO file 
    /// </summary>
    bool RenameFile (string fromFile, string toFile)
    {
      try {
        log.ErrorFormat ("RenameFile: info: rename file {0} to {1}", fromFile, toFile);
        File.Move (fromFile, toFile);
        return true;
      }
      catch (Exception e) {
        log.ErrorFormat ("RenameFile: unable to rename file {0} to {1} {2}", fromFile, toFile, e);
        return false;
      }
    }

    /// <summary>
    /// copy file to temporary folder
    /// </summary>
    bool CopyFile (string fromFile, string toFile)
    {
      try {
        log.ErrorFormat ("CopyFile: info: copy file {0} to {1}", fromFile, toFile);
        File.Copy (fromFile, toFile, true);
        return true;
      }
      catch (Exception e) {
        log.ErrorFormat ("CopyFile: unable to copy file {0} to {1} {2}", fromFile, toFile, e);
        return false;
      }
    }

    /// <summary>
    /// remove  ISO file
    /// </summary>
    void RemoveFile (string file)
    {
      try {
        log.ErrorFormat ("RemoveFile: info: delete file {0}", file);
        File.Delete (file);
        return;
      }
      catch (Exception e) {
        log.ErrorFormat ("RemoveFile: unable to delete file {0} {1}", file, e);
        return;
      }
    }

    /// <summary>
    /// create folder
    /// </summary>
    bool CreateFolder (string folder)
    {
      try {
        log.ErrorFormat ("CreateFolder: info: create folder {0}", folder);
        Directory.CreateDirectory (folder);
        return true;
      }
      catch (Exception e) {
        log.ErrorFormat ("CreateFolder: unable to create folder {0} {1}", folder, e);
        return false;
      }
    }

    /// <summary>
    /// run stamping process in temporary folder
    /// DO NOT WORK INSIDE WINDOWS SERVICE IF NETWORK DRIVES ARE USED !!!
    /// </summary>
    int RunStampingProcess ()
    {
      try {
        // use/create temp working directory to run process
        string tempWorkingDir = Path.Combine (m_tempFolder, "idx-temp");
        if (!Directory.Exists (tempWorkingDir)) {
          if (!CreateFolder (tempWorkingDir)) {
            return -1;
          }
        }
        Process process = new Process ();
        // process.StartInfo = new ProcessStartInfo (m_stampingProcess, m_tempISOFileFullPath);
        process.StartInfo = new ProcessStartInfo (m_stampingProcess, "\""+ m_tempISOFileFullPath + "\"");
        process.StartInfo.WorkingDirectory = tempWorkingDir;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = false;
        process.StartInfo.RedirectStandardOutput = false;

        log.ErrorFormat ("RunStampingProcess: info: starting {0} {1}", m_stampingProcess, m_tempISOFileFullPath);
        log.ErrorFormat ("RunStampingProcess: info: WorkingDirectory {0}", tempWorkingDir);
        process.Start ();
        while (!process.HasExited) {
          Thread.Sleep (100);
        }
        if (process.ExitCode !=0) {
          log.ErrorFormat ("RunStampingProcess: info: exitCode={0}", process.ExitCode);
          // move error file to ISO folder
          string mceErrorFile = string.Concat (m_tempISOFileFullPath, LEM_STAMP_ERROR_EXTENSION);
          if (File.Exists (mceErrorFile)) {
            string errorFile = string.Concat (m_ISOFileFullPath, m_errorFileExtension);
            log.ErrorFormat ("RunStampingProcess: stamping errors in {0}", errorFile);
            CopyFile (mceErrorFile, errorFile);
            RemoveFile (mceErrorFile);
            RemoveFile (m_tempISOFileFullPath);
          }
        }
        return process.ExitCode;
      }
      catch( Exception e) {
        log.ErrorFormat ("RunStampingProcess: exception {0}", e);
        return -1;
      }
    }

    /// <summary>
    /// run stamping process in temporary folder
    /// WORKS ONLY IF THERE IS A USER CURRENTLY CONNECTED !!!
    /// </summary>
    int RunStampingProcessAsCurrentUser ()
    {
      try {
        string commandLine = m_stampingProcess + " \"" + m_tempISOFileFullPath + "\"";
        log.ErrorFormat ("RunStampingProcessAsCurrentUser: info: starting {0}", commandLine);
        ImpersonationUtils.LaunchAsCurrentUser (commandLine);
        log.ErrorFormat ("RunStampingProcessAsCurrentUser: info: start end {0}", commandLine);

        // wait process end
        bool processIsRunning = true;
        while (processIsRunning) {
          var process = Process.GetProcessesByName ("Lem_Stamp").FirstOrDefault ();
          if(process != null) {
            Thread.Sleep (100);
          }
          else {
            processIsRunning = false;
          }
        }

        // move error file if any to ISO folder
        string mceErrorFile = string.Concat (m_tempISOFileFullPath, LEM_STAMP_ERROR_EXTENSION);
        if (File.Exists (mceErrorFile)) {
          string errorFile = string.Concat (m_ISOFileFullPath, m_errorFileExtension);
          log.ErrorFormat ("RunStampingProcessAsCurrentUser: stamping errors in {0}", errorFile);
          CopyFile (mceErrorFile, errorFile);
          RemoveFile (mceErrorFile);
        }
        log.ErrorFormat ("RunStampingProcessAsCurrentUser: info: stamp process ended {0}", commandLine);

        return 0;
      }
      catch (Exception e) {
        log.ErrorFormat ("RunStampingProcessAsCurrentUser: exception {0}", e);
        return -1;
      }
    }

    /// <summary>
    /// Get PPR From ISO file
    /// tag line format: ...[PPR] = value...
    /// </summary>
    private string GetPPRFromISoFile (string programFile)
    {
      int MAX_LINES = 100;
      int lineCount = 1;
      string programPPR = "";

      bool tagFound = false;
      log.ErrorFormat ("GetPPRFromISoFile: info: file={0}", programFile);

      try {
        using (StreamReader reader = new StreamReader (programFile)) {
          while (!tagFound && !reader.EndOfStream && lineCount < MAX_LINES) {
            string line = reader.ReadLine ();
            if (line.IndexOf (PPR_TAG_KEYWORD, 0) != -1) {
              log.ErrorFormat ("GetPPRFromISoFile: info: line found: {0}", line);
              var match = PPR_TAG_REGEX.Match (line);
              if (!match.Success) {
                log.ErrorFormat ("GetPPRFromISoFile: PPR Tag line found with bad format, continue:{0}", line);
              }
              else {
                if (match.Groups["destination"].Success) {
                  programPPR = match.Groups["destination"].Value.Trim ();
                  tagFound = true;
                }
                else {
                  log.ErrorFormat ("GetPPRFromISoFile: PPR Tag line found with bad format, continue:{0}", line);
                }
              }
            }
            lineCount++;
          }
        }
        if (!tagFound) {
          log.ErrorFormat ("GetPPRFromISoFile: no PPR tag line found in: {0}", programFile);
        }
      }
      catch (Exception e) {
        log.ErrorFormat ("GetPPRFromISoFile: failed to read file: {0}, {1}", programFile, e);
      }
      log.ErrorFormat ("GetPPRFromISoFile: info: PPR tag: {0}", programPPR);
      return programPPR;
    }

    /// <summary>
    /// Get destination folder from PPR in option file
    /// If PPR contains whitespaces, replace by underscore in config file key
    /// </summary>
    private string GetDestinationFromPPR (string programPPR)
    {
      log.ErrorFormat ("GetDestinationFromPPR: info: programPPR={0}", programPPR);
      string programPPRKey = programPPR.Replace (' ', '_');
      string destination = Lemoine.Info.ConfigSet.LoadAndGet<string> (string.Concat(PPR_TAG_PREFIX_KEY, programPPRKey), "");
      log.ErrorFormat ("GetDestinationFromPPR: info: destination={0}", destination);
      return destination.Trim();
    }
    #endregion Methods  
  }
}
