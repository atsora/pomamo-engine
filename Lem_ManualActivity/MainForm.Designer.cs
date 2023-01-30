// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_ManualActivity
{
  partial class MainForm
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
      this.machineModeSelection1 = new Lemoine.DataReferenceControls.MachineModeSelection();
      this.button1 = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.dateTimePicker_End = new Lemoine.BaseControls.DateTimePicker();
      this.dateTimePicker_Start = new Lemoine.BaseControls.DateTimePicker();
      this.checkBox_noEndDate = new System.Windows.Forms.CheckBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.machineTreeView = new Lemoine.DataReferenceControls.DisplayableTreeView();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // machineModeSelection1
      // 
      this.machineModeSelection1.AutoSize = true;
      this.machineModeSelection1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModeSelection1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.machineModeSelection1.Location = new System.Drawing.Point(4, 16);
      this.machineModeSelection1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineModeSelection1.Name = "machineModeSelection1";
      this.machineModeSelection1.Size = new System.Drawing.Size(370, 226);
      this.machineModeSelection1.TabIndex = 3;
      // 
      // button1
      // 
      this.button1.Dock = System.Windows.Forms.DockStyle.Right;
      this.button1.Location = new System.Drawing.Point(663, 365);
      this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(105, 36);
      this.button1.TabIndex = 5;
      this.button1.Text = "Validate";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.Button1Click);
      // 
      // groupBox1
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
      this.groupBox1.Controls.Add(this.tableLayoutPanel2);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.groupBox1.Location = new System.Drawing.Point(4, 254);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.groupBox1.Size = new System.Drawing.Size(764, 105);
      this.groupBox1.TabIndex = 6;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Date range";
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 2;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.Controls.Add(this.dateTimePicker_End, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.dateTimePicker_Start, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.checkBox_noEndDate, 1, 2);
      this.tableLayoutPanel2.Controls.Add(this.label4, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.label5, 0, 1);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 16);
      this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 3;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(756, 86);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // dateTimePicker_End
      // 
      this.dateTimePicker_End.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateTimePicker_End.Location = new System.Drawing.Point(90, 33);
      this.dateTimePicker_End.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dateTimePicker_End.Name = "dateTimePicker_End";
      this.dateTimePicker_End.Size = new System.Drawing.Size(662, 20);
      this.dateTimePicker_End.TabIndex = 3;
      this.dateTimePicker_End.Value = new System.DateTime(2020, 11, 26, 14, 47, 20, 79);
      this.dateTimePicker_End.WithSeconds = true;
      // 
      // dateTimePicker_Start
      // 
      this.dateTimePicker_Start.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateTimePicker_Start.Location = new System.Drawing.Point(90, 3);
      this.dateTimePicker_Start.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dateTimePicker_Start.Name = "dateTimePicker_Start";
      this.dateTimePicker_Start.Size = new System.Drawing.Size(662, 20);
      this.dateTimePicker_Start.TabIndex = 2;
      this.dateTimePicker_Start.Value = new System.DateTime(2020, 11, 26, 14, 47, 20, 81);
      this.dateTimePicker_Start.WithSeconds = true;
      // 
      // checkBox_noEndDate
      // 
      this.checkBox_noEndDate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBox_noEndDate.Location = new System.Drawing.Point(90, 63);
      this.checkBox_noEndDate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.checkBox_noEndDate.Name = "checkBox_noEndDate";
      this.checkBox_noEndDate.Size = new System.Drawing.Size(662, 20);
      this.checkBox_noEndDate.TabIndex = 4;
      this.checkBox_noEndDate.Text = "No End Date";
      this.checkBox_noEndDate.UseVisualStyleBackColor = true;
      this.checkBox_noEndDate.CheckedChanged += new System.EventHandler(this.CheckBox_noEndDateCheckedChanged);
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(4, 0);
      this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(78, 30);
      this.label4.TabIndex = 0;
      this.label4.Text = "From";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(4, 30);
      this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(78, 30);
      this.label5.TabIndex = 1;
      this.label5.Text = "To";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.machineModeSelection1);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.groupBox2.Location = new System.Drawing.Point(390, 3);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.groupBox2.Size = new System.Drawing.Size(378, 245);
      this.groupBox2.TabIndex = 7;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Machine Modes";
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.machineTreeView);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.groupBox3.Location = new System.Drawing.Point(4, 3);
      this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.groupBox3.Size = new System.Drawing.Size(378, 245);
      this.groupBox3.TabIndex = 8;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Machines";
      // 
      // machineTreeView
      // 
      this.machineTreeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.machineTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.machineTreeView.Location = new System.Drawing.Point(4, 16);
      this.machineTreeView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineTreeView.MultiSelection = true;
      this.machineTreeView.Name = "machineTreeView";
      this.machineTreeView.Size = new System.Drawing.Size(370, 226);
      this.machineTreeView.TabIndex = 0;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.button1, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 111F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(772, 404);
      this.tableLayoutPanel1.TabIndex = 9;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(772, 404);
      this.Controls.Add(this.tableLayoutPanel1);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "MainForm";
      this.Text = "Lem_ManualActivity";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.Load += new System.EventHandler(this.MainFormLoad);
      this.groupBox1.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox checkBox_noEndDate;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button button1;
    private Lemoine.DataReferenceControls.MachineModeSelection machineModeSelection1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private Lemoine.DataReferenceControls.DisplayableTreeView machineTreeView;
    private Lemoine.BaseControls.DateTimePicker dateTimePicker_End;
    private Lemoine.BaseControls.DateTimePicker dateTimePicker_Start;
  }
}
