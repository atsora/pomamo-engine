// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class ErrorDialog
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.RichTextBox richText;
    private System.Windows.Forms.Button buttonQuit;
    private System.Windows.Forms.RichTextBox richTextLabel;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDialog));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.richText = new System.Windows.Forms.RichTextBox();
      this.buttonQuit = new System.Windows.Forms.Button();
      this.richTextLabel = new System.Windows.Forms.RichTextBox();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 121F));
      this.baseLayout.Controls.Add(this.pictureBox, 0, 0);
      this.baseLayout.Controls.Add(this.richText, 0, 1);
      this.baseLayout.Controls.Add(this.buttonQuit, 2, 2);
      this.baseLayout.Controls.Add(this.richTextLabel, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
      this.baseLayout.Size = new System.Drawing.Size(464, 322);
      this.baseLayout.TabIndex = 0;
      // 
      // pictureBox
      // 
      this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox.Image")));
      this.pictureBox.Location = new System.Drawing.Point(3, 3);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(69, 69);
      this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.pictureBox.TabIndex = 0;
      this.pictureBox.TabStop = false;
      // 
      // richText
      // 
      this.baseLayout.SetColumnSpan(this.richText, 3);
      this.richText.DetectUrls = false;
      this.richText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richText.Location = new System.Drawing.Point(3, 78);
      this.richText.Name = "richText";
      this.richText.ReadOnly = true;
      this.richText.Size = new System.Drawing.Size(458, 212);
      this.richText.TabIndex = 2;
      this.richText.Text = "";
      // 
      // buttonQuit
      // 
      this.buttonQuit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonQuit.Location = new System.Drawing.Point(346, 293);
      this.buttonQuit.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.buttonQuit.Name = "buttonQuit";
      this.buttonQuit.Size = new System.Drawing.Size(115, 26);
      this.buttonQuit.TabIndex = 3;
      this.buttonQuit.Text = "Quit";
      this.buttonQuit.UseVisualStyleBackColor = true;
      this.buttonQuit.Click += new System.EventHandler(this.ButtonQuitClick);
      // 
      // richTextLabel
      // 
      this.richTextLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.baseLayout.SetColumnSpan(this.richTextLabel, 2);
      this.richTextLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextLabel.Location = new System.Drawing.Point(78, 24);
      this.richTextLabel.Margin = new System.Windows.Forms.Padding(3, 24, 3, 3);
      this.richTextLabel.Name = "richTextLabel";
      this.richTextLabel.ReadOnly = true;
      this.richTextLabel.Size = new System.Drawing.Size(383, 48);
      this.richTextLabel.TabIndex = 4;
      this.richTextLabel.Text = "An error occured. Details:";
      this.richTextLabel.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RichTextLabelLinkClicked);
      // 
      // ErrorDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(464, 322);
      this.Controls.Add(this.baseLayout);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ErrorDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Oops";
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);

    }
  }
}
