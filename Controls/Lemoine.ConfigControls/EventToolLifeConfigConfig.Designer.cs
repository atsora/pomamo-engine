// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class EventToolLifeConfigConfig
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
      this.eventToolLifeConfigDataGridView = new Lemoine.BaseControls.SortableDataGridView();
      this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineFilterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.mosColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.eventTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.eventLevelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.addButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.eventToolLifeConfigDataGridView)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // eventToolLifeConfigDataGridView
      // 
      this.eventToolLifeConfigDataGridView.AllowUserToAddRows = false;
      this.eventToolLifeConfigDataGridView.AllowUserToResizeRows = false;
      this.eventToolLifeConfigDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.eventToolLifeConfigDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.eventToolLifeConfigDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.eventToolLifeConfigDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
      this.idColumn,
      this.machineFilterColumn,
      this.mosColumn,
      this.eventTypeColumn,
      this.eventLevelColumn});
      this.eventToolLifeConfigDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventToolLifeConfigDataGridView.Location = new System.Drawing.Point(0, 0);
      this.eventToolLifeConfigDataGridView.Name = "eventToolLifeConfigDataGridView";
      this.eventToolLifeConfigDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.eventToolLifeConfigDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.eventToolLifeConfigDataGridView.Size = new System.Drawing.Size(640, 315);
      this.eventToolLifeConfigDataGridView.TabIndex = 0;
      this.eventToolLifeConfigDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.eventToolLifeConfigDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // idColumn
      // 
      this.idColumn.DataPropertyName = "Id";
      this.idColumn.HeaderText = "idColumn";
      this.idColumn.Name = "idColumn";
      this.idColumn.ReadOnly = true;
      this.idColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.idColumn.Visible = false;
      this.idColumn.Width = 56;
      // 
      // machineFilterColumn
      // 
      this.machineFilterColumn.DataPropertyName = "MachineFilter";
      this.machineFilterColumn.HeaderText = "MachineFilter";
      this.machineFilterColumn.Name = "machineFilterColumn";
      this.machineFilterColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.machineFilterColumn.Width = 76;
      // 
      // mosColumn
      // 
      this.mosColumn.DataPropertyName = "MachineObservationState";
      this.mosColumn.HeaderText = "MachineObservationState";
      this.mosColumn.Name = "mosColumn";
      this.mosColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.mosColumn.Width = 136;
      // 
      // eventTypeColumn
      // 
      this.eventTypeColumn.DataPropertyName = "Type";
      this.eventTypeColumn.HeaderText = "ToolLifeConfigType";
      this.eventTypeColumn.Name = "eventTypeColumn";
      this.eventTypeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.eventTypeColumn.Width = 105;
      // 
      // eventLevelColumn
      // 
      this.eventLevelColumn.DataPropertyName = "Level";
      this.eventLevelColumn.HeaderText = "EventLevel";
      this.eventLevelColumn.Name = "eventLevelColumn";
      this.eventLevelColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.eventLevelColumn.Width = 67;
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.eventToolLifeConfigDataGridView);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.addButton);
      this.splitContainer1.Size = new System.Drawing.Size(640, 344);
      this.splitContainer1.SplitterDistance = 315;
      this.splitContainer1.TabIndex = 1;
      // 
      // addButton
      // 
      this.addButton.Cursor = System.Windows.Forms.Cursors.Default;
      this.addButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.addButton.Location = new System.Drawing.Point(0, 0);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(75, 25);
      this.addButton.TabIndex = 0;
      this.addButton.Text = "button1";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.AddButtonClick);
      // 
      // EventToolLifeConfigConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "EventToolLifeConfigConfig";
      this.Size = new System.Drawing.Size(640, 344);
      this.Load += new System.EventHandler(this.EventToolLifeConfigConfigLoad);
      this.Validated += new System.EventHandler(this.EventToolLifeConfigConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.eventToolLifeConfigDataGridView)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.DataGridViewTextBoxColumn eventLevelColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn eventTypeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn mosColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineFilterColumn;
    private System.Windows.Forms.Button addButton;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.BaseControls.SortableDataGridView eventToolLifeConfigDataGridView;
  }
}
