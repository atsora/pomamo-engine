// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class DateTimeDialog
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
      this.dateTimeSelection = new Lemoine.DataReferenceControls.DateTimeSelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(105, 268);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(186, 268);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // dateTimeSelection
      // 
      this.dateTimeSelection.Location = new System.Drawing.Point(12, 12);
      this.dateTimeSelection.Name = "dateTimeSelection";
      this.dateTimeSelection.Size = new System.Drawing.Size(260, 250);
      this.dateTimeSelection.TabIndex = 2;
      // 
      // DateTimeDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(273, 303);
      this.Controls.Add(this.dateTimeSelection);
      this.Name = "DateTimeDialog";
      this.Text = "DateTimeDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.dateTimeSelection, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.DateTimeSelection dateTimeSelection;
  }
}