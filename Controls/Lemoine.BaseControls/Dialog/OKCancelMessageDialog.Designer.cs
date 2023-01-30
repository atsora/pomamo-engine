// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class OKCancelMessageDialog
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
      this.labelMessage = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(227, 136);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(308, 136);
      // 
      // labelMessage
      // 
      this.labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                  | System.Windows.Forms.AnchorStyles.Left) 
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelMessage.Location = new System.Drawing.Point(12, 9);
      this.labelMessage.Name = "labelMessage";
      this.labelMessage.Size = new System.Drawing.Size(371, 114);
      this.labelMessage.TabIndex = 2;
      // 
      // OKCancelMessageDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(395, 171);
      this.Controls.Add(this.labelMessage);
      this.Name = "OKCancelMessageDialog";
      this.Text = "Form1";
      this.Controls.SetChildIndex(this.labelMessage, 0);
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label labelMessage;
  }
}
