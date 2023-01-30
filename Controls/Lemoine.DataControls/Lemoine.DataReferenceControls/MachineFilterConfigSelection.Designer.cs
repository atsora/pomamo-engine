// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class MachineFilterConfigSelection
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.listBox = new System.Windows.Forms.ListBox();
      this.selectedValueLabel = new System.Windows.Forms.Label();
      this.selectedValueTextBox = new System.Windows.Forms.TextBox();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.listBox);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.selectedValueLabel);
      this.splitContainer1.Panel2.Controls.Add(this.selectedValueTextBox);
      this.splitContainer1.Size = new System.Drawing.Size(405, 273);
      this.splitContainer1.SplitterDistance = 242;
      this.splitContainer1.TabIndex = 0;
      // 
      // listBox
      // 
      this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBox.FormattingEnabled = true;
      this.listBox.Location = new System.Drawing.Point(0, 0);
      this.listBox.Name = "listBox";
      this.listBox.Size = new System.Drawing.Size(405, 242);
      this.listBox.TabIndex = 0;
      this.listBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListBoxMouseDoubleClick);
      // 
      // selectedValueLabel
      // 
      this.selectedValueLabel.Location = new System.Drawing.Point(3, 5);
      this.selectedValueLabel.Name = "selectedValueLabel";
      this.selectedValueLabel.Size = new System.Drawing.Size(92, 17);
      this.selectedValueLabel.TabIndex = 1;
      this.selectedValueLabel.Text = "label1";
      // 
      // selectedValueTextBox
      // 
      this.selectedValueTextBox.Location = new System.Drawing.Point(101, 2);
      this.selectedValueTextBox.Name = "selectedValueTextBox";
      this.selectedValueTextBox.ReadOnly = true;
      this.selectedValueTextBox.Size = new System.Drawing.Size(301, 20);
      this.selectedValueTextBox.TabIndex = 0;
      // 
      // MachineFilterConfigSelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "MachineFilterConfigSelection";
      this.Size = new System.Drawing.Size(405, 273);
      this.Load += new System.EventHandler(this.MachineFilterConfigSelectionLoad);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.Panel2.PerformLayout();
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TextBox selectedValueTextBox;
    private System.Windows.Forms.Label selectedValueLabel;
    private System.Windows.Forms.ListBox listBox;
    private System.Windows.Forms.SplitContainer splitContainer1;

  }
}
