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
  /// Description of MachineFilterConfigSelection.
  /// Master DialogBox implementing all FilterItem Dialog.
  /// Return only one value.
  /// 
  /// Impl. Dialogs :
  /// - MachineDialog
  /// - MachineCategoryDialog
  /// - MachineSubCategoryDialog
  /// - CellDialog
  /// - CompanyDialog
  /// - DepartmentDialog
  /// </summary>
  public partial class MachineFilterConfigSelection : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterConfigSelection).FullName);
    
    /// <summary>
    /// Return the Index from selected item of the listBox.
    /// </summary>
    public int selectedTypeOfFilter {
      get {
        return listBox.SelectedIndex;
      }
    }
    
    /// <summary>
    /// Return a DialogResult from invoked Dialog.
    /// </summary>
    public DialogResult Result { get; private set; }
    
    /// <summary>
    /// Return a selected Machine, MachineCategory, MachineSubCategory
    /// Company, Cell, Department.
    /// </summary>
    public Object Value { get; private set; }
    
    /// <summary>
    /// Default Constructor
    /// </summary>
    public MachineFilterConfigSelection()
    {
      InitializeComponent();
      
      this.selectedValueLabel.Text = PulseCatalog.GetString ("MachineFilterConfigSelectionLabel");
    }
    
    void MachineFilterConfigSelectionLoad(object sender, EventArgs e)
    {
      listBox.DataSource = Enum.GetValues(typeof(MachineFilterConfigEnum));
    }
    
    void ListBoxMouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      int index = this.listBox.IndexFromPoint(e.Location);
      if (index != System.Windows.Forms.ListBox.NoMatches)
      {
        this.launchSelectedDialog(index);
      }
    }
    
    /// <summary>
    /// Launch a Dialog based on index of selectedItem from Listbox
    /// </summary>
    /// <param name="index"></param>
    public void launchSelectedDialog(int index) {
      object value = null;
      
      switch(index) {
          case (int)MachineFilterConfigEnum.Machine: {
            var dialog = new MachineDialog();
            Result = dialog.ShowDialog();
            value = dialog.SelectedValue;
            break;
          }
          case (int)MachineFilterConfigEnum.MachineCategory:{
            var dialog = new MachineCategoryDialog();
            Result = dialog.ShowDialog();
            value = dialog.SelectedValue;
            break;
          }
          case (int)MachineFilterConfigEnum.MachineSubCategory:{
            var dialog = new MachineSubCategoryDialog();
            Result = dialog.ShowDialog();
            value = dialog.SelectedValue;
            break;
          }
          case (int)MachineFilterConfigEnum.Cell:{
            var dialog = new CellDialog();
            Result = dialog.ShowDialog();
            value = dialog.SelectedValue;
            break;
          }
          case (int)MachineFilterConfigEnum.Company:{
            var dialog = new CompanyDialog();
            Result = dialog.ShowDialog();
            value = dialog.SelectedValue;
            break;
          }
          case (int)MachineFilterConfigEnum.Department:{
            var dialog = new DepartmentDialog();
            Result = dialog.ShowDialog();
            value = dialog.SelectedValue;
            break;
          }
        default:
          break;
      }
      
      this.selectedValueTextBox.Text = "";
      if (value is IMachine) {
        var machine = value as IMachine;
        this.selectedValueTextBox.Text = machine.Name;
      }
      if (value is IMachineCategory) {
        var machineCategory = value as IMachineCategory;
        this.selectedValueTextBox.Text = machineCategory.Name;
      }
      if (value is IMachineSubCategory) {
        var machineSubCategory = value as IMachineSubCategory;
        this.selectedValueTextBox.Text = machineSubCategory.Name;
      }
      if (value is ICell) {
        var cell = value as ICell;
        this.selectedValueTextBox.Text = cell.Name;
      }
      if (value is ICompany) {
        var company = value as ICompany;
        this.selectedValueTextBox.Text = company.Name;
      }
      if (value is IDepartment) {
        var department = value as IDepartment;
        this.selectedValueTextBox.Text = department.Name;
      }
      
      if (this.selectedValueTextBox.Text != "") {
        Value = value;
      }
    }
  }
}
