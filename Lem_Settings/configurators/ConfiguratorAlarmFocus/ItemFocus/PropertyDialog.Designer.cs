// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarmFocus
{
  partial class PropertyDialog
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textProperty;
    private System.Windows.Forms.TextBox textValue;
    
    /// <summary>
    /// Disposes resources used by the form.
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
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.textProperty = new System.Windows.Forms.TextBox();
      this.textValue = new System.Windows.Forms.TextBox();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(96, 116);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(177, 116);
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
      | System.Windows.Forms.AnchorStyles.Left) 
      | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 56F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.textProperty, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.textValue, 1, 2);
      this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 13);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 4;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(239, 89);
      this.tableLayoutPanel1.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 20);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(50, 24);
      this.label1.TabIndex = 0;
      this.label1.Text = "Property";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 44);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(50, 24);
      this.label2.TabIndex = 1;
      this.label2.Text = "Value";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // textProperty
      // 
      this.textProperty.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textProperty.Location = new System.Drawing.Point(59, 23);
      this.textProperty.Name = "textProperty";
      this.textProperty.Size = new System.Drawing.Size(177, 20);
      this.textProperty.TabIndex = 2;
      // 
      // textValue
      // 
      this.textValue.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textValue.Location = new System.Drawing.Point(59, 47);
      this.textValue.Name = "textValue";
      this.textValue.Size = new System.Drawing.Size(177, 20);
      this.textValue.TabIndex = 3;
      // 
      // PropertyDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(264, 151);
      this.Controls.Add(this.tableLayoutPanel1);
      this.MaximumSize = new System.Drawing.Size(2000, 190);
      this.MinimumSize = new System.Drawing.Size(280, 190);
      this.Name = "PropertyDialog";
      this.Text = "Add a property to match";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }
  }
}
