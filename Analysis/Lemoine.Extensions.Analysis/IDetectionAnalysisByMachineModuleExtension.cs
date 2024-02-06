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
  public interface IDetectionAnalysisByMachineModuleExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="operationDetection">not null</param>
    /// <param name="operationCycleDetection">not null</param>
    /// <param name="sequenceDetection">not null</param>
    /// <param name="sequenceMilestoneDetection">not null</param>
    /// <returns>If false is returned, this extension must be skipped</returns>
    bool Initialize (IMachineModule machineModule, IOperationDetection operationDetection, IOperationCycleDetection operationCycleDetection, ISequenceDetection sequenceDetection, ISequenceMilestoneDetection sequenceMilestoneDetection);

    /// <summary>
    /// Restricted transaction level
    /// </summary>
    ModelDAO.TransactionLevel RestrictedTransactionLevel { get; set; }

    /// <summary>
    /// Should the detection analysis be run in a unique serializable transaction ?
    /// </summary>
    /// <param name="detection"></param>
    bool UseUniqueSerializableTransaction (IMachineModuleDetection detection);

    /// <summary>
    /// Extension to DetectionAnalysis: process a specific detection
    /// </summary>
    /// <param name="detection"></param>
    void ProcessDetection (IMachineModuleDetection detection);
  }
}
