// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of ExceptionDialog.
  /// </summary>
  public partial class ExceptionDialog : Form
  {
    readonly Exception m_exception;
    readonly bool m_quit;
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="exception">Exception</param>
    /// <param name="quit">If true "Quit" is written. Otherwise "Ok"</param>
    public ExceptionDialog(Exception exception, bool quit)
    {
      InitializeComponent();
      m_exception = exception;
      m_quit = quit;
    }
    #endregion // Constructors

    /// <summary>
    /// Get the text that is associated to the exception
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    protected virtual string GetText (Exception ex)
    {
      return ex.Message + "\n\nStack trace: \n" + ex.StackTrace;
    }
    
    #region Event reactions
    void ExceptionDialogLoad(object sender, EventArgs e)
    {
      string text = GetText (m_exception);
      richText.Text = text;
      if (m_quit) {
        buttonQuit.Text = Lemoine.I18N.PulseCatalog.GetString ("Quit");
      }
      else {
        buttonQuit.Text = Lemoine.I18N.PulseCatalog.GetString ("Ok");
      }
    }

    void RichTextLabelLinkClicked(object sender, LinkClickedEventArgs e)
    {
    }
    
    void ButtonQuitClick(object sender, EventArgs e)
    {
      this.Close();
    }
    #endregion // Event reactions
  }
}
