// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for ModificationStatus
  /// </summary>
  public class ModificationStatusDTO
  {
    /// <summary>
    /// Id of modification
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Status of modification
    /// </summary>
    public Lemoine.Model.AnalysisStatus Status { get; set; }
    
  }
}
