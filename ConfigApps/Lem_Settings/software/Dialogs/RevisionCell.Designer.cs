// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class RevisionCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelItem;
    private System.Windows.Forms.Label labelProgress;
    private Lemoine.BaseControls.List.ListTextValue listErrors;
    private System.Windows.Forms.Label labelDetails;
    private System.Windows.Forms.Button buttonDetails;
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.ImageList imageList;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionCell));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelItem = new System.Windows.Forms.Label();
      this.labelProgress = new System.Windows.Forms.Label();
      this.listErrors = new Lemoine.BaseControls.List.ListTextValue();
      this.labelDetails = new System.Windows.Forms.Label();
      this.buttonDetails = new System.Windows.Forms.Button();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.Controls.Add(this.labelItem, 1, 0);
      this.baseLayout.Controls.Add(this.labelProgress, 1, 2);
      this.baseLayout.Controls.Add(this.listErrors, 1, 3);
      this.baseLayout.Controls.Add(this.labelDetails, 1, 1);
      this.baseLayout.Controls.Add(this.buttonDetails, 2, 0);
      this.baseLayout.Controls.Add(this.pictureBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(240, 100);
      this.baseLayout.TabIndex = 0;
      // 
      // labelItem
      // 
      this.labelItem.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelItem.Location = new System.Drawing.Point(53, 0);
      this.labelItem.Name = "labelItem";
      this.labelItem.Size = new System.Drawing.Size(124, 20);
      this.labelItem.TabIndex = 4;
      this.labelItem.Text = "labelItem";
      this.labelItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelProgress
      // 
      this.baseLayout.SetColumnSpan(this.labelProgress, 2);
      this.labelProgress.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelProgress.Location = new System.Drawing.Point(53, 40);
      this.labelProgress.Name = "labelProgress";
      this.labelProgress.Size = new System.Drawing.Size(184, 20);
      this.labelProgress.TabIndex = 5;
      this.labelProgress.Text = "labelProgress";
      this.labelProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // listErrors
      // 
      this.listErrors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listErrors, 2);
      this.listErrors.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listErrors.Location = new System.Drawing.Point(50, 60);
      this.listErrors.Margin = new System.Windows.Forms.Padding(0);
      this.listErrors.Name = "listErrors";
      this.listErrors.Size = new System.Drawing.Size(190, 40);
      this.listErrors.TabIndex = 7;
      // 
      // labelDetails
      // 
      this.baseLayout.SetColumnSpan(this.labelDetails, 2);
      this.labelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDetails.Location = new System.Drawing.Point(53, 20);
      this.labelDetails.Name = "labelDetails";
      this.labelDetails.Size = new System.Drawing.Size(184, 20);
      this.labelDetails.TabIndex = 9;
      this.labelDetails.Text = "labelDetails";
      this.labelDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonDetails
      // 
      this.buttonDetails.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDetails.Location = new System.Drawing.Point(180, 0);
      this.buttonDetails.Margin = new System.Windows.Forms.Padding(0);
      this.buttonDetails.Name = "buttonDetails";
      this.buttonDetails.Size = new System.Drawing.Size(60, 20);
      this.buttonDetails.TabIndex = 10;
      this.buttonDetails.Text = "Details";
      this.buttonDetails.UseVisualStyleBackColor = true;
      this.buttonDetails.Click += new System.EventHandler(this.ButtonDetailsClick);
      // 
      // pictureBox
      // 
      this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox.Location = new System.Drawing.Point(3, 3);
      this.pictureBox.Name = "pictureBox";
      this.baseLayout.SetRowSpan(this.pictureBox, 3);
      this.pictureBox.Size = new System.Drawing.Size(44, 54);
      this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.pictureBox.TabIndex = 11;
      this.pictureBox.TabStop = false;
      // 
      // imageList
      // 
      this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
      this.imageList.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList.Images.SetKeyName(0, "clock.png");
      this.imageList.Images.SetKeyName(1, "validated.png");
      this.imageList.Images.SetKeyName(2, "warning.png");
      // 
      // RevisionCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "RevisionCell";
      this.Size = new System.Drawing.Size(240, 100);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);

    }
  }
}
