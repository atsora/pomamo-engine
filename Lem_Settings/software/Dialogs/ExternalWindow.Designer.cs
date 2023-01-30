// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class ExternalWindow
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the form.
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExternalWindow));
      this.table = new System.Windows.Forms.TableLayoutPanel();
      this.guiLeft = new Lem_Settings.GuiLeft();
      this.guiRight = new Lem_Settings.GuiRight();
      this.table.SuspendLayout();
      this.SuspendLayout();
      // 
      // table
      // 
      this.table.ColumnCount = 3;
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
      this.table.Controls.Add(this.guiLeft, 0, 0);
      this.table.Controls.Add(this.guiRight, 2, 0);
      this.table.Dock = System.Windows.Forms.DockStyle.Fill;
      this.table.Location = new System.Drawing.Point(0, 0);
      this.table.Name = "table";
      this.table.RowCount = 1;
      this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.table.Size = new System.Drawing.Size(684, 362);
      this.table.TabIndex = 1;
      // 
      // guiLeft
      // 
      this.guiLeft.Dock = System.Windows.Forms.DockStyle.Fill;
      this.guiLeft.EditorsEnabled = false;
      this.guiLeft.Location = new System.Drawing.Point(0, 0);
      this.guiLeft.Margin = new System.Windows.Forms.Padding(0);
      this.guiLeft.Name = "guiLeft";
      this.guiLeft.Size = new System.Drawing.Size(150, 362);
      this.guiLeft.TabIndex = 0;
      // 
      // guiRight
      // 
      this.guiRight.Dock = System.Windows.Forms.DockStyle.Fill;
      this.guiRight.Location = new System.Drawing.Point(534, 0);
      this.guiRight.Margin = new System.Windows.Forms.Padding(0);
      this.guiRight.Name = "guiRight";
      this.guiRight.Size = new System.Drawing.Size(150, 362);
      this.guiRight.TabIndex = 1;
      // 
      // ExternalWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(684, 362);
      this.Controls.Add(this.table);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimumSize = new System.Drawing.Size(700, 400);
      this.Name = "ExternalWindow";
      this.Text = "ExternalWindow";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExternalWindowFormClosing);
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExternalWindowFormClosed);
      this.Shown += new System.EventHandler(this.ExternalWindowShown);
      this.table.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private Lem_Settings.GuiRight guiRight;
    private Lem_Settings.GuiLeft guiLeft;
    private System.Windows.Forms.TableLayoutPanel table;
  }
}
