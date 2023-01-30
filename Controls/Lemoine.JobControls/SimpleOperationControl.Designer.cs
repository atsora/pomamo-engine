// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class SimpleOperationControl
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
      this.panel = new System.Windows.Forms.Panel();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.nameLbl = new System.Windows.Forms.Label();
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.lockCheckBox = new System.Windows.Forms.CheckBox();
      this.saveBtn = new System.Windows.Forms.Button();
      this.createBtn = new System.Windows.Forms.Button();
      this.cancelBtn = new System.Windows.Forms.Button();
      this.resetBtn = new System.Windows.Forms.Button();
      this.codeLbl = new System.Windows.Forms.Label();
      this.machineFilterSelection = new Lemoine.DataReferenceControls.MachineFilterSelection();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.machineFilterLabel = new System.Windows.Forms.Label();
      this.codeTextBox = new System.Windows.Forms.TextBox();
      this.documentLinkLbl = new System.Windows.Forms.Label();
      this.quantityTextBox = new System.Windows.Forms.TextBox();
      this.unloadingTimeLabel = new System.Windows.Forms.Label();
      this.typeLbl = new System.Windows.Forms.Label();
      this.typeComboBox = new System.Windows.Forms.ComboBox();
      this.quantityLbl = new System.Windows.Forms.Label();
      this.unloadingTimeSpanPicker = new Lemoine.BaseControls.NullableTimeSpanPicker();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.documentLinkBtn = new System.Windows.Forms.Button();
      this.documentLinkTextBox = new System.Windows.Forms.TextBox();
      this.weightTextBox = new System.Windows.Forms.TextBox();
      this.loadingTimeSpanPicker = new Lemoine.BaseControls.NullableTimeSpanPicker();
      this.weightLbl = new System.Windows.Forms.Label();
      this.loadingTimeLabel = new System.Windows.Forms.Label();
      this.machiningTimeSpanPicker = new Lemoine.BaseControls.NullableTimeSpanPicker();
      this.teardownTimeSpanPicker = new Lemoine.BaseControls.NullableTimeSpanPicker();
      this.estimatedMachiningHoursLbl = new System.Windows.Forms.Label();
      this.setupTimeSpanPicker = new Lemoine.BaseControls.NullableTimeSpanPicker();
      this.estimatedSetupHoursLbl = new System.Windows.Forms.Label();
      this.estimatedTearDownhoursLbl = new System.Windows.Forms.Label();
      this.checkBoxArchive = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.panel.SuspendLayout();
      this.baseLayout.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel
      // 
      this.panel.AutoScroll = true;
      this.panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panel.Controls.Add(this.baseLayout);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(341, 575);
      this.panel.TabIndex = 35;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 149F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.nameLbl, 0, 0);
      this.baseLayout.Controls.Add(this.tableLayoutPanel, 0, 16);
      this.baseLayout.Controls.Add(this.lockCheckBox, 1, 12);
      this.baseLayout.Controls.Add(this.saveBtn, 2, 14);
      this.baseLayout.Controls.Add(this.createBtn, 2, 15);
      this.baseLayout.Controls.Add(this.cancelBtn, 1, 15);
      this.baseLayout.Controls.Add(this.resetBtn, 1, 14);
      this.baseLayout.Controls.Add(this.codeLbl, 0, 1);
      this.baseLayout.Controls.Add(this.machineFilterSelection, 1, 11);
      this.baseLayout.Controls.Add(this.nameTextBox, 1, 0);
      this.baseLayout.Controls.Add(this.machineFilterLabel, 0, 11);
      this.baseLayout.Controls.Add(this.codeTextBox, 1, 1);
      this.baseLayout.Controls.Add(this.documentLinkLbl, 0, 2);
      this.baseLayout.Controls.Add(this.quantityTextBox, 1, 9);
      this.baseLayout.Controls.Add(this.unloadingTimeLabel, 0, 8);
      this.baseLayout.Controls.Add(this.typeLbl, 0, 10);
      this.baseLayout.Controls.Add(this.typeComboBox, 1, 10);
      this.baseLayout.Controls.Add(this.quantityLbl, 0, 9);
      this.baseLayout.Controls.Add(this.unloadingTimeSpanPicker, 1, 8);
      this.baseLayout.Controls.Add(this.tableLayoutPanel2, 1, 2);
      this.baseLayout.Controls.Add(this.weightTextBox, 1, 3);
      this.baseLayout.Controls.Add(this.loadingTimeSpanPicker, 1, 7);
      this.baseLayout.Controls.Add(this.weightLbl, 0, 3);
      this.baseLayout.Controls.Add(this.loadingTimeLabel, 0, 7);
      this.baseLayout.Controls.Add(this.machiningTimeSpanPicker, 1, 4);
      this.baseLayout.Controls.Add(this.teardownTimeSpanPicker, 1, 6);
      this.baseLayout.Controls.Add(this.estimatedMachiningHoursLbl, 0, 4);
      this.baseLayout.Controls.Add(this.setupTimeSpanPicker, 1, 5);
      this.baseLayout.Controls.Add(this.estimatedSetupHoursLbl, 0, 5);
      this.baseLayout.Controls.Add(this.estimatedTearDownhoursLbl, 0, 6);
      this.baseLayout.Controls.Add(this.checkBoxArchive, 1, 13);
      this.baseLayout.Controls.Add(this.label1, 0, 13);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 17;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(341, 575);
      this.baseLayout.TabIndex = 118;
      // 
      // nameLbl
      // 
      this.nameLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nameLbl.Location = new System.Drawing.Point(3, 0);
      this.nameLbl.Name = "nameLbl";
      this.nameLbl.Size = new System.Drawing.Size(143, 26);
      this.nameLbl.TabIndex = 45;
      this.nameLbl.Text = "Name";
      this.nameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.AutoSize = true;
      this.tableLayoutPanel.ColumnCount = 1;
      this.baseLayout.SetColumnSpan(this.tableLayoutPanel, 3);
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(3, 515);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
      this.tableLayoutPanel.RowCount = 1;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
      this.tableLayoutPanel.Size = new System.Drawing.Size(335, 57);
      this.tableLayoutPanel.TabIndex = 36;
      // 
      // lockCheckBox
      // 
      this.lockCheckBox.AutoSize = true;
      this.baseLayout.SetColumnSpan(this.lockCheckBox, 2);
      this.lockCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lockCheckBox.Location = new System.Drawing.Point(152, 399);
      this.lockCheckBox.Name = "lockCheckBox";
      this.lockCheckBox.Size = new System.Drawing.Size(186, 20);
      this.lockCheckBox.TabIndex = 117;
      this.lockCheckBox.Text = "Auto update lock";
      this.lockCheckBox.UseVisualStyleBackColor = true;
      this.lockCheckBox.CheckedChanged += new System.EventHandler(this.lockCheckBox_CheckedChanged);
      // 
      // saveBtn
      // 
      this.saveBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.saveBtn.Location = new System.Drawing.Point(248, 451);
      this.saveBtn.Name = "saveBtn";
      this.saveBtn.Size = new System.Drawing.Size(90, 26);
      this.saveBtn.TabIndex = 47;
      this.saveBtn.Text = "Save";
      this.saveBtn.UseVisualStyleBackColor = true;
      this.saveBtn.Click += new System.EventHandler(this.SaveBtnClick);
      // 
      // createBtn
      // 
      this.createBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.createBtn.Location = new System.Drawing.Point(248, 483);
      this.createBtn.Name = "createBtn";
      this.createBtn.Size = new System.Drawing.Size(90, 26);
      this.createBtn.TabIndex = 110;
      this.createBtn.Text = "Create";
      this.createBtn.UseVisualStyleBackColor = true;
      this.createBtn.Click += new System.EventHandler(this.CreateBtnClick);
      // 
      // cancelBtn
      // 
      this.cancelBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cancelBtn.Location = new System.Drawing.Point(152, 483);
      this.cancelBtn.Name = "cancelBtn";
      this.cancelBtn.Size = new System.Drawing.Size(90, 26);
      this.cancelBtn.TabIndex = 53;
      this.cancelBtn.Text = "Cancel";
      this.cancelBtn.UseVisualStyleBackColor = true;
      this.cancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
      // 
      // resetBtn
      // 
      this.resetBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.resetBtn.Location = new System.Drawing.Point(152, 451);
      this.resetBtn.Name = "resetBtn";
      this.resetBtn.Size = new System.Drawing.Size(90, 26);
      this.resetBtn.TabIndex = 100;
      this.resetBtn.Text = "Reset";
      this.resetBtn.UseVisualStyleBackColor = true;
      this.resetBtn.Click += new System.EventHandler(this.ResetBtnClick);
      // 
      // codeLbl
      // 
      this.codeLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.codeLbl.Location = new System.Drawing.Point(3, 26);
      this.codeLbl.Name = "codeLbl";
      this.codeLbl.Size = new System.Drawing.Size(143, 26);
      this.codeLbl.TabIndex = 48;
      this.codeLbl.Text = "Code";
      this.codeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // machineFilterSelection
      // 
      this.baseLayout.SetColumnSpan(this.machineFilterSelection, 2);
      this.machineFilterSelection.DisplayedProperty = "SelectionText";
      this.machineFilterSelection.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineFilterSelection.Location = new System.Drawing.Point(152, 289);
      this.machineFilterSelection.Name = "machineFilterSelection";
      this.machineFilterSelection.Nullable = true;
      this.machineFilterSelection.SelectedMachineFilter = null;
      this.machineFilterSelection.Size = new System.Drawing.Size(186, 104);
      this.machineFilterSelection.TabIndex = 116;
      this.machineFilterSelection.AfterSelect += new System.EventHandler(this.machineFilterSelection_AfterSelect);
      // 
      // nameTextBox
      // 
      this.baseLayout.SetColumnSpan(this.nameTextBox, 2);
      this.nameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nameTextBox.Location = new System.Drawing.Point(152, 3);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(186, 20);
      this.nameTextBox.TabIndex = 35;
      this.nameTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // machineFilterLabel
      // 
      this.machineFilterLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineFilterLabel.Location = new System.Drawing.Point(3, 286);
      this.machineFilterLabel.Name = "machineFilterLabel";
      this.machineFilterLabel.Size = new System.Drawing.Size(143, 110);
      this.machineFilterLabel.TabIndex = 115;
      this.machineFilterLabel.Text = "Machine filter";
      this.machineFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // codeTextBox
      // 
      this.baseLayout.SetColumnSpan(this.codeTextBox, 2);
      this.codeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.codeTextBox.Location = new System.Drawing.Point(152, 29);
      this.codeTextBox.Name = "codeTextBox";
      this.codeTextBox.Size = new System.Drawing.Size(186, 20);
      this.codeTextBox.TabIndex = 37;
      this.codeTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // documentLinkLbl
      // 
      this.documentLinkLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.documentLinkLbl.Location = new System.Drawing.Point(3, 52);
      this.documentLinkLbl.Name = "documentLinkLbl";
      this.documentLinkLbl.Size = new System.Drawing.Size(143, 26);
      this.documentLinkLbl.TabIndex = 49;
      this.documentLinkLbl.Text = "Document Link";
      this.documentLinkLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // quantityTextBox
      // 
      this.baseLayout.SetColumnSpan(this.quantityTextBox, 2);
      this.quantityTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.quantityTextBox.Location = new System.Drawing.Point(152, 237);
      this.quantityTextBox.Name = "quantityTextBox";
      this.quantityTextBox.Size = new System.Drawing.Size(186, 20);
      this.quantityTextBox.TabIndex = 80;
      this.quantityTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.quantityTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // unloadingTimeLabel
      // 
      this.unloadingTimeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.unloadingTimeLabel.Location = new System.Drawing.Point(3, 208);
      this.unloadingTimeLabel.Name = "unloadingTimeLabel";
      this.unloadingTimeLabel.Size = new System.Drawing.Size(143, 26);
      this.unloadingTimeLabel.TabIndex = 113;
      this.unloadingTimeLabel.Text = "Estimated Unloading Time";
      this.unloadingTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // typeLbl
      // 
      this.typeLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.typeLbl.Location = new System.Drawing.Point(3, 260);
      this.typeLbl.Name = "typeLbl";
      this.typeLbl.Size = new System.Drawing.Size(143, 26);
      this.typeLbl.TabIndex = 46;
      this.typeLbl.Text = "Type";
      this.typeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // typeComboBox
      // 
      this.baseLayout.SetColumnSpan(this.typeComboBox, 2);
      this.typeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.typeComboBox.FormattingEnabled = true;
      this.typeComboBox.Location = new System.Drawing.Point(152, 263);
      this.typeComboBox.Name = "typeComboBox";
      this.typeComboBox.Size = new System.Drawing.Size(186, 21);
      this.typeComboBox.TabIndex = 90;
      this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // quantityLbl
      // 
      this.quantityLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.quantityLbl.Location = new System.Drawing.Point(3, 234);
      this.quantityLbl.Name = "quantityLbl";
      this.quantityLbl.Size = new System.Drawing.Size(143, 26);
      this.quantityLbl.TabIndex = 56;
      this.quantityLbl.Text = "Quantity";
      this.quantityLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // unloadingTimeSpanPicker
      // 
      this.baseLayout.SetColumnSpan(this.unloadingTimeSpanPicker, 2);
      this.unloadingTimeSpanPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.unloadingTimeSpanPicker.Location = new System.Drawing.Point(152, 211);
      this.unloadingTimeSpanPicker.Name = "unloadingTimeSpanPicker";
      this.unloadingTimeSpanPicker.Size = new System.Drawing.Size(186, 20);
      this.unloadingTimeSpanPicker.TabIndex = 114;
      this.unloadingTimeSpanPicker.Value = null;
      this.unloadingTimeSpanPicker.ValueAsHours = null;
      this.unloadingTimeSpanPicker.ValueChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 2;
      this.baseLayout.SetColumnSpan(this.tableLayoutPanel2, 2);
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
      this.tableLayoutPanel2.Controls.Add(this.documentLinkBtn, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.documentLinkTextBox, 0, 0);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(149, 52);
      this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 1;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(192, 26);
      this.tableLayoutPanel2.TabIndex = 50;
      // 
      // documentLinkBtn
      // 
      this.documentLinkBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.documentLinkBtn.Location = new System.Drawing.Point(159, 3);
      this.documentLinkBtn.Name = "documentLinkBtn";
      this.documentLinkBtn.Size = new System.Drawing.Size(30, 20);
      this.documentLinkBtn.TabIndex = 55;
      this.documentLinkBtn.Text = "...";
      this.documentLinkBtn.UseVisualStyleBackColor = true;
      this.documentLinkBtn.Click += new System.EventHandler(this.DocumentLinkBtnClick);
      // 
      // documentLinkTextBox
      // 
      this.documentLinkTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.documentLinkTextBox.Location = new System.Drawing.Point(3, 3);
      this.documentLinkTextBox.Name = "documentLinkTextBox";
      this.documentLinkTextBox.Size = new System.Drawing.Size(150, 20);
      this.documentLinkTextBox.TabIndex = 38;
      this.documentLinkTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      this.documentLinkTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DocumentLinkTextBoxKeyDown);
      // 
      // weightTextBox
      // 
      this.baseLayout.SetColumnSpan(this.weightTextBox, 2);
      this.weightTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.weightTextBox.Location = new System.Drawing.Point(152, 81);
      this.weightTextBox.Name = "weightTextBox";
      this.weightTextBox.Size = new System.Drawing.Size(186, 20);
      this.weightTextBox.TabIndex = 39;
      this.weightTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.weightTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // loadingTimeSpanPicker
      // 
      this.baseLayout.SetColumnSpan(this.loadingTimeSpanPicker, 2);
      this.loadingTimeSpanPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.loadingTimeSpanPicker.Location = new System.Drawing.Point(152, 185);
      this.loadingTimeSpanPicker.Name = "loadingTimeSpanPicker";
      this.loadingTimeSpanPicker.Size = new System.Drawing.Size(186, 20);
      this.loadingTimeSpanPicker.TabIndex = 112;
      this.loadingTimeSpanPicker.Value = null;
      this.loadingTimeSpanPicker.ValueAsHours = null;
      this.loadingTimeSpanPicker.ValueChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // weightLbl
      // 
      this.weightLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.weightLbl.Location = new System.Drawing.Point(3, 78);
      this.weightLbl.Name = "weightLbl";
      this.weightLbl.Size = new System.Drawing.Size(143, 26);
      this.weightLbl.TabIndex = 54;
      this.weightLbl.Text = "Weight";
      this.weightLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // loadingTimeLabel
      // 
      this.loadingTimeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.loadingTimeLabel.Location = new System.Drawing.Point(3, 182);
      this.loadingTimeLabel.Name = "loadingTimeLabel";
      this.loadingTimeLabel.Size = new System.Drawing.Size(143, 26);
      this.loadingTimeLabel.TabIndex = 111;
      this.loadingTimeLabel.Text = "Estimated Loading Time";
      this.loadingTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // machiningTimeSpanPicker
      // 
      this.baseLayout.SetColumnSpan(this.machiningTimeSpanPicker, 2);
      this.machiningTimeSpanPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machiningTimeSpanPicker.Location = new System.Drawing.Point(152, 107);
      this.machiningTimeSpanPicker.Name = "machiningTimeSpanPicker";
      this.machiningTimeSpanPicker.Size = new System.Drawing.Size(186, 20);
      this.machiningTimeSpanPicker.TabIndex = 58;
      this.machiningTimeSpanPicker.Value = null;
      this.machiningTimeSpanPicker.ValueAsHours = null;
      this.machiningTimeSpanPicker.ValueChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // teardownTimeSpanPicker
      // 
      this.baseLayout.SetColumnSpan(this.teardownTimeSpanPicker, 2);
      this.teardownTimeSpanPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.teardownTimeSpanPicker.Location = new System.Drawing.Point(152, 159);
      this.teardownTimeSpanPicker.Name = "teardownTimeSpanPicker";
      this.teardownTimeSpanPicker.Size = new System.Drawing.Size(186, 20);
      this.teardownTimeSpanPicker.TabIndex = 78;
      this.teardownTimeSpanPicker.Value = null;
      this.teardownTimeSpanPicker.ValueAsHours = null;
      this.teardownTimeSpanPicker.ValueChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // estimatedMachiningHoursLbl
      // 
      this.estimatedMachiningHoursLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.estimatedMachiningHoursLbl.Location = new System.Drawing.Point(3, 104);
      this.estimatedMachiningHoursLbl.Name = "estimatedMachiningHoursLbl";
      this.estimatedMachiningHoursLbl.Size = new System.Drawing.Size(143, 26);
      this.estimatedMachiningHoursLbl.TabIndex = 50;
      this.estimatedMachiningHoursLbl.Text = "Estimated Machining Time";
      this.estimatedMachiningHoursLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // setupTimeSpanPicker
      // 
      this.baseLayout.SetColumnSpan(this.setupTimeSpanPicker, 2);
      this.setupTimeSpanPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.setupTimeSpanPicker.Location = new System.Drawing.Point(152, 133);
      this.setupTimeSpanPicker.Name = "setupTimeSpanPicker";
      this.setupTimeSpanPicker.Size = new System.Drawing.Size(186, 20);
      this.setupTimeSpanPicker.TabIndex = 75;
      this.setupTimeSpanPicker.Value = null;
      this.setupTimeSpanPicker.ValueAsHours = null;
      this.setupTimeSpanPicker.ValueChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // estimatedSetupHoursLbl
      // 
      this.estimatedSetupHoursLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.estimatedSetupHoursLbl.Location = new System.Drawing.Point(3, 130);
      this.estimatedSetupHoursLbl.Name = "estimatedSetupHoursLbl";
      this.estimatedSetupHoursLbl.Size = new System.Drawing.Size(143, 26);
      this.estimatedSetupHoursLbl.TabIndex = 51;
      this.estimatedSetupHoursLbl.Text = "Estimated SetUp Time";
      this.estimatedSetupHoursLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // estimatedTearDownhoursLbl
      // 
      this.estimatedTearDownhoursLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.estimatedTearDownhoursLbl.Location = new System.Drawing.Point(3, 156);
      this.estimatedTearDownhoursLbl.Name = "estimatedTearDownhoursLbl";
      this.estimatedTearDownhoursLbl.Size = new System.Drawing.Size(143, 26);
      this.estimatedTearDownhoursLbl.TabIndex = 52;
      this.estimatedTearDownhoursLbl.Text = "Estimated Tear Down Time";
      this.estimatedTearDownhoursLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // checkBoxArchive
      // 
      this.checkBoxArchive.AutoSize = true;
      this.baseLayout.SetColumnSpan(this.checkBoxArchive, 2);
      this.checkBoxArchive.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxArchive.Location = new System.Drawing.Point(152, 425);
      this.checkBoxArchive.Name = "checkBoxArchive";
      this.checkBoxArchive.Size = new System.Drawing.Size(186, 20);
      this.checkBoxArchive.TabIndex = 119;
      this.checkBoxArchive.UseVisualStyleBackColor = true;
      this.checkBoxArchive.CheckedChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 422);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(143, 26);
      this.label1.TabIndex = 118;
      this.label1.Text = "Archived";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // SimpleOperationControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.Controls.Add(this.panel);
      this.Name = "SimpleOperationControl";
      this.Size = new System.Drawing.Size(341, 575);
      this.panel.ResumeLayout(false);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel2.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Label loadingTimeLabel;
    private Lemoine.BaseControls.NullableTimeSpanPicker loadingTimeSpanPicker;
    private System.Windows.Forms.Label unloadingTimeLabel;
    private Lemoine.BaseControls.NullableTimeSpanPicker unloadingTimeSpanPicker;
    private Lemoine.BaseControls.NullableTimeSpanPicker setupTimeSpanPicker;
    private Lemoine.BaseControls.NullableTimeSpanPicker teardownTimeSpanPicker;
    private Lemoine.BaseControls.NullableTimeSpanPicker machiningTimeSpanPicker;
    private System.Windows.Forms.Label quantityLbl;
    private System.Windows.Forms.TextBox quantityTextBox;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Button documentLinkBtn;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button resetBtn;
    private System.Windows.Forms.Button createBtn;
    private System.Windows.Forms.Label weightLbl;
    private System.Windows.Forms.TextBox weightTextBox;
    private System.Windows.Forms.Label nameLbl;
    private System.Windows.Forms.Label typeLbl;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.ComboBox typeComboBox;
    private System.Windows.Forms.Label codeLbl;
    private System.Windows.Forms.Label documentLinkLbl;
    private System.Windows.Forms.Label estimatedMachiningHoursLbl;
    private System.Windows.Forms.Label estimatedSetupHoursLbl;
    private System.Windows.Forms.Label estimatedTearDownhoursLbl;
    private System.Windows.Forms.TextBox codeTextBox;
    private System.Windows.Forms.TextBox documentLinkTextBox;
    private System.Windows.Forms.Button cancelBtn;
    private System.Windows.Forms.CheckBox lockCheckBox;
    private DataReferenceControls.MachineFilterSelection machineFilterSelection;
    private System.Windows.Forms.Label machineFilterLabel;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox checkBoxArchive;
  }
}
