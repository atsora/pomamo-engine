// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class PartControl
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
      this.checkBoxArchive = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.resetBtn = new System.Windows.Forms.Button();
      this.estimatedHoursTextBox = new System.Windows.Forms.TextBox();
      this.typeComboBox = new System.Windows.Forms.ComboBox();
      this.cancelBtn = new System.Windows.Forms.Button();
      this.codeTextBox = new System.Windows.Forms.TextBox();
      this.saveBtn = new System.Windows.Forms.Button();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.createBtn = new System.Windows.Forms.Button();
      this.nameLbl = new System.Windows.Forms.Label();
      this.codeLbl = new System.Windows.Forms.Label();
      this.documentLinkLbl = new System.Windows.Forms.Label();
      this.estimatedHoursLbl = new System.Windows.Forms.Label();
      this.typeLbl = new System.Windows.Forms.Label();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.documentLinkBtn = new System.Windows.Forms.Button();
      this.documentLinkTextBox = new System.Windows.Forms.TextBox();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.panel.SuspendLayout();
      this.baseLayout.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel
      // 
      this.panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panel.Controls.Add(this.baseLayout);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(296, 258);
      this.panel.TabIndex = 28;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 101F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.checkBoxArchive, 1, 5);
      this.baseLayout.Controls.Add(this.label1, 0, 5);
      this.baseLayout.Controls.Add(this.tableLayoutPanel, 0, 8);
      this.baseLayout.Controls.Add(this.resetBtn, 1, 6);
      this.baseLayout.Controls.Add(this.estimatedHoursTextBox, 1, 3);
      this.baseLayout.Controls.Add(this.typeComboBox, 1, 4);
      this.baseLayout.Controls.Add(this.cancelBtn, 1, 7);
      this.baseLayout.Controls.Add(this.codeTextBox, 1, 1);
      this.baseLayout.Controls.Add(this.saveBtn, 2, 6);
      this.baseLayout.Controls.Add(this.nameTextBox, 1, 0);
      this.baseLayout.Controls.Add(this.createBtn, 2, 7);
      this.baseLayout.Controls.Add(this.nameLbl, 0, 0);
      this.baseLayout.Controls.Add(this.codeLbl, 0, 1);
      this.baseLayout.Controls.Add(this.documentLinkLbl, 0, 2);
      this.baseLayout.Controls.Add(this.estimatedHoursLbl, 0, 3);
      this.baseLayout.Controls.Add(this.typeLbl, 0, 4);
      this.baseLayout.Controls.Add(this.tableLayoutPanel2, 1, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 9;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(296, 258);
      this.baseLayout.TabIndex = 46;
      // 
      // checkBoxArchive
      // 
      this.checkBoxArchive.AutoSize = true;
      this.baseLayout.SetColumnSpan(this.checkBoxArchive, 2);
      this.checkBoxArchive.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxArchive.Location = new System.Drawing.Point(104, 133);
      this.checkBoxArchive.Name = "checkBoxArchive";
      this.checkBoxArchive.Size = new System.Drawing.Size(189, 20);
      this.checkBoxArchive.TabIndex = 60;
      this.checkBoxArchive.UseVisualStyleBackColor = true;
      this.checkBoxArchive.CheckedChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 130);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(95, 26);
      this.label1.TabIndex = 59;
      this.label1.Text = "Archived";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.AutoSize = true;
      this.tableLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
      this.tableLayoutPanel.ColumnCount = 1;
      this.baseLayout.SetColumnSpan(this.tableLayoutPanel, 3);
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(3, 223);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
      this.tableLayoutPanel.RowCount = 1;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.Size = new System.Drawing.Size(290, 32);
      this.tableLayoutPanel.TabIndex = 29;
      // 
      // resetBtn
      // 
      this.resetBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.resetBtn.Location = new System.Drawing.Point(104, 159);
      this.resetBtn.Name = "resetBtn";
      this.resetBtn.Size = new System.Drawing.Size(91, 26);
      this.resetBtn.TabIndex = 33;
      this.resetBtn.Text = "Reset";
      this.resetBtn.UseVisualStyleBackColor = true;
      this.resetBtn.Click += new System.EventHandler(this.ResetBtnClick);
      // 
      // estimatedHoursTextBox
      // 
      this.baseLayout.SetColumnSpan(this.estimatedHoursTextBox, 2);
      this.estimatedHoursTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.estimatedHoursTextBox.Location = new System.Drawing.Point(104, 81);
      this.estimatedHoursTextBox.Name = "estimatedHoursTextBox";
      this.estimatedHoursTextBox.Size = new System.Drawing.Size(189, 20);
      this.estimatedHoursTextBox.TabIndex = 31;
      this.estimatedHoursTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.estimatedHoursTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // typeComboBox
      // 
      this.baseLayout.SetColumnSpan(this.typeComboBox, 2);
      this.typeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.typeComboBox.FormattingEnabled = true;
      this.typeComboBox.Location = new System.Drawing.Point(104, 107);
      this.typeComboBox.Name = "typeComboBox";
      this.typeComboBox.Size = new System.Drawing.Size(189, 21);
      this.typeComboBox.TabIndex = 32;
      this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // cancelBtn
      // 
      this.cancelBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cancelBtn.Location = new System.Drawing.Point(104, 191);
      this.cancelBtn.Name = "cancelBtn";
      this.cancelBtn.Size = new System.Drawing.Size(91, 26);
      this.cancelBtn.TabIndex = 41;
      this.cancelBtn.Text = "Cancel";
      this.cancelBtn.UseVisualStyleBackColor = true;
      this.cancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
      // 
      // codeTextBox
      // 
      this.baseLayout.SetColumnSpan(this.codeTextBox, 2);
      this.codeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.codeTextBox.Location = new System.Drawing.Point(104, 29);
      this.codeTextBox.Name = "codeTextBox";
      this.codeTextBox.Size = new System.Drawing.Size(189, 20);
      this.codeTextBox.TabIndex = 29;
      this.codeTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // saveBtn
      // 
      this.saveBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.saveBtn.Location = new System.Drawing.Point(201, 159);
      this.saveBtn.Name = "saveBtn";
      this.saveBtn.Size = new System.Drawing.Size(92, 26);
      this.saveBtn.TabIndex = 40;
      this.saveBtn.Text = "Save";
      this.saveBtn.UseVisualStyleBackColor = true;
      this.saveBtn.Click += new System.EventHandler(this.SaveBtnClick);
      // 
      // nameTextBox
      // 
      this.baseLayout.SetColumnSpan(this.nameTextBox, 2);
      this.nameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nameTextBox.Location = new System.Drawing.Point(104, 3);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(189, 20);
      this.nameTextBox.TabIndex = 28;
      this.nameTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // createBtn
      // 
      this.createBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.createBtn.Location = new System.Drawing.Point(201, 191);
      this.createBtn.Name = "createBtn";
      this.createBtn.Size = new System.Drawing.Size(92, 26);
      this.createBtn.TabIndex = 34;
      this.createBtn.Text = "Create";
      this.createBtn.UseVisualStyleBackColor = true;
      this.createBtn.Click += new System.EventHandler(this.CreateBtnClick);
      // 
      // nameLbl
      // 
      this.nameLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nameLbl.Location = new System.Drawing.Point(3, 0);
      this.nameLbl.Name = "nameLbl";
      this.nameLbl.Size = new System.Drawing.Size(95, 26);
      this.nameLbl.TabIndex = 35;
      this.nameLbl.Text = "Name";
      this.nameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // codeLbl
      // 
      this.codeLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.codeLbl.Location = new System.Drawing.Point(3, 26);
      this.codeLbl.Name = "codeLbl";
      this.codeLbl.Size = new System.Drawing.Size(95, 26);
      this.codeLbl.TabIndex = 36;
      this.codeLbl.Text = "Code";
      this.codeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // documentLinkLbl
      // 
      this.documentLinkLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.documentLinkLbl.Location = new System.Drawing.Point(3, 52);
      this.documentLinkLbl.Name = "documentLinkLbl";
      this.documentLinkLbl.Size = new System.Drawing.Size(95, 26);
      this.documentLinkLbl.TabIndex = 37;
      this.documentLinkLbl.Text = "Document link";
      this.documentLinkLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // estimatedHoursLbl
      // 
      this.estimatedHoursLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.estimatedHoursLbl.Location = new System.Drawing.Point(3, 78);
      this.estimatedHoursLbl.Name = "estimatedHoursLbl";
      this.estimatedHoursLbl.Size = new System.Drawing.Size(95, 26);
      this.estimatedHoursLbl.TabIndex = 38;
      this.estimatedHoursLbl.Text = "Estimated hours";
      this.estimatedHoursLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // typeLbl
      // 
      this.typeLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.typeLbl.Location = new System.Drawing.Point(3, 104);
      this.typeLbl.Name = "typeLbl";
      this.typeLbl.Size = new System.Drawing.Size(95, 26);
      this.typeLbl.TabIndex = 39;
      this.typeLbl.Text = "Type";
      this.typeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 2;
      this.baseLayout.SetColumnSpan(this.tableLayoutPanel2, 2);
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 39F));
      this.tableLayoutPanel2.Controls.Add(this.documentLinkBtn, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.documentLinkTextBox, 0, 0);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(101, 52);
      this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 1;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(195, 26);
      this.tableLayoutPanel2.TabIndex = 42;
      // 
      // documentLinkBtn
      // 
      this.documentLinkBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.documentLinkBtn.Location = new System.Drawing.Point(159, 3);
      this.documentLinkBtn.Name = "documentLinkBtn";
      this.documentLinkBtn.Size = new System.Drawing.Size(33, 20);
      this.documentLinkBtn.TabIndex = 45;
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
      this.documentLinkTextBox.TabIndex = 30;
      this.documentLinkTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      this.documentLinkTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DocumentLinkTextBoxKeyDown);
      // 
      // PartControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.Controls.Add(this.panel);
      this.Name = "PartControl";
      this.Size = new System.Drawing.Size(296, 258);
      this.panel.ResumeLayout(false);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel2.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Button documentLinkBtn;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button resetBtn;
    private System.Windows.Forms.Button createBtn;
    private System.Windows.Forms.Label nameLbl;
    private System.Windows.Forms.Label codeLbl;
    private System.Windows.Forms.Label documentLinkLbl;
    private System.Windows.Forms.Label estimatedHoursLbl;
    private System.Windows.Forms.Label typeLbl;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.TextBox codeTextBox;
    private System.Windows.Forms.TextBox documentLinkTextBox;
    private System.Windows.Forms.TextBox estimatedHoursTextBox;
    private System.Windows.Forms.ComboBox typeComboBox;
    private System.Windows.Forms.Button cancelBtn;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox checkBoxArchive;
  }
}
