// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorLine
{
  partial class Page4
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
      this.checkPart = new System.Windows.Forms.CheckBox();
      this.checkOperations = new System.Windows.Forms.CheckBox();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.label1, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.checkPart, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.checkOperations, 1, 3);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 5;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(350, 250);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(95, 87);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(157, 25);
      this.label1.TabIndex = 1;
      this.label1.Text = "Delete line";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkPart
      // 
      this.checkPart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkPart.Location = new System.Drawing.Point(98, 115);
      this.checkPart.Name = "checkPart";
      this.checkPart.Size = new System.Drawing.Size(154, 19);
      this.checkPart.TabIndex = 2;
      this.checkPart.Text = "with produced part(s)";
      this.checkPart.UseVisualStyleBackColor = true;
      // 
      // checkOperations
      // 
      this.checkOperations.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkOperations.Location = new System.Drawing.Point(98, 140);
      this.checkOperations.Name = "checkOperations";
      this.checkOperations.Size = new System.Drawing.Size(154, 19);
      this.checkOperations.TabIndex = 4;
      this.checkOperations.Text = "with associated operations";
      this.checkOperations.UseVisualStyleBackColor = true;
      // 
      // Page4
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "Page4";
      this.Size = new System.Drawing.Size(350, 250);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.CheckBox checkOperations;
    private System.Windows.Forms.CheckBox checkPart;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label label1;
  }
}
