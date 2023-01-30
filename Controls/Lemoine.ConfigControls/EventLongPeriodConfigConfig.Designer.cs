// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class EventLongPeriodConfigConfig
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
      this.eventLongPeriodConfigDataGridView = new Lemoine.BaseControls.SortableDataGridView();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.addButton = new System.Windows.Forms.Button();
      this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.monitoredMachineColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineModeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.MachineObservationState = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.triggerDurationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.eventLevelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.eventLongPeriodConfigDataGridView)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // eventLongPeriodConfigDataGridView
      // 
      this.eventLongPeriodConfigDataGridView.AllowUserToAddRows = false;
      this.eventLongPeriodConfigDataGridView.AllowUserToResizeRows = false;
      this.eventLongPeriodConfigDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.eventLongPeriodConfigDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.eventLongPeriodConfigDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.eventLongPeriodConfigDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.idColumn,
                  this.monitoredMachineColumn,
                  this.machineModeColumn,
                  this.MachineObservationState,
                  this.triggerDurationColumn,
                  this.eventLevelColumn});
      this.eventLongPeriodConfigDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventLongPeriodConfigDataGridView.Location = new System.Drawing.Point(0, 0);
      this.eventLongPeriodConfigDataGridView.Name = "eventLongPeriodConfigDataGridView";
      this.eventLongPeriodConfigDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.eventLongPeriodConfigDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.eventLongPeriodConfigDataGridView.Size = new System.Drawing.Size(640, 315);
      this.eventLongPeriodConfigDataGridView.TabIndex = 0;
      this.eventLongPeriodConfigDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.eventLongPeriodConfigDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.eventLongPeriodConfigDataGridView);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.addButton);
      this.splitContainer1.Size = new System.Drawing.Size(640, 344);
      this.splitContainer1.SplitterDistance = 315;
      this.splitContainer1.TabIndex = 1;
      // 
      // addButton
      // 
      this.addButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.addButton.Location = new System.Drawing.Point(0, 0);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(75, 25);
      this.addButton.TabIndex = 0;
      this.addButton.Text = "button1";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.AddButtonClick);
      // 
      // idColumn
      // 
      this.idColumn.DataPropertyName = "Id";
      this.idColumn.HeaderText = "idColumn";
      this.idColumn.Name = "idColumn";
      this.idColumn.ReadOnly = true;
      this.idColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.idColumn.Visible = false;
      this.idColumn.Width = 56;
      // 
      // monitoredMachineColumn
      // 
      this.monitoredMachineColumn.DataPropertyName = "MonitoredMachine";
      this.monitoredMachineColumn.HeaderText = "MonitoredMachine";
      this.monitoredMachineColumn.Name = "monitoredMachineColumn";
      this.monitoredMachineColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.monitoredMachineColumn.Width = 101;
      // 
      // machineModeColumn
      // 
      this.machineModeColumn.DataPropertyName = "MachineMode";
      this.machineModeColumn.HeaderText = "machineMode";
      this.machineModeColumn.Name = "machineModeColumn";
      this.machineModeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.machineModeColumn.Width = 80;
      // 
      // MachineObservationState
      // 
      this.MachineObservationState.DataPropertyName = "MachineObservationState";
      this.MachineObservationState.HeaderText = "machineObservationState";
      this.MachineObservationState.Name = "MachineObservationState";
      this.MachineObservationState.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.MachineObservationState.Width = 135;
      // 
      // triggerDurationColumn
      // 
      this.triggerDurationColumn.DataPropertyName = "TriggerDuration";
      this.triggerDurationColumn.HeaderText = "TriggerDuration";
      this.triggerDurationColumn.Name = "triggerDurationColumn";
      this.triggerDurationColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.triggerDurationColumn.Width = 86;
      // 
      // eventLevelColumn
      // 
      this.eventLevelColumn.DataPropertyName = "Level";
      this.eventLevelColumn.HeaderText = "EventLevel";
      this.eventLevelColumn.Name = "eventLevelColumn";
      this.eventLevelColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.eventLevelColumn.Width = 67;
      // 
      // EventLongPeriodConfigConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "EventLongPeriodConfigConfig";
      this.Size = new System.Drawing.Size(640, 344);
      this.Load += new System.EventHandler(this.EventLongPeriodConfigConfigLoad);
      this.Validated += new System.EventHandler(this.EventLongPeriodConfigConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.eventLongPeriodConfigDataGridView)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewTextBoxColumn eventLevelColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn triggerDurationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn MachineObservationState;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineModeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn monitoredMachineColumn;
    private System.Windows.Forms.Button addButton;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.BaseControls.SortableDataGridView eventLongPeriodConfigDataGridView;
  }
}
