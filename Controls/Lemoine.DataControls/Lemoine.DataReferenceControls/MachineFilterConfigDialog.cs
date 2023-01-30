// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of MachineFilterConfigDialog.
  /// </summary>
  public partial class MachineFilterConfigDialog : OKCancelDialog, IValueDialog<Object[]>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterConfigDialog).FullName);
    
    /// <summary>
    /// Default Constructor
    /// </summary>
    public MachineFilterConfigDialog()
    {
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString ("MachineFilterConfigDialogTitle");
      this.itemRuleComboBox.DataSource = Enum.GetNames(typeof(MachineFilterRule));
    }
    
    void OkButtonClick(object sender, EventArgs e)
    {
      if (this.machineFilterConfigSelection.Value == null) {
        MessageBoxCentered.Show("The selection cannot be empty, double click on the kind of filter you want.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        this.DialogResult = DialogResult.None;
      } else {
        this.DialogResult = DialogResult.OK;
      }
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    
    /// <summary>
    /// Index 0 contain Machine / MachineCategory / MachineSubCategory / Company / Cell / Department
    /// Index 1 contain MachineRule Index 
    /// </summary>
    public object[] SelectedValue {
      get {
        return new Object[]{machineFilterConfigSelection.Value, itemRuleComboBox.SelectedIndex};
      }
      set {;}
    }
    
    /// <summary>
    /// Not Implemented
    /// </summary>
    public System.Collections.Generic.IList<object[]> SelectedValues {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
  }
}
