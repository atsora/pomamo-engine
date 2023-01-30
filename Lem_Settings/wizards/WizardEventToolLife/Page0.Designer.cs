// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardEventToolLife
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
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.radioDeleteAndAdd = new System.Windows.Forms.RadioButton();
      this.radioAddOnly = new System.Windows.Forms.RadioButton();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.dataGrid = new System.Windows.Forms.DataGridView();
      this.radioDeleteOnly = new System.Windows.Forms.RadioButton();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 113);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(364, 25);
      this.label2.TabIndex = 3;
      this.label2.Text = "Old configuration";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(364, 25);
      this.label1.TabIndex = 2;
      this.label1.Text = "Choose an option for the previously configured tool life events";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // radioDeleteAndAdd
      // 
      this.radioDeleteAndAdd.AutoCheck = false;
      this.radioDeleteAndAdd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioDeleteAndAdd.Location = new System.Drawing.Point(15, 53);
      this.radioDeleteAndAdd.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
      this.radioDeleteAndAdd.Name = "radioDeleteAndAdd";
      this.radioDeleteAndAdd.Size = new System.Drawing.Size(352, 19);
      this.radioDeleteAndAdd.TabIndex = 1;
      this.radioDeleteAndAdd.TabStop = true;
      this.radioDeleteAndAdd.Text = "First clear all and then add tool life events";
      this.radioDeleteAndAdd.UseVisualStyleBackColor = true;
      this.radioDeleteAndAdd.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RadioDeleteAndAddMouseClick);
      // 
      // radioAddOnly
      // 
      this.radioAddOnly.AutoCheck = false;
      this.radioAddOnly.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioAddOnly.Location = new System.Drawing.Point(15, 28);
      this.radioAddOnly.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
      this.radioAddOnly.Name = "radioAddOnly";
      this.radioAddOnly.Size = new System.Drawing.Size(352, 19);
      this.radioAddOnly.TabIndex = 0;
      this.radioAddOnly.TabStop = true;
      this.radioAddOnly.Text = "Add tool life events, overwrite if needed";
      this.radioAddOnly.UseVisualStyleBackColor = true;
      this.radioAddOnly.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RadioAddOnlyMouseClick);
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 370F));
      this.baseLayout.Controls.Add(this.radioAddOnly, 0, 1);
      this.baseLayout.Controls.Add(this.radioDeleteAndAdd, 0, 2);
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 0, 5);
      this.baseLayout.Controls.Add(this.dataGrid, 0, 6);
      this.baseLayout.Controls.Add(this.radioDeleteOnly, 0, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 7;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
      // 
      // dataGrid
      // 
      this.dataGrid.AllowUserToAddRows = false;
      this.dataGrid.AllowUserToDeleteRows = false;
      this.dataGrid.AllowUserToResizeRows = false;
      this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGrid.Location = new System.Drawing.Point(3, 141);
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.Size = new System.Drawing.Size(364, 146);
      this.dataGrid.TabIndex = 4;
      // 
      // radioDeleteOnly
      // 
      this.radioDeleteOnly.AutoCheck = false;
      this.radioDeleteOnly.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioDeleteOnly.Location = new System.Drawing.Point(15, 78);
      this.radioDeleteOnly.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
      this.radioDeleteOnly.Name = "radioDeleteOnly";
      this.radioDeleteOnly.Size = new System.Drawing.Size(352, 19);
      this.radioDeleteOnly.TabIndex = 5;
      this.radioDeleteOnly.TabStop = true;
      this.radioDeleteOnly.Text = "Just clear all";
      this.radioDeleteOnly.UseVisualStyleBackColor = true;
      this.radioDeleteOnly.Click += new System.EventHandler(this.RadioDeleteOnlyClick);
      // 
      // Page0
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page0";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.RadioButton radioAddOnly;
    private System.Windows.Forms.RadioButton radioDeleteAndAdd;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.DataGridView dataGrid;
    private System.Windows.Forms.RadioButton radioDeleteOnly;
  }
}
