// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardAddReason
{
  partial class PageSelectableReason
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
      this.checkDetails = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.listMachineFilters = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Controls.Add(this.checkDetails, 1, 0);
      this.baseLayout.Controls.Add(this.label2, 0, 0);
      this.baseLayout.Controls.Add(this.listMachineFilters, 1, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
      // 
      // checkDetails
      // 
      this.checkDetails.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkDetails.Location = new System.Drawing.Point(93, 3);
      this.checkDetails.Name = "checkDetails";
      this.checkDetails.Size = new System.Drawing.Size(274, 19);
      this.checkDetails.TabIndex = 0;
      this.checkDetails.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 28);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(90, 262);
      this.label1.TabIndex = 1;
      this.label1.Text = "Machine filter";
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(90, 25);
      this.label2.TabIndex = 2;
      this.label2.Text = "Details required";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // listMachineFilters
      // 
      this.listMachineFilters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listMachineFilters.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listMachineFilters.Location = new System.Drawing.Point(90, 25);
      this.listMachineFilters.Margin = new System.Windows.Forms.Padding(0);
      this.listMachineFilters.Name = "listMachineFilters";
      this.listMachineFilters.Size = new System.Drawing.Size(280, 265);
      this.listMachineFilters.Sorted = true;
      this.listMachineFilters.TabIndex = 3;
      // 
      // PageSelectableReason
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PageSelectableReason";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox checkDetails;
    private System.Windows.Forms.Label label2;
    private Lemoine.BaseControls.List.ListTextValue listMachineFilters;
  }
}
