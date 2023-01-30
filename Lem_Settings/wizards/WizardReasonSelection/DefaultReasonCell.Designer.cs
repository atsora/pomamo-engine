// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardReasonSelection
{
  partial class DefaultReasonCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.CheckBox checkReason;
    private System.Windows.Forms.Label labelReasonDisplay;
    private Lemoine.BaseControls.Marker markerReasonColor;
    private System.Windows.Forms.TableLayoutPanel layoutConf;
    private System.Windows.Forms.CheckBox checkOverwriteRequired;
    private Lemoine.BaseControls.ComboboxTextValue comboboxMachineFilterInclude;
    private System.Windows.Forms.Label labelDescription;
    private Lemoine.BaseControls.ComboboxTextValue comboboxMachineFilterExclude;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.CheckBox checkMaxTime;
    
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
      this.layoutConf = new System.Windows.Forms.TableLayoutPanel();
      this.checkOverwriteRequired = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.checkMaxTime = new System.Windows.Forms.CheckBox();
      this.comboboxMachineFilterExclude = new Lemoine.BaseControls.ComboboxTextValue();
      this.comboboxMachineFilterInclude = new Lemoine.BaseControls.ComboboxTextValue();
      this.timeMax = new Lemoine.BaseControls.DurationPicker();
      this.checkReason = new System.Windows.Forms.CheckBox();
      this.markerReasonColor = new Lemoine.BaseControls.Marker();
      this.labelReasonDisplay = new System.Windows.Forms.Label();
      this.labelDescription = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.layoutConf.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.timeMax)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
      this.baseLayout.Controls.Add(this.layoutConf, 2, 1);
      this.baseLayout.Controls.Add(this.checkReason, 0, 0);
      this.baseLayout.Controls.Add(this.markerReasonColor, 1, 0);
      this.baseLayout.Controls.Add(this.labelReasonDisplay, 2, 0);
      this.baseLayout.Controls.Add(this.labelDescription, 3, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(310, 100);
      this.baseLayout.TabIndex = 1;
      // 
      // layoutConf
      // 
      this.layoutConf.ColumnCount = 3;
      this.baseLayout.SetColumnSpan(this.layoutConf, 2);
      this.layoutConf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
      this.layoutConf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
      this.layoutConf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.layoutConf.Controls.Add(this.checkOverwriteRequired, 0, 0);
      this.layoutConf.Controls.Add(this.label2, 0, 1);
      this.layoutConf.Controls.Add(this.label3, 0, 2);
      this.layoutConf.Controls.Add(this.checkMaxTime, 1, 0);
      this.layoutConf.Controls.Add(this.comboboxMachineFilterExclude, 1, 2);
      this.layoutConf.Controls.Add(this.comboboxMachineFilterInclude, 1, 1);
      this.layoutConf.Controls.Add(this.timeMax, 2, 0);
      this.layoutConf.Dock = System.Windows.Forms.DockStyle.Fill;
      this.layoutConf.Location = new System.Drawing.Point(46, 26);
      this.layoutConf.Margin = new System.Windows.Forms.Padding(0);
      this.layoutConf.Name = "layoutConf";
      this.layoutConf.RowCount = 4;
      this.layoutConf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.layoutConf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.layoutConf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.layoutConf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.layoutConf.Size = new System.Drawing.Size(264, 74);
      this.layoutConf.TabIndex = 3;
      // 
      // checkOverwriteRequired
      // 
      this.checkOverwriteRequired.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkOverwriteRequired.Location = new System.Drawing.Point(3, 0);
      this.checkOverwriteRequired.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.checkOverwriteRequired.Name = "checkOverwriteRequired";
      this.checkOverwriteRequired.Size = new System.Drawing.Size(114, 25);
      this.checkOverwriteRequired.TabIndex = 1;
      this.checkOverwriteRequired.Text = "overwrite required";
      this.checkOverwriteRequired.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 25);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(114, 22);
      this.label2.TabIndex = 6;
      this.label2.Text = "Inclusion";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(0, 47);
      this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(114, 22);
      this.label3.TabIndex = 7;
      this.label3.Text = "Exclusion";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkMaxTime
      // 
      this.checkMaxTime.Location = new System.Drawing.Point(120, 0);
      this.checkMaxTime.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.checkMaxTime.Name = "checkMaxTime";
      this.checkMaxTime.Size = new System.Drawing.Size(71, 22);
      this.checkMaxTime.TabIndex = 8;
      this.checkMaxTime.Text = "max time";
      this.checkMaxTime.UseVisualStyleBackColor = true;
      this.checkMaxTime.CheckedChanged += new System.EventHandler(this.CheckMaxTimeCheckedChanged);
      // 
      // comboboxMachineFilterExclude
      // 
      this.layoutConf.SetColumnSpan(this.comboboxMachineFilterExclude, 2);
      this.comboboxMachineFilterExclude.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboboxMachineFilterExclude.DropDownTextColor = System.Drawing.Color.Red;
      this.comboboxMachineFilterExclude.Location = new System.Drawing.Point(117, 47);
      this.comboboxMachineFilterExclude.Margin = new System.Windows.Forms.Padding(0);
      this.comboboxMachineFilterExclude.Name = "comboboxMachineFilterExclude";
      this.comboboxMachineFilterExclude.Size = new System.Drawing.Size(147, 22);
      this.comboboxMachineFilterExclude.Sorted = true;
      this.comboboxMachineFilterExclude.TabIndex = 3;
      // 
      // comboboxMachineFilterInclude
      // 
      this.layoutConf.SetColumnSpan(this.comboboxMachineFilterInclude, 2);
      this.comboboxMachineFilterInclude.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboboxMachineFilterInclude.DropDownTextColor = System.Drawing.Color.ForestGreen;
      this.comboboxMachineFilterInclude.Location = new System.Drawing.Point(117, 25);
      this.comboboxMachineFilterInclude.Margin = new System.Windows.Forms.Padding(0);
      this.comboboxMachineFilterInclude.Name = "comboboxMachineFilterInclude";
      this.comboboxMachineFilterInclude.Size = new System.Drawing.Size(147, 22);
      this.comboboxMachineFilterInclude.Sorted = true;
      this.comboboxMachineFilterInclude.TabIndex = 2;
      // 
      // timeMax
      // 
      this.timeMax.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timeMax.Duration = System.TimeSpan.Parse("00:00:00");
      this.timeMax.Location = new System.Drawing.Point(191, 0);
      this.timeMax.Margin = new System.Windows.Forms.Padding(0);
      this.timeMax.Maximum = new decimal(new int[] {
            500654079,
            20,
            0,
            0});
      this.timeMax.MinimumSize = new System.Drawing.Size(73, 0);
      this.timeMax.Name = "timeMax";
      this.timeMax.Size = new System.Drawing.Size(73, 20);
      this.timeMax.TabIndex = 9;
      this.timeMax.TimeSpanValue = System.TimeSpan.Parse("00:00:00");
      this.timeMax.WithMs = false;
      // 
      // checkReason
      // 
      this.checkReason.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkReason.Location = new System.Drawing.Point(0, 0);
      this.checkReason.Margin = new System.Windows.Forms.Padding(0);
      this.checkReason.Name = "checkReason";
      this.checkReason.Size = new System.Drawing.Size(20, 26);
      this.checkReason.TabIndex = 0;
      this.checkReason.UseVisualStyleBackColor = true;
      this.checkReason.CheckedChanged += new System.EventHandler(this.CheckReasonCheckedChanged);
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
      // labelReasonDisplay
      // 
      this.labelReasonDisplay.AutoEllipsis = true;
      this.labelReasonDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelReasonDisplay.Location = new System.Drawing.Point(49, 0);
      this.labelReasonDisplay.Name = "labelReasonDisplay";
      this.labelReasonDisplay.Size = new System.Drawing.Size(99, 26);
      this.labelReasonDisplay.TabIndex = 1;
      this.labelReasonDisplay.Text = "...";
      this.labelReasonDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelDescription
      // 
      this.labelDescription.AutoEllipsis = true;
      this.labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelDescription.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.labelDescription.Location = new System.Drawing.Point(154, 0);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(153, 26);
      this.labelDescription.TabIndex = 4;
      this.labelDescription.Text = "...";
      this.labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // DefaultReasonCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.MaximumSize = new System.Drawing.Size(0, 200);
      this.MinimumSize = new System.Drawing.Size(310, 100);
      this.Name = "DefaultReasonCell";
      this.Size = new System.Drawing.Size(310, 100);
      this.baseLayout.ResumeLayout(false);
      this.layoutConf.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.timeMax)).EndInit();
      this.ResumeLayout(false);

    }

    private Lemoine.BaseControls.DurationPicker timeMax;
  }
}
