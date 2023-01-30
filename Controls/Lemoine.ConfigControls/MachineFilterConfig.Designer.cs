// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MachineFilterConfig
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.dataGridViewFilters = new System.Windows.Forms.DataGridView();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.machineFilterItemDataGridView = new System.Windows.Forms.DataGridView();
      this.machineFilterItemIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineFilterItemRuleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineFilterItemDescriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineFilterItemOrderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.itemFilterAddButton = new System.Windows.Forms.Button();
      this.idMachineFilterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.versionMachineVersionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.nameMachineFilterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.initialSetColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFilters)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.machineFilterItemDataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.dataGridViewFilters);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Size = new System.Drawing.Size(712, 372);
      this.splitContainer1.SplitterDistance = 244;
      this.splitContainer1.TabIndex = 0;
      // 
      // dataGridViewFilters
      // 
      this.dataGridViewFilters.AllowUserToResizeRows = false;
      this.dataGridViewFilters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.dataGridViewFilters.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.dataGridViewFilters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridViewFilters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idMachineFilterColumn,
            this.versionMachineVersionColumn,
            this.nameMachineFilterColumn,
            this.initialSetColumn});
      this.dataGridViewFilters.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridViewFilters.Location = new System.Drawing.Point(0, 0);
      this.dataGridViewFilters.MultiSelect = false;
      this.dataGridViewFilters.Name = "dataGridViewFilters";
      this.dataGridViewFilters.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridViewFilters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewFilters.Size = new System.Drawing.Size(244, 372);
      this.dataGridViewFilters.TabIndex = 0;
      this.dataGridViewFilters.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.dataGridViewFilters.SelectionChanged += new System.EventHandler(this.DataGridViewSelectionChanged);
      this.dataGridViewFilters.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.DataGridViewUserAddedRow);
      this.dataGridViewFilters.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      this.dataGridViewFilters.Validated += new System.EventHandler(this.MachineFilterConfigValidated);
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.machineFilterItemDataGridView);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.itemFilterAddButton);
      this.splitContainer2.Size = new System.Drawing.Size(464, 372);
      this.splitContainer2.SplitterDistance = 343;
      this.splitContainer2.TabIndex = 1;
      // 
      // machineFilterItemDataGridView
      // 
      this.machineFilterItemDataGridView.AllowUserToAddRows = false;
      this.machineFilterItemDataGridView.AllowUserToResizeRows = false;
      this.machineFilterItemDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.machineFilterItemDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.machineFilterItemIdColumn,
            this.machineFilterItemRuleColumn,
            this.machineFilterItemDescriptionColumn,
            this.machineFilterItemOrderColumn});
      this.machineFilterItemDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineFilterItemDataGridView.Location = new System.Drawing.Point(0, 0);
      this.machineFilterItemDataGridView.MultiSelect = false;
      this.machineFilterItemDataGridView.Name = "machineFilterItemDataGridView";
      this.machineFilterItemDataGridView.ReadOnly = true;
      this.machineFilterItemDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.machineFilterItemDataGridView.Size = new System.Drawing.Size(464, 343);
      this.machineFilterItemDataGridView.TabIndex = 0;
      this.machineFilterItemDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.MachineFilterItemDataGridViewCellFormatting);
      this.machineFilterItemDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.MachineFilterItemDataGridViewUserDeletingRow);
      // 
      // machineFilterItemIdColumn
      // 
      this.machineFilterItemIdColumn.DataPropertyName = "Id";
      this.machineFilterItemIdColumn.HeaderText = "Id";
      this.machineFilterItemIdColumn.Name = "machineFilterItemIdColumn";
      this.machineFilterItemIdColumn.ReadOnly = true;
      this.machineFilterItemIdColumn.Visible = false;
      // 
      // machineFilterItemRuleColumn
      // 
      this.machineFilterItemRuleColumn.DataPropertyName = "Rule";
      this.machineFilterItemRuleColumn.HeaderText = "Rule";
      this.machineFilterItemRuleColumn.Name = "machineFilterItemRuleColumn";
      this.machineFilterItemRuleColumn.ReadOnly = true;
      // 
      // machineFilterItemDescriptionColumn
      // 
      this.machineFilterItemDescriptionColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
      this.machineFilterItemDescriptionColumn.HeaderText = "Description";
      this.machineFilterItemDescriptionColumn.Name = "machineFilterItemDescriptionColumn";
      this.machineFilterItemDescriptionColumn.ReadOnly = true;
      this.machineFilterItemDescriptionColumn.Width = 85;
      // 
      // machineFilterItemOrderColumn
      // 
      this.machineFilterItemOrderColumn.DataPropertyName = "Order";
      this.machineFilterItemOrderColumn.HeaderText = "Order";
      this.machineFilterItemOrderColumn.Name = "machineFilterItemOrderColumn";
      this.machineFilterItemOrderColumn.ReadOnly = true;
      this.machineFilterItemOrderColumn.Visible = false;
      // 
      // itemFilterAddButton
      // 
      this.itemFilterAddButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.itemFilterAddButton.Location = new System.Drawing.Point(0, 0);
      this.itemFilterAddButton.Name = "itemFilterAddButton";
      this.itemFilterAddButton.Size = new System.Drawing.Size(75, 25);
      this.itemFilterAddButton.TabIndex = 0;
      this.itemFilterAddButton.Text = "button1";
      this.itemFilterAddButton.UseVisualStyleBackColor = true;
      this.itemFilterAddButton.Click += new System.EventHandler(this.ItemFilterAddButtonClick);
      // 
      // idMachineFilterColumn
      // 
      this.idMachineFilterColumn.DataPropertyName = "Id";
      this.idMachineFilterColumn.HeaderText = "Id";
      this.idMachineFilterColumn.Name = "idMachineFilterColumn";
      this.idMachineFilterColumn.Width = 41;
      // 
      // versionMachineVersionColumn
      // 
      this.versionMachineVersionColumn.DataPropertyName = "Version";
      this.versionMachineVersionColumn.HeaderText = "Version";
      this.versionMachineVersionColumn.Name = "versionMachineVersionColumn";
      this.versionMachineVersionColumn.Visible = false;
      this.versionMachineVersionColumn.Width = 67;
      // 
      // nameMachineFilterColumn
      // 
      this.nameMachineFilterColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
      this.nameMachineFilterColumn.DataPropertyName = "Name";
      this.nameMachineFilterColumn.HeaderText = "Name";
      this.nameMachineFilterColumn.Name = "nameMachineFilterColumn";
      this.nameMachineFilterColumn.Width = 60;
      // 
      // initialSetColumn
      // 
      this.initialSetColumn.DataPropertyName = "Initialset";
      this.initialSetColumn.HeaderText = "InitialSet";
      this.initialSetColumn.Name = "initialSetColumn";
      this.initialSetColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.initialSetColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.initialSetColumn.Width = 72;
      // 
      // MachineFilterConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "MachineFilterConfig";
      this.Size = new System.Drawing.Size(712, 372);
      this.Load += new System.EventHandler(this.MachineFilterConfigLoad);
      this.Validated += new System.EventHandler(this.MachineFilterConfigValidated);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFilters)).EndInit();
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.machineFilterItemDataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.DataGridView machineFilterItemDataGridView;
    private System.Windows.Forms.Button itemFilterAddButton;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.DataGridView dataGridViewFilters;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineFilterItemIdColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineFilterItemRuleColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineFilterItemDescriptionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineFilterItemOrderColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn idMachineFilterColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn versionMachineVersionColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameMachineFilterColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn initialSetColumn;
    }
}
