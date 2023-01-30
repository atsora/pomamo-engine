// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarms
{
  partial class AlarmCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelLine1;
    private System.Windows.Forms.Label labelLine2;
    private System.Windows.Forms.Label labelLine3;
    private System.Windows.Forms.Label labelLine4;
    private System.Windows.Forms.Label labelLine5;
    private System.Windows.Forms.Button buttonEdit;
    private System.Windows.Forms.Button buttonRemove;
    
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
      this.labelLine1 = new System.Windows.Forms.Label();
      this.labelLine2 = new System.Windows.Forms.Label();
      this.labelLine3 = new System.Windows.Forms.Label();
      this.labelLine4 = new System.Windows.Forms.Label();
      this.labelLine5 = new System.Windows.Forms.Label();
      this.buttonRemove = new System.Windows.Forms.Button();
      this.buttonEdit = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.AutoSize = true;
      this.baseLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.Controls.Add(this.labelLine1, 0, 0);
      this.baseLayout.Controls.Add(this.labelLine2, 0, 1);
      this.baseLayout.Controls.Add(this.labelLine3, 0, 2);
      this.baseLayout.Controls.Add(this.labelLine4, 0, 3);
      this.baseLayout.Controls.Add(this.labelLine5, 0, 4);
      this.baseLayout.Controls.Add(this.buttonRemove, 1, 4);
      this.baseLayout.Controls.Add(this.buttonEdit, 1, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 5;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.baseLayout.Size = new System.Drawing.Size(200, 101);
      this.baseLayout.TabIndex = 9;
      // 
      // labelLine1
      // 
      this.labelLine1.AutoEllipsis = true;
      this.baseLayout.SetColumnSpan(this.labelLine1, 2);
      this.labelLine1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelLine1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelLine1.Location = new System.Drawing.Point(3, 0);
      this.labelLine1.Name = "labelLine1";
      this.labelLine1.Size = new System.Drawing.Size(194, 20);
      this.labelLine1.TabIndex = 0;
      this.labelLine1.Text = "label1";
      this.labelLine1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.labelLine1.UseCompatibleTextRendering = true;
      // 
      // labelLine2
      // 
      this.labelLine2.AutoEllipsis = true;
      this.baseLayout.SetColumnSpan(this.labelLine2, 2);
      this.labelLine2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelLine2.Location = new System.Drawing.Point(3, 20);
      this.labelLine2.Name = "labelLine2";
      this.labelLine2.Size = new System.Drawing.Size(194, 20);
      this.labelLine2.TabIndex = 1;
      this.labelLine2.Text = "label2";
      this.labelLine2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.labelLine2.UseCompatibleTextRendering = true;
      // 
      // labelLine3
      // 
      this.labelLine3.AutoEllipsis = true;
      this.baseLayout.SetColumnSpan(this.labelLine3, 2);
      this.labelLine3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelLine3.Location = new System.Drawing.Point(3, 40);
      this.labelLine3.Name = "labelLine3";
      this.labelLine3.Size = new System.Drawing.Size(194, 20);
      this.labelLine3.TabIndex = 2;
      this.labelLine3.Text = "label3";
      this.labelLine3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.labelLine3.UseCompatibleTextRendering = true;
      // 
      // labelLine4
      // 
      this.labelLine4.AutoEllipsis = true;
      this.labelLine4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelLine4.Location = new System.Drawing.Point(3, 60);
      this.labelLine4.Name = "labelLine4";
      this.labelLine4.Size = new System.Drawing.Size(134, 20);
      this.labelLine4.TabIndex = 3;
      this.labelLine4.Text = "label4";
      this.labelLine4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.labelLine4.UseCompatibleTextRendering = true;
      // 
      // labelLine5
      // 
      this.labelLine5.AutoEllipsis = true;
      this.labelLine5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelLine5.Location = new System.Drawing.Point(3, 80);
      this.labelLine5.Name = "labelLine5";
      this.labelLine5.Size = new System.Drawing.Size(134, 21);
      this.labelLine5.TabIndex = 8;
      this.labelLine5.Text = "label5";
      this.labelLine5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.labelLine5.UseCompatibleTextRendering = true;
      // 
      // buttonRemove
      // 
      this.buttonRemove.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRemove.Location = new System.Drawing.Point(140, 80);
      this.buttonRemove.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(57, 21);
      this.buttonRemove.TabIndex = 1;
      this.buttonRemove.Text = "Remove";
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
      // 
      // buttonEdit
      // 
      this.buttonEdit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonEdit.Location = new System.Drawing.Point(140, 60);
      this.buttonEdit.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.buttonEdit.Name = "buttonEdit";
      this.buttonEdit.Size = new System.Drawing.Size(57, 20);
      this.buttonEdit.TabIndex = 2;
      this.buttonEdit.Text = "Edit";
      this.buttonEdit.UseVisualStyleBackColor = true;
      this.buttonEdit.Click += new System.EventHandler(this.ButtonEditClick);
      // 
      // AlarmCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.MinimumSize = new System.Drawing.Size(200, 101);
      this.Name = "AlarmCell";
      this.Size = new System.Drawing.Size(200, 101);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
  }
}
