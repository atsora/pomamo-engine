// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MachineObservationStateConfig
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
      this.translationKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.userRequiredColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.shiftRequiredColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.onSiteColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.siteAttendanceChangeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.versionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.nameOrTranslationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.linkOperationDirectionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.isProductionColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.isSetupColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.laborCostColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
                  this.translationKeyColumn,
                  this.userRequiredColumn,
                  this.shiftRequiredColumn,
                  this.onSiteColumn,
                  this.siteAttendanceChangeColumn,
                  this.versionColumn,
                  this.nameOrTranslationColumn,
                  this.linkOperationDirectionColumn,
                  this.isProductionColumn,
                  this.isSetupColumn,
                  this.laborCostColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(541, 245);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellDoubleClick);
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
      this.idColumn.Width = 41;
      // 
      // nameColumn
      // 
      this.nameColumn.DataPropertyName = "Name";
      this.nameColumn.HeaderText = "Name";
      this.nameColumn.Name = "nameColumn";
      this.nameColumn.Width = 60;
      // 
      // translationKeyColumn
      // 
      this.translationKeyColumn.DataPropertyName = "TranslationKey";
      this.translationKeyColumn.HeaderText = "TranslationKey";
      this.translationKeyColumn.Name = "translationKeyColumn";
      this.translationKeyColumn.Width = 102;
      // 
      // userRequiredColumn
      // 
      this.userRequiredColumn.DataPropertyName = "UserRequired";
      this.userRequiredColumn.HeaderText = "UserRequired";
      this.userRequiredColumn.Name = "userRequiredColumn";
      this.userRequiredColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.userRequiredColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.userRequiredColumn.Width = 97;
      // 
      // shiftRequiredColumn
      // 
      this.shiftRequiredColumn.DataPropertyName = "ShiftRequired";
      this.shiftRequiredColumn.HeaderText = "ShiftRequired";
      this.shiftRequiredColumn.Name = "shiftRequiredColumn";
      this.shiftRequiredColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.shiftRequiredColumn.Width = 96;
      // 
      // onSiteColumn
      // 
      this.onSiteColumn.DataPropertyName = "OnSite";
      this.onSiteColumn.HeaderText = "OnSite";
      this.onSiteColumn.Name = "onSiteColumn";
      this.onSiteColumn.Width = 64;
      // 
      // siteAttendanceChangeColumn
      // 
      this.siteAttendanceChangeColumn.DataPropertyName = "SiteAttendanceChange";
      this.siteAttendanceChangeColumn.HeaderText = "SiteAttendanceChange";
      this.siteAttendanceChangeColumn.Name = "siteAttendanceChangeColumn";
      this.siteAttendanceChangeColumn.Width = 142;
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
      // nameOrTranslationColumn
      // 
      this.nameOrTranslationColumn.DataPropertyName = "NameOrTranslation";
      this.nameOrTranslationColumn.HeaderText = "NameOrTranslation";
      this.nameOrTranslationColumn.Name = "nameOrTranslationColumn";
      this.nameOrTranslationColumn.ReadOnly = true;
      this.nameOrTranslationColumn.Visible = false;
      this.nameOrTranslationColumn.Width = 123;
      // 
      // linkOperationDirectionColumn
      // 
      this.linkOperationDirectionColumn.DataPropertyName = "LinkOperationDirection";
      this.linkOperationDirectionColumn.FillWeight = 102.2208F;
      this.linkOperationDirectionColumn.HeaderText = "Link Operation Direction";
      this.linkOperationDirectionColumn.Name = "linkOperationDirectionColumn";
      this.linkOperationDirectionColumn.Width = 133;
      // 
      // isProductionColumn
      // 
      this.isProductionColumn.DataPropertyName = "IsProduction";
      this.isProductionColumn.HeaderText = "IsProduction";
      this.isProductionColumn.Name = "isProductionColumn";
      this.isProductionColumn.Width = 72;
      // 
      // isSetupColumn
      // 
      this.isSetupColumn.DataPropertyName = "IsSetup";
      this.isSetupColumn.HeaderText = "IsSetup";
      this.isSetupColumn.Name = "isSetupColumn";
      this.isSetupColumn.Width = 70;
      // 
      // laborCostColumn
      // 
      this.laborCostColumn.DataPropertyName = "LaborCost";
      this.laborCostColumn.HeaderText = "LaborCost";
      this.laborCostColumn.Name = "laborCostColumn";
      this.laborCostColumn.Width = 82;
      // 
      // MachineObservationStateConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "MachineObservationStateConfig";
      this.Size = new System.Drawing.Size(541, 245);
      this.Load += new System.EventHandler(this.MachineObservationStateConfigLoad);
      this.Validated += new System.EventHandler(this.MachineObservationStateConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewCheckBoxColumn isProductionColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isSetupColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn laborCostColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn shiftRequiredColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameOrTranslationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn siteAttendanceChangeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn onSiteColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn userRequiredColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn linkOperationDirectionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn translationKeyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
  }
}
