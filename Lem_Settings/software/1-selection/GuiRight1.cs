// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Description of GuiRight1.
  /// </summary>
  public partial class GuiRight1 : UserControl
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public GuiRight1()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the current item, may be null
    /// </summary>
    /// <param name="item"></param>
    public void SetCurrentItem(IItem item)
    {
      if (item == null) {
        richTextBox.Text = "The central part of the interface gathers different items, " +
          "which will help you to configure the system.\n\n" +
          "By clicking on an item, a description will appear in the right panel. " +
          "Double-clicking or using the \"validate\" button will open the item.\n\n" +
          "Filtering the items is possible by choosing a category, searching a text, " +
          "displaying recent and favorite items thanks to the left panel.\n\n" +
          "To add an item in the favorites, right-click on it and choose \"Mark as favorite\".";
      } else {
        richTextBox.Text = item.Description;
      }
    }
    #endregion // Methods
  }
}
