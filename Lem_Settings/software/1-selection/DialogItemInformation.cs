// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Windows.Forms;
using Lemoine.ModelDAO;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Description of DialogItemInformation.
  /// </summary>
  public partial class DialogItemInformation : Form
  {
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="item"></param>
    public DialogItemInformation(IItem item)
    {
      InitializeComponent();
      
      // Title and identification
      item.Context.ViewMode = false;
      labelTitle.Text = item.Title;
      labelId.Text = item.ID + "." + item.SubID;
      
      // View mode, for configurators
      if (item is IConfigurator) {
        baseLayout.RowStyles[3].Height = 25;
        if ((item.Flags & LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED) != 0) {
          labelViewMode.Text = "Available";
        }
        else {
          labelViewMode.Text = "Not available";
        }
      } else {
        baseLayout.RowStyles[3].Height = 0;
      }
      
      // Path of the dll and possibly the ini file
      labelDllPath.Text = item.DllPath;
      labelIniPath.Text = item.IniPath;
      if (labelIniPath.Text == "") {
        labelIniPath.Text = "-";
      }
    }
    #endregion // Constructors
  }
}
