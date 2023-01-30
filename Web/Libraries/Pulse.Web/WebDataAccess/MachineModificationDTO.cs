// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// DTO for MachineModificationDTO.
  /// </summary>
  public class MachineModificationDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// MachineId
    /// </summary>
    public int MachineId { get; set; }
    
    /// <summary>
    /// Analysis status
    /// </summary>
    public int AnalysisStatus { get; set; }
  }
}
