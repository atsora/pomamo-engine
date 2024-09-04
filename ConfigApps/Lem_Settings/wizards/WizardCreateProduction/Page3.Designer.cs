// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateProduction
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
      this.scrollTable = new Lemoine.BaseControls.ScrollTable();
      this.SuspendLayout();
      // 
      // scrollTable
      // 
      this.scrollTable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.scrollTable.HorizontalMenu = null;
      this.scrollTable.Location = new System.Drawing.Point(0, 0);
      this.scrollTable.Margin = new System.Windows.Forms.Padding(0);
      this.scrollTable.Name = "scrollTable";
      this.scrollTable.Size = new System.Drawing.Size(350, 250);
      this.scrollTable.TabIndex = 0;
      this.scrollTable.VerticalMenu = null;
      this.scrollTable.CellChanged += new System.Action<int, int>(this.ScrollTableCellChanged);
      this.scrollTable.VerticalMenuClicked += new System.Action<int, int>(this.OnMenuClicked);
      this.scrollTable.HorizontalMenuClicked += new System.Action<int, int>(this.OnHMenuClicked);
      this.scrollTable.VerticalMenuOpen += new System.Action<int>(this.ScrollTableVerticalMenuOpen);
      this.scrollTable.VerticalMenuClosed += new System.Action<int>(this.ScrollTableVerticalMenuClosed);
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.scrollTable);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(350, 250);
      this.ResumeLayout(false);
    }
    private Lemoine.BaseControls.ScrollTable scrollTable;
  }
}
