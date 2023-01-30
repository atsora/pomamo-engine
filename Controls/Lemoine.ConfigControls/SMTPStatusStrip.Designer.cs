// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class SMTPStatusStrip
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
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.smtpStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.smtpStatusLed = new System.Windows.Forms.ToolStripStatusLabel();
      this.smtpStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
      this.sendEmailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.moreInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                  this.smtpStatusLabel,
                  this.smtpStatusLed,
                  this.smtpStripSplitButton});
      this.statusStrip1.Location = new System.Drawing.Point(0, 2);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.ShowItemToolTips = true;
      this.statusStrip1.Size = new System.Drawing.Size(575, 22);
      this.statusStrip1.TabIndex = 0;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // smtpStatusLabel
      // 
      this.smtpStatusLabel.Name = "smtpStatusLabel";
      this.smtpStatusLabel.Size = new System.Drawing.Size(94, 17);
      this.smtpStatusLabel.Text = "smtpStatusLabel";
      // 
      // smtpStatusLed
      // 
      this.smtpStatusLed.Name = "smtpStatusLed";
      this.smtpStatusLed.Size = new System.Drawing.Size(23, 17);
      this.smtpStatusLed.Text = "OK";
      // 
      // smtpStripSplitButton
      // 
      this.smtpStripSplitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.smtpStripSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.smtpStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                  this.sendEmailToolStripMenuItem,
                  this.moreInformationToolStripMenuItem});
      this.smtpStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.smtpStripSplitButton.Name = "smtpStripSplitButton";
      this.smtpStripSplitButton.Size = new System.Drawing.Size(16, 20);
      this.smtpStripSplitButton.Text = "toolStripSplitButton1";
      // 
      // sendEmailToolStripMenuItem
      // 
      this.sendEmailToolStripMenuItem.Name = "sendEmailToolStripMenuItem";
      this.sendEmailToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
      this.sendEmailToolStripMenuItem.Text = "Send Email";
      this.sendEmailToolStripMenuItem.Click += new System.EventHandler(this.SendEmailToolStripMenuItemClick);
      // 
      // moreInformationToolStripMenuItem
      // 
      this.moreInformationToolStripMenuItem.Name = "moreInformationToolStripMenuItem";
      this.moreInformationToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
      this.moreInformationToolStripMenuItem.Text = "More Information";
      this.moreInformationToolStripMenuItem.Click += new System.EventHandler(this.MoreInformationToolStripMenuItemClick);
      // 
      // SMTPStatusStrip
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CausesValidation = false;
      this.Controls.Add(this.statusStrip1);
      this.Name = "SMTPStatusStrip";
      this.Size = new System.Drawing.Size(575, 24);
      this.Load += new System.EventHandler(this.SMTPStatusStripLoad);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
    private System.Windows.Forms.ToolStripMenuItem moreInformationToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem sendEmailToolStripMenuItem;
    private System.Windows.Forms.ToolStripSplitButton smtpStripSplitButton;
    private System.Windows.Forms.ToolStripStatusLabel smtpStatusLed;
    private System.Windows.Forms.ToolStripStatusLabel smtpStatusLabel;
    private System.Windows.Forms.StatusStrip statusStrip1;
  }
}
