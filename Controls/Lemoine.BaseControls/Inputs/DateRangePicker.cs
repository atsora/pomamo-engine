// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lemoine.Model;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Date/time range picker
  /// </summary>
  public partial class DateRangePicker : UserControl
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public DateRangePicker ()
    {
      InitializeComponent ();
    }

    /// <summary>
    /// Local date/time range
    /// </summary>
    public IRange<DateTime> DateRange
    {
      get {
        LowerBound<DateTime> lower;
        if (lowerCheckBox.Checked) {
          lower = lowerDatePicker.Date;
        }
        else {
          lower = new LowerBound<DateTime> (null);
        }
        UpperBound<DateTime> upper;
        if (upperCheckBox.Checked) {
          upper = upperDatePicker.Date;
        }
        else {
          upper = new UpperBound<DateTime> (null);
        }
        return new Range<DateTime> (lower, upper, true, true);
      }
      set {
        if (value.Lower.HasValue) {
          lowerCheckBox.Checked = true;
          if (value.LowerInclusive) {
            lowerDatePicker.Value = value.Lower.Value;
          }
          else {
            lowerDatePicker.Value = value.Lower.Value.AddDays (1);
          }
        }
        else {
          lowerCheckBox.Checked = false;
        }
        if (value.Upper.HasValue) {
          upperCheckBox.Checked = true;
          if (value.UpperInclusive) {
            upperDatePicker.Value = value.Upper.Value;
          }
          else {
            upperDatePicker.Value = value.Upper.Value.AddDays (-1);
          }
        }
        else {
          upperCheckBox.Checked = false;
        }
      }
    }


    private void lowerCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      lowerDatePicker.Enabled = lowerCheckBox.Checked;
    }

    private void upperCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      upperDatePicker.Enabled = upperCheckBox.Checked;
    }
  }
}
