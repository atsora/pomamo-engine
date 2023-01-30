// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for MachineExtendedStatus
  /// </summary>
  public class MachineDetailedStatusDTO
  {
    /// <summary>
    /// Machine Id
    /// </summary>
    public int MachineId { get; set; }
    
    /// <summary>
    /// number of pieces done
    /// </summary>
    public int NbPiecesDone { get; set; }
    
    /* TO ADD : GoalPeriod / GoalNow / color */
  }
}
