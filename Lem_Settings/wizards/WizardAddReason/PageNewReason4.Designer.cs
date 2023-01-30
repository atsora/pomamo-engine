// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardAddReason
{
  partial class PageNewReason4
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
      this.radioNone = new System.Windows.Forms.RadioButton();
      this.radioRight = new System.Windows.Forms.RadioButton();
      this.radioLeft = new System.Windows.Forms.RadioButton();
      this.label1 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 239F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.radioNone, 1, 2);
      this.baseLayout.Controls.Add(this.radioRight, 1, 3);
      this.baseLayout.Controls.Add(this.radioLeft, 1, 4);
      this.baseLayout.Controls.Add(this.label1, 1, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
      // 
      // radioNone
      // 
      this.radioNone.Checked = true;
      this.radioNone.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioNone.Location = new System.Drawing.Point(68, 108);
      this.radioNone.Name = "radioNone";
      this.radioNone.Size = new System.Drawing.Size(233, 34);
      this.radioNone.TabIndex = 0;
      this.radioNone.TabStop = true;
      this.radioNone.Text = "no changes regarding the previous and following operation periods";
      this.radioNone.UseVisualStyleBackColor = true;
      // 
      // radioRight
      // 
      this.radioRight.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioRight.Location = new System.Drawing.Point(68, 148);
      this.radioRight.Name = "radioRight";
      this.radioRight.Size = new System.Drawing.Size(233, 34);
      this.radioRight.TabIndex = 1;
      this.radioRight.Text = "the period of the operation to come will be extended to include this reason";
      this.radioRight.UseVisualStyleBackColor = true;
      // 
      // radioLeft
      // 
      this.radioLeft.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioLeft.Location = new System.Drawing.Point(68, 188);
      this.radioLeft.Name = "radioLeft";
      this.radioLeft.Size = new System.Drawing.Size(233, 34);
      this.radioLeft.TabIndex = 2;
      this.radioLeft.Text = "the period of the previous operation will be extended to include this reason";
      this.radioLeft.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(68, 65);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(233, 40);
      this.label1.TabIndex = 3;
      this.label1.Text = "If this reason is detected:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // PageNewReason4
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PageNewReason4";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.RadioButton radioNone;
    private System.Windows.Forms.RadioButton radioRight;
    private System.Windows.Forms.RadioButton radioLeft;
    private System.Windows.Forms.Label label1;
  }
}
