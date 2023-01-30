// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorRedirectEvent
{
  partial class ActionCell
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label labelTitle;
    private System.Windows.Forms.Label labelDescription;
    private System.Windows.Forms.Button buttonOpenFile;
    private System.Windows.Forms.CheckBox checkEnable;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActionCell));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.labelTitle = new System.Windows.Forms.Label();
      this.labelDescription = new System.Windows.Forms.Label();
      this.checkEnable = new System.Windows.Forms.CheckBox();
      this.buttonOpenFile = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.AutoSize = true;
      this.baseLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.baseLayout.Controls.Add(this.labelTitle, 0, 0);
      this.baseLayout.Controls.Add(this.labelDescription, 0, 1);
      this.baseLayout.Controls.Add(this.checkEnable, 1, 0);
      this.baseLayout.Controls.Add(this.buttonOpenFile, 2, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(265, 70);
      this.baseLayout.TabIndex = 9;
      // 
      // labelTitle
      // 
      this.labelTitle.AutoEllipsis = true;
      this.labelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelTitle.Location = new System.Drawing.Point(3, 0);
      this.labelTitle.Name = "labelTitle";
      this.labelTitle.Size = new System.Drawing.Size(207, 25);
      this.labelTitle.TabIndex = 0;
      this.labelTitle.Text = "labelTitle";
      this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.labelTitle.UseCompatibleTextRendering = true;
      // 
      // labelDescription
      // 
      this.labelDescription.AutoEllipsis = true;
      this.baseLayout.SetColumnSpan(this.labelDescription, 3);
      this.labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDescription.Location = new System.Drawing.Point(3, 25);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(259, 45);
      this.labelDescription.TabIndex = 1;
      this.labelDescription.Text = "labelDescription";
      this.labelDescription.UseCompatibleTextRendering = true;
      // 
      // checkEnable
      // 
      this.checkEnable.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.checkEnable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkEnable.Location = new System.Drawing.Point(216, 3);
      this.checkEnable.Name = "checkEnable";
      this.checkEnable.Size = new System.Drawing.Size(16, 19);
      this.checkEnable.TabIndex = 3;
      this.checkEnable.UseVisualStyleBackColor = true;
      this.checkEnable.CheckedChanged += new System.EventHandler(this.CheckEnableCheckedChanged);
      // 
      // buttonOpenFile
      // 
      this.buttonOpenFile.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOpenFile.Image = ((System.Drawing.Image)(resources.GetObject("buttonOpenFile.Image")));
      this.buttonOpenFile.Location = new System.Drawing.Point(235, 0);
      this.buttonOpenFile.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.buttonOpenFile.Name = "buttonOpenFile";
      this.buttonOpenFile.Size = new System.Drawing.Size(27, 25);
      this.buttonOpenFile.TabIndex = 2;
      this.buttonOpenFile.UseVisualStyleBackColor = true;
      this.buttonOpenFile.Click += new System.EventHandler(this.ButtonOpenFileClick);
      // 
      // AlertCell
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.MinimumSize = new System.Drawing.Size(200, 70);
      this.Name = "AlertCell";
      this.Size = new System.Drawing.Size(265, 70);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
  }
}
