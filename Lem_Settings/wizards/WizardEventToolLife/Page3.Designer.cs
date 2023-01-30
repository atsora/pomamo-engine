// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardEventToolLife
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
      this.listMoss = new Lemoine.BaseControls.List.ListTextValue();
      this.checkAll = new System.Windows.Forms.CheckBox();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.listMoss, 0, 0);
      this.baseLayout.Controls.Add(this.checkAll, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 1;
      // 
      // listMoss
      // 
      this.listMoss.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listMoss.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listMoss.Location = new System.Drawing.Point(0, 0);
      this.listMoss.Margin = new System.Windows.Forms.Padding(0);
      this.listMoss.MultipleSelection = true;
      this.listMoss.Name = "listMoss";
      this.listMoss.Size = new System.Drawing.Size(370, 270);
      this.listMoss.Sorted = true;
      this.listMoss.TabIndex = 2;
      // 
      // checkAll
      // 
      this.checkAll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkAll.Location = new System.Drawing.Point(3, 273);
      this.checkAll.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.checkAll.Name = "checkAll";
      this.checkAll.Size = new System.Drawing.Size(364, 17);
      this.checkAll.TabIndex = 3;
      this.checkAll.Text = "All";
      this.checkAll.UseVisualStyleBackColor = true;
      this.checkAll.CheckedChanged += new System.EventHandler(this.CheckAllCheckedChanged);
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.List.ListTextValue listMoss;
    private System.Windows.Forms.CheckBox checkAll;
  }
}
