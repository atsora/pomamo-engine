// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Description of IOperationCycleDetection.
  /// </summary>
  public interface IOperationCycleDetectionExtension: IDetectionExtension
  {
    /// <summary>
    /// Start a detection process for the specified machine module
    /// 
    /// It is not run inside a transaction
    /// </summary>
    void DetectionProcessStart ();
    
    /// <summary>
    /// Complete a detection process for the specified machine module
    /// 
    /// It is not run inside a transaction
    /// </summary>
    void DetectionProcessComplete ();
    
    /// <summary>
    /// An error was raised during the detection process
    /// 
    /// It is not run inside a transaction
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="ex"></param>
    void DetectionProcessError (IMachineModule machineModule, Exception ex);
    
    /// <summary>
    /// Start a cycle
    /// </summary>
    /// <param name="operationCycle">Partial cycle which has just started</param>
    void StartCycle (IOperationCycle operationCycle);
    
    /// <summary>
    /// Stop a cycle
    /// </summary>
    /// <param name="operationCycle">Cycle that corresponds to the stopped cycle</param>
    void StopCycle (IOperationCycle operationCycle);
    
    /// <summary>
    /// A 'between cycles' period was created
    /// </summary>
    /// <param name="betweenCycles"></param>
    void CreateBetweenCycle (IBetweenCycles betweenCycles);
  }
}
