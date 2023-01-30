// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

/*
 * Crée par SharpDevelop.
 * Utilisateur: Davy
 * Date: 29/01/2015
 * Heure: 14:47
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace Lem_Settings
{
  partial class MainForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.table = new System.Windows.Forms.TableLayoutPanel();
      this.menuStrip = new System.Windows.Forms.MenuStrip();
      this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.leftPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.rightPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.develToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.printCentralAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.getReferencedassembliesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.onlinehelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.modificationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // table
      // 
      this.table.ColumnCount = 3;
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
      this.table.Dock = System.Windows.Forms.DockStyle.Fill;
      this.table.Location = new System.Drawing.Point(0, 24);
      this.table.Name = "table";
      this.table.RowCount = 1;
      this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.table.Size = new System.Drawing.Size(684, 338);
      this.table.TabIndex = 0;
      // 
      // menuStrip
      // 
      this.menuStrip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("menuStrip.BackgroundImage")));
      this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.editToolStripMenuItem,
      this.viewToolStripMenuItem,
      this.develToolStripMenuItem,
      this.helpToolStripMenuItem,
      this.modificationToolStripMenuItem});
      this.menuStrip.Location = new System.Drawing.Point(0, 0);
      this.menuStrip.Name = "menuStrip";
      this.menuStrip.Padding = new System.Windows.Forms.Padding(2, 2, 0, 2);
      this.menuStrip.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.menuStrip.Size = new System.Drawing.Size(684, 24);
      this.menuStrip.TabIndex = 1;
      this.menuStrip.Text = "menuStrip";
      // 
      // editToolStripMenuItem
      // 
      this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.preferencesToolStripMenuItem});
      this.editToolStripMenuItem.ForeColor = System.Drawing.Color.White;
      this.editToolStripMenuItem.Name = "editToolStripMenuItem";
      this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
      this.editToolStripMenuItem.Text = "&Edit";
      this.editToolStripMenuItem.DropDownClosed += new System.EventHandler(this.ToolStripMenuItemDropDownClosed);
      this.editToolStripMenuItem.MouseEnter += new System.EventHandler(this.ToolStripMenuItemMouseEnter);
      this.editToolStripMenuItem.MouseLeave += new System.EventHandler(this.ToolStripMenuItemMouseLeave);
      // 
      // preferencesToolStripMenuItem
      // 
      this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
      this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
      this.preferencesToolStripMenuItem.Text = "&Preferences";
      this.preferencesToolStripMenuItem.Click += new System.EventHandler(this.PreferencesMenuClick);
      // 
      // viewToolStripMenuItem
      // 
      this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.leftPanelToolStripMenuItem,
      this.rightPanelToolStripMenuItem});
      this.viewToolStripMenuItem.ForeColor = System.Drawing.Color.White;
      this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
      this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.viewToolStripMenuItem.Text = "&View";
      this.viewToolStripMenuItem.DropDownClosed += new System.EventHandler(this.ToolStripMenuItemDropDownClosed);
      this.viewToolStripMenuItem.MouseEnter += new System.EventHandler(this.ToolStripMenuItemMouseEnter);
      this.viewToolStripMenuItem.MouseLeave += new System.EventHandler(this.ToolStripMenuItemMouseLeave);
      // 
      // leftPanelToolStripMenuItem
      // 
      this.leftPanelToolStripMenuItem.Checked = true;
      this.leftPanelToolStripMenuItem.CheckOnClick = true;
      this.leftPanelToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.leftPanelToolStripMenuItem.Name = "leftPanelToolStripMenuItem";
      this.leftPanelToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
      this.leftPanelToolStripMenuItem.Text = "&Left panel";
      this.leftPanelToolStripMenuItem.Click += new System.EventHandler(this.LeftPanelMenuClick);
      // 
      // rightPanelToolStripMenuItem
      // 
      this.rightPanelToolStripMenuItem.Checked = true;
      this.rightPanelToolStripMenuItem.CheckOnClick = true;
      this.rightPanelToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.rightPanelToolStripMenuItem.Name = "rightPanelToolStripMenuItem";
      this.rightPanelToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
      this.rightPanelToolStripMenuItem.Text = "&Right panel";
      this.rightPanelToolStripMenuItem.Click += new System.EventHandler(this.RightPanelMenuClick);
      // 
      // develToolStripMenuItem
      // 
      this.develToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.printCentralAreaToolStripMenuItem,
      this.getReferencedassembliesToolStripMenuItem});
      this.develToolStripMenuItem.ForeColor = System.Drawing.Color.White;
      this.develToolStripMenuItem.Name = "develToolStripMenuItem";
      this.develToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
      this.develToolStripMenuItem.Text = "&Devel";
      this.develToolStripMenuItem.DropDownClosed += new System.EventHandler(this.ToolStripMenuItemDropDownClosed);
      this.develToolStripMenuItem.MouseEnter += new System.EventHandler(this.ToolStripMenuItemMouseEnter);
      this.develToolStripMenuItem.MouseLeave += new System.EventHandler(this.ToolStripMenuItemMouseLeave);
      // 
      // printCentralAreaToolStripMenuItem
      // 
      this.printCentralAreaToolStripMenuItem.Name = "printCentralAreaToolStripMenuItem";
      this.printCentralAreaToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
      this.printCentralAreaToolStripMenuItem.Text = "&Print item page(s)";
      this.printCentralAreaToolStripMenuItem.Click += new System.EventHandler(this.PrintCentralAreaToolStripMenuItemClick);
      // 
      // getReferencedassembliesToolStripMenuItem
      // 
      this.getReferencedassembliesToolStripMenuItem.Name = "getReferencedassembliesToolStripMenuItem";
      this.getReferencedassembliesToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
      this.getReferencedassembliesToolStripMenuItem.Text = "Get referenced &assemblies";
      this.getReferencedassembliesToolStripMenuItem.Click += new System.EventHandler(this.GetReferencedassembliesToolStripMenuItemClick);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.onlinehelpToolStripMenuItem,
      this.aboutToolStripMenuItem});
      this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.White;
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(24, 20);
      this.helpToolStripMenuItem.Text = "&?";
      this.helpToolStripMenuItem.DropDownClosed += new System.EventHandler(this.ToolStripMenuItemDropDownClosed);
      this.helpToolStripMenuItem.MouseEnter += new System.EventHandler(this.ToolStripMenuItemMouseEnter);
      this.helpToolStripMenuItem.MouseLeave += new System.EventHandler(this.ToolStripMenuItemMouseLeave);
      // 
      // modificationToolStripMenuItem
      // 
      this.modificationToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.modificationToolStripMenuItem.ForeColor = System.Drawing.Color.White;
      this.modificationToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("modificationToolStripMenuItem.Image")));
      this.modificationToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.modificationToolStripMenuItem.Name = "modificationToolStripMenuItem";
      this.modificationToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
      this.modificationToolStripMenuItem.Size = new System.Drawing.Size(32, 20);
      this.modificationToolStripMenuItem.Text = " 2";
      this.modificationToolStripMenuItem.Visible = false;
      this.modificationToolStripMenuItem.Click += new System.EventHandler(this.ModificationToolStripMenuItemClick);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.ControlLight;
      this.ClientSize = new System.Drawing.Size(684, 362);
      this.Controls.Add(this.table);
      this.Controls.Add(this.menuStrip);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.KeyPreview = true;
      this.MainMenuStrip = this.menuStrip;
      this.MinimumSize = new System.Drawing.Size(700, 400);
      this.Name = "MainForm";
      this.Text = "Settings";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormFormClosed);
      this.Load += new System.EventHandler(this.MainFormLoad);
      this.Shown += new System.EventHandler(this.MainFormShown);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainFormKeyDown);
      this.menuStrip.ResumeLayout(false);
      this.menuStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    private System.Windows.Forms.ToolStripMenuItem onlinehelpToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem rightPanelToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem leftPanelToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
    private System.Windows.Forms.MenuStrip menuStrip;
    private System.Windows.Forms.TableLayoutPanel table;
    private System.Windows.Forms.ToolStripMenuItem develToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem printCentralAreaToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem getReferencedassembliesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem modificationToolStripMenuItem;
  }
}
