// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateStampingConfig
{
  partial class Page2
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
      this.jsonTextBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // jsonTextBox
      // 
      this.jsonTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.jsonTextBox.Location = new System.Drawing.Point(0, 0);
      this.jsonTextBox.Multiline = true;
      this.jsonTextBox.Name = "jsonTextBox";
      this.jsonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.jsonTextBox.Size = new System.Drawing.Size(432, 335);
      this.jsonTextBox.TabIndex = 0;
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.jsonTextBox);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(432, 335);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private System.Windows.Forms.TextBox jsonTextBox;
  }
}
