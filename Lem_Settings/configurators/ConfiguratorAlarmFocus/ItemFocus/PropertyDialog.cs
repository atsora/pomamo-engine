// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.BaseControls;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of Form1.
  /// </summary>
  public partial class PropertyDialog : OKCancelDialog
  {
    #region Getters / Setters
    /// <summary>
    /// Property
    /// </summary>
    public string Property
    {
      get { return textProperty.Text; }
      set { textProperty.Text = value; }
    }

    /// <summary>
    /// Property
    /// </summary>
    public string Value
    {
      get { return textValue.Text; }
      set { textValue.Text = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PropertyDialog ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors
  }
}
