// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class VerticalScrollLayout
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
      Utils.ClearControls(flow.Controls);
      Utils.ClearControls(table.Controls);
      
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
      this.panel = new System.Windows.Forms.Panel();
      this.flow = new System.Windows.Forms.FlowLayoutPanel();
      this.table = new System.Windows.Forms.TableLayoutPanel();
      this.labelTitle = new System.Windows.Forms.Label();
      this.vScroll = new System.Windows.Forms.VScrollBar();
      this.baseLayout.SuspendLayout();
      this.panel.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.baseLayout.Controls.Add(this.panel, 0, 1);
      this.baseLayout.Controls.Add(this.labelTitle, 0, 0);
      this.baseLayout.Controls.Add(this.vScroll, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(60, 80);
      this.baseLayout.TabIndex = 0;
      // 
      // panel
      // 
      this.panel.Controls.Add(this.flow);
      this.panel.Controls.Add(this.table);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(0, 14);
      this.panel.Margin = new System.Windows.Forms.Padding(0);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(46, 66);
      this.panel.TabIndex = 1;
      this.panel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PanelMouseDoubleClick);
      // 
      // flow
      // 
      this.flow.AutoSize = true;
      this.flow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.flow.Location = new System.Drawing.Point(0, 0);
      this.flow.Margin = new System.Windows.Forms.Padding(0);
      this.flow.MinimumSize = new System.Drawing.Size(20, 10);
      this.flow.Name = "flow";
      this.flow.Size = new System.Drawing.Size(20, 10);
      this.flow.TabIndex = 1;
      this.flow.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.FlowMouseDoubleClick);
      // 
      // table
      // 
      this.table.AutoSize = true;
      this.table.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.table.ColumnCount = 1;
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.table.Location = new System.Drawing.Point(0, 0);
      this.table.Margin = new System.Windows.Forms.Padding(0);
      this.table.MinimumSize = new System.Drawing.Size(10, 20);
      this.table.Name = "table";
      this.table.RowCount = 1;
      this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.table.Size = new System.Drawing.Size(10, 20);
      this.table.TabIndex = 0;
      this.table.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TableMouseClick);
      this.table.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TableMouseDoubleClick);
      // 
      // labelTitle
      // 
      this.labelTitle.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelTitle.Location = new System.Drawing.Point(0, 0);
      this.labelTitle.Margin = new System.Windows.Forms.Padding(0);
      this.labelTitle.Name = "labelTitle";
      this.labelTitle.Size = new System.Drawing.Size(46, 14);
      this.labelTitle.TabIndex = 2;
      this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.labelTitle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LabelTitleMouseDoubleClick);
      // 
      // vScroll
      // 
      this.vScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.vScroll.Location = new System.Drawing.Point(46, 0);
      this.vScroll.Name = "vScroll";
      this.baseLayout.SetRowSpan(this.vScroll, 2);
      this.vScroll.Size = new System.Drawing.Size(14, 80);
      this.vScroll.TabIndex = 0;
      this.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.VScrollScroll);
      // 
      // VerticalScrollLayout
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "VerticalScrollLayout";
      this.Size = new System.Drawing.Size(60, 80);
      this.SizeChanged += new System.EventHandler(this.VerticalScrollLayoutSizeChanged);
      this.baseLayout.ResumeLayout(false);
      this.panel.ResumeLayout(false);
      this.panel.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Label labelTitle;
    private System.Windows.Forms.FlowLayoutPanel flow;
    private System.Windows.Forms.TableLayoutPanel table;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.VScrollBar vScroll;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
