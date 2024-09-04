// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Description of GuiRight.
  /// </summary>
  public partial class GuiRight : UserControl
  {
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public GuiRight()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the current configurator page
    /// </summary>
    /// <param name="page">current page</param>
    public void SetCurrentPage(IItemPage page)
    {
      richTextBox.Text = page.Help;
    }
    #endregion // Methods
  }
}
