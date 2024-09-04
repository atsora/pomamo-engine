// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots.MachineState
{
  partial class MachineStatePage3
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
      this.checkShift = new System.Windows.Forms.CheckBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.checkForceRebuild = new System.Windows.Forms.CheckBox();
      this.label5 = new System.Windows.Forms.Label();
      this.listState = new Lemoine.BaseControls.List.ListTextValue();
      this.comboShift = new Lemoine.BaseControls.ComboboxTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.checkShift, 1, 2);
      this.baseLayout.Controls.Add(this.label3, 0, 0);
      this.baseLayout.Controls.Add(this.label4, 0, 2);
      this.baseLayout.Controls.Add(this.checkForceRebuild, 1, 3);
      this.baseLayout.Controls.Add(this.label5, 2, 3);
      this.baseLayout.Controls.Add(this.listState, 2, 0);
      this.baseLayout.Controls.Add(this.comboShift, 2, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.Size = new System.Drawing.Size(268, 156);
      this.baseLayout.TabIndex = 1;
      // 
      // checkShift
      // 
      this.checkShift.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkShift.Location = new System.Drawing.Point(73, 105);
      this.checkShift.Name = "checkShift";
      this.checkShift.Size = new System.Drawing.Size(14, 21);
      this.checkShift.TabIndex = 0;
      this.checkShift.UseVisualStyleBackColor = true;
      this.checkShift.CheckedChanged += new System.EventHandler(this.CheckShiftCheckedChanged);
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(64, 27);
      this.label3.TabIndex = 1;
      this.label3.Text = "State";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(3, 102);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 27);
      this.label4.TabIndex = 2;
      this.label4.Text = "Shift";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkForceRebuild
      // 
      this.checkForceRebuild.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkForceRebuild.Location = new System.Drawing.Point(73, 132);
      this.checkForceRebuild.Name = "checkForceRebuild";
      this.checkForceRebuild.Size = new System.Drawing.Size(14, 21);
      this.checkForceRebuild.TabIndex = 5;
      this.checkForceRebuild.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(93, 129);
      this.label5.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(172, 24);
      this.label5.TabIndex = 6;
      this.label5.Text = "Force re-build state templates";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // listState
      // 
      this.listState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listState.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listState.Location = new System.Drawing.Point(93, 0);
      this.listState.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.listState.Name = "listState";
      this.baseLayout.SetRowSpan(this.listState, 2);
      this.listState.Size = new System.Drawing.Size(172, 99);
      this.listState.Sorted = true;
      this.listState.TabIndex = 7;
      // 
      // comboShift
      // 
      this.comboShift.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboShift.Enabled = false;
      this.comboShift.Location = new System.Drawing.Point(93, 105);
      this.comboShift.Name = "comboShift";
      this.comboShift.Size = new System.Drawing.Size(172, 21);
      this.comboShift.TabIndex = 8;
      // 
      // MachineStatePage3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "MachineStatePage3";
      this.Size = new System.Drawing.Size(268, 156);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.CheckBox checkForceRebuild;
    private Lemoine.BaseControls.ComboboxTextValue comboShift;
    private Lemoine.BaseControls.List.ListTextValue listState;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.CheckBox checkShift;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
