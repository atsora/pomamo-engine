// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of UsageDialog.
  /// </summary>
  public partial class UsageDialog : Form
  {
    readonly string m_usageText;
    readonly string m_additionalText;
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="text">Text to display</param>
    public UsageDialog(string text)
    {
      InitializeComponent();
      m_usageText = text;
      m_additionalText = null;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="usageText"></param>
    /// <param name="additionalText"></param>
    public UsageDialog(string usageText, string additionalText)
    {
      InitializeComponent ();
      m_usageText = usageText;
      m_additionalText = additionalText;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Get the text to display
    /// </summary>
    /// <returns></returns>
    protected virtual string GetText ()
    {
      if (string.IsNullOrEmpty (m_additionalText)) {
        return m_usageText;
      }
      else {
        return string.Join ("\n\n", new string[] {m_additionalText, m_usageText});
      }
    }
    
    #region Event reactions
    void ButtonOkClick(object sender, EventArgs e)
    {
      this.Close();
    }
    
    void UsageDialogLoad(object sender, EventArgs e)
    {
      richTextBox.Text = GetText ();
      this.Text = Lemoine.I18N.PulseCatalog.GetString ("Usage");
      buttonOk.Text = Lemoine.I18N.PulseCatalog.GetString ("Ok");
    }
    #endregion // Event reactions
  }
}
