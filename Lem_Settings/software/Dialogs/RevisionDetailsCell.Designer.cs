// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class RevisionDetailsCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelMainInformation;
    private System.Windows.Forms.Label labelStatus;
    private System.Windows.Forms.Label labelDetails;
    private System.Windows.Forms.PictureBox pictureBox;
    
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelMainInformation = new System.Windows.Forms.Label();
      this.labelStatus = new System.Windows.Forms.Label();
      this.labelDetails = new System.Windows.Forms.Label();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 197F));
      this.baseLayout.Controls.Add(this.labelMainInformation, 1, 0);
      this.baseLayout.Controls.Add(this.labelStatus, 1, 1);
      this.baseLayout.Controls.Add(this.labelDetails, 1, 2);
      this.baseLayout.Controls.Add(this.pictureBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.Size = new System.Drawing.Size(247, 57);
      this.baseLayout.TabIndex = 0;
      // 
      // labelMainInformation
      // 
      this.labelMainInformation.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelMainInformation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelMainInformation.Location = new System.Drawing.Point(53, 0);
      this.labelMainInformation.Name = "labelMainInformation";
      this.labelMainInformation.Size = new System.Drawing.Size(191, 19);
      this.labelMainInformation.TabIndex = 0;
      this.labelMainInformation.Text = "Type (#id)";
      this.labelMainInformation.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // labelStatus
      // 
      this.labelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelStatus.Location = new System.Drawing.Point(53, 19);
      this.labelStatus.Name = "labelStatus";
      this.labelStatus.Size = new System.Drawing.Size(191, 19);
      this.labelStatus.TabIndex = 1;
      this.labelStatus.Text = "labelStatus";
      this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelDetails
      // 
      this.labelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDetails.Location = new System.Drawing.Point(53, 38);
      this.labelDetails.Name = "labelDetails";
      this.labelDetails.Size = new System.Drawing.Size(191, 19);
      this.labelDetails.TabIndex = 2;
      this.labelDetails.Text = "labelDetails";
      // 
      // pictureBox
      // 
      this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox.Location = new System.Drawing.Point(0, 0);
      this.pictureBox.Margin = new System.Windows.Forms.Padding(0);
      this.pictureBox.Name = "pictureBox";
      this.baseLayout.SetRowSpan(this.pictureBox, 3);
      this.pictureBox.Size = new System.Drawing.Size(50, 57);
      this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.pictureBox.TabIndex = 3;
      this.pictureBox.TabStop = false;
      // 
      // RevisionDetailsCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "RevisionDetailsCell";
      this.Size = new System.Drawing.Size(247, 57);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);

    }
  }
}
