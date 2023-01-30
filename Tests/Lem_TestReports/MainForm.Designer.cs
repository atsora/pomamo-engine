// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_TestReports
{
  partial class MainForm
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the form.
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
      this.components = new System.ComponentModel.Container();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.newButton = new System.Windows.Forms.ToolStripButton();
      this.openButton = new System.Windows.Forms.ToolStripButton();
      this.saveButton = new System.Windows.Forms.ToolStripButton();
      this.parametersGroupBox = new System.Windows.Forms.GroupBox();
      this.urlParamatersLabel = new System.Windows.Forms.Label();
      this.jdbcServerLabel = new System.Windows.Forms.Label();
      this.urlParametersTextBox = new System.Windows.Forms.TextBox();
      this.jdbcServerTextBox = new System.Windows.Forms.TextBox();
      this.changeViewer_comboBox = new System.Windows.Forms.ComboBox();
      this.autoDiffCheckBox = new System.Windows.Forms.CheckBox();
      this.parallelLabel = new System.Windows.Forms.Label();
      this.parallelNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.noneButton = new System.Windows.Forms.Button();
      this.allButton = new System.Windows.Forms.Button();
      this.runButton = new System.Windows.Forms.Button();
      this.formatComboBox = new System.Windows.Forms.ComboBox();
      this.formatLabel = new System.Windows.Forms.Label();
      this.refDirectoryButton = new System.Windows.Forms.Button();
      this.outDirectoryButton = new System.Windows.Forms.Button();
      this.refDirectoryTextBox = new System.Windows.Forms.TextBox();
      this.refDirectoryLabel = new System.Windows.Forms.Label();
      this.outDirectoryTextBox = new System.Windows.Forms.TextBox();
      this.outDirectoryLabel = new System.Windows.Forms.Label();
      this.viewerUrlTextBox = new System.Windows.Forms.TextBox();
      this.viewerUrlLabel = new System.Windows.Forms.Label();
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      this.selectColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.urlColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.viewColumn = new System.Windows.Forms.DataGridViewButtonColumn();
      this.messageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.actionColumn = new System.Windows.Forms.DataGridViewButtonColumn();
      this.diffColumn = new System.Windows.Forms.DataGridViewButtonColumn();
      this.copyColumn = new System.Windows.Forms.DataGridViewButtonColumn();
      this.statusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Duration = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.waitStatusTimer = new System.Windows.Forms.Timer(this.components);
      this.toolStrip1.SuspendLayout();
      this.parametersGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.parallelNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      this.SuspendLayout();
      // 
      // toolStrip1
      // 
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newButton,
            this.openButton,
            this.saveButton});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.toolStrip1.Size = new System.Drawing.Size(1521, 34);
      this.toolStrip1.TabIndex = 0;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // newButton
      // 
      this.newButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.newButton.Name = "newButton";
      this.newButton.Size = new System.Drawing.Size(51, 29);
      this.newButton.Text = "New";
      this.newButton.Click += new System.EventHandler(this.NewButtonClick);
      // 
      // openButton
      // 
      this.openButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.openButton.Name = "openButton";
      this.openButton.Size = new System.Drawing.Size(72, 29);
      this.openButton.Text = "Open...";
      this.openButton.Click += new System.EventHandler(this.OpenButtonClick);
      // 
      // saveButton
      // 
      this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.saveButton.Name = "saveButton";
      this.saveButton.Size = new System.Drawing.Size(65, 29);
      this.saveButton.Text = "Save...";
      this.saveButton.Click += new System.EventHandler(this.SaveButtonClick);
      // 
      // parametersGroupBox
      // 
      this.parametersGroupBox.Controls.Add(this.urlParamatersLabel);
      this.parametersGroupBox.Controls.Add(this.jdbcServerLabel);
      this.parametersGroupBox.Controls.Add(this.urlParametersTextBox);
      this.parametersGroupBox.Controls.Add(this.jdbcServerTextBox);
      this.parametersGroupBox.Controls.Add(this.changeViewer_comboBox);
      this.parametersGroupBox.Controls.Add(this.autoDiffCheckBox);
      this.parametersGroupBox.Controls.Add(this.parallelLabel);
      this.parametersGroupBox.Controls.Add(this.parallelNumericUpDown);
      this.parametersGroupBox.Controls.Add(this.noneButton);
      this.parametersGroupBox.Controls.Add(this.allButton);
      this.parametersGroupBox.Controls.Add(this.runButton);
      this.parametersGroupBox.Controls.Add(this.formatComboBox);
      this.parametersGroupBox.Controls.Add(this.formatLabel);
      this.parametersGroupBox.Controls.Add(this.refDirectoryButton);
      this.parametersGroupBox.Controls.Add(this.outDirectoryButton);
      this.parametersGroupBox.Controls.Add(this.refDirectoryTextBox);
      this.parametersGroupBox.Controls.Add(this.refDirectoryLabel);
      this.parametersGroupBox.Controls.Add(this.outDirectoryTextBox);
      this.parametersGroupBox.Controls.Add(this.outDirectoryLabel);
      this.parametersGroupBox.Controls.Add(this.viewerUrlTextBox);
      this.parametersGroupBox.Controls.Add(this.viewerUrlLabel);
      this.parametersGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.parametersGroupBox.Location = new System.Drawing.Point(0, 34);
      this.parametersGroupBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.parametersGroupBox.Name = "parametersGroupBox";
      this.parametersGroupBox.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.parametersGroupBox.Size = new System.Drawing.Size(1521, 252);
      this.parametersGroupBox.TabIndex = 1;
      this.parametersGroupBox.TabStop = false;
      this.parametersGroupBox.Text = "Global parameters";
      // 
      // urlParamatersLabel
      // 
      this.urlParamatersLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.urlParamatersLabel.Location = new System.Drawing.Point(882, 133);
      this.urlParamatersLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.urlParamatersLabel.Name = "urlParamatersLabel";
      this.urlParamatersLabel.Size = new System.Drawing.Size(184, 45);
      this.urlParamatersLabel.TabIndex = 21;
      this.urlParamatersLabel.Text = "Url parameters:";
      this.urlParamatersLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // jdbcServerLabel
      // 
      this.jdbcServerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.jdbcServerLabel.Location = new System.Drawing.Point(882, 83);
      this.jdbcServerLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.jdbcServerLabel.Name = "jdbcServerLabel";
      this.jdbcServerLabel.Size = new System.Drawing.Size(184, 45);
      this.jdbcServerLabel.TabIndex = 20;
      this.jdbcServerLabel.Text = "Jdbc server:";
      this.jdbcServerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // urlParametersTextBox
      // 
      this.urlParametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.urlParametersTextBox.Location = new System.Drawing.Point(1078, 140);
      this.urlParametersTextBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.urlParametersTextBox.Name = "urlParametersTextBox";
      this.urlParametersTextBox.Size = new System.Drawing.Size(423, 31);
      this.urlParametersTextBox.TabIndex = 19;
      // 
      // jdbcServerTextBox
      // 
      this.jdbcServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.jdbcServerTextBox.Location = new System.Drawing.Point(1078, 90);
      this.jdbcServerTextBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.jdbcServerTextBox.Name = "jdbcServerTextBox";
      this.jdbcServerTextBox.Size = new System.Drawing.Size(423, 31);
      this.jdbcServerTextBox.TabIndex = 18;
      // 
      // changeViewer_comboBox
      // 
      this.changeViewer_comboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.changeViewer_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.changeViewer_comboBox.FormattingEnabled = true;
      this.changeViewer_comboBox.Items.AddRange(new object[] {
            "PulseReporting"});
      this.changeViewer_comboBox.Location = new System.Drawing.Point(1269, 35);
      this.changeViewer_comboBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.changeViewer_comboBox.Name = "changeViewer_comboBox";
      this.changeViewer_comboBox.Size = new System.Drawing.Size(231, 33);
      this.changeViewer_comboBox.TabIndex = 17;
      this.changeViewer_comboBox.DropDown += new System.EventHandler(this.ChangeViewer_comboBoxDropDown);
      this.changeViewer_comboBox.SelectionChangeCommitted += new System.EventHandler(this.ChangeViewer_comboBoxSelectionChangeCommitted);
      // 
      // autoDiffCheckBox
      // 
      this.autoDiffCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.autoDiffCheckBox.Location = new System.Drawing.Point(900, 190);
      this.autoDiffCheckBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.autoDiffCheckBox.Name = "autoDiffCheckBox";
      this.autoDiffCheckBox.Size = new System.Drawing.Size(111, 47);
      this.autoDiffCheckBox.TabIndex = 15;
      this.autoDiffCheckBox.Text = "Auto diff";
      this.autoDiffCheckBox.UseVisualStyleBackColor = true;
      // 
      // parallelLabel
      // 
      this.parallelLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parallelLabel.Location = new System.Drawing.Point(1021, 190);
      this.parallelLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.parallelLabel.Name = "parallelLabel";
      this.parallelLabel.Size = new System.Drawing.Size(160, 45);
      this.parallelLabel.TabIndex = 14;
      this.parallelLabel.Text = "Simultaneous run:";
      this.parallelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // parallelNumericUpDown
      // 
      this.parallelNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parallelNumericUpDown.Location = new System.Drawing.Point(1189, 198);
      this.parallelNumericUpDown.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.parallelNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.parallelNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.parallelNumericUpDown.Name = "parallelNumericUpDown";
      this.parallelNumericUpDown.Size = new System.Drawing.Size(66, 31);
      this.parallelNumericUpDown.TabIndex = 13;
      this.parallelNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // noneButton
      // 
      this.noneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.noneButton.Location = new System.Drawing.Point(747, 190);
      this.noneButton.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.noneButton.Name = "noneButton";
      this.noneButton.Size = new System.Drawing.Size(71, 45);
      this.noneButton.TabIndex = 12;
      this.noneButton.Text = "None";
      this.noneButton.UseVisualStyleBackColor = true;
      this.noneButton.Click += new System.EventHandler(this.NoneButtonClick);
      // 
      // allButton
      // 
      this.allButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.allButton.Location = new System.Drawing.Point(829, 190);
      this.allButton.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.allButton.Name = "allButton";
      this.allButton.Size = new System.Drawing.Size(61, 45);
      this.allButton.TabIndex = 11;
      this.allButton.Text = "All";
      this.allButton.UseVisualStyleBackColor = true;
      this.allButton.Click += new System.EventHandler(this.AllButtonClick);
      // 
      // runButton
      // 
      this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.runButton.Location = new System.Drawing.Point(1267, 190);
      this.runButton.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.runButton.Name = "runButton";
      this.runButton.Size = new System.Drawing.Size(234, 45);
      this.runButton.TabIndex = 10;
      this.runButton.Text = "Run selected tests";
      this.runButton.UseVisualStyleBackColor = true;
      this.runButton.Click += new System.EventHandler(this.RunButtonClick);
      // 
      // formatComboBox
      // 
      this.formatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.formatComboBox.FormattingEnabled = true;
      this.formatComboBox.Items.AddRange(new object[] {
            "PDF",
            "Postscript"});
      this.formatComboBox.Location = new System.Drawing.Point(159, 192);
      this.formatComboBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.formatComboBox.Name = "formatComboBox";
      this.formatComboBox.Size = new System.Drawing.Size(198, 33);
      this.formatComboBox.TabIndex = 9;
      // 
      // formatLabel
      // 
      this.formatLabel.Location = new System.Drawing.Point(17, 185);
      this.formatLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.formatLabel.Name = "formatLabel";
      this.formatLabel.Size = new System.Drawing.Size(133, 45);
      this.formatLabel.TabIndex = 8;
      this.formatLabel.Text = "Format:";
      this.formatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // refDirectoryButton
      // 
      this.refDirectoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.refDirectoryButton.Location = new System.Drawing.Point(830, 133);
      this.refDirectoryButton.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.refDirectoryButton.Name = "refDirectoryButton";
      this.refDirectoryButton.Size = new System.Drawing.Size(40, 45);
      this.refDirectoryButton.TabIndex = 7;
      this.refDirectoryButton.Text = "...";
      this.refDirectoryButton.UseVisualStyleBackColor = true;
      this.refDirectoryButton.Click += new System.EventHandler(this.RefDirectoryButtonClick);
      // 
      // outDirectoryButton
      // 
      this.outDirectoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.outDirectoryButton.Location = new System.Drawing.Point(830, 83);
      this.outDirectoryButton.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.outDirectoryButton.Name = "outDirectoryButton";
      this.outDirectoryButton.Size = new System.Drawing.Size(40, 45);
      this.outDirectoryButton.TabIndex = 6;
      this.outDirectoryButton.Text = "...";
      this.outDirectoryButton.UseVisualStyleBackColor = true;
      this.outDirectoryButton.Click += new System.EventHandler(this.OutDirectoryButtonClick);
      // 
      // refDirectoryTextBox
      // 
      this.refDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.refDirectoryTextBox.Location = new System.Drawing.Point(159, 140);
      this.refDirectoryTextBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.refDirectoryTextBox.Name = "refDirectoryTextBox";
      this.refDirectoryTextBox.Size = new System.Drawing.Size(659, 31);
      this.refDirectoryTextBox.TabIndex = 5;
      this.refDirectoryTextBox.Text = "C:\\Devel\\pulsetests\\FunctionalTests\\ref\\BirtReports";
      // 
      // refDirectoryLabel
      // 
      this.refDirectoryLabel.Location = new System.Drawing.Point(17, 133);
      this.refDirectoryLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.refDirectoryLabel.Name = "refDirectoryLabel";
      this.refDirectoryLabel.Size = new System.Drawing.Size(133, 45);
      this.refDirectoryLabel.TabIndex = 4;
      this.refDirectoryLabel.Text = "REF directory:";
      this.refDirectoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // outDirectoryTextBox
      // 
      this.outDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.outDirectoryTextBox.Location = new System.Drawing.Point(159, 90);
      this.outDirectoryTextBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.outDirectoryTextBox.Name = "outDirectoryTextBox";
      this.outDirectoryTextBox.Size = new System.Drawing.Size(659, 31);
      this.outDirectoryTextBox.TabIndex = 3;
      this.outDirectoryTextBox.Text = "C:\\Devel\\pulsetests\\FunctionalTests\\out\\BirtReports";
      // 
      // outDirectoryLabel
      // 
      this.outDirectoryLabel.Location = new System.Drawing.Point(17, 83);
      this.outDirectoryLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.outDirectoryLabel.Name = "outDirectoryLabel";
      this.outDirectoryLabel.Size = new System.Drawing.Size(133, 45);
      this.outDirectoryLabel.TabIndex = 2;
      this.outDirectoryLabel.Text = "OUT directory:";
      this.outDirectoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // viewerUrlTextBox
      // 
      this.viewerUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.viewerUrlTextBox.Location = new System.Drawing.Point(159, 38);
      this.viewerUrlTextBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.viewerUrlTextBox.Name = "viewerUrlTextBox";
      this.viewerUrlTextBox.Size = new System.Drawing.Size(1097, 31);
      this.viewerUrlTextBox.TabIndex = 1;
      this.viewerUrlTextBox.Text = "http://lctr:8080/pulsereporting/";
      // 
      // viewerUrlLabel
      // 
      this.viewerUrlLabel.Location = new System.Drawing.Point(17, 31);
      this.viewerUrlLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.viewerUrlLabel.Name = "viewerUrlLabel";
      this.viewerUrlLabel.Size = new System.Drawing.Size(133, 45);
      this.viewerUrlLabel.TabIndex = 0;
      this.viewerUrlLabel.Text = "Viewer URL:";
      this.viewerUrlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // dataGridView1
      // 
      this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.selectColumn,
            this.nameColumn,
            this.urlColumn,
            this.viewColumn,
            this.messageColumn,
            this.actionColumn,
            this.diffColumn,
            this.copyColumn,
            this.statusColumn,
            this.Duration});
      this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView1.Location = new System.Drawing.Point(0, 286);
      this.dataGridView1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.RowHeadersWidth = 62;
      this.dataGridView1.Size = new System.Drawing.Size(1521, 486);
      this.dataGridView1.TabIndex = 2;
      this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1CellContentClick);
      this.dataGridView1.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.DataGridView1RowsAdded);
      // 
      // selectColumn
      // 
      this.selectColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.selectColumn.HeaderText = "";
      this.selectColumn.MinimumWidth = 15;
      this.selectColumn.Name = "selectColumn";
      this.selectColumn.Width = 27;
      // 
      // nameColumn
      // 
      this.nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.nameColumn.HeaderText = "Test name";
      this.nameColumn.MinimumWidth = 8;
      this.nameColumn.Name = "nameColumn";
      this.nameColumn.Width = 127;
      // 
      // urlColumn
      // 
      this.urlColumn.HeaderText = "URL";
      this.urlColumn.MinimumWidth = 8;
      this.urlColumn.Name = "urlColumn";
      // 
      // viewColumn
      // 
      this.viewColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.viewColumn.HeaderText = "View";
      this.viewColumn.MinimumWidth = 8;
      this.viewColumn.Name = "viewColumn";
      this.viewColumn.Text = "";
      this.viewColumn.Width = 55;
      // 
      // messageColumn
      // 
      this.messageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.messageColumn.HeaderText = "Message";
      this.messageColumn.MinimumWidth = 8;
      this.messageColumn.Name = "messageColumn";
      this.messageColumn.ReadOnly = true;
      this.messageColumn.Width = 118;
      // 
      // actionColumn
      // 
      this.actionColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.actionColumn.HeaderText = "Action";
      this.actionColumn.MinimumWidth = 10;
      this.actionColumn.Name = "actionColumn";
      this.actionColumn.Width = 69;
      // 
      // diffColumn
      // 
      this.diffColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.diffColumn.HeaderText = "Diff";
      this.diffColumn.MinimumWidth = 8;
      this.diffColumn.Name = "diffColumn";
      this.diffColumn.ReadOnly = true;
      this.diffColumn.Width = 47;
      // 
      // copyColumn
      // 
      this.copyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.copyColumn.HeaderText = "Out > Ref";
      this.copyColumn.MinimumWidth = 8;
      this.copyColumn.Name = "copyColumn";
      this.copyColumn.ReadOnly = true;
      this.copyColumn.Width = 95;
      // 
      // statusColumn
      // 
      this.statusColumn.HeaderText = "Status";
      this.statusColumn.MinimumWidth = 8;
      this.statusColumn.Name = "statusColumn";
      this.statusColumn.Visible = false;
      // 
      // Duration
      // 
      this.Duration.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.Duration.HeaderText = "Duration";
      this.Duration.MinimumWidth = 8;
      this.Duration.Name = "Duration";
      this.Duration.Width = 117;
      // 
      // openFileDialog
      // 
      this.openFileDialog.DefaultExt = "reportTests";
      this.openFileDialog.Filter = "Report tests|*.reportTests|All files|*.*";
      this.openFileDialog.Title = "Open report tests";
      // 
      // saveFileDialog
      // 
      this.saveFileDialog.DefaultExt = "reportTests";
      this.saveFileDialog.Filter = "Report tests|*.reportTests|All files|*.*";
      // 
      // folderBrowserDialog
      // 
      this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
      this.folderBrowserDialog.SelectedPath = "C:\\Devel\\pulsetests\\FunctionalTests";
      // 
      // waitStatusTimer
      // 
      this.waitStatusTimer.Tick += new System.EventHandler(this.WaitStatusTimerTick);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1521, 772);
      this.Controls.Add(this.dataGridView1);
      this.Controls.Add(this.parametersGroupBox);
      this.Controls.Add(this.toolStrip1);
      this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.Name = "MainForm";
      this.Text = "Lem_TestReports";
      this.Load += new System.EventHandler(this.MainFormLoad);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.parametersGroupBox.ResumeLayout(false);
      this.parametersGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.parallelNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    private System.Windows.Forms.ComboBox changeViewer_comboBox;
    private System.Windows.Forms.CheckBox autoDiffCheckBox;
    private System.Windows.Forms.Label parallelLabel;
    private System.Windows.Forms.NumericUpDown parallelNumericUpDown;
    private System.Windows.Forms.Timer waitStatusTimer;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private System.Windows.Forms.SaveFileDialog saveFileDialog;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.DataGridViewButtonColumn actionColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn messageColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn statusColumn;
    private System.Windows.Forms.Button allButton;
    private System.Windows.Forms.Button noneButton;
    private System.Windows.Forms.DataGridViewButtonColumn copyColumn;
    private System.Windows.Forms.DataGridViewButtonColumn diffColumn;
    private System.Windows.Forms.DataGridViewButtonColumn viewColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn urlColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn selectColumn;
    private System.Windows.Forms.DataGridView dataGridView1;
    private System.Windows.Forms.Button runButton;
    private System.Windows.Forms.Label formatLabel;
    private System.Windows.Forms.ComboBox formatComboBox;
    private System.Windows.Forms.TextBox viewerUrlTextBox;
    private System.Windows.Forms.Label outDirectoryLabel;
    private System.Windows.Forms.TextBox outDirectoryTextBox;
    private System.Windows.Forms.Label refDirectoryLabel;
    private System.Windows.Forms.TextBox refDirectoryTextBox;
    private System.Windows.Forms.Button outDirectoryButton;
    private System.Windows.Forms.Button refDirectoryButton;
    private System.Windows.Forms.Label viewerUrlLabel;
    private System.Windows.Forms.GroupBox parametersGroupBox;
    private System.Windows.Forms.ToolStripButton saveButton;
    private System.Windows.Forms.ToolStripButton openButton;
    private System.Windows.Forms.ToolStripButton newButton;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.DataGridViewTextBoxColumn Duration;
    private System.Windows.Forms.Label jdbcServerLabel;
    private System.Windows.Forms.TextBox urlParametersTextBox;
    private System.Windows.Forms.TextBox jdbcServerTextBox;
    private System.Windows.Forms.Label urlParamatersLabel;
  }
}
