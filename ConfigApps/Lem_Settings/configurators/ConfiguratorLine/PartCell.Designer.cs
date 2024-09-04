// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorLine
{
  partial class PartCell
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
      this.labelPartNumber = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.textName = new System.Windows.Forms.TextBox();
      this.textCode = new System.Windows.Forms.TextBox();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.labelPartNumber, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 0, 1);
      this.baseLayout.Controls.Add(this.label3, 0, 2);
      this.baseLayout.Controls.Add(this.textName, 1, 1);
      this.baseLayout.Controls.Add(this.textCode, 1, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.Size = new System.Drawing.Size(150, 78);
      this.baseLayout.TabIndex = 0;
      // 
      // labelPartNumber
      // 
      this.baseLayout.SetColumnSpan(this.labelPartNumber, 2);
      this.labelPartNumber.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPartNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelPartNumber.Location = new System.Drawing.Point(20, 0);
      this.labelPartNumber.Margin = new System.Windows.Forms.Padding(20, 0, 3, 0);
      this.labelPartNumber.Name = "labelPartNumber";
      this.labelPartNumber.Size = new System.Drawing.Size(127, 26);
      this.labelPartNumber.TabIndex = 0;
      this.labelPartNumber.Text = "labelPartNumber";
      this.labelPartNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 26);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(44, 26);
      this.label2.TabIndex = 1;
      this.label2.Text = "Name";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 52);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(44, 26);
      this.label3.TabIndex = 2;
      this.label3.Text = "Code";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textName
      // 
      this.textName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textName.Location = new System.Drawing.Point(53, 29);
      this.textName.Name = "textName";
      this.textName.Size = new System.Drawing.Size(94, 20);
      this.textName.TabIndex = 3;
      this.textName.TextChanged += new System.EventHandler(this.TextNameTextChanged);
      // 
      // textCode
      // 
      this.textCode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textCode.Location = new System.Drawing.Point(53, 55);
      this.textCode.Name = "textCode";
      this.textCode.Size = new System.Drawing.Size(94, 20);
      this.textCode.TabIndex = 4;
      this.textCode.TextChanged += new System.EventHandler(this.TextCodeTextChanged);
      // 
      // PartCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PartCell";
      this.Size = new System.Drawing.Size(150, 78);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.TextBox textCode;
    private System.Windows.Forms.TextBox textName;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelPartNumber;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
