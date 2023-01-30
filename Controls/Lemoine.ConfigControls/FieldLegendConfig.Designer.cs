// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class FieldLegendConfig
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldLegendConfig));
      this.dataGridView = new Lemoine.BaseControls.SortableDataGridView();
      this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.versionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.stringValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.minValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.maxValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.textColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.fieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.rangeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.fieldSelection1 = new Lemoine.DataReferenceControls.FieldSelection();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
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
                                           this.stringValueColumn,
                                           this.minValueColumn,
                                           this.maxValueColumn,
                                           this.textColumn,
                                           this.colorColumn,
                                           this.fieldColumn,
                                           this.rangeColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(396, 245);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellDoubleClick);
      this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridViewCellFormatting);
      this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
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
      // stringValueColumn
      // 
      this.stringValueColumn.DataPropertyName = "StringValue";
      this.stringValueColumn.HeaderText = "StringValue";
      this.stringValueColumn.Name = "stringValueColumn";
      // 
      // minValueColumn
      // 
      this.minValueColumn.DataPropertyName = "MinValue";
      this.minValueColumn.HeaderText = "MinValue";
      this.minValueColumn.Name = "minValueColumn";
      // 
      // maxValueColumn
      // 
      this.maxValueColumn.DataPropertyName = "MaxValue";
      this.maxValueColumn.HeaderText = "MaxValue";
      this.maxValueColumn.Name = "maxValueColumn";
      // 
      // textColumn
      // 
      this.textColumn.DataPropertyName = "Text";
      this.textColumn.HeaderText = "Text";
      this.textColumn.Name = "textColumn";
      // 
      // colorColumn
      // 
      this.colorColumn.DataPropertyName = "Color";
      this.colorColumn.HeaderText = "Color";
      this.colorColumn.Name = "colorColumn";
      // 
      // fieldColumn
      // 
      this.fieldColumn.DataPropertyName = "Field";
      this.fieldColumn.HeaderText = "Field";
      this.fieldColumn.Name = "fieldColumn";
      this.fieldColumn.Visible = false;
      // 
      // rangeColumn
      // 
      this.rangeColumn.DataPropertyName = "Range";
      this.rangeColumn.HeaderText = "Range";
      this.rangeColumn.Name = "RangeColumn";
      this.rangeColumn.ReadOnly = true;
      this.rangeColumn.Visible = false;
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.fieldSelection1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.dataGridView);
      this.splitContainer1.Size = new System.Drawing.Size(541, 245);
      this.splitContainer1.SplitterDistance = 141;
      this.splitContainer1.TabIndex = 2;
      // 
      // fieldSelection1
      // 
      this.fieldSelection1.DisplayedProperty = "SelectionText";
      this.fieldSelection1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fieldSelection1.Location = new System.Drawing.Point(0, 0);
      this.fieldSelection1.Name = "fieldSelection1";
      this.fieldSelection1.SelectedField = null;
      this.fieldSelection1.SelectedFields = ((System.Collections.Generic.IList<Lemoine.Model.IField>)(resources.GetObject("fieldSelection1.SelectedFields")));
      this.fieldSelection1.Size = new System.Drawing.Size(141, 245);
      this.fieldSelection1.TabIndex = 0;
      this.fieldSelection1.AfterSelect += new System.EventHandler(this.FieldSelection1AfterSelect);
      // 
      // FieldLegendConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "FieldLegendConfig";
      this.Size = new System.Drawing.Size(541, 245);
      this.Load += new System.EventHandler(this.FieldLegendConfigLoad);
      this.Validated += new System.EventHandler(this.FieldLegendConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewTextBoxColumn rangeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn fieldColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn colorColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn textColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn maxValueColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn minValueColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn stringValueColumn;
    private Lemoine.DataReferenceControls.FieldSelection fieldSelection1;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
  }
}
