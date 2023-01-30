// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of Form1.
  /// </summary>
  public partial class OKCancelMessageDialog : OKCancelDialog
  {
    #region Getters / Setters
    /// <summary>
    /// Message to show in dialog
    /// </summary>
    public string Message { 
      get { return this.labelMessage.Text; }
      set { this.labelMessage.Text = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OKCancelMessageDialog()
    {
      InitializeComponent();
    }
    #endregion // Constructors
  }
}
