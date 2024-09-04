// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateLine
{
  partial class Page2
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page2));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label6 = new System.Windows.Forms.Label();
      this.textOperationName = new System.Windows.Forms.TextBox();
      this.buttonDown = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.buttonUp = new System.Windows.Forms.Button();
      this.textOperationCode = new System.Windows.Forms.TextBox();
      this.buttonAdd = new System.Windows.Forms.Button();
      this.label5 = new System.Windows.Forms.Label();
      this.buttonRemove = new System.Windows.Forms.Button();
      this.listBox = new Lemoine.BaseControls.List.ListTextValue();
      this.numericParts = new System.Windows.Forms.NumericUpDown();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericParts)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.baseLayout.Controls.Add(this.label6, 1, 0);
      this.baseLayout.Controls.Add(this.textOperationName, 1, 1);
      this.baseLayout.Controls.Add(this.buttonDown, 1, 8);
      this.baseLayout.Controls.Add(this.label2, 1, 2);
      this.baseLayout.Controls.Add(this.buttonUp, 1, 7);
      this.baseLayout.Controls.Add(this.textOperationCode, 1, 3);
      this.baseLayout.Controls.Add(this.buttonAdd, 1, 9);
      this.baseLayout.Controls.Add(this.label5, 1, 4);
      this.baseLayout.Controls.Add(this.buttonRemove, 1, 10);
      this.baseLayout.Controls.Add(this.numericParts, 1, 5);
      this.baseLayout.Controls.Add(this.listBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 11;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 83F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 0;
      // 
      // label6
      // 
      this.baseLayout.SetColumnSpan(this.label6, 2);
      this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label6.Location = new System.Drawing.Point(195, 0);
      this.label6.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(155, 20);
      this.label6.TabIndex = 9;
      this.label6.Text = "Name";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textOperationName
      // 
      this.baseLayout.SetColumnSpan(this.textOperationName, 2);
      this.textOperationName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textOperationName.Location = new System.Drawing.Point(195, 20);
      this.textOperationName.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.textOperationName.Name = "textOperationName";
      this.textOperationName.Size = new System.Drawing.Size(155, 20);
      this.textOperationName.TabIndex = 10;
      this.textOperationName.Leave += new System.EventHandler(this.TextOperationNameLeave);
      // 
      // buttonDown
      // 
      this.buttonDown.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDown.Image = ((System.Drawing.Image)(resources.GetObject("buttonDown.Image")));
      this.buttonDown.Location = new System.Drawing.Point(195, 163);
      this.buttonDown.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(27, 27);
      this.buttonDown.TabIndex = 4;
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.ButtonDownClick);
      // 
      // label2
      // 
      this.baseLayout.SetColumnSpan(this.label2, 2);
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(195, 43);
      this.label2.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(155, 20);
      this.label2.TabIndex = 12;
      this.label2.Text = "Code";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonUp
      // 
      this.buttonUp.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonUp.Image = ((System.Drawing.Image)(resources.GetObject("buttonUp.Image")));
      this.buttonUp.Location = new System.Drawing.Point(195, 133);
      this.buttonUp.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(27, 27);
      this.buttonUp.TabIndex = 3;
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.ButtonUpClick);
      // 
      // textOperationCode
      // 
      this.baseLayout.SetColumnSpan(this.textOperationCode, 2);
      this.textOperationCode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textOperationCode.Location = new System.Drawing.Point(195, 63);
      this.textOperationCode.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.textOperationCode.Name = "textOperationCode";
      this.textOperationCode.Size = new System.Drawing.Size(155, 20);
      this.textOperationCode.TabIndex = 13;
      this.textOperationCode.Leave += new System.EventHandler(this.TextOperationCodeLeave);
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
      // label5
      // 
      this.baseLayout.SetColumnSpan(this.label5, 2);
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(195, 86);
      this.label5.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(155, 20);
      this.label5.TabIndex = 3;
      this.label5.Text = "Max parts / cycle";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      // listBox
      // 
      this.listBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBox.Location = new System.Drawing.Point(0, 0);
      this.listBox.Margin = new System.Windows.Forms.Padding(0);
      this.listBox.Name = "listBox";
      this.baseLayout.SetRowSpan(this.listBox, 11);
      this.listBox.SelectedIndex = -1;
      this.listBox.SelectedIndexes = null;
      this.listBox.SelectedText = "";
      this.listBox.SelectedTexts = null;
      this.listBox.SelectedValue = null;
      this.listBox.SelectedValues = null;
      this.listBox.Size = new System.Drawing.Size(192, 250);
      this.listBox.TabIndex = 14;
      this.listBox.ItemChanged += new System.Action<string, object>(this.ListBoxItemChanged);
      // 
      // numericParts
      // 
      this.baseLayout.SetColumnSpan(this.numericParts, 2);
      this.numericParts.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericParts.Location = new System.Drawing.Point(195, 106);
      this.numericParts.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.numericParts.Maximum = new decimal(new int[] {
      100000,
      0,
      0,
      0});
      this.numericParts.Minimum = new decimal(new int[] {
      1,
      0,
      0,
      0});
      this.numericParts.Name = "numericParts";
      this.numericParts.Size = new System.Drawing.Size(155, 20);
      this.numericParts.TabIndex = 8;
      this.numericParts.ThousandsSeparator = true;
      this.numericParts.Value = new decimal(new int[] {
      1,
      0,
      0,
      0});
      this.numericParts.Leave += new System.EventHandler(this.NumericPartsLeave);
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.MinimumSize = new System.Drawing.Size(350, 250);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericParts)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.List.ListTextValue listBox;
    private System.Windows.Forms.TextBox textOperationCode;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button buttonRemove;
    private System.Windows.Forms.NumericUpDown numericParts;
    private System.Windows.Forms.TextBox textOperationName;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Button buttonAdd;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.Button buttonUp;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
