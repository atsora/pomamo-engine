// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class PageComputer
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
      this.listComputers = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // listComputers
      // 
      this.listComputers.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listComputers.Location = new System.Drawing.Point(0, 0);
      this.listComputers.Margin = new System.Windows.Forms.Padding(0);
      this.listComputers.Name = "listComputers";
      this.listComputers.SelectedIndex = -1;
      this.listComputers.SelectedText = "";
      this.listComputers.SelectedValue = null;
      this.listComputers.Size = new System.Drawing.Size(370, 290);
      this.listComputers.Sorted = true;
      this.listComputers.TabIndex = 0;
      // 
      // PageComputer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listComputers);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PageComputer";
      this.Size = new System.Drawing.Size(370, 290);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.List.ListTextValue listComputers;
  }
}
