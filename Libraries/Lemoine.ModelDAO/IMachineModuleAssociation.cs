// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IMachineModuleAssociation.
  /// </summary>
  public interface IMachineModuleAssociation: IMachineModification
  {
    /// <summary>
    /// Reference to the MachineModule
    /// 
    /// It can't be null
    /// </summary>
    IMachineModule MachineModule { get; set; }
    
    /// <summary>
    /// UTC begin date/time of the association
    /// </summary>
    LowerBound<DateTime> Begin { get; set; }

    /// <summary>
    /// UTC end date/time (optional) of the association
    /// </summary>
    UpperBound<DateTime> End { get; set; }
  }
}
