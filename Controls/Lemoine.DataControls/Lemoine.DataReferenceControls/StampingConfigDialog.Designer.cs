// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.DataReferenceControls
{
  partial class StampingConfigDialog
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Disposes resources used by the control.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing) {
        if (components != null) {
          components.Dispose ();
        }
      }
      base.Dispose (disposing);
    }

    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent ()
    {
      this.stampingConfigSelection = new Lemoine.DataReferenceControls.StampingConfigSelection ();
      this.SuspendLayout ();
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point (126, 237);
      // 
      // cancelButton
      // 
      this.cancelButton.Location = new System.Drawing.Point (207, 237);
      // 
      // stampingConfigSelection
      // 
      this.stampingConfigSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.stampingConfigSelection.DisplayedProperty = "SelectionText";
      this.stampingConfigSelection.Location = new System.Drawing.Point (0, 0);
      this.stampingConfigSelection.Name = "stampingConfigSelection";
      this.stampingConfigSelection.SelectedStampingConfig = null;
      this.stampingConfigSelection.Size = new System.Drawing.Size (288, 235);
      this.stampingConfigSelection.TabIndex = 3;
      this.stampingConfigSelection.AfterSelect += stampingConfigSelection_AfterSelect;
      // 
      // StampingConfigDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size (294, 272);
      this.Controls.Add (this.stampingConfigSelection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "StampingConfigDialog";
      this.Text = "StampingConfigDialog";
      this.Load += new System.EventHandler (this.StampingConfigDialogLoad);
      this.Controls.SetChildIndex (this.cancelButton, 0);
      this.Controls.SetChildIndex (this.okButton, 0);
      this.Controls.SetChildIndex (this.stampingConfigSelection, 0);
      this.ResumeLayout (false);
    }
    private Lemoine.DataReferenceControls.StampingConfigSelection stampingConfigSelection;
  }
}
