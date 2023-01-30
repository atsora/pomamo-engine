// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MachineModuleConfig
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
      this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.codeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.externalCodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.monitoredMachineColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.cncAcquisitionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.configPrefixColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.configParametersColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.sequenceDetectionMethodColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.SequenceVariableColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.milestoneVariableColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.startCycleDetectionMethodColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.startCycleVariableColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.cycleDetectionMethodColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.CycleVariableColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.detectionMethodVariableColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.autoSequenceActivityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.defaultDetectionMethodColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // dataGridView
      // 
      this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idColumn,
            this.versionColumn,
            this.nameColumn,
            this.codeColumn,
            this.externalCodeColumn,
            this.monitoredMachineColumn,
            this.cncAcquisitionColumn,
            this.configPrefixColumn,
            this.configParametersColumn,
            this.sequenceDetectionMethodColumn,
            this.SequenceVariableColumn,
            this.milestoneVariableColumn,
            this.startCycleDetectionMethodColumn,
            this.startCycleVariableColumn,
            this.cycleDetectionMethodColumn,
            this.CycleVariableColumn,
            this.detectionMethodVariableColumn,
            this.autoSequenceActivityColumn,
            this.defaultDetectionMethodColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(855, 385);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.dataGridView.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.DataGridViewUserAddedRow);
      this.dataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // idColumn
      // 
      this.idColumn.DataPropertyName = "Id";
      this.idColumn.HeaderText = "Id";
      this.idColumn.Name = "idColumn";
      this.idColumn.ReadOnly = true;
      // 
      // versionColumn
      // 
      this.versionColumn.DataPropertyName = "Version";
      this.versionColumn.HeaderText = "Version";
      this.versionColumn.Name = "versionColumn";
      this.versionColumn.ReadOnly = true;
      this.versionColumn.Visible = false;
      // 
      // nameColumn
      // 
      this.nameColumn.DataPropertyName = "Name";
      this.nameColumn.HeaderText = "Name";
      this.nameColumn.Name = "nameColumn";
      // 
      // codeColumn
      // 
      this.codeColumn.DataPropertyName = "Code";
      this.codeColumn.HeaderText = "Code";
      this.codeColumn.Name = "codeColumn";
      // 
      // externalCodeColumn
      // 
      this.externalCodeColumn.DataPropertyName = "ExternalCode";
      this.externalCodeColumn.HeaderText = "ExternalCode";
      this.externalCodeColumn.Name = "externalCodeColumn";
      // 
      // monitoredMachineColumn
      // 
      this.monitoredMachineColumn.DataPropertyName = "MonitoredMachine";
      this.monitoredMachineColumn.HeaderText = "MonitoredMachine";
      this.monitoredMachineColumn.Name = "monitoredMachineColumn";
      // 
      // cncAcquisitionColumn
      // 
      this.cncAcquisitionColumn.DataPropertyName = "CncAcquisition";
      this.cncAcquisitionColumn.HeaderText = "CncAcquisition";
      this.cncAcquisitionColumn.Name = "cncAcquisitionColumn";
      // 
      // configPrefixColumn
      // 
      this.configPrefixColumn.DataPropertyName = "ConfigPrefix";
      this.configPrefixColumn.HeaderText = "ConfigPrefix";
      this.configPrefixColumn.Name = "configPrefixColumn";
      // 
      // configParametersColumn
      // 
      this.configParametersColumn.DataPropertyName = "ConfigParameters";
      this.configParametersColumn.HeaderText = "ConfigParameters";
      this.configParametersColumn.Name = "configParametersColumn";
      // 
      // sequenceDetectionMethodColumn
      // 
      this.sequenceDetectionMethodColumn.DataPropertyName = "SequenceDetectionMethod";
      this.sequenceDetectionMethodColumn.HeaderText = "SequenceDetectionMethod";
      this.sequenceDetectionMethodColumn.Name = "sequenceDetectionMethodColumn";
      // 
      // SequenceVariableColumn
      // 
      this.SequenceVariableColumn.DataPropertyName = "SequenceVariable";
      this.SequenceVariableColumn.HeaderText = "SequenceVariable";
      this.SequenceVariableColumn.Name = "SequenceVariableColumn";
      // 
      // milestoneVariableColumn
      // 
      this.milestoneVariableColumn.DataPropertyName = "MilestoneVariable";
      this.milestoneVariableColumn.HeaderText = "Milestone Variable";
      this.milestoneVariableColumn.Name = "milestoneVariableColumn";
      // 
      // startCycleDetectionMethodColumn
      // 
      this.startCycleDetectionMethodColumn.DataPropertyName = "StartCycleDetectionMethod";
      this.startCycleDetectionMethodColumn.HeaderText = "StartCycleDetectionMethod";
      this.startCycleDetectionMethodColumn.Name = "startCycleDetectionMethodColumn";
      // 
      // startCycleVariableColumn
      // 
      this.startCycleVariableColumn.DataPropertyName = "StartCycleVariable";
      this.startCycleVariableColumn.HeaderText = "StartCycleVariable";
      this.startCycleVariableColumn.Name = "startCycleVariableColumn";
      // 
      // cycleDetectionMethodColumn
      // 
      this.cycleDetectionMethodColumn.DataPropertyName = "CycleDetectionMethod";
      this.cycleDetectionMethodColumn.HeaderText = "CycleDetectionMethod";
      this.cycleDetectionMethodColumn.Name = "cycleDetectionMethodColumn";
      // 
      // CycleVariableColumn
      // 
      this.CycleVariableColumn.DataPropertyName = "CycleVariable";
      this.CycleVariableColumn.HeaderText = "CycleVariable";
      this.CycleVariableColumn.Name = "CycleVariableColumn";
      // 
      // detectionMethodVariableColumn
      // 
      this.detectionMethodVariableColumn.DataPropertyName = "DetectionMethodVariable";
      this.detectionMethodVariableColumn.HeaderText = "DetectionMethodVariable";
      this.detectionMethodVariableColumn.Name = "detectionMethodVariableColumn";
      // 
      // autoSequenceActivityColumn
      // 
      this.autoSequenceActivityColumn.DataPropertyName = "AutoSequenceActivity";
      this.autoSequenceActivityColumn.HeaderText = "AutoSequenceActivity";
      this.autoSequenceActivityColumn.Name = "autoSequenceActivityColumn";
      // 
      // defaultDetectionMethodColumn
      // 
      this.defaultDetectionMethodColumn.DataPropertyName = "DefaultDetectionMethod";
      this.defaultDetectionMethodColumn.HeaderText = "DefaultDetectionMethod";
      this.defaultDetectionMethodColumn.Name = "defaultDetectionMethodColumn";
      this.defaultDetectionMethodColumn.ReadOnly = true;
      this.defaultDetectionMethodColumn.Visible = false;
      // 
      // MachineModuleConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "MachineModuleConfig";
      this.Size = new System.Drawing.Size(855, 385);
      this.Load += new System.EventHandler(this.MachineModuleConfigLoad);
      this.Validated += new System.EventHandler(this.MachineModuleConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn codeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn externalCodeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn monitoredMachineColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn cncAcquisitionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn configPrefixColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn configParametersColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn sequenceDetectionMethodColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn SequenceVariableColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn milestoneVariableColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn startCycleDetectionMethodColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn startCycleVariableColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn cycleDetectionMethodColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn CycleVariableColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn detectionMethodVariableColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn autoSequenceActivityColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn defaultDetectionMethodColumn;
  }
}
