// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots.UserMachine
{
  partial class UserMachinePage2
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private Lemoine.BaseControls.List.ListTextValue listUsers;
    
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
      this.listUsers = new Lemoine.BaseControls.List.ListTextValue();
      this.SuspendLayout();
      // 
      // listUsers
      // 
      this.listUsers.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listUsers.Location = new System.Drawing.Point(0, 0);
      this.listUsers.Margin = new System.Windows.Forms.Padding(0);
      this.listUsers.MultipleSelection = true;
      this.listUsers.Name = "listUsers";
      this.listUsers.Size = new System.Drawing.Size(150, 150);
      this.listUsers.Sorted = true;
      this.listUsers.TabIndex = 1;
      // 
      // UserMachinePage2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listUsers);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "UserMachinePage2";
      this.ResumeLayout(false);

    }
  }
}
