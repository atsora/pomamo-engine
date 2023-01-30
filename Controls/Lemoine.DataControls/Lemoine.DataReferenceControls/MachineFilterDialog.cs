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
  /// Description of MachineFilterDialog.
  /// </summary>
  public partial class MachineFilterDialog : OKCancelDialog, IValueDialog<IMachineFilter>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MachineFilter a valid value ?
    /// </summary>
    public bool Nullable {
      get { return machineFilterSelection1.Nullable; }
      set { machineFilterSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  machineFilterSelection1.DisplayedProperty; }
      set { machineFilterSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected MachineFilter or null if no MachineFilter is selected
    /// </summary>
    public IMachineFilter SelectedValue {
      get
      {
        return this.machineFilterSelection1.SelectedMachineFilter;
      }
      set {
        this.machineFilterSelection1.SelectedMachineFilter = value;
      }
    }
    
    /// <summary>
    /// Selected MachineFilters or null if no MachineFilter is selected
    /// </summary>
    public IList<IMachineFilter> SelectedValues {
      get {
        return this.machineFilterSelection1.SelectedMachineFilters;
      }
      set {
        this.machineFilterSelection1.SelectedMachineFilters = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineFilterDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("MachineFilterDialogTitle");
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
