// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class ReasonConfig
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
      this.nameOrTranslationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.codeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.descriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.descriptionTranslationKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.descriptionOrTranslationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.displayPriorityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.reportColorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.customColorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.customReportColorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.linkOperationDirectionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.reasonGroupColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.nameOrTranslationColumn,
            this.codeColumn,
            this.descriptionColumn,
            this.descriptionTranslationKeyColumn,
            this.descriptionOrTranslationColumn,
            this.displayPriorityColumn,
            this.colorColumn,
            this.reportColorColumn,
            this.customColorColumn,
            this.customReportColorColumn,
            this.linkOperationDirectionColumn,
            this.reasonGroupColumn});
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
      this.idColumn.FillWeight = 82.23354F;
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
      this.nameColumn.FillWeight = 102.2208F;
      this.nameColumn.HeaderText = "Name";
      this.nameColumn.Name = "nameColumn";
      this.nameColumn.Width = 60;
      // 
      // translationKeyColumn
      // 
      this.translationKeyColumn.DataPropertyName = "TranslationKey";
      this.translationKeyColumn.FillWeight = 102.2208F;
      this.translationKeyColumn.HeaderText = "Translation Key";
      this.translationKeyColumn.Name = "translationKeyColumn";
      this.translationKeyColumn.Width = 96;
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
      // codeColumn
      // 
      this.codeColumn.DataPropertyName = "Code";
      this.codeColumn.FillWeight = 102.2208F;
      this.codeColumn.HeaderText = "Code";
      this.codeColumn.Name = "codeColumn";
      this.codeColumn.Width = 57;
      // 
      // descriptionColumn
      // 
      this.descriptionColumn.DataPropertyName = "Description";
      this.descriptionColumn.FillWeight = 102.2208F;
      this.descriptionColumn.HeaderText = "Description";
      this.descriptionColumn.Name = "descriptionColumn";
      this.descriptionColumn.Width = 85;
      // 
      // descriptionTranslationKeyColumn
      // 
      this.descriptionTranslationKeyColumn.DataPropertyName = "DescriptionTranslationKey";
      this.descriptionTranslationKeyColumn.FillWeight = 102.2208F;
      this.descriptionTranslationKeyColumn.HeaderText = "DescriptionTranslationKey";
      this.descriptionTranslationKeyColumn.Name = "descriptionTranslationKeyColumn";
      this.descriptionTranslationKeyColumn.Width = 155;
      // 
      // descriptionOrTranslationColumn
      // 
      this.descriptionOrTranslationColumn.DataPropertyName = "DescriptionOrTranslation";
      this.descriptionOrTranslationColumn.HeaderText = "DescriptionOrTranslation";
      this.descriptionOrTranslationColumn.Name = "descriptionOrTranslationColumn";
      this.descriptionOrTranslationColumn.ReadOnly = true;
      this.descriptionOrTranslationColumn.Visible = false;
      this.descriptionOrTranslationColumn.Width = 148;
      // 
      // displayPriorityColumn
      // 
      this.displayPriorityColumn.DataPropertyName = "DisplayPriority";
      this.displayPriorityColumn.HeaderText = "Display priority";
      this.displayPriorityColumn.Name = "displayPriorityColumn";
      this.displayPriorityColumn.Width = 91;
      // 
      // colorColumn
      // 
      this.colorColumn.DataPropertyName = "Color";
      this.colorColumn.FillWeight = 102.2208F;
      this.colorColumn.HeaderText = "Color";
      this.colorColumn.Name = "colorColumn";
      this.colorColumn.ReadOnly = true;
      this.colorColumn.Visible = false;
      this.colorColumn.Width = 56;
      // 
      // reportColorColumn
      // 
      this.reportColorColumn.DataPropertyName = "ReportColor";
      this.reportColorColumn.HeaderText = "ReportColor";
      this.reportColorColumn.Name = "reportColorColumn";
      this.reportColorColumn.ReadOnly = true;
      this.reportColorColumn.Visible = false;
      this.reportColorColumn.Width = 88;
      // 
      // customColorColumn
      // 
      this.customColorColumn.DataPropertyName = "CustomColor";
      this.customColorColumn.HeaderText = "CustomColor";
      this.customColorColumn.Name = "customColorColumn";
      this.customColorColumn.Width = 91;
      // 
      // customReportColorColumn
      // 
      this.customReportColorColumn.DataPropertyName = "CustomReportColor";
      this.customReportColorColumn.HeaderText = "CustomReportColor";
      this.customReportColorColumn.Name = "customReportColorColumn";
      this.customReportColorColumn.Width = 123;
      // 
      // linkOperationDirectionColumn
      // 
      this.linkOperationDirectionColumn.DataPropertyName = "LinkOperationDirection";
      this.linkOperationDirectionColumn.FillWeight = 102.2208F;
      this.linkOperationDirectionColumn.HeaderText = "Link Operation Direction";
      this.linkOperationDirectionColumn.Name = "linkOperationDirectionColumn";
      this.linkOperationDirectionColumn.Width = 133;
      // 
      // reasonGroupColumn
      // 
      this.reasonGroupColumn.DataPropertyName = "ReasonGroup";
      this.reasonGroupColumn.FillWeight = 102.2208F;
      this.reasonGroupColumn.HeaderText = "Reason Group";
      this.reasonGroupColumn.Name = "reasonGroupColumn";
      this.reasonGroupColumn.Width = 93;
      // 
      // ReasonConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "ReasonConfig";
      this.Size = new System.Drawing.Size(541, 245);
      this.Load += new System.EventHandler(this.ReasonConfigLoad);
      this.Validated += new System.EventHandler(this.ReasonConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn translationKeyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameOrTranslationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn codeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn descriptionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn descriptionTranslationKeyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn descriptionOrTranslationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn displayPriorityColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn colorColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reportColorColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn customColorColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn customReportColorColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn linkOperationDirectionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reasonGroupColumn;
  }
}
