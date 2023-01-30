// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Stamp
  /// 
  /// This new table is aimed at replacing with the Stamp Detection
  /// and Sequence tables the sfkstartend and sfkoperation tables.
  /// 
  /// This table makes the association between a stamp and:
  /// <item>an operation cycle</item>
  /// <item>a sequence</item>
  /// <item>an operation</item>
  /// <item>a component</item>
  /// </summary>
  public interface IStamp: IVersionable, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Reference to the ISO file where this identifier is found
    /// </summary>
    IIsoFile IsoFile { get; set; }
    
    /// <summary>
    /// Position in the ISO file of the stamp
    /// </summary>
    Nullable<int> Position { get; set; }
    
    /// <summary>
    /// Reference to the associated sequence
    /// </summary>
    ISequence Sequence { get; set; }
    
    /// <summary>
    /// Reference to the associated operation
    /// </summary>
    IOperation Operation { get; set; }
    
    /// <summary>
    /// Reference to the associated component
    /// </summary>
    IComponent Component { get; set; }
    
    /// <summary>
    /// Does this stamp refers to an operation cycle begin ?
    /// 
    /// Default is false
    /// </summary>
    bool OperationCycleBegin { get; set; }
    
    /// <summary>
    /// Does this stamp refers to an operation cycle end ?
    /// 
    /// Default is false
    /// </summary>
    bool OperationCycleEnd { get; set; }
    
    /// <summary>
    /// Does this stamp refers to an ISO File end ?
    /// 
    /// Default is false
    /// </summary>
    bool IsoFileEnd { get; set; }
  }
}
