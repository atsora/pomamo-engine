// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Association between a sequence and an operation model
  /// </summary>
  public interface ISequenceOperationModel : IVersionable, IDataWithIdentifiers, IEquatable<ISequenceOperationModel>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated sequence
    /// 
    /// not null
    /// </summary>
    ISequence Sequence { get; }

    /// <summary>
    /// Associated operation model
    /// 
    /// not null
    /// </summary>
    IOperationModel OperationModel { get; }

    /// <summary>
    /// Sequence order in a specific operation model
    /// </summary>
    double Order { get; set; }

    /// <summary>
    /// Applicable only for this path number
    /// </summary>
    int? PathNumber { get; }

    /// <summary>
    /// Durations
    /// </summary>
    IEnumerable<ISequenceDuration> Durations { get; }
  }
}
