// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardChangeProductionQuantities
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
      this.radioYes = new System.Windows.Forms.RadioButton();
      this.labelPreviousTarget = new System.Windows.Forms.Label();
      this.numericQtt = new System.Windows.Forms.NumericUpDown();
      this.radioNo = new System.Windows.Forms.RadioButton();
      this.radioAuto = new System.Windows.Forms.RadioButton();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericQtt)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.radioYes, 0, 4);
      this.baseLayout.Controls.Add(this.labelPreviousTarget, 0, 1);
      this.baseLayout.Controls.Add(this.numericQtt, 1, 4);
      this.baseLayout.Controls.Add(this.radioNo, 0, 5);
      this.baseLayout.Controls.Add(this.radioAuto, 0, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 7;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 1;
      // 
      // radioYes
      // 
      this.radioYes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioYes.Location = new System.Drawing.Point(3, 158);
      this.radioYes.Name = "radioYes";
      this.radioYes.Size = new System.Drawing.Size(122, 18);
      this.radioYes.TabIndex = 1;
      this.radioYes.Text = "Custom";
      this.radioYes.UseVisualStyleBackColor = true;
      this.radioYes.CheckedChanged += new System.EventHandler(this.RadioYesCheckedChanged);
      // 
      // labelPreviousTarget
      // 
      this.baseLayout.SetColumnSpan(this.labelPreviousTarget, 2);
      this.labelPreviousTarget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPreviousTarget.Location = new System.Drawing.Point(0, 87);
      this.labelPreviousTarget.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelPreviousTarget.Name = "labelPreviousTarget";
      this.labelPreviousTarget.Size = new System.Drawing.Size(367, 24);
      this.labelPreviousTarget.TabIndex = 4;
      this.labelPreviousTarget.Text = "Previous quantity: ...";
      this.labelPreviousTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // numericQtt
      // 
      this.numericQtt.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericQtt.Enabled = false;
      this.numericQtt.Location = new System.Drawing.Point(131, 158);
      this.numericQtt.Maximum = new decimal(new int[] {
      1000000000,
      0,
      0,
      0});
      this.numericQtt.Name = "numericQtt";
      this.numericQtt.Size = new System.Drawing.Size(236, 20);
      this.numericQtt.TabIndex = 3;
      // 
      // radioNo
      // 
      this.baseLayout.SetColumnSpan(this.radioNo, 2);
      this.radioNo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioNo.Location = new System.Drawing.Point(3, 182);
      this.radioNo.Name = "radioNo";
      this.radioNo.Size = new System.Drawing.Size(364, 18);
      this.radioNo.TabIndex = 0;
      this.radioNo.Text = "Don\'t change";
      this.radioNo.UseVisualStyleBackColor = true;
      this.radioNo.CheckedChanged += new System.EventHandler(this.RadioNoCheckedChanged);
      // 
      // radioAuto
      // 
      this.radioAuto.Checked = true;
      this.baseLayout.SetColumnSpan(this.radioAuto, 2);
      this.radioAuto.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioAuto.Location = new System.Drawing.Point(3, 134);
      this.radioAuto.Name = "radioAuto";
      this.radioAuto.Size = new System.Drawing.Size(364, 18);
      this.radioAuto.TabIndex = 5;
      this.radioAuto.TabStop = true;
      this.radioAuto.Text = "Auto";
      this.radioAuto.UseVisualStyleBackColor = true;
      this.radioAuto.CheckedChanged += new System.EventHandler(this.RadioAutoCheckedChanged);
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericQtt)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.RadioButton radioAuto;
    private System.Windows.Forms.Label labelPreviousTarget;
    private System.Windows.Forms.NumericUpDown numericQtt;
    private System.Windows.Forms.RadioButton radioYes;
    private System.Windows.Forms.RadioButton radioNo;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
