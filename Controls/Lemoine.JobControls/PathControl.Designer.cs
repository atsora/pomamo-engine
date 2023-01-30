// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class PathControl
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.operationTextBox = new System.Windows.Forms.TextBox();
      this.operationLbl = new System.Windows.Forms.Label();
      this.numberTextBox = new System.Windows.Forms.TextBox();
      this.numberLbl = new System.Windows.Forms.Label();
      this.resetBtn = new System.Windows.Forms.Button();
      this.cancelBtn = new System.Windows.Forms.Button();
      this.createBtn = new System.Windows.Forms.Button();
      this.saveBtn = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.operationTextBox);
      this.panel1.Controls.Add(this.operationLbl);
      this.panel1.Controls.Add(this.numberTextBox);
      this.panel1.Controls.Add(this.numberLbl);
      this.panel1.Controls.Add(this.resetBtn);
      this.panel1.Controls.Add(this.cancelBtn);
      this.panel1.Controls.Add(this.createBtn);
      this.panel1.Controls.Add(this.saveBtn);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(358, 122);
      this.panel1.TabIndex = 0;
      // 
      // operationTextBox
      // 
      this.operationTextBox.Location = new System.Drawing.Point(166, 35);
      this.operationTextBox.Name = "operationTextBox";
      this.operationTextBox.ReadOnly = true;
      this.operationTextBox.Size = new System.Drawing.Size(180, 20);
      this.operationTextBox.TabIndex = 35;
      // 
      // operationLbl
      // 
      this.operationLbl.Location = new System.Drawing.Point(3, 35);
      this.operationLbl.Name = "operationLbl";
      this.operationLbl.Size = new System.Drawing.Size(160, 21);
      this.operationLbl.TabIndex = 36;
      this.operationLbl.Text = "Operation";
      this.operationLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // numberTextBox
      // 
      this.numberTextBox.Location = new System.Drawing.Point(166, 9);
      this.numberTextBox.Name = "numberTextBox";
      this.numberTextBox.Size = new System.Drawing.Size(180, 20);
      this.numberTextBox.TabIndex = 20;
      this.numberTextBox.TextChanged += new System.EventHandler(this.NameTextBoxTextChanged);
      // 
      // numberLbl
      // 
      this.numberLbl.Location = new System.Drawing.Point(3, 9);
      this.numberLbl.Name = "numberLbl";
      this.numberLbl.Size = new System.Drawing.Size(160, 21);
      this.numberLbl.TabIndex = 21;
      this.numberLbl.Text = "Number";
      this.numberLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // resetBtn
      // 
      this.resetBtn.Location = new System.Drawing.Point(166, 61);
      this.resetBtn.Name = "resetBtn";
      this.resetBtn.Size = new System.Drawing.Size(75, 23);
      this.resetBtn.TabIndex = 37;
      this.resetBtn.Text = "Reset";
      this.resetBtn.UseVisualStyleBackColor = true;
      this.resetBtn.Click += new System.EventHandler(this.ResetBtnClick);
      // 
      // cancelBtn
      // 
      this.cancelBtn.Location = new System.Drawing.Point(166, 61);
      this.cancelBtn.Name = "cancelBtn";
      this.cancelBtn.Size = new System.Drawing.Size(75, 23);
      this.cancelBtn.TabIndex = 40;
      this.cancelBtn.Text = "Cancel";
      this.cancelBtn.UseVisualStyleBackColor = true;
      this.cancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
      // 
      // createBtn
      // 
      this.createBtn.Location = new System.Drawing.Point(271, 61);
      this.createBtn.Name = "createBtn";
      this.createBtn.Size = new System.Drawing.Size(75, 23);
      this.createBtn.TabIndex = 38;
      this.createBtn.Text = "Create";
      this.createBtn.UseVisualStyleBackColor = true;
      this.createBtn.Click += new System.EventHandler(this.CreateBtnClick);
      // 
      // saveBtn
      // 
      this.saveBtn.Location = new System.Drawing.Point(271, 61);
      this.saveBtn.Name = "saveBtn";
      this.saveBtn.Size = new System.Drawing.Size(75, 23);
      this.saveBtn.TabIndex = 39;
      this.saveBtn.Text = "Save";
      this.saveBtn.UseVisualStyleBackColor = true;
      this.saveBtn.Click += new System.EventHandler(this.SaveBtnClick);
      // 
      // PathControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.panel1);
      this.Name = "PathControl";
      this.Size = new System.Drawing.Size(358, 122);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button cancelBtn;
    private System.Windows.Forms.Button createBtn;
    private System.Windows.Forms.Button resetBtn;
    private System.Windows.Forms.Label operationLbl;
    private System.Windows.Forms.TextBox operationTextBox;
    private System.Windows.Forms.Label numberLbl;
    private System.Windows.Forms.TextBox numberTextBox;
    private System.Windows.Forms.Panel panel1;
  }
}
