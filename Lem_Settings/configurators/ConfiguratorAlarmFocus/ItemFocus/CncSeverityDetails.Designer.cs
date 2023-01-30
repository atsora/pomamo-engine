// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarmFocus
{
  partial class CncSeverityDetails
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelName;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScroll;
    private System.Windows.Forms.PictureBox pictureHelp;
    private System.Windows.Forms.Button buttonMenu;
    private Lemoine.BaseControls.StackedWidget stackedWidget;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.TableLayoutPanel tableLayout;
    private System.Windows.Forms.Button buttonAddPattern;
    private System.Windows.Forms.Label label1;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CncSeverityDetails));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelName = new System.Windows.Forms.Label();
      this.pictureHelp = new System.Windows.Forms.PictureBox();
      this.buttonMenu = new System.Windows.Forms.Button();
      this.stackedWidget = new Lemoine.BaseControls.StackedWidget();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.verticalScroll = new Lemoine.BaseControls.VerticalScrollLayout();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonAddPattern = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureHelp)).BeginInit();
      this.stackedWidget.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tableLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.baseLayout.Controls.Add(this.labelName, 1, 0);
      this.baseLayout.Controls.Add(this.pictureHelp, 0, 0);
      this.baseLayout.Controls.Add(this.buttonMenu, 2, 0);
      this.baseLayout.Controls.Add(this.stackedWidget, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(293, 59);
      this.baseLayout.TabIndex = 0;
      // 
      // labelName
      // 
      this.labelName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelName.ForeColor = System.Drawing.SystemColors.Window;
      this.labelName.Location = new System.Drawing.Point(25, 0);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(233, 30);
      this.labelName.TabIndex = 0;
      this.labelName.Text = "Severity name";
      this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pictureHelp
      // 
      this.pictureHelp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureHelp.BackgroundImage")));
      this.pictureHelp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.pictureHelp.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureHelp.Location = new System.Drawing.Point(3, 0);
      this.pictureHelp.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.pictureHelp.Name = "pictureHelp";
      this.pictureHelp.Size = new System.Drawing.Size(19, 30);
      this.pictureHelp.TabIndex = 2;
      this.pictureHelp.TabStop = false;
      // 
      // buttonMenu
      // 
      this.buttonMenu.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonMenu.Image = ((System.Drawing.Image)(resources.GetObject("buttonMenu.Image")));
      this.buttonMenu.Location = new System.Drawing.Point(263, 2);
      this.buttonMenu.Margin = new System.Windows.Forms.Padding(2);
      this.buttonMenu.Name = "buttonMenu";
      this.buttonMenu.Size = new System.Drawing.Size(28, 26);
      this.buttonMenu.TabIndex = 3;
      this.buttonMenu.UseVisualStyleBackColor = true;
      this.buttonMenu.Click += new System.EventHandler(this.ButtonMenuClick);
      // 
      // stackedWidget
      // 
      this.baseLayout.SetColumnSpan(this.stackedWidget, 3);
      this.stackedWidget.Controls.Add(this.tabPage1);
      this.stackedWidget.Controls.Add(this.tabPage2);
      this.stackedWidget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.stackedWidget.Location = new System.Drawing.Point(0, 30);
      this.stackedWidget.Margin = new System.Windows.Forms.Padding(0);
      this.stackedWidget.Name = "stackedWidget";
      this.stackedWidget.SelectedIndex = 0;
      this.stackedWidget.Size = new System.Drawing.Size(293, 29);
      this.stackedWidget.TabIndex = 4;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.verticalScroll);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(285, 3);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // verticalScroll
      // 
      this.verticalScroll.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScroll.ContainerMargin = new System.Windows.Forms.Padding(0, 6, 0, 6);
      this.verticalScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScroll.Location = new System.Drawing.Point(0, 0);
      this.verticalScroll.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Name = "verticalScroll";
      this.verticalScroll.Size = new System.Drawing.Size(285, 3);
      this.verticalScroll.TabIndex = 1;
      this.verticalScroll.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScroll.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // tabPage2
      // 
      this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage2.Controls.Add(this.tableLayout);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(285, 3);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      // 
      // tableLayout
      // 
      this.tableLayout.BackColor = System.Drawing.SystemColors.Window;
      this.tableLayout.ColumnCount = 3;
      this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 97F));
      this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.tableLayout.Controls.Add(this.buttonAddPattern, 1, 2);
      this.tableLayout.Controls.Add(this.label1, 0, 1);
      this.tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayout.Location = new System.Drawing.Point(0, 0);
      this.tableLayout.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayout.Name = "tableLayout";
      this.tableLayout.RowCount = 4;
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayout.Size = new System.Drawing.Size(285, 3);
      this.tableLayout.TabIndex = 0;
      // 
      // buttonAddPattern
      // 
      this.buttonAddPattern.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAddPattern.Location = new System.Drawing.Point(97, -2);
      this.buttonAddPattern.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
      this.buttonAddPattern.Name = "buttonAddPattern";
      this.buttonAddPattern.Size = new System.Drawing.Size(91, 24);
      this.buttonAddPattern.TabIndex = 1;
      this.buttonAddPattern.Text = "Add a pattern";
      this.buttonAddPattern.UseVisualStyleBackColor = true;
      this.buttonAddPattern.Click += new System.EventHandler(this.ButtonAddPatternClick);
      // 
      // label1
      // 
      this.tableLayout.SetColumnSpan(this.label1, 3);
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label1.Location = new System.Drawing.Point(3, -25);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(279, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "No patterns are defined yet";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // CncSeverityDetails
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "CncSeverityDetails";
      this.Size = new System.Drawing.Size(293, 59);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureHelp)).EndInit();
      this.stackedWidget.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tableLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
