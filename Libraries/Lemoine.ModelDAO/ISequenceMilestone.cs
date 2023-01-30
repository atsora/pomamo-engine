// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// 
  /// </summary>
  public interface ISequenceMilestone : IVersionable
  {
    /// <summary>
    /// Reference to the MachineModule
    /// 
    /// not null
    /// </summary>
    IMachineModule MachineModule { get; }

    /// <summary>
    /// Date/time of the data
    /// </summary>
    DateTime DateTime { get; set; }

    /// <summary>
    /// Associated sequence if known
    /// </summary>
    ISequence Sequence { get; set; }

    /// <summary>
    /// Milestone
    /// </summary>
    TimeSpan Milestone { get; set; }

    /// <summary>
    /// Completed
    /// </summary>
    bool Completed { get; set; }
  }
}
