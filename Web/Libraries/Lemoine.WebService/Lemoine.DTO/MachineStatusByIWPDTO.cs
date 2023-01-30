// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for MachineStatusByIWPDTO.
  /// </summary>
  public class MachineStatusByIWPDTO
  {
    /// <summary>
    /// Text
    /// </summary>
    public string LineDisplay { get; set; }
    
    /// <summary>
    /// Text
    /// </summary>
    public string IWPDisplay { get; set; }
    
    /// <summary>
    /// number of pieces done during the whole period
    /// </summary>
    public int NbPiecesDone { get; set; }
    
    /// <summary>
    /// number of pieces to do during the whole period
    /// </summary>
    public int GoalPeriod { get; set; }
    
    /// <summary>
    /// number of pieces to do between begin and now
    /// </summary>
    public int GoalNow { get; set; }
    
    /// <summary>
    /// List of machine status (nb of Pieces done) by machine 
    /// </summary>
    public List<MachineDetailedStatusDTO> List { get; set; }
    
  }
}
