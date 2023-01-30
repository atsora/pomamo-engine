// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorEventLevel
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.textBox = new System.Windows.Forms.TextBox();
      this.numeric = new System.Windows.Forms.NumericUpDown();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numeric)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Controls.Add(this.label2, 0, 2);
      this.baseLayout.Controls.Add(this.textBox, 1, 1);
      this.baseLayout.Controls.Add(this.numeric, 1, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 123);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(82, 22);
      this.label1.TabIndex = 0;
      this.label1.Text = "Name";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 145);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(82, 22);
      this.label2.TabIndex = 1;
      this.label2.Text = "Priority";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textBox
      // 
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.Location = new System.Drawing.Point(85, 123);
      this.textBox.Margin = new System.Windows.Forms.Padding(0);
      this.textBox.Name = "textBox";
      this.textBox.Size = new System.Drawing.Size(285, 20);
      this.textBox.TabIndex = 2;
      // 
      // numeric
      // 
      this.numeric.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numeric.Location = new System.Drawing.Point(85, 145);
      this.numeric.Margin = new System.Windows.Forms.Padding(0);
      this.numeric.Maximum = new decimal(new int[] {
      1000,
      0,
      0,
      0});
      this.numeric.Name = "numeric";
      this.numeric.Size = new System.Drawing.Size(285, 20);
      this.numeric.TabIndex = 3;
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numeric)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBox;
    private System.Windows.Forms.NumericUpDown numeric;
  }
}
