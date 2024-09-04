// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewLinePlanning
{
  partial class CalendarCell
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
      this.verticalScrollLayout = new Lemoine.BaseControls.VerticalScrollLayout();
      this.SuspendLayout();
      // 
      // verticalScrollLayout
      // 
      this.verticalScrollLayout.ColumnCount = 0;
      this.verticalScrollLayout.ContainerMargin = new System.Windows.Forms.Padding(1, 0, 0, 0);
      this.verticalScrollLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLayout.FlowLayoutMode = true;
      this.verticalScrollLayout.Location = new System.Drawing.Point(0, 0);
      this.verticalScrollLayout.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.MouseWheelStep = 5;
      this.verticalScrollLayout.Name = "verticalScrollLayout";
      this.verticalScrollLayout.RowCount = 0;
      this.verticalScrollLayout.Size = new System.Drawing.Size(50, 70);
      this.verticalScrollLayout.TabIndex = 0;
      this.verticalScrollLayout.Title = "30";
      this.verticalScrollLayout.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLayout.TitleForeColor = System.Drawing.SystemColors.ControlDarkDark;
      // 
      // CalendarCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.verticalScrollLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "CalendarCell";
      this.Size = new System.Drawing.Size(50, 70);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLayout;
  }
}
