// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class StampingConfigSelection
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing && (components != null)) {
        components.Dispose ();
      }
      base.Dispose (disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ()
    {
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel ();
      this.nullCheckBox = new System.Windows.Forms.CheckBox ();
      this.listBox = new System.Windows.Forms.ListBox ();
      this.tableLayoutPanel1.SuspendLayout ();
      this.SuspendLayout ();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle ());
      this.tableLayoutPanel1.Controls.Add (this.nullCheckBox, 0, 1);
      this.tableLayoutPanel1.Controls.Add (this.listBox, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point (0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size (262, 229);
      this.tableLayoutPanel1.TabIndex = 2;
      // 
      // nullCheckBox
      // 
      this.nullCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nullCheckBox.Location = new System.Drawing.Point (3, 202);
      this.nullCheckBox.Name = "nullCheckBox";
      this.nullCheckBox.Size = new System.Drawing.Size (336, 24);
      this.nullCheckBox.TabIndex = 2;
      this.nullCheckBox.Text = "No stamping config";
      this.nullCheckBox.UseVisualStyleBackColor = true;
      this.nullCheckBox.CheckedChanged += new System.EventHandler (this.nullCheckBox_CheckedChanged);
      // 
      // listBox
      // 
      this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listBox.Location = new System.Drawing.Point (3, 3);
      this.listBox.Name = "listBox";
      this.listBox.Size = new System.Drawing.Size (336, 193);
      this.listBox.TabIndex = 3;
      this.listBox.SelectedIndexChanged += new System.EventHandler (this.listBox_SelectedIndexChanged);
      // 
      // StampingConfigSelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add (this.tableLayoutPanel1);
      this.Name = "StampingConfigSelection";
      this.Size = new System.Drawing.Size (262, 229);
      this.Load += new System.EventHandler (this.StampingConfigSelection_Load);
      this.tableLayoutPanel1.ResumeLayout (false);
      this.ResumeLayout (false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.CheckBox nullCheckBox;
    private System.Windows.Forms.ListBox listBox;
  }
}
