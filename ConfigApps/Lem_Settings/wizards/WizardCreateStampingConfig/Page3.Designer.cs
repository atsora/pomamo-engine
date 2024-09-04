// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateStampingConfig
{
  partial class Page3
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing && (components != null)) {
        components.Dispose ();
      }
      base.Dispose (disposing);
    }

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ()
    {
      this.monitoredMachineSelection = new Lemoine.DataReferenceControls.MonitoredMachineSelection ();
      this.SuspendLayout();
      // 
      // monitoredMachineSelection
      // 
      this.monitoredMachineSelection.Dock = System.Windows.Forms.DockStyle.Fill;
      this.monitoredMachineSelection.Location = new System.Drawing.Point (0, 0);
      this.monitoredMachineSelection.Name = "monitoredMachineSelection";
      this.monitoredMachineSelection.Size = new System.Drawing.Size (432, 335);
      this.monitoredMachineSelection.TabIndex = 0;
      this.monitoredMachineSelection.MultiSelect = true;
      this.monitoredMachineSelection.Nullable = true;
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add (this.monitoredMachineSelection);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(448, 350);
      this.ResumeLayout(false);
      this.PerformLayout ();

    }

    private Lemoine.DataReferenceControls.MonitoredMachineSelection monitoredMachineSelection;
  }
}
