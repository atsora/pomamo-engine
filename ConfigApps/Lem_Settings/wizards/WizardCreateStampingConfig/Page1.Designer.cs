// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardCreateStampingConfig
{
  partial class Page1
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing && (components != null)) {
        components.Dispose ();
      }
      base.Dispose (disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ()
    {
      this.panel1 = new System.Windows.Forms.Panel ();
      this.existingLabel = new System.Windows.Forms.Label ();
      this.newLabel = new System.Windows.Forms.Label ();
      this.nameTextBox = new System.Windows.Forms.TextBox ();
      this.templateSelectButton = new System.Windows.Forms.Button ();
      this.nameLabel = new System.Windows.Forms.Label ();
      this.templatePathLabel = new System.Windows.Forms.Label ();
      this.templatePathTextBox = new System.Windows.Forms.TextBox ();
      this.templateOpenFileDialog = new System.Windows.Forms.OpenFileDialog ();
      this.stampingConfigSelection = new Lemoine.DataReferenceControls.StampingConfigSelection ();
      this.panel1.SuspendLayout ();
      this.SuspendLayout ();
      // 
      // panel1
      // 
      this.panel1.Controls.Add (this.stampingConfigSelection);
      this.panel1.Controls.Add (this.existingLabel);
      this.panel1.Controls.Add (this.newLabel);
      this.panel1.Controls.Add (this.nameTextBox);
      this.panel1.Controls.Add (this.templateSelectButton);
      this.panel1.Controls.Add (this.nameLabel);
      this.panel1.Controls.Add (this.templatePathLabel);
      this.panel1.Controls.Add (this.templatePathTextBox);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point (0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size (577, 300);
      this.panel1.TabIndex = 0;
      // 
      // existingLabel
      // 
      this.existingLabel.AutoSize = true;
      this.existingLabel.Font = new System.Drawing.Font ("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.existingLabel.Location = new System.Drawing.Point (3, 101);
      this.existingLabel.Name = "existingLabel";
      this.existingLabel.Size = new System.Drawing.Size (50, 15);
      this.existingLabel.TabIndex = 6;
      this.existingLabel.Text = "Existing";
      // 
      // newLabel
      // 
      this.newLabel.AutoSize = true;
      this.newLabel.Font = new System.Drawing.Font ("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.newLabel.Location = new System.Drawing.Point (3, 11);
      this.newLabel.Name = "newLabel";
      this.newLabel.Size = new System.Drawing.Size (33, 15);
      this.newLabel.TabIndex = 5;
      this.newLabel.Text = "New";
      // 
      // nameTextBox
      // 
      this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.nameTextBox.Location = new System.Drawing.Point (277, 30);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size (297, 23);
      this.nameTextBox.TabIndex = 1;
      this.nameTextBox.TextChanged += new System.EventHandler (this.nameTextBox_TextChanged);
      // 
      // templateSelectButton
      // 
      this.templateSelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.templateSelectButton.Location = new System.Drawing.Point (499, 63);
      this.templateSelectButton.Name = "templateSelectButton";
      this.templateSelectButton.Size = new System.Drawing.Size (75, 23);
      this.templateSelectButton.TabIndex = 4;
      this.templateSelectButton.Text = "Select...";
      this.templateSelectButton.UseVisualStyleBackColor = true;
      this.templateSelectButton.Click += new System.EventHandler (this.templateOpenButton_Click);
      // 
      // nameLabel
      // 
      this.nameLabel.AutoSize = true;
      this.nameLabel.Location = new System.Drawing.Point (3, 33);
      this.nameLabel.Name = "nameLabel";
      this.nameLabel.Size = new System.Drawing.Size (253, 15);
      this.nameLabel.TabIndex = 0;
      this.nameLabel.Text = "Name of the stamping config (post-processor)";
      this.nameLabel.Click += new System.EventHandler (this.nameLabel_Click);
      // 
      // templatePathLabel
      // 
      this.templatePathLabel.AutoSize = true;
      this.templatePathLabel.Location = new System.Drawing.Point (3, 63);
      this.templatePathLabel.Name = "templatePathLabel";
      this.templatePathLabel.Size = new System.Drawing.Size (82, 15);
      this.templatePathLabel.TabIndex = 2;
      this.templatePathLabel.Text = "Template path";
      this.templatePathLabel.Click += new System.EventHandler (this.templatePathLabel_Click);
      // 
      // templatePathTextBox
      // 
      this.templatePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.templatePathTextBox.Location = new System.Drawing.Point (277, 61);
      this.templatePathTextBox.Name = "templatePathTextBox";
      this.templatePathTextBox.ReadOnly = true;
      this.templatePathTextBox.Size = new System.Drawing.Size (216, 23);
      this.templatePathTextBox.TabIndex = 3;
      // 
      // templateOpenFileDialog
      // 
      this.templateOpenFileDialog.DefaultExt = "json";
      this.templateOpenFileDialog.Filter = "Json Files (*.json)|*.json";
      this.templateOpenFileDialog.Title = "Template selection";
      // 
      // stampingConfigSelection
      // 
      this.stampingConfigSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.stampingConfigSelection.Location = new System.Drawing.Point (3, 119);
      this.stampingConfigSelection.Name = "stampingConfigSelection";
      this.stampingConfigSelection.Size = new System.Drawing.Size (571, 178);
      this.stampingConfigSelection.TabIndex = 7;
      this.stampingConfigSelection.MultiSelect = false;
      this.stampingConfigSelection.Nullable = true;
      this.stampingConfigSelection.AfterSelect += new System.EventHandler (this.stampingConfigSelection_AfterSelect);
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add (this.panel1);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size (577, 300);
      this.panel1.ResumeLayout (false);
      this.panel1.PerformLayout ();
      this.ResumeLayout (false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label nameLabel;
    private System.Windows.Forms.TextBox nameTextBox;
    private System.Windows.Forms.Label templatePathLabel;
    private System.Windows.Forms.Button templateSelectButton;
    private System.Windows.Forms.TextBox templatePathTextBox;
    private System.Windows.Forms.OpenFileDialog templateOpenFileDialog;
    private System.Windows.Forms.Label newLabel;
    private System.Windows.Forms.Label existingLabel;
    private Lemoine.DataReferenceControls.StampingConfigSelection stampingConfigSelection;
  }
}
