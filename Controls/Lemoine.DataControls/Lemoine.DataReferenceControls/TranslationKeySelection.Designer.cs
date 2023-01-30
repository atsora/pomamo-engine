// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class TranslationKeySelection
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
    	this.dataGridView = new System.Windows.Forms.DataGridView();
    	this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
    	this.nullCheckBox = new System.Windows.Forms.CheckBox();
    	((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
    	this.tableLayoutPanel1.SuspendLayout();
    	this.SuspendLayout();
    	// 
    	// dataGridView
    	// 
    	this.dataGridView.AllowUserToAddRows = false;
    	this.dataGridView.AllowUserToDeleteRows = false;
    	this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
    	this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
    	this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
    	this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.dataGridView.Location = new System.Drawing.Point(3, 3);
    	this.dataGridView.MultiSelect = false;
    	this.dataGridView.Name = "dataGridView";
    	this.dataGridView.ReadOnly = true;
    	this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
    	this.dataGridView.Size = new System.Drawing.Size(336, 198);
    	this.dataGridView.TabIndex = 0;
    	// 
    	// tableLayoutPanel1
    	// 
    	this.tableLayoutPanel1.ColumnCount = 1;
    	this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
    	this.tableLayoutPanel1.Controls.Add(this.nullCheckBox, 0, 1);
    	this.tableLayoutPanel1.Controls.Add(this.dataGridView, 0, 0);
    	this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
    	this.tableLayoutPanel1.Name = "tableLayoutPanel1";
    	this.tableLayoutPanel1.RowCount = 2;
    	this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
    	this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
    	this.tableLayoutPanel1.Size = new System.Drawing.Size(342, 234);
    	this.tableLayoutPanel1.TabIndex = 1;
    	// 
    	// nullCheckBox
    	// 
    	this.nullCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.nullCheckBox.Location = new System.Drawing.Point(3, 207);
    	this.nullCheckBox.Name = "nullCheckBox";
    	this.nullCheckBox.Size = new System.Drawing.Size(336, 24);
    	this.nullCheckBox.TabIndex = 2;
    	this.nullCheckBox.Text = "No machine observation state";
    	this.nullCheckBox.UseVisualStyleBackColor = true;
    	this.nullCheckBox.CheckedChanged += new System.EventHandler(this.NullCheckBoxCheckedChanged);
    	// 
    	// TranslationKeySelection
    	// 
    	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.Controls.Add(this.tableLayoutPanel1);
    	this.Name = "TranslationKeySelection";
    	this.Size = new System.Drawing.Size(342, 234);
    	this.Load += new System.EventHandler(this.TranslationKeySelectionLoad);
    	((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
    	this.tableLayoutPanel1.ResumeLayout(false);
    	this.ResumeLayout(false);
    }
    private System.Windows.Forms.CheckBox nullCheckBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.DataGridView dataGridView;
  }
}
