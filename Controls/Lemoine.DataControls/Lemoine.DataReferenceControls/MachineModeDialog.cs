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
  /// Description of MachineModeDialog.
  /// </summary>
  public partial class MachineModeDialog : OKCancelDialog, IValueDialog<IMachineMode>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MachineMode a valid value ?
    /// </summary>
    public bool Nullable {
      get { return machineModeSelection1.Nullable; }
      set { machineModeSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  machineModeSelection1.DisplayedProperty; }
      set { machineModeSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected MachineMode or null if no MachineMode is selected
    /// 
    /// Setable selection
    /// </summary>
    public IMachineMode SelectedValue {
      get
      {
        return this.machineModeSelection1.SelectedMachineMode;
      }
      set {
        this.machineModeSelection1.SelectedMachineMode = value;
      }
    }
    
    /// <summary>
    /// Selected MachineModes or null if no MachineModes is selected
    /// 
    /// Setable selection
    /// </summary>
    public IList<IMachineMode> SelectedValues {
      get {
        return this.machineModeSelection1.SelectedMachineModes;
      }
      set {
        this.machineModeSelection1.SelectedMachineModes = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineModeDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("MachineModeDialogTitle");
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
