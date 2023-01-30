// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls.Inputs
{
  partial class ColorPicker
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TextBox textBox;
    private System.Windows.Forms.Button buttonChangeColor;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.Marker marker;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorPicker));
      this.textBox = new System.Windows.Forms.TextBox();
      this.buttonChangeColor = new System.Windows.Forms.Button();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.marker = new Lemoine.BaseControls.Marker();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // textBox
      // 
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.Location = new System.Drawing.Point(0, 3);
      this.textBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
      this.textBox.Name = "textBox";
      this.textBox.Size = new System.Drawing.Size(61, 20);
      this.textBox.TabIndex = 0;
      this.textBox.Validated += new System.EventHandler(this.TextBoxValidated);
      // 
      // buttonChangeColor
      // 
      this.buttonChangeColor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.buttonChangeColor.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonChangeColor.Image = ((System.Drawing.Image)(resources.GetObject("buttonChangeColor.Image")));
      this.buttonChangeColor.Location = new System.Drawing.Point(85, 0);
      this.buttonChangeColor.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.buttonChangeColor.MinimumSize = new System.Drawing.Size(18, 18);
      this.buttonChangeColor.Name = "buttonChangeColor";
      this.buttonChangeColor.Size = new System.Drawing.Size(25, 26);
      this.buttonChangeColor.TabIndex = 1;
      this.buttonChangeColor.UseVisualStyleBackColor = true;
      this.buttonChangeColor.Click += new System.EventHandler(this.ButtonChangeColorClick);
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 18F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.Controls.Add(this.buttonChangeColor, 2, 0);
      this.baseLayout.Controls.Add(this.textBox, 0, 0);
      this.baseLayout.Controls.Add(this.marker, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(110, 26);
      this.baseLayout.TabIndex = 3;
      // 
      // marker
      // 
      this.marker.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker.Location = new System.Drawing.Point(64, 3);
      this.marker.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.marker.Name = "marker";
      this.marker.Size = new System.Drawing.Size(18, 20);
      this.marker.TabIndex = 2;
      this.marker.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MarkerMouseClick);
      // 
      // ColorPicker
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.MaximumSize = new System.Drawing.Size(0, 26);
      this.MinimumSize = new System.Drawing.Size(110, 26);
      this.Name = "ColorPicker";
      this.Size = new System.Drawing.Size(110, 26);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);

    }
  }
}
