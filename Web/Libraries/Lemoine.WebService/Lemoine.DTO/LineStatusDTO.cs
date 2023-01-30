// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of LineStatusDTO.
  /// </summary>
  public class LineStatusDTO
  {
    #region Members
    /// <summary>
    /// text displayed as line's label
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Start date of production line
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End date of production line
    /// </summary>
    public string Deadline { get; set; }
    
    /// <summary>
    /// Current status of global production line 
    /// Expected values are: "overdue", "ontime"
    /// </summary>
    public string LineStatus { get; set; }
    
    /// <summary>
    /// current status of operation included in production line
    /// </summary>
    public List<IntermediateWorkPieceStatusDTO> IntermediateWorkPieceStatus { get; set; }
       
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (LineStatusDTO).FullName);


    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public LineStatusDTO ()
    {
    }

    #endregion // Constructors

  }

}
