// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateMachine
{
  partial class PageOrder
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PageOrder));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.listMachines = new Lemoine.BaseControls.List.ListTextValue();
      this.buttonBottom = new System.Windows.Forms.Button();
      this.buttonTop = new System.Windows.Forms.Button();
      this.buttonUp = new System.Windows.Forms.Button();
      this.buttonDown = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.Controls.Add(this.listMachines, 0, 0);
      this.baseLayout.Controls.Add(this.buttonBottom, 1, 5);
      this.baseLayout.Controls.Add(this.buttonTop, 1, 0);
      this.baseLayout.Controls.Add(this.buttonUp, 1, 2);
      this.baseLayout.Controls.Add(this.buttonDown, 1, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 0;
      // 
      // listMachines
      // 
      this.listMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listMachines.Location = new System.Drawing.Point(0, 0);
      this.listMachines.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.listMachines.Name = "listMachines";
      this.baseLayout.SetRowSpan(this.listMachines, 6);
      this.listMachines.SelectedIndex = -1;
      this.listMachines.SelectedText = "";
      this.listMachines.SelectedValue = null;
      this.listMachines.Size = new System.Drawing.Size(321, 250);
      this.listMachines.Sorted = false;
      this.listMachines.TabIndex = 0;
      // 
      // buttonBottom
      // 
      this.buttonBottom.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonBottom.Image = ((System.Drawing.Image)(resources.GetObject("buttonBottom.Image")));
      this.buttonBottom.Location = new System.Drawing.Point(324, 224);
      this.buttonBottom.Margin = new System.Windows.Forms.Padding(0);
      this.buttonBottom.Name = "buttonBottom";
      this.buttonBottom.Size = new System.Drawing.Size(26, 26);
      this.buttonBottom.TabIndex = 4;
      this.buttonBottom.UseVisualStyleBackColor = true;
      this.buttonBottom.Click += new System.EventHandler(this.ButtonBottomClick);
      // 
      // buttonTop
      // 
      this.buttonTop.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonTop.Image = ((System.Drawing.Image)(resources.GetObject("buttonTop.Image")));
      this.buttonTop.Location = new System.Drawing.Point(324, 0);
      this.buttonTop.Margin = new System.Windows.Forms.Padding(0);
      this.buttonTop.Name = "buttonTop";
      this.buttonTop.Size = new System.Drawing.Size(26, 26);
      this.buttonTop.TabIndex = 3;
      this.buttonTop.UseVisualStyleBackColor = true;
      this.buttonTop.Click += new System.EventHandler(this.ButtonTopClick);
      // 
      // buttonUp
      // 
      this.buttonUp.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonUp.Image = ((System.Drawing.Image)(resources.GetObject("buttonUp.Image")));
      this.buttonUp.Location = new System.Drawing.Point(324, 99);
      this.buttonUp.Margin = new System.Windows.Forms.Padding(0);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(26, 26);
      this.buttonUp.TabIndex = 1;
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.ButtonUpClick);
      // 
      // buttonDown
      // 
      this.buttonDown.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDown.Image = ((System.Drawing.Image)(resources.GetObject("buttonDown.Image")));
      this.buttonDown.Location = new System.Drawing.Point(324, 125);
      this.buttonDown.Margin = new System.Windows.Forms.Padding(0);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(26, 26);
      this.buttonDown.TabIndex = 2;
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.ButtonDownClick);
      // 
      // PageOrder
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PageOrder";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Button buttonTop;
    private System.Windows.Forms.Button buttonBottom;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.Button buttonUp;
    private Lemoine.BaseControls.List.ListTextValue listMachines;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
