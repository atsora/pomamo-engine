// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorPlugins
{
  partial class PagePackageInformation
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PagePackageInformation));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.labelDisplayedName = new System.Windows.Forms.Label();
      this.labelIdentifyingName = new System.Windows.Forms.Label();
      this.labelDescription = new System.Windows.Forms.Label();
      this.labelStackedWidgetTitle = new System.Windows.Forms.Label();
      this.buttonLeft = new System.Windows.Forms.Button();
      this.buttonRight = new System.Windows.Forms.Button();
      this.stackedWidget = new Lemoine.BaseControls.StackedWidget();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.listPlugins = new Lemoine.BaseControls.VerticalScrollLayout();
      this.labelNoPlugins = new System.Windows.Forms.Label();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.listFiles = new Lemoine.BaseControls.VerticalScrollLayout();
      this.labelNoFiles = new System.Windows.Forms.Label();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.listItems = new Lemoine.BaseControls.VerticalScrollLayout();
      this.labelNoItems = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.tagsLabel = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.stackedWidget.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 108F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label3, 0, 1);
      this.baseLayout.Controls.Add(this.label5, 0, 3);
      this.baseLayout.Controls.Add(this.labelDisplayedName, 1, 0);
      this.baseLayout.Controls.Add(this.labelIdentifyingName, 1, 1);
      this.baseLayout.Controls.Add(this.labelDescription, 1, 3);
      this.baseLayout.Controls.Add(this.labelStackedWidgetTitle, 0, 4);
      this.baseLayout.Controls.Add(this.buttonLeft, 2, 4);
      this.baseLayout.Controls.Add(this.buttonRight, 3, 4);
      this.baseLayout.Controls.Add(this.stackedWidget, 0, 5);
      this.baseLayout.Controls.Add(this.label2, 0, 2);
      this.baseLayout.Controls.Add(this.tagsLabel, 1, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 59F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(432, 335);
      this.baseLayout.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(104, 28);
      this.label1.TabIndex = 3;
      this.label1.Text = "Name";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label3.Location = new System.Drawing.Point(0, 28);
      this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(104, 28);
      this.label3.TabIndex = 5;
      this.label3.Text = "Identification";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label5.Location = new System.Drawing.Point(0, 87);
      this.label5.Margin = new System.Windows.Forms.Padding(0, 3, 4, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(104, 56);
      this.label5.TabIndex = 7;
      this.label5.Text = "Description";
      // 
      // labelDisplayedName
      // 
      this.baseLayout.SetColumnSpan(this.labelDisplayedName, 3);
      this.labelDisplayedName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDisplayedName.Location = new System.Drawing.Point(112, 0);
      this.labelDisplayedName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelDisplayedName.Name = "labelDisplayedName";
      this.labelDisplayedName.Size = new System.Drawing.Size(316, 28);
      this.labelDisplayedName.TabIndex = 8;
      this.labelDisplayedName.Text = "labelDisplayedName";
      this.labelDisplayedName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelIdentifyingName
      // 
      this.baseLayout.SetColumnSpan(this.labelIdentifyingName, 3);
      this.labelIdentifyingName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelIdentifyingName.Location = new System.Drawing.Point(112, 28);
      this.labelIdentifyingName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelIdentifyingName.Name = "labelIdentifyingName";
      this.labelIdentifyingName.Size = new System.Drawing.Size(316, 28);
      this.labelIdentifyingName.TabIndex = 9;
      this.labelIdentifyingName.Text = "labelIdentifyingName";
      this.labelIdentifyingName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelDescription
      // 
      this.labelDescription.Location = new System.Drawing.Point(112, 87);
      this.labelDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 0);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(260, 55);
      this.labelDescription.TabIndex = 10;
      this.labelDescription.Text = "labelDescription";
      // 
      // labelStackedWidgetTitle
      // 
      this.baseLayout.SetColumnSpan(this.labelStackedWidgetTitle, 2);
      this.labelStackedWidgetTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelStackedWidgetTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.labelStackedWidgetTitle.Location = new System.Drawing.Point(0, 143);
      this.labelStackedWidgetTitle.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.labelStackedWidgetTitle.Name = "labelStackedWidgetTitle";
      this.labelStackedWidgetTitle.Size = new System.Drawing.Size(372, 28);
      this.labelStackedWidgetTitle.TabIndex = 11;
      this.labelStackedWidgetTitle.Text = "Plugins";
      this.labelStackedWidgetTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonLeft
      // 
      this.buttonLeft.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonLeft.Image = ((System.Drawing.Image)(resources.GetObject("buttonLeft.Image")));
      this.buttonLeft.Location = new System.Drawing.Point(376, 143);
      this.buttonLeft.Margin = new System.Windows.Forms.Padding(0);
      this.buttonLeft.Name = "buttonLeft";
      this.buttonLeft.Size = new System.Drawing.Size(28, 28);
      this.buttonLeft.TabIndex = 12;
      this.buttonLeft.UseVisualStyleBackColor = true;
      this.buttonLeft.Click += new System.EventHandler(this.ButtonLeftClick);
      // 
      // buttonRight
      // 
      this.buttonRight.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRight.Image = ((System.Drawing.Image)(resources.GetObject("buttonRight.Image")));
      this.buttonRight.Location = new System.Drawing.Point(404, 143);
      this.buttonRight.Margin = new System.Windows.Forms.Padding(0);
      this.buttonRight.Name = "buttonRight";
      this.buttonRight.Size = new System.Drawing.Size(28, 28);
      this.buttonRight.TabIndex = 13;
      this.buttonRight.UseVisualStyleBackColor = true;
      this.buttonRight.Click += new System.EventHandler(this.ButtonRightClick);
      // 
      // stackedWidget
      // 
      this.baseLayout.SetColumnSpan(this.stackedWidget, 4);
      this.stackedWidget.Controls.Add(this.tabPage1);
      this.stackedWidget.Controls.Add(this.tabPage2);
      this.stackedWidget.Controls.Add(this.tabPage3);
      this.stackedWidget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.stackedWidget.Location = new System.Drawing.Point(0, 173);
      this.stackedWidget.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.stackedWidget.Name = "stackedWidget";
      this.stackedWidget.SelectedIndex = 0;
      this.stackedWidget.Size = new System.Drawing.Size(432, 162);
      this.stackedWidget.TabIndex = 14;
      // 
      // tabPage1
      // 
      this.tabPage1.BackColor = System.Drawing.Color.Transparent;
      this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tabPage1.Controls.Add(this.listPlugins);
      this.tabPage1.Controls.Add(this.labelNoPlugins);
      this.tabPage1.Location = new System.Drawing.Point(4, 24);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(424, 134);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPlugins";
      // 
      // listPlugins
      // 
      this.listPlugins.BackColor = System.Drawing.Color.White;
      this.listPlugins.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.listPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listPlugins.Location = new System.Drawing.Point(0, 0);
      this.listPlugins.Margin = new System.Windows.Forms.Padding(0);
      this.listPlugins.Name = "listPlugins";
      this.listPlugins.Size = new System.Drawing.Size(422, 132);
      this.listPlugins.TabIndex = 1;
      this.listPlugins.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.listPlugins.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // labelNoPlugins
      // 
      this.labelNoPlugins.BackColor = System.Drawing.Color.White;
      this.labelNoPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelNoPlugins.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point);
      this.labelNoPlugins.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.labelNoPlugins.Location = new System.Drawing.Point(0, 0);
      this.labelNoPlugins.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelNoPlugins.Name = "labelNoPlugins";
      this.labelNoPlugins.Size = new System.Drawing.Size(422, 132);
      this.labelNoPlugins.TabIndex = 0;
      this.labelNoPlugins.Text = "none";
      this.labelNoPlugins.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tabPage2
      // 
      this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tabPage2.Controls.Add(this.listFiles);
      this.tabPage2.Controls.Add(this.labelNoFiles);
      this.tabPage2.Location = new System.Drawing.Point(4, 24);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(424, 162);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabFiles";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // listFiles
      // 
      this.listFiles.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.listFiles.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listFiles.Location = new System.Drawing.Point(0, 0);
      this.listFiles.Margin = new System.Windows.Forms.Padding(0);
      this.listFiles.Name = "listFiles";
      this.listFiles.Size = new System.Drawing.Size(422, 160);
      this.listFiles.TabIndex = 2;
      this.listFiles.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.listFiles.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // labelNoFiles
      // 
      this.labelNoFiles.BackColor = System.Drawing.Color.White;
      this.labelNoFiles.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelNoFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point);
      this.labelNoFiles.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.labelNoFiles.Location = new System.Drawing.Point(0, 0);
      this.labelNoFiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelNoFiles.Name = "labelNoFiles";
      this.labelNoFiles.Size = new System.Drawing.Size(422, 160);
      this.labelNoFiles.TabIndex = 1;
      this.labelNoFiles.Text = "none";
      this.labelNoFiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tabPage3
      // 
      this.tabPage3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tabPage3.Controls.Add(this.listItems);
      this.tabPage3.Controls.Add(this.labelNoItems);
      this.tabPage3.Location = new System.Drawing.Point(4, 24);
      this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(424, 162);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "tabItems";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // listItems
      // 
      this.listItems.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.listItems.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listItems.Location = new System.Drawing.Point(0, 0);
      this.listItems.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.listItems.Name = "listItems";
      this.listItems.Size = new System.Drawing.Size(422, 160);
      this.listItems.TabIndex = 2;
      this.listItems.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.listItems.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // labelNoItems
      // 
      this.labelNoItems.BackColor = System.Drawing.Color.White;
      this.labelNoItems.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelNoItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point);
      this.labelNoItems.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.labelNoItems.Location = new System.Drawing.Point(0, 0);
      this.labelNoItems.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelNoItems.Name = "labelNoItems";
      this.labelNoItems.Size = new System.Drawing.Size(422, 160);
      this.labelNoItems.TabIndex = 1;
      this.labelNoItems.Text = "none";
      this.labelNoItems.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label2.Location = new System.Drawing.Point(3, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(102, 28);
      this.label2.TabIndex = 15;
      this.label2.Text = "Tags";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tagsLabel
      // 
      this.tagsLabel.AutoSize = true;
      this.tagsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tagsLabel.Location = new System.Drawing.Point(111, 56);
      this.tagsLabel.Name = "tagsLabel";
      this.tagsLabel.Size = new System.Drawing.Size(262, 28);
      this.tagsLabel.TabIndex = 16;
      this.tagsLabel.Text = "Tags";
      this.tagsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // PagePackageInformation
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "PagePackageInformation";
      this.Size = new System.Drawing.Size(432, 335);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.stackedWidget.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label labelDisplayedName;
    private System.Windows.Forms.Label labelIdentifyingName;
    private System.Windows.Forms.Label labelDescription;
    private System.Windows.Forms.Label labelStackedWidgetTitle;
    private System.Windows.Forms.Button buttonLeft;
    private System.Windows.Forms.Button buttonRight;
    private Lemoine.BaseControls.StackedWidget stackedWidget;
    private System.Windows.Forms.TabPage tabPage1;
    private Lemoine.BaseControls.VerticalScrollLayout listPlugins;
    private System.Windows.Forms.Label labelNoPlugins;
    private System.Windows.Forms.TabPage tabPage2;
    private Lemoine.BaseControls.VerticalScrollLayout listFiles;
    private System.Windows.Forms.TabPage tabPage3;
    private Lemoine.BaseControls.VerticalScrollLayout listItems;
    private System.Windows.Forms.Label labelNoItems;
    private System.Windows.Forms.Label labelNoFiles;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label tagsLabel;
  }
}
