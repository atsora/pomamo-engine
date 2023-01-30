// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class TimelinesWidget
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimelinesWidget));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.verticalScrollLayout = new Lemoine.BaseControls.VerticalScrollLayout();
      this.horizontalBar1 = new System.Windows.Forms.Label();
      this.timeAxis = new Lemoine.BaseControls.TimeAxis();
      this.buttonNext = new System.Windows.Forms.Button();
      this.buttonPrevious = new System.Windows.Forms.Button();
      this.buttonChangePeriod = new System.Windows.Forms.Button();
      this.buttonZoomIn = new System.Windows.Forms.Button();
      this.buttonZoomOut = new System.Windows.Forms.Button();
      this.horizontalBar2 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 5;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.Controls.Add(this.verticalScrollLayout, 0, 2);
      this.baseLayout.Controls.Add(this.horizontalBar1, 0, 1);
      this.baseLayout.Controls.Add(this.timeAxis, 0, 0);
      this.baseLayout.Controls.Add(this.buttonNext, 4, 4);
      this.baseLayout.Controls.Add(this.buttonPrevious, 3, 4);
      this.baseLayout.Controls.Add(this.buttonChangePeriod, 2, 4);
      this.baseLayout.Controls.Add(this.buttonZoomIn, 1, 4);
      this.baseLayout.Controls.Add(this.buttonZoomOut, 0, 4);
      this.baseLayout.Controls.Add(this.horizontalBar2, 0, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 5;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.Size = new System.Drawing.Size(403, 203);
      this.baseLayout.TabIndex = 0;
      // 
      // verticalScrollLayout
      // 
      this.verticalScrollLayout.BackColor = System.Drawing.SystemColors.ScrollBar;
      this.baseLayout.SetColumnSpan(this.verticalScrollLayout, 5);
      this.verticalScrollLayout.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLayout.Location = new System.Drawing.Point(0, 28);
      this.verticalScrollLayout.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.MouseWheelStep = 30;
      this.verticalScrollLayout.Name = "verticalScrollLayout";
      this.verticalScrollLayout.ScrollCtrlDisabled = true;
      this.verticalScrollLayout.Size = new System.Drawing.Size(403, 151);
      this.verticalScrollLayout.TabIndex = 0;
      this.verticalScrollLayout.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLayout.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // horizontalBar1
      // 
      this.horizontalBar1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.baseLayout.SetColumnSpan(this.horizontalBar1, 5);
      this.horizontalBar1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.horizontalBar1.Location = new System.Drawing.Point(0, 27);
      this.horizontalBar1.Margin = new System.Windows.Forms.Padding(0);
      this.horizontalBar1.Name = "horizontalBar1";
      this.horizontalBar1.Size = new System.Drawing.Size(403, 1);
      this.horizontalBar1.TabIndex = 1;
      // 
      // timeAxis
      // 
      this.timeAxis.AutoSize = true;
      this.baseLayout.SetColumnSpan(this.timeAxis, 5);
      this.timeAxis.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timeAxis.Location = new System.Drawing.Point(0, 0);
      this.timeAxis.Margin = new System.Windows.Forms.Padding(0);
      this.timeAxis.Name = "timeAxis";
      this.timeAxis.Size = new System.Drawing.Size(403, 27);
      this.timeAxis.TabIndex = 5;
      // 
      // buttonNext
      // 
      this.buttonNext.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonNext.Image = ((System.Drawing.Image)(resources.GetObject("buttonNext.Image")));
      this.buttonNext.Location = new System.Drawing.Point(377, 181);
      this.buttonNext.Margin = new System.Windows.Forms.Padding(1, 1, 1, 0);
      this.buttonNext.Name = "buttonNext";
      this.buttonNext.Size = new System.Drawing.Size(25, 22);
      this.buttonNext.TabIndex = 4;
      this.buttonNext.UseVisualStyleBackColor = true;
      this.buttonNext.Click += new System.EventHandler(this.ButtonRightClick);
      // 
      // buttonPrevious
      // 
      this.buttonPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonPrevious.Image = ((System.Drawing.Image)(resources.GetObject("buttonPrevious.Image")));
      this.buttonPrevious.Location = new System.Drawing.Point(350, 181);
      this.buttonPrevious.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.buttonPrevious.Name = "buttonPrevious";
      this.buttonPrevious.Size = new System.Drawing.Size(26, 22);
      this.buttonPrevious.TabIndex = 2;
      this.buttonPrevious.UseVisualStyleBackColor = true;
      this.buttonPrevious.Click += new System.EventHandler(this.ButtonLeftClick);
      // 
      // buttonChangePeriod
      // 
      this.buttonChangePeriod.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonChangePeriod.Location = new System.Drawing.Point(54, 181);
      this.buttonChangePeriod.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
      this.buttonChangePeriod.Name = "buttonChangePeriod";
      this.buttonChangePeriod.Size = new System.Drawing.Size(295, 22);
      this.buttonChangePeriod.TabIndex = 3;
      this.buttonChangePeriod.UseVisualStyleBackColor = true;
      this.buttonChangePeriod.Click += new System.EventHandler(this.ButtonChangePeriodClick);
      // 
      // buttonZoomIn
      // 
      this.buttonZoomIn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("buttonZoomIn.Image")));
      this.buttonZoomIn.Location = new System.Drawing.Point(27, 181);
      this.buttonZoomIn.Margin = new System.Windows.Forms.Padding(0, 1, 1, 0);
      this.buttonZoomIn.Name = "buttonZoomIn";
      this.buttonZoomIn.Size = new System.Drawing.Size(26, 22);
      this.buttonZoomIn.TabIndex = 7;
      this.buttonZoomIn.UseVisualStyleBackColor = true;
      this.buttonZoomIn.Click += new System.EventHandler(this.ButtonZoomInClick);
      // 
      // buttonZoomOut
      // 
      this.buttonZoomOut.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("buttonZoomOut.Image")));
      this.buttonZoomOut.Location = new System.Drawing.Point(1, 181);
      this.buttonZoomOut.Margin = new System.Windows.Forms.Padding(1, 1, 1, 0);
      this.buttonZoomOut.Name = "buttonZoomOut";
      this.buttonZoomOut.Size = new System.Drawing.Size(25, 22);
      this.buttonZoomOut.TabIndex = 6;
      this.buttonZoomOut.UseVisualStyleBackColor = true;
      this.buttonZoomOut.Click += new System.EventHandler(this.ButtonZoomOutClick);
      // 
      // horizontalBar2
      // 
      this.horizontalBar2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.baseLayout.SetColumnSpan(this.horizontalBar2, 5);
      this.horizontalBar2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.horizontalBar2.Location = new System.Drawing.Point(0, 179);
      this.horizontalBar2.Margin = new System.Windows.Forms.Padding(0);
      this.horizontalBar2.Name = "horizontalBar2";
      this.horizontalBar2.Size = new System.Drawing.Size(403, 1);
      this.horizontalBar2.TabIndex = 8;
      // 
      // TimelinesWidget
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "TimelinesWidget";
      this.Size = new System.Drawing.Size(403, 203);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label horizontalBar2;
    private System.Windows.Forms.Button buttonZoomIn;
    private System.Windows.Forms.Button buttonZoomOut;
    private Lemoine.BaseControls.TimeAxis timeAxis;
    private System.Windows.Forms.Button buttonNext;
    private System.Windows.Forms.Button buttonChangePeriod;
    private System.Windows.Forms.Button buttonPrevious;
    private System.Windows.Forms.Label horizontalBar1;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLayout;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
