// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;

namespace WizardEventLongPeriod
{
  /// <summary>
  /// Description of MachineModeCell.
  /// </summary>
  public partial class MachineModeCell : UserControl
  {
    #region Getters / Setters
    /// <summary>
    /// Machine mode represented
    /// </summary>
    public IMachineMode MachineMode { get; private set; }
    
    /// <summary>
    /// Get / set the checked state
    /// </summary>
    public bool Checked {
      get { return checkBox.Checked; }
      set { checkBox.Checked = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineModeCell(IMachineMode machineMode)
    {
      MachineMode = machineMode;
      InitializeComponent();
      labelModeName.Text = MachineMode.Display;
      marker.Brush = new SolidBrush(ColorTranslator.FromHtml(MachineMode.Color));
      if (MachineMode.Running.HasValue) {
        labelRunning.Text = MachineMode.Running.Value ? "running" : "idle";
      }
      else {
        labelRunning.Text = "unknown";
      }
    }
    #endregion // Constructors
  }
}
