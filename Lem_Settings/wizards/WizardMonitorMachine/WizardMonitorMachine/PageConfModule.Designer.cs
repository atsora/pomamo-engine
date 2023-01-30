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
    private void InitializeComponent()
    {
      this.cncConfiguratorControl = new WizardMonitorMachine.CncConfiguratorControl();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelOldConfiguration = new System.Windows.Forms.Label();
      this.cncConfiguratorControlOld = new WizardMonitorMachine.CncConfiguratorControl();
      this.labelNewConfiguration = new System.Windows.Forms.Label();
      this.textOldParameters = new System.Windows.Forms.TextBox();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // cncConfiguratorControl
      // 
      this.cncConfiguratorControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cncConfiguratorControl.Location = new System.Drawing.Point(0, 18);
      this.cncConfiguratorControl.Margin = new System.Windows.Forms.Padding(0);
      this.cncConfiguratorControl.Name = "cncConfiguratorControl";
      this.cncConfiguratorControl.Size = new System.Drawing.Size(350, 115);
      this.cncConfiguratorControl.TabIndex = 0;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.cncConfiguratorControl, 0, 1);
      this.baseLayout.Controls.Add(this.labelOldConfiguration, 0, 2);
      this.baseLayout.Controls.Add(this.cncConfiguratorControlOld, 0, 3);
      this.baseLayout.Controls.Add(this.labelNewConfiguration, 0, 0);
      this.baseLayout.Controls.Add(this.textOldParameters, 0, 4);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 5;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 1;
      // 
      // labelOldConfiguration
      // 
      this.labelOldConfiguration.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.labelOldConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelOldConfiguration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelOldConfiguration.ForeColor = System.Drawing.SystemColors.ControlLightLight;
      this.labelOldConfiguration.Location = new System.Drawing.Point(0, 133);
      this.labelOldConfiguration.Margin = new System.Windows.Forms.Padding(0);
      this.labelOldConfiguration.Name = "labelOldConfiguration";
      this.labelOldConfiguration.Size = new System.Drawing.Size(350, 18);
      this.labelOldConfiguration.TabIndex = 1;
      this.labelOldConfiguration.Text = "Old configuration";
      this.labelOldConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // cncConfiguratorControlOld
      // 
      this.cncConfiguratorControlOld.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cncConfiguratorControlOld.Location = new System.Drawing.Point(0, 151);
      this.cncConfiguratorControlOld.Margin = new System.Windows.Forms.Padding(0);
      this.cncConfiguratorControlOld.Name = "cncConfiguratorControlOld";
      this.cncConfiguratorControlOld.Size = new System.Drawing.Size(350, 76);
      this.cncConfiguratorControlOld.TabIndex = 2;
      // 
      // labelNewConfiguration
      // 
      this.labelNewConfiguration.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.labelNewConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelNewConfiguration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelNewConfiguration.ForeColor = System.Drawing.SystemColors.ControlLightLight;
      this.labelNewConfiguration.Location = new System.Drawing.Point(0, 0);
      this.labelNewConfiguration.Margin = new System.Windows.Forms.Padding(0);
      this.labelNewConfiguration.Name = "labelNewConfiguration";
      this.labelNewConfiguration.Size = new System.Drawing.Size(350, 18);
      this.labelNewConfiguration.TabIndex = 3;
      this.labelNewConfiguration.Text = "New configuration";
      this.labelNewConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // textOldParameters
      // 
      this.textOldParameters.BackColor = System.Drawing.SystemColors.Control;
      this.textOldParameters.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textOldParameters.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textOldParameters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textOldParameters.Location = new System.Drawing.Point(3, 230);
      this.textOldParameters.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.textOldParameters.Name = "textOldParameters";
      this.textOldParameters.Size = new System.Drawing.Size(344, 13);
      this.textOldParameters.TabIndex = 4;
      this.textOldParameters.Text = "Old parameters";
      this.textOldParameters.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // PageConfModule
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PageConfModule";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);

    }
    private WizardMonitorMachine.CncConfiguratorControl cncConfiguratorControl;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelOldConfiguration;
    private WizardMonitorMachine.CncConfiguratorControl cncConfiguratorControlOld;
    private System.Windows.Forms.Label labelNewConfiguration;
    private System.Windows.Forms.TextBox textOldParameters;
  }
}
