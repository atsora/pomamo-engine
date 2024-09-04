// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Drawing;

namespace WizardMonitorMachine
{
  partial class MachineModuleCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelAcquisitionName;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label labelAcquisitionFile;
    private System.Windows.Forms.Label labelAcquisitionComputer;
    private System.Windows.Forms.Button buttonMenu;
    private Lemoine.BaseControls.List.ListTextValue listParameters;
    private System.Windows.Forms.Label labelProcess;
    
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
      this.labelAcquisitionName = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.labelAcquisitionFile = new System.Windows.Forms.Label();
      this.buttonMenu = new System.Windows.Forms.Button();
      this.listParameters = new Lemoine.BaseControls.List.ListTextValue();
      this.label7 = new System.Windows.Forms.Label();
      this.labelAcquisitionComputer = new System.Windows.Forms.Label();
      this.labelProcess = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 53F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
      this.baseLayout.Controls.Add(this.labelAcquisitionName, 1, 0);
      this.baseLayout.Controls.Add(this.label5, 0, 0);
      this.baseLayout.Controls.Add(this.label6, 0, 2);
      this.baseLayout.Controls.Add(this.label8, 0, 3);
      this.baseLayout.Controls.Add(this.labelAcquisitionFile, 1, 2);
      this.baseLayout.Controls.Add(this.buttonMenu, 3, 2);
      this.baseLayout.Controls.Add(this.listParameters, 1, 3);
      this.baseLayout.Controls.Add(this.label7, 0, 1);
      this.baseLayout.Controls.Add(this.labelAcquisitionComputer, 1, 1);
      this.baseLayout.Controls.Add(this.labelProcess, 2, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
      this.baseLayout.Size = new System.Drawing.Size(322, 159);
      this.baseLayout.TabIndex = 0;
      // 
      // labelAcquisitionName
      // 
      this.baseLayout.SetColumnSpan(this.labelAcquisitionName, 3);
      this.labelAcquisitionName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelAcquisitionName.Location = new System.Drawing.Point(118, 0);
      this.labelAcquisitionName.Name = "labelAcquisitionName";
      this.labelAcquisitionName.Size = new System.Drawing.Size(201, 25);
      this.labelAcquisitionName.TabIndex = 0;
      this.labelAcquisitionName.Text = "-";
      this.labelAcquisitionName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(3, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(109, 25);
      this.label5.TabIndex = 2;
      this.label5.Text = "Acquisition name";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label6
      // 
      this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label6.Location = new System.Drawing.Point(3, 50);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(109, 25);
      this.label6.TabIndex = 3;
      this.label6.Text = "Configuration file";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label8
      // 
      this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label8.Location = new System.Drawing.Point(3, 78);
      this.label8.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(109, 81);
      this.label8.TabIndex = 5;
      this.label8.Text = "Parameters";
      // 
      // labelAcquisitionFile
      // 
      this.baseLayout.SetColumnSpan(this.labelAcquisitionFile, 2);
      this.labelAcquisitionFile.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelAcquisitionFile.Location = new System.Drawing.Point(118, 50);
      this.labelAcquisitionFile.Name = "labelAcquisitionFile";
      this.labelAcquisitionFile.Size = new System.Drawing.Size(166, 25);
      this.labelAcquisitionFile.TabIndex = 7;
      this.labelAcquisitionFile.Text = "-";
      this.labelAcquisitionFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonMenu
      // 
      this.buttonMenu.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonMenu.Enabled = false;
      this.buttonMenu.Image = new Bitmap ("menu.png");
      this.buttonMenu.Location = new System.Drawing.Point(287, 50);
      this.buttonMenu.Margin = new System.Windows.Forms.Padding(0, 0, 2, 2);
      this.buttonMenu.Name = "buttonMenu";
      this.buttonMenu.Size = new System.Drawing.Size(33, 23);
      this.buttonMenu.TabIndex = 12;
      this.buttonMenu.UseVisualStyleBackColor = true;
      this.buttonMenu.Click += new System.EventHandler(this.ButtonOpenClick);
      // 
      // listParameters
      // 
      this.listParameters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listParameters, 3);
      this.listParameters.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listParameters.Location = new System.Drawing.Point(118, 75);
      this.listParameters.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.listParameters.Name = "listParameters";
      this.listParameters.Size = new System.Drawing.Size(201, 81);
      this.listParameters.TabIndex = 16;
      // 
      // label7
      // 
      this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label7.Location = new System.Drawing.Point(3, 25);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(109, 25);
      this.label7.TabIndex = 4;
      this.label7.Text = "Computer";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelAcquisitionComputer
      // 
      this.labelAcquisitionComputer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelAcquisitionComputer.Location = new System.Drawing.Point(118, 25);
      this.labelAcquisitionComputer.Name = "labelAcquisitionComputer";
      this.labelAcquisitionComputer.Size = new System.Drawing.Size(113, 25);
      this.labelAcquisitionComputer.TabIndex = 10;
      this.labelAcquisitionComputer.Text = "-";
      this.labelAcquisitionComputer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelProcess
      // 
      this.baseLayout.SetColumnSpan(this.labelProcess, 2);
      this.labelProcess.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelProcess.Location = new System.Drawing.Point(237, 25);
      this.labelProcess.Name = "labelProcess";
      this.labelProcess.Size = new System.Drawing.Size(82, 25);
      this.labelProcess.TabIndex = 17;
      this.labelProcess.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // MachineModuleCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "MachineModuleCell";
      this.Size = new System.Drawing.Size(322, 159);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
