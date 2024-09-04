// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewLinePlanning
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
      this.treeQuantities = new System.Windows.Forms.TreeView();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.labelLine = new System.Windows.Forms.Label();
      this.verticalScroll = new Lemoine.BaseControls.VerticalScrollLayout();
      this.timelinesWidget = new Lemoine.BaseControls.TimelinesWidget();
      this.labelPart = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 5;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.Controls.Add(this.treeQuantities, 3, 3);
      this.baseLayout.Controls.Add(this.pictureBox, 0, 0);
      this.baseLayout.Controls.Add(this.labelLine, 1, 0);
      this.baseLayout.Controls.Add(this.verticalScroll, 0, 3);
      this.baseLayout.Controls.Add(this.timelinesWidget, 0, 2);
      this.baseLayout.Controls.Add(this.labelPart, 1, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 1;
      // 
      // treeQuantities
      // 
      this.baseLayout.SetColumnSpan(this.treeQuantities, 2);
      this.treeQuantities.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeQuantities.Location = new System.Drawing.Point(187, 143);
      this.treeQuantities.Name = "treeQuantities";
      this.treeQuantities.Size = new System.Drawing.Size(160, 104);
      this.treeQuantities.TabIndex = 8;
      // 
      // pictureBox
      // 
      this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox.Location = new System.Drawing.Point(3, 3);
      this.pictureBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.pictureBox.Name = "pictureBox";
      this.baseLayout.SetRowSpan(this.pictureBox, 2);
      this.pictureBox.Size = new System.Drawing.Size(37, 34);
      this.pictureBox.TabIndex = 9;
      this.pictureBox.TabStop = false;
      // 
      // labelLine
      // 
      this.baseLayout.SetColumnSpan(this.labelLine, 4);
      this.labelLine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelLine.Location = new System.Drawing.Point(43, 0);
      this.labelLine.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
      this.labelLine.Name = "labelLine";
      this.labelLine.Size = new System.Drawing.Size(304, 19);
      this.labelLine.TabIndex = 11;
      this.labelLine.Text = "labelLine";
      this.labelLine.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // verticalScroll
      // 
      this.verticalScroll.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScroll.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.verticalScroll, 3);
      this.verticalScroll.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScroll.Location = new System.Drawing.Point(3, 143);
      this.verticalScroll.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.verticalScroll.Name = "verticalScroll";
      this.verticalScroll.Size = new System.Drawing.Size(181, 104);
      this.verticalScroll.TabIndex = 12;
      this.verticalScroll.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScroll.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // timelinesWidget
      // 
      this.timelinesWidget.BarHeight = 48;
      this.baseLayout.SetColumnSpan(this.timelinesWidget, 5);
      this.timelinesWidget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timelinesWidget.Location = new System.Drawing.Point(0, 40);
      this.timelinesWidget.Margin = new System.Windows.Forms.Padding(0);
      this.timelinesWidget.Name = "timelinesWidget";
      this.timelinesWidget.NoBorders = true;
      this.timelinesWidget.Size = new System.Drawing.Size(350, 100);
      this.timelinesWidget.TabIndex = 13;
      // 
      // labelPart
      // 
      this.baseLayout.SetColumnSpan(this.labelPart, 4);
      this.labelPart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPart.Location = new System.Drawing.Point(43, 21);
      this.labelPart.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
      this.labelPart.Name = "labelPart";
      this.labelPart.Size = new System.Drawing.Size(304, 19);
      this.labelPart.TabIndex = 14;
      this.labelPart.Text = "labelPart";
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label labelPart;
    private Lemoine.BaseControls.TimelinesWidget timelinesWidget;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScroll;
    private System.Windows.Forms.Label labelLine;
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.TreeView treeQuantities;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
