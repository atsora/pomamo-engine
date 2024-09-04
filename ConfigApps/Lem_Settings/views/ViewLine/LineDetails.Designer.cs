// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewLine
{
  partial class LineDetails
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
      this.label3 = new System.Windows.Forms.Label();
      this.labelName = new System.Windows.Forms.Label();
      this.labelCode = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.listParts = new Lemoine.BaseControls.List.ListTextValue();
      this.listBoxPeriod = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Controls.Add(this.label3, 0, 2);
      this.baseLayout.Controls.Add(this.labelName, 1, 1);
      this.baseLayout.Controls.Add(this.labelCode, 1, 2);
      this.baseLayout.Controls.Add(this.label2, 0, 7);
      this.baseLayout.Controls.Add(this.label4, 0, 4);
      this.baseLayout.Controls.Add(this.listParts, 0, 5);
      this.baseLayout.Controls.Add(this.listBoxPeriod, 0, 8);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 9;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(249, 272);
      this.baseLayout.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 5);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(69, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Name";
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 25);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(69, 20);
      this.label3.TabIndex = 2;
      this.label3.Text = "Code";
      // 
      // labelName
      // 
      this.labelName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelName.Location = new System.Drawing.Point(78, 5);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(168, 20);
      this.labelName.TabIndex = 1;
      this.labelName.Text = "labelName";
      // 
      // labelCode
      // 
      this.labelCode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelCode.Location = new System.Drawing.Point(78, 25);
      this.labelCode.Name = "labelCode";
      this.labelCode.Size = new System.Drawing.Size(168, 20);
      this.labelCode.TabIndex = 3;
      this.labelCode.Text = "labelCode";
      // 
      // label2
      // 
      this.baseLayout.SetColumnSpan(this.label2, 2);
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(3, 135);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(243, 20);
      this.label2.TabIndex = 4;
      this.label2.Text = "Planned production periods";
      // 
      // label4
      // 
      this.baseLayout.SetColumnSpan(this.label4, 2);
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(3, 50);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(243, 20);
      this.label4.TabIndex = 6;
      this.label4.Text = "Produced part(s)";
      // 
      // listParts
      // 
      this.listParts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listParts, 2);
      this.listParts.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listParts.Location = new System.Drawing.Point(0, 70);
      this.listParts.Margin = new System.Windows.Forms.Padding(0);
      this.listParts.Name = "listParts";
      this.listParts.SelectedIndex = -1;
      this.listParts.SelectedIndexes = null;
      this.listParts.SelectedText = "";
      this.listParts.SelectedTexts = null;
      this.listParts.SelectedValue = null;
      this.listParts.SelectedValues = null;
      this.listParts.Size = new System.Drawing.Size(249, 60);
      this.listParts.Sorted = true;
      this.listParts.TabIndex = 7;
      // 
      // listBoxPeriod
      // 
      this.listBoxPeriod.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listBoxPeriod, 2);
      this.listBoxPeriod.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBoxPeriod.Location = new System.Drawing.Point(0, 155);
      this.listBoxPeriod.Margin = new System.Windows.Forms.Padding(0);
      this.listBoxPeriod.Name = "listBoxPeriod";
      this.listBoxPeriod.ReverseOrder = true;
      this.listBoxPeriod.SelectedIndex = -1;
      this.listBoxPeriod.SelectedIndexes = null;
      this.listBoxPeriod.SelectedText = "";
      this.listBoxPeriod.SelectedTexts = null;
      this.listBoxPeriod.SelectedValue = null;
      this.listBoxPeriod.SelectedValues = null;
      this.listBoxPeriod.Size = new System.Drawing.Size(249, 117);
      this.listBoxPeriod.Sorted = true;
      this.listBoxPeriod.TabIndex = 8;
      // 
      // LineDetails
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "LineDetails";
      this.Size = new System.Drawing.Size(249, 272);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.List.ListTextValue listParts;
    private System.Windows.Forms.Label label4;
    private Lemoine.BaseControls.List.ListTextValue listBoxPeriod;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelCode;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
