// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.Stamping.StampingFileFlows
{
  /// <summary>
  /// On success, remove the source file
  /// On error, move the file
  /// Use a temporary file
  /// </summary>
  public class RemoveOnSuccessMoveOnFailure: IStampingFileFlow
  {
    readonly ILog log = LogManager.GetLogger (typeof (RemoveOnSuccessMoveOnFailure).FullName);

    readonly string m_inputFilePath;
    readonly string m_outputFilePath;
    readonly string m_tempFilePath;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="stamperParametersProvider"></param>
    public RemoveOnSuccessMoveOnFailure (IStamperParametersProvider stamperParametersProvider)
      : this (stamperParametersProvider.InputFilePath, stamperParametersProvider.OutputFilePath)
    { }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    public RemoveOnSuccessMoveOnFailure (string inputFilePath, string outputFilePath)
    {
      m_inputFilePath = inputFilePath;
      m_outputFilePath = outputFilePath;
      m_tempFilePath = Path.GetTempFileName ();
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
    public string StamperOutputFilePath => m_tempFilePath;

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    /// <returns></returns>
    public async Task OnSuccessAsync ()
    {
      try {
        if (log.IsDebugEnabled) {
          log.Debug ($"OnSuccessAsync: move {m_tempFilePath} to {m_outputFilePath}");
        }
        File.Move (m_tempFilePath, m_outputFilePath, true);
      }
      catch (Exception ex) {
        log.Error ($"OnSuccessAsync: exception => call OnFailureAsync", ex);
        await OnFailureAsync ();
        return;
      }

      try {
        if (log.IsDebugEnabled) {
          log.Debug ($"OnSuccessAsync: remove {m_inputFilePath}");
        }
        File.Delete (m_inputFilePath);
      }
      catch (Exception ex) {
        log.Error ($"OnSuccessAsync: exception while deleting {m_inputFilePath}", ex);
      }
    }

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    /// <returns></returns>
    public Task OnFailureAsync ()
    {
      try {
        if (log.IsDebugEnabled) {
          log.Debug ($"OnFailureAsync: move {m_inputFilePath} to {m_outputFilePath}");
        }
        File.Move (m_inputFilePath, m_outputFilePath);
      }
      catch (Exception ex) {
        log.Error ($"OnFailureAsync: exception", ex);
      }
      return Task.CompletedTask;
    }
  }
}
