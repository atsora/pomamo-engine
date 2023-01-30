// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Control made of 7 checkboxes enabling the choice of the days within a week
  /// </summary>
  public partial class DayInWeekPicker : UserControl
  {
    /// <summary>
    /// Flags defining the days
    /// </summary>
    public enum Days
    {
      /// <summary>
      /// Monday
      /// </summary>
      Mon =  1,
      /// <summary>
      /// Tuesday
      /// </summary>
      Tue =  2,
      /// <summary>
      /// Wednesday
      /// </summary>
      Wed =  4,
      /// <summary>
      /// Thursday
      /// </summary>
      Thu =  8,
      /// <summary>
      /// Friday
      /// </summary>
      Fri = 16,
      /// <summary>
      /// Saturday
      /// </summary>
      Sat = 32,
      /// <summary>
      /// Sunday
      /// </summary>
      Sun = 64
    };

    #region Getters / Setters
    /// <summary>
    /// Days selected in the week
    /// </summary>
    public int DaysInWeek {
      get {
        int val = 0;
        if (checkBoxMonday.Checked) {
          val += (int)Days.Mon;
        }

        if (checkBoxTuesday.Checked) {
          val += (int)Days.Tue;
        }

        if (checkBoxWednesday.Checked) {
          val += (int)Days.Wed;
        }

        if (checkBoxThursday.Checked) {
          val += (int)Days.Thu;
        }

        if (checkBoxFriday.Checked) {
          val += (int)Days.Fri;
        }

        if (checkBoxSaturday.Checked) {
          val += (int)Days.Sat;
        }

        if (checkBoxSunday.Checked) {
          val += (int)Days.Sun;
        }

        return val;
      }
      set {
        if (value == 0) {
          value = (int)Days.Mon | (int)Days.Tue | (int)Days.Wed | (int)Days.Thu |
            (int)Days.Fri | (int)Days.Sat | (int)Days.Sun;
        }
        checkBoxMonday.Checked    = ((value & (int)Days.Mon) != 0);
        checkBoxTuesday.Checked   = ((value & (int)Days.Tue) != 0);
        checkBoxWednesday.Checked = ((value & (int)Days.Wed) != 0);
        checkBoxThursday.Checked  = ((value & (int)Days.Thu) != 0);
        checkBoxFriday.Checked    = ((value & (int)Days.Fri) != 0);
        checkBoxSaturday.Checked  = ((value & (int)Days.Sat) != 0);
        checkBoxSunday.Checked    = ((value & (int)Days.Sun) != 0);
      }
    }
    
    /// <summary>
    /// Return the number of days checked
    /// </summary>
    public int DayCount {
      get {
        int val = 0;
        if (checkBoxMonday.Checked) {
          val++;
        }

        if (checkBoxTuesday.Checked) {
          val++;
        }

        if (checkBoxWednesday.Checked) {
          val++;
        }

        if (checkBoxThursday.Checked) {
          val++;
        }

        if (checkBoxFriday.Checked) {
          val++;
        }

        if (checkBoxSaturday.Checked) {
          val++;
        }

        if (checkBoxSunday.Checked) {
          val++;
        }

        return val;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public DayInWeekPicker()
    {
      InitializeComponent();
      
      // Tooltips
      new ToolTip().SetToolTip(checkBoxMonday, "Monday");
      new ToolTip().SetToolTip(checkBoxTuesday, "Tuesday");
      new ToolTip().SetToolTip(checkBoxWednesday, "Wednesday");
      new ToolTip().SetToolTip(checkBoxThursday, "Thursday");
      new ToolTip().SetToolTip(checkBoxFriday, "Friday");
      new ToolTip().SetToolTip(checkBoxSaturday, "Saturday");
      new ToolTip().SetToolTip(checkBoxSunday, "Sunday");
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Format the days: "Monday, Wednesday, Sunday" for instance
    /// </summary>
    /// <returns>a string</returns>
    public string FormattedDays()
    {
      IList<string> stringList = new List<string>();
      if (checkBoxMonday.Checked) {
        stringList.Add("Monday");
      }

      if (checkBoxTuesday.Checked) {
        stringList.Add("Tuesday");
      }

      if (checkBoxWednesday.Checked) {
        stringList.Add("Wednesday");
      }

      if (checkBoxThursday.Checked) {
        stringList.Add("Thursday");
      }

      if (checkBoxFriday.Checked) {
        stringList.Add("Friday");
      }

      if (checkBoxSaturday.Checked) {
        stringList.Add("Saturday");
      }

      if (checkBoxSunday.Checked) {
        stringList.Add("Sunday");
      }

      return String.Join(", ", stringList.ToArray());
    }
    #endregion // Methods
  }
}
