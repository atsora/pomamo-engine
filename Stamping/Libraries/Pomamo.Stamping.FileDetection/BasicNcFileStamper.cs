// Copyright (C) 2024 Atsora Solutions
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
  /// Stamp a file after specifying the config name
  /// </summary>
  public static class BasicNcFileStamper
  {
    static readonly string STAMPER_PROCESS_NAME_KEY = "Stamping.BasicNcFileStamper.StamperProcess";
    static readonly string STAMPER_PROCESS_NAME_DEFAULT = "Lem_Stamper.Console.exe"; // relative to the program directory, or $@"C:\Program Files\{PulseInfo.ProductFolderName}\Lem_Stamper.Console.exe";

    static readonly string USE_CURRENT_USER_KEY = "BasicNcFileStamper.UseCurrentUser";
    static readonly bool USE_CURRENT_USER_DEFAULT = false;

    /// <summary>
    /// Additional options to set to Lem_Stamper.Console.exe
    /// </summary>
    static readonly string STAMPER_OPTIONS_KEY = "Stamping.BasicNcFileStamper.StamperOptions";
    static readonly string STAMPER_OPTIONS_DEFAULT = "";

    static readonly ILog log = LogManager.GetLogger (typeof (PprFileStamper).FullName);

    /// <summary>
    /// Run the stamping process
    /// </summary>
    /// <returns>exit code</returns>
    public static int RunStampingProcessFromConfigName (string inputFile, string outputFolder, string configName, string machineName = "", TimeSpan? operationMachiningDuration = null, int operationId = 0, CancellationToken cancellationToken = default)
    {
      try {
        if (!string.IsNullOrWhiteSpace (outputFolder) && !Directory.Exists (outputFolder)) {
          Directory.CreateDirectory (outputFolder);
        }
        cancellationToken.ThrowIfCancellationRequested ();

        var useCurrentUser = Lemoine.Info.ConfigSet
          .LoadAndGet<bool> (USE_CURRENT_USER_KEY, USE_CURRENT_USER_DEFAULT);

        // TODO: useCurrentUser:  await Lemoine.Core.Security.Identity.RunImpersonatedAsExplorerUserAsync (async () => await ProcessIndexFileAsync (indexFilePath));

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
        var arguments = $"-i \"{inputFile}\" -n \"{configName}\"";
        if (!string.IsNullOrEmpty (machineName)) {
          arguments += $" -s MachineName={machineName}";
        }
        if (!string.IsNullOrWhiteSpace (outputFolder)) {
          arguments += $" -o \"{outputFolder}\"";
        }
        if (operationMachiningDuration is not null) {
          arguments += $" -s OperationMachiningDuration={operationMachiningDuration}";
        }
        if (0 < operationId) {
          arguments += $" -s OperationId={operationId}";
        }
        // TODO: component name
        var additionalOptions = Lemoine.Info.ConfigSet.LoadAndGet (STAMPER_OPTIONS_KEY, STAMPER_OPTIONS_DEFAULT);
        if (!string.IsNullOrEmpty (additionalOptions)) {
          arguments += " " + additionalOptions;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"RunStampingProcess: arguments are {arguments}");
        }
        process.StartInfo.Arguments = arguments;
        cancellationToken.ThrowIfCancellationRequested ();

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

  }
}
