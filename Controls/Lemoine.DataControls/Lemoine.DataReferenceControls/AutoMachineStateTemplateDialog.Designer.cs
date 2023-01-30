// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class AutoMachineStateTemplateDialog
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
      this.autoMachineStateTemplateSelection = new Lemoine.DataReferenceControls.AutoMachineStateTemplateSelection();
      this.SuspendLayout();
      // 
      // autoMachineStateTemplateSelection
      // 
      this.autoMachineStateTemplateSelection.Location = new System.Drawing.Point(12, 12);
      this.autoMachineStateTemplateSelection.Name = "autoMachineStateTemplateSelection";
      this.autoMachineStateTemplateSelection.Nullable = true;
      this.autoMachineStateTemplateSelection.SelectedT = null;
      this.autoMachineStateTemplateSelection.Size = new System.Drawing.Size(268, 220);
      this.autoMachineStateTemplateSelection.TabIndex = 2;
      // 
      // UserDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 273);
      this.Controls.Add(this.autoMachineStateTemplateSelection);
      this.Name = "AutoMachineStateTemplateDialog";
      this.Text = "AutoMachineStateTemplateDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.autoMachineStateTemplateSelection, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.AutoMachineStateTemplateSelection autoMachineStateTemplateSelection;
  }
}