// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardInitialConfiguration
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
      this.checkToolLifeEvents = new System.Windows.Forms.CheckBox();
      this.checkPeriodEvents = new System.Windows.Forms.CheckBox();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.checkToolLifeEvents, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.checkPeriodEvents, 0, 2);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 4;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(398, 263);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // checkToolLifeEvents
      // 
      this.checkToolLifeEvents.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.checkToolLifeEvents.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkToolLifeEvents.Location = new System.Drawing.Point(0, 61);
      this.checkToolLifeEvents.Margin = new System.Windows.Forms.Padding(0);
      this.checkToolLifeEvents.Name = "checkToolLifeEvents";
      this.checkToolLifeEvents.Size = new System.Drawing.Size(398, 70);
      this.checkToolLifeEvents.TabIndex = 0;
      this.checkToolLifeEvents.Text = resources.GetString("checkToolLifeEvents.Text");
      this.checkToolLifeEvents.TextAlign = System.Drawing.ContentAlignment.TopLeft;
      this.checkToolLifeEvents.UseVisualStyleBackColor = true;
      // 
      // checkPeriodEvents
      // 
      this.checkPeriodEvents.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.checkPeriodEvents.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkPeriodEvents.Location = new System.Drawing.Point(0, 131);
      this.checkPeriodEvents.Margin = new System.Windows.Forms.Padding(0);
      this.checkPeriodEvents.Name = "checkPeriodEvents";
      this.checkPeriodEvents.Size = new System.Drawing.Size(398, 70);
      this.checkPeriodEvents.TabIndex = 1;
      this.checkPeriodEvents.Text = "Add default long period events:\r\n  - warning after an unexpected idle time of 10 " +
  "min,\r\n  - error after an unexpected idle time of 20 min.";
      this.checkPeriodEvents.TextAlign = System.Drawing.ContentAlignment.TopLeft;
      this.checkPeriodEvents.UseVisualStyleBackColor = true;
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(398, 263);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.CheckBox checkToolLifeEvents;
    private System.Windows.Forms.CheckBox checkPeriodEvents;
  }
}
