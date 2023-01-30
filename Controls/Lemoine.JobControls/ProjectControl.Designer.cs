// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class ProjectControl
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
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.panel = new System.Windows.Forms.Panel();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.nameLbl = new System.Windows.Forms.Label();
      this.cancelBtn = new System.Windows.Forms.Button();
      this.resetBtn = new System.Windows.Forms.Button();
      this.codeLbl = new System.Windows.Forms.Label();
      this.documentLinkLbl = new System.Windows.Forms.Label();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.codeTextBox = new System.Windows.Forms.TextBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.documentLinkBtn = new System.Windows.Forms.Button();
      this.documentLinkTextBox = new System.Windows.Forms.TextBox();
      this.saveBtn = new System.Windows.Forms.Button();
      this.createBtn = new System.Windows.Forms.Button();
      this.checkBoxArchive = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.panel.SuspendLayout();
      this.baseLayout.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.AutoSize = true;
      this.tableLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
      this.tableLayoutPanel.ColumnCount = 1;
      this.baseLayout.SetColumnSpan(this.tableLayoutPanel, 3);
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(3, 171);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
      this.tableLayoutPanel.RowCount = 1;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.Size = new System.Drawing.Size(289, 204);
      this.tableLayoutPanel.TabIndex = 18;
      // 
      // panel
      // 
      this.panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panel.BackColor = System.Drawing.SystemColors.Control;
      this.panel.Controls.Add(this.baseLayout);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(295, 378);
      this.panel.TabIndex = 19;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.nameLbl, 0, 0);
      this.baseLayout.Controls.Add(this.tableLayoutPanel, 0, 6);
      this.baseLayout.Controls.Add(this.cancelBtn, 1, 5);
      this.baseLayout.Controls.Add(this.resetBtn, 1, 4);
      this.baseLayout.Controls.Add(this.codeLbl, 0, 1);
      this.baseLayout.Controls.Add(this.documentLinkLbl, 0, 2);
      this.baseLayout.Controls.Add(this.nameTextBox, 1, 0);
      this.baseLayout.Controls.Add(this.codeTextBox, 1, 1);
      this.baseLayout.Controls.Add(this.tableLayoutPanel2, 1, 2);
      this.baseLayout.Controls.Add(this.saveBtn, 2, 4);
      this.baseLayout.Controls.Add(this.createBtn, 2, 5);
      this.baseLayout.Controls.Add(this.checkBoxArchive, 1, 3);
      this.baseLayout.Controls.Add(this.label1, 0, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 7;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(295, 378);
      this.baseLayout.TabIndex = 46;
      // 
      // nameLbl
      // 
      this.nameLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nameLbl.Location = new System.Drawing.Point(3, 0);
      this.nameLbl.Name = "nameLbl";
      this.nameLbl.Size = new System.Drawing.Size(82, 26);
      this.nameLbl.TabIndex = 23;
      this.nameLbl.Text = "Name";
      this.nameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // cancelBtn
      // 
      this.cancelBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cancelBtn.Location = new System.Drawing.Point(91, 139);
      this.cancelBtn.Name = "cancelBtn";
      this.cancelBtn.Size = new System.Drawing.Size(97, 26);
      this.cancelBtn.TabIndex = 27;
      this.cancelBtn.Text = "Cancel";
      this.cancelBtn.UseVisualStyleBackColor = true;
      this.cancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
      // 
      // resetBtn
      // 
      this.resetBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.resetBtn.Location = new System.Drawing.Point(91, 107);
      this.resetBtn.Name = "resetBtn";
      this.resetBtn.Size = new System.Drawing.Size(97, 26);
      this.resetBtn.TabIndex = 21;
      this.resetBtn.Text = "Reset";
      this.resetBtn.UseVisualStyleBackColor = true;
      this.resetBtn.Click += new System.EventHandler(this.ResetBtnClick);
      // 
      // codeLbl
      // 
      this.codeLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.codeLbl.Location = new System.Drawing.Point(3, 26);
      this.codeLbl.Name = "codeLbl";
      this.codeLbl.Size = new System.Drawing.Size(82, 26);
      this.codeLbl.TabIndex = 24;
      this.codeLbl.Text = "Code";
      this.codeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // documentLinkLbl
      // 
      this.documentLinkLbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.documentLinkLbl.Location = new System.Drawing.Point(3, 52);
      this.documentLinkLbl.Name = "documentLinkLbl";
      this.documentLinkLbl.Size = new System.Drawing.Size(82, 26);
      this.documentLinkLbl.TabIndex = 25;
      this.documentLinkLbl.Text = "Document link";
      this.documentLinkLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // nameTextBox
      // 
      this.baseLayout.SetColumnSpan(this.nameTextBox, 2);
      this.nameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nameTextBox.Location = new System.Drawing.Point(91, 3);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(201, 20);
      this.nameTextBox.TabIndex = 18;
      this.nameTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // codeTextBox
      // 
      this.baseLayout.SetColumnSpan(this.codeTextBox, 2);
      this.codeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.codeTextBox.Location = new System.Drawing.Point(91, 29);
      this.codeTextBox.Name = "codeTextBox";
      this.codeTextBox.Size = new System.Drawing.Size(201, 20);
      this.codeTextBox.TabIndex = 19;
      this.codeTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 2;
      this.baseLayout.SetColumnSpan(this.tableLayoutPanel2, 2);
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
      this.tableLayoutPanel2.Controls.Add(this.documentLinkBtn, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.documentLinkTextBox, 0, 0);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(88, 52);
      this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 1;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(207, 26);
      this.tableLayoutPanel2.TabIndex = 46;
      // 
      // documentLinkBtn
      // 
      this.documentLinkBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.documentLinkBtn.Location = new System.Drawing.Point(178, 3);
      this.documentLinkBtn.Name = "documentLinkBtn";
      this.documentLinkBtn.Size = new System.Drawing.Size(26, 20);
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
      this.documentLinkTextBox.Size = new System.Drawing.Size(169, 20);
      this.documentLinkTextBox.TabIndex = 20;
      this.documentLinkTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      this.documentLinkTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DocumentLinkTextBoxKeyDown);
      // 
      // saveBtn
      // 
      this.saveBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.saveBtn.Location = new System.Drawing.Point(194, 107);
      this.saveBtn.Name = "saveBtn";
      this.saveBtn.Size = new System.Drawing.Size(98, 26);
      this.saveBtn.TabIndex = 26;
      this.saveBtn.Text = "Save";
      this.saveBtn.UseVisualStyleBackColor = true;
      this.saveBtn.Click += new System.EventHandler(this.SaveBtnClick);
      // 
      // createBtn
      // 
      this.createBtn.Dock = System.Windows.Forms.DockStyle.Fill;
      this.createBtn.Location = new System.Drawing.Point(194, 139);
      this.createBtn.Name = "createBtn";
      this.createBtn.Size = new System.Drawing.Size(98, 26);
      this.createBtn.TabIndex = 22;
      this.createBtn.Text = "Create";
      this.createBtn.UseVisualStyleBackColor = true;
      this.createBtn.Click += new System.EventHandler(this.CreateBtnClick);
      // 
      // checkBoxArchive
      // 
      this.checkBoxArchive.AutoSize = true;
      this.baseLayout.SetColumnSpan(this.checkBoxArchive, 2);
      this.checkBoxArchive.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxArchive.Location = new System.Drawing.Point(91, 81);
      this.checkBoxArchive.Name = "checkBoxArchive";
      this.checkBoxArchive.Size = new System.Drawing.Size(201, 20);
      this.checkBoxArchive.TabIndex = 47;
      this.checkBoxArchive.UseVisualStyleBackColor = true;
      this.checkBoxArchive.CheckedChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 78);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(82, 26);
      this.label1.TabIndex = 48;
      this.label1.Text = "Archived";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // ProjectControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.panel);
      this.Name = "ProjectControl";
      this.Size = new System.Drawing.Size(295, 378);
      this.panel.ResumeLayout(false);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel2.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Button documentLinkBtn;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button resetBtn;
    private System.Windows.Forms.Button createBtn;
    private System.Windows.Forms.Button cancelBtn;
    private System.Windows.Forms.Label nameLbl;
    private System.Windows.Forms.Label codeLbl;
    private System.Windows.Forms.Label documentLinkLbl;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.TextBox codeTextBox;
    private System.Windows.Forms.TextBox documentLinkTextBox;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.CheckBox checkBoxArchive;
    private System.Windows.Forms.Label label1;
  }
}
