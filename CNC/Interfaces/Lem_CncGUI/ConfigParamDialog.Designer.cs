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
    protected override void Dispose (bool disposing)
    {
      if (disposing) {
        if (components != null) {
          components.Dispose ();
        }
      }
      base.Dispose (disposing);
    }

    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent ()
    {
      cncAcquisitionGroupBox = new System.Windows.Forms.GroupBox ();
      keyParamsTextBox = new System.Windows.Forms.TextBox ();
      fileRepositorySelection1 = new Lemoine.DataReferenceControls.FileRepositorySelection ();
      nameTextBox = new System.Windows.Forms.TextBox ();
      label1 = new System.Windows.Forms.Label ();
      parametersTextBox = new System.Windows.Forms.TextBox ();
      parametersLabel = new System.Windows.Forms.Label ();
      prefixTextBox = new System.Windows.Forms.TextBox ();
      prefixLabel = new System.Windows.Forms.Label ();
      okButton = new System.Windows.Forms.Button ();
      machineModulesGroupBox = new System.Windows.Forms.GroupBox ();
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel ();
      label2 = new System.Windows.Forms.Label ();
      label3 = new System.Windows.Forms.Label ();
      label4 = new System.Windows.Forms.Label ();
      machineModuleLabel1 = new System.Windows.Forms.Label ();
      prefixTextBox1 = new System.Windows.Forms.TextBox ();
      parametersTextBox1 = new System.Windows.Forms.TextBox ();
      machineModuleLabel2 = new System.Windows.Forms.Label ();
      prefixTextBox2 = new System.Windows.Forms.TextBox ();
      parametersTextBox2 = new System.Windows.Forms.TextBox ();
      machineModuleLabel3 = new System.Windows.Forms.Label ();
      prefixTextBox3 = new System.Windows.Forms.TextBox ();
      parametersTextBox3 = new System.Windows.Forms.TextBox ();
      machineModuleLabel4 = new System.Windows.Forms.Label ();
      prefixTextBox4 = new System.Windows.Forms.TextBox ();
      parametersTextBox4 = new System.Windows.Forms.TextBox ();
      cncAcquisitionGroupBox.SuspendLayout ();
      machineModulesGroupBox.SuspendLayout ();
      tableLayoutPanel1.SuspendLayout ();
      SuspendLayout ();
      // 
      // cncAcquisitionGroupBox
      // 
      cncAcquisitionGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      cncAcquisitionGroupBox.Controls.Add (keyParamsTextBox);
      cncAcquisitionGroupBox.Controls.Add (fileRepositorySelection1);
      cncAcquisitionGroupBox.Controls.Add (nameTextBox);
      cncAcquisitionGroupBox.Controls.Add (label1);
      cncAcquisitionGroupBox.Controls.Add (parametersTextBox);
      cncAcquisitionGroupBox.Controls.Add (parametersLabel);
      cncAcquisitionGroupBox.Controls.Add (prefixTextBox);
      cncAcquisitionGroupBox.Controls.Add (prefixLabel);
      cncAcquisitionGroupBox.Location = new System.Drawing.Point (15, 15);
      cncAcquisitionGroupBox.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      cncAcquisitionGroupBox.Name = "cncAcquisitionGroupBox";
      cncAcquisitionGroupBox.Padding = new System.Windows.Forms.Padding (4, 3, 4, 3);
      cncAcquisitionGroupBox.Size = new System.Drawing.Size (410, 349);
      cncAcquisitionGroupBox.TabIndex = 0;
      cncAcquisitionGroupBox.TabStop = false;
      cncAcquisitionGroupBox.Text = "Cnc Acquisition";
      // 
      // keyParamsTextBox
      // 
      keyParamsTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      keyParamsTextBox.Location = new System.Drawing.Point (8, 314);
      keyParamsTextBox.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      keyParamsTextBox.Name = "keyParamsTextBox";
      keyParamsTextBox.Size = new System.Drawing.Size (394, 23);
      keyParamsTextBox.TabIndex = 9;
      // 
      // fileRepositorySelection1
      // 
      fileRepositorySelection1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      fileRepositorySelection1.AutoScroll = true;
      fileRepositorySelection1.Location = new System.Drawing.Point (8, 22);
      fileRepositorySelection1.Margin = new System.Windows.Forms.Padding (5, 3, 5, 3);
      fileRepositorySelection1.Name = "fileRepositorySelection1";
      fileRepositorySelection1.NSpace = "cncconfigs";
      fileRepositorySelection1.Size = new System.Drawing.Size (227, 257);
      fileRepositorySelection1.TabIndex = 8;
      // 
      // nameTextBox
      // 
      nameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      nameTextBox.Location = new System.Drawing.Point (243, 48);
      nameTextBox.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      nameTextBox.Name = "nameTextBox";
      nameTextBox.Size = new System.Drawing.Size (159, 23);
      nameTextBox.TabIndex = 7;
      nameTextBox.Text = "Test";
      // 
      // label1
      // 
      label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      label1.Location = new System.Drawing.Point (243, 18);
      label1.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size (46, 27);
      label1.TabIndex = 6;
      label1.Text = "Name";
      label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // parametersTextBox
      // 
      parametersTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      parametersTextBox.Location = new System.Drawing.Point (9, 285);
      parametersTextBox.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      parametersTextBox.Name = "parametersTextBox";
      parametersTextBox.Size = new System.Drawing.Size (394, 23);
      parametersTextBox.TabIndex = 5;
      // 
      // parametersLabel
      // 
      parametersLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      parametersLabel.Location = new System.Drawing.Point (271, 254);
      parametersLabel.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      parametersLabel.Name = "parametersLabel";
      parametersLabel.Size = new System.Drawing.Size (135, 25);
      parametersLabel.TabIndex = 4;
      parametersLabel.Text = "Parameters";
      parametersLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // prefixTextBox
      // 
      prefixTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      prefixTextBox.Location = new System.Drawing.Point (243, 105);
      prefixTextBox.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      prefixTextBox.Name = "prefixTextBox";
      prefixTextBox.Size = new System.Drawing.Size (159, 23);
      prefixTextBox.TabIndex = 1;
      // 
      // prefixLabel
      // 
      prefixLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      prefixLabel.Location = new System.Drawing.Point (243, 75);
      prefixLabel.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      prefixLabel.Name = "prefixLabel";
      prefixLabel.Size = new System.Drawing.Size (46, 27);
      prefixLabel.TabIndex = 0;
      prefixLabel.Text = "Prefix";
      prefixLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // okButton
      // 
      okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      okButton.Location = new System.Drawing.Point (337, 562);
      okButton.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      okButton.Name = "okButton";
      okButton.Size = new System.Drawing.Size (88, 27);
      okButton.TabIndex = 1;
      okButton.Text = "OK";
      okButton.UseVisualStyleBackColor = true;
      okButton.Click += OkButtonClick;
      // 
      // machineModulesGroupBox
      // 
      machineModulesGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      machineModulesGroupBox.Controls.Add (tableLayoutPanel1);
      machineModulesGroupBox.Location = new System.Drawing.Point (15, 370);
      machineModulesGroupBox.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      machineModulesGroupBox.Name = "machineModulesGroupBox";
      machineModulesGroupBox.Padding = new System.Windows.Forms.Padding (4, 3, 4, 3);
      machineModulesGroupBox.Size = new System.Drawing.Size (410, 185);
      machineModulesGroupBox.TabIndex = 2;
      machineModulesGroupBox.TabStop = false;
      machineModulesGroupBox.Text = "Machine modules";
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
      tableLayoutPanel1.ColumnCount = 3;
      tableLayoutPanel1.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 50F));
      tableLayoutPanel1.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Absolute, 117F));
      tableLayoutPanel1.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 50F));
      tableLayoutPanel1.Controls.Add (label2, 0, 0);
      tableLayoutPanel1.Controls.Add (label3, 1, 0);
      tableLayoutPanel1.Controls.Add (label4, 2, 0);
      tableLayoutPanel1.Controls.Add (machineModuleLabel1, 0, 1);
      tableLayoutPanel1.Controls.Add (prefixTextBox1, 1, 1);
      tableLayoutPanel1.Controls.Add (parametersTextBox1, 2, 1);
      tableLayoutPanel1.Controls.Add (machineModuleLabel2, 0, 2);
      tableLayoutPanel1.Controls.Add (prefixTextBox2, 1, 2);
      tableLayoutPanel1.Controls.Add (parametersTextBox2, 2, 2);
      tableLayoutPanel1.Controls.Add (machineModuleLabel3, 0, 3);
      tableLayoutPanel1.Controls.Add (prefixTextBox3, 1, 3);
      tableLayoutPanel1.Controls.Add (parametersTextBox3, 2, 3);
      tableLayoutPanel1.Controls.Add (machineModuleLabel4, 0, 4);
      tableLayoutPanel1.Controls.Add (prefixTextBox4, 1, 4);
      tableLayoutPanel1.Controls.Add (parametersTextBox4, 2, 4);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel1.Location = new System.Drawing.Point (4, 19);
      tableLayoutPanel1.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 5;
      tableLayoutPanel1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 20F));
      tableLayoutPanel1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 20F));
      tableLayoutPanel1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 20F));
      tableLayoutPanel1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 20F));
      tableLayoutPanel1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 20F));
      tableLayoutPanel1.Size = new System.Drawing.Size (402, 163);
      tableLayoutPanel1.TabIndex = 0;
      // 
      // label2
      // 
      label2.Dock = System.Windows.Forms.DockStyle.Fill;
      label2.Location = new System.Drawing.Point (5, 1);
      label2.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size (132, 31);
      label2.TabIndex = 0;
      label2.Text = "Machine module";
      label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      label3.Dock = System.Windows.Forms.DockStyle.Fill;
      label3.Location = new System.Drawing.Point (146, 1);
      label3.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size (109, 31);
      label3.TabIndex = 1;
      label3.Text = "Prefix";
      label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      label4.Dock = System.Windows.Forms.DockStyle.Fill;
      label4.Location = new System.Drawing.Point (264, 1);
      label4.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      label4.Name = "label4";
      label4.Size = new System.Drawing.Size (133, 31);
      label4.TabIndex = 2;
      label4.Text = "Parameters";
      label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // machineModuleLabel1
      // 
      machineModuleLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
      machineModuleLabel1.Location = new System.Drawing.Point (5, 33);
      machineModuleLabel1.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      machineModuleLabel1.Name = "machineModuleLabel1";
      machineModuleLabel1.Size = new System.Drawing.Size (132, 31);
      machineModuleLabel1.TabIndex = 3;
      machineModuleLabel1.Click += MachineModuleLabel1Click;
      // 
      // prefixTextBox1
      // 
      prefixTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      prefixTextBox1.Location = new System.Drawing.Point (146, 36);
      prefixTextBox1.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      prefixTextBox1.Name = "prefixTextBox1";
      prefixTextBox1.Size = new System.Drawing.Size (109, 23);
      prefixTextBox1.TabIndex = 4;
      // 
      // parametersTextBox1
      // 
      parametersTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      parametersTextBox1.Location = new System.Drawing.Point (264, 36);
      parametersTextBox1.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      parametersTextBox1.Name = "parametersTextBox1";
      parametersTextBox1.Size = new System.Drawing.Size (133, 23);
      parametersTextBox1.TabIndex = 5;
      // 
      // machineModuleLabel2
      // 
      machineModuleLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
      machineModuleLabel2.Location = new System.Drawing.Point (5, 65);
      machineModuleLabel2.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      machineModuleLabel2.Name = "machineModuleLabel2";
      machineModuleLabel2.Size = new System.Drawing.Size (132, 31);
      machineModuleLabel2.TabIndex = 3;
      machineModuleLabel2.Click += MachineModuleLabel2Click;
      // 
      // prefixTextBox2
      // 
      prefixTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      prefixTextBox2.Location = new System.Drawing.Point (146, 68);
      prefixTextBox2.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      prefixTextBox2.Name = "prefixTextBox2";
      prefixTextBox2.Size = new System.Drawing.Size (109, 23);
      prefixTextBox2.TabIndex = 4;
      // 
      // parametersTextBox2
      // 
      parametersTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      parametersTextBox2.Location = new System.Drawing.Point (264, 68);
      parametersTextBox2.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      parametersTextBox2.Name = "parametersTextBox2";
      parametersTextBox2.Size = new System.Drawing.Size (133, 23);
      parametersTextBox2.TabIndex = 5;
      // 
      // machineModuleLabel3
      // 
      machineModuleLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
      machineModuleLabel3.Location = new System.Drawing.Point (5, 97);
      machineModuleLabel3.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      machineModuleLabel3.Name = "machineModuleLabel3";
      machineModuleLabel3.Size = new System.Drawing.Size (132, 31);
      machineModuleLabel3.TabIndex = 3;
      machineModuleLabel3.Click += MachineModuleLabel3Click;
      // 
      // prefixTextBox3
      // 
      prefixTextBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      prefixTextBox3.Location = new System.Drawing.Point (146, 100);
      prefixTextBox3.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      prefixTextBox3.Name = "prefixTextBox3";
      prefixTextBox3.Size = new System.Drawing.Size (109, 23);
      prefixTextBox3.TabIndex = 4;
      // 
      // parametersTextBox3
      // 
      parametersTextBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      parametersTextBox3.Location = new System.Drawing.Point (264, 100);
      parametersTextBox3.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      parametersTextBox3.Name = "parametersTextBox3";
      parametersTextBox3.Size = new System.Drawing.Size (133, 23);
      parametersTextBox3.TabIndex = 5;
      // 
      // machineModuleLabel4
      // 
      machineModuleLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
      machineModuleLabel4.Location = new System.Drawing.Point (5, 129);
      machineModuleLabel4.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
      machineModuleLabel4.Name = "machineModuleLabel4";
      machineModuleLabel4.Size = new System.Drawing.Size (132, 33);
      machineModuleLabel4.TabIndex = 3;
      machineModuleLabel4.Click += MachineModuleLabel4Click;
      // 
      // prefixTextBox4
      // 
      prefixTextBox4.Dock = System.Windows.Forms.DockStyle.Fill;
      prefixTextBox4.Location = new System.Drawing.Point (146, 132);
      prefixTextBox4.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      prefixTextBox4.Name = "prefixTextBox4";
      prefixTextBox4.Size = new System.Drawing.Size (109, 23);
      prefixTextBox4.TabIndex = 4;
      // 
      // parametersTextBox4
      // 
      parametersTextBox4.Dock = System.Windows.Forms.DockStyle.Fill;
      parametersTextBox4.Location = new System.Drawing.Point (264, 132);
      parametersTextBox4.Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      parametersTextBox4.Name = "parametersTextBox4";
      parametersTextBox4.Size = new System.Drawing.Size (133, 23);
      parametersTextBox4.TabIndex = 5;
      // 
      // ConfigParamDialog
      // 
      AutoScaleDimensions = new System.Drawing.SizeF (7F, 15F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      ClientSize = new System.Drawing.Size (439, 602);
      Controls.Add (machineModulesGroupBox);
      Controls.Add (okButton);
      Controls.Add (cncAcquisitionGroupBox);
      Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "ConfigParamDialog";
      ShowInTaskbar = false;
      Text = "Cnc Config+Param";
      cncAcquisitionGroupBox.ResumeLayout (false);
      cncAcquisitionGroupBox.PerformLayout ();
      machineModulesGroupBox.ResumeLayout (false);
      tableLayoutPanel1.ResumeLayout (false);
      tableLayoutPanel1.PerformLayout ();
      ResumeLayout (false);
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
    private System.Windows.Forms.TextBox keyParamsTextBox;
  }
}
