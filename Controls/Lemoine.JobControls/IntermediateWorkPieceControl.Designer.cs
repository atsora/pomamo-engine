// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class IntermediateWorkPieceControl
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
      this.documentLinkBtn = new System.Windows.Forms.Button();
      this.resetBtn = new System.Windows.Forms.Button();
      this.operationQuantityNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.cancelBtn = new System.Windows.Forms.Button();
      this.weightTextBox = new System.Windows.Forms.TextBox();
      this.documentLinkTextBox = new System.Windows.Forms.TextBox();
      this.codeTextBox = new System.Windows.Forms.TextBox();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.operationQuantityLbl = new System.Windows.Forms.Label();
      this.weightLbl = new System.Windows.Forms.Label();
      this.documentLinkLbl = new System.Windows.Forms.Label();
      this.codeLbl = new System.Windows.Forms.Label();
      this.nameLbl = new System.Windows.Forms.Label();
      this.createBtn = new System.Windows.Forms.Button();
      this.saveBtn = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.panel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.operationQuantityNumericUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.AutoSize = true;
      this.tableLayoutPanel.ColumnCount = 1;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.Location = new System.Drawing.Point(4, 164);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
      this.tableLayoutPanel.RowCount = 1;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.Size = new System.Drawing.Size(345, 23);
      this.tableLayoutPanel.TabIndex = 30;
      // 
      // panel
      // 
      this.panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panel.Controls.Add(this.documentLinkBtn);
      this.panel.Controls.Add(this.tableLayoutPanel);
      this.panel.Controls.Add(this.resetBtn);
      this.panel.Controls.Add(this.operationQuantityNumericUpDown);
      this.panel.Controls.Add(this.cancelBtn);
      this.panel.Controls.Add(this.weightTextBox);
      this.panel.Controls.Add(this.documentLinkTextBox);
      this.panel.Controls.Add(this.codeTextBox);
      this.panel.Controls.Add(this.nameTextBox);
      this.panel.Controls.Add(this.operationQuantityLbl);
      this.panel.Controls.Add(this.weightLbl);
      this.panel.Controls.Add(this.documentLinkLbl);
      this.panel.Controls.Add(this.codeLbl);
      this.panel.Controls.Add(this.nameLbl);
      this.panel.Controls.Add(this.createBtn);
      this.panel.Controls.Add(this.saveBtn);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(362, 202);
      this.panel.TabIndex = 31;
      // 
      // documentLinkBtn
      // 
      this.documentLinkBtn.Location = new System.Drawing.Point(323, 54);
      this.documentLinkBtn.Name = "documentLinkBtn";
      this.documentLinkBtn.Size = new System.Drawing.Size(25, 23);
      this.documentLinkBtn.TabIndex = 45;
      this.documentLinkBtn.Text = "...";
      this.documentLinkBtn.UseVisualStyleBackColor = true;
      this.documentLinkBtn.Click += new System.EventHandler(this.DocumentLinkBtnClick);
      // 
      // resetBtn
      // 
      this.resetBtn.Location = new System.Drawing.Point(169, 135);
      this.resetBtn.Name = "resetBtn";
      this.resetBtn.Size = new System.Drawing.Size(75, 23);
      this.resetBtn.TabIndex = 35;
      this.resetBtn.Text = "Reset";
      this.resetBtn.UseVisualStyleBackColor = true;
      this.resetBtn.Click += new System.EventHandler(this.ResetBtnClick);
      // 
      // operationQuantityNumericUpDown
      // 
      this.operationQuantityNumericUpDown.Location = new System.Drawing.Point(169, 105);
      this.operationQuantityNumericUpDown.Maximum = new decimal(new int[] {
                  10000,
                  0,
                  0,
                  0});
      this.operationQuantityNumericUpDown.Name = "operationQuantityNumericUpDown";
      this.operationQuantityNumericUpDown.Size = new System.Drawing.Size(179, 20);
      this.operationQuantityNumericUpDown.TabIndex = 34;
      this.operationQuantityNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.operationQuantityNumericUpDown.ValueChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // cancelBtn
      // 
      this.cancelBtn.Location = new System.Drawing.Point(169, 135);
      this.cancelBtn.Name = "cancelBtn";
      this.cancelBtn.Size = new System.Drawing.Size(75, 23);
      this.cancelBtn.TabIndex = 43;
      this.cancelBtn.Text = "Cancel";
      this.cancelBtn.UseVisualStyleBackColor = true;
      this.cancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
      // 
      // weightTextBox
      // 
      this.weightTextBox.Location = new System.Drawing.Point(169, 80);
      this.weightTextBox.Name = "weightTextBox";
      this.weightTextBox.Size = new System.Drawing.Size(180, 20);
      this.weightTextBox.TabIndex = 33;
      this.weightTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.weightTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // documentLinkTextBox
      // 
      this.documentLinkTextBox.Location = new System.Drawing.Point(169, 55);
      this.documentLinkTextBox.Name = "documentLinkTextBox";
      this.documentLinkTextBox.Size = new System.Drawing.Size(150, 20);
      this.documentLinkTextBox.TabIndex = 32;
      this.documentLinkTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      this.documentLinkTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DocumentLinkTextBoxKeyDown);
      // 
      // codeTextBox
      // 
      this.codeTextBox.Location = new System.Drawing.Point(169, 30);
      this.codeTextBox.Name = "codeTextBox";
      this.codeTextBox.Size = new System.Drawing.Size(180, 20);
      this.codeTextBox.TabIndex = 31;
      this.codeTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // nameTextBox
      // 
      this.nameTextBox.Location = new System.Drawing.Point(169, 5);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(180, 20);
      this.nameTextBox.TabIndex = 30;
      this.nameTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // operationQuantityLbl
      // 
      this.operationQuantityLbl.Location = new System.Drawing.Point(4, 105);
      this.operationQuantityLbl.Name = "operationQuantityLbl";
      this.operationQuantityLbl.Size = new System.Drawing.Size(160, 21);
      this.operationQuantityLbl.TabIndex = 41;
      this.operationQuantityLbl.Text = "Operation Quantity";
      this.operationQuantityLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // weightLbl
      // 
      this.weightLbl.Location = new System.Drawing.Point(4, 80);
      this.weightLbl.Name = "weightLbl";
      this.weightLbl.Size = new System.Drawing.Size(160, 21);
      this.weightLbl.TabIndex = 40;
      this.weightLbl.Text = "Weight";
      this.weightLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // documentLinkLbl
      // 
      this.documentLinkLbl.Location = new System.Drawing.Point(4, 55);
      this.documentLinkLbl.Name = "documentLinkLbl";
      this.documentLinkLbl.Size = new System.Drawing.Size(160, 21);
      this.documentLinkLbl.TabIndex = 39;
      this.documentLinkLbl.Text = "Document link";
      this.documentLinkLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // codeLbl
      // 
      this.codeLbl.Location = new System.Drawing.Point(4, 30);
      this.codeLbl.Name = "codeLbl";
      this.codeLbl.Size = new System.Drawing.Size(160, 21);
      this.codeLbl.TabIndex = 38;
      this.codeLbl.Text = "Code";
      this.codeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // nameLbl
      // 
      this.nameLbl.Location = new System.Drawing.Point(4, 5);
      this.nameLbl.Name = "nameLbl";
      this.nameLbl.Size = new System.Drawing.Size(160, 21);
      this.nameLbl.TabIndex = 37;
      this.nameLbl.Text = "Name";
      this.nameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // createBtn
      // 
      this.createBtn.Location = new System.Drawing.Point(273, 135);
      this.createBtn.Name = "createBtn";
      this.createBtn.Size = new System.Drawing.Size(75, 23);
      this.createBtn.TabIndex = 36;
      this.createBtn.Text = "Create";
      this.createBtn.UseVisualStyleBackColor = true;
      this.createBtn.Click += new System.EventHandler(this.CreateBtnClick);
      // 
      // saveBtn
      // 
      this.saveBtn.Location = new System.Drawing.Point(273, 135);
      this.saveBtn.Name = "saveBtn";
      this.saveBtn.Size = new System.Drawing.Size(75, 23);
      this.saveBtn.TabIndex = 42;
      this.saveBtn.Text = "Save";
      this.saveBtn.UseVisualStyleBackColor = true;
      this.saveBtn.Click += new System.EventHandler(this.SaveBtnClick);
      // 
      // IntermediateWorkPieceControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.Controls.Add(this.panel);
      this.Name = "IntermediateWorkPieceControl";
      this.Size = new System.Drawing.Size(362, 202);
      this.panel.ResumeLayout(false);
      this.panel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.operationQuantityNumericUpDown)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Button documentLinkBtn;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button resetBtn;
    private System.Windows.Forms.Button createBtn;
    private System.Windows.Forms.NumericUpDown operationQuantityNumericUpDown;
    private System.Windows.Forms.Label operationQuantityLbl;
    private System.Windows.Forms.TextBox weightTextBox;
    private System.Windows.Forms.Label nameLbl;
    private System.Windows.Forms.Label codeLbl;
    private System.Windows.Forms.Label documentLinkLbl;
    private System.Windows.Forms.Label weightLbl;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.TextBox codeTextBox;
    private System.Windows.Forms.TextBox documentLinkTextBox;
    private System.Windows.Forms.Button cancelBtn;
  }
}
