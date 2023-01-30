// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorPlugins
{
  partial class FileCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.PictureBox pictureWarning;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label labelDirectory;
    private System.Windows.Forms.Label labelCreationDate;
    private System.Windows.Forms.Label labelModifiedDate;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileCell));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelName = new System.Windows.Forms.Label();
      this.pictureWarning = new System.Windows.Forms.PictureBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.labelDirectory = new System.Windows.Forms.Label();
      this.labelCreationDate = new System.Windows.Forms.Label();
      this.labelModifiedDate = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureWarning)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.Controls.Add(this.labelName, 1, 0);
      this.baseLayout.Controls.Add(this.pictureWarning, 2, 0);
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 0, 1);
      this.baseLayout.Controls.Add(this.label3, 0, 2);
      this.baseLayout.Controls.Add(this.label4, 0, 3);
      this.baseLayout.Controls.Add(this.labelDirectory, 1, 3);
      this.baseLayout.Controls.Add(this.labelCreationDate, 1, 1);
      this.baseLayout.Controls.Add(this.labelModifiedDate, 1, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(323, 102);
      this.baseLayout.TabIndex = 1;
      // 
      // labelName
      // 
      this.labelName.AutoEllipsis = true;
      this.labelName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelName.Location = new System.Drawing.Point(100, 0);
      this.labelName.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(193, 22);
      this.labelName.TabIndex = 6;
      this.labelName.Text = "labelName";
      this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pictureWarning
      // 
      this.pictureWarning.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
      this.pictureWarning.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureWarning.ErrorImage = null;
      this.pictureWarning.Image = ((System.Drawing.Image)(resources.GetObject("pictureWarning.Image")));
      this.pictureWarning.InitialImage = null;
      this.pictureWarning.Location = new System.Drawing.Point(299, 3);
      this.pictureWarning.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.pictureWarning.Name = "pictureWarning";
      this.baseLayout.SetRowSpan(this.pictureWarning, 2);
      this.pictureWarning.Size = new System.Drawing.Size(24, 39);
      this.pictureWarning.TabIndex = 13;
      this.pictureWarning.TabStop = false;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(94, 22);
      this.label1.TabIndex = 14;
      this.label1.Text = "Name";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 22);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(94, 20);
      this.label2.TabIndex = 15;
      this.label2.Text = "Creation date";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 42);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(94, 20);
      this.label3.TabIndex = 16;
      this.label3.Text = "Last modified date";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(3, 65);
      this.label4.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(94, 37);
      this.label4.TabIndex = 17;
      this.label4.Text = "Directory";
      // 
      // labelDirectory
      // 
      this.labelDirectory.AutoEllipsis = true;
      this.baseLayout.SetColumnSpan(this.labelDirectory, 2);
      this.labelDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDirectory.Location = new System.Drawing.Point(100, 65);
      this.labelDirectory.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
      this.labelDirectory.Name = "labelDirectory";
      this.labelDirectory.Size = new System.Drawing.Size(220, 37);
      this.labelDirectory.TabIndex = 18;
      this.labelDirectory.Text = "labelDirectory";
      // 
      // labelCreationDate
      // 
      this.labelCreationDate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelCreationDate.Location = new System.Drawing.Point(100, 22);
      this.labelCreationDate.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelCreationDate.Name = "labelCreationDate";
      this.labelCreationDate.Size = new System.Drawing.Size(193, 20);
      this.labelCreationDate.TabIndex = 19;
      this.labelCreationDate.Text = "labelCreationDate";
      this.labelCreationDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelModifiedDate
      // 
      this.baseLayout.SetColumnSpan(this.labelModifiedDate, 2);
      this.labelModifiedDate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelModifiedDate.Location = new System.Drawing.Point(100, 42);
      this.labelModifiedDate.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelModifiedDate.Name = "labelModifiedDate";
      this.labelModifiedDate.Size = new System.Drawing.Size(220, 20);
      this.labelModifiedDate.TabIndex = 20;
      this.labelModifiedDate.Text = "labelModifiedDate";
      this.labelModifiedDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // FileCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "FileCell";
      this.Size = new System.Drawing.Size(323, 102);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureWarning)).EndInit();
      this.ResumeLayout(false);

    }
  }
}
