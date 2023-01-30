// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lem_MachineStateTemplateGUI
{
  /// <summary>
  /// Description of MachineStateTemplateMainPage.
  /// </summary>
  public partial class MachineStateTemplateMainPage : UserControl
  {
    #region Members
    DateTime? m_endDatetime = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateMainPage).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return selectedd MachineStateTemplate
    /// </summary>
    IMachineStateTemplate SelectedMachineStateTemplate {
      get {
        return machineStateTemplateSelection.SelectedValue;
      }
    }
    
    /// <summary>
    /// Return UTC Begin DateTime
    /// </summary>
    DateTime SelectedBeginDate {
      get {
        return this.beginDateTimePicker.Value.ToUniversalTime();
      }
    }
    
    /// <summary>
    /// Return UTC End DateTime
    /// </summary>
    DateTime? SelectedEndDate {
      get {
        return this.m_endDatetime;
      }
      set {
        this.m_endDatetime = value;
        if(value != null)
          this.endDateTimePicker.Value = value.Value;
      }
    }
    
    /// <summary>
    /// Return selected shift, can be null
    /// </summary>
    IShift SelectedShift {
      get {
        if (shiftCheckBox.Checked) {
          return this.shiftSelection.SelectedShift;
        }
        else {
          return null;
        }
      }
    }
    
    /// <summary>
    /// Return selected machine
    /// Return null if no machine selected
    /// </summary>
    IList<IMachine> SelectedMachines {
      get {
        return this.GetSelectedMachines();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// UC for adding MachineStateTemplateAsociation adding
    /// </summary>
    public MachineStateTemplateMainPage()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    void MachineStateTemplateMainPageLoad(object sender, EventArgs e)
    {
      //I18N
      this.machineStateTemplateGroupBox.Text = PulseCatalog.GetString ("MachineStateTemplateSelection");
      this.shiftGroupBox.Text = PulseCatalog.GetString("ShiftSelection");
      this.addButton.Text = PulseCatalog.GetString("Add");
      this.shiftCheckBox.Text = PulseCatalog.GetString("AddShift");
      this.endDateCheckBox.Text = PulseCatalog.GetString("NoEndDate");
      this.beginDateLabel.Text = PulseCatalog.GetString("From :");
      this.endDateLabel.Text = PulseCatalog.GetString("To :");
      this.forceCheckBox.Text = PulseCatalog.GetString ("ForceReBuildingMachineStateTemplates");
      //
      this.beginDateTimePicker.Format = DateTimePickerFormat.Custom;
      this.endDateTimePicker.Format = DateTimePickerFormat.Custom;
      this.beginDateTimePicker.Value = DateTime.Now;
      this.SelectedEndDate = DateTime.Now.AddDays(1);

      // Machine list
      machineTreeView.ClearOrders();
      machineTreeView.AddOrder("Sort by department", new string[] {"Company", "Department"});
      machineTreeView.AddOrder("Sort by category", new string[] {"Company", "Category"});
      machineTreeView.ClearElements();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          // Insert all machines
          foreach (var machine in machines)
            machineTreeView.AddElement(machine);
        }
      }
      machineTreeView.RefreshTreeview();
      machineTreeView.SelectedOrder = 0;
      
      this.machineStateTemplateSelection.MultiSelect = false;
      this.shiftCheckBox.Checked = false;
      this.shiftSelection.Visible = false;
    }
    
    /// <summary>
    /// Return all selected machine(s)
    /// </summary>
    /// <returns>Set&lt;IMachine&gt;</returns>
    IList<IMachine> GetSelectedMachines()
    {
      IList<IDisplayable> machines = machineTreeView.SelectedElements;
      IList<IMachine> result = new List<IMachine>();
      foreach (var displayable in machines)
        if (displayable is IMachine)
          result.Add(displayable as IMachine);
      
      return (result.Count == 0) ? null : result;
    }
    
    /// <summary>
    /// Check if all Data in UserControl is ok
    /// </summary>
    /// <returns></returns>
    bool CheckFormValid(){
      
      if (SelectedEndDate.HasValue){
        if (DateTime.Compare(SelectedBeginDate, SelectedEndDate.Value) > 0) {
          MessageBox.Show("Start date (From) should be less than end date (To)");
          return false;
        }
      }
      
      if (SelectedMachineStateTemplate == null) {
        MessageBox.Show("Select one MachineStateTemplate");
        return false;
      }
      
      if (SelectedMachines == null) {
        MessageBox.Show("Select at least one machine, department or company");
        return false;
      }
      
      return true;
    }
    
    #endregion // Methods
    
    #region Events
    void EndDateCheckBoxCheckStateChanged(object sender, EventArgs e)
    {
      if(this.endDateCheckBox.Checked){
        this.SelectedEndDate = null;
        this.endDateTimePicker.Enabled = false;
      }
      else {
        this.SelectedEndDate = this.endDateTimePicker.Value;
        this.endDateTimePicker.Enabled = true;
      }
    }
    
    void BeginDateTimePickerValueChanged(object sender, EventArgs e)
    {
      this.endDateTimePicker.MinDate = this.SelectedBeginDate;
    }
    
    void EndDateTimePickerValueChanged(object sender, EventArgs e)
    {
      this.SelectedEndDate = this.endDateTimePicker.Value;
    }
    
    void AddButtonClick(object sender, EventArgs e)
    {
      if(this.CheckFormValid()){
        MachineStateTemplateProgressForm mstpf = new MachineStateTemplateProgressForm();
        mstpf.SelectedMachines = SelectedMachines;
        mstpf.SelectedMachineStateTemplate = SelectedMachineStateTemplate;
        mstpf.SelectedBeginDate = SelectedBeginDate;
        mstpf.SelectedEndDate = SelectedEndDate;
        mstpf.SelectedShift = SelectedShift;
        mstpf.Force = forceCheckBox.Checked;
        mstpf.ShowDialog();
      }
    }
    
    void ShiftCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if(this.shiftCheckBox.Checked == false){
        this.shiftSelection.Visible = false;
      }
      else {
        this.shiftSelection.Visible = true;
      }
    }
    #endregion
    
  }
}
