using System.Windows.Forms;

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class OperationTreeView
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OperationTreeView));
      this.contextMenuStripTreeView = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripWorkOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripProject = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripJob = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripComponent = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripPart = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripIntermediateWorkPiece = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripOperation = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripSimpleOperation = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.disclosurePanel = new Lemoine.BaseControls.DisclosurePanel();
      this.searchPnl = new System.Windows.Forms.Panel();
      this.statusGroupBox = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.statusComboBox = new System.Windows.Forms.ComboBox();
      this.labelWorkOrderStatus = new System.Windows.Forms.Label();
      this.labelProjectStatus = new System.Windows.Forms.Label();
      this.projectCombobox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.operationCombobox = new System.Windows.Forms.ComboBox();
      this.searchGroupBox = new System.Windows.Forms.GroupBox();
      this.searchBtn = new System.Windows.Forms.Button();
      this.searchComboBox = new System.Windows.Forms.ComboBox();
      this.searchTextBox = new System.Windows.Forms.TextBox();
      this.m_treeViewMS = new Lemoine.JobControls.TreeViewMS();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.imageListDrag = new System.Windows.Forms.ImageList(this.components);
      this.contextMenuStripSequence = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStripPath = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.searchPnl.SuspendLayout();
      this.statusGroupBox.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.searchGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // contextMenuStripTreeView
      // 
      this.contextMenuStripTreeView.Name = "contextMenuStrip";
      this.contextMenuStripTreeView.Size = new System.Drawing.Size(61, 4);
      // 
      // contextMenuStripWorkOrder
      // 
      this.contextMenuStripWorkOrder.Name = "contextMenuStripWorkOrder";
      this.contextMenuStripWorkOrder.Size = new System.Drawing.Size(61, 4);
      // 
      // contextMenuStripProject
      // 
      this.contextMenuStripProject.Name = "contextMenuStripProject";
      this.contextMenuStripProject.Size = new System.Drawing.Size(61, 4);
      // 
      // contextMenuStripJob
      // 
      this.contextMenuStripJob.Name = "contextMenuStripJob";
      this.contextMenuStripJob.Size = new System.Drawing.Size(61, 4);
      // 
      // contextMenuStripComponent
      // 
      this.contextMenuStripComponent.Name = "contextMenuStripComponent";
      this.contextMenuStripComponent.Size = new System.Drawing.Size(61, 4);
      // 
      // contextMenuStripPart
      // 
      this.contextMenuStripPart.Name = "contextMenuStripPart";
      this.contextMenuStripPart.Size = new System.Drawing.Size(61, 4);
      // 
      // contextMenuStripIntermediateWorkPiece
      // 
      this.contextMenuStripIntermediateWorkPiece.Name = "contextMenuStripIntermediateWorkPiece";
      this.contextMenuStripIntermediateWorkPiece.Size = new System.Drawing.Size(61, 4);
      // 
      // contextMenuStripOperation
      // 
      this.contextMenuStripOperation.Name = "contextMenuStripOperation";
      this.contextMenuStripOperation.Size = new System.Drawing.Size(61, 4);
      this.contextMenuStripOperation.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripOperationOpening);
      // 
      // contextMenuStripSimpleOperation
      // 
      this.contextMenuStripSimpleOperation.Name = "contextMenuStripSimpleOperation";
      this.contextMenuStripSimpleOperation.Size = new System.Drawing.Size(61, 4);
      this.contextMenuStripSimpleOperation.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripSimpleOperationOpening);
      // 
      // disclosurePanel
      // 
      this.disclosurePanel.Content = null;
      this.disclosurePanel.ContentHeight = 150;
      this.disclosurePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.disclosurePanel.Location = new System.Drawing.Point(0, 609);
      this.disclosurePanel.Name = "disclosurePanel";
      this.disclosurePanel.Size = new System.Drawing.Size(356, 25);
      this.disclosurePanel.State = false;
      this.disclosurePanel.TabIndex = 10;
      this.disclosurePanel.Title = null;
      // 
      // searchPnl
      // 
      this.searchPnl.AutoSize = true;
      this.searchPnl.Controls.Add(this.statusGroupBox);
      this.searchPnl.Controls.Add(this.searchGroupBox);
      this.searchPnl.Dock = System.Windows.Forms.DockStyle.Top;
      this.searchPnl.Location = new System.Drawing.Point(0, 0);
      this.searchPnl.Name = "searchPnl";
      this.searchPnl.Size = new System.Drawing.Size(356, 131);
      this.searchPnl.TabIndex = 11;
      // 
      // statusGroupBox
      // 
      this.statusGroupBox.Controls.Add(this.tableLayoutPanel1);
      this.statusGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.statusGroupBox.Location = new System.Drawing.Point(0, 39);
      this.statusGroupBox.Name = "statusGroupBox";
      this.statusGroupBox.Size = new System.Drawing.Size(356, 92);
      this.statusGroupBox.TabIndex = 11;
      this.statusGroupBox.TabStop = false;
      this.statusGroupBox.Text = "Filters";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.statusComboBox, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.labelWorkOrderStatus, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.labelProjectStatus, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.projectCombobox, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.operationCombobox, 1, 2);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(350, 73);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // statusComboBox
      // 
      this.statusComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.statusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.statusComboBox.Location = new System.Drawing.Point(178, 3);
      this.statusComboBox.Name = "statusComboBox";
      this.statusComboBox.Size = new System.Drawing.Size(169, 21);
      this.statusComboBox.TabIndex = 0;
      this.statusComboBox.SelectionChangeCommitted += new System.EventHandler(this.StatusComboBoxSelectionChangeCommitted);
      // 
      // labelWorkOrderStatus
      // 
      this.labelWorkOrderStatus.AutoSize = true;
      this.labelWorkOrderStatus.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelWorkOrderStatus.Location = new System.Drawing.Point(3, 0);
      this.labelWorkOrderStatus.Name = "labelWorkOrderStatus";
      this.labelWorkOrderStatus.Size = new System.Drawing.Size(169, 24);
      this.labelWorkOrderStatus.TabIndex = 1;
      this.labelWorkOrderStatus.Text = "Workorder status";
      this.labelWorkOrderStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelProjectStatus
      // 
      this.labelProjectStatus.AutoSize = true;
      this.labelProjectStatus.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelProjectStatus.Location = new System.Drawing.Point(3, 24);
      this.labelProjectStatus.Name = "labelProjectStatus";
      this.labelProjectStatus.Size = new System.Drawing.Size(169, 24);
      this.labelProjectStatus.TabIndex = 2;
      this.labelProjectStatus.Text = "Project status";
      this.labelProjectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // projectCombobox
      // 
      this.projectCombobox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.projectCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.projectCombobox.Location = new System.Drawing.Point(178, 27);
      this.projectCombobox.Name = "projectCombobox";
      this.projectCombobox.Size = new System.Drawing.Size(169, 21);
      this.projectCombobox.TabIndex = 3;
      this.projectCombobox.SelectedIndexChanged += new System.EventHandler(this.projectCombobox_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(169, 25);
      this.label1.TabIndex = 4;
      this.label1.Text = "Operation status";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // operationCombobox
      // 
      this.operationCombobox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.operationCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.operationCombobox.Location = new System.Drawing.Point(178, 51);
      this.operationCombobox.Name = "operationCombobox";
      this.operationCombobox.Size = new System.Drawing.Size(169, 21);
      this.operationCombobox.TabIndex = 5;
      this.operationCombobox.SelectedIndexChanged += new System.EventHandler(this.operationCombobox_SelectedIndexChanged);
      // 
      // searchGroupBox
      // 
      this.searchGroupBox.Controls.Add(this.searchBtn);
      this.searchGroupBox.Controls.Add(this.searchComboBox);
      this.searchGroupBox.Controls.Add(this.searchTextBox);
      this.searchGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.searchGroupBox.Location = new System.Drawing.Point(0, 0);
      this.searchGroupBox.Name = "searchGroupBox";
      this.searchGroupBox.Size = new System.Drawing.Size(356, 39);
      this.searchGroupBox.TabIndex = 10;
      this.searchGroupBox.TabStop = false;
      this.searchGroupBox.Visible = false;
      // 
      // searchBtn
      // 
      this.searchBtn.ImageIndex = 8;
      this.searchBtn.Location = new System.Drawing.Point(318, 8);
      this.searchBtn.Name = "searchBtn";
      this.searchBtn.Size = new System.Drawing.Size(25, 25);
      this.searchBtn.TabIndex = 3;
      this.searchBtn.UseVisualStyleBackColor = true;
      // 
      // searchComboBox
      // 
      this.searchComboBox.FormattingEnabled = true;
      this.searchComboBox.Location = new System.Drawing.Point(174, 11);
      this.searchComboBox.Name = "searchComboBox";
      this.searchComboBox.Size = new System.Drawing.Size(128, 21);
      this.searchComboBox.TabIndex = 2;
      // 
      // searchTextBox
      // 
      this.searchTextBox.Location = new System.Drawing.Point(9, 12);
      this.searchTextBox.Name = "searchTextBox";
      this.searchTextBox.Size = new System.Drawing.Size(159, 20);
      this.searchTextBox.TabIndex = 0;
      // 
      // m_treeViewMS
      // 
      this.m_treeViewMS.AllowDrop = true;
      this.m_treeViewMS.BackColor = System.Drawing.SystemColors.Window;
      this.m_treeViewMS.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_treeViewMS.HideSelection = false;
      this.m_treeViewMS.ImageIndex = 0;
      this.m_treeViewMS.ImageList = this.imageList;
      this.m_treeViewMS.ItemHeight = 25;
      this.m_treeViewMS.Location = new System.Drawing.Point(0, 131);
      this.m_treeViewMS.MultiSelect = true;
      this.m_treeViewMS.Name = "m_treeViewMS";
      this.m_treeViewMS.SelectedImageIndex = 0;
      this.m_treeViewMS.SelectedNodes = new System.Windows.Forms.TreeNode[0];
      this.m_treeViewMS.Size = new System.Drawing.Size(356, 478);
      this.m_treeViewMS.TabIndex = 12;
      this.m_treeViewMS.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterExpand);
      this.m_treeViewMS.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.TreeViewItemDrag);
      this.m_treeViewMS.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewMSAfterSelect);
      this.m_treeViewMS.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewNodeMouseClick);
      this.m_treeViewMS.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeViewDragDrop);
      this.m_treeViewMS.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeViewDragEnter);
      this.m_treeViewMS.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeViewDragOver);
      this.m_treeViewMS.DragLeave += new System.EventHandler(this.TreeViewDragLeave);
      this.m_treeViewMS.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TreeViewMSMouseClick);
      this.m_treeViewMS.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeViewMSMouseDown);
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
      this.imageList.Images.SetKeyName(9, "sequence.png");
      // 
      // imageListDrag
      // 
      this.imageListDrag.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imageListDrag.ImageSize = new System.Drawing.Size(16, 16);
      this.imageListDrag.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // contextMenuStripSequence
      // 
      this.contextMenuStripSequence.Name = "contextMenuStripPart";
      this.contextMenuStripSequence.Size = new System.Drawing.Size(61, 4);
      // 
      // contextMenuStripPath
      // 
      this.contextMenuStripPath.Name = "contextMenuStripProject";
      this.contextMenuStripPath.Size = new System.Drawing.Size(61, 4);
      this.contextMenuStripPath.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripPathOpening);
      // 
      // OperationTreeView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.m_treeViewMS);
      this.Controls.Add(this.searchPnl);
      this.Controls.Add(this.disclosurePanel);
      this.Name = "OperationTreeView";
      this.Size = new System.Drawing.Size(356, 634);
      this.searchPnl.ResumeLayout(false);
      this.statusGroupBox.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.searchGroupBox.ResumeLayout(false);
      this.searchGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    private System.Windows.Forms.ContextMenuStrip contextMenuStripPath;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripSequence;
    private System.Windows.Forms.ImageList imageList;
    private System.Windows.Forms.ImageList imageListDrag;
    private System.Windows.Forms.ComboBox statusComboBox;
    private System.Windows.Forms.GroupBox statusGroupBox;
    private Lemoine.JobControls.TreeViewMS m_treeViewMS;
    private System.Windows.Forms.Panel searchPnl;
    private Lemoine.BaseControls.DisclosurePanel disclosurePanel;
    private System.Windows.Forms.GroupBox searchGroupBox;
    private System.Windows.Forms.ComboBox searchComboBox;
    private System.Windows.Forms.TextBox searchTextBox;
    private System.Windows.Forms.Button searchBtn;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripTreeView;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripSimpleOperation;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripOperation;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripIntermediateWorkPiece;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripPart;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripComponent;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripJob;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripProject;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripWorkOrder;
    private TableLayoutPanel tableLayoutPanel1;
    private Label labelWorkOrderStatus;
    private Label labelProjectStatus;
    private ComboBox projectCombobox;
    private Label label1;
    private ComboBox operationCombobox;
  }
}
