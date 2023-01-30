// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Translator
{
  partial class CultureSelectionForm
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
    	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CultureSelectionForm));
    	this.languageLabel = new System.Windows.Forms.Label();
    	this.cultureOkButton = new System.Windows.Forms.Button();
    	this.languageComboBox = new System.Windows.Forms.ComboBox();
    	this.localeLabel = new System.Windows.Forms.Label();
    	this.countryLabel = new System.Windows.Forms.Label();
    	this.regionCB = new System.Windows.Forms.CheckBox();
    	this.SuspendLayout();
    	// 
    	// languageLabel
    	// 
    	resources.ApplyResources(this.languageLabel, "languageLabel");
    	this.languageLabel.Name = "languageLabel";
    	// 
    	// cultureOkButton
    	// 
    	resources.ApplyResources(this.cultureOkButton, "cultureOkButton");
    	this.cultureOkButton.Name = "cultureOkButton";
    	this.cultureOkButton.UseVisualStyleBackColor = true;
    	this.cultureOkButton.Click += new System.EventHandler(this.CultureOkButtonClick);
    	// 
    	// languageComboBox
    	// 
    	this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
    	this.languageComboBox.FormattingEnabled = true;
    	resources.ApplyResources(this.languageComboBox, "languageComboBox");
    	this.languageComboBox.Name = "languageComboBox";
    	this.languageComboBox.Sorted = true;
    	this.languageComboBox.SelectedValueChanged += new System.EventHandler(this.LanguageComboBoxSelectedValueChanged);
    	// 
    	// localeLabel
    	// 
    	resources.ApplyResources(this.localeLabel, "localeLabel");
    	this.localeLabel.Name = "localeLabel";
    	// 
    	// countryLabel
    	// 
    	resources.ApplyResources(this.countryLabel, "countryLabel");
    	this.countryLabel.Name = "countryLabel";
    	// 
    	// regionCB
    	// 
    	this.regionCB.Checked = true;
    	this.regionCB.CheckState = System.Windows.Forms.CheckState.Checked;
    	resources.ApplyResources(this.regionCB, "regionCB");
    	this.regionCB.Name = "regionCB";
    	this.regionCB.UseVisualStyleBackColor = true;
    	this.regionCB.CheckedChanged += new System.EventHandler(this.RegionCBCheckedChanged);
    	// 
    	// CultureSelectionForm
    	// 
    	resources.ApplyResources(this, "$this");
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.ControlBox = false;
    	this.Controls.Add(this.regionCB);
    	this.Controls.Add(this.countryLabel);
    	this.Controls.Add(this.localeLabel);
    	this.Controls.Add(this.languageComboBox);
    	this.Controls.Add(this.cultureOkButton);
    	this.Controls.Add(this.languageLabel);
    	this.MaximizeBox = false;
    	this.MinimizeBox = false;
    	this.Name = "CultureSelectionForm";
    	this.ShowInTaskbar = false;
    	this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
    	this.Load += new System.EventHandler(this.CultureSelectionFormLoad);
    	this.ResumeLayout(false);
    }
    private System.Windows.Forms.CheckBox regionCB;
    private System.Windows.Forms.Label localeLabel;
    private System.Windows.Forms.ComboBox languageComboBox;
    private System.Windows.Forms.Button cultureOkButton;
    private System.Windows.Forms.Label countryLabel;
    private System.Windows.Forms.Label languageLabel;
  }
}
