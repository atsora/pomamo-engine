// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarmFocus
{
  partial class SeverityWidget
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.CheckBox checkFocus;
    private Lemoine.BaseControls.Inputs.ColorPicker colorPicker;
    private System.Windows.Forms.PictureBox pictureHelp;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SeverityWidget));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelName = new System.Windows.Forms.Label();
      this.checkFocus = new System.Windows.Forms.CheckBox();
      this.colorPicker = new Lemoine.BaseControls.Inputs.ColorPicker();
      this.pictureHelp = new System.Windows.Forms.PictureBox();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureHelp)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 31F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Controls.Add(this.labelName, 1, 0);
      this.baseLayout.Controls.Add(this.checkFocus, 2, 0);
      this.baseLayout.Controls.Add(this.colorPicker, 3, 0);
      this.baseLayout.Controls.Add(this.pictureHelp, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(310, 25);
      this.baseLayout.TabIndex = 0;
      // 
      // labelName
      // 
      this.labelName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelName.Location = new System.Drawing.Point(23, 0);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(233, 25);
      this.labelName.TabIndex = 0;
      this.labelName.Text = "Name";
      this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkFocus
      // 
      this.checkFocus.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkFocus.Checked = true;
      this.checkFocus.CheckState = System.Windows.Forms.CheckState.Indeterminate;
      this.checkFocus.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkFocus.Location = new System.Drawing.Point(259, 0);
      this.checkFocus.Margin = new System.Windows.Forms.Padding(0);
      this.checkFocus.Name = "checkFocus";
      this.checkFocus.Size = new System.Drawing.Size(31, 25);
      this.checkFocus.TabIndex = 1;
      this.checkFocus.ThreeState = true;
      this.checkFocus.UseVisualStyleBackColor = true;
      this.checkFocus.CheckStateChanged += new System.EventHandler(this.CheckFocusCheckStateChanged);
      // 
      // colorPicker
      // 
      this.colorPicker.SelectedColor = System.Drawing.Color.Transparent;
      this.colorPicker.Compact = true;
      this.colorPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.colorPicker.Location = new System.Drawing.Point(290, 0);
      this.colorPicker.Margin = new System.Windows.Forms.Padding(0);
      this.colorPicker.MaximumSize = new System.Drawing.Size(0, 26);
      this.colorPicker.Name = "colorPicker";
      this.colorPicker.NullColorPossible = true;
      this.colorPicker.Size = new System.Drawing.Size(20, 25);
      this.colorPicker.TabIndex = 2;
      this.colorPicker.ColorChanged += new System.Action<System.Nullable<System.Drawing.Color>>(this.ColorPickerColorChanged);
      // 
      // pictureHelp
      // 
      this.pictureHelp.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureHelp.Image = ((System.Drawing.Image)(resources.GetObject("pictureHelp.Image")));
      this.pictureHelp.InitialImage = null;
      this.pictureHelp.Location = new System.Drawing.Point(0, 0);
      this.pictureHelp.Margin = new System.Windows.Forms.Padding(0);
      this.pictureHelp.Name = "pictureHelp";
      this.pictureHelp.Size = new System.Drawing.Size(20, 25);
      this.pictureHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.pictureHelp.TabIndex = 3;
      this.pictureHelp.TabStop = false;
      // 
      // SeverityWidget
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "SeverityWidget";
      this.Size = new System.Drawing.Size(310, 25);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureHelp)).EndInit();
      this.ResumeLayout(false);

    }
  }
}
