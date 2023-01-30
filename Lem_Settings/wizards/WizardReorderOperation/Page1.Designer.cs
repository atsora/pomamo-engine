// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardReorderOperation
{
  partial class Page1
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the control.
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
      this.listParts = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // listParts
      // 
      this.listParts.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listParts.Location = new System.Drawing.Point(0, 0);
      this.listParts.Margin = new System.Windows.Forms.Padding(0);
      this.listParts.Name = "listParts";
      this.listParts.SelectedIndex = -1;
      this.listParts.SelectedIndexes = null;
      this.listParts.SelectedText = "";
      this.listParts.SelectedTexts = null;
      this.listParts.SelectedValue = null;
      this.listParts.SelectedValues = null;
      this.listParts.Size = new System.Drawing.Size(370, 290);
      this.listParts.Sorted = true;
      this.listParts.TabIndex = 0;
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listParts);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.List.ListTextValue listParts;
  }
}
