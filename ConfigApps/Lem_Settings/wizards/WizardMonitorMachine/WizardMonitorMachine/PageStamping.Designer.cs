// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class PageStamping
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelNewConfiguration = new System.Windows.Forms.Label();
      this.labelOldConfiguration = new System.Windows.Forms.Label();
      this.moduleStampingNew = new WizardMonitorMachine.ModuleStamping();
      this.moduleStampingOld = new WizardMonitorMachine.ModuleStamping();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.labelNewConfiguration, 0, 0);
      this.baseLayout.Controls.Add(this.labelOldConfiguration, 0, 2);
      this.baseLayout.Controls.Add(this.moduleStampingNew, 0, 1);
      this.baseLayout.Controls.Add(this.moduleStampingOld, 0, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
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
      this.labelNewConfiguration.Size = new System.Drawing.Size(370, 18);
      this.labelNewConfiguration.TabIndex = 0;
      this.labelNewConfiguration.Text = "New configuration";
      this.labelNewConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelOldConfiguration
      // 
      this.labelOldConfiguration.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.labelOldConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelOldConfiguration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelOldConfiguration.ForeColor = System.Drawing.SystemColors.ControlLightLight;
      this.labelOldConfiguration.Location = new System.Drawing.Point(0, 170);
      this.labelOldConfiguration.Margin = new System.Windows.Forms.Padding(0);
      this.labelOldConfiguration.Name = "labelOldConfiguration";
      this.labelOldConfiguration.Size = new System.Drawing.Size(370, 18);
      this.labelOldConfiguration.TabIndex = 1;
      this.labelOldConfiguration.Text = "Old configuration";
      this.labelOldConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // moduleStampingNew
      // 
      this.moduleStampingNew.Dock = System.Windows.Forms.DockStyle.Fill;
      this.moduleStampingNew.Location = new System.Drawing.Point(0, 18);
      this.moduleStampingNew.Margin = new System.Windows.Forms.Padding(0);
      this.moduleStampingNew.Name = "moduleStampingNew";
      this.moduleStampingNew.Size = new System.Drawing.Size(370, 152);
      this.moduleStampingNew.TabIndex = 2;
      // 
      // moduleStampingOld
      // 
      this.moduleStampingOld.Dock = System.Windows.Forms.DockStyle.Fill;
      this.moduleStampingOld.Location = new System.Drawing.Point(0, 188);
      this.moduleStampingOld.Margin = new System.Windows.Forms.Padding(0);
      this.moduleStampingOld.Name = "moduleStampingOld";
      this.moduleStampingOld.ReadOnly = true;
      this.moduleStampingOld.Size = new System.Drawing.Size(370, 102);
      this.moduleStampingOld.TabIndex = 3;
      // 
      // PageStamping
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PageStamping";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelNewConfiguration;
    private System.Windows.Forms.Label labelOldConfiguration;
    private WizardMonitorMachine.ModuleStamping moduleStampingNew;
    private WizardMonitorMachine.ModuleStamping moduleStampingOld;
  }
}
