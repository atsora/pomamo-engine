// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorPlugins
{
  partial class PagePluginInformation
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.labelName = new System.Windows.Forms.Label();
      this.labelIdentifier = new System.Windows.Forms.Label();
      this.labelDescription = new System.Windows.Forms.Label();
      this.labelPath = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.listPackages = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 106F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label1, 0, 2);
      this.baseLayout.Controls.Add(this.label2, 0, 0);
      this.baseLayout.Controls.Add(this.label4, 0, 1);
      this.baseLayout.Controls.Add(this.label5, 0, 3);
      this.baseLayout.Controls.Add(this.labelName, 1, 0);
      this.baseLayout.Controls.Add(this.labelIdentifier, 1, 1);
      this.baseLayout.Controls.Add(this.labelDescription, 1, 2);
      this.baseLayout.Controls.Add(this.labelPath, 1, 3);
      this.baseLayout.Controls.Add(this.label3, 0, 4);
      this.baseLayout.Controls.Add(this.listPackages, 1, 4);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 5;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(0, 51);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(103, 106);
      this.label1.TabIndex = 4;
      this.label1.Text = "Description";
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(103, 24);
      this.label2.TabIndex = 6;
      this.label2.Text = "Name";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(0, 24);
      this.label4.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(103, 24);
      this.label4.TabIndex = 10;
      this.label4.Text = "Identification";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(0, 157);
      this.label5.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(103, 24);
      this.label5.TabIndex = 11;
      this.label5.Text = "Dll path";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelName
      // 
      this.labelName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelName.Location = new System.Drawing.Point(106, 0);
      this.labelName.Margin = new System.Windows.Forms.Padding(0);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(264, 24);
      this.labelName.TabIndex = 13;
      this.labelName.Text = "labelName";
      this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelIdentifier
      // 
      this.labelIdentifier.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelIdentifier.Location = new System.Drawing.Point(106, 24);
      this.labelIdentifier.Margin = new System.Windows.Forms.Padding(0);
      this.labelIdentifier.Name = "labelIdentifier";
      this.labelIdentifier.Size = new System.Drawing.Size(264, 24);
      this.labelIdentifier.TabIndex = 14;
      this.labelIdentifier.Text = "labelIdentifier";
      this.labelIdentifier.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelDescription
      // 
      this.labelDescription.AutoEllipsis = true;
      this.labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDescription.Location = new System.Drawing.Point(106, 51);
      this.labelDescription.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(264, 106);
      this.labelDescription.TabIndex = 16;
      this.labelDescription.Text = "labelDescription";
      // 
      // labelPath
      // 
      this.labelPath.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPath.Location = new System.Drawing.Point(106, 157);
      this.labelPath.Margin = new System.Windows.Forms.Padding(0);
      this.labelPath.Name = "labelPath";
      this.labelPath.Size = new System.Drawing.Size(264, 24);
      this.labelPath.TabIndex = 17;
      this.labelPath.Text = "labelPath";
      this.labelPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(0, 184);
      this.label3.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(103, 106);
      this.label3.TabIndex = 18;
      this.label3.Text = "Packages using this plugin";
      // 
      // listPackages
      // 
      this.listPackages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listPackages.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listPackages.Location = new System.Drawing.Point(106, 181);
      this.listPackages.Margin = new System.Windows.Forms.Padding(0);
      this.listPackages.Name = "listPackages";
      this.listPackages.Size = new System.Drawing.Size(264, 109);
      this.listPackages.Sorted = true;
      this.listPackages.TabIndex = 19;
      // 
      // PagePluginInformation
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PagePluginInformation";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label labelPath;
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.Label labelIdentifier;
    private System.Windows.Forms.Label labelDescription;
    private System.Windows.Forms.Label label3;
    private Lemoine.BaseControls.List.ListTextValue listPackages;
  }
}
