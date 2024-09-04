// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateLine
{
  partial class DialogNewPart
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the form.
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.radioNew = new System.Windows.Forms.RadioButton();
      this.radioExisting = new System.Windows.Forms.RadioButton();
      this.buttonCopy = new System.Windows.Forms.Button();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.textPartName = new System.Windows.Forms.TextBox();
      this.textPartCode = new System.Windows.Forms.TextBox();
      this.comboPart = new Lemoine.BaseControls.ComboboxTextValue();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonOk = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.baseLayout.Controls.Add(this.radioNew, 0, 0);
      this.baseLayout.Controls.Add(this.radioExisting, 0, 3);
      this.baseLayout.Controls.Add(this.buttonCopy, 3, 0);
      this.baseLayout.Controls.Add(this.label4, 0, 1);
      this.baseLayout.Controls.Add(this.label5, 0, 2);
      this.baseLayout.Controls.Add(this.textPartName, 1, 1);
      this.baseLayout.Controls.Add(this.textPartCode, 1, 2);
      this.baseLayout.Controls.Add(this.comboPart, 0, 4);
      this.baseLayout.Controls.Add(this.buttonCancel, 2, 6);
      this.baseLayout.Controls.Add(this.buttonOk, 3, 6);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.baseLayout.Location = new System.Drawing.Point(3, 3);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 7;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.Size = new System.Drawing.Size(368, 156);
      this.baseLayout.TabIndex = 10;
      // 
      // radioNew
      // 
      this.radioNew.Checked = true;
      this.baseLayout.SetColumnSpan(this.radioNew, 2);
      this.radioNew.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioNew.Location = new System.Drawing.Point(3, 0);
      this.radioNew.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.radioNew.Name = "radioNew";
      this.radioNew.Size = new System.Drawing.Size(185, 23);
      this.radioNew.TabIndex = 7;
      this.radioNew.TabStop = true;
      this.radioNew.Text = "New part";
      this.radioNew.UseVisualStyleBackColor = true;
      this.radioNew.CheckedChanged += new System.EventHandler(this.RadioNewCheckedChanged);
      // 
      // radioExisting
      // 
      this.baseLayout.SetColumnSpan(this.radioExisting, 4);
      this.radioExisting.Dock = System.Windows.Forms.DockStyle.Fill;
      this.radioExisting.Location = new System.Drawing.Point(3, 69);
      this.radioExisting.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.radioExisting.Name = "radioExisting";
      this.radioExisting.Size = new System.Drawing.Size(365, 23);
      this.radioExisting.TabIndex = 3;
      this.radioExisting.Text = "Existing part";
      this.radioExisting.UseVisualStyleBackColor = true;
      this.radioExisting.CheckedChanged += new System.EventHandler(this.RadioExistingCheckedChanged);
      // 
      // buttonCopy
      // 
      this.buttonCopy.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCopy.Location = new System.Drawing.Point(278, 0);
      this.buttonCopy.Margin = new System.Windows.Forms.Padding(0, 0, 3, 2);
      this.buttonCopy.Name = "buttonCopy";
      this.buttonCopy.Size = new System.Drawing.Size(87, 21);
      this.buttonCopy.TabIndex = 2;
      this.buttonCopy.Text = "Copy from line";
      this.buttonCopy.UseVisualStyleBackColor = true;
      this.buttonCopy.Click += new System.EventHandler(this.ButtonCopyClick);
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(20, 23);
      this.label4.Margin = new System.Windows.Forms.Padding(20, 0, 3, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(47, 23);
      this.label4.TabIndex = 8;
      this.label4.Text = "Name";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(20, 46);
      this.label5.Margin = new System.Windows.Forms.Padding(20, 0, 3, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(47, 23);
      this.label5.TabIndex = 9;
      this.label5.Text = "Code";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textPartName
      // 
      this.baseLayout.SetColumnSpan(this.textPartName, 3);
      this.textPartName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textPartName.Location = new System.Drawing.Point(70, 23);
      this.textPartName.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.textPartName.Name = "textPartName";
      this.textPartName.Size = new System.Drawing.Size(295, 20);
      this.textPartName.TabIndex = 0;
      // 
      // textPartCode
      // 
      this.baseLayout.SetColumnSpan(this.textPartCode, 3);
      this.textPartCode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textPartCode.Location = new System.Drawing.Point(70, 46);
      this.textPartCode.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.textPartCode.Name = "textPartCode";
      this.textPartCode.Size = new System.Drawing.Size(295, 20);
      this.textPartCode.TabIndex = 1;
      // 
      // comboPart
      // 
      this.baseLayout.SetColumnSpan(this.comboPart, 4);
      this.comboPart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboPart.Enabled = false;
      this.comboPart.Location = new System.Drawing.Point(20, 92);
      this.comboPart.Margin = new System.Windows.Forms.Padding(20, 0, 3, 3);
      this.comboPart.Name = "comboPart";
      this.comboPart.SelectedIndex = -1;
      this.comboPart.SelectedText = "";
      this.comboPart.SelectedValue = null;
      this.comboPart.Size = new System.Drawing.Size(345, 21);
      this.comboPart.Sorted = true;
      this.comboPart.TabIndex = 4;
      // 
      // buttonCancel
      // 
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCancel.Location = new System.Drawing.Point(191, 133);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(87, 23);
      this.buttonCancel.TabIndex = 6;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
      // 
      // buttonOk
      // 
      this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOk.Location = new System.Drawing.Point(281, 133);
      this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(87, 23);
      this.buttonOk.TabIndex = 5;
      this.buttonOk.Text = "Ok";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
      // 
      // DialogNewPart
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(374, 162);
      this.Controls.Add(this.baseLayout);
      this.MinimumSize = new System.Drawing.Size(390, 200);
      this.Name = "DialogNewPart";
      this.Padding = new System.Windows.Forms.Padding(3);
      this.Text = "Add a part";
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.Button buttonCancel;
    private Lemoine.BaseControls.ComboboxTextValue comboPart;
    private System.Windows.Forms.TextBox textPartCode;
    private System.Windows.Forms.TextBox textPartName;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button buttonCopy;
    private System.Windows.Forms.RadioButton radioExisting;
    private System.Windows.Forms.RadioButton radioNew;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
