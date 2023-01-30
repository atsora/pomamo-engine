// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lem_Settings
{
  /// <summary>
  /// Description of TextDialog.
  /// </summary>
  public partial class TextDialog : Form
  {
    #region Getters / Setters
    /// <summary>
    /// Displayed text
    /// </summary>
    public string DisplayedText {
      get { return richTextBox.Text; }
      set { richTextBox.Text = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TextDialog()
    {
      InitializeComponent();
    }
    #endregion // Constructors
    
    #region Event reactions
    void ButtonCopyClick(object sender, EventArgs e)
    {
      Clipboard.SetText(DisplayedText);
      labelSuccess.Show();
    }
    
    void ButtonCloseClick(object sender, EventArgs e)
    {
      this.Close();
    }
    #endregion // Event reactions
  }
}
