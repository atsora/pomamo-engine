// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class DisclosurePanel
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
      this.headerPanel = new System.Windows.Forms.Panel();
      this.TitleLbl = new System.Windows.Forms.Label();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.contentPanel = new System.Windows.Forms.Panel();
      this.headerPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // headerPanel
      // 
      this.headerPanel.Controls.Add(this.TitleLbl);
      this.headerPanel.Controls.Add(this.pictureBox);
      this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.headerPanel.Location = new System.Drawing.Point(0, 0);
      this.headerPanel.Name = "headerPanel";
      this.headerPanel.Size = new System.Drawing.Size(360, 25);
      this.headerPanel.TabIndex = 1;
      // 
      // TitleLbl
      // 
      this.TitleLbl.AutoSize = true;
      this.TitleLbl.Location = new System.Drawing.Point(21, 5);
      this.TitleLbl.Margin = new System.Windows.Forms.Padding(0);
      this.TitleLbl.Name = "TitleLbl";
      this.TitleLbl.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.TitleLbl.Size = new System.Drawing.Size(40, 13);
      this.TitleLbl.TabIndex = 2;
      this.TitleLbl.Text = "label1";
      this.TitleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.TitleLbl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMouseClick);
      // 
      // pictureBox
      // 
      this.pictureBox.Location = new System.Drawing.Point(5, 4);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(16, 16);
      this.pictureBox.TabIndex = 1;
      this.pictureBox.TabStop = false;
      this.pictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMouseClick);
      // 
      // contentPanel
      // 
      this.contentPanel.AutoScroll = true;
      this.contentPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.contentPanel.Location = new System.Drawing.Point(0, 25);
      this.contentPanel.Name = "contentPanel";
      this.contentPanel.Size = new System.Drawing.Size(360, 50);
      this.contentPanel.TabIndex = 2;
      this.contentPanel.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.ContentPanelControlAdded);
      // 
      // DisclosurePanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.contentPanel);
      this.Controls.Add(this.headerPanel);
      this.Name = "DisclosurePanel";
      this.Size = new System.Drawing.Size(360, 77);
      this.Load += new System.EventHandler(this.DisclosurePanelLoad);
      this.headerPanel.ResumeLayout(false);
      this.headerPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Panel contentPanel;
    private System.Windows.Forms.Panel headerPanel;
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.Label TitleLbl;
  }
}
