// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class FileSelector
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private Lemoine.BaseControls.ComboboxTextValue combobox;
    private System.Windows.Forms.TableLayoutPanel layout;
    private System.Windows.Forms.Button buttonUpload;
    private System.Windows.Forms.Button buttonDelete;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileSelector));
      this.combobox = new Lemoine.BaseControls.ComboboxTextValue();
      this.layout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonUpload = new System.Windows.Forms.Button();
      this.buttonDelete = new System.Windows.Forms.Button();
      this.layout.SuspendLayout();
      this.SuspendLayout();
      // 
      // combobox
      // 
      this.combobox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.combobox.Location = new System.Drawing.Point(0, 1);
      this.combobox.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
      this.combobox.Name = "combobox";
      this.combobox.Size = new System.Drawing.Size(95, 23);
      this.combobox.Sorted = true;
      this.combobox.TabIndex = 0;
      this.combobox.ItemChanged += new System.Action<string, object>(this.ComboboxItemChanged);
      // 
      // layout
      // 
      this.layout.ColumnCount = 3;
      this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.layout.Controls.Add(this.combobox, 0, 0);
      this.layout.Controls.Add(this.buttonUpload, 1, 0);
      this.layout.Controls.Add(this.buttonDelete, 2, 0);
      this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.layout.Location = new System.Drawing.Point(0, 0);
      this.layout.Margin = new System.Windows.Forms.Padding(0);
      this.layout.Name = "layout";
      this.layout.RowCount = 1;
      this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.layout.Size = new System.Drawing.Size(151, 24);
      this.layout.TabIndex = 1;
      // 
      // buttonUpload
      // 
      this.buttonUpload.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonUpload.Enabled = false;
      this.buttonUpload.Image = ((System.Drawing.Image)(resources.GetObject("buttonUpload.Image")));
      this.buttonUpload.Location = new System.Drawing.Point(98, 0);
      this.buttonUpload.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.buttonUpload.Name = "buttonUpload";
      this.buttonUpload.Size = new System.Drawing.Size(25, 24);
      this.buttonUpload.TabIndex = 1;
      this.buttonUpload.UseVisualStyleBackColor = true;
      this.buttonUpload.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonUploadMouseClick);
      // 
      // buttonDelete
      // 
      this.buttonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDelete.Enabled = false;
      this.buttonDelete.Image = ((System.Drawing.Image)(resources.GetObject("buttonDelete.Image")));
      this.buttonDelete.Location = new System.Drawing.Point(126, 0);
      this.buttonDelete.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new System.Drawing.Size(25, 24);
      this.buttonDelete.TabIndex = 2;
      this.buttonDelete.UseVisualStyleBackColor = true;
      this.buttonDelete.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonDeleteMouseClick);
      // 
      // FileSelector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.layout);
      this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.MinimumSize = new System.Drawing.Size(0, 24);
      this.Name = "FileSelector";
      this.Size = new System.Drawing.Size(151, 24);
      this.layout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
