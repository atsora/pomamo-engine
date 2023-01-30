// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class ComputerConfig
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
      this.addressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.isLctrColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.isLpstColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.isCncColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.isWebColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.isAlertColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.isAutoReasonColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.isSynchronizationColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.webServiceUrlColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.versionColumn,
            this.nameColumn,
            this.addressColumn,
            this.isLctrColumn,
            this.isLpstColumn,
            this.isCncColumn,
            this.isWebColumn,
            this.isAlertColumn,
            this.isAutoReasonColumn,
            this.isSynchronizationColumn,
            this.webServiceUrlColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(742, 245);
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
      this.idColumn.Visible = false;
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
      // nameColumn
      // 
      this.nameColumn.DataPropertyName = "Name";
      this.nameColumn.HeaderText = "Name";
      this.nameColumn.Name = "nameColumn";
      this.nameColumn.Width = 60;
      // 
      // addressColumn
      // 
      this.addressColumn.DataPropertyName = "Address";
      this.addressColumn.HeaderText = "addressColumn";
      this.addressColumn.Name = "addressColumn";
      this.addressColumn.Width = 104;
      // 
      // isLctrColumn
      // 
      this.isLctrColumn.DataPropertyName = "IsLctr";
      this.isLctrColumn.HeaderText = "isLctr";
      this.isLctrColumn.Name = "isLctrColumn";
      this.isLctrColumn.Width = 38;
      // 
      // isLpstColumn
      // 
      this.isLpstColumn.DataPropertyName = "IsLpst";
      this.isLpstColumn.HeaderText = "isLpst";
      this.isLpstColumn.Name = "isLpstColumn";
      this.isLpstColumn.Width = 40;
      // 
      // isCncColumn
      // 
      this.isCncColumn.DataPropertyName = "IsCnc";
      this.isCncColumn.HeaderText = "isCnc";
      this.isCncColumn.Name = "isCncColumn";
      this.isCncColumn.Width = 39;
      // 
      // isWebColumn
      // 
      this.isWebColumn.DataPropertyName = "IsWeb";
      this.isWebColumn.HeaderText = "isWeb";
      this.isWebColumn.Name = "isWebColumn";
      this.isWebColumn.Width = 43;
      // 
      // isAlertColumn
      // 
      this.isAlertColumn.DataPropertyName = "IsAlert";
      this.isAlertColumn.HeaderText = "isAlert";
      this.isAlertColumn.Name = "isAlertColumn";
      this.isAlertColumn.Width = 41;
      // 
      // isAutoReasonColumn
      // 
      this.isAutoReasonColumn.DataPropertyName = "IsAutoReason";
      this.isAutoReasonColumn.HeaderText = "isAutoReason";
      this.isAutoReasonColumn.Name = "isAutoReasonColumn";
      this.isAutoReasonColumn.Width = 79;
      // 
      // isSynchronizationColumn
      // 
      this.isSynchronizationColumn.DataPropertyName = "IsSynchronization";
      this.isSynchronizationColumn.HeaderText = "isSynchronization";
      this.isSynchronizationColumn.Name = "isSynchronizationColumn";
      this.isSynchronizationColumn.Width = 95;
      // 
      // webServiceUrlColumn
      // 
      this.webServiceUrlColumn.DataPropertyName = "WebServiceUrl";
      this.webServiceUrlColumn.HeaderText = "Web service url";
      this.webServiceUrlColumn.Name = "webServiceUrlColumn";
      this.webServiceUrlColumn.Width = 87;
      // 
      // ComputerConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "ComputerConfig";
      this.Size = new System.Drawing.Size(742, 245);
      this.Load += new System.EventHandler(this.ComputerConfigLoad);
      this.Validated += new System.EventHandler(this.ComputerConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn addressColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isLctrColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isLpstColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isCncColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isWebColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isAlertColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isAutoReasonColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isSynchronizationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn webServiceUrlColumn;
  }
}
