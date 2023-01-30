// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorTemplate
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
      this.buttonPageOk = new System.Windows.Forms.Button();
      this.labelTime = new System.Windows.Forms.Label();
      this.buttonPageNoOk = new System.Windows.Forms.Button();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonPageOk
      // 
      this.buttonPageOk.Location = new System.Drawing.Point(3, 3);
      this.buttonPageOk.Name = "buttonPageOk";
      this.buttonPageOk.Size = new System.Drawing.Size(94, 23);
      this.buttonPageOk.TabIndex = 0;
      this.buttonPageOk.Text = "Page - ok";
      this.buttonPageOk.UseVisualStyleBackColor = true;
      this.buttonPageOk.Click += new System.EventHandler(this.ButtonPageOkClick);
      // 
      // labelTime
      // 
      this.labelTime.Location = new System.Drawing.Point(3, 145);
      this.labelTime.Name = "labelTime";
      this.labelTime.Size = new System.Drawing.Size(169, 23);
      this.labelTime.TabIndex = 1;
      this.labelTime.Text = "-";
      // 
      // buttonPageNoOk
      // 
      this.buttonPageNoOk.Location = new System.Drawing.Point(188, 3);
      this.buttonPageNoOk.Name = "buttonPageNoOk";
      this.buttonPageNoOk.Size = new System.Drawing.Size(94, 23);
      this.buttonPageNoOk.TabIndex = 2;
      this.buttonPageNoOk.Text = "Page - no ok";
      this.buttonPageNoOk.UseVisualStyleBackColor = true;
      this.buttonPageNoOk.Click += new System.EventHandler(this.ButtonPageNoOkClick);
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.labelTime, 0, 1);
      this.baseLayout.Controls.Add(this.buttonPageNoOk, 1, 0);
      this.baseLayout.Controls.Add(this.buttonPageOk, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Button buttonPageNoOk;
    private System.Windows.Forms.Label labelTime;
    private System.Windows.Forms.Button buttonPageOk;
  }
}
