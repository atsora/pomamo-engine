// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class XmlReaderDialog
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.RichTextBox richTextBox;
    
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
      this.richTextBox = new System.Windows.Forms.RichTextBox();
      this.SuspendLayout();
      // 
      // richTextBox
      // 
      this.richTextBox.AcceptsTab = true;
      this.richTextBox.BackColor = System.Drawing.SystemColors.Window;
      this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.richTextBox.Location = new System.Drawing.Point(0, 0);
      this.richTextBox.Margin = new System.Windows.Forms.Padding(0);
      this.richTextBox.Name = "richTextBox";
      this.richTextBox.Size = new System.Drawing.Size(584, 266);
      this.richTextBox.TabIndex = 1;
      this.richTextBox.Text = "";
      // 
      // XmlReaderDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(584, 266);
      this.Controls.Add(this.richTextBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "XmlReaderDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Xml reader";
      this.ResumeLayout(false);

    }
  }
}
