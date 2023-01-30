// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_MachineStateTemplateGUI
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
      this.machineStateTemplateMainPage1 = new Lem_MachineStateTemplateGUI.MachineStateTemplateMainPage();
      this.SuspendLayout();
      // 
      // machineStateTemplateMainPage1
      // 
      this.machineStateTemplateMainPage1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateMainPage1.Location = new System.Drawing.Point(0, 0);
      this.machineStateTemplateMainPage1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineStateTemplateMainPage1.Name = "machineStateTemplateMainPage1";
      this.machineStateTemplateMainPage1.Size = new System.Drawing.Size(700, 577);
      this.machineStateTemplateMainPage1.TabIndex = 0;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(700, 577);
      this.Controls.Add(this.machineStateTemplateMainPage1);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "MainForm";
      this.Text = "Lem_MachineStateTemplateGUI";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.ResumeLayout(false);

    }
    private Lem_MachineStateTemplateGUI.MachineStateTemplateMainPage machineStateTemplateMainPage1;
  }
}
