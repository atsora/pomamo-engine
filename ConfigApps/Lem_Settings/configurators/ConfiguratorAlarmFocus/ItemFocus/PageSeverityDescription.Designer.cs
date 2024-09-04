// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarmFocus
{
  partial class PageSeverityDescription
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.textName = new System.Windows.Forms.TextBox();
      this.textDescription = new System.Windows.Forms.RichTextBox();
      this.comboStopStatus = new Lemoine.BaseControls.ComboboxTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 0, 1);
      this.baseLayout.Controls.Add(this.label3, 0, 2);
      this.baseLayout.Controls.Add(this.textName, 1, 0);
      this.baseLayout.Controls.Add(this.textDescription, 1, 1);
      this.baseLayout.Controls.Add(this.comboStopStatus, 1, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(0, 3);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(90, 17);
      this.label1.TabIndex = 0;
      this.label1.Text = "Name";
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 26);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 6, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(90, 244);
      this.label2.TabIndex = 1;
      this.label2.Text = "Description";
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(0, 273);
      this.label3.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(90, 17);
      this.label3.TabIndex = 2;
      this.label3.Text = "Program stopped";
      // 
      // textName
      // 
      this.textName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textName.Location = new System.Drawing.Point(93, 0);
      this.textName.Margin = new System.Windows.Forms.Padding(0);
      this.textName.Name = "textName";
      this.textName.Size = new System.Drawing.Size(277, 20);
      this.textName.TabIndex = 3;
      // 
      // textDescription
      // 
      this.textDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textDescription.Location = new System.Drawing.Point(93, 23);
      this.textDescription.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.textDescription.Name = "textDescription";
      this.textDescription.Size = new System.Drawing.Size(277, 244);
      this.textDescription.TabIndex = 5;
      this.textDescription.Text = "";
      // 
      // comboStopStatus
      // 
      this.comboStopStatus.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboStopStatus.Location = new System.Drawing.Point(93, 270);
      this.comboStopStatus.Margin = new System.Windows.Forms.Padding(0);
      this.comboStopStatus.Name = "comboStopStatus";
      this.comboStopStatus.Size = new System.Drawing.Size(277, 20);
      this.comboStopStatus.TabIndex = 6;
      // 
      // PageSeverityDescription
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PageSeverityDescription";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textName;
    private System.Windows.Forms.RichTextBox textDescription;
    private Lemoine.BaseControls.ComboboxTextValue comboStopStatus;
  }
}
