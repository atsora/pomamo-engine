// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of IntermediateWorkPieceStatusDTO.
  /// </summary>
  public class IntermediateWorkPieceStatusDTO
  {
    #region Members
    /// <summary>
    /// Operation id
    /// </summary>
    public int Id {get; set;}
    
    /// <summary>
    /// Operation label
    /// </summary>
    public string Display {get; set;}
    
    /// <summary>
    /// Global production expected for Operation
    /// </summary>
    public int GlobalExpected { get; set; }
    
    /// <summary>
    /// Global production completed by Operation
    /// </summary>
    public int GlobalCompleted { get; set; }

    /// <summary>
    /// Global production expected for during current shift
    /// </summary>
    public int ShiftExpected { get; set; }

    /// <summary>
    /// Global production completed for during current shift
    /// </summary>
    public int ShiftCompleted { get; set; }

    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (IntermediateWorkPieceStatusDTO).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public IntermediateWorkPieceStatusDTO ()
    {
    }
    #endregion // Constructors

  }
}
