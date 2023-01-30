// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class EventCncValueInputDialog
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
      this.eventCncValueConfigInput = new Lemoine.ConfigControls.EventCncValueConfigInput();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(162, 139);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(243, 139);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // eventCncValueConfigInput
      // 
      this.eventCncValueConfigInput.Location = new System.Drawing.Point(12, 12);
      this.eventCncValueConfigInput.Name = "eventCncValueConfigInput";
      this.eventCncValueConfigInput.OkButton = null;
      this.eventCncValueConfigInput.SelectedCondition = "";
      this.eventCncValueConfigInput.SelectedMessage = "";
      this.eventCncValueConfigInput.SelectedName = "";
      this.eventCncValueConfigInput.Size = new System.Drawing.Size(314, 121);
      this.eventCncValueConfigInput.TabIndex = 2;
      // 
      // EventCncValueInputDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(330, 174);
      this.Controls.Add(this.eventCncValueConfigInput);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "EventCncValueInputDialog";
      this.Text = "EventCncValueInputDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.eventCncValueConfigInput, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.ConfigControls.EventCncValueConfigInput eventCncValueConfigInput;
    
  }
}
