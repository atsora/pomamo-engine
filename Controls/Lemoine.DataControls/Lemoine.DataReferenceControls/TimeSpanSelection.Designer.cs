// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class TimeSpanSelection
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
      this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.nullCheckBox, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.dateTimePicker, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(99, 58);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // nullCheckBox
      // 
      this.nullCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
      this.nullCheckBox.Location = new System.Drawing.Point(3, 31);
      this.nullCheckBox.Name = "nullCheckBox";
      this.nullCheckBox.Size = new System.Drawing.Size(278, 24);
      this.nullCheckBox.TabIndex = 2;
      this.nullCheckBox.Text = "No Time";
      this.nullCheckBox.UseVisualStyleBackColor = true;
      this.nullCheckBox.CheckedChanged += new System.EventHandler(this.NullCheckBoxCheckedChanged);
      // 
      // dateTimePicker
      // 
      this.dateTimePicker.Checked = false;
      this.dateTimePicker.Dock = System.Windows.Forms.DockStyle.Left;
      this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
      this.dateTimePicker.Location = new System.Drawing.Point(3, 3);
      this.dateTimePicker.Name = "dateTimePicker";
      this.dateTimePicker.ShowUpDown = true;
      this.dateTimePicker.Size = new System.Drawing.Size(87, 20);
      this.dateTimePicker.TabIndex = 3;
      this.dateTimePicker.Value = new System.DateTime(2014, 10, 31, 0, 0, 0, 0);
      // 
      // TimeSpanSelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "TimeSpanSelection";
      this.Size = new System.Drawing.Size(99, 58);
      this.Load += new System.EventHandler(this.TimeSpanSelectionLoad);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DateTimePicker dateTimePicker;
    private System.Windows.Forms.CheckBox nullCheckBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
