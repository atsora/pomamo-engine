// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// NullableTimePickerControl: select time in DD.HH:MM:SS format, or null
  /// Limitation: bounded to 999.23:59:59
  /// </summary>
  public partial class NullableTimeSpanPicker : UserControl
  {
    #region Members
    /// <summary>
    /// Event raised in case of a value change of the textbox
    /// </summary>
    public event EventHandler ValueChanged;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (NullableTimeSpanPicker).FullName);


    #region Getters / Setters
    /// <summary>
    /// value as timespan
    /// </summary>
    public TimeSpan? Value
    {
      get
      {
        return this.nullCheckBox.Checked ? this.durationUpDown.TimeSpanValue : (TimeSpan?)null;
      }

      set
      {
        if (value.HasValue) {
          this.nullCheckBox.Checked = true;
          try {
            this.durationUpDown.TimeSpanValue = value.Value;
          }
          catch (Exception) {
            MessageBox.Show ("Value is too big to be represented in control");          
            this.durationUpDown.Value = this.durationUpDown.Maximum;
          }
        }
        else {
          this.nullCheckBox.Checked = false;
          this.durationUpDown.Value = 0m;
        }
      }
    }

    /// <summary>
    /// value as hours
    /// </summary>
    public double? ValueAsHours
    {
      get
      {
        return this.nullCheckBox.Checked ? this.durationUpDown.TimeSpanValue.TotalHours : (double?) null;
      }

      set
      {
        if (value.HasValue) {
          this.nullCheckBox.Checked = true;
          try {
            this.durationUpDown.TimeSpanValue = TimeSpan.FromHours(value.Value);
          }
          catch (Exception) {
            MessageBox.Show ("Value is too big to be represented in control");
            this.durationUpDown.Value = this.durationUpDown.Maximum;
          }
        }
        else {
          this.nullCheckBox.Checked = false;
          this.durationUpDown.Value = 0m;
        }
      }

    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public NullableTimeSpanPicker ()
    {
      InitializeComponent ();
      this.durationUpDown.Value = 0m;
      this.durationUpDown.Enabled = false;
      this.nullCheckBox.Checked = false;
    }

    #endregion // Constructors

    /// <summary>
    /// raise the "value change" event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnValueChange (EventArgs e)
    {
      EventHandler handler = this.ValueChanged;
      if (handler != null) {
        handler (this, e);
      }
    }

    void NullCheckBoxCheckedChanged (object sender, EventArgs e)
    {
      this.durationUpDown.Enabled = this.nullCheckBox.Checked;
      this.OnValueChange (EventArgs.Empty);
    }

    void TimeSpanChanged (object sender, EventArgs e)
    {
      this.OnValueChange (EventArgs.Empty);
    }
  }
}
