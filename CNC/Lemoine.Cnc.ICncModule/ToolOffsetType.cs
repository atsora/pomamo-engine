// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// ToolOffsetType
  /// </summary>
  public enum ToolOffsetType
  {
    /// <summary>
    /// Tool length offset
    /// </summary>
    ToolLengthOffset = 0,
    /// <summary>
    /// Tool wear offset
    /// </summary>
    ToolWearOffset = 1,
    /// <summary>
    /// Tool diameter offset
    /// </summary>
    ToolDiameterOffset = 2,
    /// <summary>
    /// Tool diameter wear offset
    /// </summary>
    ToolDiameterWearOffset = 3,
  }
}
