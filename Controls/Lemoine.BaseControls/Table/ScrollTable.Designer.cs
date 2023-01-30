// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class ScrollTable
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
      Utils.ClearControls(vHeader.Controls);
      Utils.ClearControls(hHeader.Controls);
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
      this.tableBase = new System.Windows.Forms.TableLayoutPanel();
      this.hScroll = new System.Windows.Forms.HScrollBar();
      this.vScroll = new System.Windows.Forms.VScrollBar();
      this.panelTable = new System.Windows.Forms.Panel();
      this.table = new System.Windows.Forms.TableLayoutPanel();
      this.panelVHeader = new System.Windows.Forms.Panel();
      this.vHeader = new System.Windows.Forms.TableLayoutPanel();
      this.panelHHeader = new System.Windows.Forms.Panel();
      this.hHeader = new System.Windows.Forms.TableLayoutPanel();
      this.panelHFooter = new System.Windows.Forms.Panel();
      this.hFooter = new System.Windows.Forms.TableLayoutPanel();
      this.labelHFooter = new System.Windows.Forms.Label();
      this.tableBase.SuspendLayout();
      this.panelTable.SuspendLayout();
      this.panelVHeader.SuspendLayout();
      this.panelHHeader.SuspendLayout();
      this.panelHFooter.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableBase
      // 
      this.tableBase.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
      this.tableBase.ColumnCount = 3;
      this.tableBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
      this.tableBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1F));
      this.tableBase.Controls.Add(this.hScroll, 0, 3);
      this.tableBase.Controls.Add(this.vScroll, 2, 0);
      this.tableBase.Controls.Add(this.panelTable, 1, 1);
      this.tableBase.Controls.Add(this.panelVHeader, 0, 1);
      this.tableBase.Controls.Add(this.panelHHeader, 1, 0);
      this.tableBase.Controls.Add(this.panelHFooter, 1, 2);
      this.tableBase.Controls.Add(this.labelHFooter, 0, 2);
      this.tableBase.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableBase.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
      this.tableBase.Location = new System.Drawing.Point(0, 0);
      this.tableBase.Margin = new System.Windows.Forms.Padding(0);
      this.tableBase.Name = "tableBase";
      this.tableBase.RowCount = 4;
      this.tableBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
      this.tableBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
      this.tableBase.Size = new System.Drawing.Size(300, 200);
      this.tableBase.TabIndex = 0;
      // 
      // hScroll
      // 
      this.tableBase.SetColumnSpan(this.hScroll, 2);
      this.hScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.hScroll.Location = new System.Drawing.Point(1, 186);
      this.hScroll.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
      this.hScroll.Name = "hScroll";
      this.hScroll.Size = new System.Drawing.Size(285, 14);
      this.hScroll.TabIndex = 1;
      this.hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HScrollScroll);
      // 
      // vScroll
      // 
      this.vScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.vScroll.Location = new System.Drawing.Point(286, 1);
      this.vScroll.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
      this.vScroll.Name = "vScroll";
      this.tableBase.SetRowSpan(this.vScroll, 4);
      this.vScroll.Size = new System.Drawing.Size(14, 199);
      this.vScroll.TabIndex = 0;
      this.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.VScrollScroll);
      // 
      // panelTable
      // 
      this.panelTable.BackColor = System.Drawing.SystemColors.Window;
      this.panelTable.Controls.Add(this.table);
      this.panelTable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelTable.Location = new System.Drawing.Point(151, 21);
      this.panelTable.Margin = new System.Windows.Forms.Padding(1);
      this.panelTable.Name = "panelTable";
      this.panelTable.Size = new System.Drawing.Size(134, 144);
      this.panelTable.TabIndex = 5;
      this.panelTable.SizeChanged += new System.EventHandler(this.PanelTableSizeChanged);
      // 
      // table
      // 
      this.table.AutoSize = true;
      this.table.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.table.ColumnCount = 1;
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.table.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
      this.table.Location = new System.Drawing.Point(0, 0);
      this.table.Margin = new System.Windows.Forms.Padding(0);
      this.table.Name = "table";
      this.table.RowCount = 1;
      this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.table.Size = new System.Drawing.Size(0, 0);
      this.table.TabIndex = 4;
      // 
      // panelVHeader
      // 
      this.panelVHeader.Controls.Add(this.vHeader);
      this.panelVHeader.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelVHeader.Location = new System.Drawing.Point(1, 21);
      this.panelVHeader.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.panelVHeader.Name = "panelVHeader";
      this.panelVHeader.Size = new System.Drawing.Size(149, 145);
      this.panelVHeader.TabIndex = 6;
      // 
      // vHeader
      // 
      this.vHeader.AutoSize = true;
      this.vHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.vHeader.ColumnCount = 3;
      this.vHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.vHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
      this.vHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.vHeader.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
      this.vHeader.Location = new System.Drawing.Point(0, 0);
      this.vHeader.Margin = new System.Windows.Forms.Padding(0);
      this.vHeader.MinimumSize = new System.Drawing.Size(150, 0);
      this.vHeader.Name = "vHeader";
      this.vHeader.RowCount = 1;
      this.vHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.vHeader.Size = new System.Drawing.Size(170, 0);
      this.vHeader.TabIndex = 2;
      // 
      // panelHHeader
      // 
      this.panelHHeader.Controls.Add(this.hHeader);
      this.panelHHeader.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelHHeader.Location = new System.Drawing.Point(151, 1);
      this.panelHHeader.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.panelHHeader.Name = "panelHHeader";
      this.panelHHeader.Size = new System.Drawing.Size(135, 19);
      this.panelHHeader.TabIndex = 7;
      // 
      // hHeader
      // 
      this.hHeader.AutoSize = true;
      this.hHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.hHeader.ColumnCount = 1;
      this.hHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.hHeader.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
      this.hHeader.Location = new System.Drawing.Point(0, 0);
      this.hHeader.Margin = new System.Windows.Forms.Padding(0);
      this.hHeader.MinimumSize = new System.Drawing.Size(0, 17);
      this.hHeader.Name = "hHeader";
      this.hHeader.RowCount = 1;
      this.hHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.hHeader.Size = new System.Drawing.Size(0, 17);
      this.hHeader.TabIndex = 3;
      // 
      // panelHFooter
      // 
      this.panelHFooter.Controls.Add(this.hFooter);
      this.panelHFooter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelHFooter.Location = new System.Drawing.Point(151, 167);
      this.panelHFooter.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
      this.panelHFooter.Name = "panelHFooter";
      this.panelHFooter.Size = new System.Drawing.Size(135, 19);
      this.panelHFooter.TabIndex = 8;
      // 
      // hFooter
      // 
      this.hFooter.AutoSize = true;
      this.hFooter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.hFooter.ColumnCount = 1;
      this.hFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.hFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.hFooter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.hFooter.Location = new System.Drawing.Point(0, 0);
      this.hFooter.Margin = new System.Windows.Forms.Padding(0);
      this.hFooter.MinimumSize = new System.Drawing.Size(10, 17);
      this.hFooter.Name = "hFooter";
      this.hFooter.RowCount = 1;
      this.hFooter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.hFooter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
      this.hFooter.Size = new System.Drawing.Size(10, 17);
      this.hFooter.TabIndex = 0;
      // 
      // labelHFooter
      // 
      this.labelHFooter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelHFooter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelHFooter.Location = new System.Drawing.Point(3, 167);
      this.labelHFooter.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
      this.labelHFooter.Name = "labelHFooter";
      this.labelHFooter.Size = new System.Drawing.Size(144, 19);
      this.labelHFooter.TabIndex = 9;
      this.labelHFooter.Text = "Total";
      this.labelHFooter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // ScrollTable
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableBase);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "ScrollTable";
      this.Size = new System.Drawing.Size(300, 200);
      this.tableBase.ResumeLayout(false);
      this.panelTable.ResumeLayout(false);
      this.panelTable.PerformLayout();
      this.panelVHeader.ResumeLayout(false);
      this.panelVHeader.PerformLayout();
      this.panelHHeader.ResumeLayout(false);
      this.panelHHeader.PerformLayout();
      this.panelHFooter.ResumeLayout(false);
      this.panelHFooter.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label labelHFooter;
    private System.Windows.Forms.TableLayoutPanel hFooter;
    private System.Windows.Forms.Panel panelHFooter;
    private System.Windows.Forms.Panel panelHHeader;
    private System.Windows.Forms.Panel panelVHeader;
    private System.Windows.Forms.Panel panelTable;
    private System.Windows.Forms.TableLayoutPanel table;
    private System.Windows.Forms.TableLayoutPanel hHeader;
    private System.Windows.Forms.TableLayoutPanel vHeader;
    private System.Windows.Forms.HScrollBar hScroll;
    private System.Windows.Forms.VScrollBar vScroll;
    private System.Windows.Forms.TableLayoutPanel tableBase;
  }
}
