// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class PageMachine
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
      this.list = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // list
      // 
      this.list.Dock = System.Windows.Forms.DockStyle.Fill;
      this.list.Location = new System.Drawing.Point(0, 0);
      this.list.Margin = new System.Windows.Forms.Padding(0);
      this.list.Name = "list";
      this.list.Size = new System.Drawing.Size(350, 250);
      this.list.Sorted = true;
      this.list.TabIndex = 0;
      this.list.ItemChanged += new System.Action<string, object>(this.ListItemChanged);
      // 
      // PageMachine
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.list);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PageMachine";
      this.Size = new System.Drawing.Size(350, 250);
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.List.ListTextValue list;
  }
}
