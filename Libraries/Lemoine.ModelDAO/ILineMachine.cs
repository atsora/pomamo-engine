// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Association of a machine within a line, having a dedicated operation
  /// </summary>
  public interface ILineMachine: IVersionable, ISerializableModel
  {
    /// <summary>
    /// Line comprising the machine
    /// </summary>
    ILine Line { get; }
    
    /// <summary>
    /// Machine associated to a line
    /// </summary>
    IMachine Machine { get; }
    
    /// <summary>
    /// Operation associated to the machine within the line
    /// </summary>
    IOperation Operation { get; set; }
    
    /// <summary>
    /// LineMachineStatus
    /// </summary>
    LineMachineStatus LineMachineStatus { get; set; }
  }
}
