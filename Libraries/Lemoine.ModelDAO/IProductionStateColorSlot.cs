// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual model of table ReasonSlot only with the production state color and the production rate
  /// </summary>
  public interface IProductionStateColorSlot
    : IWithRange
    , IPartitionedByMachine
  {
    /// <summary>
    /// Production state color
    /// </summary>
    string Color { get; }

    /// <summary>
    /// Production rate
    /// </summary>
    double? ProductionRate { get; }

    /// <summary>
    /// Is the machine slot empty ?
    /// 
    /// If the slot is empty, it will not be inserted in the database.
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool ReferenceDataEquals (IProductionStateColorSlot other);

    /// <summary>
    /// Clone the reason color slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IProductionStateColorSlot Clone (UtcDateTimeRange range);
  }
}
