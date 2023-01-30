// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MachineStateTemplateConfig
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
      this.machineStateTemplateDataGridView = new Lemoine.BaseControls.SortableDataGridView();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.splitContainer3 = new System.Windows.Forms.SplitContainer();
      this.machineStateTemplateItemGroupBox = new System.Windows.Forms.GroupBox();
      this.machineStateTemplateItemDataGridView = new System.Windows.Forms.DataGridView();
      this.machineStateTemplateItemIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateItemOrderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateItemMachineObservationStateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateItemShiftColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateItemWeekDaysColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateItemTimePeriodOfDayColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateItemDayColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateItemAddButton = new System.Windows.Forms.Button();
      this.splitContainer4 = new System.Windows.Forms.SplitContainer();
      this.machineStateTemplateStopGroupBox = new System.Windows.Forms.GroupBox();
      this.machineStateTemplateStopDataGridView = new System.Windows.Forms.DataGridView();
      this.machineStateTemplateStopIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateStopWeekDaysColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateStopLocalTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateStopAddButton = new System.Windows.Forms.Button();
      this.machineStateTemplateIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateTranslationkeyColum = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.categoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateUserRequiredColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.machineStateTemplateShiftRequiredColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.machineStateTemplateOnSiteColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.machineStateTemplateSiteAttendanceChangeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.linkOperationDirectionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.machineStateTemplateDataGridView)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.splitContainer3.Panel1.SuspendLayout();
      this.splitContainer3.Panel2.SuspendLayout();
      this.splitContainer3.SuspendLayout();
      this.machineStateTemplateItemGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.machineStateTemplateItemDataGridView)).BeginInit();
      this.splitContainer4.Panel1.SuspendLayout();
      this.splitContainer4.Panel2.SuspendLayout();
      this.splitContainer4.SuspendLayout();
      this.machineStateTemplateStopGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.machineStateTemplateStopDataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // machineStateTemplateDataGridView
      // 
      this.machineStateTemplateDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.machineStateTemplateDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.machineStateTemplateDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.machineStateTemplateDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
      this.machineStateTemplateIdColumn,
      this.machineStateTemplateTranslationkeyColum,
      this.machineStateTemplateNameColumn,
      this.categoryColumn,
      this.machineStateTemplateUserRequiredColumn,
      this.machineStateTemplateShiftRequiredColumn,
      this.machineStateTemplateOnSiteColumn,
      this.machineStateTemplateSiteAttendanceChangeColumn,
      this.linkOperationDirectionColumn});
      this.machineStateTemplateDataGridView.Cursor = System.Windows.Forms.Cursors.Default;
      this.machineStateTemplateDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateDataGridView.Location = new System.Drawing.Point(0, 0);
      this.machineStateTemplateDataGridView.MultiSelect = false;
      this.machineStateTemplateDataGridView.Name = "machineStateTemplateDataGridView";
      this.machineStateTemplateDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.machineStateTemplateDataGridView.Size = new System.Drawing.Size(304, 441);
      this.machineStateTemplateDataGridView.TabIndex = 0;
      this.machineStateTemplateDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.machineStateTemplateDataGridView.SelectionChanged += new System.EventHandler(this.MachineStateTemplateDataGridViewSelectionChanged);
      this.machineStateTemplateDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.machineStateTemplateDataGridView);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Size = new System.Drawing.Size(667, 441);
      this.splitContainer1.SplitterDistance = 304;
      this.splitContainer1.TabIndex = 1;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.splitContainer4);
      this.splitContainer2.Size = new System.Drawing.Size(359, 441);
      this.splitContainer2.SplitterDistance = 205;
      this.splitContainer2.TabIndex = 0;
      // 
      // splitContainer3
      // 
      this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer3.Location = new System.Drawing.Point(0, 0);
      this.splitContainer3.Name = "splitContainer3";
      this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer3.Panel1
      // 
      this.splitContainer3.Panel1.Controls.Add(this.machineStateTemplateItemGroupBox);
      // 
      // splitContainer3.Panel2
      // 
      this.splitContainer3.Panel2.Controls.Add(this.machineStateTemplateItemAddButton);
      this.splitContainer3.Size = new System.Drawing.Size(359, 205);
      this.splitContainer3.SplitterDistance = 176;
      this.splitContainer3.TabIndex = 0;
      // 
      // machineStateTemplateItemGroupBox
      // 
      this.machineStateTemplateItemGroupBox.Controls.Add(this.machineStateTemplateItemDataGridView);
      this.machineStateTemplateItemGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateItemGroupBox.Location = new System.Drawing.Point(0, 0);
      this.machineStateTemplateItemGroupBox.Name = "machineStateTemplateItemGroupBox";
      this.machineStateTemplateItemGroupBox.Size = new System.Drawing.Size(359, 176);
      this.machineStateTemplateItemGroupBox.TabIndex = 1;
      this.machineStateTemplateItemGroupBox.TabStop = false;
      this.machineStateTemplateItemGroupBox.Text = "groupBox1";
      // 
      // machineStateTemplateItemDataGridView
      // 
      this.machineStateTemplateItemDataGridView.AllowUserToAddRows = false;
      this.machineStateTemplateItemDataGridView.AllowUserToResizeRows = false;
      this.machineStateTemplateItemDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.machineStateTemplateItemDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.machineStateTemplateItemDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
      this.machineStateTemplateItemIdColumn,
      this.machineStateTemplateItemOrderColumn,
      this.machineStateTemplateItemMachineObservationStateColumn,
      this.machineStateTemplateItemShiftColumn,
      this.machineStateTemplateItemWeekDaysColumn,
      this.machineStateTemplateItemTimePeriodOfDayColumn,
      this.machineStateTemplateItemDayColumn});
      this.machineStateTemplateItemDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateItemDataGridView.Location = new System.Drawing.Point(3, 16);
      this.machineStateTemplateItemDataGridView.Name = "machineStateTemplateItemDataGridView";
      this.machineStateTemplateItemDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.machineStateTemplateItemDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.machineStateTemplateItemDataGridView.Size = new System.Drawing.Size(353, 157);
      this.machineStateTemplateItemDataGridView.TabIndex = 0;
      this.machineStateTemplateItemDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.MachineStateTemplateItemDataGridViewCellValueChanged);
      this.machineStateTemplateItemDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.MachineStateTemplateItemDataGridViewUserDeletingRow);
      // 
      // machineStateTemplateItemIdColumn
      // 
      this.machineStateTemplateItemIdColumn.DataPropertyName = "Id";
      this.machineStateTemplateItemIdColumn.HeaderText = "IdColumn";
      this.machineStateTemplateItemIdColumn.Name = "machineStateTemplateItemIdColumn";
      this.machineStateTemplateItemIdColumn.ReadOnly = true;
      this.machineStateTemplateItemIdColumn.Visible = false;
      // 
      // machineStateTemplateItemOrderColumn
      // 
      this.machineStateTemplateItemOrderColumn.DataPropertyName = "Order";
      this.machineStateTemplateItemOrderColumn.HeaderText = "OrderColumn";
      this.machineStateTemplateItemOrderColumn.Name = "machineStateTemplateItemOrderColumn";
      this.machineStateTemplateItemOrderColumn.Visible = false;
      // 
      // machineStateTemplateItemMachineObservationStateColumn
      // 
      this.machineStateTemplateItemMachineObservationStateColumn.DataPropertyName = "MachineObservationState";
      this.machineStateTemplateItemMachineObservationStateColumn.HeaderText = "MachineObservationStateColumn";
      this.machineStateTemplateItemMachineObservationStateColumn.Name = "machineStateTemplateItemMachineObservationStateColumn";
      this.machineStateTemplateItemMachineObservationStateColumn.Width = 190;
      // 
      // machineStateTemplateItemShiftColumn
      // 
      this.machineStateTemplateItemShiftColumn.DataPropertyName = "Shift";
      this.machineStateTemplateItemShiftColumn.HeaderText = "ShiftColumn";
      this.machineStateTemplateItemShiftColumn.Name = "machineStateTemplateItemShiftColumn";
      this.machineStateTemplateItemShiftColumn.Width = 88;
      // 
      // machineStateTemplateItemWeekDaysColumn
      // 
      this.machineStateTemplateItemWeekDaysColumn.DataPropertyName = "WeekDays";
      this.machineStateTemplateItemWeekDaysColumn.HeaderText = "WeekDaysColumn";
      this.machineStateTemplateItemWeekDaysColumn.Name = "machineStateTemplateItemWeekDaysColumn";
      this.machineStateTemplateItemWeekDaysColumn.Width = 120;
      // 
      // machineStateTemplateItemTimePeriodOfDayColumn
      // 
      this.machineStateTemplateItemTimePeriodOfDayColumn.DataPropertyName = "TimePeriod";
      this.machineStateTemplateItemTimePeriodOfDayColumn.HeaderText = "TimePeriodColumn";
      this.machineStateTemplateItemTimePeriodOfDayColumn.Name = "machineStateTemplateItemTimePeriodOfDayColumn";
      this.machineStateTemplateItemTimePeriodOfDayColumn.Width = 120;
      // 
      // machineStateTemplateItemDayColumn
      // 
      this.machineStateTemplateItemDayColumn.DataPropertyName = "Day";
      this.machineStateTemplateItemDayColumn.HeaderText = "DayColumn";
      this.machineStateTemplateItemDayColumn.Name = "machineStateTemplateItemDayColumn";
      this.machineStateTemplateItemDayColumn.Width = 86;
      // 
      // machineStateTemplateItemAddButton
      // 
      this.machineStateTemplateItemAddButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.machineStateTemplateItemAddButton.Location = new System.Drawing.Point(0, 0);
      this.machineStateTemplateItemAddButton.Name = "machineStateTemplateItemAddButton";
      this.machineStateTemplateItemAddButton.Size = new System.Drawing.Size(75, 25);
      this.machineStateTemplateItemAddButton.TabIndex = 0;
      this.machineStateTemplateItemAddButton.Text = "button1";
      this.machineStateTemplateItemAddButton.UseVisualStyleBackColor = true;
      this.machineStateTemplateItemAddButton.Click += new System.EventHandler(this.MachineStateTemplateItemAddButtonClick);
      // 
      // splitContainer4
      // 
      this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer4.Location = new System.Drawing.Point(0, 0);
      this.splitContainer4.Name = "splitContainer4";
      this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer4.Panel1
      // 
      this.splitContainer4.Panel1.Controls.Add(this.machineStateTemplateStopGroupBox);
      // 
      // splitContainer4.Panel2
      // 
      this.splitContainer4.Panel2.Controls.Add(this.machineStateTemplateStopAddButton);
      this.splitContainer4.Size = new System.Drawing.Size(359, 232);
      this.splitContainer4.SplitterDistance = 202;
      this.splitContainer4.TabIndex = 0;
      // 
      // machineStateTemplateStopGroupBox
      // 
      this.machineStateTemplateStopGroupBox.Controls.Add(this.machineStateTemplateStopDataGridView);
      this.machineStateTemplateStopGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateStopGroupBox.Location = new System.Drawing.Point(0, 0);
      this.machineStateTemplateStopGroupBox.Name = "machineStateTemplateStopGroupBox";
      this.machineStateTemplateStopGroupBox.Size = new System.Drawing.Size(359, 202);
      this.machineStateTemplateStopGroupBox.TabIndex = 2;
      this.machineStateTemplateStopGroupBox.TabStop = false;
      this.machineStateTemplateStopGroupBox.Text = "groupBox2";
      // 
      // machineStateTemplateStopDataGridView
      // 
      this.machineStateTemplateStopDataGridView.AllowUserToAddRows = false;
      this.machineStateTemplateStopDataGridView.AllowUserToResizeRows = false;
      this.machineStateTemplateStopDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.machineStateTemplateStopDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.machineStateTemplateStopDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
      this.machineStateTemplateStopIdColumn,
      this.machineStateTemplateStopWeekDaysColumn,
      this.machineStateTemplateStopLocalTimeColumn});
      this.machineStateTemplateStopDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateStopDataGridView.Location = new System.Drawing.Point(3, 16);
      this.machineStateTemplateStopDataGridView.Name = "machineStateTemplateStopDataGridView";
      this.machineStateTemplateStopDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.machineStateTemplateStopDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.machineStateTemplateStopDataGridView.Size = new System.Drawing.Size(353, 183);
      this.machineStateTemplateStopDataGridView.TabIndex = 0;
      this.machineStateTemplateStopDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.MachineStateTemplateStopDataGridViewCellValueChanged);
      this.machineStateTemplateStopDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.MachineStateTemplateStopDataGridViewUserDeletingRow);
      // 
      // machineStateTemplateStopIdColumn
      // 
      this.machineStateTemplateStopIdColumn.DataPropertyName = "Id";
      this.machineStateTemplateStopIdColumn.HeaderText = "IdColumn";
      this.machineStateTemplateStopIdColumn.Name = "machineStateTemplateStopIdColumn";
      this.machineStateTemplateStopIdColumn.ReadOnly = true;
      this.machineStateTemplateStopIdColumn.Visible = false;
      // 
      // machineStateTemplateStopWeekDaysColumn
      // 
      this.machineStateTemplateStopWeekDaysColumn.DataPropertyName = "WeekDays";
      this.machineStateTemplateStopWeekDaysColumn.HeaderText = "WeekDaysColumn";
      this.machineStateTemplateStopWeekDaysColumn.Name = "machineStateTemplateStopWeekDaysColumn";
      this.machineStateTemplateStopWeekDaysColumn.Width = 120;
      // 
      // machineStateTemplateStopLocalTimeColumn
      // 
      this.machineStateTemplateStopLocalTimeColumn.DataPropertyName = "LocalTime";
      this.machineStateTemplateStopLocalTimeColumn.HeaderText = "LocalTimeColumn";
      this.machineStateTemplateStopLocalTimeColumn.Name = "machineStateTemplateStopLocalTimeColumn";
      this.machineStateTemplateStopLocalTimeColumn.Width = 116;
      // 
      // machineStateTemplateStopAddButton
      // 
      this.machineStateTemplateStopAddButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.machineStateTemplateStopAddButton.Location = new System.Drawing.Point(0, 0);
      this.machineStateTemplateStopAddButton.Name = "machineStateTemplateStopAddButton";
      this.machineStateTemplateStopAddButton.Size = new System.Drawing.Size(75, 26);
      this.machineStateTemplateStopAddButton.TabIndex = 0;
      this.machineStateTemplateStopAddButton.Text = "button1";
      this.machineStateTemplateStopAddButton.UseVisualStyleBackColor = true;
      this.machineStateTemplateStopAddButton.Click += new System.EventHandler(this.MachineStateTemplateStopAddButtonClick);
      // 
      // machineStateTemplateIdColumn
      // 
      this.machineStateTemplateIdColumn.DataPropertyName = "Id";
      this.machineStateTemplateIdColumn.HeaderText = "IdColumn";
      this.machineStateTemplateIdColumn.Name = "machineStateTemplateIdColumn";
      this.machineStateTemplateIdColumn.ReadOnly = true;
      this.machineStateTemplateIdColumn.Visible = false;
      this.machineStateTemplateIdColumn.Width = 76;
      // 
      // machineStateTemplateTranslationkeyColum
      // 
      this.machineStateTemplateTranslationkeyColum.DataPropertyName = "TranslationKey";
      this.machineStateTemplateTranslationkeyColum.HeaderText = "translationkeyColum";
      this.machineStateTemplateTranslationkeyColum.Name = "machineStateTemplateTranslationkeyColum";
      this.machineStateTemplateTranslationkeyColum.Width = 126;
      // 
      // machineStateTemplateNameColumn
      // 
      this.machineStateTemplateNameColumn.DataPropertyName = "Name";
      this.machineStateTemplateNameColumn.HeaderText = "NameColumn";
      this.machineStateTemplateNameColumn.Name = "machineStateTemplateNameColumn";
      this.machineStateTemplateNameColumn.Width = 95;
      // 
      // categoryColumn
      // 
      this.categoryColumn.DataPropertyName = "Category";
      this.categoryColumn.HeaderText = "Category";
      this.categoryColumn.Name = "categoryColumn";
      this.categoryColumn.Width = 74;
      // 
      // machineStateTemplateUserRequiredColumn
      // 
      this.machineStateTemplateUserRequiredColumn.DataPropertyName = "UserRequired";
      this.machineStateTemplateUserRequiredColumn.HeaderText = "UserRequired";
      this.machineStateTemplateUserRequiredColumn.Name = "machineStateTemplateUserRequiredColumn";
      this.machineStateTemplateUserRequiredColumn.Width = 78;
      // 
      // machineStateTemplateShiftRequiredColumn
      // 
      this.machineStateTemplateShiftRequiredColumn.DataPropertyName = "ShiftRequired";
      this.machineStateTemplateShiftRequiredColumn.HeaderText = "ShiftRequired";
      this.machineStateTemplateShiftRequiredColumn.Name = "machineStateTemplateShiftRequiredColumn";
      this.machineStateTemplateShiftRequiredColumn.Width = 77;
      // 
      // machineStateTemplateOnSiteColumn
      // 
      this.machineStateTemplateOnSiteColumn.DataPropertyName = "OnSite";
      this.machineStateTemplateOnSiteColumn.HeaderText = "OnSite";
      this.machineStateTemplateOnSiteColumn.Name = "machineStateTemplateOnSiteColumn";
      this.machineStateTemplateOnSiteColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.machineStateTemplateOnSiteColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.machineStateTemplateOnSiteColumn.Width = 64;
      // 
      // machineStateTemplateSiteAttendanceChangeColumn
      // 
      this.machineStateTemplateSiteAttendanceChangeColumn.DataPropertyName = "SiteAttendanceChange";
      this.machineStateTemplateSiteAttendanceChangeColumn.HeaderText = "SiteAttendanceChange";
      this.machineStateTemplateSiteAttendanceChangeColumn.Name = "machineStateTemplateSiteAttendanceChangeColumn";
      this.machineStateTemplateSiteAttendanceChangeColumn.Width = 142;
      // 
      // linkOperationDirectionColumn
      // 
      this.linkOperationDirectionColumn.DataPropertyName = "LinkOperationDirection";
      this.linkOperationDirectionColumn.FillWeight = 102.2208F;
      this.linkOperationDirectionColumn.HeaderText = "Link Operation Direction";
      this.linkOperationDirectionColumn.Name = "linkOperationDirectionColumn";
      this.linkOperationDirectionColumn.Width = 133;
      // 
      // MachineStateTemplateConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "MachineStateTemplateConfig";
      this.Size = new System.Drawing.Size(667, 441);
      this.Load += new System.EventHandler(this.MachineStateTemplateConfigLoad);
      this.Enter += new System.EventHandler(this.MachineStateTemplateConfigEnter);
      this.Leave += new System.EventHandler(this.MachineStateTemplateConfigLeave);
      this.Validated += new System.EventHandler(this.MachineStateTemplateConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.machineStateTemplateDataGridView)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.ResumeLayout(false);
      this.splitContainer3.Panel1.ResumeLayout(false);
      this.splitContainer3.Panel2.ResumeLayout(false);
      this.splitContainer3.ResumeLayout(false);
      this.machineStateTemplateItemGroupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.machineStateTemplateItemDataGridView)).EndInit();
      this.splitContainer4.Panel1.ResumeLayout(false);
      this.splitContainer4.Panel2.ResumeLayout(false);
      this.splitContainer4.ResumeLayout(false);
      this.machineStateTemplateStopGroupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.machineStateTemplateStopDataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.GroupBox machineStateTemplateStopGroupBox;
    private System.Windows.Forms.GroupBox machineStateTemplateItemGroupBox;
    private System.Windows.Forms.Button machineStateTemplateStopAddButton;
    private System.Windows.Forms.SplitContainer splitContainer4;
    private System.Windows.Forms.Button machineStateTemplateItemAddButton;
    private System.Windows.Forms.SplitContainer splitContainer3;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateNameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateTranslationkeyColum;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateStopLocalTimeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateStopWeekDaysColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateStopIdColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateItemDayColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateItemTimePeriodOfDayColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateItemWeekDaysColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateItemShiftColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateItemMachineObservationStateColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateItemOrderColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateItemIdColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateSiteAttendanceChangeColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn machineStateTemplateOnSiteColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn machineStateTemplateShiftRequiredColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn machineStateTemplateUserRequiredColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateIdColumn;
    private System.Windows.Forms.DataGridView machineStateTemplateStopDataGridView;
    private System.Windows.Forms.DataGridView machineStateTemplateItemDataGridView;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private Lemoine.BaseControls.SortableDataGridView machineStateTemplateDataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn categoryColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn linkOperationDirectionColumn;
  }
}
