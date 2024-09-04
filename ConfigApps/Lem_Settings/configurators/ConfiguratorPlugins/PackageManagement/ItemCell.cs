// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of ItemCell.
  /// </summary>
  public partial class ItemCell : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ItemCell).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ItemCell(IItem item)
    {
      InitializeComponent();
      
      pictureBox.Image = item.Image;
      labelName.Text = item.Title;
      labelDescription.Text = item.Description;
    }
    #endregion // Constructors
  }
}
