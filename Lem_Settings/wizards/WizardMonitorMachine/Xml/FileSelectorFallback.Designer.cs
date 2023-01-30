// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

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
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileSelectorFallback));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.textBox = new System.Windows.Forms.TextBox();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.Controls.Add(this.pictureBox, 1, 0);
      this.baseLayout.Controls.Add(this.textBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(150, 22);
      this.baseLayout.TabIndex = 0;
      // 
      // pictureBox
      // 
      this.pictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox.BackgroundImage")));
      this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox.Location = new System.Drawing.Point(124, 0);
      this.pictureBox.Margin = new System.Windows.Forms.Padding(0);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(26, 22);
      this.pictureBox.TabIndex = 0;
      this.pictureBox.TabStop = false;
      // 
      // textBox
      // 
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.Location = new System.Drawing.Point(0, 0);
      this.textBox.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.textBox.Name = "textBox";
      this.textBox.Size = new System.Drawing.Size(121, 20);
      this.textBox.TabIndex = 1;
      // 
      // FileSelectorFallback
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.MinimumSize = new System.Drawing.Size(0, 22);
      this.Name = "FileSelectorFallback";
      this.Size = new System.Drawing.Size(150, 22);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);

    }
  }
}
