// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// To keep track of ISO file periods
  /// </summary>
  public interface IIsoFileSlot : ISlot, IVersionable, IComparable<IIsoFileSlot>, IPartitionedByMachineModule
  {
    /// <summary>
    /// Isofile (not null)
    /// </summary>
    IIsoFile IsoFile { get; set; }
  }
}
