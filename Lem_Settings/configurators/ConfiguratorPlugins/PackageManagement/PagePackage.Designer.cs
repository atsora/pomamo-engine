// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorPlugins
{
  partial class PagePackage
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PagePackage));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonAdd = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.verticalScrollLayout = new Lemoine.BaseControls.VerticalScrollLayout();
      this.labelNoPackages = new System.Windows.Forms.Label();
      this.tagsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
      this.baseLayout.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.Controls.Add(this.buttonAdd, 1, 2);
      this.baseLayout.Controls.Add(this.panel1, 0, 1);
      this.baseLayout.Controls.Add(this.tagsFlowLayoutPanel, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
      this.baseLayout.Size = new System.Drawing.Size(432, 335);
      this.baseLayout.TabIndex = 3;
      // 
      // buttonAdd
      // 
      this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonAdd.Image")));
      this.buttonAdd.Location = new System.Drawing.Point(404, 307);
      this.buttonAdd.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new System.Drawing.Size(28, 28);
      this.buttonAdd.TabIndex = 0;
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
      // 
      // panel1
      // 
      this.baseLayout.SetColumnSpan(this.panel1, 2);
      this.panel1.Controls.Add(this.verticalScrollLayout);
      this.panel1.Controls.Add(this.labelNoPackages);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 31);
      this.panel1.Margin = new System.Windows.Forms.Padding(0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(432, 273);
      this.panel1.TabIndex = 3;
      // 
      // verticalScrollLayout
      // 
      this.verticalScrollLayout.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScrollLayout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.verticalScrollLayout.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLayout.Location = new System.Drawing.Point(0, 0);
      this.verticalScrollLayout.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Name = "verticalScrollLayout";
      this.verticalScrollLayout.Size = new System.Drawing.Size(432, 273);
      this.verticalScrollLayout.TabIndex = 2;
      this.verticalScrollLayout.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.verticalScrollLayout.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // labelNoPackages
      // 
      this.labelNoPackages.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelNoPackages.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point);
      this.labelNoPackages.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.labelNoPackages.Location = new System.Drawing.Point(0, 0);
      this.labelNoPackages.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelNoPackages.Name = "labelNoPackages";
      this.labelNoPackages.Size = new System.Drawing.Size(432, 273);
      this.labelNoPackages.TabIndex = 0;
      this.labelNoPackages.Text = "no packages";
      this.labelNoPackages.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tagsFlowLayoutPanel
      // 
      this.tagsFlowLayoutPanel.AutoScroll = true;
      this.tagsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tagsFlowLayoutPanel.Location = new System.Drawing.Point(3, 3);
      this.tagsFlowLayoutPanel.Name = "tagsFlowLayoutPanel";
      this.tagsFlowLayoutPanel.Size = new System.Drawing.Size(398, 25);
      this.tagsFlowLayoutPanel.TabIndex = 4;
      // 
      // PagePackage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "PagePackage";
      this.Size = new System.Drawing.Size(432, 335);
      this.baseLayout.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Button buttonAdd;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLayout;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label labelNoPackages;
    private System.Windows.Forms.FlowLayoutPanel tagsFlowLayoutPanel;
  }
}
