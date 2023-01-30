// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class EventLevelSelection
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
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.eventListbox = new System.Windows.Forms.ListBox();
      this.nullCheckBox = new System.Windows.Forms.CheckBox();
      this.tableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.ColumnCount = 1;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel.Controls.Add(this.eventListbox, 0, 0);
      this.tableLayoutPanel.Controls.Add(this.nullCheckBox, 0, 1);
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.RowCount = 2;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 88.96797F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.03203F));
      this.tableLayoutPanel.Size = new System.Drawing.Size(253, 281);
      this.tableLayoutPanel.TabIndex = 1;
      // 
      // eventListbox
      // 
      this.eventListbox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventListbox.FormattingEnabled = true;
      this.eventListbox.Location = new System.Drawing.Point(3, 3);
      this.eventListbox.Name = "eventListbox";
      this.eventListbox.Size = new System.Drawing.Size(247, 244);
      this.eventListbox.TabIndex = 1;
      this.eventListbox.SelectedIndexChanged += new System.EventHandler(this.ListBoxSelectedIndexChanged);
      // 
      // nullCheckBox
      // 
      this.nullCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
      this.nullCheckBox.Location = new System.Drawing.Point(3, 253);
      this.nullCheckBox.Name = "nullCheckBox";
      this.nullCheckBox.Size = new System.Drawing.Size(104, 25);
      this.nullCheckBox.TabIndex = 0;
      this.nullCheckBox.Text = "nullcheckBox";
      this.nullCheckBox.UseVisualStyleBackColor = true;
      this.nullCheckBox.CheckedChanged += new System.EventHandler(this.NullCheckBoxCheckedChanged);
      // 
      // EventLevelSelection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel);
      this.Name = "EventLevelSelection";
      this.Size = new System.Drawing.Size(253, 281);
      this.Load += new System.EventHandler(this.EventLevelSelectionLoad);
      this.tableLayoutPanel.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.CheckBox nullCheckBox;
    private System.Windows.Forms.ListBox eventListbox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
  }
}
