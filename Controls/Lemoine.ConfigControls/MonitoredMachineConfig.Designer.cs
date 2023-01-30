// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MonitoredMachineConfig
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
      this.codeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.externalCodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.monitoringTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.displayPriorityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.mainMachineModuleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.mainCncAcquisitionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.performanceFieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.operationBarColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.palletChangingDurationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.disabledPalletChangingDurationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineModulesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.companyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.departmentColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.categoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.subCategoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.stampingConfigColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.versionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.operationFromCncColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // dataGridView
      // 
      this.dataGridView.AllowUserToDeleteRows = false;
      this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idColumn,
            this.nameColumn,
            this.codeColumn,
            this.externalCodeColumn,
            this.monitoringTypeColumn,
            this.displayPriorityColumn,
            this.mainMachineModuleColumn,
            this.mainCncAcquisitionColumn,
            this.performanceFieldColumn,
            this.operationBarColumn,
            this.palletChangingDurationColumn,
            this.disabledPalletChangingDurationColumn,
            this.machineModulesColumn,
            this.companyColumn,
            this.departmentColumn,
            this.categoryColumn,
            this.subCategoryColumn,
            this.stampingConfigColumn,
            this.versionColumn,
            this.operationFromCncColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(866, 472);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.dataGridView.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.DataGridViewUserAddedRow);
      // 
      // idColumn
      // 
      this.idColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.idColumn.DataPropertyName = "Id";
      this.idColumn.HeaderText = "Id";
      this.idColumn.Name = "idColumn";
      this.idColumn.ReadOnly = true;
      this.idColumn.Width = 41;
      // 
      // nameColumn
      // 
      this.nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.nameColumn.DataPropertyName = "Name";
      this.nameColumn.HeaderText = "Name";
      this.nameColumn.Name = "nameColumn";
      this.nameColumn.Width = 60;
      // 
      // codeColumn
      // 
      this.codeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.codeColumn.DataPropertyName = "Code";
      this.codeColumn.HeaderText = "Code";
      this.codeColumn.Name = "codeColumn";
      this.codeColumn.Width = 57;
      // 
      // externalCodeColumn
      // 
      this.externalCodeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.externalCodeColumn.DataPropertyName = "ExternalCode";
      this.externalCodeColumn.HeaderText = "External Code";
      this.externalCodeColumn.Name = "externalCodeColumn";
      this.externalCodeColumn.Width = 98;
      // 
      // monitoringTypeColumn
      // 
      this.monitoringTypeColumn.DataPropertyName = "MonitoringType";
      this.monitoringTypeColumn.HeaderText = "Monitoring type";
      this.monitoringTypeColumn.Name = "monitoringTypeColumn";
      this.monitoringTypeColumn.Visible = false;
      this.monitoringTypeColumn.Width = 104;
      // 
      // displayPriorityColumn
      // 
      this.displayPriorityColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.displayPriorityColumn.DataPropertyName = "DisplayPriority";
      this.displayPriorityColumn.HeaderText = "Display Priority";
      this.displayPriorityColumn.Name = "displayPriorityColumn";
      // 
      // mainMachineModuleColumn
      // 
      this.mainMachineModuleColumn.DataPropertyName = "MainMachineModule";
      this.mainMachineModuleColumn.HeaderText = "MainMachineModule";
      this.mainMachineModuleColumn.Name = "mainMachineModuleColumn";
      this.mainMachineModuleColumn.Width = 131;
      // 
      // mainCncAcquisitionColumn
      // 
      this.mainCncAcquisitionColumn.DataPropertyName = "MainCncAcquisition";
      this.mainCncAcquisitionColumn.HeaderText = "MainCncAcquisition";
      this.mainCncAcquisitionColumn.Name = "mainCncAcquisitionColumn";
      this.mainCncAcquisitionColumn.Visible = false;
      this.mainCncAcquisitionColumn.Width = 125;
      // 
      // performanceFieldColumn
      // 
      this.performanceFieldColumn.DataPropertyName = "PerformanceField";
      this.performanceFieldColumn.HeaderText = "PerformanceField";
      this.performanceFieldColumn.Name = "performanceFieldColumn";
      this.performanceFieldColumn.Width = 114;
      // 
      // operationBarColumn
      // 
      this.operationBarColumn.DataPropertyName = "OperationBar";
      this.operationBarColumn.HeaderText = "OperationBar";
      this.operationBarColumn.Name = "operationBarColumn";
      this.operationBarColumn.Width = 94;
      // 
      // palletChangingDurationColumn
      // 
      this.palletChangingDurationColumn.DataPropertyName = "PalletChangingDuration";
      this.palletChangingDurationColumn.HeaderText = "PalletChangingDuration";
      this.palletChangingDurationColumn.Name = "palletChangingDurationColumn";
      this.palletChangingDurationColumn.Width = 143;
      // 
      // disabledPalletChangingDurationColumn
      // 
      this.disabledPalletChangingDurationColumn.DataPropertyName = "PalletChangingDurationAsString";
      this.disabledPalletChangingDurationColumn.HeaderText = "DisabledPalletChangingDuration";
      this.disabledPalletChangingDurationColumn.Name = "disabledPalletChangingDurationColumn";
      this.disabledPalletChangingDurationColumn.Visible = false;
      this.disabledPalletChangingDurationColumn.Width = 184;
      // 
      // machineModulesColumn
      // 
      this.machineModulesColumn.DataPropertyName = "MachineModules";
      this.machineModulesColumn.HeaderText = "MachineModules";
      this.machineModulesColumn.Name = "machineModulesColumn";
      this.machineModulesColumn.ReadOnly = true;
      this.machineModulesColumn.Visible = false;
      this.machineModulesColumn.Width = 113;
      // 
      // companyColumn
      // 
      this.companyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.companyColumn.DataPropertyName = "Company";
      this.companyColumn.HeaderText = "Company";
      this.companyColumn.Name = "companyColumn";
      this.companyColumn.Visible = false;
      this.companyColumn.Width = 76;
      // 
      // departmentColumn
      // 
      this.departmentColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.departmentColumn.DataPropertyName = "Department";
      this.departmentColumn.HeaderText = "Department";
      this.departmentColumn.Name = "departmentColumn";
      this.departmentColumn.Visible = false;
      this.departmentColumn.Width = 87;
      // 
      // categoryColumn
      // 
      this.categoryColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.categoryColumn.DataPropertyName = "Category";
      this.categoryColumn.HeaderText = "Category";
      this.categoryColumn.Name = "categoryColumn";
      this.categoryColumn.Visible = false;
      this.categoryColumn.Width = 74;
      // 
      // subCategoryColumn
      // 
      this.subCategoryColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.subCategoryColumn.DataPropertyName = "SubCategory";
      this.subCategoryColumn.HeaderText = "Sub-category";
      this.subCategoryColumn.Name = "subCategoryColumn";
      this.subCategoryColumn.Visible = false;
      this.subCategoryColumn.Width = 95;
      // 
      // stampingConfigColumn
      // 
      this.stampingConfigColumn.DataPropertyName = "StampingConfigByName";
      this.stampingConfigColumn.HeaderText = "StampingConfig";
      this.stampingConfigColumn.Name = "stampingConfigColumn";
      this.stampingConfigColumn.Width = 106;
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
      // operationFromCncColumn
      // 
      this.operationFromCncColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.operationFromCncColumn.DataPropertyName = "OperationFromCnc";
      this.operationFromCncColumn.HeaderText = "OperationFromCnc";
      this.operationFromCncColumn.Name = "operationFromCncColumn";
      this.operationFromCncColumn.Width = 101;
      // 
      // MonitoredMachineConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "MonitoredMachineConfig";
      this.Size = new System.Drawing.Size(866, 472);
      this.Load += new System.EventHandler(this.MonitoredMachineConfigLoad);
      this.Validated += new System.EventHandler(this.MonitoredMachineConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn codeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn externalCodeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn monitoringTypeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn displayPriorityColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn mainMachineModuleColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn mainCncAcquisitionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn performanceFieldColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn operationBarColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn palletChangingDurationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn disabledPalletChangingDurationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineModulesColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn companyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn departmentColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn categoryColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn subCategoryColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn stampingConfigColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn operationFromCncColumn;
  }
}
