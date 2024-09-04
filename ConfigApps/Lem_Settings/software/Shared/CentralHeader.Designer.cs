// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings.Gui.Shared_controls
{
  partial class CentralHeader
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
      this.label = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label
      // 
      this.label.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label.Location = new System.Drawing.Point(0, 0);
      this.label.Margin = new System.Windows.Forms.Padding(0);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(200, 20);
      this.label.TabIndex = 0;
      this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // CentralHeader
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "CentralHeader";
      this.Size = new System.Drawing.Size(200, 20);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label label;
  }
}
