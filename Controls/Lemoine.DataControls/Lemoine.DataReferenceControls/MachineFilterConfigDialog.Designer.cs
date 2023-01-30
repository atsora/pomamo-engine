// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class MachineFilterConfigDialog
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
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
      this.machineFilterConfigSelection = new Lemoine.DataReferenceControls.MachineFilterConfigSelection();
      this.itemRuleComboBox = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(102, 233);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(183, 233);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // machineFilterConfigSelection
      // 
      this.machineFilterConfigSelection.Location = new System.Drawing.Point(12, 12);
      this.machineFilterConfigSelection.Name = "machineFilterConfigSelection";
      this.machineFilterConfigSelection.Size = new System.Drawing.Size(246, 185);
      this.machineFilterConfigSelection.TabIndex = 2;
      // 
      // itemRuleComboBox
      // 
      this.itemRuleComboBox.FormattingEnabled = true;
      this.itemRuleComboBox.Location = new System.Drawing.Point(115, 203);
      this.itemRuleComboBox.Name = "itemRuleComboBox";
      this.itemRuleComboBox.Size = new System.Drawing.Size(143, 21);
      this.itemRuleComboBox.TabIndex = 3;
      // 
      // MachineFilterConfigDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(270, 268);
      this.Controls.Add(this.itemRuleComboBox);
      this.Controls.Add(this.machineFilterConfigSelection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "MachineFilterConfigDialog";
      this.Text = "MachineFilterConfigDialog";
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.machineFilterConfigSelection, 0);
      this.Controls.SetChildIndex(this.itemRuleComboBox, 0);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.ComboBox itemRuleComboBox;
    private Lemoine.DataReferenceControls.MachineFilterConfigSelection machineFilterConfigSelection;
  }
}
