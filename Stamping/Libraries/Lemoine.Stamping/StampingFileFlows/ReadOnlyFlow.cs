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
    readonly string m_outputFilePath;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="stamperParametersProvider"></param>
    public ReadOnlyFlow (IStamperParametersProvider stamperParametersProvider)
      : this (stamperParametersProvider.InputFilePath, stamperParametersProvider.OutputFilePath)
    { }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    public ReadOnlyFlow (string inputFilePath, string outputFilePath)
    {
      m_inputFilePath = inputFilePath;
      m_outputFilePath = outputFilePath;
    }

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string InputFilePath => m_inputFilePath;

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string OutputFilePath => m_outputFilePath;

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string StamperInputFilePath => m_inputFilePath;

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string StamperOutputFilePath => m_outputFilePath;

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
