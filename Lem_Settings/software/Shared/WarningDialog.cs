// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Description of ErrorDialog.
  /// </summary>
  public partial class WarningDialog : Form
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="ignorePossible">True if SuperAdmin can click on "Ignore"</param>
    public WarningDialog(String message, bool ignorePossible)
    {
      this.StartPosition = FormStartPosition.CenterParent;
      InitializeComponent();
      pictureBox.Image = SystemIcons.Warning.ToBitmap();
      label.Text = message;
      buttonDontCare.Visible = (ContextManager.UserCategory == LemSettingsGlobal.UserCategory.SUPER_ADMIN);
      buttonDontCare.Enabled = ignorePossible;
    }
    #endregion // Constructors
    
    #region Event reactions
    void ButtonDontCareClick(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Ignore;
      Close();
    }
    
    void ButtonOkClick(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }
    #endregion // Event reactions
  }
}
