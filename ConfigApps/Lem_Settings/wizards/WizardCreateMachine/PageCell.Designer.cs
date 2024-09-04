// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateMachine
{
  partial class PageCell
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
      this.listCell = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // listCell
      // 
      this.listCell.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listCell.Location = new System.Drawing.Point(0, 0);
      this.listCell.Margin = new System.Windows.Forms.Padding(0);
      this.listCell.Name = "listCell";
      this.listCell.SelectedIndex = -1;
      this.listCell.SelectedText = "";
      this.listCell.SelectedValue = null;
      this.listCell.Size = new System.Drawing.Size(350, 250);
      this.listCell.Sorted = true;
      this.listCell.TabIndex = 0;
      // 
      // PageCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listCell);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PageCell";
      this.Size = new System.Drawing.Size(350, 250);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.List.ListTextValue listCell;
  }
}
