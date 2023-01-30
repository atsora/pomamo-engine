// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class WeekDayDialog
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
      this.weekDaySelection1 = new Lemoine.DataReferenceControls.WeekDaySelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(124, 270);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(205, 270);
      // 
      // weekDaySelection1
      // 
      this.weekDaySelection1.Location = new System.Drawing.Point(12, 12);
      this.weekDaySelection1.Name = "weekDaySelection1";
      this.weekDaySelection1.SelectedDays = Lemoine.Model.WeekDay.None;
      this.weekDaySelection1.Size = new System.Drawing.Size(272, 244);
      this.weekDaySelection1.TabIndex = 2;
      // 
      // WeekDayDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 305);
      this.Controls.Add(this.weekDaySelection1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "WeekDayDialog";
      this.Text = "WeekDayConfig";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.weekDaySelection1, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.WeekDaySelection weekDaySelection1;
  }
}
