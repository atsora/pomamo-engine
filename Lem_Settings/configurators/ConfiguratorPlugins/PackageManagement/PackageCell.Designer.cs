// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorPlugins
{
  partial class PackageCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.PictureBox pictureWarning;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageCell));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelName = new System.Windows.Forms.Label();
      this.pictureWarning = new System.Windows.Forms.PictureBox();
      this.buttonActivate = new System.Windows.Forms.Button();
      this.buttonDeactivate = new System.Windows.Forms.Button();
      this.buttonDelete = new System.Windows.Forms.Button();
      this.buttonView = new System.Windows.Forms.Button();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.baseLayout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureWarning)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 6;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
      this.baseLayout.Controls.Add(this.labelName, 0, 0);
      this.baseLayout.Controls.Add(this.pictureWarning, 1, 0);
      this.baseLayout.Controls.Add(this.buttonActivate, 2, 0);
      this.baseLayout.Controls.Add(this.buttonDeactivate, 3, 0);
      this.baseLayout.Controls.Add(this.buttonDelete, 4, 0);
      this.baseLayout.Controls.Add(this.buttonView, 5, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 1;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.Size = new System.Drawing.Size(222, 30);
      this.baseLayout.TabIndex = 1;
      // 
      // labelName
      // 
      this.labelName.AutoEllipsis = true;
      this.labelName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelName.Location = new System.Drawing.Point(0, 0);
      this.labelName.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(76, 30);
      this.labelName.TabIndex = 6;
      this.labelName.Text = "labelName";
      this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pictureWarning
      // 
      this.pictureWarning.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
      this.pictureWarning.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureWarning.Image = ((System.Drawing.Image)(resources.GetObject("pictureWarning.Image")));
      this.pictureWarning.Location = new System.Drawing.Point(82, 5);
      this.pictureWarning.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
      this.pictureWarning.Name = "pictureWarning";
      this.pictureWarning.Size = new System.Drawing.Size(24, 25);
      this.pictureWarning.TabIndex = 13;
      this.pictureWarning.TabStop = false;
      // 
      // buttonActivate
      // 
      this.buttonActivate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonActivate.Image = ((System.Drawing.Image)(resources.GetObject("buttonActivate.Image")));
      this.buttonActivate.Location = new System.Drawing.Point(109, 2);
      this.buttonActivate.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
      this.buttonActivate.Name = "buttonActivate";
      this.buttonActivate.Size = new System.Drawing.Size(26, 26);
      this.buttonActivate.TabIndex = 14;
      this.toolTip1.SetToolTip(this.buttonActivate, "Activate the package");
      this.buttonActivate.UseVisualStyleBackColor = true;
      this.buttonActivate.Click += new System.EventHandler(this.buttonActivate_Click);
      // 
      // buttonDeactivate
      // 
      this.buttonDeactivate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDeactivate.Image = ((System.Drawing.Image)(resources.GetObject("buttonDeactivate.Image")));
      this.buttonDeactivate.Location = new System.Drawing.Point(138, 2);
      this.buttonDeactivate.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
      this.buttonDeactivate.Name = "buttonDeactivate";
      this.buttonDeactivate.Size = new System.Drawing.Size(26, 26);
      this.buttonDeactivate.TabIndex = 15;
      this.toolTip1.SetToolTip(this.buttonDeactivate, "Deactivate the package");
      this.buttonDeactivate.UseVisualStyleBackColor = true;
      this.buttonDeactivate.Click += new System.EventHandler(this.buttonDeactivate_Click);
      // 
      // buttonDelete
      // 
      this.buttonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonDelete.Image = ((System.Drawing.Image)(resources.GetObject("buttonDelete.Image")));
      this.buttonDelete.Location = new System.Drawing.Point(167, 2);
      this.buttonDelete.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new System.Drawing.Size(26, 26);
      this.buttonDelete.TabIndex = 16;
      this.toolTip1.SetToolTip(this.buttonDelete, "Delete the package");
      this.buttonDelete.UseVisualStyleBackColor = true;
      this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
      // 
      // buttonView
      // 
      this.buttonView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonView.Image = ((System.Drawing.Image)(resources.GetObject("buttonView.Image")));
      this.buttonView.Location = new System.Drawing.Point(196, 2);
      this.buttonView.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
      this.buttonView.Name = "buttonView";
      this.buttonView.Size = new System.Drawing.Size(26, 26);
      this.buttonView.TabIndex = 17;
      this.toolTip1.SetToolTip(this.buttonView, "View the details of the package");
      this.buttonView.UseVisualStyleBackColor = true;
      this.buttonView.Click += new System.EventHandler(this.buttonView_Click);
      // 
      // PackageCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PackageCell";
      this.Size = new System.Drawing.Size(222, 30);
      this.baseLayout.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureWarning)).EndInit();
      this.ResumeLayout(false);

    }

    private System.Windows.Forms.Button buttonActivate;
    private System.Windows.Forms.Button buttonDeactivate;
    private System.Windows.Forms.Button buttonDelete;
    private System.Windows.Forms.Button buttonView;
    private System.Windows.Forms.ToolTip toolTip1;
  }
}
