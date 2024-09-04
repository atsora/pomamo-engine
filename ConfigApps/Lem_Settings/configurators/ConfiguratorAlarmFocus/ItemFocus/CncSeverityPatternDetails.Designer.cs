// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarmFocus
{
  partial class CncSeverityPatternDetails
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Button buttonEdit;
    private System.Windows.Forms.Button buttonRemove;
    private System.Windows.Forms.Label labelPattern;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CncSeverityPatternDetails));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonEdit = new System.Windows.Forms.Button();
      this.buttonRemove = new System.Windows.Forms.Button();
      this.labelPattern = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.Controls.Add(this.buttonEdit, 1, 0);
      this.baseLayout.Controls.Add(this.buttonRemove, 2, 0);
      this.baseLayout.Controls.Add(this.labelPattern, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(186, 37);
      this.baseLayout.TabIndex = 0;
      // 
      // buttonEdit
      // 
      this.buttonEdit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonEdit.BackgroundImage")));
      this.buttonEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.buttonEdit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonEdit.Location = new System.Drawing.Point(106, 0);
      this.buttonEdit.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.buttonEdit.Name = "buttonEdit";
      this.buttonEdit.Size = new System.Drawing.Size(37, 34);
      this.buttonEdit.TabIndex = 0;
      this.buttonEdit.UseVisualStyleBackColor = true;
      this.buttonEdit.Click += new System.EventHandler(this.ButtonEditClick);
      // 
      // buttonRemove
      // 
      this.buttonRemove.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonRemove.BackgroundImage")));
      this.buttonRemove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.buttonRemove.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRemove.Location = new System.Drawing.Point(146, 0);
      this.buttonRemove.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(37, 34);
      this.buttonRemove.TabIndex = 1;
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
      // 
      // labelPattern
      // 
      this.labelPattern.AutoEllipsis = true;
      this.labelPattern.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPattern.Location = new System.Drawing.Point(3, 0);
      this.labelPattern.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.labelPattern.Name = "labelPattern";
      this.labelPattern.Size = new System.Drawing.Size(100, 34);
      this.labelPattern.TabIndex = 2;
      this.labelPattern.Text = "Pattern...";
      this.labelPattern.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // CncSeverityPatternDetails
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "CncSeverityPatternDetails";
      this.Size = new System.Drawing.Size(186, 37);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
