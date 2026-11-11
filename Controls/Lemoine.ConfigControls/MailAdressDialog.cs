// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Mail;
using Lemoine.BaseControls;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of MailAdressDialog.
  /// </summary>
  public partial class MailAdressDialog : OKCancelDialog
  {

    static readonly ILog log = LogManager.GetLogger(typeof (MailAdressDialog).FullName);

    /// <summary>
    /// Return a MailAdress object or null
    /// </summary>
    public string FromAdress => this.fromTextBox.Text;

    /// <summary>
    /// Return a MailAdress object or null
    /// </summary>
    public string ToAdress => this.toTextBox.Text;

    #region Constructors
    /// <summary>
    /// Create a dialog for email adress input (from and to)
    /// </summary>
    public MailAdressDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      toLabel.Text = PulseCatalog.GetString("To");
      fromLabel.Text = PulseCatalog.GetString("From");
      okButton.Enabled = false;      
    }
    #endregion // Constructors

    /// <summary>
    /// Validate Input
    /// </summary>
    /// <returns></returns>
    void ValidateForm(){
      if(string.IsNullOrEmpty(toTextBox.Text) 
          && string.IsNullOrEmpty(fromTextBox.Text)){
        okButton.Enabled = false;
        return;
      }
      okButton.Enabled = true;
    }
    
    void FromTextBoxTextChanged(object sender, EventArgs e)
    {
      ValidateForm();
    }
    
    void ToTextBoxTextChanged(object sender, EventArgs e)
    {
      ValidateForm();
    }
  }
}
