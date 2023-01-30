// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class TimePeriodOfDayDialog
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
      this.timePeriodOfDaySelection = new Lemoine.DataReferenceControls.TimePeriodOfDaySelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(65, 85);
      this.okButton.TabIndex = 3;
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(146, 85);
      this.cancelButton.TabIndex = 4;
      // 
      // timePeriodOfDaySelection
      // 
      this.timePeriodOfDaySelection.Location = new System.Drawing.Point(12, 12);
      this.timePeriodOfDaySelection.Name = "timePeriodOfDaySelection";
      this.timePeriodOfDaySelection.Nullable = true;
      this.timePeriodOfDaySelection.Size = new System.Drawing.Size(205, 63);
      this.timePeriodOfDaySelection.TabIndex = 5;
      // 
      // TimePeriodOfDayDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(233, 120);
      this.Controls.Add(this.timePeriodOfDaySelection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "TimePeriodOfDayDialog";
      this.Text = "TimeSpanSelectionDialog";
      this.Load += new System.EventHandler(this.TimePeriodOfDayDialogLoad);
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.timePeriodOfDaySelection, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.TimePeriodOfDaySelection timePeriodOfDaySelection;
  }
}
