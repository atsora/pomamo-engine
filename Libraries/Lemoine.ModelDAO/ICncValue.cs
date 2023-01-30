// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Cnc value
  /// 
  /// It contains only different kind of data that is received directly from the Cnc. The description of these data is in the Field table.
  /// 
  /// It contains now only the data from the machine and its past data can't be modified.
  /// Only the current data can be modified and only from some data coming from the CNC.
  /// </summary>
  public interface ICncValue
    : IVersionable<long>
    , IPartitionedByMachineModule
    , Lemoine.Model.ISerializableModel
    , IWithDateTimeRange
  {
    /// <summary>
    /// Associated field
    /// </summary>
    IField Field { get; }
    
    /// <summary>
    /// Begin UTC date/time
    /// </summary>
    DateTime Begin { get; }
    
    /// <summary>
    /// End UTC date/time
    /// </summary>
    DateTime End { get; set; }

    /// <summary>
    /// Length
    /// </summary>
    TimeSpan Length { get; }
    
    /// <summary>
    /// String value in case the corresponding Field refers to a String
    /// </summary>
    string String { get; set; }
    
    /// <summary>
    /// Int value in case the corresponding Field refers to an Int32
    /// </summary>
    Nullable<int> Int { get; set; }
    
    /// <summary>
    /// Double or average value in case the corresponding Field refers to a Double
    /// </summary>
    Nullable<double> Double { get; set; }
    
    /// <summary>
    /// String, Int or Double value according to the Type property of the field
    /// </summary>
    object Value { get; set; }
    
    /// <summary>
    /// Standard Deviation in case the aggregation type is Average
    /// </summary>
    Nullable<double> Deviation { get; set; }
    
    /// <summary>
    /// Was the Cnc value interrupted ?
    /// </summary>
    bool Stopped { get; set; }
    
    /// <summary>
    /// <see cref="ISlot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
  }
}
