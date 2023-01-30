// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Business.Operation
{
  /// <summary>
  /// 
  /// </summary>
  public interface IPartProductionDataRange
  {
    /// <summary>
    /// Range
    /// </summary>
    UtcDateTimeRange Range { get; }

    /// <summary>
    /// Number of completed pieces
    /// </summary>
    double NbPieces { get; }

    /// <summary>
    /// Target
    /// </summary>
    double? Goal { get; }

    /// <summary>
    /// In progress ?
    /// </summary>
    bool InProgress { get; }
  }
}
