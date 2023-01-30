// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_MTConnectSimpleViewer
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
    	this.urlTextBox = new System.Windows.Forms.TextBox();
    	this.updateButton = new System.Windows.Forms.Button();
    	this.urlLabel = new System.Windows.Forms.Label();
    	this.spindleSpeedOverrideLabel = new System.Windows.Forms.Label();
    	this.refreshNumericUpDown = new System.Windows.Forms.NumericUpDown();
    	this.label2 = new System.Windows.Forms.Label();
    	this.refreshTimer = new System.Windows.Forms.Timer(this.components);
    	this.webBrowser = new System.Windows.Forms.WebBrowser();
    	((System.ComponentModel.ISupportInitialize)(this.refreshNumericUpDown)).BeginInit();
    	this.SuspendLayout();
    	// 
    	// urlTextBox
    	// 
    	this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
    	    	    	| System.Windows.Forms.AnchorStyles.Right)));
    	this.urlTextBox.Location = new System.Drawing.Point(145, 13);
    	this.urlTextBox.Name = "urlTextBox";
    	this.urlTextBox.Size = new System.Drawing.Size(548, 20);
    	this.urlTextBox.TabIndex = 0;
    	this.urlTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UrlTextBoxKeyDown);
    	// 
    	// updateButton
    	// 
    	this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
    	this.updateButton.Location = new System.Drawing.Point(699, 11);
    	this.updateButton.Name = "updateButton";
    	this.updateButton.Size = new System.Drawing.Size(75, 23);
    	this.updateButton.TabIndex = 1;
    	this.updateButton.Text = "Update";
    	this.updateButton.UseVisualStyleBackColor = true;
    	this.updateButton.Click += new System.EventHandler(this.UpdateButtonClick);
    	// 
    	// urlLabel
    	// 
    	this.urlLabel.Location = new System.Drawing.Point(13, 9);
    	this.urlLabel.Name = "urlLabel";
    	this.urlLabel.Size = new System.Drawing.Size(126, 23);
    	this.urlLabel.TabIndex = 2;
    	this.urlLabel.Text = "MTConnect agent URL";
    	this.urlLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
    	// 
    	// spindleSpeedOverrideLabel
    	// 
    	this.spindleSpeedOverrideLabel.Location = new System.Drawing.Point(12, 229);
    	this.spindleSpeedOverrideLabel.Name = "spindleSpeedOverrideLabel";
    	this.spindleSpeedOverrideLabel.Size = new System.Drawing.Size(626, 23);
    	this.spindleSpeedOverrideLabel.TabIndex = 10;
    	this.spindleSpeedOverrideLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
    	// 
    	// refreshNumericUpDown
    	// 
    	this.refreshNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
    	this.refreshNumericUpDown.Location = new System.Drawing.Point(876, 12);
    	this.refreshNumericUpDown.Maximum = new decimal(new int[] {
    	    	    	60,
    	    	    	0,
    	    	    	0,
    	    	    	0});
    	this.refreshNumericUpDown.Minimum = new decimal(new int[] {
    	    	    	1,
    	    	    	0,
    	    	    	0,
    	    	    	0});
    	this.refreshNumericUpDown.Name = "refreshNumericUpDown";
    	this.refreshNumericUpDown.Size = new System.Drawing.Size(41, 20);
    	this.refreshNumericUpDown.TabIndex = 14;
    	this.refreshNumericUpDown.Value = new decimal(new int[] {
    	    	    	3,
    	    	    	0,
    	    	    	0,
    	    	    	0});
    	this.refreshNumericUpDown.ValueChanged += new System.EventHandler(this.RefreshNumericUpDownValueChanged);
    	// 
    	// label2
    	// 
    	this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
    	this.label2.Location = new System.Drawing.Point(782, 8);
    	this.label2.Name = "label2";
    	this.label2.Size = new System.Drawing.Size(88, 23);
    	this.label2.TabIndex = 15;
    	this.label2.Text = "Refresh rate (s)";
    	this.label2.TextAlign = System.Drawing.ContentAlignment.BottomRight;
    	// 
    	// refreshTimer
    	// 
    	this.refreshTimer.Enabled = true;
    	this.refreshTimer.Interval = 1000;
    	this.refreshTimer.Tick += new System.EventHandler(this.RefreshTimerTick);
    	// 
    	// webBrowser
    	// 
    	this.webBrowser.AllowWebBrowserDrop = false;
    	this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
    	    	    	| System.Windows.Forms.AnchorStyles.Left) 
    	    	    	| System.Windows.Forms.AnchorStyles.Right)));
    	this.webBrowser.IsWebBrowserContextMenuEnabled = false;
    	this.webBrowser.Location = new System.Drawing.Point(0, 40);
    	this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
    	this.webBrowser.Name = "webBrowser";
    	this.webBrowser.ScriptErrorsSuppressed = true;
    	this.webBrowser.Size = new System.Drawing.Size(929, 769);
    	this.webBrowser.TabIndex = 16;
    	this.webBrowser.WebBrowserShortcutsEnabled = false;
    	this.webBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.WebBrowserNavigating);
    	// 
    	// MainForm
    	// 
    	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.ClientSize = new System.Drawing.Size(929, 809);
    	this.Controls.Add(this.webBrowser);
    	this.Controls.Add(this.label2);
    	this.Controls.Add(this.refreshNumericUpDown);
    	this.Controls.Add(this.spindleSpeedOverrideLabel);
    	this.Controls.Add(this.urlLabel);
    	this.Controls.Add(this.updateButton);
    	this.Controls.Add(this.urlTextBox);
    	this.Name = "MainForm";
    	this.Text = "Lem_MTConnectSimpleViewer";
    	((System.ComponentModel.ISupportInitialize)(this.refreshNumericUpDown)).EndInit();
    	this.ResumeLayout(false);
    	this.PerformLayout();
    }
    private System.Windows.Forms.WebBrowser webBrowser;
    private System.Windows.Forms.Timer refreshTimer;
    private System.Windows.Forms.NumericUpDown refreshNumericUpDown;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label spindleSpeedOverrideLabel;
    private System.Windows.Forms.Label urlLabel;
    private System.Windows.Forms.Button updateButton;
    private System.Windows.Forms.TextBox urlTextBox;
  }
}
