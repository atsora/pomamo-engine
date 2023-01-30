// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots.DayTemplate
{
  partial class DayTemplatePage3
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DayTemplatePage3));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label3 = new System.Windows.Forms.Label();
      this.listDayTemplate = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label3, 0, 0);
      this.baseLayout.Controls.Add(this.listDayTemplate, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(322, 150);
      this.baseLayout.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(84, 27);
      this.label3.TabIndex = 1;
      this.label3.Text = "Day template";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // listDayTemplate
      // 
      this.listDayTemplate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listDayTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listDayTemplate.Location = new System.Drawing.Point(90, 0);
      this.listDayTemplate.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.listDayTemplate.MultipleSelection = false;
      this.listDayTemplate.Name = "listDayTemplate";
      this.baseLayout.SetRowSpan(this.listDayTemplate, 2);
      this.listDayTemplate.SelectedIndex = -1;
      this.listDayTemplate.SelectedIndexes = ((System.Collections.Generic.IList<int>)(resources.GetObject("listDayTemplate.SelectedIndexes")));
      this.listDayTemplate.SelectedText = "";
      this.listDayTemplate.SelectedTexts = ((System.Collections.Generic.IList<string>)(resources.GetObject("listDayTemplate.SelectedTexts")));
      this.listDayTemplate.SelectedValue = null;
      this.listDayTemplate.SelectedValues = ((System.Collections.Generic.IList<object>)(resources.GetObject("listDayTemplate.SelectedValues")));
      this.listDayTemplate.Size = new System.Drawing.Size(229, 147);
      this.listDayTemplate.Sorted = true;
      this.listDayTemplate.TabIndex = 4;
      // 
      // DayTemplatePage3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "DayTemplatePage3";
      this.Size = new System.Drawing.Size(322, 150);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.List.ListTextValue listDayTemplate;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
