// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class SequenceControl
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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
      this.resetBtn = new System.Windows.Forms.Button();
      this.cancelBtn = new System.Windows.Forms.Button();
      this.operationTextBox = new System.Windows.Forms.TextBox();
      this.panel = new System.Windows.Forms.Panel();
      this.toolNumberLabel = new System.Windows.Forms.Label();
      this.toolNumberTextBox = new System.Windows.Forms.TextBox();
      this.kindComboBox = new System.Windows.Forms.ComboBox();
      this.kindLabel = new System.Windows.Forms.Label();
      this.stampingDetailsDataGridView = new System.Windows.Forms.DataGridView();
      this.IsoFileNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.StampPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.IsCycleEnd = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.IsoFileSourcePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.TargetDirectory = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.StampingDateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.stampingDetailsLabel = new System.Windows.Forms.Label();
      this.orderTextBox = new System.Windows.Forms.TextBox();
      this.orderLabel = new System.Windows.Forms.Label();
      this.estimatedTimeTimeSpanPicker = new Lemoine.BaseControls.NullableTimeSpanPicker();
      this.pathTextBox = new System.Windows.Forms.TextBox();
      this.pathLbl = new System.Windows.Forms.Label();
      this.stampingValueLbl = new System.Windows.Forms.Label();
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      this.field = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.fieldid = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.stampingdatatype = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.cadModelTextBox = new System.Windows.Forms.TextBox();
      this.toolTextBox = new System.Windows.Forms.TextBox();
      this.descriptionTextBox = new System.Windows.Forms.TextBox();
      this.operationLbl = new System.Windows.Forms.Label();
      this.toolLbl = new System.Windows.Forms.Label();
      this.cadModelLbl = new System.Windows.Forms.Label();
      this.descriptionLbl = new System.Windows.Forms.Label();
      this.estimatedtimeLbl = new System.Windows.Forms.Label();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.nameLbl = new System.Windows.Forms.Label();
      this.createBtn = new System.Windows.Forms.Button();
      this.saveBtn = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.stampingDetailsDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.stampingDetailsDataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.operationStepLabel = new System.Windows.Forms.Label();
      this.operationStepTextBox = new System.Windows.Forms.TextBox();
      this.panel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.stampingDetailsDataGridView)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      this.SuspendLayout();
      // 
      // resetBtn
      // 
      this.resetBtn.Location = new System.Drawing.Point(170, 564);
      this.resetBtn.Name = "resetBtn";
      this.resetBtn.Size = new System.Drawing.Size(75, 23);
      this.resetBtn.TabIndex = 29;
      this.resetBtn.Text = "Reset";
      this.resetBtn.UseVisualStyleBackColor = true;
      this.resetBtn.Click += new System.EventHandler(this.ResetBtnClick);
      // 
      // cancelBtn
      // 
      this.cancelBtn.Location = new System.Drawing.Point(170, 564);
      this.cancelBtn.Name = "cancelBtn";
      this.cancelBtn.Size = new System.Drawing.Size(75, 23);
      this.cancelBtn.TabIndex = 35;
      this.cancelBtn.Text = "Cancel";
      this.cancelBtn.UseVisualStyleBackColor = true;
      this.cancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
      // 
      // operationTextBox
      // 
      this.operationTextBox.Location = new System.Drawing.Point(90, 239);
      this.operationTextBox.Name = "operationTextBox";
      this.operationTextBox.ReadOnly = true;
      this.operationTextBox.Size = new System.Drawing.Size(180, 20);
      this.operationTextBox.TabIndex = 27;
      // 
      // panel
      // 
      this.panel.AutoSize = true;
      this.panel.Controls.Add(this.operationStepLabel);
      this.panel.Controls.Add(this.operationStepTextBox);
      this.panel.Controls.Add(this.toolNumberLabel);
      this.panel.Controls.Add(this.toolNumberTextBox);
      this.panel.Controls.Add(this.kindComboBox);
      this.panel.Controls.Add(this.kindLabel);
      this.panel.Controls.Add(this.stampingDetailsDataGridView);
      this.panel.Controls.Add(this.stampingDetailsLabel);
      this.panel.Controls.Add(this.orderTextBox);
      this.panel.Controls.Add(this.orderLabel);
      this.panel.Controls.Add(this.estimatedTimeTimeSpanPicker);
      this.panel.Controls.Add(this.pathTextBox);
      this.panel.Controls.Add(this.pathLbl);
      this.panel.Controls.Add(this.stampingValueLbl);
      this.panel.Controls.Add(this.dataGridView1);
      this.panel.Controls.Add(this.resetBtn);
      this.panel.Controls.Add(this.cancelBtn);
      this.panel.Controls.Add(this.operationTextBox);
      this.panel.Controls.Add(this.cadModelTextBox);
      this.panel.Controls.Add(this.toolTextBox);
      this.panel.Controls.Add(this.descriptionTextBox);
      this.panel.Controls.Add(this.operationLbl);
      this.panel.Controls.Add(this.toolLbl);
      this.panel.Controls.Add(this.cadModelLbl);
      this.panel.Controls.Add(this.descriptionLbl);
      this.panel.Controls.Add(this.estimatedtimeLbl);
      this.panel.Controls.Add(this.nameTextBox);
      this.panel.Controls.Add(this.nameLbl);
      this.panel.Controls.Add(this.createBtn);
      this.panel.Controls.Add(this.saveBtn);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(428, 614);
      this.panel.TabIndex = 22;
      // 
      // toolNumberLabel
      // 
      this.toolNumberLabel.Location = new System.Drawing.Point(4, 294);
      this.toolNumberLabel.Name = "toolNumberLabel";
      this.toolNumberLabel.Size = new System.Drawing.Size(80, 21);
      this.toolNumberLabel.TabIndex = 49;
      this.toolNumberLabel.Text = "Tool #";
      this.toolNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // toolNumberTextBox
      // 
      this.toolNumberTextBox.Location = new System.Drawing.Point(90, 295);
      this.toolNumberTextBox.Name = "toolNumberTextBox";
      this.toolNumberTextBox.Size = new System.Drawing.Size(180, 20);
      this.toolNumberTextBox.TabIndex = 48;
      this.toolNumberTextBox.TextChanged += new System.EventHandler(this.toolNumberTextBox_TextChanged);
      // 
      // kindComboBox
      // 
      this.kindComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.kindComboBox.FormattingEnabled = true;
      this.kindComboBox.Location = new System.Drawing.Point(90, 57);
      this.kindComboBox.Name = "kindComboBox";
      this.kindComboBox.Size = new System.Drawing.Size(179, 21);
      this.kindComboBox.TabIndex = 47;
      this.kindComboBox.SelectedIndexChanged += new System.EventHandler(this.KindComboBoxSelectedIndexChanged);
      // 
      // kindLabel
      // 
      this.kindLabel.Location = new System.Drawing.Point(4, 53);
      this.kindLabel.Name = "kindLabel";
      this.kindLabel.Size = new System.Drawing.Size(80, 21);
      this.kindLabel.TabIndex = 46;
      this.kindLabel.Text = "Kind";
      this.kindLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // stampingDetailsDataGridView
      // 
      this.stampingDetailsDataGridView.AllowUserToAddRows = false;
      this.stampingDetailsDataGridView.AllowUserToDeleteRows = false;
      this.stampingDetailsDataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
      dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.stampingDetailsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
      this.stampingDetailsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.stampingDetailsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IsoFileNameColumn,
            this.StampPosition,
            this.IsCycleEnd,
            this.IsoFileSourcePath,
            this.TargetDirectory,
            this.StampingDateTime});
      this.stampingDetailsDataGridView.Location = new System.Drawing.Point(90, 461);
      this.stampingDetailsDataGridView.Name = "stampingDetailsDataGridView";
      this.stampingDetailsDataGridView.ReadOnly = true;
      this.stampingDetailsDataGridView.ShowEditingIcon = false;
      this.stampingDetailsDataGridView.Size = new System.Drawing.Size(335, 94);
      this.stampingDetailsDataGridView.TabIndex = 45;
      // 
      // IsoFileNameColumn
      // 
      this.IsoFileNameColumn.HeaderText = "Iso File Name";
      this.IsoFileNameColumn.Name = "IsoFileNameColumn";
      this.IsoFileNameColumn.ReadOnly = true;
      // 
      // StampPosition
      // 
      this.StampPosition.HeaderText = "Stamp Position";
      this.StampPosition.Name = "StampPosition";
      this.StampPosition.ReadOnly = true;
      // 
      // IsCycleEnd
      // 
      this.IsCycleEnd.HeaderText = "Cycle Begin";
      this.IsCycleEnd.Name = "IsCycleEnd";
      this.IsCycleEnd.ReadOnly = true;
      // 
      // IsoFileSourcePath
      // 
      this.IsoFileSourcePath.HeaderText = "Source Directory";
      this.IsoFileSourcePath.Name = "IsoFileSourcePath";
      this.IsoFileSourcePath.ReadOnly = true;
      // 
      // TargetDirectory
      // 
      this.TargetDirectory.HeaderText = "Target Directory";
      this.TargetDirectory.Name = "TargetDirectory";
      this.TargetDirectory.ReadOnly = true;
      // 
      // StampingDateTime
      // 
      this.StampingDateTime.HeaderText = "Stamping DateTime";
      this.StampingDateTime.Name = "StampingDateTime";
      this.StampingDateTime.ReadOnly = true;
      // 
      // stampingDetailsLabel
      // 
      this.stampingDetailsLabel.Location = new System.Drawing.Point(4, 461);
      this.stampingDetailsLabel.Name = "stampingDetailsLabel";
      this.stampingDetailsLabel.Size = new System.Drawing.Size(80, 32);
      this.stampingDetailsLabel.TabIndex = 44;
      this.stampingDetailsLabel.Text = "Stamping Details";
      this.stampingDetailsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // orderTextBox
      // 
      this.orderTextBox.Enabled = false;
      this.orderTextBox.Location = new System.Drawing.Point(90, 31);
      this.orderTextBox.Name = "orderTextBox";
      this.orderTextBox.Size = new System.Drawing.Size(180, 20);
      this.orderTextBox.TabIndex = 43;
      // 
      // orderLabel
      // 
      this.orderLabel.Location = new System.Drawing.Point(3, 29);
      this.orderLabel.Name = "orderLabel";
      this.orderLabel.Size = new System.Drawing.Size(80, 21);
      this.orderLabel.TabIndex = 42;
      this.orderLabel.Text = "Order";
      this.orderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // estimatedTimeTimeSpanPicker
      // 
      this.estimatedTimeTimeSpanPicker.Location = new System.Drawing.Point(90, 86);
      this.estimatedTimeTimeSpanPicker.Name = "estimatedTimeTimeSpanPicker";
      this.estimatedTimeTimeSpanPicker.Size = new System.Drawing.Size(176, 30);
      this.estimatedTimeTimeSpanPicker.TabIndex = 41;
      this.estimatedTimeTimeSpanPicker.Value = null;
      this.estimatedTimeTimeSpanPicker.ValueAsHours = null;
      this.estimatedTimeTimeSpanPicker.ValueChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // pathTextBox
      // 
      this.pathTextBox.Location = new System.Drawing.Point(90, 269);
      this.pathTextBox.Name = "pathTextBox";
      this.pathTextBox.ReadOnly = true;
      this.pathTextBox.Size = new System.Drawing.Size(180, 20);
      this.pathTextBox.TabIndex = 40;
      // 
      // pathLbl
      // 
      this.pathLbl.Location = new System.Drawing.Point(4, 268);
      this.pathLbl.Name = "pathLbl";
      this.pathLbl.Size = new System.Drawing.Size(80, 21);
      this.pathLbl.TabIndex = 39;
      this.pathLbl.Text = "Path Number";
      this.pathLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // stampingValueLbl
      // 
      this.stampingValueLbl.Location = new System.Drawing.Point(4, 347);
      this.stampingValueLbl.Name = "stampingValueLbl";
      this.stampingValueLbl.Size = new System.Drawing.Size(80, 21);
      this.stampingValueLbl.TabIndex = 38;
      this.stampingValueLbl.Text = "Fields";
      this.stampingValueLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToResizeRows = false;
      dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
      dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.field,
            this.value,
            this.fieldid,
            this.stampingdatatype});
      this.dataGridView1.Location = new System.Drawing.Point(90, 347);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.Size = new System.Drawing.Size(335, 108);
      this.dataGridView1.TabIndex = 36;
      this.dataGridView1.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.DataGridView1CellBeginEdit);
      this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1CellClick);
      this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1CellEndEdit);
      this.dataGridView1.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.DataGridView1UserDeletingRow);
      // 
      // field
      // 
      this.field.HeaderText = "Field";
      this.field.MinimumWidth = 50;
      this.field.Name = "field";
      this.field.ReadOnly = true;
      // 
      // value
      // 
      dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.value.DefaultCellStyle = dataGridViewCellStyle8;
      this.value.HeaderText = "Value";
      this.value.MinimumWidth = 50;
      this.value.Name = "value";
      // 
      // fieldid
      // 
      this.fieldid.HeaderText = "Stamping Id";
      this.fieldid.Name = "fieldid";
      this.fieldid.Visible = false;
      // 
      // stampingdatatype
      // 
      this.stampingdatatype.HeaderText = "Stamping Data Type";
      this.stampingdatatype.Name = "stampingdatatype";
      this.stampingdatatype.Visible = false;
      // 
      // cadModelTextBox
      // 
      this.cadModelTextBox.Location = new System.Drawing.Point(90, 189);
      this.cadModelTextBox.Name = "cadModelTextBox";
      this.cadModelTextBox.ReadOnly = true;
      this.cadModelTextBox.Size = new System.Drawing.Size(180, 20);
      this.cadModelTextBox.TabIndex = 24;
      this.cadModelTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      this.cadModelTextBox.DoubleClick += new System.EventHandler(this.CadModelTextBoxDoubleClick);
      // 
      // toolTextBox
      // 
      this.toolTextBox.Location = new System.Drawing.Point(90, 214);
      this.toolTextBox.Name = "toolTextBox";
      this.toolTextBox.ReadOnly = true;
      this.toolTextBox.Size = new System.Drawing.Size(180, 20);
      this.toolTextBox.TabIndex = 26;
      this.toolTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      this.toolTextBox.DoubleClick += new System.EventHandler(this.ToolTextBoxDoubleClick);
      // 
      // descriptionTextBox
      // 
      this.descriptionTextBox.AcceptsReturn = true;
      this.descriptionTextBox.Location = new System.Drawing.Point(90, 116);
      this.descriptionTextBox.Multiline = true;
      this.descriptionTextBox.Name = "descriptionTextBox";
      this.descriptionTextBox.Size = new System.Drawing.Size(180, 67);
      this.descriptionTextBox.TabIndex = 23;
      this.descriptionTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // operationLbl
      // 
      this.operationLbl.Location = new System.Drawing.Point(4, 239);
      this.operationLbl.Name = "operationLbl";
      this.operationLbl.Size = new System.Drawing.Size(80, 21);
      this.operationLbl.TabIndex = 34;
      this.operationLbl.Text = "Operation";
      this.operationLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // toolLbl
      // 
      this.toolLbl.Location = new System.Drawing.Point(4, 214);
      this.toolLbl.Name = "toolLbl";
      this.toolLbl.Size = new System.Drawing.Size(80, 21);
      this.toolLbl.TabIndex = 33;
      this.toolLbl.Text = "Tool";
      this.toolLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // cadModelLbl
      // 
      this.cadModelLbl.Location = new System.Drawing.Point(4, 189);
      this.cadModelLbl.Name = "cadModelLbl";
      this.cadModelLbl.Size = new System.Drawing.Size(80, 21);
      this.cadModelLbl.TabIndex = 32;
      this.cadModelLbl.Text = "CAD model";
      this.cadModelLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // descriptionLbl
      // 
      this.descriptionLbl.Location = new System.Drawing.Point(4, 116);
      this.descriptionLbl.Name = "descriptionLbl";
      this.descriptionLbl.Size = new System.Drawing.Size(80, 21);
      this.descriptionLbl.TabIndex = 31;
      this.descriptionLbl.Text = "Description";
      this.descriptionLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // estimatedtimeLbl
      // 
      this.estimatedtimeLbl.Location = new System.Drawing.Point(4, 86);
      this.estimatedtimeLbl.Name = "estimatedtimeLbl";
      this.estimatedtimeLbl.Size = new System.Drawing.Size(80, 21);
      this.estimatedtimeLbl.TabIndex = 28;
      this.estimatedtimeLbl.Text = "Estimated time";
      this.estimatedtimeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // nameTextBox
      // 
      this.nameTextBox.Location = new System.Drawing.Point(90, 5);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(180, 20);
      this.nameTextBox.TabIndex = 18;
      this.nameTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // nameLbl
      // 
      this.nameLbl.Location = new System.Drawing.Point(4, 5);
      this.nameLbl.Name = "nameLbl";
      this.nameLbl.Size = new System.Drawing.Size(80, 21);
      this.nameLbl.TabIndex = 19;
      this.nameLbl.Text = "Name";
      this.nameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // createBtn
      // 
      this.createBtn.Location = new System.Drawing.Point(350, 564);
      this.createBtn.Name = "createBtn";
      this.createBtn.Size = new System.Drawing.Size(75, 23);
      this.createBtn.TabIndex = 30;
      this.createBtn.Text = "Create";
      this.createBtn.UseVisualStyleBackColor = true;
      this.createBtn.Click += new System.EventHandler(this.CreateBtnClick);
      // 
      // saveBtn
      // 
      this.saveBtn.Location = new System.Drawing.Point(350, 564);
      this.saveBtn.Name = "saveBtn";
      this.saveBtn.Size = new System.Drawing.Size(75, 23);
      this.saveBtn.TabIndex = 25;
      this.saveBtn.Text = "Save";
      this.saveBtn.UseVisualStyleBackColor = true;
      this.saveBtn.Click += new System.EventHandler(this.SaveBtnClick);
      // 
      // dataGridViewTextBoxColumn1
      // 
      this.dataGridViewTextBoxColumn1.HeaderText = "Field";
      this.dataGridViewTextBoxColumn1.MinimumWidth = 50;
      this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
      this.dataGridViewTextBoxColumn1.ReadOnly = true;
      // 
      // dataGridViewTextBoxColumn2
      // 
      dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.dataGridViewTextBoxColumn2.DefaultCellStyle = dataGridViewCellStyle9;
      this.dataGridViewTextBoxColumn2.HeaderText = "Value";
      this.dataGridViewTextBoxColumn2.MinimumWidth = 50;
      this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
      this.dataGridViewTextBoxColumn2.ReadOnly = true;
      // 
      // dataGridViewTextBoxColumn3
      // 
      this.dataGridViewTextBoxColumn3.HeaderText = "Cycle Begin";
      this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
      this.dataGridViewTextBoxColumn3.ReadOnly = true;
      // 
      // dataGridViewTextBoxColumn4
      // 
      this.dataGridViewTextBoxColumn4.HeaderText = "Source Directory";
      this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
      this.dataGridViewTextBoxColumn4.ReadOnly = true;
      // 
      // dataGridViewTextBoxColumn5
      // 
      this.dataGridViewTextBoxColumn5.HeaderText = "Target Directory";
      this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
      this.dataGridViewTextBoxColumn5.ReadOnly = true;
      // 
      // dataGridViewTextBoxColumn6
      // 
      this.dataGridViewTextBoxColumn6.HeaderText = "Stamping DateTime";
      this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
      this.dataGridViewTextBoxColumn6.ReadOnly = true;
      // 
      // dataGridViewTextBoxColumn7
      // 
      this.dataGridViewTextBoxColumn7.HeaderText = "Field";
      this.dataGridViewTextBoxColumn7.MinimumWidth = 50;
      this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
      this.dataGridViewTextBoxColumn7.ReadOnly = true;
      // 
      // dataGridViewTextBoxColumn8
      // 
      dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.dataGridViewTextBoxColumn8.DefaultCellStyle = dataGridViewCellStyle10;
      this.dataGridViewTextBoxColumn8.HeaderText = "Value";
      this.dataGridViewTextBoxColumn8.MinimumWidth = 50;
      this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
      // 
      // dataGridViewTextBoxColumn9
      // 
      this.dataGridViewTextBoxColumn9.HeaderText = "Stamping Id";
      this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
      this.dataGridViewTextBoxColumn9.Visible = false;
      // 
      // dataGridViewTextBoxColumn10
      // 
      this.dataGridViewTextBoxColumn10.HeaderText = "Stamping Data Type";
      this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
      this.dataGridViewTextBoxColumn10.Visible = false;
      // 
      // stampingDetailsDataGridViewTextBoxColumn1
      // 
      this.stampingDetailsDataGridViewTextBoxColumn1.HeaderText = "File Name";
      this.stampingDetailsDataGridViewTextBoxColumn1.Name = "stampingDetailsDataGridViewTextBoxColumn1";
      this.stampingDetailsDataGridViewTextBoxColumn1.ReadOnly = true;
      this.stampingDetailsDataGridViewTextBoxColumn1.Visible = false;
      // 
      // stampingDetailsDataGridViewTextBoxColumn2
      // 
      this.stampingDetailsDataGridViewTextBoxColumn2.HeaderText = "File Path";
      this.stampingDetailsDataGridViewTextBoxColumn2.Name = "stampingDetailsDataGridViewTextBoxColumn2";
      this.stampingDetailsDataGridViewTextBoxColumn2.ReadOnly = true;
      this.stampingDetailsDataGridViewTextBoxColumn2.Visible = false;
      // 
      // operationStepLabel
      // 
      this.operationStepLabel.Location = new System.Drawing.Point(4, 320);
      this.operationStepLabel.Name = "operationStepLabel";
      this.operationStepLabel.Size = new System.Drawing.Size(80, 21);
      this.operationStepLabel.TabIndex = 51;
      this.operationStepLabel.Text = "Operation step";
      this.operationStepLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // operationStepTextBox
      // 
      this.operationStepTextBox.Location = new System.Drawing.Point(90, 321);
      this.operationStepTextBox.Name = "operationStepTextBox";
      this.operationStepTextBox.Size = new System.Drawing.Size(180, 20);
      this.operationStepTextBox.TabIndex = 50;
      this.operationStepTextBox.TextChanged += new System.EventHandler(this.operationStepTextBox_TextChanged);
      // 
      // SequenceControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.Controls.Add(this.panel);
      this.Name = "SequenceControl";
      this.Size = new System.Drawing.Size(428, 614);
      this.panel.ResumeLayout(false);
      this.panel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.stampingDetailsDataGridView)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    private System.Windows.Forms.Label kindLabel;
    private System.Windows.Forms.ComboBox kindComboBox;
    private System.Windows.Forms.DataGridViewTextBoxColumn StampingDateTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn TargetDirectory;
    private System.Windows.Forms.DataGridViewTextBoxColumn IsCycleEnd;
    private System.Windows.Forms.DataGridViewTextBoxColumn StampPosition;
    private System.Windows.Forms.DataGridViewTextBoxColumn IsoFileSourcePath;
    private System.Windows.Forms.DataGridViewTextBoxColumn IsoFileNameColumn;
    private System.Windows.Forms.Label stampingDetailsLabel;
    private System.Windows.Forms.DataGridViewTextBoxColumn stampingDetailsDataGridViewTextBoxColumn2;
    private System.Windows.Forms.DataGridViewTextBoxColumn stampingDetailsDataGridViewTextBoxColumn1;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private System.Windows.Forms.DataGridView stampingDetailsDataGridView;
    private System.Windows.Forms.Label orderLabel;
    private System.Windows.Forms.TextBox orderTextBox;
    private Lemoine.BaseControls.NullableTimeSpanPicker estimatedTimeTimeSpanPicker;
    private System.Windows.Forms.Label pathLbl;
    private System.Windows.Forms.TextBox pathTextBox;
    private System.Windows.Forms.Label stampingValueLbl;
    private System.Windows.Forms.DataGridViewTextBoxColumn stampingdatatype;
    private System.Windows.Forms.DataGridViewTextBoxColumn fieldid;
    private System.Windows.Forms.DataGridViewTextBoxColumn value;
    private System.Windows.Forms.DataGridViewTextBoxColumn field;
    private System.Windows.Forms.DataGridView dataGridView1;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button createBtn;
    private System.Windows.Forms.Label nameLbl;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.Label estimatedtimeLbl;
    private System.Windows.Forms.Label descriptionLbl;
    private System.Windows.Forms.Label cadModelLbl;
    private System.Windows.Forms.Label toolLbl;
    private System.Windows.Forms.Label operationLbl;
    private System.Windows.Forms.TextBox descriptionTextBox;
    private System.Windows.Forms.TextBox toolTextBox;
    private System.Windows.Forms.TextBox cadModelTextBox;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.TextBox operationTextBox;
    private System.Windows.Forms.Button cancelBtn;
    private System.Windows.Forms.Button resetBtn;
    private System.Windows.Forms.TextBox toolNumberTextBox;
    private System.Windows.Forms.Label toolNumberLabel;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
    private System.Windows.Forms.Label operationStepLabel;
    private System.Windows.Forms.TextBox operationStepTextBox;
  }
}
