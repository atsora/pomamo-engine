// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class PageViewMachines
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
      this.components = new System.ComponentModel.Container();
      this.label2 = new System.Windows.Forms.Label();
      this.comboMachine = new Lemoine.BaseControls.ComboboxTextValue();
      this.label1 = new System.Windows.Forms.Label();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.markerAnalysis = new Lemoine.BaseControls.Marker();
      this.markerCurrent = new Lemoine.BaseControls.Marker();
      this.labelAnalysedDateTime = new System.Windows.Forms.Label();
      this.horizontalLine = new System.Windows.Forms.Label();
      this.panel = new System.Windows.Forms.Panel();
      this.verticalScroll = new Lemoine.BaseControls.VerticalScrollLayout();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.comboConfFile = new Lemoine.BaseControls.ComboboxTextValue();
      this.labelCurrentDateTime = new System.Windows.Forms.Label();
      this.labelModeAnalysed = new System.Windows.Forms.Label();
      this.labelModeCurrent = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.markerFact = new Lemoine.BaseControls.Marker();
      this.labelFactDateTime = new System.Windows.Forms.Label();
      this.labelModeFact = new System.Windows.Forms.Label();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
      this.toolTip3 = new System.Windows.Forms.ToolTip(this.components);
      this.baseLayout.SuspendLayout();
      this.panel.SuspendLayout();
      this.SuspendLayout();
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(370, 179);
      this.label2.TabIndex = 0;
      this.label2.Text = "No modules";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // comboMachine
      // 
      this.baseLayout.SetColumnSpan(this.comboMachine, 3);
      this.comboMachine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboMachine.Location = new System.Drawing.Point(150, 25);
      this.comboMachine.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
      this.comboMachine.Name = "comboMachine";
      this.comboMachine.Size = new System.Drawing.Size(217, 23);
      this.comboMachine.Sorted = true;
      this.comboMachine.TabIndex = 4;
      this.comboMachine.ItemChanged += new System.Action<string, object>(this.ComboMachineItemChanged);
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(3, 24);
      this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(144, 18);
      this.label1.TabIndex = 1;
      this.label1.Text = "Machine";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
      this.baseLayout.Controls.Add(this.markerAnalysis, 1, 4);
      this.baseLayout.Controls.Add(this.markerCurrent, 1, 2);
      this.baseLayout.Controls.Add(this.labelAnalysedDateTime, 3, 4);
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Controls.Add(this.horizontalLine, 0, 5);
      this.baseLayout.Controls.Add(this.panel, 0, 6);
      this.baseLayout.Controls.Add(this.label3, 0, 0);
      this.baseLayout.Controls.Add(this.label4, 0, 4);
      this.baseLayout.Controls.Add(this.label6, 0, 2);
      this.baseLayout.Controls.Add(this.comboConfFile, 1, 0);
      this.baseLayout.Controls.Add(this.comboMachine, 1, 1);
      this.baseLayout.Controls.Add(this.labelCurrentDateTime, 3, 2);
      this.baseLayout.Controls.Add(this.labelModeAnalysed, 2, 4);
      this.baseLayout.Controls.Add(this.labelModeCurrent, 2, 2);
      this.baseLayout.Controls.Add(this.label5, 0, 3);
      this.baseLayout.Controls.Add(this.markerFact, 1, 3);
      this.baseLayout.Controls.Add(this.labelFactDateTime, 3, 3);
      this.baseLayout.Controls.Add(this.labelModeFact, 2, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 7;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 4;
      // 
      // markerAnalysis
      // 
      this.markerAnalysis.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.markerAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
      this.markerAnalysis.Location = new System.Drawing.Point(150, 88);
      this.markerAnalysis.Margin = new System.Windows.Forms.Padding(0);
      this.markerAnalysis.Name = "markerAnalysis";
      this.markerAnalysis.Size = new System.Drawing.Size(20, 20);
      this.markerAnalysis.TabIndex = 5;
      // 
      // markerCurrent
      // 
      this.markerCurrent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.markerCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
      this.markerCurrent.Location = new System.Drawing.Point(150, 48);
      this.markerCurrent.Margin = new System.Windows.Forms.Padding(0);
      this.markerCurrent.Name = "markerCurrent";
      this.markerCurrent.Size = new System.Drawing.Size(20, 20);
      this.markerCurrent.TabIndex = 3;
      // 
      // labelAnalysedDateTime
      // 
      this.labelAnalysedDateTime.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelAnalysedDateTime.Location = new System.Drawing.Point(243, 88);
      this.labelAnalysedDateTime.Name = "labelAnalysedDateTime";
      this.labelAnalysedDateTime.Size = new System.Drawing.Size(124, 20);
      this.labelAnalysedDateTime.TabIndex = 11;
      this.labelAnalysedDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip3.SetToolTip(this.labelAnalysedDateTime, "End of the latest machine status");
      // 
      // horizontalLine
      // 
      this.horizontalLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.baseLayout.SetColumnSpan(this.horizontalLine, 4);
      this.horizontalLine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.horizontalLine.Location = new System.Drawing.Point(0, 110);
      this.horizontalLine.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.horizontalLine.Name = "horizontalLine";
      this.horizontalLine.Size = new System.Drawing.Size(370, 1);
      this.horizontalLine.TabIndex = 6;
      // 
      // panel
      // 
      this.baseLayout.SetColumnSpan(this.panel, 4);
      this.panel.Controls.Add(this.verticalScroll);
      this.panel.Controls.Add(this.label2);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 111);
      this.panel.Margin = new System.Windows.Forms.Padding(0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(370, 179);
      this.panel.TabIndex = 7;
      // 
      // verticalScroll
      // 
      this.verticalScroll.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScroll.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScroll.Location = new System.Drawing.Point(0, 0);
      this.verticalScroll.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Name = "verticalScroll";
      this.verticalScroll.Size = new System.Drawing.Size(370, 179);
      this.verticalScroll.TabIndex = 1;
      this.verticalScroll.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScroll.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(144, 24);
      this.label3.TabIndex = 8;
      this.label3.Text = "Configuration file";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(3, 88);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(144, 20);
      this.label4.TabIndex = 10;
      this.label4.Text = "Analysed machine status";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label6
      // 
      this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label6.Location = new System.Drawing.Point(3, 48);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(144, 20);
      this.label6.TabIndex = 12;
      this.label6.Text = "Acquisition (machine mode)";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // comboConfFile
      // 
      this.baseLayout.SetColumnSpan(this.comboConfFile, 3);
      this.comboConfFile.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboConfFile.Location = new System.Drawing.Point(150, 1);
      this.comboConfFile.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
      this.comboConfFile.Name = "comboConfFile";
      this.comboConfFile.Size = new System.Drawing.Size(217, 23);
      this.comboConfFile.Sorted = true;
      this.comboConfFile.TabIndex = 9;
      this.comboConfFile.ItemChanged += new System.Action<string, object>(this.ComboConfFileItemChanged);
      // 
      // labelCurrentDateTime
      // 
      this.labelCurrentDateTime.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelCurrentDateTime.Location = new System.Drawing.Point(243, 48);
      this.labelCurrentDateTime.Name = "labelCurrentDateTime";
      this.labelCurrentDateTime.Size = new System.Drawing.Size(124, 20);
      this.labelCurrentDateTime.TabIndex = 9;
      this.labelCurrentDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip1.SetToolTip(this.labelCurrentDateTime, "Current machine mode");
      // 
      // labelModeAnalysed
      // 
      this.labelModeAnalysed.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelModeAnalysed.Location = new System.Drawing.Point(173, 88);
      this.labelModeAnalysed.Name = "labelModeAnalysed";
      this.labelModeAnalysed.Size = new System.Drawing.Size(64, 20);
      this.labelModeAnalysed.TabIndex = 8;
      this.labelModeAnalysed.Text = "-";
      this.labelModeAnalysed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelModeCurrent
      // 
      this.labelModeCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelModeCurrent.Location = new System.Drawing.Point(173, 48);
      this.labelModeCurrent.Name = "labelModeCurrent";
      this.labelModeCurrent.Size = new System.Drawing.Size(64, 20);
      this.labelModeCurrent.TabIndex = 6;
      this.labelModeCurrent.Text = "-";
      this.labelModeCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(3, 68);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(144, 20);
      this.label5.TabIndex = 13;
      this.label5.Text = "After CncDataService";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // markerFact
      // 
      this.markerFact.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.markerFact.Dock = System.Windows.Forms.DockStyle.Fill;
      this.markerFact.Location = new System.Drawing.Point(150, 68);
      this.markerFact.Margin = new System.Windows.Forms.Padding(0);
      this.markerFact.Name = "markerFact";
      this.markerFact.Size = new System.Drawing.Size(20, 20);
      this.markerFact.TabIndex = 14;
      // 
      // labelFactDateTime
      // 
      this.labelFactDateTime.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelFactDateTime.Location = new System.Drawing.Point(243, 68);
      this.labelFactDateTime.Name = "labelFactDateTime";
      this.labelFactDateTime.Size = new System.Drawing.Size(124, 20);
      this.labelFactDateTime.TabIndex = 15;
      this.labelFactDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip2.SetToolTip(this.labelFactDateTime, "End of the latest fact");
      // 
      // labelModeFact
      // 
      this.labelModeFact.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelModeFact.Location = new System.Drawing.Point(173, 68);
      this.labelModeFact.Name = "labelModeFact";
      this.labelModeFact.Size = new System.Drawing.Size(64, 20);
      this.labelModeFact.TabIndex = 16;
      this.labelModeFact.Text = "-";
      this.labelModeFact.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // PageViewMachines
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.Name = "PageViewMachines";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.panel.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private Lemoine.BaseControls.ComboboxTextValue comboMachine;
    private System.Windows.Forms.Label label2;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScroll;
    private System.Windows.Forms.Label horizontalLine;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.Label label3;
    private Lemoine.BaseControls.ComboboxTextValue comboConfFile;
    private System.Windows.Forms.Label label4;
    private Lemoine.BaseControls.Marker markerCurrent;
    private Lemoine.BaseControls.Marker markerAnalysis;
    private System.Windows.Forms.Label labelModeCurrent;
    private System.Windows.Forms.Label labelModeAnalysed;
    private System.Windows.Forms.Label labelCurrentDateTime;
    private System.Windows.Forms.Label labelAnalysedDateTime;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
    private Lemoine.BaseControls.Marker markerFact;
    private System.Windows.Forms.Label labelFactDateTime;
    private System.Windows.Forms.Label labelModeFact;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.ToolTip toolTip2;
    private System.Windows.Forms.ToolTip toolTip3;
  }
}
