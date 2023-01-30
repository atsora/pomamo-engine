// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateProduction
{
  partial class PageAttributes
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
      this.label2 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.numericQuantity = new System.Windows.Forms.NumericUpDown();
      this.labelDescription = new System.Windows.Forms.RichTextBox();
      this.comboName = new Lemoine.BaseControls.ComboboxTextValue();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericQuantity)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 148F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label2, 0, 1);
      this.baseLayout.Controls.Add(this.label5, 0, 3);
      this.baseLayout.Controls.Add(this.numericQuantity, 1, 1);
      this.baseLayout.Controls.Add(this.labelDescription, 0, 4);
      this.baseLayout.Controls.Add(this.comboName, 1, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 9);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(145, 20);
      this.label2.TabIndex = 1;
      this.label2.Text = "Number of parts to produce";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(0, 49);
      this.label5.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(145, 23);
      this.label5.TabIndex = 10;
      this.label5.Text = "Production name";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // numericQuantity
      // 
      this.numericQuantity.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericQuantity.Increment = new decimal(new int[] {
      100,
      0,
      0,
      0});
      this.numericQuantity.Location = new System.Drawing.Point(148, 9);
      this.numericQuantity.Margin = new System.Windows.Forms.Padding(0);
      this.numericQuantity.Maximum = new decimal(new int[] {
      1000000,
      0,
      0,
      0});
      this.numericQuantity.Minimum = new decimal(new int[] {
      1,
      0,
      0,
      0});
      this.numericQuantity.Name = "numericQuantity";
      this.numericQuantity.Size = new System.Drawing.Size(202, 20);
      this.numericQuantity.TabIndex = 1;
      this.numericQuantity.Value = new decimal(new int[] {
      1000,
      0,
      0,
      0});
      // 
      // labelDescription
      // 
      this.labelDescription.BackColor = System.Drawing.SystemColors.Control;
      this.labelDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.baseLayout.SetColumnSpan(this.labelDescription, 2);
      this.labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDescription.ForeColor = System.Drawing.Color.ForestGreen;
      this.labelDescription.Location = new System.Drawing.Point(15, 87);
      this.labelDescription.Margin = new System.Windows.Forms.Padding(15, 15, 15, 3);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.ReadOnly = true;
      this.labelDescription.Size = new System.Drawing.Size(320, 151);
      this.labelDescription.TabIndex = 14;
      this.labelDescription.Text = "";
      // 
      // comboName
      // 
      this.comboName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboName.Editable = true;
      this.comboName.Location = new System.Drawing.Point(148, 49);
      this.comboName.Margin = new System.Windows.Forms.Padding(0);
      this.comboName.Name = "comboName";
      this.comboName.SelectedIndex = -1;
      this.comboName.SelectedText = "";
      this.comboName.SelectedValue = null;
      this.comboName.Size = new System.Drawing.Size(202, 23);
      this.comboName.TabIndex = 15;
      // 
      // PageAttributes
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PageAttributes";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericQuantity)).EndInit();
      this.ResumeLayout(false);

    }
    private Lemoine.BaseControls.ComboboxTextValue comboName;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.NumericUpDown numericQuantity;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.RichTextBox labelDescription;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
