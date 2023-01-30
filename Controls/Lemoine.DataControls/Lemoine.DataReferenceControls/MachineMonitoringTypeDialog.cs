// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of MachineMonitoringTypeDialog.
  /// </summary>
  public partial class MachineMonitoringTypeDialog : OKCancelDialog, IValueDialog<IMachineMonitoringType>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineMonitoringTypeDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MachineMonitoringType a valid value ?
    /// </summary>
    public bool Nullable {
      get { return machineMonitoringTypeSelection1.Nullable; }
      set { machineMonitoringTypeSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  machineMonitoringTypeSelection1.DisplayedProperty; }
      set { machineMonitoringTypeSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected MachineMonitoringType or null if no MachineMonitoringType is selected
    /// </summary>
    public IMachineMonitoringType SelectedValue {
      get
      {
        return this.machineMonitoringTypeSelection1.SelectedMachineMonitoringType;
      }
      set {
        this.machineMonitoringTypeSelection1.SelectedMachineMonitoringType = value;
      }
    }
    
    /// <summary>
    /// Selected MachineMonitoringTypes or null if no MachineMonitoringType is selected
    /// </summary>
    public IList<IMachineMonitoringType> SelectedValues {
      get
      {
        return this.machineMonitoringTypeSelection1.SelectedMachineMonitoringTypes;
      }
      set {
        this.machineMonitoringTypeSelection1.SelectedMachineMonitoringTypes = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineMonitoringTypeDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("MachineMonitoringTypeDialogTitle");
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
  }
}
