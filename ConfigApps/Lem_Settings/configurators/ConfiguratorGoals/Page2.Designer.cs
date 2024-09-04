// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorGoals
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
      this.buttonDepartment = new System.Windows.Forms.Button();
      this.buttonCategory = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.buttonDepartment, 0, 2);
      this.baseLayout.Controls.Add(this.buttonCategory, 0, 3);
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 5;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 1;
      // 
      // buttonDepartment
      // 
      this.buttonDepartment.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDepartment.Location = new System.Drawing.Point(0, 134);
      this.buttonDepartment.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.buttonDepartment.Name = "buttonDepartment";
      this.buttonDepartment.Size = new System.Drawing.Size(370, 22);
      this.buttonDepartment.TabIndex = 0;
      this.buttonDepartment.Text = "Company    >    Department    >         Cell         >    Machine";
      this.buttonDepartment.UseVisualStyleBackColor = true;
      this.buttonDepartment.Click += new System.EventHandler(this.ButtonDepartmentClick);
      // 
      // buttonCategory
      // 
      this.buttonCategory.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCategory.Location = new System.Drawing.Point(0, 159);
      this.buttonCategory.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.buttonCategory.Name = "buttonCategory";
      this.buttonCategory.Size = new System.Drawing.Size(370, 22);
      this.buttonCategory.TabIndex = 1;
      this.buttonCategory.Text = "Company    >      Category      >  SubCategory  >    Machine";
      this.buttonCategory.UseVisualStyleBackColor = true;
      this.buttonCategory.Click += new System.EventHandler(this.ButtonCategoryClick);
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 108);
      this.label1.Margin = new System.Windows.Forms.Padding(0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(370, 23);
      this.label1.TabIndex = 2;
      this.label1.Text = "Assign goals by:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Button buttonCategory;
    private System.Windows.Forms.Button buttonDepartment;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
  }
}
