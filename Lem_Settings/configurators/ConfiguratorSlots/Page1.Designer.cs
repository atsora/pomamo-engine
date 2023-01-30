// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots
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
      this.timelinesWidget = new Lemoine.BaseControls.TimelinesWidget();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.timeStart = new System.Windows.Forms.DateTimePicker();
      this.timeEnd = new System.Windows.Forms.DateTimePicker();
      this.dateStart = new System.Windows.Forms.DateTimePicker();
      this.label1 = new System.Windows.Forms.Label();
      this.dateEnd = new System.Windows.Forms.DateTimePicker();
      this.label2 = new System.Windows.Forms.Label();
      this.checkEnd = new System.Windows.Forms.CheckBox();
      this.buttonModify = new System.Windows.Forms.Button();
      this.buttonFilter = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.verticalScrollLayout = new Lemoine.BaseControls.VerticalScrollLayout();
      this.baseLayout.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.Controls.Add(this.timelinesWidget, 0, 0);
      this.baseLayout.Controls.Add(this.groupBox1, 1, 1);
      this.baseLayout.Controls.Add(this.buttonModify, 3, 2);
      this.baseLayout.Controls.Add(this.buttonFilter, 2, 2);
      this.baseLayout.Controls.Add(this.groupBox2, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 73F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // timelinesWidget
      // 
      this.timelinesWidget.BarHeight = 32;
      this.baseLayout.SetColumnSpan(this.timelinesWidget, 4);
      this.timelinesWidget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timelinesWidget.Location = new System.Drawing.Point(0, 0);
      this.timelinesWidget.Margin = new System.Windows.Forms.Padding(0);
      this.timelinesWidget.Name = "timelinesWidget";
      this.timelinesWidget.NoBorders = false;
      this.timelinesWidget.Size = new System.Drawing.Size(370, 190);
      this.timelinesWidget.TabIndex = 9;
      // 
      // groupBox1
      // 
      this.baseLayout.SetColumnSpan(this.groupBox1, 3);
      this.groupBox1.Controls.Add(this.tableLayoutPanel2);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox1.Location = new System.Drawing.Point(143, 193);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(224, 70);
      this.groupBox1.TabIndex = 10;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Selected period";
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 4;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.Controls.Add(this.timeStart, 3, 0);
      this.tableLayoutPanel2.Controls.Add(this.timeEnd, 3, 1);
      this.tableLayoutPanel2.Controls.Add(this.dateStart, 2, 0);
      this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.dateEnd, 2, 1);
      this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.checkEnd, 1, 1);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 2;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(218, 51);
      this.tableLayoutPanel2.TabIndex = 1;
      // 
      // timeStart
      // 
      this.timeStart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timeStart.Format = System.Windows.Forms.DateTimePickerFormat.Time;
      this.timeStart.Location = new System.Drawing.Point(138, 3);
      this.timeStart.Name = "timeStart";
      this.timeStart.ShowUpDown = true;
      this.timeStart.Size = new System.Drawing.Size(77, 20);
      this.timeStart.TabIndex = 3;
      this.timeStart.ValueChanged += new System.EventHandler(this.SelectedPeriodChanged);
      // 
      // timeEnd
      // 
      this.timeEnd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timeEnd.Format = System.Windows.Forms.DateTimePickerFormat.Time;
      this.timeEnd.Location = new System.Drawing.Point(138, 28);
      this.timeEnd.Name = "timeEnd";
      this.timeEnd.ShowUpDown = true;
      this.timeEnd.Size = new System.Drawing.Size(77, 20);
      this.timeEnd.TabIndex = 5;
      this.timeEnd.ValueChanged += new System.EventHandler(this.SelectedPeriodChanged);
      // 
      // dateStart
      // 
      this.dateStart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dateStart.Location = new System.Drawing.Point(56, 3);
      this.dateStart.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.dateStart.Name = "dateStart";
      this.dateStart.Size = new System.Drawing.Size(79, 20);
      this.dateStart.TabIndex = 2;
      this.dateStart.ValueChanged += new System.EventHandler(this.SelectedPeriodChanged);
      // 
      // label1
      // 
      this.tableLayoutPanel2.SetColumnSpan(this.label1, 2);
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(47, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "Start";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // dateEnd
      // 
      this.dateEnd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dateEnd.Location = new System.Drawing.Point(56, 28);
      this.dateEnd.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.dateEnd.Name = "dateEnd";
      this.dateEnd.Size = new System.Drawing.Size(79, 20);
      this.dateEnd.TabIndex = 4;
      this.dateEnd.ValueChanged += new System.EventHandler(this.SelectedPeriodChanged);
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(3, 25);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(27, 26);
      this.label2.TabIndex = 1;
      this.label2.Text = "End";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkEnd
      // 
      this.checkEnd.Checked = true;
      this.checkEnd.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkEnd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkEnd.Location = new System.Drawing.Point(36, 28);
      this.checkEnd.Name = "checkEnd";
      this.checkEnd.Size = new System.Drawing.Size(14, 20);
      this.checkEnd.TabIndex = 7;
      this.checkEnd.UseVisualStyleBackColor = true;
      this.checkEnd.CheckedChanged += new System.EventHandler(this.CheckEndCheckedChanged);
      // 
      // buttonModify
      // 
      this.buttonModify.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonModify.Location = new System.Drawing.Point(310, 266);
      this.buttonModify.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
      this.buttonModify.Name = "buttonModify";
      this.buttonModify.Size = new System.Drawing.Size(57, 21);
      this.buttonModify.TabIndex = 2;
      this.buttonModify.Text = "Add slot";
      this.buttonModify.UseVisualStyleBackColor = true;
      this.buttonModify.Click += new System.EventHandler(this.ButtonModifyClick);
      // 
      // buttonFilter
      // 
      this.buttonFilter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonFilter.Location = new System.Drawing.Point(203, 266);
      this.buttonFilter.Name = "buttonFilter";
      this.buttonFilter.Size = new System.Drawing.Size(104, 21);
      this.buttonFilter.TabIndex = 1;
      this.buttonFilter.Text = "Change machines";
      this.buttonFilter.UseVisualStyleBackColor = true;
      this.buttonFilter.Click += new System.EventHandler(this.ButtonFilterClick);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.verticalScrollLayout);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox2.Location = new System.Drawing.Point(3, 193);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.groupBox2.Name = "groupBox2";
      this.baseLayout.SetRowSpan(this.groupBox2, 2);
      this.groupBox2.Size = new System.Drawing.Size(137, 94);
      this.groupBox2.TabIndex = 12;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Legend";
      // 
      // verticalScrollLayout
      // 
      this.verticalScrollLayout.ContainerMargin = new System.Windows.Forms.Padding(3);
      this.verticalScrollLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLayout.Location = new System.Drawing.Point(3, 16);
      this.verticalScrollLayout.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.verticalScrollLayout.Name = "verticalScrollLayout";
      this.verticalScrollLayout.Size = new System.Drawing.Size(131, 75);
      this.verticalScrollLayout.TabIndex = 11;
      this.verticalScrollLayout.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLayout.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.GroupBox groupBox2;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLayout;
    private System.Windows.Forms.CheckBox checkEnd;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.DateTimePicker dateEnd;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.DateTimePicker dateStart;
    private System.Windows.Forms.DateTimePicker timeEnd;
    private System.Windows.Forms.DateTimePicker timeStart;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.GroupBox groupBox1;
    private Lemoine.BaseControls.TimelinesWidget timelinesWidget;
    private System.Windows.Forms.Button buttonModify;
    private System.Windows.Forms.Button buttonFilter;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
