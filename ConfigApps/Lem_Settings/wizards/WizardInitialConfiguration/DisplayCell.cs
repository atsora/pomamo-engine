// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace WizardInitialConfiguration
{
  /// <summary>
  /// Description of DisplayCell.
  /// </summary>
  public partial class DisplayCell : UserControl
  {
    #region Members
    string[] m_properties;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DisplayCell).FullName);

    #region Getters / Setters
    /// <summary>
    /// Set or get the pattern
    /// </summary>
    public string Pattern {
      get { return textBox.Text; }
      set { textBox.Text = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DisplayCell(string itemName, params string[] properties)
    {
      InitializeComponent();
      m_properties = properties;
      labelItem.Text = itemName;
      labelProperties.Text = string.Join(", ", properties);
      this.Dock = DockStyle.Fill;
    }
    #endregion // Constructors

    #region Methods
    public bool IsValid()
    {
      // Note: there may be some valid patterns that are not listed in m_properties
      //       So don't be too strict in the validation phase
      bool ok = true;
      
      // Change background color
      textBox.BackColor = ok ? SystemColors.Window : LemSettingsGlobal.COLOR_ERROR;
      
      return ok;
    }
    #endregion // Methods
  }
}
