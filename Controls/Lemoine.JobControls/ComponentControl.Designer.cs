// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class ComponentControl
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
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.documentLinkBtn = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.projectComboBox = new System.Windows.Forms.ComboBox();
      this.projectLbl = new System.Windows.Forms.Label();
      this.resetBtn = new System.Windows.Forms.Button();
      this.cancelBtn = new System.Windows.Forms.Button();
      this.typeComboBox = new System.Windows.Forms.ComboBox();
      this.estimatedHoursTextBox = new System.Windows.Forms.TextBox();
      this.documentLinkTextBox = new System.Windows.Forms.TextBox();
      this.codeTextBox = new System.Windows.Forms.TextBox();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.typeLbl = new System.Windows.Forms.Label();
      this.estimatedHoursLbl = new System.Windows.Forms.Label();
      this.documentLinkLbl = new System.Windows.Forms.Label();
      this.codeLbl = new System.Windows.Forms.Label();
      this.nameLbl = new System.Windows.Forms.Label();
      this.saveBtn = new System.Windows.Forms.Button();
      this.createBtn = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.panel.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel
      // 
      this.panel.BackColor = System.Drawing.SystemColors.Control;
      this.panel.Controls.Add(this.tableLayoutPanel);
      this.panel.Controls.Add(this.documentLinkBtn);
      this.panel.Controls.Add(this.panel1);
      this.panel.Controls.Add(this.projectComboBox);
      this.panel.Controls.Add(this.projectLbl);
      this.panel.Controls.Add(this.resetBtn);
      this.panel.Controls.Add(this.cancelBtn);
      this.panel.Controls.Add(this.typeComboBox);
      this.panel.Controls.Add(this.estimatedHoursTextBox);
      this.panel.Controls.Add(this.documentLinkTextBox);
      this.panel.Controls.Add(this.codeTextBox);
      this.panel.Controls.Add(this.nameTextBox);
      this.panel.Controls.Add(this.typeLbl);
      this.panel.Controls.Add(this.estimatedHoursLbl);
      this.panel.Controls.Add(this.documentLinkLbl);
      this.panel.Controls.Add(this.codeLbl);
      this.panel.Controls.Add(this.nameLbl);
      this.panel.Controls.Add(this.saveBtn);
      this.panel.Controls.Add(this.createBtn);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(363, 228);
      this.panel.TabIndex = 21;
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel.AutoSize = true;
      this.tableLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
      this.tableLayoutPanel.ColumnCount = 1;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.Location = new System.Drawing.Point(5, 194);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
      this.tableLayoutPanel.RowCount = 1;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.Size = new System.Drawing.Size(345, 19);
      this.tableLayoutPanel.TabIndex = 20;
      // 
      // documentLinkBtn
      // 
      this.documentLinkBtn.Location = new System.Drawing.Point(325, 53);
      this.documentLinkBtn.Name = "documentLinkBtn";
      this.documentLinkBtn.Size = new System.Drawing.Size(25, 23);
      this.documentLinkBtn.TabIndex = 45;
      this.documentLinkBtn.Text = "...";
      this.documentLinkBtn.UseVisualStyleBackColor = true;
      this.documentLinkBtn.Click += new System.EventHandler(this.DocumentLinkBtnClick);
      // 
      // panel1
      // 
      this.panel1.AutoSize = true;
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(363, 0);
      this.panel1.TabIndex = 19;
      // 
      // projectComboBox
      // 
      this.projectComboBox.FormattingEnabled = true;
      this.projectComboBox.Location = new System.Drawing.Point(170, 130);
      this.projectComboBox.Name = "projectComboBox";
      this.projectComboBox.Size = new System.Drawing.Size(180, 21);
      this.projectComboBox.TabIndex = 34;
      this.projectComboBox.TextUpdate += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // projectLbl
      // 
      this.projectLbl.Location = new System.Drawing.Point(5, 130);
      this.projectLbl.Name = "projectLbl";
      this.projectLbl.Size = new System.Drawing.Size(160, 21);
      this.projectLbl.TabIndex = 33;
      this.projectLbl.Text = "Project";
      this.projectLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // resetBtn
      // 
      this.resetBtn.Location = new System.Drawing.Point(170, 165);
      this.resetBtn.Name = "resetBtn";
      this.resetBtn.Size = new System.Drawing.Size(75, 23);
      this.resetBtn.TabIndex = 29;
      this.resetBtn.Text = "Reset";
      this.resetBtn.UseVisualStyleBackColor = true;
      this.resetBtn.Click += new System.EventHandler(this.ResetBtnClick);
      // 
      // cancelBtn
      // 
      this.cancelBtn.Location = new System.Drawing.Point(170, 165);
      this.cancelBtn.Name = "cancelBtn";
      this.cancelBtn.Size = new System.Drawing.Size(75, 23);
      this.cancelBtn.TabIndex = 31;
      this.cancelBtn.Text = "Cancel";
      this.cancelBtn.UseVisualStyleBackColor = true;
      this.cancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
      // 
      // typeComboBox
      // 
      this.typeComboBox.FormattingEnabled = true;
      this.typeComboBox.Location = new System.Drawing.Point(170, 105);
      this.typeComboBox.Name = "typeComboBox";
      this.typeComboBox.Size = new System.Drawing.Size(180, 21);
      this.typeComboBox.TabIndex = 28;
      this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // estimatedHoursTextBox
      // 
      this.estimatedHoursTextBox.Location = new System.Drawing.Point(170, 80);
      this.estimatedHoursTextBox.Name = "estimatedHoursTextBox";
      this.estimatedHoursTextBox.Size = new System.Drawing.Size(180, 20);
      this.estimatedHoursTextBox.TabIndex = 26;
      this.estimatedHoursTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.estimatedHoursTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // documentLinkTextBox
      // 
      this.documentLinkTextBox.Location = new System.Drawing.Point(170, 55);
      this.documentLinkTextBox.Name = "documentLinkTextBox";
      this.documentLinkTextBox.Size = new System.Drawing.Size(150, 20);
      this.documentLinkTextBox.TabIndex = 24;
      this.documentLinkTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      this.documentLinkTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DocumentLinkTextBoxKeyDown);
      // 
      // codeTextBox
      // 
      this.codeTextBox.Location = new System.Drawing.Point(170, 30);
      this.codeTextBox.Name = "codeTextBox";
      this.codeTextBox.Size = new System.Drawing.Size(180, 20);
      this.codeTextBox.TabIndex = 22;
      this.codeTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // nameTextBox
      // 
      this.nameTextBox.Location = new System.Drawing.Point(170, 5);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(180, 20);
      this.nameTextBox.TabIndex = 20;
      this.nameTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // typeLbl
      // 
      this.typeLbl.Location = new System.Drawing.Point(5, 105);
      this.typeLbl.Name = "typeLbl";
      this.typeLbl.Size = new System.Drawing.Size(160, 21);
      this.typeLbl.TabIndex = 27;
      this.typeLbl.Text = "Type";
      this.typeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // estimatedHoursLbl
      // 
      this.estimatedHoursLbl.Location = new System.Drawing.Point(5, 80);
      this.estimatedHoursLbl.Name = "estimatedHoursLbl";
      this.estimatedHoursLbl.Size = new System.Drawing.Size(160, 21);
      this.estimatedHoursLbl.TabIndex = 25;
      this.estimatedHoursLbl.Text = "Estimated hours";
      this.estimatedHoursLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // documentLinkLbl
      // 
      this.documentLinkLbl.Location = new System.Drawing.Point(5, 55);
      this.documentLinkLbl.Name = "documentLinkLbl";
      this.documentLinkLbl.Size = new System.Drawing.Size(160, 21);
      this.documentLinkLbl.TabIndex = 23;
      this.documentLinkLbl.Text = "Document link";
      this.documentLinkLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // codeLbl
      // 
      this.codeLbl.Location = new System.Drawing.Point(5, 30);
      this.codeLbl.Name = "codeLbl";
      this.codeLbl.Size = new System.Drawing.Size(160, 21);
      this.codeLbl.TabIndex = 21;
      this.codeLbl.Text = "Code";
      this.codeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // nameLbl
      // 
      this.nameLbl.Location = new System.Drawing.Point(5, 5);
      this.nameLbl.Name = "nameLbl";
      this.nameLbl.Size = new System.Drawing.Size(160, 21);
      this.nameLbl.TabIndex = 19;
      this.nameLbl.Text = "Name";
      this.nameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // saveBtn
      // 
      this.saveBtn.Location = new System.Drawing.Point(275, 165);
      this.saveBtn.Name = "saveBtn";
      this.saveBtn.Size = new System.Drawing.Size(75, 23);
      this.saveBtn.TabIndex = 30;
      this.saveBtn.Text = "Save";
      this.saveBtn.UseVisualStyleBackColor = true;
      this.saveBtn.Click += new System.EventHandler(this.SaveBtnClick);
      // 
      // createBtn
      // 
      this.createBtn.Location = new System.Drawing.Point(275, 165);
      this.createBtn.Name = "createBtn";
      this.createBtn.Size = new System.Drawing.Size(75, 23);
      this.createBtn.TabIndex = 32;
      this.createBtn.Text = "Create";
      this.createBtn.UseVisualStyleBackColor = true;
      this.createBtn.Click += new System.EventHandler(this.CreateBtnClick);
      // 
      // ComponentControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.Controls.Add(this.panel);
      this.Name = "ComponentControl";
      this.Size = new System.Drawing.Size(363, 228);
      this.panel.ResumeLayout(false);
      this.panel.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Button documentLinkBtn;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.ComboBox projectComboBox;
    private System.Windows.Forms.Label projectLbl;
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button resetBtn;
    private System.Windows.Forms.Button createBtn;
    private System.Windows.Forms.Button cancelBtn;
    private System.Windows.Forms.ComboBox typeComboBox;
    private System.Windows.Forms.TextBox estimatedHoursTextBox;
    private System.Windows.Forms.TextBox documentLinkTextBox;
    private System.Windows.Forms.TextBox codeTextBox;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.Label typeLbl;
    private System.Windows.Forms.Label estimatedHoursLbl;
    private System.Windows.Forms.Label documentLinkLbl;
    private System.Windows.Forms.Label codeLbl;
    private System.Windows.Forms.Label nameLbl;
    
  }
}
