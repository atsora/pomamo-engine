// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// DateTimePicker showing only the date
  /// </summary>
  public class DatePicker : System.Windows.Forms.DateTimePicker
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DatePicker() : base()
    {
      base.ShowUpDown = false;
      base.Format = DateTimePickerFormat.Custom;
      base.CustomFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Override of "ShowUpDown", that does nothing
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool ShowUpDown { get; set; }
    
    /// <summary>
    /// Override of "Format", that does nothing
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DateTimePickerFormat Format { get; set; }
    
    /// <summary>
    /// Override of "CustomFormat", that does nothing
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string CustomFormat { get; set; }

    /// <summary>
    /// Associated date
    /// </summary>
    public DateTime Date => new DateTime (this.Value.Year, this.Value.Month, this.Value.Day, 00, 00, 00, DateTimeKind.Unspecified);
    #endregion // Getters / Setters
  }
}
