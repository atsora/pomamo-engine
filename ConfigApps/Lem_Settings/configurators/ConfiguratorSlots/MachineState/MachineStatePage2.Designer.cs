// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots.MachineState
{
  partial class MachineStatePage2
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the control.
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
      this.displayableTreeView = new Lemoine.DataReferenceControls.DisplayableTreeView();
      this.SuspendLayout();
      // 
      // displayableTreeView
      // 
      this.displayableTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.displayableTreeView.Location = new System.Drawing.Point(0, 0);
      this.displayableTreeView.Margin = new System.Windows.Forms.Padding(0);
      this.displayableTreeView.MultiSelection = true;
      this.displayableTreeView.Name = "displayableTreeView";
      this.displayableTreeView.Size = new System.Drawing.Size(370, 290);
      this.displayableTreeView.TabIndex = 1;
      // 
      // MachineStatePage2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.displayableTreeView);
      this.Name = "MachineStatePage2";
      this.Controls.SetChildIndex(this.displayableTreeView, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.DisplayableTreeView displayableTreeView;
  }
}
