// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class SummaryPage
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
      this.treeView = new System.Windows.Forms.TreeView();
      this.SuspendLayout();
      // 
      // treeView
      // 
      this.treeView.BackColor = System.Drawing.SystemColors.Control;
      this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView.FullRowSelect = true;
      this.treeView.Location = new System.Drawing.Point(0, 0);
      this.treeView.Margin = new System.Windows.Forms.Padding(0);
      this.treeView.Name = "treeView";
      this.treeView.Size = new System.Drawing.Size(387, 254);
      this.treeView.TabIndex = 0;
      // 
      // SummaryPage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.treeView);
      this.Name = "SummaryPage";
      this.Size = new System.Drawing.Size(387, 254);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.TreeView treeView;
  }
}
