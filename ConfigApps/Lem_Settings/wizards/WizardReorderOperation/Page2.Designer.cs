// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardReorderOperation
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page2));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonDown = new System.Windows.Forms.Button();
      this.buttonUp = new System.Windows.Forms.Button();
      this.listBox = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.Controls.Add(this.buttonDown, 1, 2);
      this.baseLayout.Controls.Add(this.buttonUp, 1, 1);
      this.baseLayout.Controls.Add(this.listBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 1;
      // 
      // buttonDown
      // 
      this.buttonDown.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDown.Image = ((System.Drawing.Image)(resources.GetObject("buttonDown.Image")));
      this.buttonDown.Location = new System.Drawing.Point(343, 148);
      this.buttonDown.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(27, 27);
      this.buttonDown.TabIndex = 4;
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.ButtonDownClick);
      // 
      // buttonUp
      // 
      this.buttonUp.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonUp.Image = ((System.Drawing.Image)(resources.GetObject("buttonUp.Image")));
      this.buttonUp.Location = new System.Drawing.Point(343, 118);
      this.buttonUp.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(27, 27);
      this.buttonUp.TabIndex = 3;
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.ButtonUpClick);
      // 
      // listBox
      // 
      this.listBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBox.Location = new System.Drawing.Point(0, 0);
      this.listBox.Margin = new System.Windows.Forms.Padding(0);
      this.listBox.Name = "listBox";
      this.baseLayout.SetRowSpan(this.listBox, 4);
      this.listBox.SelectedIndex = -1;
      this.listBox.SelectedIndexes = null;
      this.listBox.SelectedText = "";
      this.listBox.SelectedTexts = null;
      this.listBox.SelectedValue = null;
      this.listBox.SelectedValues = null;
      this.listBox.Size = new System.Drawing.Size(340, 290);
      this.listBox.TabIndex = 14;
      this.listBox.ItemChanged += new System.Action<string, object>(this.ListBoxItemChanged);
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.Button buttonUp;
    private Lemoine.BaseControls.List.ListTextValue listBox;
  }
}
