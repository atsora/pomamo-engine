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
  /// Description of MonitoredMachineDialog.
  /// </summary>
  public partial class MonitoredMachineDialog : OKCancelDialog, IValueDialog<IMonitoredMachine>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MonitoredMachineDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MonitoredMachine a valid value ?
    /// </summary>
    public bool Nullable {
      get { return monitoredMachineSelection1.Nullable; }
      set { monitoredMachineSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  monitoredMachineSelection1.DisplayedProperty; }
      set { monitoredMachineSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected MonitoredMachine or null if no MonitoredMachine is selected
    /// </summary>
    public IMonitoredMachine SelectedValue {
      get
      {
        return this.monitoredMachineSelection1.SelectedMonitoredMachine;
      }
      set {
        this.monitoredMachineSelection1.SelectedMonitoredMachine = value;
      }
    }
    
    /// <summary>
    /// Selected MonitoredMachines or null if no MonitoredMachine is selected
    /// </summary>
    public IList<IMonitoredMachine> SelectedValues {
      get
      {
        return this.monitoredMachineSelection1.SelectedMonitoredMachines;
      }
      set {
        this.monitoredMachineSelection1.SelectedMonitoredMachines = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MonitoredMachineDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("MonitoredMachineDialogTitle");
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
