// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorMachineCategories
{
  partial class Page
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.listElements = new Lemoine.BaseControls.List.ListTextValue();
      this.listMachines = new Lemoine.BaseControls.List.ListTextValue();
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.buttonTop = new System.Windows.Forms.Button();
      this.buttonUp = new System.Windows.Forms.Button();
      this.buttonBottom = new System.Windows.Forms.Button();
      this.buttonRemove = new System.Windows.Forms.Button();
      this.buttonDown = new System.Windows.Forms.Button();
      this.buttonAdd = new System.Windows.Forms.Button();
      this.buttonEdit = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.tableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.label, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 2, 0);
      this.baseLayout.Controls.Add(this.listElements, 0, 1);
      this.baseLayout.Controls.Add(this.listMachines, 2, 1);
      this.baseLayout.Controls.Add(this.tableLayoutPanel, 1, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // label
      // 
      this.label.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label.Location = new System.Drawing.Point(3, 0);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(162, 18);
      this.label.TabIndex = 7;
      this.label.Text = "Companies";
      this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(205, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(162, 18);
      this.label2.TabIndex = 8;
      this.label2.Text = "Machines";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // listElements
      // 
      this.listElements.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listElements.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listElements.Location = new System.Drawing.Point(0, 18);
      this.listElements.Margin = new System.Windows.Forms.Padding(0);
      this.listElements.Name = "listElements";
      this.listElements.SelectedIndex = -1;
      this.listElements.SelectedText = "";
      this.listElements.SelectedValue = null;
      this.listElements.Size = new System.Drawing.Size(168, 272);
      this.listElements.Sorted = false;
      this.listElements.TabIndex = 9;
      this.listElements.ItemChanged += new System.Action<string, object>(this.ListElementsItemChanged);
      // 
      // listMachines
      // 
      this.listMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listMachines.Location = new System.Drawing.Point(202, 18);
      this.listMachines.Margin = new System.Windows.Forms.Padding(0);
      this.listMachines.Name = "listMachines";
      this.listMachines.SelectedIndex = -1;
      this.listMachines.SelectedText = "";
      this.listMachines.SelectedValue = null;
      this.listMachines.Size = new System.Drawing.Size(168, 272);
      this.listMachines.Sorted = true;
      this.listMachines.TabIndex = 10;
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.ColumnCount = 1;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel.Controls.Add(this.buttonTop, 0, 0);
      this.tableLayoutPanel.Controls.Add(this.buttonUp, 0, 1);
      this.tableLayoutPanel.Controls.Add(this.buttonBottom, 0, 8);
      this.tableLayoutPanel.Controls.Add(this.buttonRemove, 0, 5);
      this.tableLayoutPanel.Controls.Add(this.buttonDown, 0, 7);
      this.tableLayoutPanel.Controls.Add(this.buttonAdd, 0, 3);
      this.tableLayoutPanel.Controls.Add(this.buttonEdit, 0, 4);
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(168, 18);
      this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.RowCount = 9;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel.Size = new System.Drawing.Size(34, 272);
      this.tableLayoutPanel.TabIndex = 11;
      // 
      // buttonTop
      // 
      this.buttonTop.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonTop.Enabled = false;
      this.buttonTop.Image = ((System.Drawing.Image)(resources.GetObject("buttonTop.Image")));
      this.buttonTop.Location = new System.Drawing.Point(0, 0);
      this.buttonTop.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.buttonTop.Name = "buttonTop";
      this.buttonTop.Size = new System.Drawing.Size(30, 30);
      this.buttonTop.TabIndex = 0;
      this.buttonTop.UseVisualStyleBackColor = true;
      this.buttonTop.Click += new System.EventHandler(this.ButtonTopClick);
      // 
      // buttonUp
      // 
      this.buttonUp.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonUp.Enabled = false;
      this.buttonUp.Image = ((System.Drawing.Image)(resources.GetObject("buttonUp.Image")));
      this.buttonUp.Location = new System.Drawing.Point(0, 30);
      this.buttonUp.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(30, 30);
      this.buttonUp.TabIndex = 1;
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.ButtonUpClick);
      // 
      // buttonBottom
      // 
      this.buttonBottom.Enabled = false;
      this.buttonBottom.Image = ((System.Drawing.Image)(resources.GetObject("buttonBottom.Image")));
      this.buttonBottom.Location = new System.Drawing.Point(0, 242);
      this.buttonBottom.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.buttonBottom.Name = "buttonBottom";
      this.buttonBottom.Size = new System.Drawing.Size(30, 30);
      this.buttonBottom.TabIndex = 6;
      this.buttonBottom.UseVisualStyleBackColor = true;
      this.buttonBottom.Click += new System.EventHandler(this.ButtonBottomClick);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Enabled = false;
      this.buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemove.Image")));
      this.buttonRemove.Location = new System.Drawing.Point(0, 151);
      this.buttonRemove.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(30, 30);
      this.buttonRemove.TabIndex = 4;
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
      // 
      // buttonDown
      // 
      this.buttonDown.Enabled = false;
      this.buttonDown.Image = ((System.Drawing.Image)(resources.GetObject("buttonDown.Image")));
      this.buttonDown.Location = new System.Drawing.Point(0, 212);
      this.buttonDown.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(30, 30);
      this.buttonDown.TabIndex = 5;
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.ButtonDownClick);
      // 
      // buttonAdd
      // 
      this.buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonAdd.Image")));
      this.buttonAdd.Location = new System.Drawing.Point(0, 91);
      this.buttonAdd.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new System.Drawing.Size(30, 30);
      this.buttonAdd.TabIndex = 2;
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
      // 
      // buttonEdit
      // 
      this.buttonEdit.Enabled = false;
      this.buttonEdit.Image = ((System.Drawing.Image)(resources.GetObject("buttonEdit.Image")));
      this.buttonEdit.Location = new System.Drawing.Point(0, 121);
      this.buttonEdit.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
      this.buttonEdit.Name = "buttonEdit";
      this.buttonEdit.Size = new System.Drawing.Size(30, 30);
      this.buttonEdit.TabIndex = 3;
      this.buttonEdit.UseVisualStyleBackColor = true;
      this.buttonEdit.Click += new System.EventHandler(this.ButtonEditClick);
      // 
      // Page
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.tableLayoutPanel.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private Lemoine.BaseControls.List.ListTextValue listMachines;
    private Lemoine.BaseControls.List.ListTextValue listElements;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label;
    private System.Windows.Forms.Button buttonTop;
    private System.Windows.Forms.Button buttonUp;
    private System.Windows.Forms.Button buttonBottom;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.Button buttonRemove;
    private System.Windows.Forms.Button buttonEdit;
    private System.Windows.Forms.Button buttonAdd;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
