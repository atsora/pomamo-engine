// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.StampingFileFlows
{
  /// <summary>
  /// Read-only the input file
  /// No output file is created
  /// </summary>
  public class ReadOnlyFlow : IStampingFileFlow
  {
    readonly ILog log = LogManager.GetLogger (typeof (ReadOnlyFlow).FullName);

    readonly string m_inputFilePath;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="stamperParametersProvider"></param>
    public ReadOnlyFlow (IStamperParametersProvider stamperParametersProvider)
      : this (stamperParametersProvider.InputFilePath)
    {
      if (!string.IsNullOrEmpty (stamperParametersProvider.OutputFilePath)) {
        log.Warn ($"ReadOnlyFlow: OutputFilePath {stamperParametersProvider.OutputFilePath} not empty or null");
      }
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    public ReadOnlyFlow (string inputFilePath)
    {
      m_inputFilePath = inputFilePath;
    }

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string InputFilePath => m_inputFilePath;

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string OutputFilePath => "";

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string StamperInputFilePath => m_inputFilePath;

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string StamperOutputFilePath
    {
      get {
        log.Error ($"StamperOutputFilePath.get: invalid in case of a read-only flow");
        throw new InvalidOperationException ("No StamperOutputFilePath in case of a read-only flow");
      }
    }

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    /// <returns></returns>
    public Task OnFailureAsync ()
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    /// <returns></returns>
    public Task OnSuccessAsync ()
    {
      return Task.CompletedTask;
    }
  }
}
