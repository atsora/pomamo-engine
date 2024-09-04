// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorGoals
{
  partial class CellResetGoal
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelCompany;
    private System.Windows.Forms.NumericUpDown numericUpDown;
    
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
      this.labelCompany = new System.Windows.Forms.Label();
      this.numericUpDown = new System.Windows.Forms.NumericUpDown();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 109F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.labelCompany, 0, 0);
      this.baseLayout.Controls.Add(this.numericUpDown, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(233, 23);
      this.baseLayout.TabIndex = 0;
      // 
      // labelCompany
      // 
      this.labelCompany.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelCompany.Location = new System.Drawing.Point(0, 0);
      this.labelCompany.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelCompany.Name = "labelCompany";
      this.labelCompany.Size = new System.Drawing.Size(106, 23);
      this.labelCompany.TabIndex = 0;
      this.labelCompany.Text = "labelCompany";
      this.labelCompany.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // numericUpDown
      // 
      this.numericUpDown.DecimalPlaces = 2;
      this.numericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericUpDown.Location = new System.Drawing.Point(109, 0);
      this.numericUpDown.Margin = new System.Windows.Forms.Padding(0);
      this.numericUpDown.Maximum = new decimal(new int[] {
      100000,
      0,
      0,
      0});
      this.numericUpDown.Name = "numericUpDown";
      this.numericUpDown.Size = new System.Drawing.Size(124, 20);
      this.numericUpDown.TabIndex = 1;
      // 
      // CellResetGoal
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "CellResetGoal";
      this.Size = new System.Drawing.Size(233, 23);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown)).EndInit();
      this.ResumeLayout(false);

    }
  }
}
