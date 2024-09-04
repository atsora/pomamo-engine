// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateLine
{
  partial class Page3
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page3));
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.label2 = new System.Windows.Forms.Label();
      this.buttonAdd = new System.Windows.Forms.Button();
      this.buttonRemove = new System.Windows.Forms.Button();
      this.checkDedicated = new System.Windows.Forms.CheckBox();
      this.listBox = new Lemoine.BaseControls.List.ListTextValue();
      this.comboOperation = new System.Windows.Forms.ComboBox();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 4;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.buttonAdd, 2, 3);
      this.tableLayoutPanel1.Controls.Add(this.buttonRemove, 2, 4);
      this.tableLayoutPanel1.Controls.Add(this.checkDedicated, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.listBox, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.comboOperation, 1, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 5;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(350, 250);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(3, 0);
      this.label2.Name = "label2";
      this.label2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
      this.label2.Size = new System.Drawing.Size(90, 30);
      this.label2.TabIndex = 8;
      this.label2.Text = "Operation";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonAdd
      // 
      this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonAdd.Image")));
      this.buttonAdd.Location = new System.Drawing.Point(195, 193);
      this.buttonAdd.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new System.Drawing.Size(27, 27);
      this.buttonAdd.TabIndex = 5;
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemove.Image")));
      this.buttonRemove.Location = new System.Drawing.Point(195, 223);
      this.buttonRemove.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(27, 27);
      this.buttonRemove.TabIndex = 6;
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
      // 
      // checkDedicated
      // 
      this.checkDedicated.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.tableLayoutPanel1.SetColumnSpan(this.checkDedicated, 2);
      this.checkDedicated.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkDedicated.Location = new System.Drawing.Point(192, 30);
      this.checkDedicated.Margin = new System.Windows.Forms.Padding(0);
      this.checkDedicated.Name = "checkDedicated";
      this.checkDedicated.Size = new System.Drawing.Size(158, 25);
      this.checkDedicated.TabIndex = 8;
      this.checkDedicated.Text = "Dedicated machine";
      this.checkDedicated.UseVisualStyleBackColor = true;
      this.checkDedicated.Leave += new System.EventHandler(this.CheckDedicatedLeave);
      // 
      // listBox
      // 
      this.listBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tableLayoutPanel1.SetColumnSpan(this.listBox, 2);
      this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBox.Location = new System.Drawing.Point(0, 30);
      this.listBox.Margin = new System.Windows.Forms.Padding(0);
      this.listBox.Name = "listBox";
      this.tableLayoutPanel1.SetRowSpan(this.listBox, 4);
      this.listBox.SelectedIndex = -1;
      this.listBox.SelectedIndexes = null;
      this.listBox.SelectedText = "";
      this.listBox.SelectedTexts = null;
      this.listBox.SelectedValue = null;
      this.listBox.SelectedValues = null;
      this.listBox.Size = new System.Drawing.Size(192, 220);
      this.listBox.Sorted = true;
      this.listBox.TabIndex = 11;
      // 
      // comboOperation
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.comboOperation, 3);
      this.comboOperation.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboOperation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboOperation.FormattingEnabled = true;
      this.comboOperation.Location = new System.Drawing.Point(96, 0);
      this.comboOperation.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
      this.comboOperation.Name = "comboOperation";
      this.comboOperation.Size = new System.Drawing.Size(254, 21);
      this.comboOperation.TabIndex = 9;
      this.comboOperation.SelectedIndexChanged += new System.EventHandler(this.ComboOperationSelectedIndexChanged);
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(350, 250);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.CheckBox checkDedicated;
    private System.Windows.Forms.ComboBox comboOperation;
    private System.Windows.Forms.Label label2;
    private Lemoine.BaseControls.List.ListTextValue listBox;
    private System.Windows.Forms.Button buttonAdd;
    private System.Windows.Forms.Button buttonRemove;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
