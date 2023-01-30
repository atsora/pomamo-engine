// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_ApplyMachineModifications
{
  partial class MainForm
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
      this.label1.Location = new System.Drawing.Point(98, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "Analysis Progress";
      // 
      // modificationProgressBar1
      // 
      this.modificationProgressBar1.Location = new System.Drawing.Point(12, 40);
      this.modificationProgressBar1.Name = "modificationProgressBar1";
      this.modificationProgressBar1.Size = new System.Drawing.Size(279, 45);
      this.modificationProgressBar1.TabIndex = 1;
      this.modificationProgressBar1.Disposed += new System.EventHandler(this.ProgressBarDisposed);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(306, 97);
      this.Controls.Add(this.modificationProgressBar1);
      this.Controls.Add(this.label1);
      this.Name = "MainForm";
      this.Text = "Lem_ApplyMachineModifications";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.ResumeLayout(false);

    }
    private Lemoine.StatusControls.ModificationProgressBar modificationProgressBar1;
    private System.Windows.Forms.Label label1;
  }
}
