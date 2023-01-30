// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarms
{
  partial class Page1
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
      this.buttonFilter = new System.Windows.Forms.Button();
      this.buttonNew = new System.Windows.Forms.Button();
      this.stackedWidget = new Lemoine.BaseControls.StackedWidget();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.verticalScroll = new Lemoine.BaseControls.VerticalScrollLayout();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.label1 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.stackedWidget.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
      this.baseLayout.Controls.Add(this.buttonFilter, 0, 1);
      this.baseLayout.Controls.Add(this.buttonNew, 2, 1);
      this.baseLayout.Controls.Add(this.stackedWidget, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // buttonFilter
      // 
      this.buttonFilter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonFilter.Location = new System.Drawing.Point(0, 268);
      this.buttonFilter.Margin = new System.Windows.Forms.Padding(0);
      this.buttonFilter.Name = "buttonFilter";
      this.buttonFilter.Size = new System.Drawing.Size(85, 22);
      this.buttonFilter.TabIndex = 1;
      this.buttonFilter.Text = "Filter";
      this.buttonFilter.UseVisualStyleBackColor = true;
      this.buttonFilter.Click += new System.EventHandler(this.ButtonFilterClick);
      // 
      // buttonNew
      // 
      this.buttonNew.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonNew.Location = new System.Drawing.Point(285, 268);
      this.buttonNew.Margin = new System.Windows.Forms.Padding(0);
      this.buttonNew.Name = "buttonNew";
      this.buttonNew.Size = new System.Drawing.Size(85, 22);
      this.buttonNew.TabIndex = 2;
      this.buttonNew.Text = "New alert";
      this.buttonNew.UseVisualStyleBackColor = true;
      this.buttonNew.Click += new System.EventHandler(this.ButtonNewClick);
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
      this.stackedWidget.Size = new System.Drawing.Size(370, 268);
      this.stackedWidget.TabIndex = 3;
      // 
      // tabPage1
      // 
      this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage1.Controls.Add(this.verticalScroll);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
      this.tabPage1.Size = new System.Drawing.Size(362, 242);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      // 
      // verticalScroll
      // 
      this.verticalScroll.BackColor = System.Drawing.Color.White;
      this.verticalScroll.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.verticalScroll.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScroll.Location = new System.Drawing.Point(0, 0);
      this.verticalScroll.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Name = "verticalScroll";
      this.verticalScroll.Size = new System.Drawing.Size(362, 239);
      this.verticalScroll.TabIndex = 0;
      this.verticalScroll.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScroll.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // tabPage2
      // 
      this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage2.Controls.Add(this.label1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(362, 242);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(362, 242);
      this.label1.TabIndex = 0;
      this.label1.Text = "No alarms";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.stackedWidget.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScroll;
    private System.Windows.Forms.Button buttonFilter;
    private System.Windows.Forms.Button buttonNew;
    private Lemoine.BaseControls.StackedWidget stackedWidget;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Label label1;
  }
}
