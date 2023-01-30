// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Various kind of WorkInformations.
  /// </summary>
  public enum WorkInformationKind
  {    
    /// <summary>
    /// WorkOrder kind
    /// </summary>
    WorkOrder,
    
    /// <summary>
    /// Part kind
    /// </summary>
    Part,
    
    /// <summary>
    /// Component kind
    /// </summary>
    Component,
    
    /// <summary>
    /// Operation kind
    /// </summary>
    Operation,

    /// <summary>
    /// Job kind
    /// </summary>
    Job,

    /// <summary>
    /// Project kind
    /// </summary>
    Project
    
  }
}
