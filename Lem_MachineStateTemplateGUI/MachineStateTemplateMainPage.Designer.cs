// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_MachineStateTemplateGUI
{
  partial class MachineStateTemplateMainPage
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
      this.machineTreeViewBox = new System.Windows.Forms.GroupBox();
      this.machineTreeView = new Lemoine.DataReferenceControls.DisplayableTreeView();
      this.forceCheckBox = new System.Windows.Forms.CheckBox();
      this.addButton = new System.Windows.Forms.Button();
      this.machineStateTemplateGroupBox = new System.Windows.Forms.GroupBox();
      this.machineStateTemplateSelection = new Lemoine.DataReferenceControls.MachineStateTemplateSelection();
      this.shiftGroupBox = new System.Windows.Forms.GroupBox();
      this.shiftCheckBox = new System.Windows.Forms.CheckBox();
      this.shiftSelection = new Lemoine.DataReferenceControls.ShiftSelection();
      this.beginDateTimePicker = new System.Windows.Forms.DateTimePicker();
      this.beginDateLabel = new System.Windows.Forms.Label();
      this.endDateCheckBox = new System.Windows.Forms.CheckBox();
      this.endDateTimePicker = new System.Windows.Forms.DateTimePicker();
      this.endDateLabel = new System.Windows.Forms.Label();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.machineTreeViewBox.SuspendLayout();
      this.machineStateTemplateGroupBox.SuspendLayout();
      this.shiftGroupBox.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // machineTreeViewBox
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.machineTreeViewBox, 2);
      this.machineTreeViewBox.Controls.Add(this.machineTreeView);
      this.machineTreeViewBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineTreeViewBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.machineTreeViewBox.Location = new System.Drawing.Point(3, 249);
      this.machineTreeViewBox.Name = "machineTreeViewBox";
      this.machineTreeViewBox.Size = new System.Drawing.Size(481, 140);
      this.machineTreeViewBox.TabIndex = 0;
      this.machineTreeViewBox.TabStop = false;
      this.machineTreeViewBox.Text = "Select machines";
      // 
      // machineTreeView
      // 
      this.machineTreeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.machineTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.machineTreeView.Location = new System.Drawing.Point(3, 16);
      this.machineTreeView.MultiSelection = true;
      this.machineTreeView.Name = "machineTreeView";
      this.machineTreeView.Size = new System.Drawing.Size(475, 121);
      this.machineTreeView.TabIndex = 0;
      // 
      // forceCheckBox
      // 
      this.forceCheckBox.Checked = true;
      this.forceCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.forceCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.forceCheckBox.Location = new System.Drawing.Point(3, 395);
      this.forceCheckBox.Name = "forceCheckBox";
      this.forceCheckBox.Size = new System.Drawing.Size(237, 34);
      this.forceCheckBox.TabIndex = 0;
      this.forceCheckBox.Text = "Force re-building the state templates";
      this.forceCheckBox.UseVisualStyleBackColor = true;
      // 
      // addButton
      // 
      this.addButton.Dock = System.Windows.Forms.DockStyle.Fill;
      this.addButton.Location = new System.Drawing.Point(246, 395);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(238, 34);
      this.addButton.TabIndex = 7;
      this.addButton.Text = "button2";
      this.addButton.UseVisualStyleBackColor = false;
      this.addButton.Click += new System.EventHandler(this.AddButtonClick);
      // 
      // machineStateTemplateGroupBox
      // 
      this.machineStateTemplateGroupBox.Controls.Add(this.machineStateTemplateSelection);
      this.machineStateTemplateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.machineStateTemplateGroupBox.Location = new System.Drawing.Point(3, 3);
      this.machineStateTemplateGroupBox.Name = "machineStateTemplateGroupBox";
      this.machineStateTemplateGroupBox.Size = new System.Drawing.Size(237, 140);
      this.machineStateTemplateGroupBox.TabIndex = 0;
      this.machineStateTemplateGroupBox.TabStop = false;
      this.machineStateTemplateGroupBox.Text = "groupBox1";
      // 
      // machineStateTemplateSelection
      // 
      this.machineStateTemplateSelection.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.machineStateTemplateSelection.Location = new System.Drawing.Point(3, 16);
      this.machineStateTemplateSelection.Name = "machineStateTemplateSelection";
      this.machineStateTemplateSelection.SelectedValue = null;
      this.machineStateTemplateSelection.Size = new System.Drawing.Size(231, 121);
      this.machineStateTemplateSelection.TabIndex = 0;
      // 
      // shiftGroupBox
      // 
      this.shiftGroupBox.Controls.Add(this.shiftCheckBox);
      this.shiftGroupBox.Controls.Add(this.shiftSelection);
      this.shiftGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.shiftGroupBox.Location = new System.Drawing.Point(246, 3);
      this.shiftGroupBox.Name = "shiftGroupBox";
      this.shiftGroupBox.Size = new System.Drawing.Size(238, 140);
      this.shiftGroupBox.TabIndex = 0;
      this.shiftGroupBox.TabStop = false;
      this.shiftGroupBox.Text = "groupBox2";
      // 
      // shiftCheckBox
      // 
      this.shiftCheckBox.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.shiftCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.shiftCheckBox.Location = new System.Drawing.Point(3, 113);
      this.shiftCheckBox.Name = "shiftCheckBox";
      this.shiftCheckBox.Size = new System.Drawing.Size(232, 24);
      this.shiftCheckBox.TabIndex = 2;
      this.shiftCheckBox.Text = "Add shift";
      this.shiftCheckBox.UseVisualStyleBackColor = true;
      this.shiftCheckBox.CheckedChanged += new System.EventHandler(this.ShiftCheckBoxCheckedChanged);
      // 
      // shiftSelection
      // 
      this.shiftSelection.Dock = System.Windows.Forms.DockStyle.Top;
      this.shiftSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.shiftSelection.Location = new System.Drawing.Point(3, 16);
      this.shiftSelection.Name = "shiftSelection";
      this.shiftSelection.SelectedShift = null;
      this.shiftSelection.Size = new System.Drawing.Size(232, 120);
      this.shiftSelection.TabIndex = 1;
      // 
      // beginDateTimePicker
      // 
      this.beginDateTimePicker.CustomFormat = "dd/MM/yyyy HH:mm:ss";
      this.beginDateTimePicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.beginDateTimePicker.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.beginDateTimePicker.Location = new System.Drawing.Point(60, 3);
      this.beginDateTimePicker.Name = "beginDateTimePicker";
      this.beginDateTimePicker.Size = new System.Drawing.Size(412, 20);
      this.beginDateTimePicker.TabIndex = 3;
      this.beginDateTimePicker.Value = new System.DateTime(2014, 11, 13, 11, 46, 37, 0);
      this.beginDateTimePicker.ValueChanged += new System.EventHandler(this.BeginDateTimePickerValueChanged);
      // 
      // beginDateLabel
      // 
      this.beginDateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.beginDateLabel.Location = new System.Drawing.Point(3, 0);
      this.beginDateLabel.Name = "beginDateLabel";
      this.beginDateLabel.Size = new System.Drawing.Size(51, 25);
      this.beginDateLabel.TabIndex = 0;
      this.beginDateLabel.Text = "label1";
      this.beginDateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // endDateCheckBox
      // 
      this.endDateCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.endDateCheckBox.Location = new System.Drawing.Point(60, 53);
      this.endDateCheckBox.Name = "endDateCheckBox";
      this.endDateCheckBox.Size = new System.Drawing.Size(412, 19);
      this.endDateCheckBox.TabIndex = 2;
      this.endDateCheckBox.Text = "checkBox1";
      this.endDateCheckBox.UseVisualStyleBackColor = true;
      this.endDateCheckBox.CheckStateChanged += new System.EventHandler(this.EndDateCheckBoxCheckStateChanged);
      // 
      // endDateTimePicker
      // 
      this.endDateTimePicker.CustomFormat = "dd/MM/yyyy HH:mm:ss";
      this.endDateTimePicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.endDateTimePicker.Location = new System.Drawing.Point(60, 28);
      this.endDateTimePicker.Name = "endDateTimePicker";
      this.endDateTimePicker.Size = new System.Drawing.Size(412, 20);
      this.endDateTimePicker.TabIndex = 4;
      this.endDateTimePicker.Value = new System.DateTime(2014, 11, 13, 11, 46, 41, 0);
      this.endDateTimePicker.ValueChanged += new System.EventHandler(this.EndDateTimePickerValueChanged);
      // 
      // endDateLabel
      // 
      this.endDateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.endDateLabel.Location = new System.Drawing.Point(3, 25);
      this.endDateLabel.Name = "endDateLabel";
      this.endDateLabel.Size = new System.Drawing.Size(51, 25);
      this.endDateLabel.TabIndex = 0;
      this.endDateLabel.Text = "label2";
      this.endDateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.addButton, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.forceCheckBox, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.machineTreeViewBox, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.shiftGroupBox, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.machineStateTemplateGroupBox, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 4;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(487, 432);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // groupBox1
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
      this.groupBox1.Controls.Add(this.tableLayoutPanel2);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox1.Location = new System.Drawing.Point(3, 149);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(481, 94);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Date range";
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 2;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 57F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.Controls.Add(this.beginDateTimePicker, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.endDateTimePicker, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.endDateLabel, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.beginDateLabel, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.endDateCheckBox, 1, 2);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 3;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(475, 75);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // MachineStateTemplateMainPage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "MachineStateTemplateMainPage";
      this.Size = new System.Drawing.Size(487, 432);
      this.Load += new System.EventHandler(this.MachineStateTemplateMainPageLoad);
      this.machineTreeViewBox.ResumeLayout(false);
      this.machineStateTemplateGroupBox.ResumeLayout(false);
      this.shiftGroupBox.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.CheckBox forceCheckBox;
    private System.Windows.Forms.CheckBox shiftCheckBox;
    private System.Windows.Forms.Button addButton;
    private System.Windows.Forms.Label endDateLabel;
    private System.Windows.Forms.DateTimePicker endDateTimePicker;
    private System.Windows.Forms.CheckBox endDateCheckBox;
    private System.Windows.Forms.Label beginDateLabel;
    private System.Windows.Forms.DateTimePicker beginDateTimePicker;
    private Lemoine.DataReferenceControls.DisplayableTreeView machineTreeView;
    private Lemoine.DataReferenceControls.MachineStateTemplateSelection machineStateTemplateSelection;
    private Lemoine.DataReferenceControls.ShiftSelection shiftSelection;
    private System.Windows.Forms.GroupBox machineTreeViewBox;
    private System.Windows.Forms.GroupBox shiftGroupBox;
    private System.Windows.Forms.GroupBox machineStateTemplateGroupBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
  }
}
