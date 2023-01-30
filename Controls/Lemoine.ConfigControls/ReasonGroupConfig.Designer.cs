// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class ReasonGroupConfig
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
      this.descriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.descriptionTranslationKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.displayPriorityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.reportColorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.reasonsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.translationKeyColumn,
            this.descriptionColumn,
            this.descriptionTranslationKeyColumn,
            this.displayPriorityColumn,
            this.colorColumn,
            this.reportColorColumn,
            this.reasonsColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(541, 245);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellDoubleClick);
      this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridViewCellFormatting);
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
      // translationKeyColumn
      // 
      this.translationKeyColumn.DataPropertyName = "TranslationKey";
      this.translationKeyColumn.HeaderText = "Translation Key";
      this.translationKeyColumn.Name = "translationKeyColumn";
      this.translationKeyColumn.Width = 96;
      // 
      // descriptionColumn
      // 
      this.descriptionColumn.DataPropertyName = "Description";
      this.descriptionColumn.HeaderText = "Description";
      this.descriptionColumn.Name = "descriptionColumn";
      this.descriptionColumn.Width = 85;
      // 
      // descriptionTranslationKeyColumn
      // 
      this.descriptionTranslationKeyColumn.DataPropertyName = "DescriptionTranslationKey";
      this.descriptionTranslationKeyColumn.HeaderText = "Description Translation Key";
      this.descriptionTranslationKeyColumn.Name = "descriptionTranslationKeyColumn";
      this.descriptionTranslationKeyColumn.Width = 131;
      // 
      // displayPriorityColumn
      // 
      this.displayPriorityColumn.DataPropertyName = "DisplayPriority";
      this.displayPriorityColumn.HeaderText = "Display Priority";
      this.displayPriorityColumn.Name = "displayPriorityColumn";
      this.displayPriorityColumn.Width = 92;
      // 
      // colorColumn
      // 
      this.colorColumn.DataPropertyName = "Color";
      this.colorColumn.HeaderText = "Color";
      this.colorColumn.Name = "colorColumn";
      this.colorColumn.Width = 56;
      // 
      // reportColorColumn
      // 
      this.reportColorColumn.DataPropertyName = "ReportColor";
      this.reportColorColumn.HeaderText = "ReportColor";
      this.reportColorColumn.Name = "reportColorColumn";
      this.reportColorColumn.Width = 88;
      // 
      // reasonsColumn
      // 
      this.reasonsColumn.DataPropertyName = "Reasons";
      this.reasonsColumn.HeaderText = "Reasons";
      this.reasonsColumn.Name = "reasonsColumn";
      this.reasonsColumn.Visible = false;
      this.reasonsColumn.Width = 74;
      // 
      // ReasonGroupConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "ReasonGroupConfig";
      this.Size = new System.Drawing.Size(541, 245);
      this.Load += new System.EventHandler(this.ReasonGroupConfigLoad);
      this.Validated += new System.EventHandler(this.ReasonGroupConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn translationKeyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn descriptionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn descriptionTranslationKeyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn displayPriorityColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn colorColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reportColorColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reasonsColumn;
  }
}
