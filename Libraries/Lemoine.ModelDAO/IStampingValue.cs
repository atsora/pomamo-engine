// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table StampingValue
  /// 
  /// This table lists the different machining parameters
  /// and various information on a sequence,
  /// that could have been taken by the Stamping.
  /// 
  /// It has been designed to be very flexible and accept almost any value,
  /// as soon as the kind of value is declared in the Field table.
  /// </summary>
  public interface IStampingValue : IVersionable, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Reference to the Sequence table
    /// </summary>
    ISequence Sequence { get; }
    
    /// <summary>
    /// Reference to the Field table
    /// </summary>
    IField Field { get; }
    
    /// <summary>
    /// String value in case the corresponding Field refers to a String
    /// </summary>
    string String { get; set; }
    
    /// <summary>
    /// Int value in case the corresponding Field refers to an Int32
    /// </summary>
    Nullable<int> Int { get; set; }
    
    /// <summary>
    /// Double value in case the corresponding Field refers to a Double
    /// </summary>
    Nullable<double> Double { get; set; }
  }
}
