// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorPlugins
{
  partial class PagePluginList
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.verticalScrollLayout = new Lemoine.BaseControls.VerticalScrollLayout();
      this.labelNoPlugins = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 370F));
      this.baseLayout.Controls.Add(this.panel1, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.verticalScrollLayout);
      this.panel1.Controls.Add(this.labelNoPlugins);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Margin = new System.Windows.Forms.Padding(0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(370, 290);
      this.panel1.TabIndex = 3;
      // 
      // verticalScrollLayout
      // 
      this.verticalScrollLayout.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScrollLayout.ColumnCount = 2;
      this.verticalScrollLayout.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLayout.Location = new System.Drawing.Point(0, 0);
      this.verticalScrollLayout.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Name = "verticalScrollLayout";
      this.verticalScrollLayout.RowCount = 2;
      this.verticalScrollLayout.Size = new System.Drawing.Size(370, 290);
      this.verticalScrollLayout.TabIndex = 2;
      this.verticalScrollLayout.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLayout.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // labelNoPlugins
      // 
      this.labelNoPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelNoPlugins.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelNoPlugins.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.labelNoPlugins.Location = new System.Drawing.Point(0, 0);
      this.labelNoPlugins.Name = "labelNoPlugins";
      this.labelNoPlugins.Size = new System.Drawing.Size(370, 290);
      this.labelNoPlugins.TabIndex = 0;
      this.labelNoPlugins.Text = "No plugins";
      this.labelNoPlugins.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // PagePluginList
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PagePluginList";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLayout;
    private System.Windows.Forms.Label labelNoPlugins;
    private System.Windows.Forms.Panel panel1;
  }
}
