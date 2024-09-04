// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class GuiCenter
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GuiCenter));
      this.centralHeader = new Lem_Settings.Gui.Shared_controls.CentralHeader();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelTitle = new System.Windows.Forms.Label();
      this.panel = new System.Windows.Forms.Panel();
      this.overlayDisable = new Lemoine.BaseControls.TransparentLabel();
      this.innerTable = new System.Windows.Forms.TableLayoutPanel();
      this.buttonHome = new System.Windows.Forms.Button();
      this.buttonBack = new System.Windows.Forms.Button();
      this.buttonNext = new System.Windows.Forms.Button();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.baseLayout.SuspendLayout();
      this.panel.SuspendLayout();
      this.innerTable.SuspendLayout();
      this.SuspendLayout();
      // 
      // centralHeader
      // 
      this.centralHeader.Dock = System.Windows.Forms.DockStyle.Fill;
      this.centralHeader.HasMessage = false;
      this.centralHeader.Location = new System.Drawing.Point(0, 0);
      this.centralHeader.Margin = new System.Windows.Forms.Padding(0);
      this.centralHeader.Name = "centralHeader";
      this.centralHeader.Size = new System.Drawing.Size(450, 20);
      this.centralHeader.TabIndex = 0;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
      this.baseLayout.Controls.Add(this.labelTitle, 1, 0);
      this.baseLayout.Controls.Add(this.panel, 0, 1);
      this.baseLayout.Controls.Add(this.buttonHome, 0, 0);
      this.baseLayout.Controls.Add(this.buttonBack, 2, 0);
      this.baseLayout.Controls.Add(this.buttonNext, 3, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(460, 400);
      this.baseLayout.TabIndex = 1;
      // 
      // labelTitle
      // 
      this.labelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelTitle.Location = new System.Drawing.Point(38, 0);
      this.labelTitle.Name = "labelTitle";
      this.labelTitle.Size = new System.Drawing.Size(349, 30);
      this.labelTitle.TabIndex = 0;
      this.labelTitle.Text = "Item title";
      this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // panel
      // 
      this.panel.AutoScroll = true;
      this.panel.BackColor = System.Drawing.SystemColors.Control;
      this.panel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.baseLayout.SetColumnSpan(this.panel, 4);
      this.panel.Controls.Add(this.overlayDisable);
      this.panel.Controls.Add(this.innerTable);
      this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel.Location = new System.Drawing.Point(3, 30);
      this.panel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.panel.Name = "panel";
      this.panel.Size = new System.Drawing.Size(454, 367);
      this.panel.TabIndex = 1;
      // 
      // overlayDisable
      // 
      this.overlayDisable.BackColor = System.Drawing.Color.Transparent;
      this.overlayDisable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.overlayDisable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.overlayDisable.ForeColor = System.Drawing.SystemColors.Control;
      this.overlayDisable.Location = new System.Drawing.Point(0, 0);
      this.overlayDisable.Name = "overlayDisable";
      this.overlayDisable.Opacity = 200;
      this.overlayDisable.Size = new System.Drawing.Size(450, 363);
      this.overlayDisable.TabIndex = 1;
      this.overlayDisable.TabStop = true;
      this.overlayDisable.Text = "The database has yet to be updated,\r\nsee tasks in progress for more details.";
      this.overlayDisable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.overlayDisable.TransparentBackColor = System.Drawing.SystemColors.ControlDarkDark;
      this.overlayDisable.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OverlayDisableLinkClicked);
      // 
      // innerTable
      // 
      this.innerTable.ColumnCount = 1;
      this.innerTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.innerTable.Controls.Add(this.centralHeader, 0, 0);
      this.innerTable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.innerTable.Location = new System.Drawing.Point(0, 0);
      this.innerTable.Margin = new System.Windows.Forms.Padding(0);
      this.innerTable.Name = "innerTable";
      this.innerTable.RowCount = 2;
      this.innerTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.innerTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.innerTable.Size = new System.Drawing.Size(450, 363);
      this.innerTable.TabIndex = 0;
      // 
      // buttonHome
      // 
      this.buttonHome.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonHome.Image = ((System.Drawing.Image)(resources.GetObject("buttonHome.Image")));
      this.buttonHome.Location = new System.Drawing.Point(3, 3);
      this.buttonHome.Name = "buttonHome";
      this.buttonHome.Size = new System.Drawing.Size(29, 24);
      this.buttonHome.TabIndex = 2;
      this.buttonHome.UseVisualStyleBackColor = true;
      this.buttonHome.Click += new System.EventHandler(this.ButtonHomeClick);
      // 
      // buttonBack
      // 
      this.buttonBack.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonBack.Location = new System.Drawing.Point(393, 3);
      this.buttonBack.Name = "buttonBack";
      this.buttonBack.Size = new System.Drawing.Size(29, 24);
      this.buttonBack.TabIndex = 3;
      this.buttonBack.UseVisualStyleBackColor = true;
      this.buttonBack.Click += new System.EventHandler(this.ButtonBackClick);
      // 
      // buttonNext
      // 
      this.buttonNext.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonNext.Location = new System.Drawing.Point(428, 3);
      this.buttonNext.Name = "buttonNext";
      this.buttonNext.Size = new System.Drawing.Size(29, 24);
      this.buttonNext.TabIndex = 4;
      this.buttonNext.UseVisualStyleBackColor = true;
      this.buttonNext.Click += new System.EventHandler(this.ButtonNextClick);
      // 
      // imageList
      // 
      this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
      this.imageList.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList.Images.SetKeyName(0, "cancel.png");
      this.imageList.Images.SetKeyName(1, "go-next.png");
      this.imageList.Images.SetKeyName(2, "go-previous.png");
      this.imageList.Images.SetKeyName(3, "ok.png");
      // 
      // GuiCenter
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "GuiCenter";
      this.Size = new System.Drawing.Size(460, 400);
      this.baseLayout.ResumeLayout(false);
      this.panel.ResumeLayout(false);
      this.innerTable.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.ImageList imageList;
    private System.Windows.Forms.Button buttonNext;
    private System.Windows.Forms.Button buttonBack;
    private System.Windows.Forms.Button buttonHome;
    private System.Windows.Forms.TableLayoutPanel innerTable;
    private System.Windows.Forms.Panel panel;
    private System.Windows.Forms.Label labelTitle;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lem_Settings.Gui.Shared_controls.CentralHeader centralHeader;
    private Lemoine.BaseControls.TransparentLabel overlayDisable;
  }
}
