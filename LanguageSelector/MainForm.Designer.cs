// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_LanguageSelector
{
  partial class MainForm
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
    	this.computerLanguageCheckBox = new System.Windows.Forms.CheckBox();
    	this.alternativeLanguageLabel = new System.Windows.Forms.Label();
    	this.alternativeLanguageComboBox = new System.Windows.Forms.ComboBox();
    	this.codeLabel = new System.Windows.Forms.Label();
    	this.cancelButton = new System.Windows.Forms.Button();
    	this.okButton = new System.Windows.Forms.Button();
    	this.SuspendLayout();
    	// 
    	// computerLanguageCheckBox
    	// 
    	this.computerLanguageCheckBox.Location = new System.Drawing.Point(13, 13);
    	this.computerLanguageCheckBox.Name = "computerLanguageCheckBox";
    	this.computerLanguageCheckBox.Size = new System.Drawing.Size(328, 24);
    	this.computerLanguageCheckBox.TabIndex = 0;
    	this.computerLanguageCheckBox.Text = "computerLanguageCheckBox";
    	this.computerLanguageCheckBox.UseVisualStyleBackColor = true;
    	this.computerLanguageCheckBox.CheckedChanged += new System.EventHandler(this.ComputerLanguageCheckBoxCheckedChanged);
    	// 
    	// alternativeLanguageLabel
    	// 
    	this.alternativeLanguageLabel.Location = new System.Drawing.Point(13, 44);
    	this.alternativeLanguageLabel.Name = "alternativeLanguageLabel";
    	this.alternativeLanguageLabel.Size = new System.Drawing.Size(328, 23);
    	this.alternativeLanguageLabel.TabIndex = 1;
    	this.alternativeLanguageLabel.Text = "alternativeLanguageLabel";
    	// 
    	// alternativeLanguageComboBox
    	// 
    	this.alternativeLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
    	this.alternativeLanguageComboBox.FormattingEnabled = true;
    	this.alternativeLanguageComboBox.Location = new System.Drawing.Point(13, 71);
    	this.alternativeLanguageComboBox.Name = "alternativeLanguageComboBox";
    	this.alternativeLanguageComboBox.Size = new System.Drawing.Size(328, 21);
    	this.alternativeLanguageComboBox.TabIndex = 2;
    	this.alternativeLanguageComboBox.SelectedValueChanged += new System.EventHandler(this.AlternativeLanguageComboBoxSelectedValueChanged);
    	// 
    	// codeLabel
    	// 
    	this.codeLabel.Location = new System.Drawing.Point(13, 109);
    	this.codeLabel.Name = "codeLabel";
    	this.codeLabel.Size = new System.Drawing.Size(328, 23);
    	this.codeLabel.TabIndex = 3;
    	this.codeLabel.Text = "codeLabel";
    	// 
    	// cancelButton
    	// 
    	this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
    	    	    	| System.Windows.Forms.AnchorStyles.Right)));
    	this.cancelButton.Location = new System.Drawing.Point(82, 135);
    	this.cancelButton.Name = "cancelButton";
    	this.cancelButton.Size = new System.Drawing.Size(75, 23);
    	this.cancelButton.TabIndex = 4;
    	this.cancelButton.Text = "Cancel";
    	this.cancelButton.UseVisualStyleBackColor = true;
    	this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
    	// 
    	// okButton
    	// 
    	this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
    	    	    	| System.Windows.Forms.AnchorStyles.Right)));
    	this.okButton.Location = new System.Drawing.Point(196, 135);
    	this.okButton.Name = "okButton";
    	this.okButton.Size = new System.Drawing.Size(75, 23);
    	this.okButton.TabIndex = 5;
    	this.okButton.Text = "OK";
    	this.okButton.UseVisualStyleBackColor = true;
    	this.okButton.Click += new System.EventHandler(this.OkButtonClick);
    	// 
    	// MainForm
    	// 
    	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.ClientSize = new System.Drawing.Size(353, 167);
    	this.Controls.Add(this.okButton);
    	this.Controls.Add(this.cancelButton);
    	this.Controls.Add(this.codeLabel);
    	this.Controls.Add(this.alternativeLanguageComboBox);
    	this.Controls.Add(this.alternativeLanguageLabel);
    	this.Controls.Add(this.computerLanguageCheckBox);
    	this.Name = "MainForm";
    	this.Text = "Language Selector";
    	this.ResumeLayout(false);
    }
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Label codeLabel;
    private System.Windows.Forms.ComboBox alternativeLanguageComboBox;
    private System.Windows.Forms.Label alternativeLanguageLabel;
    private System.Windows.Forms.CheckBox computerLanguageCheckBox;
  }
}
