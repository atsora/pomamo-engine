// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Windows.Forms;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of FileSelectorFallback.
  /// </summary>
  public partial class FileSelectorFallback : UserControl
  {
    #region Getters / Setters
    /// <summary>
    /// Selected value (name of the file without the path)
    /// </summary>
    public string SelectedValue {
      get { return textBox.Text; }
      set { textBox.Text = value ?? ""; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="directoryPath">root directory</param>
    /// <param name="subDirectoryName">sub directory (can be null or empty)</param>
    /// <param name="filter">extensions allowed when browsing files</param>
    /// <param name="defaultValue">default file name (can be null or empty)</param>
    public FileSelectorFallback(string directoryPath, string subDirectoryName, string filter, string defaultValue)
    {
      InitializeComponent();
      
      // Initialization of the value
      SelectedValue = defaultValue;
      
      // Warning tooltip
      var toolTip = new ToolTip();
      toolTip.SetToolTip(pictureBox, "Cannot find or read files in directory '" +
                         Path.Combine(directoryPath, subDirectoryName) +
                         "', you could try to open LemSettings with the admin rights.");
      toolTip.AutoPopDelay = 32000;
    }
    #endregion // Constructors
  }
}
