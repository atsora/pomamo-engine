// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class PasswordDialog
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
      this.passwordLabel = new System.Windows.Forms.Label();
      this.passwordTextBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(124, 82);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(205, 83);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // passwordLabel
      // 
      this.passwordLabel.Location = new System.Drawing.Point(13, 13);
      this.passwordLabel.Name = "passwordLabel";
      this.passwordLabel.Size = new System.Drawing.Size(267, 23);
      this.passwordLabel.TabIndex = 2;
      this.passwordLabel.Text = "passwordLabel";
      // 
      // passwordTextBox
      // 
      this.passwordTextBox.Location = new System.Drawing.Point(13, 40);
      this.passwordTextBox.Name = "passwordTextBox";
      this.passwordTextBox.PasswordChar = '*';
      this.passwordTextBox.Size = new System.Drawing.Size(267, 20);
      this.passwordTextBox.TabIndex = 3;
      // 
      // PasswordDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 118);
      this.Controls.Add(this.passwordTextBox);
      this.Controls.Add(this.passwordLabel);
      this.Name = "PasswordDialog";
      this.Text = "PasswordDialog";
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PasswordDialogKeyDown);
      this.Controls.SetChildIndex(this.passwordLabel, 0);
      this.Controls.SetChildIndex(this.passwordTextBox, 0);
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
    private System.Windows.Forms.TextBox passwordTextBox;
    private System.Windows.Forms.Label passwordLabel;
  }
}
