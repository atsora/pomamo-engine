// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_ExcelDataImport
{
  partial class ExcelViewForm
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
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      this.dataSet1 = new System.Data.DataSet();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.loadExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.loadMacroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveMacroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.pruneStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.copyCellsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.setCellValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.selectSequenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.showSequenceRegionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.importIntoDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpTopicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.SelectSequenceRegionLabel = new System.Windows.Forms.Label();
      this.selectSequenceRegionTextbox = new System.Windows.Forms.TextBox();
      this.sheetNumberLabel = new System.Windows.Forms.Label();
      this.sheetNumberTextBox = new System.Windows.Forms.TextBox();
      this.excelFileNameLabel = new System.Windows.Forms.Label();
      this.excelFileNameTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.excelImportConfigFileComboBox = new System.Windows.Forms.ComboBox();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Location = new System.Drawing.Point(14, 182);
      this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.ReadOnly = true;
      this.dataGridView1.RowTemplate.Height = 23;
      this.dataGridView1.Size = new System.Drawing.Size(1060, 523);
      this.dataGridView1.TabIndex = 0;
      this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_MouseClick);
      this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_ColumnHeaderMouseClick);
      this.dataGridView1.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_RowHeaderMouseClick);
      this.dataGridView1.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dataGridView1_DeleteRow);
      // 
      // dataSet1
      // 
      this.dataSet1.DataSetName = "NewDataSet";
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.selectToolStripMenuItem,
            this.importToolStripMenuItem,
            this.helpToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
      this.menuStrip1.Size = new System.Drawing.Size(1088, 24);
      this.menuStrip1.TabIndex = 3;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadExcelToolStripMenuItem,
            this.saveExcelToolStripMenuItem,
            this.loadMacroToolStripMenuItem,
            this.saveMacroToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // loadExcelToolStripMenuItem
      // 
      this.loadExcelToolStripMenuItem.Name = "loadExcelToolStripMenuItem";
      this.loadExcelToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
      this.loadExcelToolStripMenuItem.Text = "Load Excel";
      this.loadExcelToolStripMenuItem.Click += new System.EventHandler(this.LoadExcelToolStripMenuItemClick);
      // 
      // saveExcelToolStripMenuItem
      // 
      this.saveExcelToolStripMenuItem.Enabled = false;
      this.saveExcelToolStripMenuItem.Name = "saveExcelToolStripMenuItem";
      this.saveExcelToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
      this.saveExcelToolStripMenuItem.Text = "Save as Excel";
      this.saveExcelToolStripMenuItem.Click += new System.EventHandler(this.SaveExcelToolStripMenuItemClick);
      // 
      // loadMacroToolStripMenuItem
      // 
      this.loadMacroToolStripMenuItem.Enabled = false;
      this.loadMacroToolStripMenuItem.Name = "loadMacroToolStripMenuItem";
      this.loadMacroToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
      this.loadMacroToolStripMenuItem.Text = "Load Macro";
      this.loadMacroToolStripMenuItem.Click += new System.EventHandler(this.LoadMacroToolStripMenuItemClick);
      // 
      // saveMacroToolStripMenuItem
      // 
      this.saveMacroToolStripMenuItem.Name = "saveMacroToolStripMenuItem";
      this.saveMacroToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
      this.saveMacroToolStripMenuItem.Text = "Save Macro";
      this.saveMacroToolStripMenuItem.Click += new System.EventHandler(this.SaveMacroToolStripMenuItemClick);
      // 
      // editToolStripMenuItem
      // 
      this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pruneStripMenuItem,
            this.copyCellsToolStripMenuItem,
            this.setCellValueToolStripMenuItem});
      this.editToolStripMenuItem.Name = "editToolStripMenuItem";
      this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
      this.editToolStripMenuItem.Text = "Edit";
      // 
      // pruneStripMenuItem
      // 
      this.pruneStripMenuItem.Name = "pruneStripMenuItem";
      this.pruneStripMenuItem.Size = new System.Drawing.Size(247, 22);
      this.pruneStripMenuItem.Text = "Prune Empty Rows and Columns";
      this.pruneStripMenuItem.Click += new System.EventHandler(this.PruneStripMenuItemClick);
      // 
      // copyCellsToolStripMenuItem
      // 
      this.copyCellsToolStripMenuItem.Name = "copyCellsToolStripMenuItem";
      this.copyCellsToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
      this.copyCellsToolStripMenuItem.Text = "Cell CopyTo";
      this.copyCellsToolStripMenuItem.Click += new System.EventHandler(this.CopyCellsToolStripMenuItemClick);
      // 
      // setCellValueToolStripMenuItem
      // 
      this.setCellValueToolStripMenuItem.Name = "setCellValueToolStripMenuItem";
      this.setCellValueToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
      this.setCellValueToolStripMenuItem.Text = "Set Cell Value";
      this.setCellValueToolStripMenuItem.Click += new System.EventHandler(this.SetCellValueToolStripMenuItemClick);
      // 
      // selectToolStripMenuItem
      // 
      this.selectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectSequenceToolStripMenuItem,
            this.toolStripSeparator2,
            this.showSequenceRegionToolStripMenuItem});
      this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
      this.selectToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
      this.selectToolStripMenuItem.Text = "Select";
      // 
      // selectSequenceToolStripMenuItem
      // 
      this.selectSequenceToolStripMenuItem.Name = "selectSequenceToolStripMenuItem";
      this.selectSequenceToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
      this.selectSequenceToolStripMenuItem.Text = "Select Sequence Region";
      this.selectSequenceToolStripMenuItem.Click += new System.EventHandler(this.SelectSequenceToolStripMenuItemClick);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(196, 6);
      // 
      // showSequenceRegionToolStripMenuItem
      // 
      this.showSequenceRegionToolStripMenuItem.Name = "showSequenceRegionToolStripMenuItem";
      this.showSequenceRegionToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
      this.showSequenceRegionToolStripMenuItem.Text = "Show Sequence Region";
      this.showSequenceRegionToolStripMenuItem.Click += new System.EventHandler(this.ShowSequenceRegionToolStripMenuItemClick);
      // 
      // importToolStripMenuItem
      // 
      this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importIntoDatabaseToolStripMenuItem});
      this.importToolStripMenuItem.Name = "importToolStripMenuItem";
      this.importToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
      this.importToolStripMenuItem.Text = "Import";
      // 
      // importIntoDatabaseToolStripMenuItem
      // 
      this.importIntoDatabaseToolStripMenuItem.Name = "importIntoDatabaseToolStripMenuItem";
      this.importIntoDatabaseToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
      this.importIntoDatabaseToolStripMenuItem.Text = "Import Excel File into Database";
      this.importIntoDatabaseToolStripMenuItem.Click += new System.EventHandler(this.ImportIntoDatabaseToolStripMenuItemClick);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpTopicsToolStripMenuItem});
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.helpToolStripMenuItem.Text = "Help";
      // 
      // helpTopicsToolStripMenuItem
      // 
      this.helpTopicsToolStripMenuItem.Name = "helpTopicsToolStripMenuItem";
      this.helpTopicsToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
      this.helpTopicsToolStripMenuItem.Text = "Help Topics";
      // 
      // SelectSequenceRegionLabel
      // 
      this.SelectSequenceRegionLabel.Location = new System.Drawing.Point(19, 148);
      this.SelectSequenceRegionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.SelectSequenceRegionLabel.Name = "SelectSequenceRegionLabel";
      this.SelectSequenceRegionLabel.Size = new System.Drawing.Size(162, 27);
      this.SelectSequenceRegionLabel.TabIndex = 5;
      this.SelectSequenceRegionLabel.Text = "Selected Sequence Region";
      this.SelectSequenceRegionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // selectSequenceRegionTextbox
      // 
      this.selectSequenceRegionTextbox.Location = new System.Drawing.Point(222, 148);
      this.selectSequenceRegionTextbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.selectSequenceRegionTextbox.Name = "selectSequenceRegionTextbox";
      this.selectSequenceRegionTextbox.ReadOnly = true;
      this.selectSequenceRegionTextbox.Size = new System.Drawing.Size(852, 23);
      this.selectSequenceRegionTextbox.TabIndex = 7;
      // 
      // sheetNumberLabel
      // 
      this.sheetNumberLabel.Location = new System.Drawing.Point(19, 113);
      this.sheetNumberLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.sheetNumberLabel.Name = "sheetNumberLabel";
      this.sheetNumberLabel.Size = new System.Drawing.Size(162, 27);
      this.sheetNumberLabel.TabIndex = 8;
      this.sheetNumberLabel.Text = "Import Sheet Number";
      this.sheetNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // sheetNumberTextBox
      // 
      this.sheetNumberTextBox.Location = new System.Drawing.Point(222, 113);
      this.sheetNumberTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.sheetNumberTextBox.Name = "sheetNumberTextBox";
      this.sheetNumberTextBox.Size = new System.Drawing.Size(106, 23);
      this.sheetNumberTextBox.TabIndex = 9;
      this.sheetNumberTextBox.Text = "0";
      this.sheetNumberTextBox.TextChanged += new System.EventHandler(this.SheetNumberTextBoxTextChanged);
      // 
      // excelFileNameLabel
      // 
      this.excelFileNameLabel.Location = new System.Drawing.Point(19, 77);
      this.excelFileNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.excelFileNameLabel.Name = "excelFileNameLabel";
      this.excelFileNameLabel.Size = new System.Drawing.Size(162, 27);
      this.excelFileNameLabel.TabIndex = 10;
      this.excelFileNameLabel.Text = "Excel File Name";
      this.excelFileNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // excelFileNameTextBox
      // 
      this.excelFileNameTextBox.Location = new System.Drawing.Point(222, 77);
      this.excelFileNameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.excelFileNameTextBox.Name = "excelFileNameTextBox";
      this.excelFileNameTextBox.ReadOnly = true;
      this.excelFileNameTextBox.Size = new System.Drawing.Size(852, 23);
      this.excelFileNameTextBox.TabIndex = 11;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(19, 38);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(162, 27);
      this.label1.TabIndex = 12;
      this.label1.Text = "Excel Import Config File";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // excelImportConfigFileComboBox
      // 
      this.excelImportConfigFileComboBox.FormattingEnabled = true;
      this.excelImportConfigFileComboBox.Location = new System.Drawing.Point(222, 38);
      this.excelImportConfigFileComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.excelImportConfigFileComboBox.Name = "excelImportConfigFileComboBox";
      this.excelImportConfigFileComboBox.Size = new System.Drawing.Size(852, 23);
      this.excelImportConfigFileComboBox.TabIndex = 14;
      // 
      // ExcelViewForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1088, 719);
      this.Controls.Add(this.excelImportConfigFileComboBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.excelFileNameTextBox);
      this.Controls.Add(this.excelFileNameLabel);
      this.Controls.Add(this.sheetNumberTextBox);
      this.Controls.Add(this.sheetNumberLabel);
      this.Controls.Add(this.selectSequenceRegionTextbox);
      this.Controls.Add(this.SelectSequenceRegionLabel);
      this.Controls.Add(this.dataGridView1);
      this.Controls.Add(this.menuStrip1);
      this.MainMenuStrip = this.menuStrip1;
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "ExcelViewForm";
      this.Text = "Excel Sequence Importer";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExcelViewForm_FormClosed);
      this.Load += new System.EventHandler(this.ExcelViewForm_Load);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).EndInit();
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    private System.Windows.Forms.ComboBox excelImportConfigFileComboBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox excelFileNameTextBox;
    private System.Windows.Forms.Label excelFileNameLabel;
    private System.Windows.Forms.TextBox sheetNumberTextBox;
    private System.Windows.Forms.Label sheetNumberLabel;
    private System.Windows.Forms.TextBox selectSequenceRegionTextbox;
    private System.Windows.Forms.Label SelectSequenceRegionLabel;
    private System.Windows.Forms.ToolStripMenuItem showSequenceRegionToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripMenuItem pruneStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem helpTopicsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem importIntoDatabaseToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem selectSequenceToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem setCellValueToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyCellsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveMacroToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem loadMacroToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveExcelToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem loadExcelToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.MenuStrip menuStrip1;
    
    private System.Windows.Forms.DataGridView dataGridView1;
    private System.Data.DataSet dataSet1;
        
  }
}
