// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorPlugins
{
  partial class PagePluginConf
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
      panelParam.Controls.Clear();
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
      this.panelParam = new System.Windows.Forms.Panel();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // panelParam
      // 
      this.panelParam.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelParam.Location = new System.Drawing.Point(0, 24);
      this.panelParam.Margin = new System.Windows.Forms.Padding(0);
      this.panelParam.Name = "panelParam";
      this.panelParam.Size = new System.Drawing.Size(370, 266);
      this.panelParam.TabIndex = 1;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.panelParam, 0, 1);
      this.baseLayout.Controls.Add(this.nameTextBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 1;
      // 
      // nameTextBox
      // 
      this.nameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nameTextBox.Location = new System.Drawing.Point(3, 3);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(364, 20);
      this.nameTextBox.TabIndex = 2;
      // 
      // PagePluginConf
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PagePluginConf";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Panel panelParam;
    private System.Windows.Forms.TextBox nameTextBox;
  }
}
