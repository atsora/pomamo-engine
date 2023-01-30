// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;

namespace Lem_Translator
{
  /// <summary>
  /// Description of CultureSelectionForm.
  /// </summary>
  public partial class CultureSelectionForm : Form
  {
    #region Members
    private CultureInfo selectedCulture;
    #endregion
    
    #region Constructors
    public CultureSelectionForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      selectedCulture = CultureInfo.CurrentCulture;
    }
    #endregion
    
    #region Getters / Setters
    public bool HideRegionSpecificLocales {
      set { this.regionCB.Checked = !value; }
    }
    #endregion
    
    #region Methods
    public CultureInfo SelectedCulture { get {return selectedCulture;} }
    
    void LanguageComboBoxSelectedValueChanged(object sender, EventArgs e)
    {
      selectedCulture = (CultureInfo) languageComboBox.SelectedItem;
      localeLabel.Text = "Locale code: " + selectedCulture.Name;
      countryLabel.Text = "Three-letter language code: " + selectedCulture.ThreeLetterISOLanguageName;
    }
    
    void CultureOkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close ();
      return;
    }

    void RegionCBCheckedChanged(object sender, EventArgs e)
    {
      UpdateLanguageComboBox ();
    }
    
    void UpdateLanguageComboBox ()
    {
      CultureInfo cultureToSelect = this.SelectedCulture;
      CultureInfo[] languageList;
      if (regionCB.Checked) {
        languageList = CultureInfo.GetCultures (CultureTypes.AllCultures);
      }
      else {
        languageList = CultureInfo.GetCultures (CultureTypes.NeutralCultures);
        cultureToSelect = new CultureInfo (cultureToSelect.TwoLetterISOLanguageName);
      }
      languageComboBox.DataSource = languageList;
      languageComboBox.DisplayMember = "DisplayName";
      languageComboBox.ValueMember = "Name";
      languageComboBox.SelectedItem = cultureToSelect;
    }
    #endregion
    
    void CultureSelectionFormLoad(object sender, EventArgs e)
    {
      UpdateLanguageComboBox ();
    }
  }
}
