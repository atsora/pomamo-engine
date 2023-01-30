// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class MachineDialog
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
      this.displayableTreeView = new Lemoine.DataReferenceControls.DisplayableTreeView();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(56, 197);
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point(137, 197);
      this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
      // 
      // displayableTreeView
      // 
      this.displayableTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
      | System.Windows.Forms.AnchorStyles.Left) 
      | System.Windows.Forms.AnchorStyles.Right)));
      this.displayableTreeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.displayableTreeView.Location = new System.Drawing.Point(12, 12);
      this.displayableTreeView.Name = "displayableTreeView";
      this.displayableTreeView.Size = new System.Drawing.Size(200, 179);
      this.displayableTreeView.TabIndex = 3;
      this.displayableTreeView.SelectionChanged += new System.Action(this.DisplayableTreeViewSelectionChanged);
      // 
      // MachineDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(224, 232);
      this.Controls.Add(this.displayableTreeView);
      this.MinimumSize = new System.Drawing.Size(240, 270);
      this.Name = "MachineDialog";
      this.Text = "Select a machine";
      this.Load += new System.EventHandler(this.MachineDialogLoad);
      this.Controls.SetChildIndex(this.cancelButton, 0);
      this.Controls.SetChildIndex(this.okButton, 0);
      this.Controls.SetChildIndex(this.displayableTreeView, 0);
      this.ResumeLayout(false);

    }
    private Lemoine.DataReferenceControls.DisplayableTreeView displayableTreeView;
  }
}
