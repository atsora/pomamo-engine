// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class MonitoredMachineDialog
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the form.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (components != null) {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }
    
    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent()
    {
      this.monitoredMachineSelection1 = new Lemoine.DataReferenceControls.MonitoredMachineSelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(226, 238);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(307, 238);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // monitoredMachineSelection1
      // 
      this.monitoredMachineSelection1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                                             | System.Windows.Forms.AnchorStyles.Left)
                                                                                            | System.Windows.Forms.AnchorStyles.Right)));
      this.monitoredMachineSelection1.Location = new System.Drawing.Point(12, 12);
      this.monitoredMachineSelection1.Name = "monitoredMachineSelection1";
      this.monitoredMachineSelection1.Size = new System.Drawing.Size(370, 219);
      this.monitoredMachineSelection1.TabIndex = 2;
      // 
      // MonitoredMachineDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(394, 273);
      this.Controls.Add(this.monitoredMachineSelection1);
      this.Name = "MonitoredMachineDialog";
      this.Text = "MonitoredMachineDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.monitoredMachineSelection1, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.MonitoredMachineSelection monitoredMachineSelection1;
  }
}
