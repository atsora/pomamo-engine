// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of OKCancelDialog.
  /// </summary>
  public partial class OKCancelDialog : Form
  {
    #region Constructors
    /// <summary>
    /// Default constructor
    /// Takes the icon of the parent window
    /// Center the window regarding the parent windows
    /// </summary>
    public OKCancelDialog()
    {
      this.StartPosition = FormStartPosition.CenterParent;
      if (null != Form.ActiveForm) {
        this.Icon = Form.ActiveForm.Icon;
      }
      InitializeComponent();
    }
    #endregion // Constructors
  }
}
