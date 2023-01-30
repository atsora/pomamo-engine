// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class OrderDialog
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
      this.orderSelection = new Lemoine.DataReferenceControls.OrderSelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(16, 117);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(97, 117);
      // 
      // orderSelection
      // 
      this.orderSelection.Location = new System.Drawing.Point(27, 12);
      this.orderSelection.Name = "orderSelection";
      this.orderSelection.Nullable = false;
      this.orderSelection.Size = new System.Drawing.Size(128, 100);
      this.orderSelection.TabIndex = 2;
      // 
      // OrderDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(184, 152);
      this.Controls.Add(this.orderSelection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "OrderDialog";
      this.Text = "OrderDialog";
      this.InputLanguageChanging += new System.Windows.Forms.InputLanguageChangingEventHandler(this.OrderDialogInputLanguageChanging);
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.orderSelection, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.OrderSelection orderSelection;
    
    void OrderDialogInputLanguageChanging(object sender, System.Windows.Forms.InputLanguageChangingEventArgs e)
    {
      
    }
  }
}
