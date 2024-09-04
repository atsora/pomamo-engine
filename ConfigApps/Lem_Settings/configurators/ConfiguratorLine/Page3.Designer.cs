// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorLine
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
      this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.numericPartsPerCycle = new System.Windows.Forms.NumericUpDown();
      this.textName = new System.Windows.Forms.TextBox();
      this.textCode = new System.Windows.Forms.TextBox();
      this.tableLayoutPanel3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericPartsPerCycle)).BeginInit();
      this.SuspendLayout();
      // 
      // tableLayoutPanel3
      // 
      this.tableLayoutPanel3.ColumnCount = 2;
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F));
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel3.Controls.Add(this.label1, 0, 1);
      this.tableLayoutPanel3.Controls.Add(this.label2, 0, 2);
      this.tableLayoutPanel3.Controls.Add(this.label3, 0, 3);
      this.tableLayoutPanel3.Controls.Add(this.numericPartsPerCycle, 1, 3);
      this.tableLayoutPanel3.Controls.Add(this.textName, 1, 1);
      this.tableLayoutPanel3.Controls.Add(this.textCode, 1, 2);
      this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel3.Name = "tableLayoutPanel3";
      this.tableLayoutPanel3.RowCount = 5;
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel3.Size = new System.Drawing.Size(350, 250);
      this.tableLayoutPanel3.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 87);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(92, 25);
      this.label1.TabIndex = 9;
      this.label1.Text = "Name";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 112);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(92, 25);
      this.label2.TabIndex = 11;
      this.label2.Text = "Code";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(0, 137);
      this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(92, 25);
      this.label3.TabIndex = 12;
      this.label3.Text = "Max parts / cycle";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // numericPartsPerCycle
      // 
      this.numericPartsPerCycle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericPartsPerCycle.Location = new System.Drawing.Point(95, 140);
      this.numericPartsPerCycle.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.numericPartsPerCycle.Maximum = new decimal(new int[] {
      100000,
      0,
      0,
      0});
      this.numericPartsPerCycle.Minimum = new decimal(new int[] {
      1,
      0,
      0,
      0});
      this.numericPartsPerCycle.Name = "numericPartsPerCycle";
      this.numericPartsPerCycle.Size = new System.Drawing.Size(255, 20);
      this.numericPartsPerCycle.TabIndex = 10;
      this.numericPartsPerCycle.Value = new decimal(new int[] {
      1,
      0,
      0,
      0});
      // 
      // textName
      // 
      this.textName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textName.Location = new System.Drawing.Point(95, 90);
      this.textName.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.textName.Name = "textName";
      this.textName.Size = new System.Drawing.Size(255, 20);
      this.textName.TabIndex = 13;
      // 
      // textCode
      // 
      this.textCode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textCode.Location = new System.Drawing.Point(95, 115);
      this.textCode.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.textCode.Name = "textCode";
      this.textCode.Size = new System.Drawing.Size(255, 20);
      this.textCode.TabIndex = 14;
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel3);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(350, 250);
      this.tableLayoutPanel3.ResumeLayout(false);
      this.tableLayoutPanel3.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericPartsPerCycle)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TextBox textCode;
    private System.Windows.Forms.TextBox textName;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown numericPartsPerCycle;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
  }
}
