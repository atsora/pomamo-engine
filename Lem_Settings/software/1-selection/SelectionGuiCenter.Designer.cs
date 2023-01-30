using System.Diagnostics;

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class SelectionGuiCenter
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectionGuiCenter));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.buttonOk = new System.Windows.Forms.Button();
      this.panelTwoComponents = new System.Windows.Forms.Panel();
      this.verticalScrollLayout = new Lemoine.BaseControls.VerticalScrollLayout();
      this.labelMessage = new System.Windows.Forms.Label();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.baseLayout.SuspendLayout();
      this.panelTwoComponents.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.buttonOk, 1, 0);
      this.baseLayout.Controls.Add(this.panelTwoComponents, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(460, 400);
      this.baseLayout.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(419, 30);
      this.label1.TabIndex = 0;
      this.label1.Text = "Available items";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonOk
      // 
      this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOk.Enabled = false;
      this.buttonOk.Image = ((System.Drawing.Image)(resources.GetObject("buttonOk.Image")));
      this.buttonOk.Location = new System.Drawing.Point(428, 3);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(29, 24);
      this.buttonOk.TabIndex = 2;
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
      // 
      // panelTwoComponents
      // 
      this.panelTwoComponents.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.baseLayout.SetColumnSpan(this.panelTwoComponents, 2);
      this.panelTwoComponents.Controls.Add(this.verticalScrollLayout);
      this.panelTwoComponents.Controls.Add(this.labelMessage);
      this.panelTwoComponents.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelTwoComponents.Location = new System.Drawing.Point(3, 30);
      this.panelTwoComponents.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.panelTwoComponents.Name = "panelTwoComponents";
      this.panelTwoComponents.Size = new System.Drawing.Size(454, 367);
      this.panelTwoComponents.TabIndex = 3;
      // 
      // verticalScrollLayout
      // 
      this.verticalScrollLayout.BackColor = System.Drawing.SystemColors.Window;
      this.verticalScrollLayout.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScrollLayout.Location = new System.Drawing.Point(0, 0);
      this.verticalScrollLayout.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScrollLayout.MouseWheelStep = 30;
      this.verticalScrollLayout.Name = "verticalScrollLayout";
      this.verticalScrollLayout.Size = new System.Drawing.Size(450, 363);
      this.verticalScrollLayout.TabIndex = 6;
      this.verticalScrollLayout.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScrollLayout.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // labelMessage
      // 
      this.labelMessage.BackColor = System.Drawing.SystemColors.Window;
      this.labelMessage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelMessage.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.labelMessage.Location = new System.Drawing.Point(0, 0);
      this.labelMessage.Margin = new System.Windows.Forms.Padding(0);
      this.labelMessage.Name = "labelMessage";
      this.labelMessage.Size = new System.Drawing.Size(450, 363);
      this.labelMessage.TabIndex = 4;
      this.labelMessage.Text = "message";
      this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
      this.imageList.Images.SetKeyName(5, "star.png");
      // 
      // GuiCenter1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "GuiCenter1";
      this.Size = new System.Drawing.Size(460, 400);
      this.baseLayout.ResumeLayout(false);
      this.panelTwoComponents.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.ImageList imageList;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScrollLayout;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelMessage;
    private System.Windows.Forms.Panel panelTwoComponents;
  }
}
