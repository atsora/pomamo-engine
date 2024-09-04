// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardEventLongPeriod
{
  partial class EventLevelCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.CheckBox checkBox;
    private Lemoine.BaseControls.DurationPicker durationPicker;
    
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.checkBox = new System.Windows.Forms.CheckBox();
      this.durationPicker = new Lemoine.BaseControls.DurationPicker();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 203F));
      this.baseLayout.Controls.Add(this.checkBox, 0, 0);
      this.baseLayout.Controls.Add(this.durationPicker, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(310, 50);
      this.baseLayout.TabIndex = 0;
      // 
      // checkBox
      // 
      this.checkBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBox.Location = new System.Drawing.Point(3, 3);
      this.checkBox.Name = "checkBox";
      this.checkBox.Size = new System.Drawing.Size(101, 44);
      this.checkBox.TabIndex = 0;
      this.checkBox.Text = "...";
      this.checkBox.UseVisualStyleBackColor = true;
      this.checkBox.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
      // 
      // durationPicker
      // 
      this.durationPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.durationPicker.Duration = System.TimeSpan.Parse("00:00:00");
      this.durationPicker.Enabled = false;
      this.durationPicker.Location = new System.Drawing.Point(110, 3);
      this.durationPicker.Name = "durationPicker";
      this.durationPicker.Size = new System.Drawing.Size(197, 44);
      this.durationPicker.TabIndex = 2;
      this.durationPicker.WithMs = false;
      // 
      // EventLevelCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.MinimumSize = new System.Drawing.Size(200, 26);
      this.Name = "EventLevelCell";
      this.Size = new System.Drawing.Size(310, 50);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
