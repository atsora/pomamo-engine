// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateLine
{
  partial class Page1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page1));
            this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
            this.listParts = new Lemoine.BaseControls.List.ListTextValue();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textName = new System.Windows.Forms.TextBox();
            this.textCode = new System.Windows.Forms.TextBox();
            this.baseLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // baseLayout
            // 
            this.baseLayout.ColumnCount = 3;
            this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.baseLayout.Controls.Add(this.listParts, 0, 0);
            this.baseLayout.Controls.Add(this.buttonAdd, 1, 5);
            this.baseLayout.Controls.Add(this.buttonRemove, 1, 6);
            this.baseLayout.Controls.Add(this.label1, 1, 0);
            this.baseLayout.Controls.Add(this.label2, 1, 2);
            this.baseLayout.Controls.Add(this.textName, 1, 1);
            this.baseLayout.Controls.Add(this.textCode, 1, 3);
            this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseLayout.Location = new System.Drawing.Point(0, 0);
            this.baseLayout.Name = "baseLayout";
            this.baseLayout.RowCount = 7;
            this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.baseLayout.Size = new System.Drawing.Size(350, 250);
            this.baseLayout.TabIndex = 0;
            // 
            // listParts
            // 
            this.listParts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listParts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listParts.Location = new System.Drawing.Point(0, 0);
            this.listParts.Margin = new System.Windows.Forms.Padding(0);
            this.listParts.Name = "listParts";
            this.baseLayout.SetRowSpan(this.listParts, 7);
            this.listParts.Size = new System.Drawing.Size(192, 250);
            this.listParts.Sorted = true;
            this.listParts.TabIndex = 0;
            this.listParts.ItemChanged += new System.Action<string, object>(this.ListPartsItemChanged);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonAdd.Image")));
            this.buttonAdd.Location = new System.Drawing.Point(195, 193);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(27, 27);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemove.Image")));
            this.buttonRemove.Location = new System.Drawing.Point(195, 223);
            this.buttonRemove.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(27, 27);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
            // 
            // label1
            // 
            this.baseLayout.SetColumnSpan(this.label1, 2);
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Enabled = false;
            this.label1.Location = new System.Drawing.Point(195, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.baseLayout.SetColumnSpan(this.label2, 2);
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Enabled = false;
            this.label2.Location = new System.Drawing.Point(195, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(152, 23);
            this.label2.TabIndex = 4;
            this.label2.Text = "Code";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textName
            // 
            this.baseLayout.SetColumnSpan(this.textName, 2);
            this.textName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textName.Enabled = false;
            this.textName.Location = new System.Drawing.Point(195, 26);
            this.textName.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(155, 20);
            this.textName.TabIndex = 5;
            this.textName.Leave += new System.EventHandler(this.TextNameLeave);
            // 
            // textCode
            // 
            this.baseLayout.SetColumnSpan(this.textCode, 2);
            this.textCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textCode.Enabled = false;
            this.textCode.Location = new System.Drawing.Point(195, 72);
            this.textCode.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.textCode.Name = "textCode";
            this.textCode.Size = new System.Drawing.Size(155, 20);
            this.textCode.TabIndex = 6;
            this.textCode.Leave += new System.EventHandler(this.TextCodeLeave);
            // 
            // Page1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.baseLayout);
            this.Name = "Page1";
            this.Size = new System.Drawing.Size(350, 250);
            this.baseLayout.ResumeLayout(false);
            this.baseLayout.PerformLayout();
            this.ResumeLayout(false);

    }
    private System.Windows.Forms.TextBox textCode;
    private System.Windows.Forms.TextBox textName;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button buttonRemove;
    private System.Windows.Forms.Button buttonAdd;
    private Lemoine.BaseControls.List.ListTextValue listParts;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
