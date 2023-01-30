// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class EditPreferences
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditPreferences));
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.label2 = new System.Windows.Forms.Label();
      this.comboPerfUnit = new System.Windows.Forms.ComboBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonOk = new System.Windows.Forms.Button();
      this.tabControl.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tableLayoutPanel2.SetColumnSpan(this.tabControl, 3);
      this.tabControl.Controls.Add(this.tabPage1);
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.Location = new System.Drawing.Point(3, 3);
      this.tabControl.Margin = new System.Windows.Forms.Padding(3, 3, 2, 3);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(279, 135);
      this.tabControl.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.tableLayoutPanel1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(271, 109);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.comboPerfUnit, 1, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(265, 103);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(91, 23);
      this.label2.TabIndex = 2;
      this.label2.Text = "Performance unit";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // comboPerfUnit
      // 
      this.comboPerfUnit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboPerfUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboPerfUnit.FormattingEnabled = true;
      this.comboPerfUnit.Location = new System.Drawing.Point(97, 3);
      this.comboPerfUnit.Name = "comboPerfUnit";
      this.comboPerfUnit.Size = new System.Drawing.Size(165, 21);
      this.comboPerfUnit.TabIndex = 4;
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 3;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.tableLayoutPanel2.Controls.Add(this.tabControl, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.buttonOk, 2, 1);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 2;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(284, 166);
      this.tableLayoutPanel2.TabIndex = 1;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCancel.Location = new System.Drawing.Point(147, 141);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(64, 22);
      this.buttonCancel.TabIndex = 1;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
      // 
      // buttonOk
      // 
      this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOk.Location = new System.Drawing.Point(217, 141);
      this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(64, 22);
      this.buttonOk.TabIndex = 2;
      this.buttonOk.Text = "Ok";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
      // 
      // EditPreferences
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 166);
      this.Controls.Add(this.tableLayoutPanel2);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EditPreferences";
      this.ShowInTaskbar = false;
      this.Text = "Preferences";
      this.TopMost = true;
      this.tabControl.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.ComboBox comboPerfUnit;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.TabControl tabControl;
  }
}
