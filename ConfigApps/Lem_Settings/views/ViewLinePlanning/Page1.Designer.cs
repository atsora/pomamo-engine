using Lemoine.BaseControls;

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewLinePlanning
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
      Utils.ClearControls(tableCalendar.Controls);
      verticalScrollLayout.Clear();
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page1));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonPreviousMonth = new System.Windows.Forms.Button();
      this.buttonNextMonth = new System.Windows.Forms.Button();
      this.labelDate = new System.Windows.Forms.Label();
      this.tableCalendar = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.verticalScrollLayout = new Lemoine.BaseControls.VerticalScrollLayout();
      this.baseLayout.SuspendLayout();
      this.tableCalendar.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 5;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.baseLayout.Controls.Add(this.buttonPreviousMonth, 0, 1);
      this.baseLayout.Controls.Add(this.buttonNextMonth, 2, 1);
      this.baseLayout.Controls.Add(this.labelDate, 1, 1);
      this.baseLayout.Controls.Add(this.tableCalendar, 0, 0);
      this.baseLayout.Controls.Add(this.verticalScrollLayout, 3, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 0;
      // 
      // buttonPreviousMonth
      // 
      this.buttonPreviousMonth.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonPreviousMonth.Image = ((System.Drawing.Image)(resources.GetObject("buttonPreviousMonth.Image")));
      this.buttonPreviousMonth.Location = new System.Drawing.Point(0, 225);
      this.buttonPreviousMonth.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.buttonPreviousMonth.Name = "buttonPreviousMonth";
      this.buttonPreviousMonth.Size = new System.Drawing.Size(30, 25);
      this.buttonPreviousMonth.TabIndex = 1;
      this.buttonPreviousMonth.UseVisualStyleBackColor = true;
      this.buttonPreviousMonth.Click += new System.EventHandler(this.ButtonPreviousMonthClick);
      // 
      // buttonNextMonth
      // 
      this.buttonNextMonth.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonNextMonth.Image = ((System.Drawing.Image)(resources.GetObject("buttonNextMonth.Image")));
      this.buttonNextMonth.Location = new System.Drawing.Point(210, 225);
      this.buttonNextMonth.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.buttonNextMonth.Name = "buttonNextMonth";
      this.buttonNextMonth.Size = new System.Drawing.Size(30, 25);
      this.buttonNextMonth.TabIndex = 2;
      this.buttonNextMonth.UseVisualStyleBackColor = true;
      this.buttonNextMonth.Click += new System.EventHandler(this.ButtonNextMonthClick);
      // 
      // labelDate
      // 
      this.labelDate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDate.Location = new System.Drawing.Point(33, 225);
      this.labelDate.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.labelDate.Name = "labelDate";
      this.labelDate.Size = new System.Drawing.Size(174, 25);
      this.labelDate.TabIndex = 3;
      this.labelDate.Text = "date";
      this.labelDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tableCalendar
      // 
      this.tableCalendar.BackColor = System.Drawing.SystemColors.Window;
      this.tableCalendar.ColumnCount = 7;
      this.baseLayout.SetColumnSpan(this.tableCalendar, 3);
      this.tableCalendar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.tableCalendar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.tableCalendar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.tableCalendar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.tableCalendar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.tableCalendar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.tableCalendar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.tableCalendar.Controls.Add(this.label1, 0, 0);
      this.tableCalendar.Controls.Add(this.label2, 1, 0);
      this.tableCalendar.Controls.Add(this.label3, 2, 0);
      this.tableCalendar.Controls.Add(this.label4, 3, 0);
      this.tableCalendar.Controls.Add(this.label5, 4, 0);
      this.tableCalendar.Controls.Add(this.label6, 5, 0);
      this.tableCalendar.Controls.Add(this.label7, 6, 0);
      this.tableCalendar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableCalendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tableCalendar.Location = new System.Drawing.Point(0, 0);
      this.tableCalendar.Margin = new System.Windows.Forms.Padding(0);
      this.tableCalendar.Name = "tableCalendar";
      this.tableCalendar.RowCount = 7;
      this.tableCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.tableCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.tableCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.tableCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.tableCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.tableCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.tableCalendar.Size = new System.Drawing.Size(240, 222);
      this.tableCalendar.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.SystemColors.Window;
      this.label1.Location = new System.Drawing.Point(1, 1);
      this.label1.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(33, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Mon";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label2
      // 
      this.label2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.SystemColors.Window;
      this.label2.Location = new System.Drawing.Point(35, 1);
      this.label2.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(33, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Tue";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label3
      // 
      this.label3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.ForeColor = System.Drawing.SystemColors.Window;
      this.label3.Location = new System.Drawing.Point(69, 1);
      this.label3.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(33, 13);
      this.label3.TabIndex = 2;
      this.label3.Text = "Wed";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label4
      // 
      this.label4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.ForeColor = System.Drawing.SystemColors.Window;
      this.label4.Location = new System.Drawing.Point(103, 1);
      this.label4.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(33, 13);
      this.label4.TabIndex = 3;
      this.label4.Text = "Thu";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label5
      // 
      this.label5.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.ForeColor = System.Drawing.SystemColors.Window;
      this.label5.Location = new System.Drawing.Point(137, 1);
      this.label5.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(33, 13);
      this.label5.TabIndex = 4;
      this.label5.Text = "Fri";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label6
      // 
      this.label6.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label6.ForeColor = System.Drawing.SystemColors.Window;
      this.label6.Location = new System.Drawing.Point(171, 1);
      this.label6.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(33, 13);
      this.label6.TabIndex = 5;
      this.label6.Text = "Sat";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label7
      // 
      this.label7.BackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label7.ForeColor = System.Drawing.SystemColors.Window;
      this.label7.Location = new System.Drawing.Point(205, 1);
      this.label7.Margin = new System.Windows.Forms.Padding(1, 1, 1, 0);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(34, 13);
      this.label7.TabIndex = 6;
      this.label7.Text = "Sun";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // verticalScrollLayout
      // 
      this.verticalScrollLayout.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScrollLayout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.verticalScrollLayout.ColumnCount = 1;
      this.baseLayout.SetColumnSpan(this.verticalScrollLayout, 2);
      this.verticalScrollLayout.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLayout.Location = new System.Drawing.Point(243, 0);
      this.verticalScrollLayout.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.verticalScrollLayout.Name = "verticalScrollLayout";
      this.verticalScrollLayout.RowCount = 1;
      this.baseLayout.SetRowSpan(this.verticalScrollLayout, 2);
      this.verticalScrollLayout.Size = new System.Drawing.Size(107, 250);
      this.verticalScrollLayout.TabIndex = 7;
      this.verticalScrollLayout.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLayout.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.tableCalendar.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLayout;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label labelDate;
    private System.Windows.Forms.Button buttonNextMonth;
    private System.Windows.Forms.Button buttonPreviousMonth;
    private System.Windows.Forms.TableLayoutPanel tableCalendar;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
