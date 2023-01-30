// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class ToolDialog
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
      this.toolSelection1 = new Lemoine.DataReferenceControls.ToolSelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(226, 238);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(307, 238);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // toolSelection1
      // 
      this.toolSelection1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                                             | System.Windows.Forms.AnchorStyles.Left)
                                                                                            | System.Windows.Forms.AnchorStyles.Right)));
      this.toolSelection1.Location = new System.Drawing.Point(12, 12);
      this.toolSelection1.Name = "toolSelection1";
      this.toolSelection1.Size = new System.Drawing.Size(370, 219);
      this.toolSelection1.TabIndex = 2;
      // 
      // ToolDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(394, 273);
      this.Controls.Add(this.toolSelection1);
      this.Name = "ToolDialog";
      this.Text = "ToolDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.toolSelection1, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.ToolSelection toolSelection1;
  }
}
