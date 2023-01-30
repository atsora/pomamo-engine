// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class EventLevelDialog
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
      this.eventLevelSelection = new Lemoine.DataReferenceControls.EventLevelSelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(153, 220);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(234, 220);
      // 
      // eventLevelSelection
      // 
      this.eventLevelSelection.DisplayedProperty = "SelectionText";
      this.eventLevelSelection.Location = new System.Drawing.Point(12, 12);
      this.eventLevelSelection.MultiSelect = false;
      this.eventLevelSelection.Name = "eventLevelSelection";
      this.eventLevelSelection.Nullable = false;
      this.eventLevelSelection.Size = new System.Drawing.Size(297, 202);
      this.eventLevelSelection.TabIndex = 2;
      // 
      // EventLevelDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(321, 255);
      this.Controls.Add(this.eventLevelSelection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "EventLevelDialog";
      this.Text = "EventLevelDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.eventLevelSelection, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.EventLevelSelection eventLevelSelection;
  }
}
