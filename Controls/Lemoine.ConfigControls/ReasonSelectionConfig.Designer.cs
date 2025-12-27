// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class ReasonSelectionConfig
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.machineModeSelection1 = new Lemoine.DataReferenceControls.MachineModeSelection();
      this.machineObservationStateSelection1 = new Lemoine.DataReferenceControls.MachineObservationStateSelection();
      this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.versionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineModeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineObservationStateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.reasonColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.selectableColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.detailsRequiredColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.noDetailsColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn ();
      this.machineFilterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.setAllOverColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
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
                  this.machineModeColumn,
                  this.machineObservationStateColumn,
                  this.reasonColumn,
                  this.selectableColumn,
                  this.detailsRequiredColumn,
                  this.noDetailsColumn,
                  this.machineFilterColumn,
                  this.setAllOverColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(357, 245);
      this.dataGridView.TabIndex = 0;
      this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridViewCellFormatting);
      this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.dataGridView.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.DataGridViewUserAddedRow);
      this.dataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.dataGridView);
      this.splitContainer1.Size = new System.Drawing.Size(541, 245);
      this.splitContainer1.SplitterDistance = 180;
      this.splitContainer1.TabIndex = 1;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.machineModeSelection1);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.machineObservationStateSelection1);
      this.splitContainer2.Size = new System.Drawing.Size(180, 245);
      this.splitContainer2.SplitterDistance = 85;
      this.splitContainer2.TabIndex = 0;
      // 
      // machineModeSelection1
      // 
      this.machineModeSelection1.DisplayedProperty = "SelectionText";
      this.machineModeSelection1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModeSelection1.Location = new System.Drawing.Point(0, 0);
      this.machineModeSelection1.MultiSelect = true;
      this.machineModeSelection1.Name = "machineModeSelection1";
      this.machineModeSelection1.SelectedMachineMode = null;
      this.machineModeSelection1.Size = new System.Drawing.Size(85, 245);
      this.machineModeSelection1.TabIndex = 0;
      this.machineModeSelection1.AfterSelect += new System.EventHandler(this.MachineModeSelection1AfterSelect);
      // 
      // machineObservationStateSelection1
      // 
      this.machineObservationStateSelection1.DisplayedProperty = "SelectionText";
      this.machineObservationStateSelection1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineObservationStateSelection1.Location = new System.Drawing.Point(0, 0);
      this.machineObservationStateSelection1.MultiSelect = true;
      this.machineObservationStateSelection1.Name = "machineObservationStateSelection1";
      this.machineObservationStateSelection1.SelectedMachineObservationState = null;
      this.machineObservationStateSelection1.Size = new System.Drawing.Size(91, 245);
      this.machineObservationStateSelection1.TabIndex = 0;
      this.machineObservationStateSelection1.AfterSelect += new System.EventHandler(this.MachineObservationStateSelection1AfterSelect);
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
      // machineModeColumn
      // 
      this.machineModeColumn.DataPropertyName = "MachineMode";
      this.machineModeColumn.HeaderText = "MachineMode";
      this.machineModeColumn.Name = "machineModeColumn";
      this.machineModeColumn.Visible = false;
      // 
      // machineObservationStateColumn
      // 
      this.machineObservationStateColumn.DataPropertyName = "MachineObservationState";
      this.machineObservationStateColumn.HeaderText = "MachineObservationState";
      this.machineObservationStateColumn.Name = "machineObservationStateColumn";
      this.machineObservationStateColumn.Visible = false;
      this.machineObservationStateColumn.Width = 155;
      // 
      // reasonColumn
      // 
      this.reasonColumn.DataPropertyName = "Reason";
      this.reasonColumn.HeaderText = "Reason";
      this.reasonColumn.Name = "reasonColumn";
      this.reasonColumn.Width = 69;
      // 
      // selectableColumn
      // 
      this.selectableColumn.DataPropertyName = "Selectable";
      this.selectableColumn.HeaderText = "Selectable";
      this.selectableColumn.Name = "selectableColumn";
      this.selectableColumn.Width = 63;
      // 
      // detailsRequiredColumn
      // 
      this.detailsRequiredColumn.DataPropertyName = "DetailsRequired";
      this.detailsRequiredColumn.HeaderText = "DetailsRequired";
      this.detailsRequiredColumn.Name = "detailsRequiredColumn";
      this.detailsRequiredColumn.Width = 88;
      // 
      // detailsRequiredColumn
      // 
      this.detailsRequiredColumn.DataPropertyName = "NoDetails";
      this.detailsRequiredColumn.HeaderText = "No detail";
      this.detailsRequiredColumn.Name = "noDetailsColumn";
      this.detailsRequiredColumn.Width = 68;
      // 
      // machineFilterColumn
      // 
      this.machineFilterColumn.DataPropertyName = "MachineFilter";
      this.machineFilterColumn.HeaderText = "MachineFilter";
      this.machineFilterColumn.Name = "machineFilterColumn";
      this.machineFilterColumn.Width = 95;
      // 
      // setAllOverColumn
      // 
      this.setAllOverColumn.HeaderText = "SetAllOver";
      this.setAllOverColumn.Name = "setAllOverColumn";
      this.setAllOverColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.setAllOverColumn.Width = 63;
      // 
      // ReasonSelectionConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "ReasonSelectionConfig";
      this.Size = new System.Drawing.Size(541, 245);
      this.Load += new System.EventHandler(this.ReasonSelectionConfigLoad);
      this.Validated += new System.EventHandler(this.ReasonSelectionConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridViewCheckBoxColumn setAllOverColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineFilterColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn detailsRequiredColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn noDetailsColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn selectableColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn reasonColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineObservationStateColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineModeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.DataReferenceControls.MachineObservationStateSelection machineObservationStateSelection1;
    private Lemoine.DataReferenceControls.MachineModeSelection machineModeSelection1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
  }
}
