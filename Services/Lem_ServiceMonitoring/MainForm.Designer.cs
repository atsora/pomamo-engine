// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace LemoineServiceMonitoring
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
      this.toolStrip = new System.Windows.Forms.ToolStrip();
      this.buttonStart = new System.Windows.Forms.ToolStripButton();
      this.buttonStop = new System.Windows.Forms.ToolStripButton();
      this.buttonRestart = new System.Windows.Forms.ToolStripButton();
      this.ButtonStopAll = new System.Windows.Forms.ToolStripButton();
      this.ButtonStartAll = new System.Windows.Forms.ToolStripButton();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
      this.listView = new System.Windows.Forms.ListView();
      this.columnName = new System.Windows.Forms.ColumnHeader();
      this.columMachineName = new System.Windows.Forms.ColumnHeader();
      this.columnStatus = new System.Windows.Forms.ColumnHeader();
      this.columnStartupType = new System.Windows.Forms.ColumnHeader();
      this.columnDisplay = new System.Windows.Forms.ColumnHeader();
      this.columnPID = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.autoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.manualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.disabledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      this.dependenciesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.dependsOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.statusStrip1.SuspendLayout();
      this.toolStrip.SuspendLayout();
      this.contextMenuStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar});
      this.statusStrip1.Location = new System.Drawing.Point(0, 563);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
      this.statusStrip1.Size = new System.Drawing.Size(730, 24);
      this.statusStrip1.TabIndex = 0;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // progressBar
      // 
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(467, 18);
      this.progressBar.Step = 1;
      // 
      // toolStrip
      // 
      this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonStart,
            this.buttonStop,
            this.buttonRestart,
            this.ButtonStopAll,
            this.ButtonStartAll,
            this.toolStripSeparator2,
            this.buttonRefresh});
      this.toolStrip.Location = new System.Drawing.Point(0, 0);
      this.toolStrip.Name = "toolStrip";
      this.toolStrip.Size = new System.Drawing.Size(730, 25);
      this.toolStrip.TabIndex = 1;
      this.toolStrip.Text = "toolStrip";
      // 
      // buttonStart
      // 
      this.buttonStart.Enabled = false;
      this.buttonStart.Image = ((System.Drawing.Image)(resources.GetObject("buttonStart.Image")));
      this.buttonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.buttonStart.Name = "buttonStart";
      this.buttonStart.Size = new System.Drawing.Size(51, 22);
      this.buttonStart.Text = "Start";
      this.buttonStart.Click += new System.EventHandler(this.buttonStartClick);
      // 
      // buttonStop
      // 
      this.buttonStop.Enabled = false;
      this.buttonStop.Image = ((System.Drawing.Image)(resources.GetObject("buttonStop.Image")));
      this.buttonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.buttonStop.Name = "buttonStop";
      this.buttonStop.Size = new System.Drawing.Size(51, 22);
      this.buttonStop.Text = "Stop";
      this.buttonStop.Click += new System.EventHandler(this.buttonStopClick);
      // 
      // buttonRestart
      // 
      this.buttonRestart.Enabled = false;
      this.buttonRestart.Image = ((System.Drawing.Image)(resources.GetObject("buttonRestart.Image")));
      this.buttonRestart.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.buttonRestart.Name = "buttonRestart";
      this.buttonRestart.Size = new System.Drawing.Size(63, 22);
      this.buttonRestart.Text = "Restart";
      this.buttonRestart.Click += new System.EventHandler(this.buttonRestartClick);
      // 
      // ButtonStopAll
      // 
      this.ButtonStopAll.Image = ((System.Drawing.Image)(resources.GetObject("ButtonStopAll.Image")));
      this.ButtonStopAll.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.ButtonStopAll.Name = "ButtonStopAll";
      this.ButtonStopAll.Size = new System.Drawing.Size(68, 22);
      this.ButtonStopAll.Text = "Stop All";
      this.ButtonStopAll.ToolTipText = "Stop All Lemoine Services";
      this.ButtonStopAll.Click += new System.EventHandler(this.ButtonStopAllClick);
      // 
      // ButtonStartAll
      // 
      this.ButtonStartAll.Image = ((System.Drawing.Image)(resources.GetObject("ButtonStartAll.Image")));
      this.ButtonStartAll.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.ButtonStartAll.Name = "ButtonStartAll";
      this.ButtonStartAll.Size = new System.Drawing.Size(68, 22);
      this.ButtonStartAll.Text = "Start All";
      this.ButtonStartAll.ToolTipText = "Start All Automatic Services";
      this.ButtonStartAll.Click += new System.EventHandler(this.ButtonStartAllClick);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
      // 
      // buttonRefresh
      // 
      this.buttonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("buttonRefresh.Image")));
      this.buttonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.buttonRefresh.Name = "buttonRefresh";
      this.buttonRefresh.Size = new System.Drawing.Size(66, 22);
      this.buttonRefresh.Text = "Refresh";
      this.buttonRefresh.Click += new System.EventHandler(this.buttonRefreshClick);
      // 
      // listView
      // 
      this.listView.AllowColumnReorder = true;
      this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnName,
            this.columMachineName,
            this.columnStatus,
            this.columnStartupType,
            this.columnDisplay,
            this.columnPID});
      this.listView.ContextMenuStrip = this.contextMenuStrip;
      this.listView.FullRowSelect = true;
      this.listView.Location = new System.Drawing.Point(0, 32);
      this.listView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.listView.Name = "listView";
      this.listView.Size = new System.Drawing.Size(730, 526);
      this.listView.TabIndex = 2;
      this.listView.UseCompatibleStateImageBehavior = false;
      this.listView.View = System.Windows.Forms.View.Details;
      this.listView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.itemSelectionChanged);
      // 
      // columnName
      // 
      this.columnName.Name = "columnName";
      this.columnName.Text = "Service name";
      this.columnName.Width = 147;
      // 
      // columMachineName
      // 
      this.columMachineName.Name = "columMachineName";
      this.columMachineName.Text = "Machine";
      // 
      // columnStatus
      // 
      this.columnStatus.Name = "columnStatus";
      this.columnStatus.Text = "Status";
      this.columnStatus.Width = 57;
      // 
      // columnStartupType
      // 
      this.columnStartupType.Name = "columnStartupType";
      this.columnStartupType.Text = "Startup Type";
      this.columnStartupType.Width = 73;
      // 
      // columnDisplayName
      // 
      this.columnDisplay.Name = "columnDisplay";
      this.columnDisplay.Text = "Display name";
      this.columnDisplay.Width = 211;
      // 
      // columnPID
      // 
      this.columnPID.Name = "columnPID";
      this.columnPID.Text = "PID";
      this.columnPID.Width = 45;
      // 
      // contextMenuStrip
      // 
      this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.restartToolStripMenuItem,
            this.toolStripSeparator1,
            this.autoToolStripMenuItem,
            this.manualToolStripMenuItem,
            this.disabledToolStripMenuItem,
            this.toolStripSeparator3,
            this.dependenciesToolStripMenuItem,
            this.dependsOnToolStripMenuItem});
      this.contextMenuStrip.Name = "contextMenuStripService";
      this.contextMenuStrip.Size = new System.Drawing.Size(149, 192);
      this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripOpening);
      // 
      // startToolStripMenuItem
      // 
      this.startToolStripMenuItem.Enabled = false;
      this.startToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("startToolStripMenuItem.Image")));
      this.startToolStripMenuItem.Name = "startToolStripMenuItem";
      this.startToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.startToolStripMenuItem.Text = "Start";
      this.startToolStripMenuItem.Click += new System.EventHandler(this.buttonStartClick);
      // 
      // stopToolStripMenuItem
      // 
      this.stopToolStripMenuItem.Enabled = false;
      this.stopToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("stopToolStripMenuItem.Image")));
      this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
      this.stopToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.stopToolStripMenuItem.Text = "Stop";
      this.stopToolStripMenuItem.Click += new System.EventHandler(this.buttonStopClick);
      // 
      // restartToolStripMenuItem
      // 
      this.restartToolStripMenuItem.Enabled = false;
      this.restartToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("restartToolStripMenuItem.Image")));
      this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
      this.restartToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.restartToolStripMenuItem.Text = "Restart";
      this.restartToolStripMenuItem.Click += new System.EventHandler(this.buttonRestartClick);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(145, 6);
      // 
      // autoToolStripMenuItem
      // 
      this.autoToolStripMenuItem.Checked = true;
      this.autoToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.autoToolStripMenuItem.Enabled = false;
      this.autoToolStripMenuItem.Name = "autoToolStripMenuItem";
      this.autoToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.autoToolStripMenuItem.Text = "Auto";
      this.autoToolStripMenuItem.Click += new System.EventHandler(this.buttonAutoClick);
      // 
      // manualToolStripMenuItem
      // 
      this.manualToolStripMenuItem.Name = "manualToolStripMenuItem";
      this.manualToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.manualToolStripMenuItem.Text = "Manual";
      this.manualToolStripMenuItem.Click += new System.EventHandler(this.buttonManualClick);
      // 
      // disabledToolStripMenuItem
      // 
      this.disabledToolStripMenuItem.Name = "disabledToolStripMenuItem";
      this.disabledToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.disabledToolStripMenuItem.Text = "Disabled";
      this.disabledToolStripMenuItem.Click += new System.EventHandler(this.buttonDisabledClick);
      // 
      // toolStripSeparator3
      // 
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new System.Drawing.Size(145, 6);
      // 
      // dependenciesToolStripMenuItem
      // 
      this.dependenciesToolStripMenuItem.Name = "dependenciesToolStripMenuItem";
      this.dependenciesToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.dependenciesToolStripMenuItem.Text = "Dependencies";
      // 
      // dependsOnToolStripMenuItem
      // 
      this.dependsOnToolStripMenuItem.Name = "dependsOnToolStripMenuItem";
      this.dependsOnToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.dependsOnToolStripMenuItem.Text = "Depends On";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(730, 587);
      this.Controls.Add(this.listView);
      this.Controls.Add(this.toolStrip);
      this.Controls.Add(this.statusStrip1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Service Monitoring";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.toolStrip.ResumeLayout(false);
      this.toolStrip.PerformLayout();
      this.contextMenuStrip.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    private System.Windows.Forms.ColumnHeader columMachineName;
    private System.Windows.Forms.ToolStripButton ButtonStartAll;
    private System.Windows.Forms.ToolStripMenuItem dependsOnToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem dependenciesToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ColumnHeader columnPID;
    private System.Windows.Forms.ToolStripButton ButtonStopAll;
    private System.Windows.Forms.ToolStripButton buttonRefresh;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripMenuItem disabledToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem manualToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem autoToolStripMenuItem;
    private System.Windows.Forms.ToolStripButton buttonStart;
    private System.Windows.Forms.ToolStripProgressBar progressBar;
    private System.Windows.Forms.ToolStripButton buttonStop;
    private System.Windows.Forms.ToolStripButton buttonRestart;
    private System.Windows.Forms.ColumnHeader columnDisplay;
    private System.Windows.Forms.ColumnHeader columnName;
    private System.Windows.Forms.ColumnHeader columnStartupType;
    private System.Windows.Forms.ColumnHeader columnStatus;
    private System.Windows.Forms.ListView listView;
    private System.Windows.Forms.ToolStrip toolStrip;
    private System.Windows.Forms.StatusStrip statusStrip1;
  }
}
