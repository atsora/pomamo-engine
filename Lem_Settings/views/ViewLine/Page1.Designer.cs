// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewLine
{
  partial class Page1
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
      m_machineDetails.Dispose();
      m_lineDetails.Dispose();
      m_operationDetails.Dispose();
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
      this.treeView = new System.Windows.Forms.TreeView();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelDetails = new System.Windows.Forms.Label();
      this.panel = new System.Windows.Forms.Panel();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeView
      // 
      this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView.HideSelection = false;
      this.treeView.Location = new System.Drawing.Point(0, 0);
      this.treeView.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.treeView.Name = "treeView";
      this.baseLayout.SetRowSpan(this.treeView, 2);
      this.treeView.Size = new System.Drawing.Size(172, 250);
      this.treeView.TabIndex = 0;
      this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterSelect);
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.treeView, 0, 0);
      this.baseLayout.Controls.Add(this.labelDetails, 1, 0);
      this.baseLayout.Controls.Add(this.panel, 1, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 1;
      // 
      // labelDetails
      // 
      this.labelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelDetails.Location = new System.Drawing.Point(178, 0);
      this.labelDetails.Name = "labelDetails";
      this.labelDetails.Size = new System.Drawing.Size(169, 25);
      this.labelDetails.TabIndex = 1;
      this.labelDetails.Text = "Details";
      this.labelDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // panel
      // 
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(175, 28);
      this.panel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(175, 222);
      this.panel.TabIndex = 2;
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.Label labelDetails;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.TreeView treeView;
  }
}
