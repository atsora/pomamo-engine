// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class TreeItem
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeItem));
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.labelNone = new System.Windows.Forms.Label();
      this.panelBase = new System.Windows.Forms.Panel();
      this.verticalScrollLayout = new Lemoine.BaseControls.VerticalScrollLayout();
      this.panelBase.SuspendLayout();
      this.SuspendLayout();
      // 
      // imageList
      // 
      this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
      this.imageList.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList.Images.SetKeyName(0, "configurator.png");
      this.imageList.Images.SetKeyName(1, "internet.png");
      this.imageList.Images.SetKeyName(2, "launcher.png");
      this.imageList.Images.SetKeyName(3, "view.png");
      this.imageList.Images.SetKeyName(4, "wizard.png");
      // 
      // labelNone
      // 
      this.labelNone.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelNone.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelNone.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.labelNone.Location = new System.Drawing.Point(0, 0);
      this.labelNone.Name = "labelNone";
      this.labelNone.Size = new System.Drawing.Size(96, 156);
      this.labelNone.TabIndex = 1;
      this.labelNone.Text = "none";
      this.labelNone.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // panelBase
      // 
      this.panelBase.BackColor = System.Drawing.SystemColors.Window;
      this.panelBase.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panelBase.Controls.Add(this.verticalScrollLayout);
      this.panelBase.Controls.Add(this.labelNone);
      this.panelBase.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelBase.Location = new System.Drawing.Point(0, 0);
      this.panelBase.Margin = new System.Windows.Forms.Padding(0);
      this.panelBase.Name = "panelBase";
      this.panelBase.Size = new System.Drawing.Size(100, 160);
      this.panelBase.TabIndex = 3;
      // 
      // verticalScrollLayout
      // 
      this.verticalScrollLayout.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScrollLayout.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLayout.Location = new System.Drawing.Point(0, 0);
      this.verticalScrollLayout.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Name = "verticalScrollLayout";
      this.verticalScrollLayout.Size = new System.Drawing.Size(96, 156);
      this.verticalScrollLayout.TabIndex = 3;
      this.verticalScrollLayout.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLayout.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // TreeItem
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.panelBase);
      this.Name = "TreeItem";
      this.Size = new System.Drawing.Size(100, 160);
      this.panelBase.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.ImageList imageList;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLayout;
    private System.Windows.Forms.Label labelNone;
    private System.Windows.Forms.Panel panelBase;
  }
}
