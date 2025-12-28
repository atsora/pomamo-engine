// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class ProductionStateDialog
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
      this.productionStateSelection1 = new Lemoine.DataReferenceControls.ProductionStateSelection();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(29, 227);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(110, 227);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // productionStateSelection1
      // 
      this.productionStateSelection1.Location = new System.Drawing.Point(12, 12);
      this.productionStateSelection1.Name = "productionStateSelection1";
      this.productionStateSelection1.Size = new System.Drawing.Size(192, 209);
      this.productionStateSelection1.TabIndex = 2;
      // 
      // ProductionStateDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(216, 262);
      this.Controls.Add(this.productionStateSelection1);
      this.Name = "ProductionStateDialog";
      this.Text = "ProductionStateDialog";
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.productionStateSelection1, 0);
      this.ResumeLayout(false);
    }
    private Lemoine.DataReferenceControls.ProductionStateSelection productionStateSelection1;
  }
}