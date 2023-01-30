// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class ShiftTemplateConfig
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
      this.shiftTemplateBreakGroupBox = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.shiftTemplateBreakAddButton = new System.Windows.Forms.Button();
      this.shiftTemplateBreakDataGridView = new System.Windows.Forms.DataGridView();
      this.shiftTemplateBreakIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateBreakWeekDaysColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateBreakTimePeriodColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateBreakDayColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateDataGridView = new Lemoine.BaseControls.SortableDataGridView();
      this.shiftTemplateIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.shiftTemplateItemDataGridView = new System.Windows.Forms.DataGridView();
      this.shiftTemplateItemIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateItemOrderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateItemShiftColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateItemWeekDaysColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateItemTimePeriodOfDayColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateItemDayColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftTemplateItemAddButton = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.shiftTemplateBreakGroupBox.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.shiftTemplateBreakDataGridView)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.shiftTemplateDataGridView)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.shiftTemplateItemDataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.shiftTemplateBreakGroupBox, 0, 1);
      this.baseLayout.Controls.Add(this.shiftTemplateDataGridView, 0, 0);
      this.baseLayout.Controls.Add(this.groupBox1, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(667, 441);
      this.baseLayout.TabIndex = 1;
      // 
      // shiftTemplateBreakGroupBox
      // 
      this.shiftTemplateBreakGroupBox.Controls.Add(this.tableLayoutPanel1);
      this.shiftTemplateBreakGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftTemplateBreakGroupBox.Location = new System.Drawing.Point(336, 223);
      this.shiftTemplateBreakGroupBox.Name = "shiftTemplateBreakGroupBox";
      this.shiftTemplateBreakGroupBox.Size = new System.Drawing.Size(328, 215);
      this.shiftTemplateBreakGroupBox.TabIndex = 4;
      this.shiftTemplateBreakGroupBox.TabStop = false;
      this.shiftTemplateBreakGroupBox.Text = "groupBox2";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
      this.tableLayoutPanel1.Controls.Add(this.shiftTemplateBreakAddButton, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.shiftTemplateBreakDataGridView, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(322, 196);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // shiftTemplateBreakAddButton
      // 
      this.shiftTemplateBreakAddButton.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftTemplateBreakAddButton.Location = new System.Drawing.Point(221, 173);
      this.shiftTemplateBreakAddButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.shiftTemplateBreakAddButton.Name = "shiftTemplateBreakAddButton";
      this.shiftTemplateBreakAddButton.Size = new System.Drawing.Size(98, 23);
      this.shiftTemplateBreakAddButton.TabIndex = 1;
      this.shiftTemplateBreakAddButton.Text = "Add a break";
      this.shiftTemplateBreakAddButton.UseVisualStyleBackColor = true;
      this.shiftTemplateBreakAddButton.Click += new System.EventHandler(this.ShiftTemplateBreakAddButtonClick);
      // 
      // shiftTemplateBreakDataGridView
      // 
      this.shiftTemplateBreakDataGridView.AllowUserToAddRows = false;
      this.shiftTemplateBreakDataGridView.AllowUserToResizeRows = false;
      this.shiftTemplateBreakDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.shiftTemplateBreakDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.shiftTemplateBreakDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.shiftTemplateBreakIdColumn,
                  this.shiftTemplateBreakWeekDaysColumn,
                  this.shiftTemplateBreakTimePeriodColumn,
                  this.shiftTemplateBreakDayColumn});
      this.tableLayoutPanel1.SetColumnSpan(this.shiftTemplateBreakDataGridView, 2);
      this.shiftTemplateBreakDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftTemplateBreakDataGridView.Location = new System.Drawing.Point(3, 3);
      this.shiftTemplateBreakDataGridView.Name = "shiftTemplateBreakDataGridView";
      this.shiftTemplateBreakDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.shiftTemplateBreakDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.shiftTemplateBreakDataGridView.Size = new System.Drawing.Size(316, 167);
      this.shiftTemplateBreakDataGridView.TabIndex = 0;
      this.shiftTemplateBreakDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.ShiftTemplateBreakDataGridViewCellValueChanged);
      this.shiftTemplateBreakDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.ShiftTemplateBreakDataGridViewUserDeletingRow);
      // 
      // shiftTemplateBreakIdColumn
      // 
      this.shiftTemplateBreakIdColumn.DataPropertyName = "Id";
      this.shiftTemplateBreakIdColumn.HeaderText = "IdColumn";
      this.shiftTemplateBreakIdColumn.Name = "shiftTemplateBreakIdColumn";
      this.shiftTemplateBreakIdColumn.ReadOnly = true;
      this.shiftTemplateBreakIdColumn.Visible = false;
      this.shiftTemplateBreakIdColumn.Width = 76;
      // 
      // shiftTemplateBreakWeekDaysColumn
      // 
      this.shiftTemplateBreakWeekDaysColumn.DataPropertyName = "WeekDays";
      this.shiftTemplateBreakWeekDaysColumn.HeaderText = "WeekDaysColumn";
      this.shiftTemplateBreakWeekDaysColumn.Name = "shiftTemplateBreakWeekDaysColumn";
      this.shiftTemplateBreakWeekDaysColumn.Width = 120;
      // 
      // shiftTemplateBreakTimePeriodColumn
      // 
      this.shiftTemplateBreakTimePeriodColumn.DataPropertyName = "TimePeriod";
      this.shiftTemplateBreakTimePeriodColumn.HeaderText = "TimePeriodColumn";
      this.shiftTemplateBreakTimePeriodColumn.Name = "shiftTemplateBreakTimePeriodColumn";
      this.shiftTemplateBreakTimePeriodColumn.Width = 120;
      // 
      // shiftTemplateBreakDayColumn
      // 
      this.shiftTemplateBreakDayColumn.DataPropertyName = "Day";
      this.shiftTemplateBreakDayColumn.HeaderText = "DayColumn";
      this.shiftTemplateBreakDayColumn.Name = "shiftTemplateBreakDayColumn";
      this.shiftTemplateBreakDayColumn.Width = 86;
      // 
      // shiftTemplateDataGridView
      // 
      this.shiftTemplateDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.shiftTemplateDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.shiftTemplateDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.shiftTemplateDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.shiftTemplateIdColumn,
                  this.shiftTemplateNameColumn});
      this.shiftTemplateDataGridView.Cursor = System.Windows.Forms.Cursors.Default;
      this.shiftTemplateDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftTemplateDataGridView.Location = new System.Drawing.Point(3, 3);
      this.shiftTemplateDataGridView.MultiSelect = false;
      this.shiftTemplateDataGridView.Name = "shiftTemplateDataGridView";
      this.baseLayout.SetRowSpan(this.shiftTemplateDataGridView, 2);
      this.shiftTemplateDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.shiftTemplateDataGridView.Size = new System.Drawing.Size(327, 435);
      this.shiftTemplateDataGridView.TabIndex = 1;
      this.shiftTemplateDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.shiftTemplateDataGridView.SelectionChanged += new System.EventHandler(this.ShiftTemplateDataGridViewSelectionChanged);
      this.shiftTemplateDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // shiftTemplateIdColumn
      // 
      this.shiftTemplateIdColumn.DataPropertyName = "Id";
      this.shiftTemplateIdColumn.HeaderText = "IdColumn";
      this.shiftTemplateIdColumn.Name = "shiftTemplateIdColumn";
      this.shiftTemplateIdColumn.ReadOnly = true;
      this.shiftTemplateIdColumn.Visible = false;
      this.shiftTemplateIdColumn.Width = 76;
      // 
      // shiftTemplateNameColumn
      // 
      this.shiftTemplateNameColumn.DataPropertyName = "Name";
      this.shiftTemplateNameColumn.HeaderText = "NameColumn";
      this.shiftTemplateNameColumn.Name = "shiftTemplateNameColumn";
      this.shiftTemplateNameColumn.Width = 95;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.tableLayoutPanel2);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(336, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(328, 214);
      this.groupBox1.TabIndex = 5;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Shift template item";
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 2;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 111F));
      this.tableLayoutPanel2.Controls.Add(this.shiftTemplateItemDataGridView, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.shiftTemplateItemAddButton, 1, 1);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 2;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(322, 195);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // shiftTemplateItemDataGridView
      // 
      this.shiftTemplateItemDataGridView.AllowUserToAddRows = false;
      this.shiftTemplateItemDataGridView.AllowUserToResizeRows = false;
      this.shiftTemplateItemDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.shiftTemplateItemDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.shiftTemplateItemDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.shiftTemplateItemIdColumn,
                  this.shiftTemplateItemOrderColumn,
                  this.shiftTemplateItemShiftColumn,
                  this.shiftTemplateItemWeekDaysColumn,
                  this.shiftTemplateItemTimePeriodOfDayColumn,
                  this.shiftTemplateItemDayColumn});
      this.tableLayoutPanel2.SetColumnSpan(this.shiftTemplateItemDataGridView, 2);
      this.shiftTemplateItemDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftTemplateItemDataGridView.Location = new System.Drawing.Point(3, 3);
      this.shiftTemplateItemDataGridView.Name = "shiftTemplateItemDataGridView";
      this.shiftTemplateItemDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.shiftTemplateItemDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.shiftTemplateItemDataGridView.Size = new System.Drawing.Size(316, 166);
      this.shiftTemplateItemDataGridView.TabIndex = 3;
      this.shiftTemplateItemDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.ShiftTemplateItemDataGridViewCellValueChanged);
      this.shiftTemplateItemDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.ShiftTemplateItemDataGridViewUserDeletingRow);
      // 
      // shiftTemplateItemIdColumn
      // 
      this.shiftTemplateItemIdColumn.DataPropertyName = "Id";
      this.shiftTemplateItemIdColumn.HeaderText = "IdColumn";
      this.shiftTemplateItemIdColumn.Name = "shiftTemplateItemIdColumn";
      this.shiftTemplateItemIdColumn.ReadOnly = true;
      this.shiftTemplateItemIdColumn.Visible = false;
      this.shiftTemplateItemIdColumn.Width = 76;
      // 
      // shiftTemplateItemOrderColumn
      // 
      this.shiftTemplateItemOrderColumn.DataPropertyName = "Order";
      this.shiftTemplateItemOrderColumn.HeaderText = "OrderColumn";
      this.shiftTemplateItemOrderColumn.Name = "shiftTemplateItemOrderColumn";
      this.shiftTemplateItemOrderColumn.Visible = false;
      this.shiftTemplateItemOrderColumn.Width = 93;
      // 
      // shiftTemplateItemShiftColumn
      // 
      this.shiftTemplateItemShiftColumn.DataPropertyName = "Shift";
      this.shiftTemplateItemShiftColumn.HeaderText = "ShiftColumn";
      this.shiftTemplateItemShiftColumn.Name = "shiftTemplateItemShiftColumn";
      this.shiftTemplateItemShiftColumn.Width = 88;
      // 
      // shiftTemplateItemWeekDaysColumn
      // 
      this.shiftTemplateItemWeekDaysColumn.DataPropertyName = "WeekDays";
      this.shiftTemplateItemWeekDaysColumn.HeaderText = "WeekDaysColumn";
      this.shiftTemplateItemWeekDaysColumn.Name = "shiftTemplateItemWeekDaysColumn";
      this.shiftTemplateItemWeekDaysColumn.Width = 120;
      // 
      // shiftTemplateItemTimePeriodOfDayColumn
      // 
      this.shiftTemplateItemTimePeriodOfDayColumn.DataPropertyName = "TimePeriod";
      this.shiftTemplateItemTimePeriodOfDayColumn.HeaderText = "TimePeriodColumn";
      this.shiftTemplateItemTimePeriodOfDayColumn.Name = "shiftTemplateItemTimePeriodOfDayColumn";
      this.shiftTemplateItemTimePeriodOfDayColumn.Width = 120;
      // 
      // shiftTemplateItemDayColumn
      // 
      this.shiftTemplateItemDayColumn.DataPropertyName = "Day";
      this.shiftTemplateItemDayColumn.HeaderText = "DayColumn";
      this.shiftTemplateItemDayColumn.Name = "shiftTemplateItemDayColumn";
      this.shiftTemplateItemDayColumn.Width = 86;
      // 
      // shiftTemplateItemAddButton
      // 
      this.shiftTemplateItemAddButton.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftTemplateItemAddButton.Location = new System.Drawing.Point(214, 172);
      this.shiftTemplateItemAddButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.shiftTemplateItemAddButton.Name = "shiftTemplateItemAddButton";
      this.shiftTemplateItemAddButton.Size = new System.Drawing.Size(105, 23);
      this.shiftTemplateItemAddButton.TabIndex = 2;
      this.shiftTemplateItemAddButton.Text = "Add an item";
      this.shiftTemplateItemAddButton.UseVisualStyleBackColor = true;
      this.shiftTemplateItemAddButton.Click += new System.EventHandler(this.ShiftTemplateItemAddButtonClick);
      // 
      // ShiftTemplateConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "ShiftTemplateConfig";
      this.Size = new System.Drawing.Size(667, 441);
      this.Load += new System.EventHandler(this.ShiftTemplateConfigLoad);
      this.Enter += new System.EventHandler(this.ShiftTemplateConfigEnter);
      this.Leave += new System.EventHandler(this.ShiftTemplateConfigLeave);
      this.Validated += new System.EventHandler(this.ShiftTemplateConfigValidated);
      this.baseLayout.ResumeLayout(false);
      this.shiftTemplateBreakGroupBox.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.shiftTemplateBreakDataGridView)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.shiftTemplateDataGridView)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.shiftTemplateItemDataGridView)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateBreakDayColumn;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Button shiftTemplateBreakAddButton;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateBreakTimePeriodColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateBreakWeekDaysColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateBreakIdColumn;
    private System.Windows.Forms.DataGridView shiftTemplateBreakDataGridView;
    private System.Windows.Forms.GroupBox shiftTemplateBreakGroupBox;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateItemDayColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateItemTimePeriodOfDayColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateItemWeekDaysColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateItemShiftColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateItemOrderColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateItemIdColumn;
    private System.Windows.Forms.DataGridView shiftTemplateItemDataGridView;
    private System.Windows.Forms.Button shiftTemplateItemAddButton;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateNameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftTemplateIdColumn;
    private Lemoine.BaseControls.SortableDataGridView shiftTemplateDataGridView;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
