// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class PageField
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
      this.listFields = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // listFields
      // 
      this.listFields.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listFields.Location = new System.Drawing.Point(0, 0);
      this.listFields.Margin = new System.Windows.Forms.Padding(0);
      this.listFields.Name = "listFields";
      this.listFields.SelectedIndex = -1;
      this.listFields.SelectedIndexes = null;
      this.listFields.SelectedText = "";
      this.listFields.SelectedTexts = null;
      this.listFields.SelectedValue = null;
      this.listFields.SelectedValues = null;
      this.listFields.Size = new System.Drawing.Size(370, 290);
      this.listFields.Sorted = true;
      this.listFields.TabIndex = 0;
      // 
      // PageField
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listFields);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PageField";
      this.Size = new System.Drawing.Size(370, 290);
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.List.ListTextValue listFields;
  }
}
