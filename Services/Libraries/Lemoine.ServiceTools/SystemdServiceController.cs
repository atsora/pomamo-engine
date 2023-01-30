// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.ServiceTools
{
  /// <summary>
  /// Result of a process
  /// </summary>
  public struct ProcessResult
  {
    public int ExitCode;
    public string StandardOutput;
    public string StandardError;

    public ProcessResult (int exitCode, string standardOutput, string standardError)
    {
      this.ExitCode = exitCode;
      this.StandardOutput = standardOutput;
      this.StandardError = standardError;
    }
  }

  /// <summary>
  /// SystemdServiceController
  /// </summary>
  [SupportedOSPlatform ("linux")]
  public class SystemdServiceController : IServiceController
  {
    readonly ILog log = LogManager.GetLogger (typeof (SystemdServiceController).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (SystemdServiceController).FullName);

    readonly string m_serviceName;

    /// <summary>
    /// Constructor
    /// </summary>
    public SystemdServiceController (string serviceName)
    {
      m_serviceName = serviceName;
    }

    /// <summary>
    /// <see cref="IServiceController"/>
    /// </summary>
    public string ServiceName => m_serviceName;

    /// <summary>
    /// <see cref="IServiceController"/>
    /// </summary>
    public bool IsInstalled
    {
      get {
        try {
          var loadState = GetServiceProperty ("LoadState");
          if (log.IsDebugEnabled) {
            log.Debug ($"IsInstalled: LoadState is {loadState} for {m_serviceName}");
          }
          return loadState.Trim ().Equals ("loaded"); // Else not-found
        }
        catch (Exception ex) {
          log.Error ($"IsInstalled: exception for {m_serviceName}", ex);
          throw;
        }
      }
    }

    /// <summary>
    /// <see cref="IServiceController"/>
    /// </summary>
    public bool Running
    {
      get {
        try {
          var activeStatus = GetServiceProperty ("ActiveState");
          if (log.IsDebugEnabled) {
            log.Debug ($"Running: ActiveState is {activeStatus} for {m_serviceName}");
          }
          return activeStatus.Trim ().Equals ("active");
        }
        catch (Exception ex) {
          log.Error ($"Running: exception for {m_serviceName}", ex);
          throw;
        }
      }
    }

    string SystemctlCommand => "/bin/systemctl";

    /// <summary>
    /// <see cref="IServiceController"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartServiceAsync (CancellationToken cancellationToken = default)
    {
      try {
        var arguments = $"start {m_serviceName}.service";
        var processResult = await RunCommandAsync (this.SystemctlCommand, arguments, cancellationToken: cancellationToken);
        if (0 != processResult.ExitCode) {
          log.Error ($"StartServiceAsync: systemctl {arguments} did not end successfully, exitCode={processResult.ExitCode} for {m_serviceName}");
          throw new Exception ($"systemctl {arguments} failed");
        }
      }
      catch (Exception ex) {
        log.Error ($"StartServiceAsync: exception for {m_serviceName}", ex);
        throw;
      }
    }

    string GetServiceProperty (string property, CancellationToken cancellationToken = default)
    {
      return Task.Run (() => GetServicePropertyAsync (property, cancellationToken), cancellationToken).Result;
    }

    async Task<string> GetServicePropertyAsync (string property, CancellationToken cancellationToken = default)
    {
      try {
        var arguments = $"show {m_serviceName}.service --property={property}";
        var processResult = await RunCommandAsync (this.SystemctlCommand, arguments, cancellationToken: cancellationToken);
        if (0 != processResult.ExitCode) {
          log.Error ($"GetServiceProperty: systemctl {arguments} did not end successfully, exitCode={processResult.ExitCode}");
          throw new Exception ($"systemctl {arguments} failed");
        }
        var result = processResult.StandardOutput;
        if (log.IsDebugEnabled) {
          log.Debug ($"GetServiceProperty: systemctl {arguments} returned {result}");
        }
        var keyValue = result.Split ('=');
        return keyValue[1];
      }
      catch (Exception ex) {
        log.Error ($"GetServiceProperty: exception for {m_serviceName}", ex);
        throw;
      }
    }

    static async Task<ProcessResult> RunCommandAsync (string command, string arguments, string? directory = null, TextWriter? outStreamWriter = null, TextWriter? errStreamWriter = null, CancellationToken cancellationToken = default)
    {
      ProcessStartInfo startInfo = new ProcessStartInfo ();
      startInfo.FileName = command;
      startInfo.Arguments = arguments;
      if (directory is not null) {
        startInfo.WorkingDirectory = directory;
      }
      startInfo.UseShellExecute = false;
      startInfo.CreateNoWindow = true;
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      startInfo.RedirectStandardError = true;
      startInfo.RedirectStandardOutput = true;

      var result = await RunProcessAsync (startInfo, outStreamWriter, errStreamWriter, cancellationToken: cancellationToken);
      return result;
    }

    static async Task<ProcessResult> RunProcessAsync (ProcessStartInfo processStartInfo, TextWriter? outStreamWriter = null, TextWriter? errStreamWriter = null, CancellationToken cancellationToken = default)
    {
      var standardOutput = new StringWriter ();
      var standardError = new StringWriter ();
      var taskCompletionSource = new TaskCompletionSource<ProcessResult> ();
      var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };
      process.Exited += async (sender, args) => {
        slog.Debug ($"RunProcessAsync: {processStartInfo.FileName} {processStartInfo.Arguments} exited");
        await process.WaitForExitAsync (); // To complete processing the standard / error outputs
        var standardOutputString = standardOutput.ToString ();
        var standardErrorString = standardError.ToString ();
        if (0 != process.ExitCode) {
          slog.Error ($"RunProcessAsync: exit code is {process.ExitCode} of {processStartInfo.FileName} {processStartInfo.Arguments}");
        }
        var processResult = new ProcessResult (process.ExitCode, standardOutputString, standardErrorString);
        taskCompletionSource.SetResult (processResult);
        if (null != errStreamWriter) {
          await errStreamWriter.WriteAsync (standardErrorString);
        }
        process.Dispose ();
      };
      if (processStartInfo.RedirectStandardOutput) {
        process.OutputDataReceived += (sender, outLine) => {
          slog.Debug ($"RunProcessAsync: received output data {outLine.Data}");
          if (null != outLine.Data) {
            if (null != outStreamWriter) {
              outStreamWriter.WriteLineAsync (outLine.Data);
            }
            standardOutput.WriteLine (outLine.Data);
          }
        };
      }
      if (processStartInfo.RedirectStandardError) {
        process.ErrorDataReceived += (sender, outLine) => {
          if (null != outLine.Data) {
            standardError.WriteLine (outLine.Data);
          }
        };
      }
      slog.Info ($"RunProcessAsync: about to start {processStartInfo.FileName} {processStartInfo.Arguments}");
      process.Start ();
      if (processStartInfo.RedirectStandardOutput) {
        process.BeginOutputReadLine ();
      }
      if (processStartInfo.RedirectStandardError) {
        process.BeginErrorReadLine ();
      }
      return await taskCompletionSource.Task;
    }
  }
}
