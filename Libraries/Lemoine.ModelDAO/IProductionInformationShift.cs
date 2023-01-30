// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Modification to set a production status at the end of a shift period
  /// for a specified machine and production
  /// </summary>
  public interface IProductionInformationShift: IMachineModification
  {
    /// <summary>
    /// Associated day
    /// </summary>
    DateTime Day { get; }
    
    /// <summary>
    /// Associated shift
    /// 
    /// not null
    /// </summary>
    IShift Shift { get; }
    
    /// <summary>
    /// Associated work order
    /// 
    /// nullable
    /// </summary>
    IWorkOrder WorkOrder { get; set; }
    
    /// <summary>
    /// Associated intermediate work piece
    /// 
    /// not null
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; }
    
    /// <summary>
    /// Number of produced pieces
    /// </summary>
    int Checked { get; set; }
    
    /// <summary>
    /// Number of scrapped parts
    /// 
    /// Default: 0
    /// </summary>
    int Scrapped { get; set; }
    
    /// <summary>
    /// At the end of the shift, was it the production in progress ?
    /// </summary>
    bool? InProgress { get; set; }
  }
}
