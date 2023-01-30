// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Stamp directly a file to a destination directory
  /// On failure, do nothing
  /// Keep the source file
  /// </summary>
  public class DirectFileFlow: IStampingFileFlow
  {
    readonly ILog log = LogManager.GetLogger (typeof (DirectFileFlow).FullName);

    readonly string m_inputFilePath;
    readonly string m_outputFilePath;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="stamperParametersProvider"></param>
    public DirectFileFlow (IStamperParametersProvider stamperParametersProvider)
      : this (stamperParametersProvider.InputFilePath, stamperParametersProvider.OutputFilePath)
    { }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    public DirectFileFlow (string inputFilePath, string outputFilePath)
    {
      m_inputFilePath = inputFilePath;
      m_outputFilePath = outputFilePath;
      try {
        System.IO.File.Delete (outputFilePath);
      }
      catch (Exception ex) {
        log.Error ($"DirectFileFlow: delete of {outputFilePath} failed", ex);
      }
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
