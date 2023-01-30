// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Extensions.Analysis.Detection;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Extension to DetectionAnalysis
  /// </summary>
  public interface IDetectionAnalysisExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <param name="monitoredMachine">not null</param>
    /// <param name="operationDetection">not null</param>
    /// <param name="operationCycleDetection">not null</param>
    /// <returns>If false is returned, this extension must be skipped</returns>
    bool Initialize (IMonitoredMachine monitoredMachine, IOperationDetection operationDetection, IOperationCycleDetection operationCycleDetection);

    /// <summary>
    /// Should the detection analysis be run in a unique serializable transaction ?
    /// </summary>
    /// <param name="detection"></param>
    bool UseUniqueSerializableTransaction (IMachineModuleDetection detection);

    /// <summary>
    /// Restricted transaction level
    /// </summary>
    ModelDAO.TransactionLevel RestrictedTransactionLevel { get; set; }

    /// <summary>
    /// Extension to DetectionAnalysis: process a specific detection
    /// </summary>
    /// <param name="detection"></param>
    void ProcessDetection (IMachineModuleDetection detection);
  }
}
