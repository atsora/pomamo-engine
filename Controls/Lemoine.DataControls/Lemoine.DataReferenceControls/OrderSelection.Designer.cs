// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class OrderSelection
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
      this.defaultInsertCheckBox = new System.Windows.Forms.CheckBox();
      this.specificInsertCheckBox = new System.Windows.Forms.CheckBox();
      this.indexNumericUpDown = new System.Windows.Forms.NumericUpDown();
      ((System.ComponentModel.ISupportInitialize)(this.indexNumericUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // defaultInsertCheckBox
      // 
      this.defaultInsertCheckBox.Location = new System.Drawing.Point(3, 3);
      this.defaultInsertCheckBox.Name = "defaultInsertCheckBox";
      this.defaultInsertCheckBox.Size = new System.Drawing.Size(104, 24);
      this.defaultInsertCheckBox.TabIndex = 0;
      this.defaultInsertCheckBox.Text = "defaultInsertCheckBox";
      this.defaultInsertCheckBox.UseVisualStyleBackColor = true;
      this.defaultInsertCheckBox.CheckedChanged += new System.EventHandler(this.DefaultInsertCheckBoxCheckedChanged);
      // 
      // specificInsertCheckBox
      // 
      this.specificInsertCheckBox.Location = new System.Drawing.Point(3, 33);
      this.specificInsertCheckBox.Name = "specificInsertCheckBox";
      this.specificInsertCheckBox.Size = new System.Drawing.Size(104, 24);
      this.specificInsertCheckBox.TabIndex = 1;
      this.specificInsertCheckBox.Text = "specificInsertCheckBox";
      this.specificInsertCheckBox.UseVisualStyleBackColor = true;
      this.specificInsertCheckBox.CheckedChanged += new System.EventHandler(this.SpecificInsertCheckBoxCheckedChanged);
      // 
      // indexNumericUpDown
      // 
      this.indexNumericUpDown.Location = new System.Drawing.Point(36, 63);
      this.indexNumericUpDown.Name = "indexNumericUpDown";
      this.indexNumericUpDown.Size = new System.Drawing.Size(71, 20);
      this.indexNumericUpDown.TabIndex = 2;
      this.indexNumericUpDown.ValueChanged += new System.EventHandler(this.IndexNumericUpDownValueChanged);
      // 
      // OrderSelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.indexNumericUpDown);
      this.Controls.Add(this.specificInsertCheckBox);
      this.Controls.Add(this.defaultInsertCheckBox);
      this.Name = "OrderSelection";
      this.Size = new System.Drawing.Size(128, 100);
      this.Load += new System.EventHandler(this.OrderSelectionLoad);
      ((System.ComponentModel.ISupportInitialize)(this.indexNumericUpDown)).EndInit();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.NumericUpDown indexNumericUpDown;
    private System.Windows.Forms.CheckBox specificInsertCheckBox;
    private System.Windows.Forms.CheckBox defaultInsertCheckBox;
  }
}
