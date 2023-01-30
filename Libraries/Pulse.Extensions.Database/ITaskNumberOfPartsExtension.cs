// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Interface to get the number of parts for a specific task
  /// </summary>
  public interface ITaskNumberOfPartsExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Priority
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// Initialize the plugin
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns>the plugin is active</returns>
    bool Initialize (IMachine machine);

    /// <summary>
    /// Get the number of produced parts when a task is defined
    /// </summary>
    /// <param name="shiftPieces"></param>
    /// <param name="globalPieces"></param>
    /// <param name="task">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    bool GetNumberOfProducedParts (out double shiftPieces, out double globalPieces,
                                   ITask task,
                                   DateTime? day, IShift shift);
  }
}
