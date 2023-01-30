// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardRenameProduction
{
  partial class Page3
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
      this.textName = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.labelPreviousName = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.textName, 1, 2);
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Controls.Add(this.label2, 0, 2);
      this.baseLayout.Controls.Add(this.labelPreviousName, 1, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 0;
      // 
      // textName
      // 
      this.textName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textName.Location = new System.Drawing.Point(83, 148);
      this.textName.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.textName.Name = "textName";
      this.textName.Size = new System.Drawing.Size(287, 20);
      this.textName.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 122);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(77, 23);
      this.label1.TabIndex = 1;
      this.label1.Text = "Previous name";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 145);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(77, 23);
      this.label2.TabIndex = 2;
      this.label2.Text = "New name";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelPreviousName
      // 
      this.labelPreviousName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPreviousName.Location = new System.Drawing.Point(83, 122);
      this.labelPreviousName.Name = "labelPreviousName";
      this.labelPreviousName.Size = new System.Drawing.Size(284, 23);
      this.labelPreviousName.TabIndex = 3;
      this.labelPreviousName.Text = "labelPreviousName";
      this.labelPreviousName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label labelPreviousName;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textName;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
