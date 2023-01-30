// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Represents a transition in the Machine state template flow
  /// </summary>
  public interface IMachineStateTemplateFlow: IVersionable
  {
    /// <summary>
    /// Possible transition from this machine state template
    /// 
    /// Not null
    /// </summary>
    IMachineStateTemplate From { get; set; }
    
    /// <summary>
    /// Possible transistion to this machine state template
    /// 
    /// Not null
    /// </summary>
    IMachineStateTemplate To { get; set; }
  }
}
