// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Collections;
using Lemoine.Model;
using System;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  /// <summary>
  /// Model for the table intermediateworkpiecesummary
  /// 
  /// This contains various information on an intermediate work piece
  /// </summary>
  public interface IIntermediateWorkPieceSummary : IDataWithId
  {
    /// <summary>
    /// Reference to the IntermediateWorkPiece
    /// 
    /// Not null
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; }

    /// <summary>
    /// Reference to the associated Component
    /// 
    /// Set to null if unknown
    /// </summary>
    IComponent Component { get; }

    /// <summary>
    /// Reference to the Work Order if known
    /// 
    /// Set to null if it could not be identified yet
    /// </summary>
    IWorkOrder WorkOrder { get; }

    /// <summary>
    /// Reference to the Line if known
    /// 
    /// Set to null if it could not be identified yet or it if is not applicable
    /// </summary>
    ILine Line { get; }

    /// <summary>
    /// If the option to split the operation slots by day is set,
    /// reference to the day.
    /// 
    /// null if the option to split the operation slot by day is not set
    /// </summary>
    DateTime? Day { get; }

    /// <summary>
    /// If the corresponding option is selected,
    /// reference to the shift.
    /// 
    /// null if there is no shift
    /// or if the option to split the operation slot by shift is not set
    /// </summary>
    IShift Shift { get; }

    /// <summary>
    /// Total number of work pieces as detected from the cycle detection
    /// </summary>
    int Counted { get; set; }

    /// <summary>
    /// Corrected number of work pieces that takes also into account the input information
    /// </summary>
    int Corrected { get; set; }

    /// <summary>
    /// Number of checked work pieces
    /// </summary>
    int Checked { get; set; }

    /// <summary>
    /// Number of scrapped work pieces
    /// </summary>
    int Scrapped { get; set; }

    /// <summary>
    /// Is the data empty ? It means may it be deleted because all the data are null ?
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
  }
}
