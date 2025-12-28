// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MachineModeConfig
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Disposes resources used by the control.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing) {
        if (components != null) {
          components.Dispose ();
        }
      }
      base.Dispose (disposing);
    }

    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent ()
    {
      this.dataGridView = new Lemoine.BaseControls.SortableDataGridView ();
      this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.versionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.translationKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.translationKeyOrNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.parentColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.runningColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn ();
      this.autoColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn ();
      this.manualColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn ();
      this.autoSequenceColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn ();
      this.colorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.machineModeCategoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.machineCostColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      this.nameOrTranslationKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn ();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit ();
      this.SuspendLayout ();
      // 
      // dataGridView
      // 
      this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView.Columns.AddRange (new System.Windows.Forms.DataGridViewColumn[] {
            this.idColumn,
            this.versionColumn,
            this.nameColumn,
            this.translationKeyColumn,
            this.translationKeyOrNameColumn,
            this.parentColumn,
            this.runningColumn,
            this.autoColumn,
            this.manualColumn,
            this.autoSequenceColumn,
            this.colorColumn,
            this.machineModeCategoryColumn,
            this.machineCostColumn,
            this.nameOrTranslationKeyColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point (0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size (541, 245);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler (this.DataGridViewCellDoubleClick);
      this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler (this.DataGridViewCellFormatting);
      this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler (this.DataGridViewCellValueChanged);
      this.dataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler (this.DataGridViewUserDeletingRow);
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
      // translationKeyOrNameColumn
      // 
      this.translationKeyOrNameColumn.DataPropertyName = "TranslationKeyOrName";
      this.translationKeyOrNameColumn.HeaderText = "TranslationKeyOrName";
      this.translationKeyOrNameColumn.Name = "translationKeyOrNameColumn";
      this.translationKeyOrNameColumn.ReadOnly = true;
      this.translationKeyOrNameColumn.Visible = false;
      // 
      // Parent
      // 
      this.parentColumn.DataPropertyName = "Parent";
      this.parentColumn.HeaderText = "Parent";
      this.parentColumn.Name = "Parent";
      // 
      // runningColumn
      // 
      this.runningColumn.DataPropertyName = "Running";
      this.runningColumn.HeaderText = "Running";
      this.runningColumn.Name = "runningColumn";
      this.runningColumn.ThreeState = true;
      // 
      // autoColumn
      // 
      this.autoColumn.DataPropertyName = "Auto";
      this.autoColumn.HeaderText = "Auto";
      this.autoColumn.Name = "autoColumn";
      this.autoColumn.ThreeState = true;
      // 
      // manualColumn
      // 
      this.manualColumn.DataPropertyName = "Manual";
      this.manualColumn.HeaderText = "Manual";
      this.manualColumn.Name = "manualColumn";
      this.manualColumn.ThreeState = true;
      // 
      // autoSequenceColumn
      // 
      this.autoSequenceColumn.DataPropertyName = "AutoSequence";
      this.autoSequenceColumn.HeaderText = "AutoSequence";
      this.autoSequenceColumn.Name = "autoSequenceColumn";
      // 
      // colorColumn
      // 
      this.colorColumn.DataPropertyName = "Color";
      this.colorColumn.HeaderText = "Color";
      this.colorColumn.Name = "colorColumn";
      // 
      // machineModeCategoryColumn
      // 
      this.machineModeCategoryColumn.DataPropertyName = "MachineModeCategory";
      this.machineModeCategoryColumn.HeaderText = "MachineModeCategory";
      this.machineModeCategoryColumn.Name = "machineModeCategoryColumn";
      // 
      // machineCostColumn
      // 
      this.machineCostColumn.DataPropertyName = "MachineCost";
      this.machineCostColumn.HeaderText = "MachineCost";
      this.machineCostColumn.Name = "machineCostColumn";
      // 
      // nameOrTranslationKeyColumn
      // 
      this.nameOrTranslationKeyColumn.DataPropertyName = "NameOrTranslation";
      this.nameOrTranslationKeyColumn.HeaderText = "NameOrTranslation";
      this.nameOrTranslationKeyColumn.Name = "nameOrTranslationKeyColumn";
      this.nameOrTranslationKeyColumn.ReadOnly = true;
      this.nameOrTranslationKeyColumn.Visible = false;
      // 
      // MachineModeConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add (this.dataGridView);
      this.Name = "MachineModeConfig";
      this.Size = new System.Drawing.Size (541, 245);
      this.Load += new System.EventHandler (this.MachineModeConfigLoad);
      this.Validated += new System.EventHandler (this.MachineModeConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit ();
      this.ResumeLayout (false);

    }
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn parentColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn translationKeyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn translationKeyOrNameColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn runningColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn autoColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn manualColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn autoSequenceColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn colorColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineModeCategoryColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineCostColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameOrTranslationKeyColumn;
  }
}
