// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Pulse.Extensions.Business.ShiftSummary
{
  /// <summary>
  /// What happened on a machine during a shift: the input of a <see cref="IShiftSummaryExtension"/>
  ///
  /// It is a plain value object that can be serialized in JSON, with no entity
  /// </summary>
  public interface IShiftActivityData
  {
    /// <summary>
    /// Machine id
    /// </summary>
    int MachineId { get; }

    /// <summary>
    /// Display of the machine
    /// </summary>
    string MachineDisplay { get; }

    /// <summary>
    /// Day
    /// </summary>
    DateTime Day { get; }

    /// <summary>
    /// Shift id, null if no shift is defined (the whole day is considered)
    /// </summary>
    int? ShiftId { get; }

    /// <summary>
    /// Display of the shift, nullable
    /// </summary>
    string ShiftDisplay { get; }

    /// <summary>
    /// UTC range of the shift
    /// </summary>
    UtcDateTimeRange Range { get; }

    /// <summary>
    /// The shift is over: the data will not change any more
    /// </summary>
    bool Completed { get; }

    /// <summary>
    /// Items, the most important first
    ///
    /// not null
    /// </summary>
    IList<IShiftActivityItem> Items { get; }
  }
}
