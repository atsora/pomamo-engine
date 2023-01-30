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
  public interface IOperationDuration : IVersionable, IDataWithIdentifiers, IEquatable<IOperationDuration>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated operation
    /// 
    /// not null
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Restrict to this specific model. If null, default duration for this operation
    /// 
    /// nullable
    /// </summary>
    IOperationModel OperationModel { get; set; }

    /// <summary>
    /// Restrict to this machine filter. If null, no machine restriction with a machine filter
    /// 
    /// nullable
    /// </summary>
    IMachineFilter MachineFilter { get; set; }

    /// <summary>
    /// Applicable range
    /// </summary>
    UtcDateTimeRange ApplicableRange { get; set; }

    /// <summary>
    /// Machining duration
    /// </summary>
    TimeSpan? Machining { get; set; }

    /// <summary>
    /// Loading duration
    /// </summary>
    TimeSpan? Loading { get; set; }

    /// <summary>
    /// Unloading duration
    /// </summary>
    TimeSpan? Unloading { get; set; }

    /// <summary>
    /// Set-up duration
    /// </summary>
    TimeSpan? Setup { get; set; }

    /// <summary>
    /// Tear-down duration
    /// </summary>
    TimeSpan? Teardown { get; set; }
  }
}
