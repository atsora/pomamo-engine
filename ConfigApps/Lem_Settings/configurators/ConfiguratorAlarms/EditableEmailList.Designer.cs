// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarms
{
  partial class EditableEmailList
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
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.textBoxEmail = new System.Windows.Forms.TextBox();
      this.buttonAdd = new System.Windows.Forms.Button();
      this.buttonRemove = new System.Windows.Forms.Button();
      this.listBoxEmail = new System.Windows.Forms.CheckedListBox();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel1.Controls.Add(this.textBoxEmail, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.buttonAdd, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.buttonRemove, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.listBoxEmail, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(126, 184);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // textBoxEmail
      // 
      this.textBoxEmail.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBoxEmail.Location = new System.Drawing.Point(0, 163);
      this.textBoxEmail.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.textBoxEmail.Name = "textBoxEmail";
      this.textBoxEmail.Size = new System.Drawing.Size(76, 20);
      this.textBoxEmail.TabIndex = 1;
      // 
      // buttonAdd
      // 
      this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAdd.ForeColor = System.Drawing.SystemColors.ControlText;
      this.buttonAdd.Location = new System.Drawing.Point(79, 163);
      this.buttonAdd.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new System.Drawing.Size(22, 21);
      this.buttonAdd.TabIndex = 2;
      this.buttonAdd.Text = "+";
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRemove.Location = new System.Drawing.Point(104, 163);
      this.buttonRemove.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(22, 21);
      this.buttonRemove.TabIndex = 3;
      this.buttonRemove.Text = "-";
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
      // 
      // listBoxEmail
      // 
      this.listBoxEmail.CheckOnClick = true;
      this.tableLayoutPanel1.SetColumnSpan(this.listBoxEmail, 3);
      this.listBoxEmail.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBoxEmail.FormattingEnabled = true;
      this.listBoxEmail.IntegralHeight = false;
      this.listBoxEmail.Location = new System.Drawing.Point(0, 0);
      this.listBoxEmail.Margin = new System.Windows.Forms.Padding(0);
      this.listBoxEmail.Name = "listBoxEmail";
      this.listBoxEmail.Size = new System.Drawing.Size(126, 160);
      this.listBoxEmail.TabIndex = 4;
      // 
      // EditableEmailList
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "EditableEmailList";
      this.Size = new System.Drawing.Size(126, 184);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Button buttonRemove;
    private System.Windows.Forms.Button buttonAdd;
    private System.Windows.Forms.TextBox textBoxEmail;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.CheckedListBox listBoxEmail;
  }
}
