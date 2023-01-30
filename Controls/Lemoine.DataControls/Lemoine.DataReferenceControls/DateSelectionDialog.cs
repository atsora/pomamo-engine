// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of DateTimeDialog.
  /// </summary>
  public partial class DateSelectionDialog : OKCancelDialog, IValueDialog<DateTime>
  {
    #region Members
    bool m_nullable;
    bool m_multiSelet;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DateSelectionDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null DateTime a valid value ?
    /// </summary>
    public bool Nullable {
      get { return this.m_nullable; }
      set { this.SetNullable(value);}
    }
    
    /// <summary>
    /// allow/disallow multi-selection
    /// </summary>
    public bool MultiSelect {
      get { return this.m_multiSelet; }
      set { SetMultiSelect(value); }
    }
    
    /// <summary>
    /// Sets the maximum number of days that can be selected in a month calendar control
    /// </summary>
    public int MaxSelectionCount{
      set {
        this.monthCalendar.MaxSelectionCount = value;
      }
      get {
        return this.monthCalendar.MaxSelectionCount;
      }
    }
    
    /// <summary>
    /// Set the first day of the week as displayed in the month calendar.
    /// </summary>
    public Day FirstDayOfWeek{
      set{
        this.monthCalendar.FirstDayOfWeek = value;
      }
      get{
        return this.monthCalendar.FirstDayOfWeek;
      }
    }
    
    /// <summary>
    /// Selected Date
    /// </summary>
    public DateTime SelectedValue{
      get {
        return this.monthCalendar.SelectionStart.Date;
      }
      set {
        this.monthCalendar.SelectionStart = value;
        this.monthCalendar.SelectionEnd = value;
      }
    }
    
    /// <summary>
    /// Use SelectionRange SelectedValues for multi date
    /// </summary>
    public IList<DateTime> SelectedValues {
      get {
        return new List<DateTime>(){ this.SelectedRange.Start, this.SelectedRange.End};
      }
      set {
        IList<DateTime> dateTimes = value as List<DateTime>;
        if(dateTimes != null && dateTimes.Count >= 1) {
          this.SelectedRange = new SelectionRange(dateTimes[0], dateTimes[1]);
        }
      }
    }
    
    /// <summary>
    /// Selected Range of Date
    /// </summary>
    public SelectionRange SelectedRange{
      get {
        return this.monthCalendar.SelectionRange;
      }
      set {
        this.monthCalendar.SelectionRange = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Create a MonthCalendar Selection Dialog
    /// </summary>
    public DateSelectionDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      this.Text = PulseCatalog.GetString ("DateSelectionDialogTitle");
    }
    
    /// <summary>
    /// Create a MonthCalendar Selection Dialog
    /// </summary>
    /// <param name="minDate">Set the minimum allowable date.</param>
    /// <param name="maxDate">Set the maximum allowable date</param>
    public DateSelectionDialog(DateTime minDate, DateTime maxDate){
      InitializeComponent();
      this.Text = PulseCatalog.GetString ("DateSelectionDialogTitle");
      
      this.monthCalendar.MinDate = minDate;
      this.monthCalendar.MaxDate = maxDate;
    }
    
    /// <summary>
    /// Create a MonthCalendar Selection Dialog
    /// </summary>
    /// <param name="minDate">Set the minimum allowable date.</param>
    /// <param name="maxDate">Set the maximum allowable date</param>
    /// <param name="maxSelectionCount">Sets the maximum number of days that can be selected in a month calendar control</param>
    public DateSelectionDialog(DateTime minDate, DateTime maxDate, int maxSelectionCount){
      InitializeComponent();
      this.Text = PulseCatalog.GetString ("DateSelectionDialogTitle");
      
      this.monthCalendar.MinDate = minDate;
      this.monthCalendar.MaxDate = maxDate;
      this.monthCalendar.MaxSelectionCount = maxSelectionCount;
      this.MultiSelect = maxSelectionCount > 1 ? true : false;
    }

    #endregion // Constructors

    #region Methods
    private void SetNullable(bool value){
      m_nullable = value;
      if(!value){
        this.okButton.Enabled = false;
      }
    }
    
    private void SetMultiSelect(bool value){
      m_multiSelet = value;
      if(!value){
        this.monthCalendar.MaxSelectionCount = 1;
      }
    }
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    
    void MonthCalendarDateSelected(object sender, DateRangeEventArgs e)
    {
      this.okButton.Enabled = true;
    }
    
    #endregion // Methods
  }
}
