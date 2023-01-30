// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lem_LanguageSelector
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    #region Members
    bool defaultLanguage;
    CultureInfo defaultCultureInfo;
    CultureInfo selectedCultureInfo;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (MainForm).FullName);

    #region Getters / Setters
    public CultureInfo SelectedCultureInfo {
      get { return selectedCultureInfo; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      // Add constructor code after the InitializeComponent() call.
      FillAlternativeLanguageComboBox ();
      
      // - Initialization of the members
      Debug.Assert (CultureInfo.CurrentUICulture.Equals (CultureInfo.InstalledUICulture));
      defaultCultureInfo = CultureInfo.CurrentUICulture;
      string languageInRegistry = Lemoine.I18N.TargetSpecific.LocaleSettings.GetLanguageInRegistry ();
      if (languageInRegistry == null) {
        defaultLanguage = true;
        selectedCultureInfo = CultureInfo.CurrentUICulture;
      }
      else {
        try {
          defaultLanguage = false;
          selectedCultureInfo = new CultureInfo (languageInRegistry);
        }
        catch (Exception e) {
          log.Error (e);
          defaultLanguage = true;
          selectedCultureInfo = CultureInfo.CurrentUICulture;
        }
      }

      // - Initialization of the controls
      computerLanguageCheckBox.Text = "Use the default language: "
        + defaultCultureInfo.DisplayName;
      alternativeLanguageLabel.Text = "Or use an alternative language: ";
      alternativeLanguageComboBox.SelectedItem = selectedCultureInfo;
      codeLabel.Text = "Language code: "
        + selectedCultureInfo.Name;
      if (defaultLanguage == true) {
        computerLanguageCheckBox.Checked = true;
        alternativeLanguageLabel.Enabled = false;
        alternativeLanguageComboBox.Enabled = false;
      }
      else {
        computerLanguageCheckBox.Checked = false;
        alternativeLanguageLabel.Enabled = true;
        alternativeLanguageComboBox.Enabled = true;
      }
    }
    #endregion

    #region Methods
    private void FillAlternativeLanguageComboBox ()
    {
      CultureInfo[] languageList = CultureInfo.GetCultures (CultureTypes.SpecificCultures);
      alternativeLanguageComboBox.Sorted = true;
      alternativeLanguageComboBox.DataSource = languageList;
      alternativeLanguageComboBox.DisplayMember = "DisplayName";
      alternativeLanguageComboBox.ValueMember = "Name";
    }
    #endregion
    
    void ComputerLanguageCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      defaultLanguage = computerLanguageCheckBox.Checked;
      if (defaultLanguage == true) {
        computerLanguageCheckBox.Checked = true;
        alternativeLanguageLabel.Enabled = false;
        alternativeLanguageComboBox.Enabled = false;
        selectedCultureInfo = defaultCultureInfo;
      }
      else {
        computerLanguageCheckBox.Checked = false;
        alternativeLanguageLabel.Enabled = true;
        alternativeLanguageComboBox.Enabled = true;
        selectedCultureInfo = (CultureInfo) alternativeLanguageComboBox.SelectedItem;
      }
      codeLabel.Text = "Language code: "
        + selectedCultureInfo.Name;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      Application.Exit ();
    }
    
    
    
    void OkButtonClick(object sender, EventArgs e)
    {
      // Save
      if (defaultLanguage == true) {
        if (false == Lemoine.I18N.TargetSpecific.LocaleSettings.ResetLanguageInRegistry ()) {
          MessageBox.Show ("Error while resetting the language setting. \n" +
                           "Please check you have the Administrator privilege.",
                           "Language setting",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Error);
          return;
        }
      }
      else {
        if (false == Lemoine.I18N.TargetSpecific.LocaleSettings.SetLanguageInRegistry (selectedCultureInfo.Name)) {
          MessageBox.Show ("Error while setting the language. \n" +
                           "Please check you have the Administrator privilege.",
                           "Language setting",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Error);
          return;
        }
      }
      
      Application.Exit ();
    }
    
    void AlternativeLanguageComboBoxSelectedValueChanged(object sender, EventArgs e)
    {
      selectedCultureInfo = (CultureInfo) alternativeLanguageComboBox.SelectedItem;
      codeLabel.Text = "Language code: "
        + selectedCultureInfo.Name;
    }
  }
}
