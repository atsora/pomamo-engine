// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls.List
{
  partial class ListTextValue
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
      this.listBox = new System.Windows.Forms.ListBox();
      this.SuspendLayout();
      // 
      // listBox
      // 
      this.listBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.listBox.FormattingEnabled = true;
      this.listBox.IntegralHeight = false;
      this.listBox.Location = new System.Drawing.Point(0, 0);
      this.listBox.Margin = new System.Windows.Forms.Padding(0);
      this.listBox.Name = "listBox";
      this.listBox.Size = new System.Drawing.Size(100, 150);
      this.listBox.TabIndex = 0;
      this.listBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBoxDrawItem);
      this.listBox.SelectedIndexChanged += new System.EventHandler(this.ListBoxSelectedIndexChanged);
      this.listBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListBoxMouseDoubleClick);
      // 
      // ListTextValue
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listBox);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "ListTextValue";
      this.Size = new System.Drawing.Size(100, 150);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.ListBox listBox;
  }
}
