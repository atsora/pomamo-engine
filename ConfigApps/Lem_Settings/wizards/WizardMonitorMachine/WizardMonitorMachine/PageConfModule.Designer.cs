// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class PageConfModule
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
    private void InitializeComponent ()
    {
      baseLayout = new System.Windows.Forms.TableLayoutPanel ();
      cncConfiguratorControl = new CncConfiguratorControl ();
      labelOldConfiguration = new System.Windows.Forms.Label ();
      cncConfiguratorControlOld = new CncConfiguratorControl ();
      labelNewConfiguration = new System.Windows.Forms.Label ();
      textOldParameters = new System.Windows.Forms.TextBox ();
      baseLayout.SuspendLayout ();
      SuspendLayout ();
      // 
      // baseLayout
      // 
      baseLayout.ColumnCount = 1;
      baseLayout.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 100F));
      baseLayout.Controls.Add (cncConfiguratorControl, 0, 1);
      baseLayout.Controls.Add (labelOldConfiguration, 0, 2);
      baseLayout.Controls.Add (cncConfiguratorControlOld, 0, 3);
      baseLayout.Controls.Add (labelNewConfiguration, 0, 0);
      baseLayout.Controls.Add (textOldParameters, 0, 4);
      baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      baseLayout.Location = new System.Drawing.Point (0, 0);
      baseLayout.Margin = new System.Windows.Forms.Padding (0);
      baseLayout.Name = "baseLayout";
      baseLayout.RowCount = 5;
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 21F));
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 60F));
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 21F));
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 40F));
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 25F));
      baseLayout.Size = new System.Drawing.Size (408, 288);
      baseLayout.TabIndex = 1;
      // 
      // cncConfiguratorControl
      // 
      cncConfiguratorControl.Dock = System.Windows.Forms.DockStyle.Fill;
      cncConfiguratorControl.Location = new System.Drawing.Point (0, 21);
      cncConfiguratorControl.Margin = new System.Windows.Forms.Padding (0);
      cncConfiguratorControl.Name = "cncConfiguratorControl";
      cncConfiguratorControl.Size = new System.Drawing.Size (408, 132);
      cncConfiguratorControl.TabIndex = 0;
      // 
      // labelOldConfiguration
      // 
      labelOldConfiguration.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      labelOldConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
      labelOldConfiguration.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      labelOldConfiguration.ForeColor = System.Drawing.SystemColors.ControlLightLight;
      labelOldConfiguration.Location = new System.Drawing.Point (0, 153);
      labelOldConfiguration.Margin = new System.Windows.Forms.Padding (0);
      labelOldConfiguration.Name = "labelOldConfiguration";
      labelOldConfiguration.Size = new System.Drawing.Size (408, 21);
      labelOldConfiguration.TabIndex = 1;
      labelOldConfiguration.Text = "Old configuration";
      labelOldConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // cncConfiguratorControlOld
      // 
      cncConfiguratorControlOld.Dock = System.Windows.Forms.DockStyle.Fill;
      cncConfiguratorControlOld.Location = new System.Drawing.Point (0, 174);
      cncConfiguratorControlOld.Margin = new System.Windows.Forms.Padding (0);
      cncConfiguratorControlOld.Name = "cncConfiguratorControlOld";
      cncConfiguratorControlOld.Size = new System.Drawing.Size (408, 88);
      cncConfiguratorControlOld.TabIndex = 0;
      // 
      // labelNewConfiguration
      // 
      labelNewConfiguration.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      labelNewConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
      labelNewConfiguration.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      labelNewConfiguration.ForeColor = System.Drawing.SystemColors.ControlLightLight;
      labelNewConfiguration.Location = new System.Drawing.Point (0, 0);
      labelNewConfiguration.Margin = new System.Windows.Forms.Padding (0);
      labelNewConfiguration.Name = "labelNewConfiguration";
      labelNewConfiguration.Size = new System.Drawing.Size (408, 21);
      labelNewConfiguration.TabIndex = 3;
      labelNewConfiguration.Text = "New configuration";
      labelNewConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // textOldParameters
      // 
      textOldParameters.BackColor = System.Drawing.SystemColors.Control;
      textOldParameters.BorderStyle = System.Windows.Forms.BorderStyle.None;
      textOldParameters.Dock = System.Windows.Forms.DockStyle.Fill;
      textOldParameters.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      textOldParameters.Location = new System.Drawing.Point (4, 265);
      textOldParameters.Margin = new System.Windows.Forms.Padding (4, 3, 4, 0);
      textOldParameters.Name = "textOldParameters";
      textOldParameters.Size = new System.Drawing.Size (400, 13);
      textOldParameters.TabIndex = 4;
      textOldParameters.Text = "Old parameters";
      textOldParameters.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // PageConfModule
      // 
      AutoScaleDimensions = new System.Drawing.SizeF (7F, 15F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      Controls.Add (baseLayout);
      Margin = new System.Windows.Forms.Padding (0);
      Name = "PageConfModule";
      Size = new System.Drawing.Size (408, 288);
      baseLayout.ResumeLayout (false);
      baseLayout.PerformLayout ();
      ResumeLayout (false);
    }

    private WizardMonitorMachine.CncConfiguratorControl cncConfiguratorControl;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelOldConfiguration;
    private WizardMonitorMachine.CncConfiguratorControl cncConfiguratorControlOld;
    private System.Windows.Forms.Label labelNewConfiguration;
    private System.Windows.Forms.TextBox textOldParameters;
  }
}
