// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ViewReasons
{
  partial class Page1
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
      this.verticalReasons = new Lemoine.BaseControls.VerticalScrollLayout();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.machineModeSelection = new Lemoine.DataReferenceControls.MachineModeSelection();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
      this.baseLayout.Controls.Add(this.verticalReasons, 1, 1);
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 1, 0);
      this.baseLayout.Controls.Add(this.machineModeSelection, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 4;
      // 
      // verticalReasons
      // 
      this.verticalReasons.BackColor = System.Drawing.Color.White;
      this.verticalReasons.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.verticalReasons.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalReasons.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalReasons.Location = new System.Drawing.Point(148, 15);
      this.verticalReasons.Margin = new System.Windows.Forms.Padding(0);
      this.verticalReasons.Name = "verticalReasons";
      this.verticalReasons.Size = new System.Drawing.Size(222, 275);
      this.verticalReasons.TabIndex = 0;
      this.verticalReasons.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalReasons.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(148, 15);
      this.label1.TabIndex = 5;
      this.label1.Text = "Machine modes";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(148, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(222, 15);
      this.label2.TabIndex = 6;
      this.label2.Text = "Reasons by planned state";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // machineModeSelection
      // 
      this.machineModeSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.machineModeSelection.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModeSelection.Location = new System.Drawing.Point(0, 15);
      this.machineModeSelection.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.machineModeSelection.Name = "machineModeSelection";
      this.machineModeSelection.Size = new System.Drawing.Size(145, 275);
      this.machineModeSelection.TabIndex = 9;
      this.machineModeSelection.AfterSelect += new System.EventHandler(this.MachineModeSelectionAfterSelect);
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.VerticalScrollLayout verticalReasons;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private Lemoine.DataReferenceControls.MachineModeSelection machineModeSelection;
  }
}
