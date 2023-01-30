// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class EventLevelConfig
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
      this.eventLevelDataGridView = new System.Windows.Forms.DataGridView();
      this.eventLevelidColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.eventLevelnameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.eventLevelTranslationKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.eventLevelPriorityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.eventLevelDataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // eventLevelDataGridView
      // 
      this.eventLevelDataGridView.AllowUserToResizeRows = false;
      this.eventLevelDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.eventLevelDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.eventLevelidColumn,
                  this.eventLevelnameColumn,
                  this.eventLevelTranslationKeyColumn,
                  this.eventLevelPriorityColumn});
      this.eventLevelDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventLevelDataGridView.Location = new System.Drawing.Point(0, 0);
      this.eventLevelDataGridView.Name = "eventLevelDataGridView";
      this.eventLevelDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.eventLevelDataGridView.Size = new System.Drawing.Size(541, 247);
      this.eventLevelDataGridView.TabIndex = 0;
      this.eventLevelDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
      this.eventLevelDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridViewUserDeletingRow);
      // 
      // eventLevelidColumn
      // 
      this.eventLevelidColumn.DataPropertyName = "Id";
      this.eventLevelidColumn.HeaderText = "Id";
      this.eventLevelidColumn.Name = "eventLevelidColumn";
      this.eventLevelidColumn.ReadOnly = true;
      this.eventLevelidColumn.Visible = false;
      // 
      // eventLevelnameColumn
      // 
      this.eventLevelnameColumn.DataPropertyName = "Name";
      this.eventLevelnameColumn.HeaderText = "Name";
      this.eventLevelnameColumn.Name = "eventLevelnameColumn";
      this.eventLevelnameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // eventLevelTranslationKeyColumn
      // 
      this.eventLevelTranslationKeyColumn.DataPropertyName = "TranslationKey";
      this.eventLevelTranslationKeyColumn.HeaderText = "TranslationKey";
      this.eventLevelTranslationKeyColumn.Name = "eventLevelTranslationKeyColumn";
      this.eventLevelTranslationKeyColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // eventLevelPriorityColumn
      // 
      this.eventLevelPriorityColumn.DataPropertyName = "Priority";
      this.eventLevelPriorityColumn.HeaderText = "Priority";
      this.eventLevelPriorityColumn.Name = "eventLevelPriorityColumn";
      this.eventLevelPriorityColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // EventLevelConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.eventLevelDataGridView);
      this.Name = "EventLevelConfig";
      this.Size = new System.Drawing.Size(541, 247);
      this.Load += new System.EventHandler(this.EventLevelConfigLoad);
      this.Enter += new System.EventHandler(this.EventLevelConfigEnter);
      this.Validated += new System.EventHandler(this.EventLevelConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.eventLevelDataGridView)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.DataGridView eventLevelDataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn eventLevelidColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn eventLevelnameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn eventLevelTranslationKeyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn eventLevelPriorityColumn; 
  }
}
