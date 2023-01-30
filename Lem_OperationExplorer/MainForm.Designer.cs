// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_OperationExplorer
{
  partial class MainForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.operationTreeView = new Lemoine.JobControls.OperationTreeView();
      this.informationControl = new Lemoine.JobControls.InformationControl();
      this.SuspendLayout();
      // 
      // operationTreeView
      // 
      this.operationTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                  | System.Windows.Forms.AnchorStyles.Left) 
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.operationTreeView.Location = new System.Drawing.Point(0, 0);
      this.operationTreeView.Name = "operationTreeView";
      this.operationTreeView.OrphansIsVisible = true;
      this.operationTreeView.Size = new System.Drawing.Size(356, 777);
      this.operationTreeView.TabIndex = 0;
      // 
      // informationControl
      // 
      this.informationControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.informationControl.BackColor = System.Drawing.SystemColors.Control;
      this.informationControl.Location = new System.Drawing.Point(362, 0);
      this.informationControl.Name = "informationControl";
      this.informationControl.Size = new System.Drawing.Size(462, 765);
      this.informationControl.TabIndex = 1;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(836, 777);
      this.Controls.Add(this.informationControl);
      this.Controls.Add(this.operationTreeView);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Lem_OperationExplorer";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.Load += new System.EventHandler(this.MainFormLoad);
      this.ResumeLayout(false);

    }
    private Lemoine.JobControls.InformationControl informationControl;
    private Lemoine.JobControls.OperationTreeView operationTreeView;
  }
}
