// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class TimeSpanDialog
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
      this.timeSpanSelection = new Lemoine.DataReferenceControls.TimeSpanSelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(15, 82);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(96, 82);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // timeSpanSelection
      // 
      this.timeSpanSelection.Location = new System.Drawing.Point(15, 12);
      this.timeSpanSelection.Name = "timeSpanSelection";
      this.timeSpanSelection.SelectedTimeSpan = System.TimeSpan.Parse("15:43:30");
      this.timeSpanSelection.Size = new System.Drawing.Size(156, 58);
      this.timeSpanSelection.TabIndex = 2;
      // 
      // TimeSpanDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(183, 117);
      this.Controls.Add(this.timeSpanSelection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "TimeSpanDialog";
      this.Text = "TimeSpanDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.timeSpanSelection, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.TimeSpanSelection timeSpanSelection;
  }
}
