// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardDeleteProduction
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page1));
      this.listLines = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // listLines
      // 
      this.listLines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listLines.Location = new System.Drawing.Point(0, 0);
      this.listLines.Margin = new System.Windows.Forms.Padding(0);
      this.listLines.MultipleSelection = false;
      this.listLines.Name = "listLines";
      this.listLines.SelectedIndex = -1;
      this.listLines.SelectedIndexes = ((System.Collections.Generic.IList<int>)(resources.GetObject("listLines.SelectedIndexes")));
      this.listLines.SelectedText = "";
      this.listLines.SelectedTexts = ((System.Collections.Generic.IList<string>)(resources.GetObject("listLines.SelectedTexts")));
      this.listLines.SelectedValue = null;
      this.listLines.SelectedValues = ((System.Collections.Generic.IList<object>)(resources.GetObject("listLines.SelectedValues")));
      this.listLines.Size = new System.Drawing.Size(370, 290);
      this.listLines.Sorted = true;
      this.listLines.TabIndex = 0;
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listLines);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.List.ListTextValue listLines;
  }
}
