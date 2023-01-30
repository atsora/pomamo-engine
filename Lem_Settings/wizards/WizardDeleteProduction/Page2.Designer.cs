// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardDeleteProduction
{
  partial class Page2
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page2));
      this.listProductions = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // listProductions
      // 
      this.listProductions.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listProductions.Location = new System.Drawing.Point(0, 0);
      this.listProductions.Margin = new System.Windows.Forms.Padding(0);
      this.listProductions.MultipleSelection = true;
      this.listProductions.Name = "listProductions";
      this.listProductions.SelectedIndex = -1;
      this.listProductions.SelectedIndexes = ((System.Collections.Generic.IList<int>)(resources.GetObject("listProductions.SelectedIndexes")));
      this.listProductions.SelectedText = "";
      this.listProductions.SelectedTexts = ((System.Collections.Generic.IList<string>)(resources.GetObject("listProductions.SelectedTexts")));
      this.listProductions.SelectedValue = null;
      this.listProductions.SelectedValues = ((System.Collections.Generic.IList<object>)(resources.GetObject("listProductions.SelectedValues")));
      this.listProductions.Size = new System.Drawing.Size(370, 290);
      this.listProductions.Sorted = true;
      this.listProductions.TabIndex = 0;
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listProductions);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.List.ListTextValue listProductions;
  }
}
