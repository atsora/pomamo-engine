// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class DialogDescriptionXmlFile
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DialogDescriptionXmlFile));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.labelUnit = new System.Windows.Forms.Label();
      this.richTextDescription = new System.Windows.Forms.RichTextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.listMachines = new Lemoine.BaseControls.List.ListTextValue();
      this.listControls = new Lemoine.BaseControls.List.ListTextValue();
      this.listProtocols = new Lemoine.BaseControls.List.ListTextValue();
      this.label6 = new System.Windows.Forms.Label();
      this.labelModules = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.Controls.Add(this.label1, 0, 4);
      this.baseLayout.Controls.Add(this.label2, 0, 0);
      this.baseLayout.Controls.Add(this.label3, 0, 2);
      this.baseLayout.Controls.Add(this.labelUnit, 1, 2);
      this.baseLayout.Controls.Add(this.richTextDescription, 0, 1);
      this.baseLayout.Controls.Add(this.label4, 1, 4);
      this.baseLayout.Controls.Add(this.label5, 2, 4);
      this.baseLayout.Controls.Add(this.listMachines, 0, 5);
      this.baseLayout.Controls.Add(this.listControls, 1, 5);
      this.baseLayout.Controls.Add(this.listProtocols, 2, 5);
      this.baseLayout.Controls.Add(this.label6, 0, 3);
      this.baseLayout.Controls.Add(this.labelModules, 1, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Size = new System.Drawing.Size(461, 341);
      this.baseLayout.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(3, 194);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(147, 24);
      this.label1.TabIndex = 0;
      this.label1.Text = "Supported machines";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.baseLayout.SetColumnSpan(this.label2, 3);
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(455, 24);
      this.label2.TabIndex = 1;
      this.label2.Text = "Description";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(3, 146);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(147, 24);
      this.label3.TabIndex = 2;
      this.label3.Text = "Unit";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelUnit
      // 
      this.baseLayout.SetColumnSpan(this.labelUnit, 2);
      this.labelUnit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelUnit.Location = new System.Drawing.Point(156, 146);
      this.labelUnit.Name = "labelUnit";
      this.labelUnit.Size = new System.Drawing.Size(302, 24);
      this.labelUnit.TabIndex = 3;
      this.labelUnit.Text = "labelUnit";
      this.labelUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // richTextDescription
      // 
      this.richTextDescription.BackColor = System.Drawing.SystemColors.Window;
      this.baseLayout.SetColumnSpan(this.richTextDescription, 3);
      this.richTextDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextDescription.Location = new System.Drawing.Point(3, 27);
      this.richTextDescription.Name = "richTextDescription";
      this.richTextDescription.ReadOnly = true;
      this.richTextDescription.Size = new System.Drawing.Size(455, 116);
      this.richTextDescription.TabIndex = 4;
      this.richTextDescription.Text = "";
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(156, 194);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(147, 24);
      this.label4.TabIndex = 5;
      this.label4.Text = "Supported controls";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(309, 194);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(149, 24);
      this.label5.TabIndex = 6;
      this.label5.Text = "Supported protocols";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // listMachines
      // 
      this.listMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listMachines.Location = new System.Drawing.Point(2, 218);
      this.listMachines.Margin = new System.Windows.Forms.Padding(2, 0, 2, 2);
      this.listMachines.Name = "listMachines";
      this.listMachines.Size = new System.Drawing.Size(149, 121);
      this.listMachines.TabIndex = 7;
      // 
      // listControls
      // 
      this.listControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listControls.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listControls.Location = new System.Drawing.Point(155, 218);
      this.listControls.Margin = new System.Windows.Forms.Padding(2, 0, 2, 2);
      this.listControls.Name = "listControls";
      this.listControls.Size = new System.Drawing.Size(149, 121);
      this.listControls.TabIndex = 8;
      // 
      // listProtocols
      // 
      this.listProtocols.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listProtocols.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listProtocols.Location = new System.Drawing.Point(308, 218);
      this.listProtocols.Margin = new System.Windows.Forms.Padding(2, 0, 2, 2);
      this.listProtocols.Name = "listProtocols";
      this.listProtocols.Size = new System.Drawing.Size(151, 121);
      this.listProtocols.TabIndex = 9;
      // 
      // label6
      // 
      this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label6.Location = new System.Drawing.Point(3, 170);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(147, 24);
      this.label6.TabIndex = 10;
      this.label6.Text = "Modules";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelModules
      // 
      this.baseLayout.SetColumnSpan(this.labelModules, 2);
      this.labelModules.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelModules.Location = new System.Drawing.Point(156, 170);
      this.labelModules.Name = "labelModules";
      this.labelModules.Size = new System.Drawing.Size(302, 24);
      this.labelModules.TabIndex = 11;
      this.labelModules.Text = "labelModules";
      this.labelModules.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // DialogDescriptionXmlFile
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(461, 341);
      this.Controls.Add(this.baseLayout);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "DialogDescriptionXmlFile";
      this.Text = "Xml file description";
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.RichTextBox richTextDescription;
    private System.Windows.Forms.Label labelUnit;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private Lemoine.BaseControls.List.ListTextValue listMachines;
    private Lemoine.BaseControls.List.ListTextValue listControls;
    private Lemoine.BaseControls.List.ListTextValue listProtocols;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label labelModules;
  }
}
