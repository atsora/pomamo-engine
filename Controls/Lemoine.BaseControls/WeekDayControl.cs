// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of WeekDayControl.
  /// </summary>
  public partial class WeekDayControl : UserControl
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WeekDayControl).FullName);

    #region Getters / Setters
    /// <summary>
    /// List of selected (checked) days of week
    /// </summary>
    public ICollection<DayOfWeek> SelectedDays {
      get {
        ICollection<DayOfWeek> days = new List<DayOfWeek>();
        if (this.checkedListBox1.CheckedItems.Contains(PulseCatalog.GetString("Monday"))) {
          days.Add(DayOfWeek.Monday);
        }

        if (this.checkedListBox1.CheckedItems.Contains(PulseCatalog.GetString("Tuesday"))) {
          days.Add(DayOfWeek.Tuesday);
        }

        if (this.checkedListBox1.CheckedItems.Contains(PulseCatalog.GetString("Wednesday"))) {
          days.Add(DayOfWeek.Wednesday);
        }

        if (this.checkedListBox1.CheckedItems.Contains(PulseCatalog.GetString("Thursday"))) {
          days.Add(DayOfWeek.Thursday);
        }

        if (this.checkedListBox1.CheckedItems.Contains(PulseCatalog.GetString("Friday"))) {
          days.Add(DayOfWeek.Friday);
        }

        if (this.checkedListBox1.CheckedItems.Contains(PulseCatalog.GetString("Saturday"))) {
          days.Add(DayOfWeek.Saturday);
        }

        if (this.checkedListBox1.CheckedItems.Contains(PulseCatalog.GetString("Sunday"))) {
          days.Add(DayOfWeek.Sunday);
        }

        return days;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public WeekDayControl()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.checkedListBox1.Items.Add(PulseCatalog.GetString("Monday"), true);
      this.checkedListBox1.Items.Add(PulseCatalog.GetString("Tuesday"), true);
      this.checkedListBox1.Items.Add(PulseCatalog.GetString("Wednesday"), true);
      this.checkedListBox1.Items.Add(PulseCatalog.GetString("Thursday"), true);
      this.checkedListBox1.Items.Add(PulseCatalog.GetString("Friday"), true);
      this.checkedListBox1.Items.Add(PulseCatalog.GetString("Saturday"), false);
      this.checkedListBox1.Items.Add(PulseCatalog.GetString("Sunday"), false);
      
    }

    #endregion // Constructors

    #region Methods

    #endregion // Methods
    
  }
}
