// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class CncAcquisitionConfig
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
      this.configFileColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.configPrefixColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.configKeyParamsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.configParametersColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.useProcessColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.computerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.everyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.notRespondingTimeoutColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.sleepBeforeRestartColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.versionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineModulesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.nameColumn,
            this.configFileColumn,
            this.configPrefixColumn,
            this.configKeyParamsColumn,
            this.configParametersColumn,
            this.useProcessColumn,
            this.computerColumn,
            this.everyColumn,
            this.notRespondingTimeoutColumn,
            this.sleepBeforeRestartColumn,
            this.versionColumn,
            this.machineModulesColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(1053, 397);
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
      // nameColumn
      // 
      this.nameColumn.DataPropertyName = "Name";
      this.nameColumn.HeaderText = "Name";
      this.nameColumn.Name = "nameColumn";
      // 
      // configFileColumn
      // 
      this.configFileColumn.DataPropertyName = "ConfigFile";
      this.configFileColumn.HeaderText = "ConfigFile";
      this.configFileColumn.Name = "configFileColumn";
      // 
      // configPrefixColumn
      // 
      this.configPrefixColumn.DataPropertyName = "ConfigPrefix";
      this.configPrefixColumn.HeaderText = "ConfigPrefix";
      this.configPrefixColumn.Name = "configPrefixColumn";
      // 
      // configKeyParamsColumn
      // 
      this.configKeyParamsColumn.DataPropertyName = "ConfigKeyParamsJson";
      this.configKeyParamsColumn.HeaderText = "Key params";
      this.configKeyParamsColumn.Name = "configKeyParamsColumn";
      // 
      // configParametersColumn
      // 
      this.configParametersColumn.DataPropertyName = "ConfigParameters";
      this.configParametersColumn.HeaderText = "ConfigParameters";
      this.configParametersColumn.Name = "configParametersColumn";
      // 
      // useProcessColumn
      // 
      this.useProcessColumn.DataPropertyName = "UseProcess";
      this.useProcessColumn.HeaderText = "UseProcess";
      this.useProcessColumn.Name = "useProcessColumn";
      // 
      // computerColumn
      // 
      this.computerColumn.DataPropertyName = "Computer";
      this.computerColumn.HeaderText = "Computer";
      this.computerColumn.Name = "computerColumn";
      // 
      // everyColumn
      // 
      this.everyColumn.DataPropertyName = "Every";
      this.everyColumn.HeaderText = "Every";
      this.everyColumn.Name = "everyColumn";
      // 
      // notRespondingTimeoutColumn
      // 
      this.notRespondingTimeoutColumn.DataPropertyName = "NotRespondingTimeout";
      this.notRespondingTimeoutColumn.HeaderText = "NotRespondingTimeout";
      this.notRespondingTimeoutColumn.Name = "notRespondingTimeoutColumn";
      // 
      // sleepBeforeRestartColumn
      // 
      this.sleepBeforeRestartColumn.DataPropertyName = "SleepBeforeRestart";
      this.sleepBeforeRestartColumn.HeaderText = "SleepBeforeRestart";
      this.sleepBeforeRestartColumn.Name = "sleepBeforeRestartColumn";
      // 
      // versionColumn
      // 
      this.versionColumn.DataPropertyName = "Version";
      this.versionColumn.HeaderText = "Version";
      this.versionColumn.Name = "versionColumn";
      this.versionColumn.ReadOnly = true;
      this.versionColumn.Visible = false;
      // 
      // machineModulesColumn
      // 
      this.machineModulesColumn.DataPropertyName = "MachineModules";
      this.machineModulesColumn.HeaderText = "MachineModules";
      this.machineModulesColumn.Name = "machineModulesColumn";
      this.machineModulesColumn.ReadOnly = true;
      this.machineModulesColumn.Visible = false;
      // 
      // CncAcquisitionConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "CncAcquisitionConfig";
      this.Size = new System.Drawing.Size(1053, 397);
      this.Load += new System.EventHandler(this.CncAcquisitionConfigLoad);
      this.Validated += new System.EventHandler(this.CncAcquisitionConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn configFileColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn configPrefixColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn configKeyParamsColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn configParametersColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn useProcessColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn computerColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn everyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn notRespondingTimeoutColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn sleepBeforeRestartColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineModulesColumn;
  }
}
