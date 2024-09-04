// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorEventLevel
{
  partial class Page1
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page1));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.listEventLevels = new Lemoine.BaseControls.List.ListTextValue();
      this.buttonEdit = new System.Windows.Forms.Button();
      this.buttonDelete = new System.Windows.Forms.Button();
      this.buttonAdd = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.Controls.Add(this.listEventLevels, 0, 0);
      this.baseLayout.Controls.Add(this.buttonEdit, 1, 1);
      this.baseLayout.Controls.Add(this.buttonDelete, 1, 2);
      this.baseLayout.Controls.Add(this.buttonAdd, 1, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // listEventLevels
      // 
      this.listEventLevels.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listEventLevels.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listEventLevels.Location = new System.Drawing.Point(0, 0);
      this.listEventLevels.Margin = new System.Windows.Forms.Padding(0);
      this.listEventLevels.Name = "listEventLevels";
      this.baseLayout.SetRowSpan(this.listEventLevels, 4);
      this.listEventLevels.Size = new System.Drawing.Size(340, 290);
      this.listEventLevels.Sorted = true;
      this.listEventLevels.TabIndex = 0;
      this.listEventLevels.ItemChanged += new System.Action<string, object>(this.ListEventLevelsItemChanged);
      this.listEventLevels.ItemDoubleClicked += new System.Action<string, object>(this.ListEventLevelsItemDoubleClicked);
      // 
      // buttonEdit
      // 
      this.buttonEdit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonEdit.Enabled = false;
      this.buttonEdit.Image = ((System.Drawing.Image)(resources.GetObject("buttonEdit.Image")));
      this.buttonEdit.Location = new System.Drawing.Point(343, 203);
      this.buttonEdit.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonEdit.Name = "buttonEdit";
      this.buttonEdit.Size = new System.Drawing.Size(27, 27);
      this.buttonEdit.TabIndex = 1;
      this.buttonEdit.UseVisualStyleBackColor = true;
      this.buttonEdit.Click += new System.EventHandler(this.ButtonEditClick);
      // 
      // buttonDelete
      // 
      this.buttonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDelete.Enabled = false;
      this.buttonDelete.Image = ((System.Drawing.Image)(resources.GetObject("buttonDelete.Image")));
      this.buttonDelete.Location = new System.Drawing.Point(343, 233);
      this.buttonDelete.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new System.Drawing.Size(27, 27);
      this.buttonDelete.TabIndex = 2;
      this.buttonDelete.UseVisualStyleBackColor = true;
      this.buttonDelete.Click += new System.EventHandler(this.ButtonDeleteClick);
      // 
      // buttonAdd
      // 
      this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonAdd.Image")));
      this.buttonAdd.Location = new System.Drawing.Point(343, 263);
      this.buttonAdd.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new System.Drawing.Size(27, 27);
      this.buttonAdd.TabIndex = 3;
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.List.ListTextValue listEventLevels;
    private System.Windows.Forms.Button buttonEdit;
    private System.Windows.Forms.Button buttonDelete;
    private System.Windows.Forms.Button buttonAdd;
  }
}
