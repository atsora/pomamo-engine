// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class DateRangePicker
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing && (components != null)) {
        components.Dispose ();
      }
      base.Dispose (disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ()
    {
      this.upperDatePicker = new Lemoine.BaseControls.DatePicker();
      this.lowerDatePicker = new Lemoine.BaseControls.DatePicker();
      this.lowerCheckBox = new System.Windows.Forms.CheckBox();
      this.upperCheckBox = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // upperDatePicker
      // 
      this.upperDatePicker.Location = new System.Drawing.Point(150, 3);
      this.upperDatePicker.Name = "upperDatePicker";
      this.upperDatePicker.Size = new System.Drawing.Size(98, 20);
      this.upperDatePicker.TabIndex = 1;
      this.upperDatePicker.Value = new System.DateTime(2019, 11, 12, 10, 56, 16, 468);
      // 
      // lowerDatePicker
      // 
      this.lowerDatePicker.Location = new System.Drawing.Point(24, 3);
      this.lowerDatePicker.Name = "lowerDatePicker";
      this.lowerDatePicker.Size = new System.Drawing.Size(99, 20);
      this.lowerDatePicker.TabIndex = 0;
      this.lowerDatePicker.Value = new System.DateTime(2019, 11, 12, 10, 56, 16, 482);
      // 
      // lowerCheckBox
      // 
      this.lowerCheckBox.AutoSize = true;
      this.lowerCheckBox.Checked = true;
      this.lowerCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.lowerCheckBox.Location = new System.Drawing.Point(3, 9);
      this.lowerCheckBox.Name = "lowerCheckBox";
      this.lowerCheckBox.Size = new System.Drawing.Size(15, 14);
      this.lowerCheckBox.TabIndex = 2;
      this.lowerCheckBox.UseVisualStyleBackColor = true;
      this.lowerCheckBox.CheckedChanged += new System.EventHandler(this.lowerCheckBox_CheckedChanged);
      // 
      // upperCheckBox
      // 
      this.upperCheckBox.AutoSize = true;
      this.upperCheckBox.Checked = true;
      this.upperCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.upperCheckBox.Location = new System.Drawing.Point(129, 9);
      this.upperCheckBox.Name = "upperCheckBox";
      this.upperCheckBox.Size = new System.Drawing.Size(15, 14);
      this.upperCheckBox.TabIndex = 3;
      this.upperCheckBox.UseVisualStyleBackColor = true;
      this.upperCheckBox.CheckedChanged += new System.EventHandler(this.upperCheckBox_CheckedChanged);
      // 
      // DateRangePicker
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.upperCheckBox);
      this.Controls.Add(this.lowerCheckBox);
      this.Controls.Add(this.upperDatePicker);
      this.Controls.Add(this.lowerDatePicker);
      this.Name = "DateRangePicker";
      this.Size = new System.Drawing.Size(258, 28);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DatePicker lowerDatePicker;
    private DatePicker upperDatePicker;
    private System.Windows.Forms.CheckBox lowerCheckBox;
    private System.Windows.Forms.CheckBox upperCheckBox;
  }
}
