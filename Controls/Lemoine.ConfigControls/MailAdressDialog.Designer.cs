// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MailAdressDialog
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
      this.fromLabel = new System.Windows.Forms.Label();
      this.toLabel = new System.Windows.Forms.Label();
      this.fromTextBox = new System.Windows.Forms.TextBox();
      this.toTextBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(124, 67);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(205, 67);
      // 
      // fromLabel
      // 
      this.fromLabel.Location = new System.Drawing.Point(12, 9);
      this.fromLabel.Name = "fromLabel";
      this.fromLabel.Size = new System.Drawing.Size(81, 23);
      this.fromLabel.TabIndex = 2;
      this.fromLabel.Text = "label1";
      // 
      // toLabel
      // 
      this.toLabel.Location = new System.Drawing.Point(12, 32);
      this.toLabel.Name = "toLabel";
      this.toLabel.Size = new System.Drawing.Size(81, 23);
      this.toLabel.TabIndex = 3;
      this.toLabel.Text = "label2";
      // 
      // fromTextBox
      // 
      this.fromTextBox.Location = new System.Drawing.Point(99, 6);
      this.fromTextBox.Name = "fromTextBox";
      this.fromTextBox.Size = new System.Drawing.Size(181, 20);
      this.fromTextBox.TabIndex = 4;
      this.fromTextBox.TextChanged += new System.EventHandler(this.FromTextBoxTextChanged);
      // 
      // toTextBox
      // 
      this.toTextBox.Location = new System.Drawing.Point(99, 35);
      this.toTextBox.Name = "toTextBox";
      this.toTextBox.Size = new System.Drawing.Size(181, 20);
      this.toTextBox.TabIndex = 5;
      this.toTextBox.TextChanged += new System.EventHandler(this.ToTextBoxTextChanged);
      // 
      // MailAdressDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 102);
      this.Controls.Add(this.toTextBox);
      this.Controls.Add(this.fromTextBox);
      this.Controls.Add(this.toLabel);
      this.Controls.Add(this.fromLabel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "MailAdressDialog";
      this.Text = "MailAdressDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.fromLabel, 0);
      this.Controls.SetChildIndex(this.toLabel, 0);
      this.Controls.SetChildIndex(this.fromTextBox, 0);
      this.Controls.SetChildIndex(this.toTextBox, 0);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
    private System.Windows.Forms.TextBox toTextBox;
    private System.Windows.Forms.TextBox fromTextBox;
    private System.Windows.Forms.Label toLabel;
    private System.Windows.Forms.Label fromLabel;
  }
}
