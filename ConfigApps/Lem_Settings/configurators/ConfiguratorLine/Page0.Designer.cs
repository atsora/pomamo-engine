// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorLine
{
  partial class Page0
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page0));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.listLines = new Lemoine.BaseControls.List.ListTextValue();
      this.buttonDelete = new System.Windows.Forms.Button();
      this.buttonChangeProperties = new System.Windows.Forms.Button();
      this.buttonChangeMachines = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
      this.baseLayout.Controls.Add(this.listLines, 0, 0);
      this.baseLayout.Controls.Add(this.buttonDelete, 1, 1);
      this.baseLayout.Controls.Add(this.buttonChangeProperties, 1, 2);
      this.baseLayout.Controls.Add(this.buttonChangeMachines, 1, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 0;
      // 
      // listLines
      // 
      this.listLines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listLines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listLines.Location = new System.Drawing.Point(0, 0);
      this.listLines.Margin = new System.Windows.Forms.Padding(0);
      this.listLines.MultipleSelection = false;
      this.listLines.Name = "listLines";
      this.baseLayout.SetRowSpan(this.listLines, 4);
      this.listLines.SelectedIndex = -1;
      this.listLines.SelectedIndexes = ((System.Collections.Generic.IList<int>)(resources.GetObject("listLines.SelectedIndexes")));
      this.listLines.SelectedText = "";
      this.listLines.SelectedTexts = ((System.Collections.Generic.IList<string>)(resources.GetObject("listLines.SelectedTexts")));
      this.listLines.SelectedValue = null;
      this.listLines.SelectedValues = ((System.Collections.Generic.IList<object>)(resources.GetObject("listLines.SelectedValues")));
      this.listLines.Size = new System.Drawing.Size(240, 250);
      this.listLines.Sorted = true;
      this.listLines.TabIndex = 0;
      // 
      // buttonDelete
      // 
      this.buttonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDelete.Location = new System.Drawing.Point(243, 178);
      this.buttonDelete.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new System.Drawing.Size(107, 22);
      this.buttonDelete.TabIndex = 1;
      this.buttonDelete.Text = "Delete";
      this.buttonDelete.UseVisualStyleBackColor = true;
      this.buttonDelete.Click += new System.EventHandler(this.ButtonDeleteClick);
      // 
      // buttonChangeProperties
      // 
      this.buttonChangeProperties.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonChangeProperties.Location = new System.Drawing.Point(243, 203);
      this.buttonChangeProperties.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonChangeProperties.Name = "buttonChangeProperties";
      this.buttonChangeProperties.Size = new System.Drawing.Size(107, 22);
      this.buttonChangeProperties.TabIndex = 2;
      this.buttonChangeProperties.Text = "Change properties";
      this.buttonChangeProperties.UseVisualStyleBackColor = true;
      this.buttonChangeProperties.Click += new System.EventHandler(this.ButtonChangePropertiesClick);
      // 
      // buttonChangeMachines
      // 
      this.buttonChangeMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonChangeMachines.Location = new System.Drawing.Point(243, 228);
      this.buttonChangeMachines.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.buttonChangeMachines.Name = "buttonChangeMachines";
      this.buttonChangeMachines.Size = new System.Drawing.Size(107, 22);
      this.buttonChangeMachines.TabIndex = 3;
      this.buttonChangeMachines.Text = "Change machines";
      this.buttonChangeMachines.UseVisualStyleBackColor = true;
      this.buttonChangeMachines.Click += new System.EventHandler(this.ButtonChangeMachinesClick);
      // 
      // Page0
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page0";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Button buttonChangeMachines;
    private System.Windows.Forms.Button buttonChangeProperties;
    private System.Windows.Forms.Button buttonDelete;
    private Lemoine.BaseControls.List.ListTextValue listLines;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
