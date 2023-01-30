// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class DateTimeSelection
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
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.monthCalendar = new System.Windows.Forms.MonthCalendar();
      this.timePicker = new System.Windows.Forms.DateTimePicker();
      this.tableLayoutPanel1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.nullCheckBox, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(260, 250);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // nullCheckBox
      // 
      this.nullCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nullCheckBox.Location = new System.Drawing.Point(3, 223);
      this.nullCheckBox.Name = "nullCheckBox";
      this.nullCheckBox.Size = new System.Drawing.Size(336, 24);
      this.nullCheckBox.TabIndex = 2;
      this.nullCheckBox.Text = "No <% outPut.Append(modelName); %>";
      this.nullCheckBox.UseVisualStyleBackColor = true;
      this.nullCheckBox.CheckedChanged += new System.EventHandler(this.NullCheckBoxCheckedChanged);
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 1;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.Controls.Add(this.monthCalendar, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.timePicker, 0, 1);
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 2;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.36364F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.63636F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(254, 214);
      this.tableLayoutPanel2.TabIndex = 3;
      // 
      // monthCalendar
      // 
      this.monthCalendar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.monthCalendar.Location = new System.Drawing.Point(9, 9);
      this.monthCalendar.Name = "monthCalendar";
      this.monthCalendar.TabIndex = 0;
      // 
      // timePicker
      // 
      this.timePicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
      this.timePicker.Location = new System.Drawing.Point(3, 187);
      this.timePicker.Name = "timePicker";
      this.timePicker.Size = new System.Drawing.Size(248, 20);
      this.timePicker.TabIndex = 1;
      // 
      // DateTimeSelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "DateTimeSelection";
      this.Size = new System.Drawing.Size(260, 250);
      this.Load += new System.EventHandler(this.DateTimeSelectionLoad);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DateTimePicker timePicker;
    private System.Windows.Forms.MonthCalendar monthCalendar;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.CheckBox nullCheckBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
