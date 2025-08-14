// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ManuOrderMachineAssociation
  /// </summary>
  public interface IManufacturingOrderMachineAssociation: IMachineModification
  {
    /// <summary>
    /// UTC date/time range
    /// </summary>
    UtcDateTimeRange Range { get; }

    /// <summary>
    /// Task to associate to a machine with a work order
    /// </summary>
    IManufacturingOrder ManufacturingOrder { get; set; }
    
    /// <summary>
    /// Association option
    /// </summary>
    AssociationOption? Option { get; set; }
  }
}
