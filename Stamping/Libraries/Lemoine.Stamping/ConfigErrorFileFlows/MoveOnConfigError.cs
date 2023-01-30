// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.ConfigErrorFileFlows
{
  /// <summary>
  /// Just move the file on config error if both the input and the output file path are set
  /// </summary>
  public class MoveOnConfigError: IConfigErrorFileFlow
  {
    readonly ILog log = LogManager.GetLogger (typeof (MoveOnConfigError).FullName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <returns></returns>
    public Task OnConfigError (string inputFilePath, string outputFilePath)
    {
      try {
        if (log.IsWarnEnabled) {
          log.Warn ($"OnConfigError: move {inputFilePath} to {outputFilePath}");
        }
        File.Move (inputFilePath, outputFilePath);
      }
      catch (Exception ex) {
        log.Fatal ($"OnConfigError: exception", ex);
      }
      return Task.CompletedTask;
    }
  }
}
