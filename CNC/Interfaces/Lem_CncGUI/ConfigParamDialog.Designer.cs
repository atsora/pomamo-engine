// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_CncGUI
{
  partial class ConfigParamDialog
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
      this.cncAcquisitionGroupBox = new System.Windows.Forms.GroupBox();
      this.fileRepositorySelection1 = new Lemoine.DataReferenceControls.FileRepositorySelection();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.parametersTextBox = new System.Windows.Forms.TextBox();
      this.parametersLabel = new System.Windows.Forms.Label();
      this.prefixTextBox = new System.Windows.Forms.TextBox();
      this.prefixLabel = new System.Windows.Forms.Label();
      this.okButton = new System.Windows.Forms.Button();
      this.machineModulesGroupBox = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.machineModuleLabel1 = new System.Windows.Forms.Label();
      this.prefixTextBox1 = new System.Windows.Forms.TextBox();
      this.parametersTextBox1 = new System.Windows.Forms.TextBox();
      this.machineModuleLabel2 = new System.Windows.Forms.Label();
      this.prefixTextBox2 = new System.Windows.Forms.TextBox();
      this.parametersTextBox2 = new System.Windows.Forms.TextBox();
      this.machineModuleLabel3 = new System.Windows.Forms.Label();
      this.prefixTextBox3 = new System.Windows.Forms.TextBox();
      this.parametersTextBox3 = new System.Windows.Forms.TextBox();
      this.machineModuleLabel4 = new System.Windows.Forms.Label();
      this.prefixTextBox4 = new System.Windows.Forms.TextBox();
      this.parametersTextBox4 = new System.Windows.Forms.TextBox();
      this.cncAcquisitionGroupBox.SuspendLayout();
      this.machineModulesGroupBox.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // cncAcquisitionGroupBox
      // 
      this.cncAcquisitionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cncAcquisitionGroupBox.Controls.Add(this.fileRepositorySelection1);
      this.cncAcquisitionGroupBox.Controls.Add(this.nameTextBox);
      this.cncAcquisitionGroupBox.Controls.Add(this.label1);
      this.cncAcquisitionGroupBox.Controls.Add(this.parametersTextBox);
      this.cncAcquisitionGroupBox.Controls.Add(this.parametersLabel);
      this.cncAcquisitionGroupBox.Controls.Add(this.prefixTextBox);
      this.cncAcquisitionGroupBox.Controls.Add(this.prefixLabel);
      this.cncAcquisitionGroupBox.Location = new System.Drawing.Point(13, 13);
      this.cncAcquisitionGroupBox.Name = "cncAcquisitionGroupBox";
      this.cncAcquisitionGroupBox.Size = new System.Drawing.Size(351, 281);
      this.cncAcquisitionGroupBox.TabIndex = 0;
      this.cncAcquisitionGroupBox.TabStop = false;
      this.cncAcquisitionGroupBox.Text = "Cnc Acquisition";
      // 
      // fileRepositorySelection1
      // 
      this.fileRepositorySelection1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fileRepositorySelection1.AutoScroll = true;
      this.fileRepositorySelection1.Location = new System.Drawing.Point(7, 19);
      this.fileRepositorySelection1.Name = "fileRepositorySelection1";
      this.fileRepositorySelection1.NSpace = "cncconfigs";
      this.fileRepositorySelection1.Size = new System.Drawing.Size(195, 230);
      this.fileRepositorySelection1.TabIndex = 8;
      // 
      // nameTextBox
      // 
      this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.nameTextBox.Location = new System.Drawing.Point(208, 42);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(137, 20);
      this.nameTextBox.TabIndex = 7;
      this.nameTextBox.Text = "Test";
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(208, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(39, 23);
      this.label1.TabIndex = 6;
      this.label1.Text = "Name";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.Location = new System.Drawing.Point(7, 255);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(338, 20);
      this.parametersTextBox.TabIndex = 5;
      // 
      // parametersLabel
      // 
      this.parametersLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersLabel.Location = new System.Drawing.Point(229, 230);
      this.parametersLabel.Name = "parametersLabel";
      this.parametersLabel.Size = new System.Drawing.Size(116, 22);
      this.parametersLabel.TabIndex = 4;
      this.parametersLabel.Text = "Parameters";
      this.parametersLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // prefixTextBox
      // 
      this.prefixTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.prefixTextBox.Location = new System.Drawing.Point(208, 91);
      this.prefixTextBox.Name = "prefixTextBox";
      this.prefixTextBox.Size = new System.Drawing.Size(137, 20);
      this.prefixTextBox.TabIndex = 1;
      // 
      // prefixLabel
      // 
      this.prefixLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.prefixLabel.Location = new System.Drawing.Point(208, 65);
      this.prefixLabel.Name = "prefixLabel";
      this.prefixLabel.Size = new System.Drawing.Size(39, 23);
      this.prefixLabel.TabIndex = 0;
      this.prefixLabel.Text = "Prefix";
      this.prefixLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(289, 487);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 1;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // machineModulesGroupBox
      // 
      this.machineModulesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.machineModulesGroupBox.Controls.Add(this.tableLayoutPanel1);
      this.machineModulesGroupBox.Location = new System.Drawing.Point(13, 300);
      this.machineModulesGroupBox.Name = "machineModulesGroupBox";
      this.machineModulesGroupBox.Size = new System.Drawing.Size(351, 181);
      this.machineModulesGroupBox.TabIndex = 2;
      this.machineModulesGroupBox.TabStop = false;
      this.machineModulesGroupBox.Text = "Machine modules";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.label3, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.label4, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.machineModuleLabel1, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.prefixTextBox1, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.parametersTextBox1, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.machineModuleLabel2, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.prefixTextBox2, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.parametersTextBox2, 2, 2);
      this.tableLayoutPanel1.Controls.Add(this.machineModuleLabel3, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.prefixTextBox3, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.parametersTextBox3, 2, 3);
      this.tableLayoutPanel1.Controls.Add(this.machineModuleLabel4, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.prefixTextBox4, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.parametersTextBox4, 2, 4);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 5;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(345, 162);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(4, 1);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(114, 31);
      this.label2.TabIndex = 0;
      this.label2.Text = "Machine module";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(125, 1);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(94, 31);
      this.label3.TabIndex = 1;
      this.label3.Text = "Prefix";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(226, 1);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(115, 31);
      this.label4.TabIndex = 2;
      this.label4.Text = "Parameters";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // machineModuleLabel1
      // 
      this.machineModuleLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModuleLabel1.Location = new System.Drawing.Point(4, 33);
      this.machineModuleLabel1.Name = "machineModuleLabel1";
      this.machineModuleLabel1.Size = new System.Drawing.Size(114, 31);
      this.machineModuleLabel1.TabIndex = 3;
      this.machineModuleLabel1.Click += new System.EventHandler(this.MachineModuleLabel1Click);
      // 
      // prefixTextBox1
      // 
      this.prefixTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.prefixTextBox1.Location = new System.Drawing.Point(125, 36);
      this.prefixTextBox1.Name = "prefixTextBox1";
      this.prefixTextBox1.Size = new System.Drawing.Size(94, 20);
      this.prefixTextBox1.TabIndex = 4;
      // 
      // parametersTextBox1
      // 
      this.parametersTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.parametersTextBox1.Location = new System.Drawing.Point(226, 36);
      this.parametersTextBox1.Name = "parametersTextBox1";
      this.parametersTextBox1.Size = new System.Drawing.Size(115, 20);
      this.parametersTextBox1.TabIndex = 5;
      // 
      // machineModuleLabel2
      // 
      this.machineModuleLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModuleLabel2.Location = new System.Drawing.Point(4, 65);
      this.machineModuleLabel2.Name = "machineModuleLabel2";
      this.machineModuleLabel2.Size = new System.Drawing.Size(114, 31);
      this.machineModuleLabel2.TabIndex = 3;
      this.machineModuleLabel2.Click += new System.EventHandler(this.MachineModuleLabel2Click);
      // 
      // prefixTextBox2
      // 
      this.prefixTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.prefixTextBox2.Location = new System.Drawing.Point(125, 68);
      this.prefixTextBox2.Name = "prefixTextBox2";
      this.prefixTextBox2.Size = new System.Drawing.Size(94, 20);
      this.prefixTextBox2.TabIndex = 4;
      // 
      // parametersTextBox2
      // 
      this.parametersTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.parametersTextBox2.Location = new System.Drawing.Point(226, 68);
      this.parametersTextBox2.Name = "parametersTextBox2";
      this.parametersTextBox2.Size = new System.Drawing.Size(115, 20);
      this.parametersTextBox2.TabIndex = 5;
      // 
      // machineModuleLabel3
      // 
      this.machineModuleLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModuleLabel3.Location = new System.Drawing.Point(4, 97);
      this.machineModuleLabel3.Name = "machineModuleLabel3";
      this.machineModuleLabel3.Size = new System.Drawing.Size(114, 31);
      this.machineModuleLabel3.TabIndex = 3;
      this.machineModuleLabel3.Click += new System.EventHandler(this.MachineModuleLabel3Click);
      // 
      // prefixTextBox3
      // 
      this.prefixTextBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.prefixTextBox3.Location = new System.Drawing.Point(125, 100);
      this.prefixTextBox3.Name = "prefixTextBox3";
      this.prefixTextBox3.Size = new System.Drawing.Size(94, 20);
      this.prefixTextBox3.TabIndex = 4;
      // 
      // parametersTextBox3
      // 
      this.parametersTextBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.parametersTextBox3.Location = new System.Drawing.Point(226, 100);
      this.parametersTextBox3.Name = "parametersTextBox3";
      this.parametersTextBox3.Size = new System.Drawing.Size(115, 20);
      this.parametersTextBox3.TabIndex = 5;
      // 
      // machineModuleLabel4
      // 
      this.machineModuleLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModuleLabel4.Location = new System.Drawing.Point(4, 129);
      this.machineModuleLabel4.Name = "machineModuleLabel4";
      this.machineModuleLabel4.Size = new System.Drawing.Size(114, 32);
      this.machineModuleLabel4.TabIndex = 3;
      this.machineModuleLabel4.Click += new System.EventHandler(this.MachineModuleLabel4Click);
      // 
      // prefixTextBox4
      // 
      this.prefixTextBox4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.prefixTextBox4.Location = new System.Drawing.Point(125, 132);
      this.prefixTextBox4.Name = "prefixTextBox4";
      this.prefixTextBox4.Size = new System.Drawing.Size(94, 20);
      this.prefixTextBox4.TabIndex = 4;
      // 
      // parametersTextBox4
      // 
      this.parametersTextBox4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.parametersTextBox4.Location = new System.Drawing.Point(226, 132);
      this.parametersTextBox4.Name = "parametersTextBox4";
      this.parametersTextBox4.Size = new System.Drawing.Size(115, 20);
      this.parametersTextBox4.TabIndex = 5;
      // 
      // ConfigParamDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(376, 522);
      this.Controls.Add(this.machineModulesGroupBox);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cncAcquisitionGroupBox);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ConfigParamDialog";
      this.ShowInTaskbar = false;
      this.Text = "Cnc Config+Param";
      this.cncAcquisitionGroupBox.ResumeLayout(false);
      this.cncAcquisitionGroupBox.PerformLayout();
      this.machineModulesGroupBox.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TextBox parametersTextBox1;
    private System.Windows.Forms.TextBox prefixTextBox1;
    private System.Windows.Forms.Label machineModuleLabel1;
    private System.Windows.Forms.TextBox parametersTextBox2;
    private System.Windows.Forms.TextBox prefixTextBox2;
    private System.Windows.Forms.Label machineModuleLabel2;
    private System.Windows.Forms.TextBox parametersTextBox3;
    private System.Windows.Forms.TextBox prefixTextBox3;
    private System.Windows.Forms.Label machineModuleLabel3;
    private System.Windows.Forms.TextBox parametersTextBox4;
    private System.Windows.Forms.TextBox prefixTextBox4;
    private System.Windows.Forms.Label machineModuleLabel4;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private Lemoine.DataReferenceControls.FileRepositorySelection fileRepositorySelection1;
    private System.Windows.Forms.GroupBox machineModulesGroupBox;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Label parametersLabel;
    private System.Windows.Forms.TextBox parametersTextBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.TextBox prefixTextBox;
    private System.Windows.Forms.Label prefixLabel;
    private System.Windows.Forms.GroupBox cncAcquisitionGroupBox;
  }
}
