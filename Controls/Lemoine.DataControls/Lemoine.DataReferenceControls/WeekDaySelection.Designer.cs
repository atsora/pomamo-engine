// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class WeekDaySelection
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
      this.weekDayGroupBox = new System.Windows.Forms.GroupBox();
      this.weekDayCheckedListBox = new System.Windows.Forms.CheckedListBox();
      this.extremeWeekDayGroupBox = new System.Windows.Forms.GroupBox();
      this.allDaycheckBox = new System.Windows.Forms.CheckBox();
      this.noneCheckBox = new System.Windows.Forms.CheckBox();
      this.weekDayGroupBox.SuspendLayout();
      this.extremeWeekDayGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // weekDayGroupBox
      // 
      this.weekDayGroupBox.Controls.Add(this.weekDayCheckedListBox);
      this.weekDayGroupBox.Location = new System.Drawing.Point(3, 3);
      this.weekDayGroupBox.Name = "weekDayGroupBox";
      this.weekDayGroupBox.Size = new System.Drawing.Size(145, 238);
      this.weekDayGroupBox.TabIndex = 0;
      this.weekDayGroupBox.TabStop = false;
      this.weekDayGroupBox.Text = "groupBox1";
      // 
      // weekDayCheckedListBox
      // 
      this.weekDayCheckedListBox.FormattingEnabled = true;
      this.weekDayCheckedListBox.Location = new System.Drawing.Point(6, 19);
      this.weekDayCheckedListBox.Name = "weekDayCheckedListBox";
      this.weekDayCheckedListBox.Size = new System.Drawing.Size(133, 214);
      this.weekDayCheckedListBox.TabIndex = 0;
      this.weekDayCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.WeekDayCheckedListBoxItemCheck);
      // 
      // extremeWeekDayGroupBox
      // 
      this.extremeWeekDayGroupBox.Controls.Add(this.allDaycheckBox);
      this.extremeWeekDayGroupBox.Controls.Add(this.noneCheckBox);
      this.extremeWeekDayGroupBox.Location = new System.Drawing.Point(154, 3);
      this.extremeWeekDayGroupBox.Name = "extremeWeekDayGroupBox";
      this.extremeWeekDayGroupBox.Size = new System.Drawing.Size(112, 80);
      this.extremeWeekDayGroupBox.TabIndex = 1;
      this.extremeWeekDayGroupBox.TabStop = false;
      this.extremeWeekDayGroupBox.Text = "groupBox2";
      // 
      // allDaycheckBox
      // 
      this.allDaycheckBox.Location = new System.Drawing.Point(6, 49);
      this.allDaycheckBox.Name = "allDaycheckBox";
      this.allDaycheckBox.Size = new System.Drawing.Size(100, 24);
      this.allDaycheckBox.TabIndex = 1;
      this.allDaycheckBox.Text = "checkBox2";
      this.allDaycheckBox.UseVisualStyleBackColor = true;
      this.allDaycheckBox.CheckedChanged += new System.EventHandler(this.AllDaycheckBoxCheckedChanged);
      // 
      // noneCheckBox
      // 
      this.noneCheckBox.Location = new System.Drawing.Point(6, 19);
      this.noneCheckBox.Name = "noneCheckBox";
      this.noneCheckBox.Size = new System.Drawing.Size(100, 24);
      this.noneCheckBox.TabIndex = 0;
      this.noneCheckBox.Text = "checkBox1";
      this.noneCheckBox.UseVisualStyleBackColor = true;
      this.noneCheckBox.CheckedChanged += new System.EventHandler(this.NoneCheckBoxCheckedChanged);
      // 
      // WeekDaySelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.extremeWeekDayGroupBox);
      this.Controls.Add(this.weekDayGroupBox);
      this.Name = "WeekDaySelection";
      this.Size = new System.Drawing.Size(272, 244);
      this.Load += new System.EventHandler(this.WeekDaySelectionLoad);
      this.weekDayGroupBox.ResumeLayout(false);
      this.extremeWeekDayGroupBox.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.CheckBox noneCheckBox;
    private System.Windows.Forms.CheckBox allDaycheckBox;
    private System.Windows.Forms.GroupBox extremeWeekDayGroupBox;
    private System.Windows.Forms.CheckedListBox weekDayCheckedListBox;
    private System.Windows.Forms.GroupBox weekDayGroupBox;
  }
}
