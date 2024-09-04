// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class GuiLeft1
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GuiLeft1));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.panel2 = new System.Windows.Forms.Panel();
      this.listBoxCategories = new System.Windows.Forms.ListBox();
      this.buttonClearSearch = new System.Windows.Forms.Button();
      this.textSearch = new Lemoine.BaseControls.TextBoxWatermarked();
      this.panel1 = new System.Windows.Forms.Panel();
      this.listBoxDisplay = new System.Windows.Forms.ListBox();
      this.baseLayout.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.panel2, 0, 2);
      this.baseLayout.Controls.Add(this.buttonClearSearch, 1, 3);
      this.baseLayout.Controls.Add(this.textSearch, 0, 3);
      this.baseLayout.Controls.Add(this.panel1, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.Size = new System.Drawing.Size(140, 200);
      this.baseLayout.TabIndex = 0;
      // 
      // label1
      // 
      this.baseLayout.SetColumnSpan(this.label1, 2);
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(134, 30);
      this.label1.TabIndex = 0;
      this.label1.Text = "Display";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // panel2
      // 
      this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.baseLayout.SetColumnSpan(this.panel2, 2);
      this.panel2.Controls.Add(this.listBoxCategories);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(3, 75);
      this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 0, 1);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(137, 97);
      this.panel2.TabIndex = 2;
      // 
      // listBoxCategories
      // 
      this.listBoxCategories.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.listBoxCategories.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBoxCategories.FormattingEnabled = true;
      this.listBoxCategories.IntegralHeight = false;
      this.listBoxCategories.Location = new System.Drawing.Point(0, 0);
      this.listBoxCategories.Margin = new System.Windows.Forms.Padding(0);
      this.listBoxCategories.Name = "listBoxCategories";
      this.listBoxCategories.Size = new System.Drawing.Size(133, 93);
      this.listBoxCategories.TabIndex = 1;
      this.listBoxCategories.SelectedIndexChanged += new System.EventHandler(this.ListBoxCategoriesSelectedIndexChanged);
      // 
      // buttonClearSearch
      // 
      this.buttonClearSearch.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonClearSearch.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearSearch.Image")));
      this.buttonClearSearch.Location = new System.Drawing.Point(113, 173);
      this.buttonClearSearch.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.buttonClearSearch.Name = "buttonClearSearch";
      this.buttonClearSearch.Size = new System.Drawing.Size(27, 24);
      this.buttonClearSearch.TabIndex = 3;
      this.buttonClearSearch.UseVisualStyleBackColor = true;
      this.buttonClearSearch.Click += new System.EventHandler(this.ButtonClearSearchClick);
      // 
      // textSearch
      // 
      this.textSearch.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textSearch.Location = new System.Drawing.Point(3, 175);
      this.textSearch.Margin = new System.Windows.Forms.Padding(3, 2, 0, 3);
      this.textSearch.Name = "textSearch";
      this.textSearch.Size = new System.Drawing.Size(107, 20);
      this.textSearch.TabIndex = 4;
      this.textSearch.WaterMark = "Search...";
      this.textSearch.WaterMarkActiveForeColor = System.Drawing.Color.Gray;
      this.textSearch.WaterMarkFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textSearch.WaterMarkForeColor = System.Drawing.Color.LightGray;
      this.textSearch.TextChanged += new System.EventHandler(this.TextSearchTextChanged);
      // 
      // panel1
      // 
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.baseLayout.SetColumnSpan(this.panel1, 2);
      this.panel1.Controls.Add(this.listBoxDisplay);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(3, 30);
      this.panel1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(137, 43);
      this.panel1.TabIndex = 5;
      // 
      // listBoxDisplay
      // 
      this.listBoxDisplay.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.listBoxDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBoxDisplay.FormattingEnabled = true;
      this.listBoxDisplay.Items.AddRange(new object[] {
      "All",
      "Recent",
      "Favorites"});
      this.listBoxDisplay.Location = new System.Drawing.Point(0, 0);
      this.listBoxDisplay.Margin = new System.Windows.Forms.Padding(0);
      this.listBoxDisplay.Name = "listBoxDisplay";
      this.listBoxDisplay.Size = new System.Drawing.Size(133, 39);
      this.listBoxDisplay.TabIndex = 0;
      this.listBoxDisplay.SelectedIndexChanged += new System.EventHandler(this.ListBoxDisplaySelectedIndexChanged);
      // 
      // GuiLeft1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "GuiLeft1";
      this.Size = new System.Drawing.Size(140, 200);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.ListBox listBoxCategories;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Button buttonClearSearch;
    private Lemoine.BaseControls.TextBoxWatermarked textSearch;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.ListBox listBoxDisplay;
  }
}
