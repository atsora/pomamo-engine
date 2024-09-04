// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardReasonSelection
{
  partial class Page2
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
      this.machineModeSelection = new Lemoine.DataReferenceControls.MachineModeSelection();
      this.marker = new Lemoine.BaseControls.Marker();
      this.labelDisplay = new System.Windows.Forms.Label();
      this.labelRunning = new System.Windows.Forms.Label();
      this.horizontalLine = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 111F));
      this.baseLayout.Controls.Add(this.machineModeSelection, 0, 0);
      this.baseLayout.Controls.Add(this.marker, 0, 2);
      this.baseLayout.Controls.Add(this.labelDisplay, 1, 2);
      this.baseLayout.Controls.Add(this.labelRunning, 2, 2);
      this.baseLayout.Controls.Add(this.horizontalLine, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
      // 
      // machineModeSelection
      // 
      this.baseLayout.SetColumnSpan(this.machineModeSelection, 3);
      this.machineModeSelection.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModeSelection.Location = new System.Drawing.Point(0, 0);
      this.machineModeSelection.Margin = new System.Windows.Forms.Padding(0);
      this.machineModeSelection.Name = "machineModeSelection";
      this.machineModeSelection.Size = new System.Drawing.Size(370, 264);
      this.machineModeSelection.TabIndex = 0;
      this.machineModeSelection.AfterSelect += new System.EventHandler(this.MachineModeSelectionAfterSelect);
      // 
      // marker
      // 
      this.marker.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker.Location = new System.Drawing.Point(3, 268);
      this.marker.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.marker.Name = "marker";
      this.marker.Size = new System.Drawing.Size(22, 22);
      this.marker.TabIndex = 1;
      // 
      // labelDisplay
      // 
      this.labelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDisplay.Location = new System.Drawing.Point(28, 265);
      this.labelDisplay.Name = "labelDisplay";
      this.labelDisplay.Size = new System.Drawing.Size(228, 25);
      this.labelDisplay.TabIndex = 2;
      this.labelDisplay.Text = "labelDisplay";
      this.labelDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelRunning
      // 
      this.labelRunning.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelRunning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelRunning.ForeColor = System.Drawing.Color.Blue;
      this.labelRunning.Location = new System.Drawing.Point(262, 265);
      this.labelRunning.Name = "labelRunning";
      this.labelRunning.Size = new System.Drawing.Size(105, 25);
      this.labelRunning.TabIndex = 3;
      this.labelRunning.Text = "labelRunning";
      this.labelRunning.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // horizontalLine
      // 
      this.horizontalLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.baseLayout.SetColumnSpan(this.horizontalLine, 3);
      this.horizontalLine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.horizontalLine.Location = new System.Drawing.Point(0, 264);
      this.horizontalLine.Margin = new System.Windows.Forms.Padding(0);
      this.horizontalLine.Name = "horizontalLine";
      this.horizontalLine.Size = new System.Drawing.Size(370, 1);
      this.horizontalLine.TabIndex = 4;
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.DataReferenceControls.MachineModeSelection machineModeSelection;
    private Lemoine.BaseControls.Marker marker;
    private System.Windows.Forms.Label labelDisplay;
    private System.Windows.Forms.Label labelRunning;
    private System.Windows.Forms.Label horizontalLine;
  }
}
