// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class DayTemplateConfig
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
      this.dayTemplateItemAddButton = new System.Windows.Forms.Button();
      this.dayTemplateDataGridView = new Lemoine.BaseControls.SortableDataGridView();
      this.dayTemplateIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dayTemplateNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dayTemplateItemDataGridView = new System.Windows.Forms.DataGridView();
      this.dayTemplateItemIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dayTemplateItemOrderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dayTemplateItemCutOffColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dayTemplateItemWeekDaysColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dayTemplateDataGridView)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dayTemplateItemDataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.dayTemplateItemAddButton, 0, 1);
      this.baseLayout.Controls.Add(this.dayTemplateDataGridView, 0, 0);
      this.baseLayout.Controls.Add(this.dayTemplateItemDataGridView, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.Size = new System.Drawing.Size(667, 441);
      this.baseLayout.TabIndex = 0;
      // 
      // dayTemplateItemAddButton
      // 
      this.dayTemplateItemAddButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.dayTemplateItemAddButton.Location = new System.Drawing.Point(336, 414);
      this.dayTemplateItemAddButton.Name = "dayTemplateItemAddButton";
      this.dayTemplateItemAddButton.Size = new System.Drawing.Size(96, 24);
      this.dayTemplateItemAddButton.TabIndex = 2;
      this.dayTemplateItemAddButton.Text = "Add an item";
      this.dayTemplateItemAddButton.UseVisualStyleBackColor = true;
      this.dayTemplateItemAddButton.Click += new System.EventHandler(this.DayTemplateItemAddButtonClick);
      // 
      // dayTemplateDataGridView
      // 
      this.dayTemplateDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.dayTemplateDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.dayTemplateDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dayTemplateDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.dayTemplateIdColumn,
                  this.dayTemplateNameColumn});
      this.dayTemplateDataGridView.Cursor = System.Windows.Forms.Cursors.Default;
      this.dayTemplateDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dayTemplateDataGridView.Location = new System.Drawing.Point(3, 3);
      this.dayTemplateDataGridView.MultiSelect = false;
      this.dayTemplateDataGridView.Name = "dayTemplateDataGridView";
      this.baseLayout.SetRowSpan(this.dayTemplateDataGridView, 2);
      this.dayTemplateDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dayTemplateDataGridView.Size = new System.Drawing.Size(327, 435);
      this.dayTemplateDataGridView.TabIndex = 1;
      this.dayTemplateDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.dayTemplateDataGridView.SelectionChanged += new System.EventHandler(this.DayTemplateDataGridViewSelectionChanged);
      this.dayTemplateDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // dayTemplateIdColumn
      // 
      this.dayTemplateIdColumn.DataPropertyName = "Id";
      this.dayTemplateIdColumn.HeaderText = "IdColumn";
      this.dayTemplateIdColumn.Name = "dayTemplateIdColumn";
      this.dayTemplateIdColumn.ReadOnly = true;
      this.dayTemplateIdColumn.Visible = false;
      this.dayTemplateIdColumn.Width = 76;
      // 
      // dayTemplateNameColumn
      // 
      this.dayTemplateNameColumn.DataPropertyName = "Name";
      this.dayTemplateNameColumn.HeaderText = "NameColumn";
      this.dayTemplateNameColumn.Name = "dayTemplateNameColumn";
      this.dayTemplateNameColumn.Width = 95;
      // 
      // dayTemplateItemDataGridView
      // 
      this.dayTemplateItemDataGridView.AllowUserToAddRows = false;
      this.dayTemplateItemDataGridView.AllowUserToResizeRows = false;
      this.dayTemplateItemDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.dayTemplateItemDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dayTemplateItemDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.dayTemplateItemIdColumn,
                  this.dayTemplateItemOrderColumn,
                  this.dayTemplateItemCutOffColumn,
                  this.dayTemplateItemWeekDaysColumn});
      this.dayTemplateItemDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dayTemplateItemDataGridView.Location = new System.Drawing.Point(336, 3);
      this.dayTemplateItemDataGridView.Name = "dayTemplateItemDataGridView";
      this.dayTemplateItemDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dayTemplateItemDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dayTemplateItemDataGridView.Size = new System.Drawing.Size(328, 405);
      this.dayTemplateItemDataGridView.TabIndex = 3;
      this.dayTemplateItemDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DayTemplateItemDataGridViewCellValueChanged);
      this.dayTemplateItemDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DayTemplateItemDataGridViewUserDeletingRow);
      // 
      // dayTemplateItemIdColumn
      // 
      this.dayTemplateItemIdColumn.DataPropertyName = "Id";
      this.dayTemplateItemIdColumn.HeaderText = "IdColumn";
      this.dayTemplateItemIdColumn.Name = "dayTemplateItemIdColumn";
      this.dayTemplateItemIdColumn.ReadOnly = true;
      this.dayTemplateItemIdColumn.Visible = false;
      this.dayTemplateItemIdColumn.Width = 76;
      // 
      // dayTemplateItemOrderColumn
      // 
      this.dayTemplateItemOrderColumn.DataPropertyName = "CutOff";
      this.dayTemplateItemOrderColumn.HeaderText = "OrderColumn";
      this.dayTemplateItemOrderColumn.Name = "dayTemplateItemOrderColumn";
      this.dayTemplateItemOrderColumn.Visible = false;
      this.dayTemplateItemOrderColumn.Width = 93;
      // 
      // dayTemplateItemCutOffColumn
      // 
      this.dayTemplateItemCutOffColumn.DataPropertyName = "CutOff";
      this.dayTemplateItemCutOffColumn.HeaderText = "CutOffColumn";
      this.dayTemplateItemCutOffColumn.Name = "dayTemplateItemCutOffColumn";
      this.dayTemplateItemCutOffColumn.Width = 97;
      // 
      // dayTemplateItemWeekDaysColumn
      // 
      this.dayTemplateItemWeekDaysColumn.DataPropertyName = "WeekDays";
      this.dayTemplateItemWeekDaysColumn.HeaderText = "WeekDaysColumn";
      this.dayTemplateItemWeekDaysColumn.Name = "dayTemplateItemWeekDaysColumn";
      this.dayTemplateItemWeekDaysColumn.Width = 120;
      // 
      // DayTemplateConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "DayTemplateConfig";
      this.Size = new System.Drawing.Size(667, 441);
      this.Load += new System.EventHandler(this.DayTemplateConfigLoad);
      this.Enter += new System.EventHandler(this.DayTemplateConfigEnter);
      this.Leave += new System.EventHandler(this.DayTemplateConfigLeave);
      this.Validated += new System.EventHandler(this.DayTemplateConfigValidated);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dayTemplateDataGridView)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dayTemplateItemDataGridView)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewTextBoxColumn dayTemplateItemWeekDaysColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn dayTemplateItemCutOffColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn dayTemplateItemOrderColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn dayTemplateItemIdColumn;
    private System.Windows.Forms.DataGridView dayTemplateItemDataGridView;
    private System.Windows.Forms.Button dayTemplateItemAddButton;
    private System.Windows.Forms.DataGridViewTextBoxColumn dayTemplateNameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn dayTemplateIdColumn;
    private Lemoine.BaseControls.SortableDataGridView dayTemplateDataGridView;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
