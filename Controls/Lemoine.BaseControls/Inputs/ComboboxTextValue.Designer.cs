// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class ComboboxTextValue
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
      this.comboBox = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // comboBox
      // 
      this.comboBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox.FormattingEnabled = true;
      this.comboBox.Location = new System.Drawing.Point(0, 0);
      this.comboBox.Margin = new System.Windows.Forms.Padding(0);
      this.comboBox.Name = "comboBox";
      this.comboBox.Size = new System.Drawing.Size(150, 21);
      this.comboBox.TabIndex = 0;
      this.comboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBoxDrawItem);
      this.comboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSelectedIndexChanged);
      this.comboBox.TextUpdate += new System.EventHandler(this.ComboBoxTextUpdate);
      // 
      // ComboboxTextValue
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.comboBox);
      this.Name = "ComboboxTextValue";
      this.Size = new System.Drawing.Size(150, 22);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.ComboBox comboBox;
  }
}
