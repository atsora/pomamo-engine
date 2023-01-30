// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Lemoine.I18N;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Dialog to enter a text
  /// </summary>
  public partial class TextDialog : OKCancelDialog
  {
    #region Getters / Setters
    /// <summary>
    /// Text
    /// </summary>
    public string InputText {
      get { return textTextBox.Text; }
    }

    /// <summary>
    /// Config filter
    /// </summary>
    [Category("Appearance"), Browsable(true), DefaultValue("Text"), Description("Message text")]
    public string Message {
      get { return textLabel.Text; }
      set { textLabel.Text = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TextDialog()
    {
      InitializeComponent();
      textLabel.Text = PulseCatalog.GetString ("TextLabelA");
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
    
    void TextDialogKeyDown(object sender, KeyEventArgs e)
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
