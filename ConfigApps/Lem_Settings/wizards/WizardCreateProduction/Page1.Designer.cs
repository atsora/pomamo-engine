// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateProduction
{
  partial class Page1
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
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.timelinesWidget = new Lemoine.BaseControls.TimelinesWidget();
      this.dateStart = new Lemoine.BaseControls.DatePicker();
      this.dateEnd = new Lemoine.BaseControls.DatePicker();
      this.timeStart = new Lemoine.BaseControls.TimePicker();
      this.timeEnd = new Lemoine.BaseControls.TimePicker();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
      this.baseLayout.Controls.Add(this.label3, 0, 1);
      this.baseLayout.Controls.Add(this.label4, 0, 2);
      this.baseLayout.Controls.Add(this.timelinesWidget, 0, 4);
      this.baseLayout.Controls.Add(this.dateStart, 1, 1);
      this.baseLayout.Controls.Add(this.dateEnd, 1, 2);
      this.baseLayout.Controls.Add(this.timeStart, 2, 1);
      this.baseLayout.Controls.Add(this.timeEnd, 2, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 0;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 37);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(104, 25);
      this.label3.TabIndex = 2;
      this.label3.Text = "Start date";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(3, 62);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(104, 25);
      this.label4.TabIndex = 3;
      this.label4.Text = "Wished end date";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // timelinesWidget
      // 
      this.timelinesWidget.BarHeight = 48;
      this.baseLayout.SetColumnSpan(this.timelinesWidget, 3);
      this.timelinesWidget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timelinesWidget.Location = new System.Drawing.Point(0, 112);
      this.timelinesWidget.Margin = new System.Windows.Forms.Padding(0);
      this.timelinesWidget.Name = "timelinesWidget";
      this.timelinesWidget.NoBorders = true;
      this.timelinesWidget.Size = new System.Drawing.Size(350, 100);
      this.timelinesWidget.TabIndex = 10;
      // 
      // dateStart
      // 
      this.dateStart.CustomFormat = "dd/MM/yyyy";
      this.dateStart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
      this.dateStart.Location = new System.Drawing.Point(110, 37);
      this.dateStart.Margin = new System.Windows.Forms.Padding(0);
      this.dateStart.Name = "dateStart";
      this.dateStart.Size = new System.Drawing.Size(132, 20);
      this.dateStart.TabIndex = 11;
      this.dateStart.ValueChanged += new System.EventHandler(this.DateStartValueChanged);
      // 
      // dateEnd
      // 
      this.dateEnd.CustomFormat = "dd/MM/yyyy";
      this.dateEnd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
      this.dateEnd.Location = new System.Drawing.Point(110, 62);
      this.dateEnd.Margin = new System.Windows.Forms.Padding(0);
      this.dateEnd.Name = "dateEnd";
      this.dateEnd.Size = new System.Drawing.Size(132, 20);
      this.dateEnd.TabIndex = 12;
      this.dateEnd.ValueChanged += new System.EventHandler(this.DateEndValueChanged);
      // 
      // timeStart
      // 
      this.timeStart.CustomFormat = "HH:mm";
      this.timeStart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timeStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
      this.timeStart.Location = new System.Drawing.Point(242, 37);
      this.timeStart.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.timeStart.Name = "timeStart";
      this.timeStart.ShowUpDown = true;
      this.timeStart.Size = new System.Drawing.Size(105, 20);
      this.timeStart.TabIndex = 13;
      this.timeStart.ValueChanged += new System.EventHandler(this.TimeStartValueChanged);
      // 
      // timeEnd
      // 
      this.timeEnd.CustomFormat = "HH:mm";
      this.timeEnd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timeEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
      this.timeEnd.Location = new System.Drawing.Point(242, 62);
      this.timeEnd.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.timeEnd.Name = "timeEnd";
      this.timeEnd.ShowUpDown = true;
      this.timeEnd.Size = new System.Drawing.Size(105, 20);
      this.timeEnd.TabIndex = 14;
      this.timeEnd.ValueChanged += new System.EventHandler(this.TimeEndValueChanged);
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.TimelinesWidget timelinesWidget;
    private Lemoine.BaseControls.TimePicker timeEnd;
    private Lemoine.BaseControls.TimePicker timeStart;
    private Lemoine.BaseControls.DatePicker dateEnd;
    private Lemoine.BaseControls.DatePicker dateStart;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
