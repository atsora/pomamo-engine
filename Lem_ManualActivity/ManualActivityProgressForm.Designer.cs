// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_ManualActivity
{
  partial class ManualActivityProgressForm
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
      this.label1 = new System.Windows.Forms.Label();
      this.modificationProgressBar1 = new Lemoine.StatusControls.ModificationProgressBar();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(217, 6);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(117, 27);
      this.label1.TabIndex = 0;
      this.label1.Text = "Analysis Progress";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // modificationProgressBar1
      // 
      this.modificationProgressBar1.Location = new System.Drawing.Point(24, 40);
      this.modificationProgressBar1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.modificationProgressBar1.Name = "modificationProgressBar1";
      this.modificationProgressBar1.Size = new System.Drawing.Size(478, 52);
      this.modificationProgressBar1.TabIndex = 1;
      this.modificationProgressBar1.Disposed += new System.EventHandler(this.ModificationProgressBar1Disposed);
      // 
      // ManualActivityProgressForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(530, 102);
      this.Controls.Add(this.modificationProgressBar1);
      this.Controls.Add(this.label1);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "ManualActivityProgressForm";
      this.RightToLeftLayout = true;
      this.Text = "ManualActivityProgressForm";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ManualActivityProgressForm_FormClosed);
      this.Load += new System.EventHandler(this.ManualActivityProgressFormLoad);
      this.ResumeLayout(false);

    }
    private Lemoine.StatusControls.ModificationProgressBar modificationProgressBar1;
    private System.Windows.Forms.Label label1;
  }
}
