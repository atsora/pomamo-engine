// Copyright (c) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lemoine.Core.AsyncProcess
{
  /// <summary>
  /// Extensions to <see cref="ProcessStartInfo"/>
  /// </summary>
  public static class ProcessStartInfoExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProcessStartInfoExtensions).FullName);

    public static async Task<ProcessResult> RunProcessAsync (this ProcessStartInfo processStartInfo, TextWriter outStreamWriter = null, TextWriter errStreamWriter = null)
    {
      var standardOutput = new StringWriter ();
      var standardError = new StringWriter ();
      var taskCompletionSource = new TaskCompletionSource<ProcessResult> ();
      var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };
      process.Exited += (sender, args) => {
        log.DebugFormat ($"RunProcessAsync: {processStartInfo.FileName} {processStartInfo.Arguments} exited");
        process.WaitForExit (); // To complete processing the standard / error outputs
        var standardOutputString = standardOutput.ToString ();
        var standardErrorString = standardError.ToString ();
        if (0 != process.ExitCode) {
          log.Error ($"RunProcessAsync: exit code is {process.ExitCode} of {processStartInfo.FileName} {processStartInfo.Arguments}");
        }
        var processResult = new ProcessResult (process.ExitCode, standardOutputString, standardErrorString);
        taskCompletionSource.SetResult (processResult);
        errStreamWriter?.WriteAsync (standardErrorString);
        process.Dispose ();
      };
      if (processStartInfo.RedirectStandardOutput) {
        process.OutputDataReceived += (sender, outLine) => {
          log.Debug ($"RunProcessAsync: received output data {outLine.Data}");
          if (null != outLine.Data) {
            outStreamWriter?.WriteLineAsync (outLine.Data);
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
      log.Info ($"RunProcessAsync: about to start {processStartInfo.FileName} {processStartInfo.Arguments}");
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

#endif // !NET40