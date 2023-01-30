// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class EventCncValueConfigConfig
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
      this.eventCncValueConfigDataGridView = new Lemoine.BaseControls.SortableDataGridView();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.addButton = new System.Windows.Forms.Button();
      this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.versionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.messageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.fieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineFilterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.conditionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.minDurationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.eventLevelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.eventCncValueConfigDataGridView)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // eventCncValueConfigDataGridView
      // 
      this.eventCncValueConfigDataGridView.AllowUserToAddRows = false;
      this.eventCncValueConfigDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.eventCncValueConfigDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
      this.eventCncValueConfigDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.eventCncValueConfigDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.idColumn,
                  this.versionColumn,
                  this.nameColumn,
                  this.messageColumn,
                  this.fieldColumn,
                  this.machineFilterColumn,
                  this.conditionColumn,
                  this.minDurationColumn,
                  this.eventLevelColumn});
      this.eventCncValueConfigDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventCncValueConfigDataGridView.Location = new System.Drawing.Point(0, 0);
      this.eventCncValueConfigDataGridView.Name = "eventCncValueConfigDataGridView";
      this.eventCncValueConfigDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.eventCncValueConfigDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.eventCncValueConfigDataGridView.Size = new System.Drawing.Size(712, 364);
      this.eventCncValueConfigDataGridView.TabIndex = 0;
      this.eventCncValueConfigDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.eventCncValueConfigDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
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
      this.splitContainer1.Panel1.Controls.Add(this.eventCncValueConfigDataGridView);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.addButton);
      this.splitContainer1.Size = new System.Drawing.Size(712, 393);
      this.splitContainer1.SplitterDistance = 364;
      this.splitContainer1.TabIndex = 1;
      // 
      // addButton
      // 
      this.addButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.addButton.Location = new System.Drawing.Point(0, 0);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(75, 25);
      this.addButton.TabIndex = 0;
      this.addButton.Text = "button1";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.AddButtonClick);
      // 
      // idColumn
      // 
      this.idColumn.DataPropertyName = "Id";
      this.idColumn.HeaderText = "Id";
      this.idColumn.Name = "idColumn";
      this.idColumn.ReadOnly = true;
      this.idColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.idColumn.Visible = false;
      this.idColumn.Width = 22;
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
      this.nameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.nameColumn.Width = 41;
      // 
      // messageColumn
      // 
      this.messageColumn.DataPropertyName = "Message";
      this.messageColumn.HeaderText = "messageColumn";
      this.messageColumn.Name = "messageColumn";
      this.messageColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.messageColumn.Width = 90;
      // 
      // fieldColumn
      // 
      this.fieldColumn.DataPropertyName = "Field";
      this.fieldColumn.HeaderText = "fieldColumn";
      this.fieldColumn.Name = "fieldColumn";
      this.fieldColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.fieldColumn.Width = 67;
      // 
      // machineFilterColumn
      // 
      this.machineFilterColumn.DataPropertyName = "MachineFilter";
      this.machineFilterColumn.HeaderText = "machineFilterColumn";
      this.machineFilterColumn.Name = "machineFilterColumn";
      this.machineFilterColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.machineFilterColumn.Width = 110;
      // 
      // conditionColumn
      // 
      this.conditionColumn.DataPropertyName = "Condition";
      this.conditionColumn.HeaderText = "conditionColumn";
      this.conditionColumn.Name = "conditionColumn";
      this.conditionColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.conditionColumn.Width = 91;
      // 
      // minDurationColumn
      // 
      this.minDurationColumn.DataPropertyName = "MinDuration";
      this.minDurationColumn.HeaderText = "minDurationColumn";
      this.minDurationColumn.Name = "minDurationColumn";
      this.minDurationColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.minDurationColumn.Width = 104;
      // 
      // eventLevelColumn
      // 
      this.eventLevelColumn.DataPropertyName = "Level";
      this.eventLevelColumn.HeaderText = "eventLevelColumn";
      this.eventLevelColumn.Name = "eventLevelColumn";
      this.eventLevelColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.eventLevelColumn.Width = 101;
      // 
      // EventCncValueConfigConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "EventCncValueConfigConfig";
      this.Size = new System.Drawing.Size(712, 393);
      this.Load += new System.EventHandler(this.EventCncValueConfigConfigLoad);
      this.Validated += new System.EventHandler(this.EventCncValueConfigConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.eventCncValueConfigDataGridView)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Button addButton;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.DataGridViewTextBoxColumn eventLevelColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn minDurationColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn conditionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineFilterColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn fieldColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn messageColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private Lemoine.BaseControls.SortableDataGridView eventCncValueConfigDataGridView;
  }
}
