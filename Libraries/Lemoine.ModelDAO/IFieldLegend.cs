// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table FieldLegend
  /// 
  /// This is a new table.
  /// 
  /// This new table lists for some fields some ranges of values that can be considered as a threshold.
  ///
  /// In an graphical application, a given legend may be displayed with a given color for example.
  /// </summary>
  public interface IFieldLegend: IDataWithVersion
  {
    // Note: IUnit does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Associated field
    /// </summary>
    IField Field { get; }
    
    /// <summary>
    /// String value to match
    /// 
    /// This may be null, if not applicable
    /// </summary>
    string StringValue { get; set; }
    
    /// <summary>
    /// Minimum range value
    /// 
    /// If null, no low bound
    /// </summary>
    double? MinValue { get; set; } // for the configuration
    
    /// <summary>
    /// Maximum range value
    /// 
    /// if null, no high bound
    /// </summary>
    double? MaxValue { get; set; } // for the configuration
    
    /// <summary>
    /// Applicable range
    /// </summary>
    Range<double> Range { get; set; }
        
    /// <summary>
    /// Associated text to the legend
    /// 
    /// It can't be null
    /// </summary>
    string Text { get; set; }
    
    /// <summary>
    /// Associated color to the legend
    /// 
    /// It can't be null
    /// </summary>
    string Color { get; set; }
  }
}
