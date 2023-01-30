// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class UserConfig
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
      this.codeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.externalCodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.loginColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.passwordColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.shiftColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.mobileNumberColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.emailColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.companyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.roleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.disconnectionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.codeColumn,
            this.externalCodeColumn,
            this.loginColumn,
            this.passwordColumn,
            this.shiftColumn,
            this.mobileNumberColumn,
            this.emailColumn,
            this.companyColumn,
            this.roleColumn,
            this.disconnectionColumn});
      this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView.Location = new System.Drawing.Point(0, 0);
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView.Size = new System.Drawing.Size(872, 262);
      this.dataGridView.TabIndex = 0;
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
      // nameColumn
      // 
      this.nameColumn.DataPropertyName = "Name";
      this.nameColumn.HeaderText = "Name";
      this.nameColumn.Name = "nameColumn";
      this.nameColumn.Width = 60;
      // 
      // codeColumn
      // 
      this.codeColumn.DataPropertyName = "Code";
      this.codeColumn.HeaderText = "Code";
      this.codeColumn.Name = "codeColumn";
      this.codeColumn.Width = 57;
      // 
      // externalCodeColumn
      // 
      this.externalCodeColumn.DataPropertyName = "ExternalCode";
      this.externalCodeColumn.HeaderText = "External Code";
      this.externalCodeColumn.Name = "externalCodeColumn";
      this.externalCodeColumn.Width = 98;
      // 
      // loginColumn
      // 
      this.loginColumn.DataPropertyName = "Login";
      this.loginColumn.HeaderText = "Login";
      this.loginColumn.Name = "loginColumn";
      this.loginColumn.Width = 58;
      // 
      // passwordColumn
      // 
      this.passwordColumn.DataPropertyName = "Password";
      this.passwordColumn.HeaderText = "Password";
      this.passwordColumn.MaxInputLength = 30;
      this.passwordColumn.Name = "passwordColumn";
      this.passwordColumn.Width = 78;
      // 
      // shiftColumn
      // 
      this.shiftColumn.DataPropertyName = "Shift";
      this.shiftColumn.HeaderText = "Shift";
      this.shiftColumn.Name = "shiftColumn";
      this.shiftColumn.Width = 53;
      // 
      // mobileNumberColumn
      // 
      this.mobileNumberColumn.DataPropertyName = "MobileNumber";
      this.mobileNumberColumn.HeaderText = "Mobile number";
      this.mobileNumberColumn.Name = "mobileNumberColumn";
      this.mobileNumberColumn.Width = 101;
      // 
      // emailColumn
      // 
      this.emailColumn.DataPropertyName = "EMail";
      this.emailColumn.HeaderText = "EMail";
      this.emailColumn.Name = "emailColumn";
      this.emailColumn.Width = 58;
      // 
      // companyColumn
      // 
      this.companyColumn.DataPropertyName = "Company";
      this.companyColumn.HeaderText = "Company";
      this.companyColumn.Name = "companyColumn";
      this.companyColumn.Width = 76;
      // 
      // roleColumn
      // 
      this.roleColumn.DataPropertyName = "Role";
      this.roleColumn.HeaderText = "Role";
      this.roleColumn.Name = "roleColumn";
      this.roleColumn.Width = 54;
      // 
      // disconnectionColumn
      // 
      this.disconnectionColumn.DataPropertyName = "DisconnectionTime";
      this.disconnectionColumn.HeaderText = "Disconnection";
      this.disconnectionColumn.Name = "disconnectionColumn";
      // 
      // UserConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.dataGridView);
      this.Name = "UserConfig";
      this.Size = new System.Drawing.Size(872, 262);
      this.Load += new System.EventHandler(this.UserConfigLoad);
      this.Validated += new System.EventHandler(this.UserConfigValidated);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.SortableDataGridView dataGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn versionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn codeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn externalCodeColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn loginColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn passwordColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn shiftColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn mobileNumberColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn emailColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn companyColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn roleColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn disconnectionColumn;
  }
}
