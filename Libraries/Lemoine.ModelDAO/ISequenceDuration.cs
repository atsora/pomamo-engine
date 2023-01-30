// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Sequence duration
  /// </summary>
  public interface ISequenceDuration : IVersionable, IDataWithIdentifiers, IEquatable<ISequenceDuration>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated sequence operation model
    /// </summary>
    ISequenceOperationModel SequenceOperationModel { get; }

    /// <summary>
    /// Restrict to a given machine filter
    /// </summary>
    IMachineFilter MachineFilter { get; }

    /// <summary>
    /// Applicable range
    /// </summary>
    UtcDateTimeRange ApplicableRange { get; set; }

    /// <summary>
    /// Estimated duration
    /// </summary>
    TimeSpan EstimatedDuration { get; }
  }
}
