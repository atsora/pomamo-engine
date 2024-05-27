// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class ChangePeriodDialog
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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.date1 = new Lemoine.BaseControls.DatePicker();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonOk = new System.Windows.Forms.Button();
      this.date2 = new Lemoine.BaseControls.DatePicker();
      this.time1 = new Lemoine.BaseControls.TimePicker();
      this.time2 = new Lemoine.BaseControls.TimePicker();
      this.baseLayout.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 43F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42F));
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 0, 1);
      this.baseLayout.Controls.Add(this.date1, 1, 0);
      this.baseLayout.Controls.Add(this.tableLayoutPanel1, 0, 3);
      this.baseLayout.Controls.Add(this.date2, 1, 1);
      this.baseLayout.Controls.Add(this.time1, 2, 0);
      this.baseLayout.Controls.Add(this.time2, 2, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.Size = new System.Drawing.Size(224, 76);
      this.baseLayout.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(37, 24);
      this.label1.TabIndex = 0;
      this.label1.Text = "Start";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(37, 24);
      this.label2.TabIndex = 1;
      this.label2.Text = "End";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // date1
      // 
      this.date1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.date1.Location = new System.Drawing.Point(46, 3);
      this.date1.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.date1.Name = "date1";
      this.date1.Size = new System.Drawing.Size(101, 20);
      this.date1.TabIndex = 2;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.baseLayout.SetColumnSpan(this.tableLayoutPanel1, 3);
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.buttonOk, 2, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 46);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(224, 30);
      this.tableLayoutPanel1.TabIndex = 6;
      // 
      // buttonCancel
      // 
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCancel.Location = new System.Drawing.Point(87, 3);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(64, 24);
      this.buttonCancel.TabIndex = 0;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      // 
      // buttonOk
      // 
      this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOk.Location = new System.Drawing.Point(157, 3);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(64, 24);
      this.buttonOk.TabIndex = 1;
      this.buttonOk.Text = "Ok";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
      // 
      // date2
      // 
      this.date2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.date2.Location = new System.Drawing.Point(46, 27);
      this.date2.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.date2.Name = "date2";
      this.date2.Size = new System.Drawing.Size(101, 20);
      this.date2.TabIndex = 7;
      // 
      // time1
      // 
      this.time1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.time1.Location = new System.Drawing.Point(150, 3);
      this.time1.Name = "time1";
      this.time1.Size = new System.Drawing.Size(71, 20);
      this.time1.TabIndex = 8;
      this.time1.Time = System.TimeSpan.Parse("23:17:58.5292520");
      // 
      // time2
      // 
      this.time2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.time2.Location = new System.Drawing.Point(150, 27);
      this.time2.Name = "time2";
      this.time2.Size = new System.Drawing.Size(71, 20);
      this.time2.TabIndex = 9;
      this.time2.Time = System.TimeSpan.Parse("23:17:58.5357901");
      // 
      // ChangePeriodDialog
      // 
      this.AcceptButton = this.buttonOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(224, 76);
      this.Controls.Add(this.baseLayout);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(240, 115);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(240, 115);
      this.Name = "ChangePeriodDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Change displayed period";
      this.TopMost = true;
      this.baseLayout.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private Lemoine.BaseControls.TimePicker time2;
    private Lemoine.BaseControls.TimePicker time1;
    private Lemoine.BaseControls.DatePicker date2;
    private Lemoine.BaseControls.DatePicker date1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
