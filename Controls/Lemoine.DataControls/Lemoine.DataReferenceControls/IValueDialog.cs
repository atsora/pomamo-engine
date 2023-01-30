// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Dialog that returns a value
  /// </summary>
  public interface IValueDialog<T>
  {
    /// <summary>
    /// Selected value
    /// </summary>
    T SelectedValue { get; set; }
    
    /// <summary>
    /// Selected values
    /// 
    /// Add this attribut (when impl.) for avoid SharDevelop Designer to generate corrupt resx file 
    /// [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    /// </summary>
    IList<T> SelectedValues { get; set; }
    
    /// <summary>
    /// ShowDialog method of the Form class
    /// </summary>
    /// <returns></returns>
    DialogResult ShowDialog ();
  }
}
