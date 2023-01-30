// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateProduction
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
      this.scrollTable = new Lemoine.BaseControls.ScrollTable();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.SuspendLayout();
      // 
      // scrollTable
      // 
      this.scrollTable.CheckBoxMode = true;
      this.scrollTable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.scrollTable.FirstVerticalHeaderWidth = 100;
      this.scrollTable.HorizontalFooterVisible = false;
      this.scrollTable.HorizontalMenu = null;
      this.scrollTable.Location = new System.Drawing.Point(0, 0);
      this.scrollTable.Margin = new System.Windows.Forms.Padding(0);
      this.scrollTable.Name = "scrollTable";
      this.scrollTable.SecondVerticalHeaderWidth = 0;
      this.scrollTable.Size = new System.Drawing.Size(350, 250);
      this.scrollTable.TabIndex = 0;
      this.scrollTable.VerticalMenu = null;
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
      this.Controls.Add(this.scrollTable);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(350, 250);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.ImageList imageList;
    private Lemoine.BaseControls.ScrollTable scrollTable;
  }
}
