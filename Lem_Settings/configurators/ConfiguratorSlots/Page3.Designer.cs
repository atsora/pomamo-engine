// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots
{
  partial class Page3
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
      this.groupBoxParameters = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.labelFrom = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.labelTo = new System.Windows.Forms.Label();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.groupBoxElements = new System.Windows.Forms.GroupBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.textMachines = new System.Windows.Forms.RichTextBox();
      this.groupBox2.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.baseLayout.SuspendLayout();
      this.groupBoxElements.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxParameters
      // 
      this.groupBoxParameters.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBoxParameters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBoxParameters.Location = new System.Drawing.Point(0, 104);
      this.groupBoxParameters.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
      this.groupBoxParameters.Name = "groupBoxParameters";
      this.groupBoxParameters.Size = new System.Drawing.Size(370, 186);
      this.groupBoxParameters.TabIndex = 6;
      this.groupBoxParameters.TabStop = false;
      this.groupBoxParameters.Text = "Parameters";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.tableLayoutPanel2);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox2.Location = new System.Drawing.Point(0, 64);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(370, 36);
      this.groupBox2.TabIndex = 8;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Period";
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 4;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.labelFrom, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.label2, 2, 0);
      this.tableLayoutPanel2.Controls.Add(this.labelTo, 3, 0);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 1;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(364, 17);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 17);
      this.label1.TabIndex = 0;
      this.label1.Text = "From";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelFrom
      // 
      this.labelFrom.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelFrom.Location = new System.Drawing.Point(57, 0);
      this.labelFrom.Name = "labelFrom";
      this.labelFrom.Size = new System.Drawing.Size(129, 17);
      this.labelFrom.TabIndex = 2;
      this.labelFrom.Text = "labelFrom";
      this.labelFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(192, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(34, 17);
      this.label2.TabIndex = 1;
      this.label2.Text = "To";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelTo
      // 
      this.labelTo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelTo.Location = new System.Drawing.Point(232, 0);
      this.labelTo.Name = "labelTo";
      this.labelTo.Size = new System.Drawing.Size(129, 17);
      this.labelTo.TabIndex = 3;
      this.labelTo.Text = "labelTo";
      this.labelTo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.groupBox2, 0, 1);
      this.baseLayout.Controls.Add(this.groupBoxParameters, 0, 2);
      this.baseLayout.Controls.Add(this.groupBoxElements, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 9;
      // 
      // groupBoxElements
      // 
      this.groupBoxElements.Controls.Add(this.panel1);
      this.groupBoxElements.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBoxElements.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBoxElements.Location = new System.Drawing.Point(0, 0);
      this.groupBoxElements.Margin = new System.Windows.Forms.Padding(0);
      this.groupBoxElements.Name = "groupBoxElements";
      this.groupBoxElements.Size = new System.Drawing.Size(370, 60);
      this.groupBoxElements.TabIndex = 9;
      this.groupBoxElements.TabStop = false;
      this.groupBoxElements.Text = "Elements";
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.textMachines);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(3, 16);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(364, 41);
      this.panel1.TabIndex = 0;
      // 
      // textMachines
      // 
      this.textMachines.BackColor = System.Drawing.SystemColors.Control;
      this.textMachines.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textMachines.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textMachines.Location = new System.Drawing.Point(0, 0);
      this.textMachines.Name = "textMachines";
      this.textMachines.ReadOnly = true;
      this.textMachines.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
      this.textMachines.Size = new System.Drawing.Size(364, 41);
      this.textMachines.TabIndex = 0;
      this.textMachines.Text = "";
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(370, 290);
      this.groupBox2.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.baseLayout.ResumeLayout(false);
      this.groupBoxElements.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RichTextBox textMachines;
    private System.Windows.Forms.GroupBox groupBoxElements;
    private System.Windows.Forms.Label labelTo;
    private System.Windows.Forms.Label labelFrom;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBoxParameters;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
