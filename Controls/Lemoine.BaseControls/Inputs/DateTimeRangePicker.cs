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
  public partial class DateTimeRangePicker : UserControl
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public DateTimeRangePicker ()
    {
      InitializeComponent ();
    }

    /// <summary>
    /// Local date/time range
    /// </summary>
    public LocalDateTimeRange LocalDateTimeRange
    {
      get
      {
        LowerBound<DateTime> lower;
        if (lowerCheckBox.Checked) {
          lower = lowerDateTimePicker.Value;
        }
        else {
          lower = new LowerBound<DateTime> (null);
        }
        UpperBound<DateTime> upper;
        if (upperCheckBox.Checked) {
          upper = upperDateTimePicker.Value;
        }
        else {
          upper = new UpperBound<DateTime> (null);
        }
        return new LocalDateTimeRange (lower, upper);
      }
      set
      {
        if (value.Lower.HasValue) {
          lowerCheckBox.Checked = true;
          lowerDateTimePicker.Value = value.Lower.Value;
        }
        else {
          lowerCheckBox.Checked = false;
        }
        if (value.Upper.HasValue) {
          upperCheckBox.Checked = true;
          upperDateTimePicker.Value = value.Upper.Value;
        }
        else {
          upperCheckBox.Checked = false;
        }
      }
    }

    /// <summary>
    /// Utc date/time range
    /// </summary>
    public UtcDateTimeRange UtcDateTimeRange
    {
      get { return new UtcDateTimeRange (this.LocalDateTimeRange); }
      set { this.LocalDateTimeRange = new LocalDateTimeRange (value); }
    }

    private void lowerCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      lowerDateTimePicker.Enabled = lowerCheckBox.Checked;
    }

    private void upperCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      upperDateTimePicker.Enabled = upperCheckBox.Checked;
    }
  }
}
