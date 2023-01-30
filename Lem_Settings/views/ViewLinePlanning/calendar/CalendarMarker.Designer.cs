// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewLinePlanning
{
  partial class CalendarMarker
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
        if (m_toolTip != null)
          m_toolTip.Dispose();
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
      this.SuspendLayout();
      // 
      // CalendarMarker
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "CalendarMarker";
      this.Size = new System.Drawing.Size(16, 16);
      this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CalendarMarkerMouseDoubleClick);
      this.MouseEnter += new System.EventHandler(this.CalendarMarkerMouseEnter);
      this.ResumeLayout(false);
    }
  }
}
