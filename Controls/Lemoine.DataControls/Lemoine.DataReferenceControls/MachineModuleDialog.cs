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
  /// Description of MachineModuleDialog.
  /// </summary>
  public partial class MachineModuleDialog : OKCancelDialog, IValueDialog<IMachineModule>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModuleDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MachineModule a valid value ?
    /// </summary>
    public bool Nullable {
      get { return machineModuleSelection1.Nullable; }
      set { machineModuleSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  machineModuleSelection1.DisplayedProperty; }
      set { machineModuleSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected MachineModule or null if no MachineModule is selected
    /// </summary>
    public IMachineModule SelectedValue {
      get
      {
       return this.machineModuleSelection1.SelectedMachineModule;
      }
      set {
        this.machineModuleSelection1.SelectedMachineModule = value;
      }
    }
    
    /// <summary>
    /// Selected MachineModules or null if no MachineModule is selected
    /// </summary>
    public IList<IMachineModule> SelectedValues {
      get
      {
       return this.machineModuleSelection1.SelectedMachineModules;
      }
      set {
        this.machineModuleSelection1.SelectedMachineModules = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineModuleDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("MachineModuleDialogTitle");
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
