// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardInitialConfiguration
{
  partial class DisplayCell
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
      this.labelItem = new System.Windows.Forms.Label();
      this.textBox = new System.Windows.Forms.TextBox();
      this.labelProperties = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 158F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.labelItem, 0, 0);
      this.baseLayout.Controls.Add(this.textBox, 1, 0);
      this.baseLayout.Controls.Add(this.labelProperties, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(243, 46);
      this.baseLayout.TabIndex = 0;
      // 
      // labelItem
      // 
      this.labelItem.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelItem.Location = new System.Drawing.Point(0, 0);
      this.labelItem.Margin = new System.Windows.Forms.Padding(0);
      this.labelItem.Name = "labelItem";
      this.labelItem.Size = new System.Drawing.Size(158, 23);
      this.labelItem.TabIndex = 0;
      this.labelItem.Text = "labelItem";
      this.labelItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textBox
      // 
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.Location = new System.Drawing.Point(161, 3);
      this.textBox.Name = "textBox";
      this.textBox.Size = new System.Drawing.Size(79, 20);
      this.textBox.TabIndex = 1;
      // 
      // labelProperties
      // 
      this.labelProperties.AutoEllipsis = true;
      this.baseLayout.SetColumnSpan(this.labelProperties, 2);
      this.labelProperties.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelProperties.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelProperties.ForeColor = System.Drawing.SystemColors.GrayText;
      this.labelProperties.Location = new System.Drawing.Point(0, 23);
      this.labelProperties.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelProperties.Name = "labelProperties";
      this.labelProperties.Size = new System.Drawing.Size(240, 23);
      this.labelProperties.TabIndex = 2;
      this.labelProperties.Text = "labelProperties";
      // 
      // DisplayCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.MinimumSize = new System.Drawing.Size(20, 20);
      this.Name = "DisplayCell";
      this.Size = new System.Drawing.Size(243, 46);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Label labelProperties;
    private System.Windows.Forms.TextBox textBox;
    private System.Windows.Forms.Label labelItem;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
