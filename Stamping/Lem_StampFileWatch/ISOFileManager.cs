// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.Lem_StampFileWatch
{
  /// <summary>
  /// Class to manage ISO file stamping
  /// </summary>
  public class ISOFileManager : ThreadClass, IThreadClass
  {
    static readonly string LEM_STAMP_PROCESS_NAME_KEY = "Stamping.FileWatch.StamperProcess";
    static readonly string LEM_STAMP_PROCESS_NAME_DEFAULT = "C:\\Program Files (x86)\\PULSE\\core\\Lem_Stamper.Console.exe";

    static readonly string PPR_TAG_PREFIX_KEY = "StampFileWatch.PPR.";

    static readonly ILog log = LogManager.GetLogger (typeof (ISOFileManager).FullName);

    readonly string m_isoFileFullPath = null;
    readonly string m_isoFileName = null;
    string m_renamedISOFileFullPath = null;
    string m_tempISOFileFullPath = null;
    readonly string m_tempFolder = null;
    readonly string m_tempStampedFolder = null;
    readonly string m_tempStampedISOFileFullPathFolder = null;
    readonly string m_stampingProcess = null;
    readonly string m_workingFolder = null;
    string m_programPPR = null;

    static readonly string ISO_FILE_PROGRESS_EXTENSION = ".stamp_in_progress";

    static readonly string TEMP_STAMPED_FOLDER_NAME = "stamped";

    static readonly string PPR_TAG_KEYWORD = "[PPR]";
    static readonly Regex PPR_TAG_REGEX = new Regex ("\\[PPR\\] *\\= *(?<destination>[-_a-zA-Z0-9]+( *[-_a-zA-Z0-9]*)*)", RegexOptions.Compiled);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="isoFileFullPath"></param>
    public ISOFileManager (string isoFileFullPath)
    {
      m_isoFileFullPath = isoFileFullPath;
      m_isoFileName = Path.GetFileName (isoFileFullPath);

      m_stampingProcess = Lemoine.Info.ConfigSet.LoadAndGet<string> (LEM_STAMP_PROCESS_NAME_KEY, LEM_STAMP_PROCESS_NAME_DEFAULT);
      m_workingFolder = Path.GetDirectoryName (m_stampingProcess);
      m_tempFolder = Path.Combine (Path.GetTempPath (), "StampFileWatch");
      log.Debug ($"ISOFileManager: temp folder={m_tempFolder}");
      m_tempStampedFolder = Path.Combine (m_tempFolder, TEMP_STAMPED_FOLDER_NAME);
      m_tempStampedISOFileFullPathFolder = Path.Combine (m_tempStampedFolder, m_isoFileName);
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
    /// process ISO file
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      m_renamedISOFileFullPath = string.Concat (m_isoFileFullPath, ISO_FILE_PROGRESS_EXTENSION);
      m_tempISOFileFullPath = Path.Combine (m_tempFolder, m_isoFileName);

      // test temp folder exists and create if not
      if (!Directory.Exists (m_tempFolder)) {
        if (!CreateFolder (m_tempFolder)) {
          return;
        }
      }

      ProcessIsoFile ();

      return;
    }

    /// <summary>
    /// process ISO file
    /// </summary>
    public void ProcessIsoFile ()
    {
      try {
        string finalDestinationFullPath = null;
        string finalDestinationFolder = null;
        // rename file during stamping process
        if (!RenameFile (m_isoFileFullPath, m_renamedISOFileFullPath)) {
          return;
        }
        // copy file to temporary folder for stamping
        if (!CopyFile (m_renamedISOFileFullPath, m_tempISOFileFullPath)) {
          // restore file
          RenameFile (m_renamedISOFileFullPath, m_isoFileFullPath);
          return;
        }
        // get PPR from ISO file
        m_programPPR = GetPPRFromISoFile (m_tempISOFileFullPath);

        // get destination from PPR. Default to orginal path

        // get stamping configuration for the PPR
        // default destination is 
        finalDestinationFullPath = m_isoFileFullPath;
        if (!string.IsNullOrEmpty (m_programPPR)) {
          finalDestinationFolder = GetDestinationFromPPR (m_programPPR);
          if (!string.IsNullOrEmpty (finalDestinationFolder)) {
            log.Error ($"ProcessIsoFile: programPPR {m_programPPR} not configured in options file. Keep original folder");
            finalDestinationFullPath = Path.Combine (finalDestinationFolder, m_isoFileName);
          }
        }

        log.Info ($"ProcessIsoFile: read PPR from config: info: PPR={m_programPPR} destination={finalDestinationFullPath}");

        // call stamp Process
        int stampingResult = -1;

        stampingResult = RunStampingProcess ();

        if (stampingResult != 0) {
          // restore file
          RenameFile (m_renamedISOFileFullPath, m_isoFileFullPath);
          return;
        }
        // test destination folder exists and create if not
        if (!string.IsNullOrEmpty (finalDestinationFolder)) {
          if (!Directory.Exists (finalDestinationFolder)) {
            if (!CreateFolder (finalDestinationFolder)) {
              log.Warn ($"ProcessIsoFile: unable to create destination folder {finalDestinationFolder}, use original program one");
              finalDestinationFullPath = m_isoFileFullPath;
            }
          }
        }
        // copy stamped file to destination
        if (!CopyFile (m_tempStampedISOFileFullPathFolder, finalDestinationFullPath)) {
          // if failed copy to original location
          if (!CopyFile (m_tempStampedISOFileFullPathFolder, m_isoFileFullPath)) {
            // if failed restore original file
            RenameFile (m_renamedISOFileFullPath, m_isoFileFullPath);
          }
          return;
        }
        // remove temp file
        RemoveFile (m_tempStampedISOFileFullPathFolder);
        // remove renamed
        RemoveFile (m_renamedISOFileFullPath);
      }
      catch (Exception ex) {
        log.Error ("ProcessIsoFile: exception", ex);
      }
    }

    /// <summary>
    /// rename ISO file 
    /// </summary>
    bool RenameFile (string fromFile, string toFile)
    {
      try {
        if (log.IsInfoEnabled) {
          log.Info ($"RenameFile: rename file {fromFile} to {toFile}");
        }
        File.Move (fromFile, toFile);
        return true;
      }
      catch (Exception ex) {
        log.Error ($"RenameFile: unable to rename file {fromFile} to {toFile}", ex);
        return false;
      }
    }

    /// <summary>
    /// copy file to temporary folder
    /// </summary>
    bool CopyFile (string fromFile, string toFile)
    {
      try {
        if (log.IsInfoEnabled) {
          log.Info ($"CopyFile: copy file {fromFile} to {toFile}");
        }
        File.Copy (fromFile, toFile, true);
        return true;
      }
      catch (Exception ex) {
        log.Error ($"CopyFile: unable to copy file {fromFile} to {toFile}", ex);
        return false;
      }
    }

    /// <summary>
    /// remove  ISO file
    /// </summary>
    void RemoveFile (string file)
    {
      try {
        if (log.IsInfoEnabled) {
          log.Info ($"RemoveFile: delete file {file}");
        }
        File.Delete (file);
        return;
      }
      catch (Exception ex) {
        log.Error ($"RemoveFile: unable to delete file {file}", ex);
        return;
      }
    }

    /// <summary>
    /// create folder
    /// </summary>
    bool CreateFolder (string folder)
    {
      try {
        if (log.IsInfoEnabled) {
          log.Info ($"CreateFolder: create folder {folder}");
        }
        Directory.CreateDirectory (folder);
        return true;
      }
      catch (Exception ex) {
        log.Error ($"CreateFolder: unable to create folder {folder}", ex);
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
        string tempWorkingFolder = Path.Combine (m_tempFolder, "idx-temp");
        if (!Directory.Exists (tempWorkingFolder)) {
          if (!CreateFolder (tempWorkingFolder)) {
            return -1;
          }
        }

        // folder 
        if (!Directory.Exists (m_tempStampedFolder)) {
          if (!CreateFolder (m_tempStampedFolder)) {
            log.Error ($"RunStampingProcess: unable to create folder {m_tempStampedFolder}. Skip");
            return -1;
          }
        }

        Process process = new Process {
          StartInfo = new ProcessStartInfo (m_stampingProcess)
        };
        process.StartInfo.WorkingDirectory = m_workingFolder;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = false;
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.Arguments = $"-i \"{m_tempISOFileFullPath}\" -o \"{m_tempStampedFolder}\" -n \"{m_programPPR}\"";

        log.Info ($"RunStampingProcess: info: WorkingDirectory {process.StartInfo.WorkingDirectory}");
        log.Info ($"RunStampingProcess: info: starting {m_stampingProcess} {process.StartInfo.Arguments}");
        process.Start ();
        while (!process.HasExited) {
          Thread.Sleep (100);
        }
        if (process.ExitCode != 0) {
          log.Error ($"RunStampingProcess: info: exitCode={process.ExitCode}");
        }
        return process.ExitCode;
      }
      catch (Exception e) {
        log.Error ($"RunStampingProcess: exception {e}");
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
      log.Info ($"GetPPRFromISoFile: info: file={programFile}");

      try {
        using (StreamReader reader = new StreamReader (programFile)) {
          while (!tagFound && !reader.EndOfStream && lineCount < MAX_LINES) {
            string line = reader.ReadLine ();
            if (line.IndexOf (PPR_TAG_KEYWORD, 0) != -1) {
              log.Info ($"GetPPRFromISoFile: info: line found: {line}");
              var match = PPR_TAG_REGEX.Match (line);
              if (!match.Success) {
                log.Error ($"GetPPRFromISoFile: PPR Tag line found with bad format, continue:{line}");
              }
              else {
                if (match.Groups["destination"].Success) {
                  programPPR = match.Groups["destination"].Value.Trim ().Replace (' ', '_');
                  tagFound = true;
                }
                else {
                  log.Error ($"GetPPRFromISoFile: PPR Tag line found with bad format, continue:{line}");
                }
              }
            }
            lineCount++;
          }
        }
        if (!tagFound) {
          log.Error ($"GetPPRFromISoFile: no PPR tag line found in: {programFile}");
        }
      }
      catch (Exception e) {
        log.Error ($"GetPPRFromISoFile: failed to read file: {programFile}, {e}");
      }

      log.Info ($"GetPPRFromISoFile: info: PPR tag: {programPPR}");
      return programPPR;
    }

    /// <summary>
    /// Get destination folder from PPR in option file
    /// If PPR contains whitespaces, replace by underscore in config file key
    /// </summary>
    private string GetDestinationFromPPR (string programPPR)
    {
      log.Info ($"GetDestinationFromPPR: info: programPPR={programPPR}");
      string programPPRKey = programPPR.Replace (' ', '_');
      string destination = Lemoine.Info.ConfigSet.LoadAndGet<string> (PPR_TAG_PREFIX_KEY + programPPRKey, "");
      log.Info ($"GetDestinationFromPPR: info: destination={destination}");
      return destination.Trim ();
    }
  }
}
