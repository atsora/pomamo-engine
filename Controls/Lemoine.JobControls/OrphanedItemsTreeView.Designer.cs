// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class OrphanedItemsTreeView
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
    	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrphanedItemsTreeView));
    	this.m_treeView = new System.Windows.Forms.TreeView();
    	this.imageList = new System.Windows.Forms.ImageList(this.components);
    	this.imageListDrag = new System.Windows.Forms.ImageList(this.components);
    	this.SuspendLayout();
    	// 
    	// treeView
    	// 
    	this.m_treeView.AllowDrop = true;
    	this.m_treeView.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.m_treeView.HideSelection = false;
    	this.m_treeView.ImageIndex = 0;
    	this.m_treeView.ImageList = this.imageList;
    	this.m_treeView.ItemHeight = 25;
    	this.m_treeView.Location = new System.Drawing.Point(0, 0);
    	this.m_treeView.Name = "treeView";
    	this.m_treeView.SelectedImageIndex = 0;
    	this.m_treeView.Size = new System.Drawing.Size(353, 271);
    	this.m_treeView.TabIndex = 0;
    	this.m_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterSelect);
    	this.m_treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeViewMouseDown);
    	this.m_treeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterExpand);
    	// 
    	// imageList
    	// 
    	this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
    	this.imageList.TransparentColor = System.Drawing.Color.Transparent;
    	this.imageList.Images.SetKeyName(0, "workorder.png");
    	this.imageList.Images.SetKeyName(1, "project.png");
    	this.imageList.Images.SetKeyName(2, "component.png");
    	this.imageList.Images.SetKeyName(3, "intermediateworkpiece.gif");
    	this.imageList.Images.SetKeyName(4, "operation.png");
    	this.imageList.Images.SetKeyName(5, "job.png");
    	this.imageList.Images.SetKeyName(6, "part.png");
    	this.imageList.Images.SetKeyName(7, "simpleoperation.png");
    	this.imageList.Images.SetKeyName(8, "zoom.png");
    	// 
    	// imageListDrag
    	// 
    	this.imageListDrag.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
    	this.imageListDrag.ImageSize = new System.Drawing.Size(16, 16);
    	this.imageListDrag.TransparentColor = System.Drawing.Color.Transparent;
    	// 
    	// OrphanedItemsTreeView
    	// 
    	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.Controls.Add(this.m_treeView);
    	this.Name = "OrphanedItemsTreeView";
    	this.Size = new System.Drawing.Size(353, 271);
    	this.ResumeLayout(false);
    }
    private System.Windows.Forms.ImageList imageListDrag;
    private System.Windows.Forms.ImageList imageList;
    private System.Windows.Forms.TreeView m_treeView;
  }
}
