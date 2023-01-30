// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lem_Settings
{
  /// <summary>
  /// Description of UsageDialog.
  /// </summary>
  public partial class UsageDialog : Form
  {
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="text">Text to display</param>
    public UsageDialog(string text)
    {
      InitializeComponent();
      richTextBox.Text = text;
    }
    #endregion // Constructors
    
    #region Event reactions
    void ButtonOkClick(object sender, EventArgs e)
    {
      this.Close();
    }
    #endregion // Event reactions
  }
}
