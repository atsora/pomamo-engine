// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Business.Group
{
  /// <summary>
  /// Interface for groups that are made of sub-groups
  /// that can be used for a GroupZoomExtension implementation
  /// </summary>
  public interface IZoomableGroup
  {
    /// <summary>
    /// Sub-groups
    /// </summary>
    IEnumerable<IGroup> SubGroups { get; }
  }
}
