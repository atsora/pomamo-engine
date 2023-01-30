// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class FieldConfig
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
      this.translationKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.displayColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.codeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.descriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.typeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.unitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.activeColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.stampingDataTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.cncDataAggregationTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.associatedClassColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.associatedPropertyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.averageMinTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.averageMaxDeviationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.customColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
                  this.translationKeyColumn,
                  this.displayColumn,
                  this.codeColumn,
                  this.descriptionColumn,
                  this.typeColumn,
                  this.unitColumn,
                  this.activeColumn,
                  this.stampingDataTypeColumn,
                  this.cncDataAggregationTypeColumn,
                  this.associatedClassColumn,
                  this.associatedPropertyColumn,
                  this.averageMinTimeColumn,
                  this.averageMaxDeviationColumn,
                  this.customColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(541, 245);
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
      // translationKeyColumn
      // 
      this.translationKeyColumn.DataPropertyName = "TranslationKey";
      this.translationKeyColumn.HeaderText = "TranslationKey";
      this.translationKeyColumn.Name = "translationKeyColumn";
      // 
      // displayColumn
      // 
      this.displayColumn.DataPropertyName = "Display";
      this.displayColumn.HeaderText = "Display";
      this.displayColumn.Name = "displayColumn";
      this.displayColumn.ReadOnly = true;
      this.displayColumn.Visible = false;
      // 
      // codeColumn
      // 
      this.codeColumn.DataPropertyName = "Code";
      this.codeColumn.HeaderText = "Code";
      this.codeColumn.Name = "codeColumn";
      // 
      // descriptionColumn
      // 
      this.descriptionColumn.DataPropertyName = "Description";
      this.descriptionColumn.HeaderText = "Description";
      this.descriptionColumn.Name = "descriptionColumn";
      // 
      // typeColumn
      // 
      this.typeColumn.DataPropertyName = "Type";
      this.typeColumn.HeaderText = "FieldType";
      this.typeColumn.Name = "typeColumn";
      // 
      // unitColumn
      // 
      this.unitColumn.DataPropertyName = "Unit";
      this.unitColumn.HeaderText = "Unit";
      this.unitColumn.Name = "unitColumn";
      // 
      // activeColumn
      // 
      this.activeColumn.DataPropertyName = "Active";
      this.activeColumn.HeaderText = "Active";
      this.activeColumn.Name = "activeColumn";
      // 
      // stampingDataTypeColumn
      // 
      this.stampingDataTypeColumn.DataPropertyName = "StampingDataType";
      this.stampingDataTypeColumn.HeaderText = "StampingDataType";
      this.stampingDataTypeColumn.Name = "stampingDataTypeColumn";
      // 
      // cncDataAggregationTypeColumn
      // 
      this.cncDataAggregationTypeColumn.DataPropertyName = "CncDataAggregationType";
      this.cncDataAggregationTypeColumn.HeaderText = "CncDataAggregationType";
      this.cncDataAggregationTypeColumn.Name = "cncDataAggregationTypeColumn";
      // 
      // associatedClassColumn
      // 
      this.associatedClassColumn.DataPropertyName = "AssociatedClass";
      this.associatedClassColumn.HeaderText = "AssociatedClass";
      this.associatedClassColumn.Name = "associatedClassColumn";
      this.associatedClassColumn.ReadOnly = true;
      this.associatedClassColumn.Visible = false;
      // 
      // associatedPropertyColumn
      // 
      this.associatedPropertyColumn.DataPropertyName = "AssociatedProperty";
      this.associatedPropertyColumn.HeaderText = "AssociatedProperty";
      this.associatedPropertyColumn.Name = "associatedPropertyColumn";
      this.associatedPropertyColumn.ReadOnly = true;
      this.associatedPropertyColumn.Visible = false;
      // 
      // averageMinTimeColumn
      // 
      this.averageMinTimeColumn.DataPropertyName = "AverageMinTime";
      this.averageMinTimeColumn.HeaderText = "AverageMinTime";
      this.averageMinTimeColumn.Name = "averageMinTimeColumn";
      // 
      // averageMaxDeviationColumn
      // 
      this.averageMaxDeviationColumn.DataPropertyName = "AverageMaxDeviation";
      this.averageMaxDeviationColumn.HeaderText = "AverageMaxDeviation";
      this.averageMaxDeviationColumn.Name = "averageMaxDeviationColumn";
      // 
      // customColumn
      // 
      this.customColumn.DataPropertyName = "Custom";
      this.customColumn.HeaderText = "Custom";
      this.customColumn.Name = "customColumn";
      this.customColumn.ReadOnly = true;
      this.customColumn.Visible = false;
      // 
      // FieldConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "FieldConfig";
      this.Size = new System.Drawing.Size(541, 245);
      this.Load += new System.EventHandler(this.FieldConfigLoad);
      this.Validated += new System.EventHandler(this.FieldConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewCheckBoxColumn activeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn displayColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn customColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn averageMaxDeviationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn averageMinTimeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn associatedPropertyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn associatedClassColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn cncDataAggregationTypeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn stampingDataTypeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn unitColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn typeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn descriptionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn codeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn translationKeyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
  }
}
