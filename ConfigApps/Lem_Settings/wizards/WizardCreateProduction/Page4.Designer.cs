// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateProduction
{
  partial class Page4
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
      this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.radioNo = new System.Windows.Forms.RadioButton();
      this.radioDaily = new System.Windows.Forms.RadioButton();
      this.radioWeekly = new System.Windows.Forms.RadioButton();
      this.tableDay = new System.Windows.Forms.TableLayoutPanel();
      this.label2 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.dayInWeekPicker = new Lemoine.BaseControls.DayInWeekPicker();
      this.labelTime1 = new System.Windows.Forms.Label();
      this.dateEnd1 = new Lemoine.BaseControls.DatePicker();
      this.tableWeek = new System.Windows.Forms.TableLayoutPanel();
      this.label3 = new System.Windows.Forms.Label();
      this.labelTime2 = new System.Windows.Forms.Label();
      this.dateEnd2 = new Lemoine.BaseControls.DatePicker();
      this.tableLayout.SuspendLayout();
      this.tableDay.SuspendLayout();
      this.tableWeek.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayout
      // 
      this.tableLayout.ColumnCount = 1;
      this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.55556F));
      this.tableLayout.Controls.Add(this.label1, 0, 1);
      this.tableLayout.Controls.Add(this.radioNo, 0, 2);
      this.tableLayout.Controls.Add(this.radioDaily, 0, 3);
      this.tableLayout.Controls.Add(this.radioWeekly, 0, 5);
      this.tableLayout.Controls.Add(this.tableDay, 0, 4);
      this.tableLayout.Controls.Add(this.tableWeek, 0, 6);
      this.tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayout.Location = new System.Drawing.Point(0, 0);
      this.tableLayout.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayout.Name = "tableLayout";
      this.tableLayout.RowCount = 8;
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayout.Size = new System.Drawing.Size(350, 250);
      this.tableLayout.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(0, 37);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(347, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "Recurrences";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // radioNo
      // 
      this.radioNo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioNo.Location = new System.Drawing.Point(10, 62);
      this.radioNo.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
      this.radioNo.Name = "radioNo";
      this.radioNo.Size = new System.Drawing.Size(340, 25);
      this.radioNo.TabIndex = 1;
      this.radioNo.Text = "none";
      this.radioNo.UseVisualStyleBackColor = true;
      this.radioNo.CheckedChanged += new System.EventHandler(this.RadioNoCheckedChanged);
      // 
      // radioDaily
      // 
      this.radioDaily.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioDaily.Location = new System.Drawing.Point(10, 87);
      this.radioDaily.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
      this.radioDaily.Name = "radioDaily";
      this.radioDaily.Size = new System.Drawing.Size(340, 25);
      this.radioDaily.TabIndex = 2;
      this.radioDaily.Text = "daily";
      this.radioDaily.UseVisualStyleBackColor = true;
      this.radioDaily.CheckedChanged += new System.EventHandler(this.RadioDailyCheckedChanged);
      // 
      // radioWeekly
      // 
      this.radioWeekly.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioWeekly.Location = new System.Drawing.Point(10, 162);
      this.radioWeekly.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
      this.radioWeekly.Name = "radioWeekly";
      this.radioWeekly.Size = new System.Drawing.Size(340, 25);
      this.radioWeekly.TabIndex = 3;
      this.radioWeekly.Text = "weekly";
      this.radioWeekly.UseVisualStyleBackColor = true;
      this.radioWeekly.CheckedChanged += new System.EventHandler(this.RadioWeeklyCheckedChanged);
      // 
      // tableDay
      // 
      this.tableDay.ColumnCount = 3;
      this.tableDay.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.tableDay.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableDay.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.tableDay.Controls.Add(this.label2, 0, 0);
      this.tableDay.Controls.Add(this.label4, 0, 1);
      this.tableDay.Controls.Add(this.dayInWeekPicker, 1, 1);
      this.tableDay.Controls.Add(this.labelTime1, 2, 0);
      this.tableDay.Controls.Add(this.dateEnd1, 1, 0);
      this.tableDay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableDay.Location = new System.Drawing.Point(30, 112);
      this.tableDay.Margin = new System.Windows.Forms.Padding(30, 0, 0, 0);
      this.tableDay.Name = "tableDay";
      this.tableDay.RowCount = 2;
      this.tableDay.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableDay.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableDay.Size = new System.Drawing.Size(320, 50);
      this.tableDay.TabIndex = 12;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(84, 25);
      this.label2.TabIndex = 4;
      this.label2.Text = "End";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(3, 25);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(84, 25);
      this.label4.TabIndex = 7;
      this.label4.Text = "Repeat on";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // dayInWeekPicker
      // 
      this.tableDay.SetColumnSpan(this.dayInWeekPicker, 2);
      this.dayInWeekPicker.DaysInWeek = 127;
      this.dayInWeekPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dayInWeekPicker.Location = new System.Drawing.Point(90, 25);
      this.dayInWeekPicker.Margin = new System.Windows.Forms.Padding(0);
      this.dayInWeekPicker.MinimumSize = new System.Drawing.Size(155, 20);
      this.dayInWeekPicker.Name = "dayInWeekPicker";
      this.dayInWeekPicker.Size = new System.Drawing.Size(230, 25);
      this.dayInWeekPicker.TabIndex = 6;
      // 
      // labelTime1
      // 
      this.labelTime1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelTime1.Location = new System.Drawing.Point(233, 0);
      this.labelTime1.Name = "labelTime1";
      this.labelTime1.Size = new System.Drawing.Size(84, 25);
      this.labelTime1.TabIndex = 9;
      this.labelTime1.Text = "labelTime1";
      this.labelTime1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // dateEnd1
      // 
      this.dateEnd1.CustomFormat = "dd/MM/yyyy";
      this.dateEnd1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateEnd1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
      this.dateEnd1.Location = new System.Drawing.Point(93, 3);
      this.dateEnd1.Name = "dateEnd1";
      this.dateEnd1.Size = new System.Drawing.Size(134, 20);
      this.dateEnd1.TabIndex = 10;
      this.dateEnd1.ValueChanged += new System.EventHandler(this.DateEnd1ValueChanged);
      // 
      // tableWeek
      // 
      this.tableWeek.ColumnCount = 3;
      this.tableWeek.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.tableWeek.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableWeek.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.tableWeek.Controls.Add(this.label3, 0, 0);
      this.tableWeek.Controls.Add(this.labelTime2, 2, 0);
      this.tableWeek.Controls.Add(this.dateEnd2, 1, 0);
      this.tableWeek.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableWeek.Location = new System.Drawing.Point(30, 187);
      this.tableWeek.Margin = new System.Windows.Forms.Padding(30, 0, 0, 0);
      this.tableWeek.Name = "tableWeek";
      this.tableWeek.RowCount = 1;
      this.tableWeek.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableWeek.Size = new System.Drawing.Size(320, 25);
      this.tableWeek.TabIndex = 13;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(84, 25);
      this.label3.TabIndex = 5;
      this.label3.Text = "End";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelTime2
      // 
      this.labelTime2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelTime2.Location = new System.Drawing.Point(233, 0);
      this.labelTime2.Name = "labelTime2";
      this.labelTime2.Size = new System.Drawing.Size(84, 25);
      this.labelTime2.TabIndex = 11;
      this.labelTime2.Text = "labelTime2";
      this.labelTime2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // dateEnd2
      // 
      this.dateEnd2.CustomFormat = "dd/MM/yyyy";
      this.dateEnd2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateEnd2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
      this.dateEnd2.Location = new System.Drawing.Point(93, 3);
      this.dateEnd2.Name = "dateEnd2";
      this.dateEnd2.Size = new System.Drawing.Size(134, 20);
      this.dateEnd2.TabIndex = 12;
      this.dateEnd2.ValueChanged += new System.EventHandler(this.DateEnd2ValueChanged);
      // 
      // Page4
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayout);
      this.Name = "Page4";
      this.Size = new System.Drawing.Size(350, 250);
      this.tableLayout.ResumeLayout(false);
      this.tableDay.ResumeLayout(false);
      this.tableWeek.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Label labelTime2;
    private System.Windows.Forms.Label labelTime1;
    private System.Windows.Forms.TableLayoutPanel tableWeek;
    private System.Windows.Forms.TableLayoutPanel tableDay;
    private Lemoine.BaseControls.DatePicker dateEnd2;
    private Lemoine.BaseControls.DatePicker dateEnd1;
    private System.Windows.Forms.Label label4;
    private Lemoine.BaseControls.DayInWeekPicker dayInWeekPicker;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.RadioButton radioWeekly;
    private System.Windows.Forms.RadioButton radioDaily;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.RadioButton radioNo;
    private System.Windows.Forms.TableLayoutPanel tableLayout;
  }
}
