// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Description of EditPreferences.
  /// </summary>
  public partial class EditPreferences : Form
  {
    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EditPreferences()
    {
      this.StartPosition = FormStartPosition.CenterParent;
      this.Icon = Form.ActiveForm.Icon;
      InitializeComponent();
      InitGUI();
      LoadConfiguration();
    }
    #endregion // Constructors

    #region Methods
    void InitGUI()
    {
      comboPerfUnit.Items.Add("parts per hour");
      comboPerfUnit.Items.Add("parts per minute");
      comboPerfUnit.Items.Add("seconds per part");
      comboPerfUnit.Items.Add("minutes per part");
    }
    
    void LoadConfiguration()
    {
      string perfUnit = IniFilePreferences.Get(IniFilePreferences.Field.PERFORMANCE_UNIT);
      if (perfUnit == "parts/hour") {
        comboPerfUnit.SelectedIndex = 0;
      }
      else if (perfUnit == "parts/minute") {
        comboPerfUnit.SelectedIndex = 1;
      }
      else if (perfUnit == "seconds/part") {
        comboPerfUnit.SelectedIndex = 2;
      }
      else if (perfUnit == "minutes/part") {
        comboPerfUnit.SelectedIndex = 3;
      }
      else {
        comboPerfUnit.SelectedIndex = 0;
      }
    }
    
    void SaveConfiguration()
    {
      switch (comboPerfUnit.SelectedIndex) {
        case 1:
          IniFilePreferences.Set(IniFilePreferences.Field.PERFORMANCE_UNIT, "parts/minute");
          break;
        case 2:
          IniFilePreferences.Set(IniFilePreferences.Field.PERFORMANCE_UNIT, "seconds/part");
          break;
        case 3:
          IniFilePreferences.Set(IniFilePreferences.Field.PERFORMANCE_UNIT, "minutes/part");
          break;
        default:
          IniFilePreferences.Set(IniFilePreferences.Field.PERFORMANCE_UNIT, "parts/hour");
          break;
      }
    }
    #endregion // Methods
    
    #region Event reactions
    void ButtonCancelClick(object sender, EventArgs e)
    {
      Close();
    }
    
    void ButtonOkClick(object sender, EventArgs e)
    {
      SaveConfiguration();
      Close();
    }
    #endregion // Event reactions
  }
}
