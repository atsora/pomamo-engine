// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardReasonSelection
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
    private System.Windows.Forms.CheckBox checkDetailsRequired;
    private Lemoine.BaseControls.ComboboxTextValue comboboxMachineFilter;
    private System.Windows.Forms.CheckBox checkReason;
    private System.Windows.Forms.TableLayoutPanel layoutConf;
    private System.Windows.Forms.Label labelDescription;
    private System.Windows.Forms.Label label1;
    
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
      this.checkReason = new System.Windows.Forms.CheckBox();
      this.labelReasonDisplay = new System.Windows.Forms.Label();
      this.markerReasonColor = new Lemoine.BaseControls.Marker();
      this.layoutConf = new System.Windows.Forms.TableLayoutPanel();
      this.comboboxMachineFilter = new Lemoine.BaseControls.ComboboxTextValue();
      this.label1 = new System.Windows.Forms.Label();
      this.checkDetailsRequired = new System.Windows.Forms.CheckBox();
      this.labelDescription = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.layoutConf.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
      this.baseLayout.Controls.Add(this.checkReason, 0, 0);
      this.baseLayout.Controls.Add(this.labelReasonDisplay, 2, 0);
      this.baseLayout.Controls.Add(this.markerReasonColor, 1, 0);
      this.baseLayout.Controls.Add(this.layoutConf, 2, 1);
      this.baseLayout.Controls.Add(this.labelDescription, 3, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(200, 75);
      this.baseLayout.TabIndex = 0;
      // 
      // checkReason
      // 
      this.checkReason.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkReason.Location = new System.Drawing.Point(0, 0);
      this.checkReason.Margin = new System.Windows.Forms.Padding(0);
      this.checkReason.Name = "checkReason";
      this.checkReason.Size = new System.Drawing.Size(20, 26);
      this.checkReason.TabIndex = 0;
      this.checkReason.Text = "...";
      this.checkReason.UseVisualStyleBackColor = true;
      this.checkReason.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
      // 
      // labelReasonDisplay
      // 
      this.labelReasonDisplay.AutoEllipsis = true;
      this.labelReasonDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelReasonDisplay.Location = new System.Drawing.Point(49, 0);
      this.labelReasonDisplay.Name = "labelReasonDisplay";
      this.labelReasonDisplay.Size = new System.Drawing.Size(55, 26);
      this.labelReasonDisplay.TabIndex = 1;
      this.labelReasonDisplay.Text = "...";
      this.labelReasonDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // markerReasonColor
      // 
      this.markerReasonColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.markerReasonColor.Dock = System.Windows.Forms.DockStyle.Fill;
      this.markerReasonColor.Location = new System.Drawing.Point(20, 0);
      this.markerReasonColor.Margin = new System.Windows.Forms.Padding(0);
      this.markerReasonColor.Name = "markerReasonColor";
      this.markerReasonColor.Size = new System.Drawing.Size(26, 26);
      this.markerReasonColor.TabIndex = 2;
      // 
      // layoutConf
      // 
      this.layoutConf.ColumnCount = 2;
      this.baseLayout.SetColumnSpan(this.layoutConf, 2);
      this.layoutConf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
      this.layoutConf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.layoutConf.Controls.Add(this.comboboxMachineFilter, 1, 1);
      this.layoutConf.Controls.Add(this.label1, 0, 1);
      this.layoutConf.Controls.Add(this.checkDetailsRequired, 0, 0);
      this.layoutConf.Dock = System.Windows.Forms.DockStyle.Fill;
      this.layoutConf.Location = new System.Drawing.Point(46, 26);
      this.layoutConf.Margin = new System.Windows.Forms.Padding(0);
      this.layoutConf.Name = "layoutConf";
      this.layoutConf.RowCount = 3;
      this.baseLayout.SetRowSpan(this.layoutConf, 2);
      this.layoutConf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.layoutConf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.layoutConf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.layoutConf.Size = new System.Drawing.Size(154, 49);
      this.layoutConf.TabIndex = 3;
      // 
      // comboboxMachineFilter
      // 
      this.comboboxMachineFilter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboboxMachineFilter.DropDownTextColor = System.Drawing.Color.ForestGreen;
      this.comboboxMachineFilter.Location = new System.Drawing.Point(76, 22);
      this.comboboxMachineFilter.Margin = new System.Windows.Forms.Padding(0);
      this.comboboxMachineFilter.Name = "comboboxMachineFilter";
      this.comboboxMachineFilter.Size = new System.Drawing.Size(78, 22);
      this.comboboxMachineFilter.Sorted = true;
      this.comboboxMachineFilter.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(70, 22);
      this.label1.TabIndex = 3;
      this.label1.Text = "Inclusion";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkDetailsRequired
      // 
      this.layoutConf.SetColumnSpan(this.checkDetailsRequired, 2);
      this.checkDetailsRequired.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkDetailsRequired.Location = new System.Drawing.Point(0, 0);
      this.checkDetailsRequired.Margin = new System.Windows.Forms.Padding(0);
      this.checkDetailsRequired.Name = "checkDetailsRequired";
      this.checkDetailsRequired.Size = new System.Drawing.Size(154, 22);
      this.checkDetailsRequired.TabIndex = 1;
      this.checkDetailsRequired.Text = "details required";
      this.checkDetailsRequired.UseVisualStyleBackColor = true;
      // 
      // labelDescription
      // 
      this.labelDescription.AutoEllipsis = true;
      this.labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelDescription.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.labelDescription.Location = new System.Drawing.Point(110, 0);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(87, 26);
      this.labelDescription.TabIndex = 4;
      this.labelDescription.Text = "...";
      this.labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // ReasonCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.MinimumSize = new System.Drawing.Size(200, 75);
      this.Name = "ReasonCell";
      this.Size = new System.Drawing.Size(200, 75);
      this.baseLayout.ResumeLayout(false);
      this.layoutConf.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
