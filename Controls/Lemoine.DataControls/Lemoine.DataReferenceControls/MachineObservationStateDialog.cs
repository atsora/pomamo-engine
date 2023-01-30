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
  /// Description of MachineObservationStateDialog.
  /// </summary>
  public partial class MachineObservationStateDialog : OKCancelDialog, IValueDialog<IMachineObservationState>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineObservationStateDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MachineObservationState a valid value ?
    /// </summary>
    public bool Nullable {
      get { return machineObservationStateSelection1.Nullable; }
      set { machineObservationStateSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  machineObservationStateSelection1.DisplayedProperty; }
      set { machineObservationStateSelection1.DisplayedProperty = value; }
    }    
    
    /// <summary>
    /// Selected MachineObservationState or null if no MachineObservationState is selected
    /// </summary>
    public IMachineObservationState SelectedValue {
      get
      {
        return this.machineObservationStateSelection1.SelectedMachineObservationState;
      }
      set {
        this.machineObservationStateSelection1.SelectedMachineObservationState = value;
      }
    }
    
    /// <summary>
    /// Selected MachineObservationStates or null if no MachineObservationState is selected
    /// </summary>
    public IList<IMachineObservationState> SelectedValues {
      get
      {
        return this.machineObservationStateSelection1.SelectedMachineObservationStates;
      }
      set {
        this.machineObservationStateSelection1.SelectedMachineObservationStates = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineObservationStateDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("MachineObservationStateDialogTitle");
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
