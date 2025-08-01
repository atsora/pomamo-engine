// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class ModuleStampingGrid
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScroll;

    /// <summary>
    /// Disposes resources used by the control.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing) {
        if (components != null) {
          m_textDescription.Dispose ();
          m_textCycleStart.Dispose ();
          m_textCycleEnd.Dispose ();
          m_textSequence.Dispose ();
          m_textMilestone.Dispose ();
          m_textDetection.Dispose ();
          m_labelDescription.Dispose ();
          m_labelCycleStart.Dispose ();
          m_labelCycleEnd.Dispose ();
          m_labelSequence.Dispose ();
          m_labelMilestone.Dispose ();
          m_labelDetection.Dispose ();
          components.Dispose ();
        }
      }
      base.Dispose (disposing);
    }

    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent ()
    {
      this.verticalScroll = new Lemoine.BaseControls.VerticalScrollLayout ();
      this.SuspendLayout ();
      // 
      // verticalScroll
      // 
      this.verticalScroll.ContainerMargin = new System.Windows.Forms.Padding (0);
      this.verticalScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScroll.Location = new System.Drawing.Point (0, 0);
      this.verticalScroll.Name = "verticalScroll";
      this.verticalScroll.Size = new System.Drawing.Size (132, 132);
      this.verticalScroll.TabIndex = 0;
      this.verticalScroll.TitleFont = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScroll.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // ModuleStampingGrid
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add (this.verticalScroll);
      this.Name = "ModuleStampingGrid";
      this.Size = new System.Drawing.Size (132, 132);
      this.ResumeLayout (false);

    }
  }
}
