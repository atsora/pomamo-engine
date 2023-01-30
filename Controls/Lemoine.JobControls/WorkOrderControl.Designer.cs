// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class WorkOrderControl
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
      this.statusComboBox = new System.Windows.Forms.ComboBox();
      this.statusLbl = new System.Windows.Forms.Label();
      this.documentLinkBtn = new System.Windows.Forms.Button();
      this.resetBtn = new System.Windows.Forms.Button();
      this.cancelBtn = new System.Windows.Forms.Button();
      this.documentLinkTextBox = new System.Windows.Forms.TextBox();
      this.codeTextBox = new System.Windows.Forms.TextBox();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.documentLinkLbl = new System.Windows.Forms.Label();
      this.codeLbl = new System.Windows.Forms.Label();
      this.nameLbl = new System.Windows.Forms.Label();
      this.createBtn = new System.Windows.Forms.Button();
      this.saveBtn = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.panel.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel
      // 
      this.panel.AutoSize = true;
      this.panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panel.Controls.Add(this.statusComboBox);
      this.panel.Controls.Add(this.statusLbl);
      this.panel.Controls.Add(this.documentLinkBtn);
      this.panel.Controls.Add(this.resetBtn);
      this.panel.Controls.Add(this.cancelBtn);
      this.panel.Controls.Add(this.documentLinkTextBox);
      this.panel.Controls.Add(this.codeTextBox);
      this.panel.Controls.Add(this.nameTextBox);
      this.panel.Controls.Add(this.documentLinkLbl);
      this.panel.Controls.Add(this.codeLbl);
      this.panel.Controls.Add(this.nameLbl);
      this.panel.Controls.Add(this.createBtn);
      this.panel.Controls.Add(this.saveBtn);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(353, 135);
      this.panel.TabIndex = 34;
      // 
      // statusComboBox
      // 
      this.statusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.statusComboBox.FormattingEnabled = true;
      this.statusComboBox.Location = new System.Drawing.Point(170, 80);
      this.statusComboBox.Name = "statusComboBox";
      this.statusComboBox.Size = new System.Drawing.Size(180, 21);
      this.statusComboBox.TabIndex = 46;
      this.statusComboBox.SelectedIndexChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // statusLbl
      // 
      this.statusLbl.Location = new System.Drawing.Point(5, 80);
      this.statusLbl.Name = "statusLbl";
      this.statusLbl.Size = new System.Drawing.Size(160, 21);
      this.statusLbl.TabIndex = 45;
      this.statusLbl.Text = "Status";
      this.statusLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // documentLinkBtn
      // 
      this.documentLinkBtn.Location = new System.Drawing.Point(325, 54);
      this.documentLinkBtn.Name = "documentLinkBtn";
      this.documentLinkBtn.Size = new System.Drawing.Size(25, 23);
      this.documentLinkBtn.TabIndex = 44;
      this.documentLinkBtn.Text = "...";
      this.documentLinkBtn.UseVisualStyleBackColor = true;
      this.documentLinkBtn.Click += new System.EventHandler(this.DocumentLinkBtnClick);
      // 
      // resetBtn
      // 
      this.resetBtn.Location = new System.Drawing.Point(170, 109);
      this.resetBtn.Name = "resetBtn";
      this.resetBtn.Size = new System.Drawing.Size(75, 23);
      this.resetBtn.TabIndex = 37;
      this.resetBtn.Text = "Reset";
      this.resetBtn.UseVisualStyleBackColor = true;
      this.resetBtn.Click += new System.EventHandler(this.ResetBtnClick);
      // 
      // cancelBtn
      // 
      this.cancelBtn.Location = new System.Drawing.Point(170, 109);
      this.cancelBtn.Name = "cancelBtn";
      this.cancelBtn.Size = new System.Drawing.Size(75, 23);
      this.cancelBtn.TabIndex = 43;
      this.cancelBtn.Text = "Cancel";
      this.cancelBtn.UseVisualStyleBackColor = true;
      this.cancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
      // 
      // documentLinkTextBox
      // 
      this.documentLinkTextBox.Location = new System.Drawing.Point(170, 55);
      this.documentLinkTextBox.Name = "documentLinkTextBox";
      this.documentLinkTextBox.Size = new System.Drawing.Size(150, 20);
      this.documentLinkTextBox.TabIndex = 36;
      this.documentLinkTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      this.documentLinkTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DocumentLinkTextBoxKeyDown);
      // 
      // codeTextBox
      // 
      this.codeTextBox.Location = new System.Drawing.Point(170, 30);
      this.codeTextBox.Name = "codeTextBox";
      this.codeTextBox.Size = new System.Drawing.Size(180, 20);
      this.codeTextBox.TabIndex = 35;
      this.codeTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // nameTextBox
      // 
      this.nameTextBox.Location = new System.Drawing.Point(170, 5);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(180, 20);
      this.nameTextBox.TabIndex = 34;
      this.nameTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // documentLinkLbl
      // 
      this.documentLinkLbl.Location = new System.Drawing.Point(5, 55);
      this.documentLinkLbl.Name = "documentLinkLbl";
      this.documentLinkLbl.Size = new System.Drawing.Size(160, 21);
      this.documentLinkLbl.TabIndex = 41;
      this.documentLinkLbl.Text = "Document link";
      this.documentLinkLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // codeLbl
      // 
      this.codeLbl.Location = new System.Drawing.Point(5, 30);
      this.codeLbl.Name = "codeLbl";
      this.codeLbl.Size = new System.Drawing.Size(160, 21);
      this.codeLbl.TabIndex = 40;
      this.codeLbl.Text = "Code";
      this.codeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // nameLbl
      // 
      this.nameLbl.Location = new System.Drawing.Point(5, 5);
      this.nameLbl.Name = "nameLbl";
      this.nameLbl.Size = new System.Drawing.Size(160, 21);
      this.nameLbl.TabIndex = 39;
      this.nameLbl.Text = "Name";
      this.nameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // createBtn
      // 
      this.createBtn.Location = new System.Drawing.Point(275, 109);
      this.createBtn.Name = "createBtn";
      this.createBtn.Size = new System.Drawing.Size(75, 23);
      this.createBtn.TabIndex = 38;
      this.createBtn.Text = "Create";
      this.createBtn.UseVisualStyleBackColor = true;
      this.createBtn.Click += new System.EventHandler(this.CreateBtnClick);
      // 
      // saveBtn
      // 
      this.saveBtn.Location = new System.Drawing.Point(275, 109);
      this.saveBtn.Name = "saveBtn";
      this.saveBtn.Size = new System.Drawing.Size(75, 23);
      this.saveBtn.TabIndex = 42;
      this.saveBtn.Text = "Save";
      this.saveBtn.UseVisualStyleBackColor = true;
      this.saveBtn.Click += new System.EventHandler(this.SaveBtnClick);
      // 
      // WorkOrderControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.Controls.Add(this.panel);
      this.Name = "WorkOrderControl";
      this.Size = new System.Drawing.Size(353, 135);
      this.panel.ResumeLayout(false);
      this.panel.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
    private System.Windows.Forms.ComboBox statusComboBox;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.Label statusLbl;
    private System.Windows.Forms.Button documentLinkBtn;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button resetBtn;
    private System.Windows.Forms.Button createBtn;
    private System.Windows.Forms.Label nameLbl;
    private System.Windows.Forms.Label codeLbl;
    private System.Windows.Forms.Label documentLinkLbl;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.TextBox codeTextBox;
    private System.Windows.Forms.TextBox documentLinkTextBox;
    private System.Windows.Forms.Button cancelBtn;
  }
}
