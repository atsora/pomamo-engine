// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.Impl
{
  /// <summary>
  /// Default implementation of <see cref="IStamperParametersProvider"/>
  /// </summary>
  public class StamperParametersProvider : IStamperParametersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (StamperParametersProvider).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public StamperParametersProvider (string inputFilePath, string outputFilePath = "")
    {
      this.InputFilePath = inputFilePath;
      this.OutputFilePath = outputFilePath;
    }

    /// <summary>
    /// <see cref="IStamperParametersProvider"/>
    /// </summary>
    public string InputFilePath { get; }

    /// <summary>
    /// <see cref="IStamperParametersProvider"/>
    /// </summary>
    public string OutputFilePath { get; }
  }
}
