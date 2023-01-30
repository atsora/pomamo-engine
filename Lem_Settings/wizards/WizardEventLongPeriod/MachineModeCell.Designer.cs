// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardEventLongPeriod
{
  partial class MachineModeCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private Lemoine.BaseControls.Marker marker;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelModeName;
    private System.Windows.Forms.Label labelRunning;
    private System.Windows.Forms.CheckBox checkBox;
    
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
      this.marker = new Lemoine.BaseControls.Marker();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelModeName = new System.Windows.Forms.Label();
      this.labelRunning = new System.Windows.Forms.Label();
      this.checkBox = new System.Windows.Forms.CheckBox();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // marker
      // 
      this.marker.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker.Location = new System.Drawing.Point(20, 0);
      this.marker.Margin = new System.Windows.Forms.Padding(0);
      this.marker.Name = "marker";
      this.marker.Size = new System.Drawing.Size(25, 25);
      this.marker.TabIndex = 0;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 43F));
      this.baseLayout.Controls.Add(this.marker, 1, 0);
      this.baseLayout.Controls.Add(this.labelModeName, 2, 0);
      this.baseLayout.Controls.Add(this.labelRunning, 3, 0);
      this.baseLayout.Controls.Add(this.checkBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(250, 25);
      this.baseLayout.TabIndex = 1;
      // 
      // labelModeName
      // 
      this.labelModeName.AutoEllipsis = true;
      this.labelModeName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelModeName.Location = new System.Drawing.Point(48, 0);
      this.labelModeName.Name = "labelModeName";
      this.labelModeName.Size = new System.Drawing.Size(156, 25);
      this.labelModeName.TabIndex = 1;
      this.labelModeName.Text = "labelModeName";
      this.labelModeName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelRunning
      // 
      this.labelRunning.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelRunning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelRunning.ForeColor = System.Drawing.SystemColors.MenuHighlight;
      this.labelRunning.Location = new System.Drawing.Point(207, 0);
      this.labelRunning.Margin = new System.Windows.Forms.Padding(0);
      this.labelRunning.Name = "labelRunning";
      this.labelRunning.Size = new System.Drawing.Size(43, 25);
      this.labelRunning.TabIndex = 2;
      this.labelRunning.Text = "running";
      this.labelRunning.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // checkBox
      // 
      this.checkBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBox.Location = new System.Drawing.Point(0, 0);
      this.checkBox.Margin = new System.Windows.Forms.Padding(0);
      this.checkBox.Name = "checkBox";
      this.checkBox.Size = new System.Drawing.Size(20, 25);
      this.checkBox.TabIndex = 3;
      this.checkBox.UseVisualStyleBackColor = true;
      // 
      // MachineModeCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.MaximumSize = new System.Drawing.Size(0, 25);
      this.MinimumSize = new System.Drawing.Size(250, 25);
      this.Name = "MachineModeCell";
      this.Size = new System.Drawing.Size(250, 25);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
