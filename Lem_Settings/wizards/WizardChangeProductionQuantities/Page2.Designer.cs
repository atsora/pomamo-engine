// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardChangeProductionQuantities
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page2));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.scrollTable = new Lemoine.BaseControls.ScrollTable();
      this.checkDispAllShifts = new System.Windows.Forms.CheckBox();
      this.horizontalBar = new System.Windows.Forms.Label();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.scrollTable, 0, 0);
      this.baseLayout.Controls.Add(this.checkDispAllShifts, 0, 2);
      this.baseLayout.Controls.Add(this.horizontalBar, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 2F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
      // 
      // scrollTable
      // 
      this.scrollTable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.scrollTable.HorizontalMenu = null;
      this.scrollTable.Location = new System.Drawing.Point(0, 0);
      this.scrollTable.Margin = new System.Windows.Forms.Padding(0);
      this.scrollTable.Name = "scrollTable";
      this.scrollTable.Size = new System.Drawing.Size(370, 264);
      this.scrollTable.TabIndex = 7;
      this.scrollTable.VerticalMenu = null;
      this.scrollTable.CellChanged += new System.Action<int, int>(this.ScrollTableCellChanged);
      this.scrollTable.VerticalMenuClicked += new System.Action<int, int>(this.ScrollTableVerticalMenuClicked);
      this.scrollTable.HorizontalMenuClicked += new System.Action<int, int>(this.ScrollTableHorizontalMenuClicked);
      this.scrollTable.VerticalMenuOpen += new System.Action<int>(this.ScrollTableVerticalMenuOpen);
      this.scrollTable.VerticalMenuClosed += new System.Action<int>(this.ScrollTableVerticalMenuClosed);
      // 
      // checkDispAllShifts
      // 
      this.checkDispAllShifts.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkDispAllShifts.Location = new System.Drawing.Point(3, 269);
      this.checkDispAllShifts.Name = "checkDispAllShifts";
      this.checkDispAllShifts.Size = new System.Drawing.Size(364, 18);
      this.checkDispAllShifts.TabIndex = 8;
      this.checkDispAllShifts.Text = "Display all shifts";
      this.checkDispAllShifts.UseVisualStyleBackColor = true;
      this.checkDispAllShifts.CheckedChanged += new System.EventHandler(this.CheckDispAllShiftsCheckedChanged);
      // 
      // horizontalBar
      // 
      this.horizontalBar.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.horizontalBar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.horizontalBar.Location = new System.Drawing.Point(0, 264);
      this.horizontalBar.Margin = new System.Windows.Forms.Padding(0);
      this.horizontalBar.Name = "horizontalBar";
      this.horizontalBar.Size = new System.Drawing.Size(370, 2);
      this.horizontalBar.TabIndex = 9;
      // 
      // imageList
      // 
      this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
      this.imageList.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList.Images.SetKeyName(0, "info.png");
      this.imageList.Images.SetKeyName(1, "warning.png");
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.ImageList imageList;
    private System.Windows.Forms.Label horizontalBar;
    private System.Windows.Forms.CheckBox checkDispAllShifts;
    private Lemoine.BaseControls.ScrollTable scrollTable;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
