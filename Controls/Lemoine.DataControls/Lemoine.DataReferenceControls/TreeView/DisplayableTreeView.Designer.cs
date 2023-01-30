// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class DisplayableTreeView
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
      this.comboBox = new System.Windows.Forms.ComboBox();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.treeView = new Lemoine.BaseControls.CustomTreeView();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // comboBox
      // 
      this.comboBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.comboBox.FormattingEnabled = true;
      this.comboBox.Location = new System.Drawing.Point(0, 95);
      this.comboBox.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
      this.comboBox.Name = "comboBox";
      this.comboBox.Size = new System.Drawing.Size(100, 21);
      this.comboBox.TabIndex = 1;
      this.comboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSelectedIndexChanged);
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.comboBox, 0, 1);
      this.baseLayout.Controls.Add(this.treeView, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
      this.baseLayout.Size = new System.Drawing.Size(100, 115);
      this.baseLayout.TabIndex = 2;
      // 
      // treeView
      // 
      this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView.Location = new System.Drawing.Point(0, 0);
      this.treeView.Margin = new System.Windows.Forms.Padding(0);
      this.treeView.Name = "treeView";
      this.treeView.Size = new System.Drawing.Size(100, 94);
      this.treeView.TabIndex = 2;
      // 
      // DisplayableTreeView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "DisplayableTreeView";
      this.Size = new System.Drawing.Size(100, 115);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.ComboBox comboBox;
    private Lemoine.BaseControls.CustomTreeView treeView;
  }
}
