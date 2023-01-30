// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class ReasonGroupSelection
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
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.nullCheckBox = new System.Windows.Forms.CheckBox();
      this.listBox = new System.Windows.Forms.ListBox();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.nullCheckBox, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.listBox, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(342, 234);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // nullCheckBox
      // 
      this.nullCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nullCheckBox.Location = new System.Drawing.Point(3, 207);
      this.nullCheckBox.Name = "nullCheckBox";
      this.nullCheckBox.Size = new System.Drawing.Size(336, 24);
      this.nullCheckBox.TabIndex = 2;
      this.nullCheckBox.Text = "No machine mode";
      this.nullCheckBox.UseVisualStyleBackColor = true;
      this.nullCheckBox.CheckedChanged += new System.EventHandler(this.NullCheckBoxCheckedChanged);
      // 
      // listBox
      // 
      this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBox.Location = new System.Drawing.Point(3, 3);
      this.listBox.Name = "listBox";
      this.listBox.Size = new System.Drawing.Size(336, 198);
      this.listBox.TabIndex = 3;
      this.listBox.SelectedIndexChanged += new System.EventHandler(this.ListBoxSelectedIndexChanged);
      // 
      // ReasonGroupSelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "ReasonGroupSelection";
      this.Size = new System.Drawing.Size(342, 234);
      this.Load += new System.EventHandler(this.ReasonGroupSelectionLoad);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.ListBox listBox;
    private System.Windows.Forms.CheckBox nullCheckBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
