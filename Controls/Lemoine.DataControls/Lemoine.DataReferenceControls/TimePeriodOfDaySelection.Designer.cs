// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class TimePeriodOfDaySelection
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
      this.components = new System.ComponentModel.Container();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.nullCheckBox = new System.Windows.Forms.CheckBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.beginDateTimePicker = new System.Windows.Forms.DateTimePicker();
      this.endDateTimePicker = new System.Windows.Forms.DateTimePicker();
      this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
      this.tableLayoutPanel1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.nullCheckBox, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(228, 68);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // nullCheckBox
      // 
      this.nullCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nullCheckBox.Location = new System.Drawing.Point(3, 41);
      this.nullCheckBox.Name = "nullCheckBox";
      this.nullCheckBox.Size = new System.Drawing.Size(617, 24);
      this.nullCheckBox.TabIndex = 2;
      this.nullCheckBox.Text = "No <% outPut.Append(modelName); %>";
      this.nullCheckBox.UseVisualStyleBackColor = true;
      this.nullCheckBox.CheckedChanged += new System.EventHandler(this.NullCheckBoxCheckedChanged);
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 2;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 111F));
      this.tableLayoutPanel2.Controls.Add(this.beginDateTimePicker, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.endDateTimePicker, 1, 0);
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 1;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.36364F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.63636F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(222, 30);
      this.tableLayoutPanel2.TabIndex = 3;
      // 
      // beginDateTimePicker
      // 
      this.beginDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
      this.beginDateTimePicker.Location = new System.Drawing.Point(3, 3);
      this.beginDateTimePicker.Name = "beginDateTimePicker";
      this.beginDateTimePicker.ShowUpDown = true;
      this.beginDateTimePicker.Size = new System.Drawing.Size(90, 20);
      this.beginDateTimePicker.TabIndex = 1;
      this.beginDateTimePicker.ValueChanged += new System.EventHandler(this.BeginDateTimePickerValueChanged);
      // 
      // endDateTimePicker
      // 
      this.endDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
      this.endDateTimePicker.Location = new System.Drawing.Point(114, 3);
      this.endDateTimePicker.Name = "endDateTimePicker";
      this.endDateTimePicker.ShowUpDown = true;
      this.endDateTimePicker.Size = new System.Drawing.Size(90, 20);
      this.endDateTimePicker.TabIndex = 2;
      this.endDateTimePicker.ValueChanged += new System.EventHandler(this.EndDateTimePickerValueChanged);
      // 
      // errorProvider1
      // 
      this.errorProvider1.ContainerControl = this;
      // 
      // TimePeriodOfDaySelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "TimePeriodOfDaySelection";
      this.Size = new System.Drawing.Size(228, 68);
      this.Load += new System.EventHandler(this.TimePeriodOfDaySelectionLoad);
      this.Validating += new System.ComponentModel.CancelEventHandler(this.TimePeriodOfDaySelectionValidating);
      this.Validated += new System.EventHandler(this.TimePeriodOfDaySelectionValidated);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.ErrorProvider errorProvider1;
    private System.Windows.Forms.DateTimePicker endDateTimePicker;
    private System.Windows.Forms.DateTimePicker beginDateTimePicker;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.CheckBox nullCheckBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
