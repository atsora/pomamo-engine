// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewLine
{
  partial class OperationDetails
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
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.labelName = new System.Windows.Forms.Label();
      this.labelCode = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.labelParts = new System.Windows.Forms.Label();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.labelName, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.labelCode, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.labelParts, 1, 3);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 5;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(150, 150);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 5);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(90, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Name";
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 25);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(90, 20);
      this.label3.TabIndex = 2;
      this.label3.Text = "Code";
      // 
      // labelName
      // 
      this.labelName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelName.Location = new System.Drawing.Point(99, 5);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(48, 20);
      this.labelName.TabIndex = 1;
      this.labelName.Text = "labelName";
      // 
      // labelCode
      // 
      this.labelCode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelCode.Location = new System.Drawing.Point(99, 25);
      this.labelCode.Name = "labelCode";
      this.labelCode.Size = new System.Drawing.Size(48, 20);
      this.labelCode.TabIndex = 3;
      this.labelCode.Text = "labelCode";
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 45);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(90, 20);
      this.label2.TabIndex = 4;
      this.label2.Text = "Max parts / cycle";
      // 
      // labelParts
      // 
      this.labelParts.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelParts.Location = new System.Drawing.Point(99, 45);
      this.labelParts.Name = "labelParts";
      this.labelParts.Size = new System.Drawing.Size(48, 20);
      this.labelParts.TabIndex = 5;
      this.labelParts.Text = "labelParts";
      // 
      // OperationDetails
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "OperationDetails";
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label labelParts;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelCode;
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
