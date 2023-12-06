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
using System.Runtime.InteropServices;

namespace Pomamo.Stamping.FileDetection
{
  /// <summary>
  /// Stamp a file that contains a [PPR] line
  /// </summary>
  public class PprFileStamper : ThreadClass, IThreadClass
  {
    static readonly string STAMPER_PROCESS_NAME_KEY = "Stamping.PprFileStamper.StamperProcess";
    static readonly string STAMPER_PROCESS_NAME_DEFAULT = "Lem_Stamper.Console.exe"; // relative to the program directory, or $@"C:\Program Files\{PulseInfo.ProductFolderName}\Lem_Stamper.Console.exe";

    /// <summary>
    /// Additional options to set to Lem_Stamper.Console.exe
    /// </summary>
    static readonly string STAMPER_OPTIONS_KEY = "Stamping.PprFileStamper.StamperOptions";
    static readonly string STAMPER_OPTIONS_DEFAULT = "";

    static readonly string COPY_BEFORE_KEY = "Stamping.PprFileStamper.CopyBefore";
    static readonly bool COPY_BEFORE_DEFAULT = true;

    /// <summary>
    /// Default output directory
    /// </summary>
    static readonly string DEFAULT_OUT_DIRECTORY_KEY = "Stamping.PprFileStamper.OutDirectory";
    static readonly string DEFAULT_OUT_DIRECTORY_DEFAULT = ""; // Empty means in place

    /// <summary>
    /// Source directory to consider to keep the sub-directory structure
    /// </summary>
    static readonly string SOURCE_DIRECTORY_KEY = "Stamping.PprFileStamper.SourceDirectory";
    static readonly string SOURCE_DIRECTORY_DEFAULT = ""; // Empty means: do not consider it

    /// <summary>
    /// Remove any existing destination file
    /// </summary>
    static readonly string REMOVE_EXISTING_DESTINATION_FILE_KEY = "Stamping.PprFileStamper.RemoveExistingDestinationFile";
    static readonly bool REMOVE_EXISTING_DESTINATION_FILE_DEFAULT = true;

    static readonly string PPR_TAG_PREFIX_KEY = "StampFileWatch.PPR.";

    /// <summary>
    /// Max lines to check to find the PPR
    /// </summary>
    static readonly string MAX_LINES_PPR_KEY = "Stamping.PprFileStamper.MaxLinesPpr";
    static readonly int MAX_LINES_PPR_DEFAULT = 100;

    static readonly ILog log = LogManager.GetLogger (typeof (PprFileStamper).FullName);

    readonly string m_isoFileFullPath;
    readonly string m_tempFolder;

    static readonly string ISO_FILE_PROGRESS_EXTENSION = ".stamp_in_progress";

    static readonly string TEMP_PATH_KEY = "Stamping.PprFileStamper.TempPath";
    static readonly string TEMP_PATH_DEFAULT = Path.GetTempPath ();

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
      var tempPath = Lemoine.Info.ConfigSet.LoadAndGet (TEMP_PATH_KEY, TEMP_PATH_DEFAULT);
      m_tempFolder = Path.Combine (tempPath, "PprFileStamper");

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
        finalDestinationFullPath = GetFinalDestinationFullPath (isoFileName, programPpr);

        if (string.IsNullOrEmpty (programPpr)) { // Empty if not found
          log.Error ($"ProcessIsoFile: PPR not found in {inputFile} => abort");
          throw new Exception ("No PPR found");
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
        if (File.Exists (finalDestinationFullPath)
          && Lemoine.Info.ConfigSet.LoadAndGet (REMOVE_EXISTING_DESTINATION_FILE_KEY, REMOVE_EXISTING_DESTINATION_FILE_DEFAULT)) {
          log.Warn ($"ProcessIsoFile: a file exists in the final destination path {finalDestinationFullPath}, remove it");
          File.Delete (finalDestinationFullPath);
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
            log.Error ($"ProcessIsoFile: {renamedIsoFileFullPath} still exists, which is unexpected (it may be because a file already exists in the destination directory): add the .error suffix");
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
          stampingProcess = Path.Combine (ProgramInfo.AbsoluteDirectory, stampingProcess);
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
        var arguments = $"-i \"{inputFile}\" -o \"{outputFolder}\" -n \"{programPpr}\"";
        var additionalOptions = Lemoine.Info.ConfigSet.LoadAndGet (STAMPER_OPTIONS_KEY, STAMPER_OPTIONS_DEFAULT);
        if (!string.IsNullOrEmpty (additionalOptions)) {
          arguments += " " + additionalOptions;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"RunStampingProcess: arguments are {arguments}");
        }
        process.StartInfo.Arguments = arguments;

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
      int maxLines = Lemoine.Info.ConfigSet.LoadAndGet (MAX_LINES_PPR_KEY, MAX_LINES_PPR_DEFAULT);
      int lineCount = 1;

      log.Info ($"GetPprFromIsoFile: info: file={programFile}");

      try {
        using (StreamReader reader = new StreamReader (programFile)) {
          while (!reader.EndOfStream && lineCount < maxLines) {
            var line = reader.ReadLine ();
            if (line is null) {
              log.Fatal ($"GetPprFromIsoFile: unexpected end of stream reached");
            }
            else if (line.Contains (PPR_TAG_KEYWORD)) {
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
        log.Error ($"GetPprFromIsoFile: failed to read file: {programFile}", ex);
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
      var programPPRKey = programPpr.Replace (' ', '_');
      var defaultOutDirectory = Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_OUT_DIRECTORY_KEY, DEFAULT_OUT_DIRECTORY_DEFAULT);
      var destination = Lemoine.Info.ConfigSet.LoadAndGet (PPR_TAG_PREFIX_KEY + programPPRKey, defaultOutDirectory);
      log.Info ($"GetDestinationFromPPR: info: destination={destination}");
      return destination.Trim ();
    }

    string GetFinalDestinationFolder (string programPpr)
    {
      string initialDestinationFolder;
      if (string.IsNullOrEmpty (programPpr)) {
        initialDestinationFolder = Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_OUT_DIRECTORY_KEY, DEFAULT_OUT_DIRECTORY_DEFAULT).Trim ();
      }
      else {
        initialDestinationFolder = GetDestinationFromPPR (programPpr);
      }
      var sourceDirectory = Lemoine.Info.ConfigSet.LoadAndGet (SOURCE_DIRECTORY_KEY, SOURCE_DIRECTORY_DEFAULT);
      if (string.IsNullOrEmpty (sourceDirectory)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetFinalDestinationFolder: no source directory, return {initialDestinationFolder}");
        }
        return initialDestinationFolder;
      }
      else { // Check sub-directories
        var sourceDirectoryNames = sourceDirectory.Split (Path.DirectorySeparatorChar);
        var fullPathDirectoryNames = m_isoFileFullPath.Split (Path.DirectorySeparatorChar);
        if (fullPathDirectoryNames.Length <= sourceDirectoryNames.Length) {
          log.Error ($"GetFinalDestinationFolder: invalid source directory {sourceDirectory} for path {fullPathDirectoryNames}, omit it and return {initialDestinationFolder}");
          return initialDestinationFolder;
        }
        var caseSensitivePath = !(RuntimeInformation.IsOSPlatform (OSPlatform.Windows) || RuntimeInformation.IsOSPlatform (OSPlatform.OSX));
        var finalDestinationFolder = initialDestinationFolder;
        for (int i = 0; i < fullPathDirectoryNames.Length - 1; ++i) {
          if (sourceDirectoryNames.Length <= i) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetFinalDestinationFolder: sub-directory {fullPathDirectoryNames[i]} detected, add it");
            }
            finalDestinationFolder = Path.Combine (finalDestinationFolder, fullPathDirectoryNames[i]);
          }
          else if (string.Equals (fullPathDirectoryNames[i], sourceDirectoryNames[i], caseSensitivePath ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetFinalDestinationFolder: common directory {fullPathDirectoryNames[i]} detected");
            }
          }
          else {
            log.Error ($"GetFinalDestinationFolder: sub-directory mismatch between {sourceDirectory} and {m_isoFileFullPath} => return {initialDestinationFolder}");
            return initialDestinationFolder;
          }
        }
        return finalDestinationFolder;
      }
    }

    string GetFinalDestinationFullPath (string isoFileName, string programPpr)
    {
      var finalDestinationFolder = GetFinalDestinationFolder (programPpr);
      if (!string.IsNullOrEmpty (finalDestinationFolder)) {
        if (log.IsInfoEnabled) {
          log.Info ($"GetFinalDestinationFullPath: finalDestinationFolder={finalDestinationFolder} from programPPR={programPpr}");
        }
        try {
          if (!Directory.Exists (finalDestinationFolder)) {
            Directory.CreateDirectory (finalDestinationFolder);
          }
          return Path.Combine (finalDestinationFolder, isoFileName);
        }
        catch (Exception ex) {
          log.Error ($"GetFinalDestinationFullPath: error while creating destination folder {finalDestinationFolder} => use the original destination path {m_isoFileFullPath}", ex);
          return m_isoFileFullPath;
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetFinalDestinationFullPath: no final destination recorded for programPpr={programPpr} => use {m_isoFileFullPath}");
        }
        return m_isoFileFullPath;
      }
    }
  }
}
