// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarmFocus
{
  partial class Page2
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
      this.verticalScroll = new Lemoine.BaseControls.VerticalScrollLayout();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonDefault = new System.Windows.Forms.Button();
      this.buttonAddSeverity = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.stackedWidget = new Lemoine.BaseControls.StackedWidget();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.label2 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.stackedWidget.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // verticalScroll
      // 
      this.verticalScroll.BackColor = System.Drawing.Color.White;
      this.verticalScroll.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScroll.Location = new System.Drawing.Point(0, 0);
      this.verticalScroll.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Name = "verticalScroll";
      this.verticalScroll.Size = new System.Drawing.Size(362, 233);
      this.verticalScroll.TabIndex = 0;
      this.verticalScroll.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScroll.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78F));
      this.baseLayout.Controls.Add(this.buttonDefault, 0, 2);
      this.baseLayout.Controls.Add(this.buttonAddSeverity, 2, 2);
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Controls.Add(this.stackedWidget, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // buttonDefault
      // 
      this.buttonDefault.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDefault.Location = new System.Drawing.Point(3, 263);
      this.buttonDefault.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(87, 24);
      this.buttonDefault.TabIndex = 1;
      this.buttonDefault.Text = "Restore default";
      this.buttonDefault.UseVisualStyleBackColor = true;
      this.buttonDefault.Click += new System.EventHandler(this.ButtonDefaultClick);
      // 
      // buttonAddSeverity
      // 
      this.buttonAddSeverity.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAddSeverity.Location = new System.Drawing.Point(292, 263);
      this.buttonAddSeverity.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
      this.buttonAddSeverity.Name = "buttonAddSeverity";
      this.buttonAddSeverity.Size = new System.Drawing.Size(75, 24);
      this.buttonAddSeverity.TabIndex = 2;
      this.buttonAddSeverity.Text = "Add severity";
      this.buttonAddSeverity.UseVisualStyleBackColor = true;
      this.buttonAddSeverity.Click += new System.EventHandler(this.ButtonAddSeverityClick);
      // 
      // label1
      // 
      this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.label1, 3);
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 259);
      this.label1.Margin = new System.Windows.Forms.Padding(0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(370, 1);
      this.label1.TabIndex = 3;
      // 
      // stackedWidget
      // 
      this.baseLayout.SetColumnSpan(this.stackedWidget, 3);
      this.stackedWidget.Controls.Add(this.tabPage1);
      this.stackedWidget.Controls.Add(this.tabPage2);
      this.stackedWidget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.stackedWidget.Location = new System.Drawing.Point(0, 0);
      this.stackedWidget.Margin = new System.Windows.Forms.Padding(0);
      this.stackedWidget.Name = "stackedWidget";
      this.stackedWidget.SelectedIndex = 0;
      this.stackedWidget.Size = new System.Drawing.Size(370, 259);
      this.stackedWidget.TabIndex = 4;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.verticalScroll);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(362, 233);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.label2);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(362, 233);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.BackColor = System.Drawing.SystemColors.Control;
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(362, 233);
      this.label2.TabIndex = 0;
      this.label2.Text = "No severities have been documented for this CNC yet";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.stackedWidget.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScroll;
    private System.Windows.Forms.Button buttonDefault;
    private System.Windows.Forms.Button buttonAddSeverity;
    private System.Windows.Forms.Label label1;
    private Lemoine.BaseControls.StackedWidget stackedWidget;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Label label2;
  }
}
