// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class WarningDialog
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the form.
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WarningDialog));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonDontCare = new System.Windows.Forms.Button();
      this.buttonOk = new System.Windows.Forms.Button();
      this.innerTable = new System.Windows.Forms.TableLayoutPanel();
      this.label = new System.Windows.Forms.Label();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.baseLayout.SuspendLayout();
      this.innerTable.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.baseLayout.Controls.Add(this.buttonDontCare, 1, 1);
      this.baseLayout.Controls.Add(this.buttonOk, 2, 1);
      this.baseLayout.Controls.Add(this.innerTable, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
      this.baseLayout.Size = new System.Drawing.Size(334, 132);
      this.baseLayout.TabIndex = 0;
      // 
      // buttonDontCare
      // 
      this.buttonDontCare.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDontCare.Location = new System.Drawing.Point(137, 99);
      this.buttonDontCare.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
      this.buttonDontCare.Name = "buttonDontCare";
      this.buttonDontCare.Size = new System.Drawing.Size(94, 25);
      this.buttonDontCare.TabIndex = 2;
      this.buttonDontCare.Text = "Ignore";
      this.buttonDontCare.UseVisualStyleBackColor = true;
      this.buttonDontCare.Click += new System.EventHandler(this.ButtonDontCareClick);
      // 
      // buttonOk
      // 
      this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOk.Location = new System.Drawing.Point(237, 99);
      this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 8, 8, 8);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(89, 25);
      this.buttonOk.TabIndex = 1;
      this.buttonOk.Text = "OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
      // 
      // innerTable
      // 
      this.innerTable.BackColor = System.Drawing.SystemColors.Window;
      this.innerTable.ColumnCount = 2;
      this.baseLayout.SetColumnSpan(this.innerTable, 3);
      this.innerTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
      this.innerTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.innerTable.Controls.Add(this.label, 1, 0);
      this.innerTable.Controls.Add(this.pictureBox, 0, 0);
      this.innerTable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.innerTable.Location = new System.Drawing.Point(0, 0);
      this.innerTable.Margin = new System.Windows.Forms.Padding(0);
      this.innerTable.Name = "innerTable";
      this.innerTable.RowCount = 1;
      this.innerTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.innerTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 91F));
      this.innerTable.Size = new System.Drawing.Size(334, 91);
      this.innerTable.TabIndex = 2;
      // 
      // label
      // 
      this.label.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label.Location = new System.Drawing.Point(83, 0);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(248, 91);
      this.label.TabIndex = 0;
      this.label.Text = "label";
      this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pictureBox
      // 
      this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox.Location = new System.Drawing.Point(3, 3);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(74, 85);
      this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.pictureBox.TabIndex = 1;
      this.pictureBox.TabStop = false;
      // 
      // WarningDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(334, 132);
      this.Controls.Add(this.baseLayout);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(350, 170);
      this.Name = "WarningDialog";
      this.ShowInTaskbar = false;
      this.Text = "Warning";
      this.baseLayout.ResumeLayout(false);
      this.innerTable.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.Label label;
    private System.Windows.Forms.TableLayoutPanel innerTable;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.Button buttonDontCare;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
