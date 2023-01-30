// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class DialogItemInformation
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label labelTitle;
    private System.Windows.Forms.Label labelId;
    private System.Windows.Forms.Label labelDllPath;
    private System.Windows.Forms.Label labelIniPath;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label labelViewMode;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DialogItemInformation));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.labelTitle = new System.Windows.Forms.Label();
      this.labelId = new System.Windows.Forms.Label();
      this.labelDllPath = new System.Windows.Forms.Label();
      this.labelIniPath = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.labelViewMode = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 89F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Controls.Add(this.label2, 0, 2);
      this.baseLayout.Controls.Add(this.label4, 0, 4);
      this.baseLayout.Controls.Add(this.label5, 0, 5);
      this.baseLayout.Controls.Add(this.labelTitle, 1, 1);
      this.baseLayout.Controls.Add(this.labelId, 1, 2);
      this.baseLayout.Controls.Add(this.labelDllPath, 1, 4);
      this.baseLayout.Controls.Add(this.labelIniPath, 1, 5);
      this.baseLayout.Controls.Add(this.label3, 0, 3);
      this.baseLayout.Controls.Add(this.labelViewMode, 1, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(379, 201);
      this.baseLayout.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(0, 3);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(86, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "Title";
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(0, 28);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(86, 25);
      this.label2.TabIndex = 1;
      this.label2.Text = "Identification";
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(0, 78);
      this.label4.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(86, 61);
      this.label4.TabIndex = 3;
      this.label4.Text = "Dll path";
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(0, 139);
      this.label5.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(86, 62);
      this.label5.TabIndex = 4;
      this.label5.Text = "Ini path";
      // 
      // labelTitle
      // 
      this.labelTitle.AutoEllipsis = true;
      this.labelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelTitle.Location = new System.Drawing.Point(92, 3);
      this.labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.labelTitle.Name = "labelTitle";
      this.labelTitle.Size = new System.Drawing.Size(287, 25);
      this.labelTitle.TabIndex = 5;
      this.labelTitle.Text = "labelTitle";
      // 
      // labelId
      // 
      this.labelId.AutoEllipsis = true;
      this.labelId.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelId.Location = new System.Drawing.Point(92, 28);
      this.labelId.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.labelId.Name = "labelId";
      this.labelId.Size = new System.Drawing.Size(287, 25);
      this.labelId.TabIndex = 6;
      this.labelId.Text = "labelId";
      // 
      // labelDllPath
      // 
      this.labelDllPath.AutoEllipsis = true;
      this.labelDllPath.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDllPath.Location = new System.Drawing.Point(92, 78);
      this.labelDllPath.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.labelDllPath.Name = "labelDllPath";
      this.labelDllPath.Size = new System.Drawing.Size(287, 61);
      this.labelDllPath.TabIndex = 8;
      this.labelDllPath.Text = "labelDllPath";
      // 
      // labelIniPath
      // 
      this.labelIniPath.AutoEllipsis = true;
      this.labelIniPath.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelIniPath.Location = new System.Drawing.Point(92, 139);
      this.labelIniPath.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.labelIniPath.Name = "labelIniPath";
      this.labelIniPath.Size = new System.Drawing.Size(287, 62);
      this.labelIniPath.TabIndex = 9;
      this.labelIniPath.Text = "labelIniPath";
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(0, 53);
      this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(86, 25);
      this.label3.TabIndex = 10;
      this.label3.Text = "View mode";
      // 
      // labelViewMode
      // 
      this.labelViewMode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelViewMode.Location = new System.Drawing.Point(92, 53);
      this.labelViewMode.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.labelViewMode.Name = "labelViewMode";
      this.labelViewMode.Size = new System.Drawing.Size(287, 25);
      this.labelViewMode.TabIndex = 11;
      this.labelViewMode.Text = "labelViewMode";
      // 
      // DialogItemInformation
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(379, 201);
      this.Controls.Add(this.baseLayout);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximumSize = new System.Drawing.Size(1500, 240);
      this.MinimumSize = new System.Drawing.Size(300, 240);
      this.Name = "DialogItemInformation";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Information";
      this.TopMost = true;
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
