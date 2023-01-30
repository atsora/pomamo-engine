// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewLinePlanning
{
  partial class Page2
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page2));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.verticalScrollLines = new Lemoine.BaseControls.VerticalScrollLayout();
      this.labelDayStart = new System.Windows.Forms.Label();
      this.labelDayEnd = new System.Windows.Forms.Label();
      this.buttonPrevious = new System.Windows.Forms.Button();
      this.buttonNext = new System.Windows.Forms.Button();
      this.treeQuantities = new System.Windows.Forms.TreeView();
      this.baseLayout.SuspendLayout();
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
      this.baseLayout.Controls.Add(this.label2, 1, 1);
      this.baseLayout.Controls.Add(this.label1, 1, 0);
      this.baseLayout.Controls.Add(this.verticalScrollLines, 0, 2);
      this.baseLayout.Controls.Add(this.labelDayStart, 2, 0);
      this.baseLayout.Controls.Add(this.labelDayEnd, 2, 1);
      this.baseLayout.Controls.Add(this.buttonPrevious, 0, 0);
      this.baseLayout.Controls.Add(this.buttonNext, 4, 0);
      this.baseLayout.Controls.Add(this.treeQuantities, 3, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(40, 20);
      this.label2.Margin = new System.Windows.Forms.Padding(0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(60, 20);
      this.label2.TabIndex = 0;
      this.label2.Text = "Day end";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(40, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(60, 20);
      this.label1.TabIndex = 2;
      this.label1.Text = "Day start";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // verticalScrollLines
      // 
      this.verticalScrollLines.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScrollLines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.verticalScrollLines, 3);
      this.verticalScrollLines.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLines.Location = new System.Drawing.Point(0, 43);
      this.verticalScrollLines.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.verticalScrollLines.Name = "verticalScrollLines";
      this.verticalScrollLines.Size = new System.Drawing.Size(184, 207);
      this.verticalScrollLines.TabIndex = 1;
      this.verticalScrollLines.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLines.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // labelDayStart
      // 
      this.baseLayout.SetColumnSpan(this.labelDayStart, 2);
      this.labelDayStart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDayStart.Location = new System.Drawing.Point(100, 0);
      this.labelDayStart.Margin = new System.Windows.Forms.Padding(0);
      this.labelDayStart.Name = "labelDayStart";
      this.labelDayStart.Size = new System.Drawing.Size(210, 20);
      this.labelDayStart.TabIndex = 4;
      this.labelDayStart.Text = "labelDayStart";
      this.labelDayStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelDayEnd
      // 
      this.baseLayout.SetColumnSpan(this.labelDayEnd, 2);
      this.labelDayEnd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDayEnd.Location = new System.Drawing.Point(100, 20);
      this.labelDayEnd.Margin = new System.Windows.Forms.Padding(0);
      this.labelDayEnd.Name = "labelDayEnd";
      this.labelDayEnd.Size = new System.Drawing.Size(210, 20);
      this.labelDayEnd.TabIndex = 5;
      this.labelDayEnd.Text = "labelDayEnd";
      this.labelDayEnd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonPrevious
      // 
      this.buttonPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonPrevious.Image = ((System.Drawing.Image)(resources.GetObject("buttonPrevious.Image")));
      this.buttonPrevious.Location = new System.Drawing.Point(0, 3);
      this.buttonPrevious.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
      this.buttonPrevious.Name = "buttonPrevious";
      this.baseLayout.SetRowSpan(this.buttonPrevious, 2);
      this.buttonPrevious.Size = new System.Drawing.Size(37, 34);
      this.buttonPrevious.TabIndex = 6;
      this.buttonPrevious.UseVisualStyleBackColor = true;
      this.buttonPrevious.Click += new System.EventHandler(this.ButtonPreviousClick);
      // 
      // buttonNext
      // 
      this.buttonNext.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonNext.Image = ((System.Drawing.Image)(resources.GetObject("buttonNext.Image")));
      this.buttonNext.Location = new System.Drawing.Point(313, 3);
      this.buttonNext.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.buttonNext.Name = "buttonNext";
      this.baseLayout.SetRowSpan(this.buttonNext, 2);
      this.buttonNext.Size = new System.Drawing.Size(37, 34);
      this.buttonNext.TabIndex = 7;
      this.buttonNext.UseVisualStyleBackColor = true;
      this.buttonNext.Click += new System.EventHandler(this.ButtonNextClick);
      // 
      // treeQuantities
      // 
      this.baseLayout.SetColumnSpan(this.treeQuantities, 2);
      this.treeQuantities.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeQuantities.Location = new System.Drawing.Point(187, 43);
      this.treeQuantities.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.treeQuantities.Name = "treeQuantities";
      this.treeQuantities.Size = new System.Drawing.Size(163, 207);
      this.treeQuantities.TabIndex = 8;
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.TreeView treeQuantities;
    private System.Windows.Forms.Button buttonNext;
    private System.Windows.Forms.Button buttonPrevious;
    private System.Windows.Forms.Label labelDayEnd;
    private System.Windows.Forms.Label labelDayStart;
    private System.Windows.Forms.Label label1;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLines;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
