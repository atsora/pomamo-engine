// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lem_Settings
{
  /// <summary>
  /// Description of ErrorDialog.
  /// </summary>
  public partial class ErrorDialog : Form
  {
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="text">Text to display</param>
    /// <param name="quitIfException">If true "Quit" is written. Otherwise "Ok"</param>
    public ErrorDialog(string text, bool quitIfException)
    {
      InitializeComponent();
      richText.Text = text;
      if (!quitIfException) {
        buttonQuit.Text = "Ok";
      }
    }
    #endregion // Constructors

    #region Event reactions
    void RichTextLabelLinkClicked(object sender, LinkClickedEventArgs e)
    {
      var proc = new System.Diagnostics.Process();
      proc.StartInfo.FileName = "mailto:user@domain?subject=[bug] Lem_Settings&body=" +
        richText.Text.Replace("\n", "%0D%0A").Replace("&", "%26");
      proc.Start();
    }
    
    void ButtonQuitClick(object sender, EventArgs e)
    {
      this.Close();
    }
    #endregion // Event reactions
  }
}
