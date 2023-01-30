// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorJobComponentName
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonRename = new System.Windows.Forms.Button();
      this.listElements = new Lemoine.BaseControls.List.ListTextValue();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.listIsofiles = new Lemoine.BaseControls.List.ListTextValue();
      this.textBoxJob = new System.Windows.Forms.TextBox();
      this.textBoxComponent = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 61F));
      this.baseLayout.Controls.Add(this.buttonRename, 3, 4);
      this.baseLayout.Controls.Add(this.listElements, 0, 1);
      this.baseLayout.Controls.Add(this.label3, 0, 0);
      this.baseLayout.Controls.Add(this.label4, 2, 0);
      this.baseLayout.Controls.Add(this.listIsofiles, 2, 1);
      this.baseLayout.Controls.Add(this.textBoxComponent, 1, 3);
      this.baseLayout.Controls.Add(this.label2, 0, 3);
      this.baseLayout.Controls.Add(this.label1, 0, 2);
      this.baseLayout.Controls.Add(this.textBoxJob, 1, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 5;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // buttonRename
      // 
      this.buttonRename.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRename.Location = new System.Drawing.Point(308, 265);
      this.buttonRename.Margin = new System.Windows.Forms.Padding(0);
      this.buttonRename.Name = "buttonRename";
      this.buttonRename.Size = new System.Drawing.Size(62, 25);
      this.buttonRename.TabIndex = 2;
      this.buttonRename.Text = "Rename";
      this.buttonRename.UseVisualStyleBackColor = true;
      this.buttonRename.Click += new System.EventHandler(this.ButtonRenameClick);
      // 
      // listElements
      // 
      this.listElements.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listElements, 2);
      this.listElements.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listElements.Location = new System.Drawing.Point(0, 19);
      this.listElements.Margin = new System.Windows.Forms.Padding(0);
      this.listElements.Name = "listElements";
      this.listElements.ReverseOrder = true;
      this.listElements.Size = new System.Drawing.Size(201, 196);
      this.listElements.Sorted = true;
      this.listElements.TabIndex = 5;
      this.listElements.ItemChanged += new System.Action<string, object>(this.ListElementsItemChanged);
      // 
      // label3
      // 
      this.baseLayout.SetColumnSpan(this.label3, 2);
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(0, 0);
      this.label3.Margin = new System.Windows.Forms.Padding(0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(201, 19);
      this.label3.TabIndex = 6;
      this.label3.Text = "Components to rename";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.baseLayout.SetColumnSpan(this.label4, 2);
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(204, 0);
      this.label4.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(166, 19);
      this.label4.TabIndex = 7;
      this.label4.Text = "Corresponding isofile(s)";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // listIsofiles
      // 
      this.listIsofiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listIsofiles, 2);
      this.listIsofiles.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listIsofiles.Location = new System.Drawing.Point(204, 19);
      this.listIsofiles.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.listIsofiles.Name = "listIsofiles";
      this.listIsofiles.ReverseOrder = true;
      this.listIsofiles.Size = new System.Drawing.Size(166, 196);
      this.listIsofiles.Sorted = true;
      this.listIsofiles.TabIndex = 8;
      // 
      // textBoxJob
      // 
      this.baseLayout.SetColumnSpan(this.textBoxJob, 3);
      this.textBoxJob.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBoxJob.Location = new System.Drawing.Point(94, 218);
      this.textBoxJob.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.textBoxJob.Name = "textBoxJob";
      this.textBoxJob.Size = new System.Drawing.Size(276, 20);
      this.textBoxJob.TabIndex = 3;
      // 
      // textBoxComponent
      // 
      this.baseLayout.SetColumnSpan(this.textBoxComponent, 3);
      this.textBoxComponent.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBoxComponent.Location = new System.Drawing.Point(94, 243);
      this.textBoxComponent.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.textBoxComponent.Name = "textBoxComponent";
      this.textBoxComponent.Size = new System.Drawing.Size(276, 20);
      this.textBoxComponent.TabIndex = 4;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 215);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(91, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "Job";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 240);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(91, 25);
      this.label2.TabIndex = 1;
      this.label2.Text = "Component";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button buttonRename;
    private System.Windows.Forms.TextBox textBoxJob;
    private System.Windows.Forms.TextBox textBoxComponent;
    private Lemoine.BaseControls.List.ListTextValue listElements;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private Lemoine.BaseControls.List.ListTextValue listIsofiles;
  }
}
