// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorGoals
{
  partial class CellGoalType
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
      this.labelGoalType = new System.Windows.Forms.Label();
      this.buttonEdit = new System.Windows.Forms.Button();
      this.buttonReset = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.baseLayout.Controls.Add(this.labelGoalType, 0, 0);
      this.baseLayout.Controls.Add(this.buttonEdit, 1, 0);
      this.baseLayout.Controls.Add(this.buttonReset, 2, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(269, 36);
      this.baseLayout.TabIndex = 0;
      // 
      // labelGoalType
      // 
      this.labelGoalType.AutoEllipsis = true;
      this.labelGoalType.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelGoalType.Location = new System.Drawing.Point(0, 0);
      this.labelGoalType.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.labelGoalType.Name = "labelGoalType";
      this.labelGoalType.Size = new System.Drawing.Size(126, 33);
      this.labelGoalType.TabIndex = 0;
      this.labelGoalType.Text = "labelGoalType";
      this.labelGoalType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonEdit
      // 
      this.buttonEdit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonEdit.Location = new System.Drawing.Point(132, 3);
      this.buttonEdit.Margin = new System.Windows.Forms.Padding(3, 3, 0, 6);
      this.buttonEdit.Name = "buttonEdit";
      this.buttonEdit.Size = new System.Drawing.Size(67, 27);
      this.buttonEdit.TabIndex = 1;
      this.buttonEdit.Text = "Edit";
      this.buttonEdit.UseVisualStyleBackColor = true;
      this.buttonEdit.Click += new System.EventHandler(this.ButtonEditClick);
      // 
      // buttonReset
      // 
      this.buttonReset.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonReset.Location = new System.Drawing.Point(202, 3);
      this.buttonReset.Margin = new System.Windows.Forms.Padding(3, 3, 0, 6);
      this.buttonReset.Name = "buttonReset";
      this.buttonReset.Size = new System.Drawing.Size(67, 27);
      this.buttonReset.TabIndex = 2;
      this.buttonReset.Text = "Reset";
      this.buttonReset.UseVisualStyleBackColor = true;
      this.buttonReset.Click += new System.EventHandler(this.ButtonResetClick);
      // 
      // CellGoalType
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "CellGoalType";
      this.Size = new System.Drawing.Size(269, 36);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Button buttonReset;
    private System.Windows.Forms.Button buttonEdit;
    private System.Windows.Forms.Label labelGoalType;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
