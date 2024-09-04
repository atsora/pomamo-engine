// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewReasons
{
  partial class ReasonCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelReasonDisplay;
    private Lemoine.BaseControls.Marker markerReasonColor;
    private System.Windows.Forms.Label labelReasonDetails;
    
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
      this.labelReasonDisplay = new System.Windows.Forms.Label();
      this.markerReasonColor = new Lemoine.BaseControls.Marker();
      this.labelReasonDetails = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.labelReasonDisplay, 1, 0);
      this.baseLayout.Controls.Add(this.markerReasonColor, 0, 0);
      this.baseLayout.Controls.Add(this.labelReasonDetails, 1, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(310, 40);
      this.baseLayout.TabIndex = 0;
      // 
      // labelReasonDisplay
      // 
      this.labelReasonDisplay.AutoEllipsis = true;
      this.labelReasonDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelReasonDisplay.Location = new System.Drawing.Point(37, 0);
      this.labelReasonDisplay.Name = "labelReasonDisplay";
      this.labelReasonDisplay.Size = new System.Drawing.Size(270, 20);
      this.labelReasonDisplay.TabIndex = 1;
      this.labelReasonDisplay.Text = "...";
      this.labelReasonDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // markerReasonColor
      // 
      this.markerReasonColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.markerReasonColor.Dock = System.Windows.Forms.DockStyle.Fill;
      this.markerReasonColor.Location = new System.Drawing.Point(0, 0);
      this.markerReasonColor.Margin = new System.Windows.Forms.Padding(0);
      this.markerReasonColor.Name = "markerReasonColor";
      this.baseLayout.SetRowSpan(this.markerReasonColor, 2);
      this.markerReasonColor.Size = new System.Drawing.Size(34, 40);
      this.markerReasonColor.TabIndex = 2;
      // 
      // labelReasonDetails
      // 
      this.labelReasonDetails.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelReasonDetails.Location = new System.Drawing.Point(37, 20);
      this.labelReasonDetails.Name = "labelReasonDetails";
      this.labelReasonDetails.Size = new System.Drawing.Size(270, 20);
      this.labelReasonDetails.TabIndex = 3;
      this.labelReasonDetails.Text = "details";
      // 
      // ReasonCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.MinimumSize = new System.Drawing.Size(310, 40);
      this.Name = "ReasonCell";
      this.Size = new System.Drawing.Size(310, 40);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
