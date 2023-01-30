// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.StatusControls
{
  partial class ModificationProgressBar
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
      this.analysisProgressBar = new System.Windows.Forms.ProgressBar();
      this.SuspendLayout();
      // 
      // analysisProgressBar
      // 
      this.analysisProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.analysisProgressBar.Location = new System.Drawing.Point(0, 0);
      this.analysisProgressBar.Name = "analysisProgressBar";
      this.analysisProgressBar.Size = new System.Drawing.Size(155, 18);
      this.analysisProgressBar.TabIndex = 0;
      // 
      // ModificationProgressBar
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.analysisProgressBar);
      this.Name = "ModificationProgressBar";
      this.Size = new System.Drawing.Size(155, 18);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.ProgressBar analysisProgressBar;
  }
}
