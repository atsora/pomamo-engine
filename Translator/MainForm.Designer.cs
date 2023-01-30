// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Translator
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
    	this.toolStrip1 = new System.Windows.Forms.ToolStrip();
    	this.openButton = new System.Windows.Forms.ToolStripButton();
    	this.saveButton = new System.Windows.Forms.ToolStripButton();
    	this.exportButton = new System.Windows.Forms.ToolStripButton();
    	this.importButton = new System.Windows.Forms.ToolStripButton();
    	this.tabControl = new System.Windows.Forms.TabControl();
    	this.MMSMNRTabPage = new System.Windows.Forms.TabPage();
    	this.listViewMMSMNR = new System.Windows.Forms.ListView();
    	this.columnHeaderKey = new System.Windows.Forms.ColumnHeader();
    	this.columnHeaderSource = new System.Windows.Forms.ColumnHeader();
    	this.columnHeaderLocale = new System.Windows.Forms.ColumnHeader();
    	this.MMSMSGTabPage = new System.Windows.Forms.TabPage();
    	this.listViewMMSMSG = new System.Windows.Forms.ListView();
    	this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
    	this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
    	this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
    	this.REPORTTabPage = new System.Windows.Forms.TabPage();
    	this.listViewREPORT = new System.Windows.Forms.ListView();
    	this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
    	this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
    	this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
    	this.DotNetTabPage = new System.Windows.Forms.TabPage();
    	this.listViewDotNet = new System.Windows.Forms.ListView();
    	this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
    	this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
    	this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
    	this.BirtTabPage = new System.Windows.Forms.TabPage();
    	this.listViewBirt = new System.Windows.Forms.ListView();
    	this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
    	this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
    	this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
    	this.translationTextBox = new System.Windows.Forms.TextBox();
    	this.toolStrip2 = new System.Windows.Forms.ToolStrip();
    	this.nextToolStripButton = new System.Windows.Forms.ToolStripButton();
    	this.prevToolStripButton = new System.Windows.Forms.ToolStripButton();
    	this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
    	this.searchLabel = new System.Windows.Forms.ToolStripLabel();
    	this.searchTextBox = new System.Windows.Forms.ToolStripTextBox();
    	this.nextSearchButton = new System.Windows.Forms.ToolStripButton();
    	this.previousSearchButton = new System.Windows.Forms.ToolStripButton();
    	this.statusStrip1 = new System.Windows.Forms.StatusStrip();
    	this.translationFileLabel = new System.Windows.Forms.ToolStripStatusLabel();
    	this.exportFileDialog = new System.Windows.Forms.SaveFileDialog();
    	this.importFileDialog = new System.Windows.Forms.OpenFileDialog();
    	this.toolStrip1.SuspendLayout();
    	this.tabControl.SuspendLayout();
    	this.MMSMNRTabPage.SuspendLayout();
    	this.MMSMSGTabPage.SuspendLayout();
    	this.REPORTTabPage.SuspendLayout();
    	this.DotNetTabPage.SuspendLayout();
    	this.BirtTabPage.SuspendLayout();
    	this.toolStrip2.SuspendLayout();
    	this.statusStrip1.SuspendLayout();
    	this.SuspendLayout();
    	// 
    	// toolStrip1
    	// 
    	this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
    	this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
    	    	    	this.openButton,
    	    	    	this.saveButton,
    	    	    	this.exportButton,
    	    	    	this.importButton});
    	this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
    	resources.ApplyResources(this.toolStrip1, "toolStrip1");
    	this.toolStrip1.Name = "toolStrip1";
    	// 
    	// openButton
    	// 
    	resources.ApplyResources(this.openButton, "openButton");
    	this.openButton.Name = "openButton";
    	this.openButton.Click += new System.EventHandler(this.OpenButtonClick);
    	// 
    	// saveButton
    	// 
    	resources.ApplyResources(this.saveButton, "saveButton");
    	this.saveButton.Name = "saveButton";
    	this.saveButton.Click += new System.EventHandler(this.SaveButtonClick);
    	// 
    	// exportButton
    	// 
    	resources.ApplyResources(this.exportButton, "exportButton");
    	this.exportButton.Name = "exportButton";
    	this.exportButton.Click += new System.EventHandler(this.ExportButtonClick);
    	// 
    	// importButton
    	// 
    	resources.ApplyResources(this.importButton, "importButton");
    	this.importButton.Name = "importButton";
    	this.importButton.Click += new System.EventHandler(this.ImportButtonClick);
    	// 
    	// tabControl
    	// 
    	resources.ApplyResources(this.tabControl, "tabControl");
    	this.tabControl.Controls.Add(this.MMSMNRTabPage);
    	this.tabControl.Controls.Add(this.MMSMSGTabPage);
    	this.tabControl.Controls.Add(this.REPORTTabPage);
    	this.tabControl.Controls.Add(this.DotNetTabPage);
    	this.tabControl.Controls.Add(this.BirtTabPage);
    	this.tabControl.Name = "tabControl";
    	this.tabControl.SelectedIndex = 0;
    	// 
    	// MMSMNRTabPage
    	// 
    	this.MMSMNRTabPage.Controls.Add(this.listViewMMSMNR);
    	resources.ApplyResources(this.MMSMNRTabPage, "MMSMNRTabPage");
    	this.MMSMNRTabPage.Name = "MMSMNRTabPage";
    	this.MMSMNRTabPage.UseVisualStyleBackColor = true;
    	this.MMSMNRTabPage.Paint += new System.Windows.Forms.PaintEventHandler(this.MMSMNRTabPagePaint);
    	// 
    	// listViewMMSMNR
    	// 
    	this.listViewMMSMNR.AllowColumnReorder = true;
    	this.listViewMMSMNR.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
    	    	    	this.columnHeaderKey,
    	    	    	this.columnHeaderSource,
    	    	    	this.columnHeaderLocale});
    	resources.ApplyResources(this.listViewMMSMNR, "listViewMMSMNR");
    	this.listViewMMSMNR.FullRowSelect = true;
    	this.listViewMMSMNR.GridLines = true;
    	this.listViewMMSMNR.HideSelection = false;
    	this.listViewMMSMNR.MultiSelect = false;
    	this.listViewMMSMNR.Name = "listViewMMSMNR";
    	this.listViewMMSMNR.ShowGroups = false;
    	this.listViewMMSMNR.UseCompatibleStateImageBehavior = false;
    	this.listViewMMSMNR.View = System.Windows.Forms.View.Details;
    	this.listViewMMSMNR.VisibleChanged += new System.EventHandler(this.ListViewMMSMNRVisibleChanged);
    	this.listViewMMSMNR.SelectedIndexChanged += new System.EventHandler(this.ListViewMMSMNRSelectedIndexChanged);
    	this.listViewMMSMNR.Enter += new System.EventHandler(this.ListViewMMSMNREnter);
    	// 
    	// columnHeaderKey
    	// 
    	resources.ApplyResources(this.columnHeaderKey, "columnHeaderKey");
    	// 
    	// columnHeaderSource
    	// 
    	resources.ApplyResources(this.columnHeaderSource, "columnHeaderSource");
    	// 
    	// columnHeaderLocale
    	// 
    	resources.ApplyResources(this.columnHeaderLocale, "columnHeaderLocale");
    	// 
    	// MMSMSGTabPage
    	// 
    	this.MMSMSGTabPage.Controls.Add(this.listViewMMSMSG);
    	resources.ApplyResources(this.MMSMSGTabPage, "MMSMSGTabPage");
    	this.MMSMSGTabPage.Name = "MMSMSGTabPage";
    	this.MMSMSGTabPage.UseVisualStyleBackColor = true;
    	this.MMSMSGTabPage.Paint += new System.Windows.Forms.PaintEventHandler(this.MMSMSGTabPagePaint);
    	// 
    	// listViewMMSMSG
    	// 
    	this.listViewMMSMSG.AllowColumnReorder = true;
    	this.listViewMMSMSG.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
    	    	    	this.columnHeader1,
    	    	    	this.columnHeader2,
    	    	    	this.columnHeader3});
    	resources.ApplyResources(this.listViewMMSMSG, "listViewMMSMSG");
    	this.listViewMMSMSG.FullRowSelect = true;
    	this.listViewMMSMSG.GridLines = true;
    	this.listViewMMSMSG.HideSelection = false;
    	this.listViewMMSMSG.MultiSelect = false;
    	this.listViewMMSMSG.Name = "listViewMMSMSG";
    	this.listViewMMSMSG.ShowGroups = false;
    	this.listViewMMSMSG.UseCompatibleStateImageBehavior = false;
    	this.listViewMMSMSG.View = System.Windows.Forms.View.Details;
    	this.listViewMMSMSG.VisibleChanged += new System.EventHandler(this.ListViewMMSMSGVisibleChanged);
    	this.listViewMMSMSG.SelectedIndexChanged += new System.EventHandler(this.ListViewMMSMSGSelectedIndexChanged);
    	this.listViewMMSMSG.Enter += new System.EventHandler(this.ListViewMMSMSGEnter);
    	// 
    	// columnHeader1
    	// 
    	resources.ApplyResources(this.columnHeader1, "columnHeader1");
    	// 
    	// columnHeader2
    	// 
    	resources.ApplyResources(this.columnHeader2, "columnHeader2");
    	// 
    	// columnHeader3
    	// 
    	resources.ApplyResources(this.columnHeader3, "columnHeader3");
    	// 
    	// REPORTTabPage
    	// 
    	this.REPORTTabPage.Controls.Add(this.listViewREPORT);
    	resources.ApplyResources(this.REPORTTabPage, "REPORTTabPage");
    	this.REPORTTabPage.Name = "REPORTTabPage";
    	this.REPORTTabPage.UseVisualStyleBackColor = true;
    	this.REPORTTabPage.Paint += new System.Windows.Forms.PaintEventHandler(this.REPORTTabPagePaint);
    	// 
    	// listViewREPORT
    	// 
    	this.listViewREPORT.AllowColumnReorder = true;
    	this.listViewREPORT.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
    	    	    	this.columnHeader4,
    	    	    	this.columnHeader5,
    	    	    	this.columnHeader6});
    	resources.ApplyResources(this.listViewREPORT, "listViewREPORT");
    	this.listViewREPORT.FullRowSelect = true;
    	this.listViewREPORT.GridLines = true;
    	this.listViewREPORT.HideSelection = false;
    	this.listViewREPORT.MultiSelect = false;
    	this.listViewREPORT.Name = "listViewREPORT";
    	this.listViewREPORT.ShowGroups = false;
    	this.listViewREPORT.UseCompatibleStateImageBehavior = false;
    	this.listViewREPORT.View = System.Windows.Forms.View.Details;
    	this.listViewREPORT.VisibleChanged += new System.EventHandler(this.ListViewREPORTVisibleChanged);
    	this.listViewREPORT.SelectedIndexChanged += new System.EventHandler(this.ListViewREPORTSelectedIndexChanged);
    	this.listViewREPORT.Enter += new System.EventHandler(this.ListViewREPORTEnter);
    	// 
    	// columnHeader4
    	// 
    	resources.ApplyResources(this.columnHeader4, "columnHeader4");
    	// 
    	// columnHeader5
    	// 
    	resources.ApplyResources(this.columnHeader5, "columnHeader5");
    	// 
    	// columnHeader6
    	// 
    	resources.ApplyResources(this.columnHeader6, "columnHeader6");
    	// 
    	// DotNetTabPage
    	// 
    	this.DotNetTabPage.Controls.Add(this.listViewDotNet);
    	resources.ApplyResources(this.DotNetTabPage, "DotNetTabPage");
    	this.DotNetTabPage.Name = "DotNetTabPage";
    	this.DotNetTabPage.UseVisualStyleBackColor = true;
    	this.DotNetTabPage.Paint += new System.Windows.Forms.PaintEventHandler(this.DotNetTabPagePaint);
    	// 
    	// listViewDotNet
    	// 
    	this.listViewDotNet.AllowColumnReorder = true;
    	this.listViewDotNet.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
    	    	    	this.columnHeader7,
    	    	    	this.columnHeader8,
    	    	    	this.columnHeader9});
    	resources.ApplyResources(this.listViewDotNet, "listViewDotNet");
    	this.listViewDotNet.FullRowSelect = true;
    	this.listViewDotNet.GridLines = true;
    	this.listViewDotNet.HideSelection = false;
    	this.listViewDotNet.MultiSelect = false;
    	this.listViewDotNet.Name = "listViewDotNet";
    	this.listViewDotNet.ShowGroups = false;
    	this.listViewDotNet.UseCompatibleStateImageBehavior = false;
    	this.listViewDotNet.View = System.Windows.Forms.View.Details;
    	this.listViewDotNet.VisibleChanged += new System.EventHandler(this.ListViewDotNetVisibleChanged);
    	this.listViewDotNet.SelectedIndexChanged += new System.EventHandler(this.ListViewDotNetSelectedIndexChanged);
    	this.listViewDotNet.Enter += new System.EventHandler(this.ListViewDotNetEnter);
    	// 
    	// columnHeader7
    	// 
    	resources.ApplyResources(this.columnHeader7, "columnHeader7");
    	// 
    	// columnHeader8
    	// 
    	resources.ApplyResources(this.columnHeader8, "columnHeader8");
    	// 
    	// columnHeader9
    	// 
    	resources.ApplyResources(this.columnHeader9, "columnHeader9");
    	// 
    	// BirtTabPage
    	// 
    	this.BirtTabPage.Controls.Add(this.listViewBirt);
    	resources.ApplyResources(this.BirtTabPage, "BirtTabPage");
    	this.BirtTabPage.Name = "BirtTabPage";
    	this.BirtTabPage.UseVisualStyleBackColor = true;
    	this.BirtTabPage.Paint += new System.Windows.Forms.PaintEventHandler(this.BirtTabPagePaint);
    	// 
    	// listViewBirt
    	// 
    	this.listViewBirt.AllowColumnReorder = true;
    	this.listViewBirt.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
    	    	    	this.columnHeader10,
    	    	    	this.columnHeader11,
    	    	    	this.columnHeader12});
    	resources.ApplyResources(this.listViewBirt, "listViewBirt");
    	this.listViewBirt.FullRowSelect = true;
    	this.listViewBirt.GridLines = true;
    	this.listViewBirt.HideSelection = false;
    	this.listViewBirt.MultiSelect = false;
    	this.listViewBirt.Name = "listViewBirt";
    	this.listViewBirt.ShowGroups = false;
    	this.listViewBirt.UseCompatibleStateImageBehavior = false;
    	this.listViewBirt.View = System.Windows.Forms.View.Details;
    	this.listViewBirt.VisibleChanged += new System.EventHandler(this.ListViewBirtVisibleChanged);
    	this.listViewBirt.SelectedIndexChanged += new System.EventHandler(this.ListViewBirtSelectedIndexChanged);
    	this.listViewBirt.Enter += new System.EventHandler(this.ListViewBirtEnter);
    	// 
    	// columnHeader10
    	// 
    	resources.ApplyResources(this.columnHeader10, "columnHeader10");
    	// 
    	// columnHeader11
    	// 
    	resources.ApplyResources(this.columnHeader11, "columnHeader11");
    	// 
    	// columnHeader12
    	// 
    	resources.ApplyResources(this.columnHeader12, "columnHeader12");
    	// 
    	// translationTextBox
    	// 
    	resources.ApplyResources(this.translationTextBox, "translationTextBox");
    	this.translationTextBox.Name = "translationTextBox";
    	this.translationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TranslationTextBoxKeyDown);
    	this.translationTextBox.Leave += new System.EventHandler(this.TranslationTextBoxCommitChange);
    	// 
    	// toolStrip2
    	// 
    	this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
    	this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
    	    	    	this.nextToolStripButton,
    	    	    	this.prevToolStripButton,
    	    	    	this.toolStripSeparator1,
    	    	    	this.searchLabel,
    	    	    	this.searchTextBox,
    	    	    	this.nextSearchButton,
    	    	    	this.previousSearchButton});
    	this.toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
    	resources.ApplyResources(this.toolStrip2, "toolStrip2");
    	this.toolStrip2.Name = "toolStrip2";
    	// 
    	// nextToolStripButton
    	// 
    	resources.ApplyResources(this.nextToolStripButton, "nextToolStripButton");
    	this.nextToolStripButton.Name = "nextToolStripButton";
    	this.nextToolStripButton.Click += new System.EventHandler(this.NextToolStripButtonClick);
    	// 
    	// prevToolStripButton
    	// 
    	resources.ApplyResources(this.prevToolStripButton, "prevToolStripButton");
    	this.prevToolStripButton.Name = "prevToolStripButton";
    	this.prevToolStripButton.Click += new System.EventHandler(this.PrevToolStripButtonClick);
    	// 
    	// toolStripSeparator1
    	// 
    	this.toolStripSeparator1.Name = "toolStripSeparator1";
    	resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
    	// 
    	// searchLabel
    	// 
    	resources.ApplyResources(this.searchLabel, "searchLabel");
    	this.searchLabel.Name = "searchLabel";
    	// 
    	// searchTextBox
    	// 
    	this.searchTextBox.Name = "searchTextBox";
    	resources.ApplyResources(this.searchTextBox, "searchTextBox");
    	this.searchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchTextBoxKeyDown);
    	// 
    	// nextSearchButton
    	// 
    	this.nextSearchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
    	resources.ApplyResources(this.nextSearchButton, "nextSearchButton");
    	this.nextSearchButton.Name = "nextSearchButton";
    	this.nextSearchButton.Click += new System.EventHandler(this.NextSearchButtonClick);
    	// 
    	// previousSearchButton
    	// 
    	this.previousSearchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
    	resources.ApplyResources(this.previousSearchButton, "previousSearchButton");
    	this.previousSearchButton.Name = "previousSearchButton";
    	this.previousSearchButton.Click += new System.EventHandler(this.PreviousSearchButtonClick);
    	// 
    	// statusStrip1
    	// 
    	this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
    	    	    	this.translationFileLabel});
    	resources.ApplyResources(this.statusStrip1, "statusStrip1");
    	this.statusStrip1.Name = "statusStrip1";
    	// 
    	// translationFileLabel
    	// 
    	this.translationFileLabel.Name = "translationFileLabel";
    	resources.ApplyResources(this.translationFileLabel, "translationFileLabel");
    	// 
    	// exportFileDialog
    	// 
    	this.exportFileDialog.DefaultExt = "zip";
    	resources.ApplyResources(this.exportFileDialog, "exportFileDialog");
    	this.exportFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.ExportFileDialogFileOk);
    	// 
    	// importFileDialog
    	// 
    	this.importFileDialog.DefaultExt = "zip";
    	resources.ApplyResources(this.importFileDialog, "importFileDialog");
    	this.importFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.ImportFileDialogFileOk);
    	// 
    	// MainForm
    	// 
    	resources.ApplyResources(this, "$this");
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.Controls.Add(this.translationTextBox);
    	this.Controls.Add(this.statusStrip1);
    	this.Controls.Add(this.toolStrip2);
    	this.Controls.Add(this.tabControl);
    	this.Controls.Add(this.toolStrip1);
    	this.Name = "MainForm";
    	this.Load += new System.EventHandler(this.MainFormLoad);
    	this.toolStrip1.ResumeLayout(false);
    	this.toolStrip1.PerformLayout();
    	this.tabControl.ResumeLayout(false);
    	this.MMSMNRTabPage.ResumeLayout(false);
    	this.MMSMSGTabPage.ResumeLayout(false);
    	this.REPORTTabPage.ResumeLayout(false);
    	this.DotNetTabPage.ResumeLayout(false);
    	this.BirtTabPage.ResumeLayout(false);
    	this.toolStrip2.ResumeLayout(false);
    	this.toolStrip2.PerformLayout();
    	this.statusStrip1.ResumeLayout(false);
    	this.statusStrip1.PerformLayout();
    	this.ResumeLayout(false);
    	this.PerformLayout();
    }
    private System.Windows.Forms.ToolStripButton previousSearchButton;
    private System.Windows.Forms.ToolStripButton nextSearchButton;
    private System.Windows.Forms.ToolStripTextBox searchTextBox;
    private System.Windows.Forms.ToolStripLabel searchLabel;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.OpenFileDialog importFileDialog;
    private System.Windows.Forms.ToolStripButton importButton;
    private System.Windows.Forms.SaveFileDialog exportFileDialog;
    private System.Windows.Forms.ToolStripButton exportButton;
    private System.Windows.Forms.ToolStripStatusLabel translationFileLabel;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripButton prevToolStripButton;
    private System.Windows.Forms.ToolStripButton nextToolStripButton;
    private System.Windows.Forms.ToolStrip toolStrip2;
    private System.Windows.Forms.ColumnHeader columnHeader12;
    private System.Windows.Forms.ColumnHeader columnHeader11;
    private System.Windows.Forms.ColumnHeader columnHeader10;
    private System.Windows.Forms.ListView listViewBirt;
    private System.Windows.Forms.ListView listViewDotNet;
    private System.Windows.Forms.ColumnHeader columnHeader9;
    private System.Windows.Forms.ColumnHeader columnHeader8;
    private System.Windows.Forms.ColumnHeader columnHeader7;
    private System.Windows.Forms.ListView listViewREPORT;
    private System.Windows.Forms.ColumnHeader columnHeader6;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ListView listViewMMSMSG;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.TextBox translationTextBox;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.ListView listViewMMSMNR;
    private System.Windows.Forms.ColumnHeader columnHeaderLocale;
    private System.Windows.Forms.ColumnHeader columnHeaderSource;
    private System.Windows.Forms.ColumnHeader columnHeaderKey;
    private System.Windows.Forms.TabPage BirtTabPage;
    private System.Windows.Forms.TabPage DotNetTabPage;
    private System.Windows.Forms.TabPage REPORTTabPage;
    private System.Windows.Forms.TabPage MMSMSGTabPage;
    private System.Windows.Forms.TabPage MMSMNRTabPage;
    private System.Windows.Forms.ToolStripButton saveButton;
    private System.Windows.Forms.ToolStripButton openButton;
    private System.Windows.Forms.ToolStrip toolStrip1;
    
  }
}
