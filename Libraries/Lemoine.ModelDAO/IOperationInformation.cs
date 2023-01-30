// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IOperationInformation.
  /// </summary>
  public interface IOperationInformation : IGlobalModification
  {
    
    /// <summary>
    /// Reference to the Operation
    /// 
    /// Not null
    /// </summary>
    IOperation Operation { get; }
    
    /// <summary>
    /// Old machining duration
    /// </summary>
    TimeSpan? OldMachiningDuration { get ; set; }

  }
}
