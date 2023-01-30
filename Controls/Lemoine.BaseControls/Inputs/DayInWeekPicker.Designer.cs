// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class DayInWeekPicker
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
      this.checkBoxMonday = new System.Windows.Forms.CheckBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.checkBoxTuesday = new System.Windows.Forms.CheckBox();
      this.checkBoxWednesday = new System.Windows.Forms.CheckBox();
      this.checkBoxThursday = new System.Windows.Forms.CheckBox();
      this.checkBoxFriday = new System.Windows.Forms.CheckBox();
      this.checkBoxSaturday = new System.Windows.Forms.CheckBox();
      this.checkBoxSunday = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkBoxMonday
      // 
      this.checkBoxMonday.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkBoxMonday.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxMonday.Location = new System.Drawing.Point(28, 0);
      this.checkBoxMonday.Margin = new System.Windows.Forms.Padding(0);
      this.checkBoxMonday.Name = "checkBoxMonday";
      this.checkBoxMonday.Size = new System.Drawing.Size(14, 20);
      this.checkBoxMonday.TabIndex = 0;
      this.checkBoxMonday.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 9;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.checkBoxTuesday, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxWednesday, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxThursday, 4, 0);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxFriday, 5, 0);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxSaturday, 6, 0);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxSunday, 7, 0);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxMonday, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.label1, 8, 0);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(155, 20);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // checkBoxTuesday
      // 
      this.checkBoxTuesday.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkBoxTuesday.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxTuesday.Location = new System.Drawing.Point(42, 0);
      this.checkBoxTuesday.Margin = new System.Windows.Forms.Padding(0);
      this.checkBoxTuesday.Name = "checkBoxTuesday";
      this.checkBoxTuesday.Size = new System.Drawing.Size(14, 20);
      this.checkBoxTuesday.TabIndex = 2;
      this.checkBoxTuesday.UseVisualStyleBackColor = true;
      // 
      // checkBoxWednesday
      // 
      this.checkBoxWednesday.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkBoxWednesday.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxWednesday.Location = new System.Drawing.Point(56, 0);
      this.checkBoxWednesday.Margin = new System.Windows.Forms.Padding(0);
      this.checkBoxWednesday.Name = "checkBoxWednesday";
      this.checkBoxWednesday.Size = new System.Drawing.Size(14, 20);
      this.checkBoxWednesday.TabIndex = 3;
      this.checkBoxWednesday.UseVisualStyleBackColor = true;
      // 
      // checkBoxThursday
      // 
      this.checkBoxThursday.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkBoxThursday.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxThursday.Location = new System.Drawing.Point(70, 0);
      this.checkBoxThursday.Margin = new System.Windows.Forms.Padding(0);
      this.checkBoxThursday.Name = "checkBoxThursday";
      this.checkBoxThursday.Size = new System.Drawing.Size(14, 20);
      this.checkBoxThursday.TabIndex = 4;
      this.checkBoxThursday.UseVisualStyleBackColor = true;
      // 
      // checkBoxFriday
      // 
      this.checkBoxFriday.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkBoxFriday.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxFriday.Location = new System.Drawing.Point(84, 0);
      this.checkBoxFriday.Margin = new System.Windows.Forms.Padding(0);
      this.checkBoxFriday.Name = "checkBoxFriday";
      this.checkBoxFriday.Size = new System.Drawing.Size(14, 20);
      this.checkBoxFriday.TabIndex = 5;
      this.checkBoxFriday.UseVisualStyleBackColor = true;
      // 
      // checkBoxSaturday
      // 
      this.checkBoxSaturday.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkBoxSaturday.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxSaturday.Location = new System.Drawing.Point(98, 0);
      this.checkBoxSaturday.Margin = new System.Windows.Forms.Padding(0);
      this.checkBoxSaturday.Name = "checkBoxSaturday";
      this.checkBoxSaturday.Size = new System.Drawing.Size(14, 20);
      this.checkBoxSaturday.TabIndex = 6;
      this.checkBoxSaturday.UseVisualStyleBackColor = true;
      // 
      // checkBoxSunday
      // 
      this.checkBoxSunday.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkBoxSunday.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxSunday.Location = new System.Drawing.Point(112, 0);
      this.checkBoxSunday.Margin = new System.Windows.Forms.Padding(0);
      this.checkBoxSunday.Name = "checkBoxSunday";
      this.checkBoxSunday.Size = new System.Drawing.Size(14, 20);
      this.checkBoxSunday.TabIndex = 1;
      this.checkBoxSunday.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(126, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(32, 22);
      this.label1.TabIndex = 8;
      this.label1.Text = "Sun";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(31, 22);
      this.label2.TabIndex = 9;
      this.label2.Text = "Mon";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // DayInWeekPicker
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.MinimumSize = new System.Drawing.Size(155, 20);
      this.Name = "DayInWeekPicker";
      this.Size = new System.Drawing.Size(155, 20);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox checkBoxSunday;
    private System.Windows.Forms.CheckBox checkBoxSaturday;
    private System.Windows.Forms.CheckBox checkBoxFriday;
    private System.Windows.Forms.CheckBox checkBoxThursday;
    private System.Windows.Forms.CheckBox checkBoxWednesday;
    private System.Windows.Forms.CheckBox checkBoxTuesday;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.CheckBox checkBoxMonday;
  }
}
