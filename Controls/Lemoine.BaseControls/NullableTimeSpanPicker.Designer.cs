// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class NullableTimeSpanPicker
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
      this.nullCheckBox = new System.Windows.Forms.CheckBox();
      this.durationUpDown = new Lemoine.BaseControls.DurationPicker();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(this.durationUpDown)).BeginInit();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // nullCheckBox
      // 
      this.nullCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nullCheckBox.Location = new System.Drawing.Point(3, 3);
      this.nullCheckBox.Name = "nullCheckBox";
      this.nullCheckBox.Size = new System.Drawing.Size(15, 20);
      this.nullCheckBox.TabIndex = 0;
      this.nullCheckBox.UseVisualStyleBackColor = true;
      this.nullCheckBox.CheckedChanged += new System.EventHandler(this.NullCheckBoxCheckedChanged);
      // 
      // durationUpDown
      // 
      this.durationUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
      this.durationUpDown.Duration = System.TimeSpan.Parse("00:00:00");
      this.durationUpDown.Location = new System.Drawing.Point(24, 3);
      this.durationUpDown.Maximum = new decimal(new int[] {
            86399999,
            0,
            0,
            0});
      this.durationUpDown.Name = "durationUpDown";
      this.durationUpDown.Size = new System.Drawing.Size(115, 20);
      this.durationUpDown.TabIndex = 1;
      this.durationUpDown.TimeSpanValue = System.TimeSpan.Parse("00:00:00");
      this.durationUpDown.WithMs = true;
      this.durationUpDown.ValueChanged += new System.EventHandler(this.TimeSpanChanged);
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 21F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.durationUpDown, 1, 0);
      this.baseLayout.Controls.Add(this.nullCheckBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(142, 26);
      this.baseLayout.TabIndex = 2;
      // 
      // NullableTimeSpanPicker
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "NullableTimeSpanPicker";
      this.Size = new System.Drawing.Size(142, 26);
      ((System.ComponentModel.ISupportInitialize)(this.durationUpDown)).EndInit();
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.DurationPicker durationUpDown;
    private System.Windows.Forms.CheckBox nullCheckBox;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
