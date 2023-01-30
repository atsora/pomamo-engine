// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Dialog to enter a password
  /// </summary>
  public partial class PasswordDialog : OKCancelDialog
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (PasswordDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Password
    /// </summary>
    public string Password {
      get { return passwordTextBox.Text; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PasswordDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      passwordLabel.Text = PulseCatalog.GetString ("PasswordLabelA");
    }
    #endregion // Constructors
    
    // Note: I do not know yet, why the three methods
    //       below are necessary so that it works
    //       although it should not
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close ();
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close ();
    }
    
    void PasswordDialogKeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Enter) {
        OkButtonClick (sender, e);
      }
      else if (e.KeyData == Keys.Escape) {
        CancelButtonClick (sender, e);
      }
    }
  }
}
