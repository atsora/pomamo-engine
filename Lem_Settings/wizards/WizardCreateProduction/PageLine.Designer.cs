// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateProduction
{
  partial class PageLine
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PageLine));
      this.listLine = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // listLine
      // 
      this.listLine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listLine.Location = new System.Drawing.Point(0, 0);
      this.listLine.Margin = new System.Windows.Forms.Padding(0);
      this.listLine.MultipleSelection = false;
      this.listLine.Name = "listLine";
      this.listLine.SelectedIndex = -1;
      this.listLine.SelectedIndexes = ((System.Collections.Generic.IList<int>)(resources.GetObject("listLine.SelectedIndexes")));
      this.listLine.SelectedText = "";
      this.listLine.SelectedTexts = ((System.Collections.Generic.IList<string>)(resources.GetObject("listLine.SelectedTexts")));
      this.listLine.SelectedValue = null;
      this.listLine.SelectedValues = ((System.Collections.Generic.IList<object>)(resources.GetObject("listLine.SelectedValues")));
      this.listLine.Size = new System.Drawing.Size(350, 250);
      this.listLine.Sorted = true;
      this.listLine.TabIndex = 0;
      // 
      // PageLine
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listLine);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PageLine";
      this.Size = new System.Drawing.Size(350, 250);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.List.ListTextValue listLine;
  }
}
