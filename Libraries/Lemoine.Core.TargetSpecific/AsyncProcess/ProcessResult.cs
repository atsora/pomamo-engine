// Copyright (c) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Core.AsyncProcess
{
  /// <summary>
  /// Result of a process execution
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
}
