// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Current Cnc value
  /// </summary>
  public interface ICurrentCncValue: IVersionable
  {
    /// <summary>
    /// Associated machine module
    /// </summary>
    IMachineModule MachineModule { get; }
    
    /// <summary>
    /// Associated field
    /// </summary>
    IField Field { get; }
    
    /// <summary>
    /// UTC date/time stamp
    /// </summary>
    DateTime DateTime { get; set; }

    /// <summary>
    /// String value in case the corresponding Field refers to a String
    /// </summary>
    string String { get; set; }
    
    /// <summary>
    /// Int value in case the corresponding Field refers to an Int32
    /// </summary>
    int? Int { get; set; }
    
    /// <summary>
    /// Double or average value in case the corresponding Field refers to a Double
    /// </summary>
    double? Double { get; set; }
    
    /// <summary>
    /// String, Int or Double value according to the Type property of the field
    /// </summary>
    object Value { get; set; }
  }
}
