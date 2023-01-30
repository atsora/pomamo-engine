// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MachineStatusConfig
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
      this.versionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.monitoredMachineColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.cncMachineModeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineModeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineObservationStateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.reasonColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.reasonDetailsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.reasonSlotEndColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.defaultReasonColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.reasonMachineAssociationEndColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.manualActivityColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.manualActivityEndColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.AddButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // dataGridView
      // 
      this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.idColumn,
                  this.versionColumn,
                  this.monitoredMachineColumn,
                  this.cncMachineModeColumn,
                  this.machineModeColumn,
                  this.machineObservationStateColumn,
                  this.shiftColumn,
                  this.reasonColumn,
                  this.reasonDetailsColumn,
                  this.reasonSlotEndColumn,
                  this.defaultReasonColumn,
                  this.reasonMachineAssociationEndColumn,
                  this.manualActivityColumn,
                  this.manualActivityEndColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(541, 216);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.dataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // idColumn
      // 
      this.idColumn.DataPropertyName = "Id";
      this.idColumn.HeaderText = "Id";
      this.idColumn.Name = "idColumn";
      this.idColumn.ReadOnly = true;
      this.idColumn.Width = 41;
      // 
      // versionColumn
      // 
      this.versionColumn.DataPropertyName = "Version";
      this.versionColumn.HeaderText = "Version";
      this.versionColumn.Name = "versionColumn";
      this.versionColumn.ReadOnly = true;
      this.versionColumn.Visible = false;
      this.versionColumn.Width = 67;
      // 
      // monitoredMachineColumn
      // 
      this.monitoredMachineColumn.DataPropertyName = "MonitoredMachine";
      this.monitoredMachineColumn.HeaderText = "monitoredMachineColumn";
      this.monitoredMachineColumn.Name = "monitoredMachineColumn";
      this.monitoredMachineColumn.Width = 154;
      // 
      // cncMachineModeColumn
      // 
      this.cncMachineModeColumn.DataPropertyName = "CncMachineMode";
      this.cncMachineModeColumn.HeaderText = "cncMachineModeColumn";
      this.cncMachineModeColumn.Name = "cncMachineModeColumn";
      this.cncMachineModeColumn.Width = 153;
      // 
      // machineModeColumn
      // 
      this.machineModeColumn.DataPropertyName = "MachineMode";
      this.machineModeColumn.HeaderText = "machineModeColumn";
      this.machineModeColumn.Name = "machineModeColumn";
      this.machineModeColumn.Width = 134;
      // 
      // machineObservationStateColumn
      // 
      this.machineObservationStateColumn.DataPropertyName = "MachineObservationState";
      this.machineObservationStateColumn.HeaderText = "machineObservationStateColumn";
      this.machineObservationStateColumn.Name = "machineObservationStateColumn";
      this.machineObservationStateColumn.Width = 189;
      // 
      // shiftColumn
      // 
      this.shiftColumn.DataPropertyName = "Shift";
      this.shiftColumn.HeaderText = "shiftColumn";
      this.shiftColumn.Name = "shiftColumn";
      this.shiftColumn.Width = 86;
      // 
      // reasonColumn
      // 
      this.reasonColumn.DataPropertyName = "Reason";
      this.reasonColumn.HeaderText = "reasonColumn";
      this.reasonColumn.Name = "reasonColumn";
      this.reasonColumn.Width = 99;
      // 
      // reasonDetailsColumn
      // 
      this.reasonDetailsColumn.DataPropertyName = "ReasonDetails";
      this.reasonDetailsColumn.HeaderText = "reasonDetailsColumn";
      this.reasonDetailsColumn.Name = "reasonDetailsColumn";
      this.reasonDetailsColumn.Width = 131;
      // 
      // reasonSlotEndColumn
      // 
      this.reasonSlotEndColumn.DataPropertyName = "ReasonSlotEnd";
      this.reasonSlotEndColumn.HeaderText = "reasonSlotEndColumn";
      this.reasonSlotEndColumn.Name = "reasonSlotEndColumn";
      this.reasonSlotEndColumn.Width = 136;
      // 
      // defaultReasonColumn
      // 
      this.defaultReasonColumn.DataPropertyName = "DefaultReason";
      this.defaultReasonColumn.HeaderText = "defaultReasonColumn";
      this.defaultReasonColumn.Name = "defaultReasonColumn";
      this.defaultReasonColumn.Width = 117;
      // 
      // reasonMachineAssociationEndColumn
      // 
      this.reasonMachineAssociationEndColumn.DataPropertyName = "ReasonMachineAssociaionEnd";
      this.reasonMachineAssociationEndColumn.HeaderText = "reasonMachineAssociationEndColumn";
      this.reasonMachineAssociationEndColumn.Name = "reasonMachineAssociationEndColumn";
      this.reasonMachineAssociationEndColumn.Width = 213;
      // 
      // manualActivityColumn
      // 
      this.manualActivityColumn.DataPropertyName = "ManualActivity";
      this.manualActivityColumn.HeaderText = "manualActivityColumn";
      this.manualActivityColumn.Name = "manualActivityColumn";
      this.manualActivityColumn.Width = 116;
      // 
      // manualActivityEndColumn
      // 
      this.manualActivityEndColumn.DataPropertyName = "ManualActivityEnd";
      this.manualActivityEndColumn.HeaderText = "manualActivityEndColumn";
      this.manualActivityEndColumn.Name = "manualActivityEndColumn";
      this.manualActivityEndColumn.Width = 154;
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.dataGridView);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.AddButton);
      this.splitContainer1.Size = new System.Drawing.Size(541, 245);
      this.splitContainer1.SplitterDistance = 216;
      this.splitContainer1.TabIndex = 1;
      // 
      // AddButton
      // 
      this.AddButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.AddButton.Location = new System.Drawing.Point(0, 0);
      this.AddButton.Name = "AddButton";
      this.AddButton.Size = new System.Drawing.Size(75, 25);
      this.AddButton.TabIndex = 0;
      this.AddButton.Text = "button1";
      this.AddButton.UseVisualStyleBackColor = true;
      this.AddButton.Click += new System.EventHandler(this.AddButtonClick);
      // 
      // MachineStatusConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "MachineStatusConfig";
      this.Size = new System.Drawing.Size(541, 245);
      this.Load += new System.EventHandler(this.MachineStatusConfigLoad);
      this.Validated += new System.EventHandler(this.MachineStatusConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewTextBoxColumn manualActivityEndColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn manualActivityColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reasonMachineAssociationEndColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn defaultReasonColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reasonSlotEndColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reasonDetailsColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reasonColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineObservationStateColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineModeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn cncMachineModeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn monitoredMachineColumn;
    private System.Windows.Forms.Button AddButton;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
  }
}
