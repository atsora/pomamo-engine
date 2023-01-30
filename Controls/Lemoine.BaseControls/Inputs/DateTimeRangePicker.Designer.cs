// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class DateTimeRangePicker
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
      this.upperDateTimePicker = new Lemoine.BaseControls.DateTimePicker();
      this.lowerDateTimePicker = new Lemoine.BaseControls.DateTimePicker();
      this.lowerCheckBox = new System.Windows.Forms.CheckBox();
      this.upperCheckBox = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // upperDateTimePicker
      // 
      this.upperDateTimePicker.Location = new System.Drawing.Point(204, 3);
      this.upperDateTimePicker.Name = "upperDateTimePicker";
      this.upperDateTimePicker.Size = new System.Drawing.Size(156, 20);
      this.upperDateTimePicker.TabIndex = 1;
      this.upperDateTimePicker.Value = new System.DateTime(2019, 11, 12, 10, 56, 16, 468);
      this.upperDateTimePicker.WithSeconds = true;
      // 
      // lowerDateTimePicker
      // 
      this.lowerDateTimePicker.Location = new System.Drawing.Point(24, 3);
      this.lowerDateTimePicker.Name = "lowerDateTimePicker";
      this.lowerDateTimePicker.Size = new System.Drawing.Size(153, 20);
      this.lowerDateTimePicker.TabIndex = 0;
      this.lowerDateTimePicker.Value = new System.DateTime(2019, 11, 12, 10, 56, 16, 482);
      this.lowerDateTimePicker.WithSeconds = true;
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
      this.upperCheckBox.Location = new System.Drawing.Point(183, 9);
      this.upperCheckBox.Name = "upperCheckBox";
      this.upperCheckBox.Size = new System.Drawing.Size(15, 14);
      this.upperCheckBox.TabIndex = 3;
      this.upperCheckBox.UseVisualStyleBackColor = true;
      this.upperCheckBox.CheckedChanged += new System.EventHandler(this.upperCheckBox_CheckedChanged);
      // 
      // DateTimeRangePicker
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.upperCheckBox);
      this.Controls.Add(this.lowerCheckBox);
      this.Controls.Add(this.upperDateTimePicker);
      this.Controls.Add(this.lowerDateTimePicker);
      this.Name = "DateTimeRangePicker";
      this.Size = new System.Drawing.Size(368, 28);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DateTimePicker lowerDateTimePicker;
    private DateTimePicker upperDateTimePicker;
    private System.Windows.Forms.CheckBox lowerCheckBox;
    private System.Windows.Forms.CheckBox upperCheckBox;
  }
}
