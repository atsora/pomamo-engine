// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class EmailConfigConfig
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
      this.dataGridView = new Lemoine.BaseControls.SortableDataGridView();
      this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.freeFilterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.maxLevelPriority = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.eventLevelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineFilterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.toColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ccColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.bccColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.activeColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.weekDaysColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.timePeriodColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.beginColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.endColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.autoPurgeColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // dataGridView
      // 
      this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.idColumn,
                  this.nameColumn,
                  this.dataTypeColumn,
                  this.freeFilterColumn,
                  this.maxLevelPriority,
                  this.eventLevelColumn,
                  this.machineColumn,
                  this.machineFilterColumn,
                  this.toColumn,
                  this.ccColumn,
                  this.bccColumn,
                  this.activeColumn,
                  this.weekDaysColumn,
                  this.timePeriodColumn,
                  this.beginColumn,
                  this.endColumn,
                  this.autoPurgeColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(800, 245);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridViewCellFormatting);
      this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.dataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // idColumn
      // 
      this.idColumn.DataPropertyName = "Id";
      this.idColumn.HeaderText = "Id";
      this.idColumn.Name = "idColumn";
      this.idColumn.ReadOnly = true;
      this.idColumn.Visible = false;
      this.idColumn.Width = 41;
      // 
      // nameColumn
      // 
      this.nameColumn.DataPropertyName = "Name";
      this.nameColumn.HeaderText = "Name";
      this.nameColumn.Name = "nameColumn";
      this.nameColumn.Width = 60;
      // 
      // dataTypeColumn
      // 
      this.dataTypeColumn.DataPropertyName = "DataType";
      this.dataTypeColumn.HeaderText = "DataType";
      this.dataTypeColumn.Name = "dataTypeColumn";
      this.dataTypeColumn.Width = 79;
      // 
      // freeFilterColumn
      // 
      this.freeFilterColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.freeFilterColumn.DataPropertyName = "FreeFilter";
      this.freeFilterColumn.HeaderText = "FreeFilter";
      this.freeFilterColumn.Name = "freeFilterColumn";
      this.freeFilterColumn.Width = 75;
      // 
      // maxLevelPriority
      // 
      this.maxLevelPriority.DataPropertyName = "MaxLevelPriority";
      this.maxLevelPriority.HeaderText = "MaxLevelPriority";
      this.maxLevelPriority.Name = "maxLevelPriority";
      this.maxLevelPriority.Width = 109;
      // 
      // eventLevelColumn
      // 
      this.eventLevelColumn.DataPropertyName = "EventLevel";
      this.eventLevelColumn.HeaderText = "EventLevel";
      this.eventLevelColumn.Name = "eventLevelColumn";
      this.eventLevelColumn.Width = 86;
      // 
      // machineColumn
      // 
      this.machineColumn.DataPropertyName = "Machine";
      this.machineColumn.HeaderText = "Machine";
      this.machineColumn.Name = "machineColumn";
      this.machineColumn.Width = 73;
      // 
      // machineFilterColumn
      // 
      this.machineFilterColumn.DataPropertyName = "MachineFilter";
      this.machineFilterColumn.HeaderText = "MachineFilter";
      this.machineFilterColumn.Name = "machineFilterColumn";
      this.machineFilterColumn.Width = 95;
      // 
      // toColumn
      // 
      this.toColumn.DataPropertyName = "To";
      this.toColumn.HeaderText = "To";
      this.toColumn.Name = "toColumn";
      this.toColumn.Width = 45;
      // 
      // ccColumn
      // 
      this.ccColumn.DataPropertyName = "Cc";
      this.ccColumn.HeaderText = "Cc";
      this.ccColumn.Name = "ccColumn";
      this.ccColumn.Width = 45;
      // 
      // bccColumn
      // 
      this.bccColumn.DataPropertyName = "Bcc";
      this.bccColumn.HeaderText = "Bcc";
      this.bccColumn.Name = "bccColumn";
      this.bccColumn.Width = 51;
      // 
      // activeColumn
      // 
      this.activeColumn.DataPropertyName = "Active";
      this.activeColumn.HeaderText = "Active";
      this.activeColumn.Name = "activeColumn";
      this.activeColumn.Width = 43;
      // 
      // weekDaysColumn
      // 
      this.weekDaysColumn.DataPropertyName = "WeekDays";
      this.weekDaysColumn.HeaderText = "WeekDays";
      this.weekDaysColumn.Name = "weekDaysColumn";
      this.weekDaysColumn.Width = 85;
      // 
      // timePeriodColumn
      // 
      this.timePeriodColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.timePeriodColumn.DataPropertyName = "TimePeriod";
      this.timePeriodColumn.HeaderText = "TimePeriod";
      this.timePeriodColumn.Name = "timePeriodColumn";
      this.timePeriodColumn.Width = 85;
      // 
      // beginColumn
      // 
      this.beginColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.beginColumn.DataPropertyName = "LocalBegin";
      this.beginColumn.HeaderText = "Begin";
      this.beginColumn.Name = "beginColumn";
      this.beginColumn.Width = 59;
      // 
      // endColumn
      // 
      this.endColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.endColumn.DataPropertyName = "LocalEnd";
      this.endColumn.HeaderText = "End";
      this.endColumn.Name = "endColumn";
      this.endColumn.Width = 51;
      // 
      // autoPurgeColumn
      // 
      this.autoPurgeColumn.DataPropertyName = "AutoPurge";
      this.autoPurgeColumn.HeaderText = "AutoPurge";
      this.autoPurgeColumn.Name = "autoPurgeColumn";
      this.autoPurgeColumn.Width = 63;
      // 
      // EmailConfigConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "EmailConfigConfig";
      this.Size = new System.Drawing.Size(800, 245);
      this.Load += new System.EventHandler(this.EmailConfigConfigLoad);
      this.Validated += new System.EventHandler(this.EmailConfigConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewTextBoxColumn endColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn beginColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn autoPurgeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn timePeriodColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn weekDaysColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn activeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn bccColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn ccColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn toColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineFilterColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn eventLevelColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn maxLevelPriority;
    private System.Windows.Forms.DataGridViewTextBoxColumn freeFilterColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataTypeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
  }
}
