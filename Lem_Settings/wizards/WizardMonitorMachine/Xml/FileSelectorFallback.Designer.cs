// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Drawing;

namespace WizardMonitorMachine
{
  partial class FileSelectorFallback
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.TextBox textBox;
    
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
    private void InitializeComponent ()
    {
      baseLayout = new System.Windows.Forms.TableLayoutPanel ();
      pictureBox = new System.Windows.Forms.PictureBox ();
      textBox = new System.Windows.Forms.TextBox ();
      baseLayout.SuspendLayout ();
      ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit ();
      SuspendLayout ();
      // 
      // baseLayout
      // 
      baseLayout.ColumnCount = 2;
      baseLayout.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 100F));
      baseLayout.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Absolute, 30F));
      baseLayout.Controls.Add (pictureBox, 1, 0);
      baseLayout.Controls.Add (textBox, 0, 0);
      baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      baseLayout.Location = new System.Drawing.Point (0, 0);
      baseLayout.Margin = new System.Windows.Forms.Padding (0);
      baseLayout.Name = "baseLayout";
      baseLayout.RowCount = 1;
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 100F));
      baseLayout.Size = new System.Drawing.Size (175, 25);
      baseLayout.TabIndex = 0;
      // 
      // pictureBox
      // 
      pictureBox.BackgroundImage = new Bitmap ("fallback.png");
      pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      pictureBox.Location = new System.Drawing.Point (145, 0);
      pictureBox.Margin = new System.Windows.Forms.Padding (0);
      pictureBox.Name = "pictureBox";
      pictureBox.Size = new System.Drawing.Size (30, 25);
      pictureBox.TabIndex = 0;
      pictureBox.TabStop = false;
      // 
      // textBox
      // 
      textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      textBox.Location = new System.Drawing.Point (0, 0);
      textBox.Margin = new System.Windows.Forms.Padding (0, 0, 4, 0);
      textBox.Name = "textBox";
      textBox.Size = new System.Drawing.Size (141, 23);
      textBox.TabIndex = 1;
      // 
      // FileSelectorFallback
      // 
      AutoScaleDimensions = new System.Drawing.SizeF (7F, 15F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      Controls.Add (baseLayout);
      Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      MinimumSize = new System.Drawing.Size (0, 25);
      Name = "FileSelectorFallback";
      Size = new System.Drawing.Size (175, 25);
      baseLayout.ResumeLayout (false);
      baseLayout.PerformLayout ();
      ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit ();
      ResumeLayout (false);
    }
  }
}
