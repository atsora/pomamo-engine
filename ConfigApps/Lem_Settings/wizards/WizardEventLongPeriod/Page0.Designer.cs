// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardEventLongPeriod
{
  partial class Page0
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
      this.radioAddOnly = new System.Windows.Forms.RadioButton();
      this.radioDeleteAndAdd = new System.Windows.Forms.RadioButton();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 265F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.radioAddOnly, 1, 1);
      this.baseLayout.Controls.Add(this.radioDeleteAndAdd, 1, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
      // 
      // radioAddOnly
      // 
      this.radioAddOnly.AutoCheck = false;
      this.radioAddOnly.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioAddOnly.Location = new System.Drawing.Point(55, 123);
      this.radioAddOnly.Name = "radioAddOnly";
      this.radioAddOnly.Size = new System.Drawing.Size(259, 19);
      this.radioAddOnly.TabIndex = 0;
      this.radioAddOnly.TabStop = true;
      this.radioAddOnly.Text = "Add long period events, overwrite if needed";
      this.radioAddOnly.UseVisualStyleBackColor = true;
      this.radioAddOnly.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RadioAddOnlyMouseClick);
      // 
      // radioDeleteAndAdd
      // 
      this.radioDeleteAndAdd.AutoCheck = false;
      this.radioDeleteAndAdd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioDeleteAndAdd.Location = new System.Drawing.Point(55, 148);
      this.radioDeleteAndAdd.Name = "radioDeleteAndAdd";
      this.radioDeleteAndAdd.Size = new System.Drawing.Size(259, 19);
      this.radioDeleteAndAdd.TabIndex = 1;
      this.radioDeleteAndAdd.TabStop = true;
      this.radioDeleteAndAdd.Text = "First clear all and then add long period events";
      this.radioDeleteAndAdd.UseVisualStyleBackColor = true;
      this.radioDeleteAndAdd.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RadioDeleteAndAddMouseClick);
      // 
      // Page0
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page0";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.RadioButton radioAddOnly;
    private System.Windows.Forms.RadioButton radioDeleteAndAdd;
  }
}
