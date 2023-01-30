// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class MachineStateTemplateRightConfig
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
      this.splitContainer4 = new System.Windows.Forms.SplitContainer();
      this.roleSelectionGroupBox = new System.Windows.Forms.GroupBox();
      this.roleSelection = new Lemoine.DataReferenceControls.RoleSelection();
      this.defaultSelectionGroupBox = new System.Windows.Forms.GroupBox();
      this.deniedRadioButton = new System.Windows.Forms.RadioButton();
      this.grantedRadioButton = new System.Windows.Forms.RadioButton();
      this.defaultLabel = new System.Windows.Forms.Label();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.splitContainer3 = new System.Windows.Forms.SplitContainer();
      this.exceptionGroupBox = new System.Windows.Forms.GroupBox();
      this.machineStateTemplateRightDataGridView = new System.Windows.Forms.DataGridView();
      this.machineStateTemplateRightIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateRightAccessColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.machineStateTemplateRightMachineStateTemplateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.AddButton = new System.Windows.Forms.Button();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.splitContainer4.Panel1.SuspendLayout();
      this.splitContainer4.Panel2.SuspendLayout();
      this.splitContainer4.SuspendLayout();
      this.roleSelectionGroupBox.SuspendLayout();
      this.defaultSelectionGroupBox.SuspendLayout();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.splitContainer3.Panel1.SuspendLayout();
      this.splitContainer3.Panel2.SuspendLayout();
      this.splitContainer3.SuspendLayout();
      this.exceptionGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.machineStateTemplateRightDataGridView)).BeginInit();
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
      this.splitContainer1.Panel1.Controls.Add(this.splitContainer4);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Size = new System.Drawing.Size(616, 441);
      this.splitContainer1.SplitterDistance = 229;
      this.splitContainer1.TabIndex = 0;
      // 
      // splitContainer4
      // 
      this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer4.Location = new System.Drawing.Point(0, 0);
      this.splitContainer4.Name = "splitContainer4";
      this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer4.Panel1
      // 
      this.splitContainer4.Panel1.Controls.Add(this.roleSelectionGroupBox);
      // 
      // splitContainer4.Panel2
      // 
      this.splitContainer4.Panel2.Controls.Add(this.defaultSelectionGroupBox);
      this.splitContainer4.Size = new System.Drawing.Size(229, 441);
      this.splitContainer4.SplitterDistance = 337;
      this.splitContainer4.TabIndex = 0;
      // 
      // roleSelectionGroupBox
      // 
      this.roleSelectionGroupBox.Controls.Add(this.roleSelection);
      this.roleSelectionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.roleSelectionGroupBox.Location = new System.Drawing.Point(0, 0);
      this.roleSelectionGroupBox.Name = "roleSelectionGroupBox";
      this.roleSelectionGroupBox.Size = new System.Drawing.Size(229, 337);
      this.roleSelectionGroupBox.TabIndex = 1;
      this.roleSelectionGroupBox.TabStop = false;
      this.roleSelectionGroupBox.Text = "groupBox1";
      // 
      // roleSelection
      // 
      this.roleSelection.DisplayedProperty = "Display";
      this.roleSelection.Dock = System.Windows.Forms.DockStyle.Fill;
      this.roleSelection.Location = new System.Drawing.Point(3, 16);
      this.roleSelection.MultiSelect = false;
      this.roleSelection.Name = "roleSelection";
      this.roleSelection.Nullable = false;
      this.roleSelection.Size = new System.Drawing.Size(223, 318);
      this.roleSelection.TabIndex = 0;
      this.roleSelection.AfterSelect += new System.EventHandler(this.RoleSelectionAfterSelect);
      // 
      // defaultSelectionGroupBox
      // 
      this.defaultSelectionGroupBox.Controls.Add(this.deniedRadioButton);
      this.defaultSelectionGroupBox.Controls.Add(this.grantedRadioButton);
      this.defaultSelectionGroupBox.Controls.Add(this.defaultLabel);
      this.defaultSelectionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.defaultSelectionGroupBox.Location = new System.Drawing.Point(0, 0);
      this.defaultSelectionGroupBox.Name = "defaultSelectionGroupBox";
      this.defaultSelectionGroupBox.Size = new System.Drawing.Size(229, 100);
      this.defaultSelectionGroupBox.TabIndex = 0;
      this.defaultSelectionGroupBox.TabStop = false;
      this.defaultSelectionGroupBox.Text = "groupBox2";
      // 
      // deniedRadioButton
      // 
      this.deniedRadioButton.Location = new System.Drawing.Point(112, 57);
      this.deniedRadioButton.Name = "deniedRadioButton";
      this.deniedRadioButton.Size = new System.Drawing.Size(104, 24);
      this.deniedRadioButton.TabIndex = 2;
      this.deniedRadioButton.TabStop = true;
      this.deniedRadioButton.Text = "radioButton2";
      this.deniedRadioButton.UseVisualStyleBackColor = true;
      this.deniedRadioButton.CheckedChanged += new System.EventHandler(this.DeniedRadioButtonCheckedChanged);
      // 
      // grantedRadioButton
      // 
      this.grantedRadioButton.Location = new System.Drawing.Point(112, 27);
      this.grantedRadioButton.Name = "grantedRadioButton";
      this.grantedRadioButton.Size = new System.Drawing.Size(104, 24);
      this.grantedRadioButton.TabIndex = 1;
      this.grantedRadioButton.TabStop = true;
      this.grantedRadioButton.Text = "radioButton1";
      this.grantedRadioButton.UseVisualStyleBackColor = true;
      this.grantedRadioButton.CheckedChanged += new System.EventHandler(this.GrantedRadioButtonCheckedChanged);
      // 
      // defaultLabel
      // 
      this.defaultLabel.Location = new System.Drawing.Point(6, 27);
      this.defaultLabel.Name = "defaultLabel";
      this.defaultLabel.Size = new System.Drawing.Size(100, 23);
      this.defaultLabel.TabIndex = 0;
      this.defaultLabel.Text = "label1";
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
      this.splitContainer2.Panel2Collapsed = true;
      this.splitContainer2.Size = new System.Drawing.Size(383, 441);
      this.splitContainer2.SplitterDistance = 220;
      this.splitContainer2.TabIndex = 1;
      // 
      // splitContainer3
      // 
      this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer3.Location = new System.Drawing.Point(0, 0);
      this.splitContainer3.Name = "splitContainer3";
      this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer3.Panel1
      // 
      this.splitContainer3.Panel1.Controls.Add(this.exceptionGroupBox);
      // 
      // splitContainer3.Panel2
      // 
      this.splitContainer3.Panel2.Controls.Add(this.AddButton);
      this.splitContainer3.Size = new System.Drawing.Size(383, 441);
      this.splitContainer3.SplitterDistance = 412;
      this.splitContainer3.TabIndex = 0;
      // 
      // exceptionGroupBox
      // 
      this.exceptionGroupBox.Controls.Add(this.machineStateTemplateRightDataGridView);
      this.exceptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.exceptionGroupBox.Location = new System.Drawing.Point(0, 0);
      this.exceptionGroupBox.Name = "exceptionGroupBox";
      this.exceptionGroupBox.Size = new System.Drawing.Size(383, 412);
      this.exceptionGroupBox.TabIndex = 1;
      this.exceptionGroupBox.TabStop = false;
      this.exceptionGroupBox.Text = "groupBox1";
      // 
      // machineStateTemplateRightDataGridView
      // 
      this.machineStateTemplateRightDataGridView.AllowUserToAddRows = false;
      this.machineStateTemplateRightDataGridView.AllowUserToResizeRows = false;
      this.machineStateTemplateRightDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
      this.machineStateTemplateRightDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.machineStateTemplateRightDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                  this.machineStateTemplateRightIdColumn,
                  this.machineStateTemplateRightAccessColumn,
                  this.machineStateTemplateRightMachineStateTemplateColumn});
      this.machineStateTemplateRightDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateRightDataGridView.Location = new System.Drawing.Point(3, 16);
      this.machineStateTemplateRightDataGridView.Name = "machineStateTemplateRightDataGridView";
      this.machineStateTemplateRightDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.machineStateTemplateRightDataGridView.Size = new System.Drawing.Size(377, 393);
      this.machineStateTemplateRightDataGridView.TabIndex = 0;
      this.machineStateTemplateRightDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.MachineStateTemplateRightDataGridViewCellFormatting);
      this.machineStateTemplateRightDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.MachineStateTemplateRightDataGridViewUserDeletingRow);
      // 
      // machineStateTemplateRightIdColumn
      // 
      this.machineStateTemplateRightIdColumn.DataPropertyName = "Id";
      this.machineStateTemplateRightIdColumn.HeaderText = "IdColumn";
      this.machineStateTemplateRightIdColumn.Name = "machineStateTemplateRightIdColumn";
      this.machineStateTemplateRightIdColumn.ReadOnly = true;
      this.machineStateTemplateRightIdColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.machineStateTemplateRightIdColumn.Visible = false;
      // 
      // machineStateTemplateRightAccessColumn
      // 
      this.machineStateTemplateRightAccessColumn.DataPropertyName = "AccessPrivilege";
      this.machineStateTemplateRightAccessColumn.HeaderText = "RightAccess";
      this.machineStateTemplateRightAccessColumn.Name = "machineStateTemplateRightAccessColumn";
      this.machineStateTemplateRightAccessColumn.ReadOnly = true;
      this.machineStateTemplateRightAccessColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.machineStateTemplateRightAccessColumn.Width = 73;
      // 
      // machineStateTemplateRightMachineStateTemplateColumn
      // 
      this.machineStateTemplateRightMachineStateTemplateColumn.DataPropertyName = "MachineStateTemplate";
      this.machineStateTemplateRightMachineStateTemplateColumn.HeaderText = "MachineStateTemplate";
      this.machineStateTemplateRightMachineStateTemplateColumn.Name = "machineStateTemplateRightMachineStateTemplateColumn";
      this.machineStateTemplateRightMachineStateTemplateColumn.ReadOnly = true;
      this.machineStateTemplateRightMachineStateTemplateColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.machineStateTemplateRightMachineStateTemplateColumn.Width = 123;
      // 
      // AddButton
      // 
      this.AddButton.Dock = System.Windows.Forms.DockStyle.Left;
      this.AddButton.Location = new System.Drawing.Point(0, 0);
      this.AddButton.Name = "AddButton";
      this.AddButton.Size = new System.Drawing.Size(75, 25);
      this.AddButton.TabIndex = 0;
      this.AddButton.Text = "button1";
      this.AddButton.UseVisualStyleBackColor = true;
      this.AddButton.Click += new System.EventHandler(this.AddButtonClick);
      // 
      // MachineStateTemplateRightConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "MachineStateTemplateRightConfig";
      this.Size = new System.Drawing.Size(616, 441);
      this.Load += new System.EventHandler(this.MachineStateTemplateRightConfigLoad);
      this.Validated += new System.EventHandler(this.MachineStateTemplateRightConfigValidated);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer4.Panel1.ResumeLayout(false);
      this.splitContainer4.Panel2.ResumeLayout(false);
      this.splitContainer4.ResumeLayout(false);
      this.roleSelectionGroupBox.ResumeLayout(false);
      this.defaultSelectionGroupBox.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.ResumeLayout(false);
      this.splitContainer3.Panel1.ResumeLayout(false);
      this.splitContainer3.Panel2.ResumeLayout(false);
      this.splitContainer3.ResumeLayout(false);
      this.exceptionGroupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.machineStateTemplateRightDataGridView)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.GroupBox exceptionGroupBox;
    private System.Windows.Forms.Label defaultLabel;
    private System.Windows.Forms.RadioButton grantedRadioButton;
    private System.Windows.Forms.RadioButton deniedRadioButton;
    private System.Windows.Forms.GroupBox defaultSelectionGroupBox;
    private Lemoine.DataReferenceControls.RoleSelection roleSelection;
    private System.Windows.Forms.GroupBox roleSelectionGroupBox;
    private System.Windows.Forms.SplitContainer splitContainer4;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateRightMachineStateTemplateColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateRightAccessColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn machineStateTemplateRightIdColumn;
    private System.Windows.Forms.DataGridView machineStateTemplateRightDataGridView;
    private System.Windows.Forms.Button AddButton;
    private System.Windows.Forms.SplitContainer splitContainer3;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.SplitContainer splitContainer1;
  }
}
