// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of WeekDayCreation.
  /// </summary>
  public partial class WeekDaySelection : UserControl
  {
    #region Members
    private System.Windows.Forms.Button m_okButton;
    private WeekDay m_weekDay;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WeekDaySelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return selected WeekDay
    /// </summary>
    public WeekDay SelectedDays{
      get{
        return GetSelectedWeekDay();
      }
      set{ 
        m_weekDay = value;
      }
    }
    
    /// <summary>
    /// The the OkButton from parent Form
    /// Used if Nullable == false
    /// </summary>
    public Button SetOkButton{
      set {
        this.m_okButton = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public WeekDaySelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      weekDayGroupBox.Text = PulseCatalog.GetString ("Day");
      extremeWeekDayGroupBox.Text = PulseCatalog.GetString ("Rule");
      
      this.weekDayCheckedListBox.CheckOnClick = true;
    }

    #endregion // Constructors

    #region Methods
    
    void WeekDaySelectionLoad(object sender, EventArgs e)
    {
      noneCheckBox.Text = Enum.GetName(typeof(WeekDay), WeekDay.None);
      allDaycheckBox.Text = Enum.GetName(typeof(WeekDay), WeekDay.AllDays);
      
      weekDayCheckedListBox.Items.Clear();
      
      foreach(String name in Enum.GetNames(typeof(WeekDay))){
        if(name != "None" && name != "AllDays") {
          weekDayCheckedListBox.Items.Add(PulseCatalog.GetString(name));
        }
      }
      this.SetSelectedWeekDay();
    }
    
    /// <summary>
    /// Return a WeekDay corresponding to selected infos.
    /// </summary>
    /// <returns></returns>
    private WeekDay GetSelectedWeekDay(){
      if(noneCheckBox.Checked) {
        return WeekDay.None;
      }

      if (allDaycheckBox.Checked) {
        return WeekDay.AllDays;
      }

      //TOSEE checkeditem = all checked or checked + intermediate ?
      WeekDay weekDays = WeekDay.None;  
      if (this.weekDayCheckedListBox.CheckedItems.Contains(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Monday)))) {
        weekDays |= WeekDay.Monday;
      }

      if (this.weekDayCheckedListBox.CheckedItems.Contains(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Tuesday)))) {
        weekDays |= WeekDay.Tuesday;
      }

      if (this.weekDayCheckedListBox.CheckedItems.Contains(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Wednesday)))) {
        weekDays |= WeekDay.Wednesday;
      }

      if (this.weekDayCheckedListBox.CheckedItems.Contains(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Thursday)))) {
        weekDays |= WeekDay.Thursday;
      }

      if (this.weekDayCheckedListBox.CheckedItems.Contains(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Friday)))) {
        weekDays |= WeekDay.Friday;
      }

      if (this.weekDayCheckedListBox.CheckedItems.Contains(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Saturday)))) {
        weekDays |= WeekDay.Saturday;
      }

      if (this.weekDayCheckedListBox.CheckedItems.Contains(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Sunday)))) {
        weekDays |= WeekDay.Sunday;
      }

      return weekDays;
    }
    
    /// <summary>
    /// Set a WeekDay for ease existing data modification
    /// </summary>
    private void SetSelectedWeekDay(){
      
      if(m_weekDay.HasFlag(WeekDay.AllDays)){
        allDaycheckBox.Checked = true;
        return;
      }
      else {
        allDaycheckBox.Checked = false;
      }

      if (m_weekDay.HasFlag(WeekDay.Monday)) {
        weekDayCheckedListBox.SetItemChecked(weekDayCheckedListBox.Items.IndexOf(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Monday))),true);
      }

      if (m_weekDay.HasFlag(WeekDay.Tuesday)) {
        weekDayCheckedListBox.SetItemChecked(weekDayCheckedListBox.Items.IndexOf(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Tuesday))),true);
      }

      if (m_weekDay.HasFlag(WeekDay.Wednesday)) {
        weekDayCheckedListBox.SetItemChecked(weekDayCheckedListBox.Items.IndexOf(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Wednesday))),true);
      }

      if (m_weekDay.HasFlag(WeekDay.Thursday)) {
        weekDayCheckedListBox.SetItemChecked(weekDayCheckedListBox.Items.IndexOf(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Thursday))),true);
      }

      if (m_weekDay.HasFlag(WeekDay.Friday)) {
        weekDayCheckedListBox.SetItemChecked(weekDayCheckedListBox.Items.IndexOf(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Friday))),true);
      }

      if (m_weekDay.HasFlag(WeekDay.Saturday)) {
        weekDayCheckedListBox.SetItemChecked(weekDayCheckedListBox.Items.IndexOf(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Saturday))),true);
      }

      if (m_weekDay.HasFlag(WeekDay.Sunday)) {
        weekDayCheckedListBox.SetItemChecked(weekDayCheckedListBox.Items.IndexOf(PulseCatalog.GetString(Enum.GetName(typeof(WeekDay),WeekDay.Sunday))),true);
      }

      if (weekDayCheckedListBox.CheckedItems.Count == 0) {
        noneCheckBox.Checked = true;
      }
    }
    
    /// <summary>
    /// Set all item in CheckedListBox to a State
    /// </summary>
    /// <param name="checkedListBox"></param>
    /// <param name="checkState">True : checked / False : unchecked</param>
    private void SetAllItemToCheckState(CheckedListBox checkedListBox, bool checkState){
      for (int index = 0; index < this.weekDayCheckedListBox.Items.Count; index++) {
        this.weekDayCheckedListBox.SetItemChecked(index,checkState);
      }
    }
    
    #endregion // Methods
    
    void NoneCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if(this.noneCheckBox.Checked){
        this.allDaycheckBox.Checked = false;
        if(this.m_okButton != null) {
          this.m_okButton.Enabled = true;
        }

        weekDayCheckedListBox.Enabled = false;
        SetAllItemToCheckState(this.weekDayCheckedListBox,false);
      }
      else{
        weekDayCheckedListBox.Enabled = true;
      }
    }
    
    void AllDaycheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if(this.allDaycheckBox.Checked){
        this.noneCheckBox.Checked = false;
        if(this.m_okButton != null) {
          this.m_okButton.Enabled = true;
        }

        weekDayCheckedListBox.Enabled = false;
        SetAllItemToCheckState(this.weekDayCheckedListBox,true);
      }
      else{
        weekDayCheckedListBox.Enabled = true;
      }
    }
    
    void WeekDayCheckedListBoxItemCheck(object sender, ItemCheckEventArgs e)
    {
      this.m_okButton.Enabled = true;
    }
    
  }
}
