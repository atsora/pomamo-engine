// Copyright (C) 2009-2023 Lemoine Automation Technologies 2023 Nicolas Relange
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Info;

namespace Pomamo.Stamping.FileDetection
{
  /// <summary>
  /// Stamp a file that contains a [PPR] line
  /// </summary>
  public class PprFileStamper : ThreadClass, IThreadClass
  {
    static readonly string STAMPER_PROCESS_NAME_KEY = "Stamping.FileWatch.StamperProcess";
    static readonly string STAMPER_PROCESS_NAME_DEFAULT = "Lem_Stamper.Console.exe"; // relative to the program directory, or $@"C:\Program Files\{PulseInfo.ProductFolderName}\Lem_Stamper.Console.exe";

    static readonly string COPY_BEFORE_KEY = "Stamping.PprFileStamper.CopyBefore";
    static readonly bool COPY_BEFORE_DEFAULT = true;

    static readonly string PPR_TAG_PREFIX_KEY = "StampFileWatch.PPR.";

    static readonly ILog log = LogManager.GetLogger (typeof (PprFileStamper).FullName);

    readonly string m_isoFileFullPath;
    readonly string m_tempFolder;

    static readonly string ISO_FILE_PROGRESS_EXTENSION = ".stamp_in_progress";

    static readonly string TEMP_STAMPED_FOLDER_NAME = "stamped";

    static readonly string PPR_TAG_KEYWORD = "[PPR]";
    static readonly Regex PPR_TAG_REGEX = new Regex ("\\[PPR\\] *\\= *(?<destination>[-_a-zA-Z0-9]+( *[-_a-zA-Z0-9]*)*)", RegexOptions.Compiled);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="isoFileFullPath"></param>
    public PprFileStamper (string isoFileFullPath)
    {
      m_isoFileFullPath = isoFileFullPath;
      m_tempFolder = Path.Combine (Path.GetTempPath (), "StampFileWatch");
      
      if (log.IsDebugEnabled) {
        log.Debug ($"PprFileStamper: path={m_isoFileFullPath}, temp folder={m_tempFolder}");
      }
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
      try {
        // test temp folder exists and create if not
        if (!Directory.Exists (m_tempFolder)) {
          Directory.CreateDirectory (m_tempFolder);
        }

        ProcessIsoFile ();
      }
      catch (Exception ex) {
        log.Error ("Run: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// process ISO file
    /// </summary>
    public void ProcessIsoFile ()
    {
      var copyBefore = Lemoine.Info.ConfigSet
        .LoadAndGet (COPY_BEFORE_KEY, COPY_BEFORE_DEFAULT);
      var renamedIsoFileFullPath = string.Concat (m_isoFileFullPath, ISO_FILE_PROGRESS_EXTENSION);
      var isoFileName = Path.GetFileName (m_isoFileFullPath);
      var inputFile = copyBefore
        ? Path.Combine (m_tempFolder, isoFileName)
        : renamedIsoFileFullPath;
      var tempStampedFolder = Path.Combine (m_tempFolder, TEMP_STAMPED_FOLDER_NAME);
      var inputFileName = Path.GetFileName (inputFile);
      var tempStampedFilePath = Path.Combine (tempStampedFolder, inputFileName);
      string? finalDestinationFullPath = null;

      try {
        if (!Directory.Exists (tempStampedFolder)) {
          Directory.CreateDirectory (tempStampedFolder);
        }

        // rename file during stamping process
        File.Move (m_isoFileFullPath, renamedIsoFileFullPath);

        if (copyBefore) {
          try {
            File.Copy (renamedIsoFileFullPath, inputFile);
          }
          catch (Exception ex) {
            log.Error ($"ProcessIsoFile: exception while copying {renamedIsoFileFullPath} into {inputFile}", ex);
            File.Move (renamedIsoFileFullPath, m_isoFileFullPath);
            throw;
          }
        }

        // get PPR from ISO file
        var programPpr = GetPprFromIsoFile (inputFile);
        if (string.IsNullOrEmpty (programPpr)) { // Empty if not found
          log.Error ($"ProcessIsoFile: PPR not found in {inputFile} => abort");
          File.Move (renamedIsoFileFullPath, m_isoFileFullPath);
          if (copyBefore) {
            File.Delete (inputFile);
          }
          return;
        }

        // get destination from PPR. Default to orginal path
        var finalDestinationFolder = GetDestinationFromPPR (programPpr);
        if (!string.IsNullOrEmpty (finalDestinationFolder)) {
          if (log.IsInfoEnabled) {
            log.Info ($"ProcessIsoFile: finalDestinationFolder={finalDestinationFolder} from programPPR={programPpr}");
          }
          try {
            if (!Directory.Exists (finalDestinationFolder)) {
              Directory.CreateDirectory (finalDestinationFolder);
            }
            finalDestinationFullPath = Path.Combine (finalDestinationFolder, isoFileName);
          }
          catch (Exception ex) {
            log.Error ($"ProcessIsoFile: error while creating destination folder {finalDestinationFolder} => use the original destination path {m_isoFileFullPath}", ex);
            finalDestinationFullPath = m_isoFileFullPath;
          }
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessIsoFile: no final destination recorded for programPpr={programPpr} => use {m_isoFileFullPath}");
          }
          finalDestinationFullPath = m_isoFileFullPath;
        }

        int stampingResult;
        try {
          stampingResult = RunStampingProcess (inputFile, tempStampedFolder, programPpr);
        }
        catch (Exception ex) {
          log.Error ($"ProcessIsoFile: RunStampingProcess failed with exception", ex);
          stampingResult = -1;
        }
        if (stampingResult != 0) {
          log.Error ($"ProcessIsoFile: return code was {stampingResult} => restore {renamedIsoFileFullPath} into {m_isoFileFullPath}");
          File.Move (renamedIsoFileFullPath, m_isoFileFullPath);
          if (copyBefore) {
            File.Delete (inputFile);
          }
          if (File.Exists (tempStampedFilePath)) {
            File.Delete (tempStampedFilePath);
          }
          return;
        }

        // copy stamped file to destination
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessIsoFile: move final file {tempStampedFilePath} into {finalDestinationFullPath}");
        }
        File.Move (tempStampedFilePath, finalDestinationFullPath);
        if (File.Exists (renamedIsoFileFullPath)) {
          log.Info ($"ProcessIsoFile: remove renamed file {renamedIsoFileFullPath}");
          File.Delete (renamedIsoFileFullPath);
        }
      }
      catch (Exception ex) {
        log.Error ("ProcessIsoFile: exception", ex);
        try {
          if (finalDestinationFullPath is not null) {
            if (!File.Exists (finalDestinationFullPath) && File.Exists (renamedIsoFileFullPath)) {
              log.Info ($"ProcessIsoFile: fallback: move {renamedIsoFileFullPath} into {finalDestinationFullPath}");
              File.Move (renamedIsoFileFullPath, finalDestinationFullPath);
            }
          }
          else {
            if (File.Exists (renamedIsoFileFullPath) && !File.Exists (m_isoFileFullPath)) {
              File.Move (renamedIsoFileFullPath, m_isoFileFullPath);
            }
          }
        }
        catch (Exception ex2) {
          log.Error ($"ProcessIsoFile: exception in fallback", ex2);
          throw;
        }
      }
      finally {
        try {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessIsoFile: remove temporary files {inputFile} and {tempStampedFilePath}");
          }
          if (File.Exists (renamedIsoFileFullPath)) {
            log.Error ($"ProcessIsoFile: {renamedIsoFileFullPath} still exists, which is unexpected");
            File.Move (renamedIsoFileFullPath, m_isoFileFullPath + ".error");
          }
          if (copyBefore) {
            File.Delete (inputFile);
          }
          if (File.Exists (tempStampedFilePath)) {
            File.Delete (tempStampedFilePath);
          }
        }
        catch (Exception ex3) {
          log.Error ($"ProcessIsoFile: exception while cleaning up the files", ex3);
        }
      }
    }

    /// <summary>
    /// run stamping process in temporary folder
    /// DO NOT WORK INSIDE WINDOWS SERVICE IF NETWORK DRIVES ARE USED !!!
    /// </summary>
    int RunStampingProcess (string inputFile, string outputFolder, string programPpr)
    {
      try {
        if (!Directory.Exists (outputFolder)) {
          Directory.CreateDirectory (outputFolder);
        }

        var stampingProcess = Lemoine.Info.ConfigSet.LoadAndGet<string> (STAMPER_PROCESS_NAME_KEY, STAMPER_PROCESS_NAME_DEFAULT);
        if (!Path.IsPathRooted (stampingProcess)) {
          stampingProcess = Path.Combine (ProgramInfo.AbsolutePath, stampingProcess);
        }
        if (log.IsInfoEnabled) {
          log.Info ($"RunStampingProcess: stamping process is {stampingProcess}");
        }
        var process = new Process {
          StartInfo = new ProcessStartInfo (stampingProcess)
        };
        process.StartInfo.WorkingDirectory = Path.GetDirectoryName (stampingProcess);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = false;
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.Arguments = $"-i \"{inputFile}\" -o \"{outputFolder}\" -n \"{programPpr}\"";

        log.Info ($"RunStampingProcess: starting {stampingProcess} {process.StartInfo.Arguments} in {process.StartInfo.WorkingDirectory}");
        process.Start ();
        while (!process.HasExited) {
          Thread.Sleep (100);
        }
        if (process.ExitCode != 0) {
          log.Error ($"RunStampingProcess: exitCode={process.ExitCode}");
        }
        return process.ExitCode;
      }
      catch (Exception ex) {
        log.Error ($"RunStampingProcess: exception {ex}");
        throw;
      }
    }

    /// <summary>
    /// Get PPR From ISO file
    /// tag line format: ...[PPR] = value...
    /// 
    /// Return an empty string if the PPR tag was not found in the first lines
    /// </summary>
    private string GetPprFromIsoFile (string programFile)
    {
      int MAX_LINES = 100;
      int lineCount = 1;

      log.Info ($"GetPprFromIsoFile: info: file={programFile}");

      try {
        using (StreamReader reader = new StreamReader (programFile)) {
          while (!reader.EndOfStream && lineCount < MAX_LINES) {
            string line = reader.ReadLine ();
            if (line.Contains (PPR_TAG_KEYWORD)) {
              log.Info ($"GetPprFromIsoFile: info: line found: {line}");
              var match = PPR_TAG_REGEX.Match (line);
              if (!match.Success) {
                log.Error ($"GetPprFromIsoFile: PPR Tag line found with bad format, continue:{line}");
              }
              else {
                if (match.Groups["destination"].Success) {
                  var ppr = match.Groups["destination"].Value.Trim ().Replace (' ', '_');
                  log.Info ($"GetPprFromIsoFile: found {ppr} for {programFile}");
                  return ppr;
                }
                else {
                  log.Error ($"GetPprFromIsoFile: PPR Tag line found with bad format, continue:{line}");
                }
              }
            }
            lineCount++;
          }
        }
        log.Error ($"GetPprFromIsoFile: no PPR tag line found in: {programFile}");
        return "";
      }
      catch (Exception ex) {
        log.Error ($"GetPprFromIsoFile: failed to read file: {programFile}, {ex}");
        throw;
      }
    }

    /// <summary>
    /// Get destination folder from PPR in option file
    /// If PPR contains whitespaces, replace by underscore in config file key
    /// </summary>
    private string GetDestinationFromPPR (string programPpr)
    {
      log.Info ($"GetDestinationFromPPR: info: programPpr={programPpr}");
      string programPPRKey = programPpr.Replace (' ', '_');
      string destination = Lemoine.Info.ConfigSet.LoadAndGet<string> (PPR_TAG_PREFIX_KEY + programPPRKey, "");
      log.Info ($"GetDestinationFromPPR: info: destination={destination}");
      return destination.Trim ();
    }
  }
}
