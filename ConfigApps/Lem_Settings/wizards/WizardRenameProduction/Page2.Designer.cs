// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardRenameProduction
{
  partial class Page2
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
      this.listProductions = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.checkShowAll = new System.Windows.Forms.CheckBox();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // listProductions
      // 
      this.listProductions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listProductions.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listProductions.Location = new System.Drawing.Point(0, 0);
      this.listProductions.Margin = new System.Windows.Forms.Padding(0);
      this.listProductions.Name = "listProductions";
      this.listProductions.SelectedIndex = -1;
      this.listProductions.SelectedIndexes = null;
      this.listProductions.SelectedText = "";
      this.listProductions.SelectedTexts = null;
      this.listProductions.SelectedValue = null;
      this.listProductions.SelectedValues = null;
      this.listProductions.Size = new System.Drawing.Size(370, 270);
      this.listProductions.Sorted = true;
      this.listProductions.TabIndex = 0;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.listProductions, 0, 0);
      this.baseLayout.Controls.Add(this.checkShowAll, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 1;
      // 
      // checkShowAll
      // 
      this.checkShowAll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkShowAll.Location = new System.Drawing.Point(0, 273);
      this.checkShowAll.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.checkShowAll.Name = "checkShowAll";
      this.checkShowAll.Size = new System.Drawing.Size(370, 17);
      this.checkShowAll.TabIndex = 1;
      this.checkShowAll.Text = "Show previous production periods";
      this.checkShowAll.UseVisualStyleBackColor = true;
      this.checkShowAll.CheckedChanged += new System.EventHandler(this.CheckShowAllCheckedChanged);
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.List.ListTextValue listProductions;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.CheckBox checkShowAll;
  }
}
