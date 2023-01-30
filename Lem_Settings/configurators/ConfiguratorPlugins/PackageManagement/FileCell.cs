// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of FileCell.
  /// </summary>
  public partial class FileCell : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FileCell).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public FileCell(string filePath)
    {
      InitializeComponent();
      
      // Name and directory
      labelName.Text = Path.GetFileName(filePath);
      labelDirectory.Text = Path.GetDirectoryName(filePath);
      
      if (File.Exists(filePath)) {
        pictureWarning.Hide();
        var fi = new FileInfo(filePath);
        labelCreationDate.Text = fi.CreationTime.ToShortDateString() + " - " +
          fi.CreationTime.ToLongTimeString();
        labelModifiedDate.Text = fi.LastWriteTime.ToShortDateString() + " - " +
          fi.LastWriteTime.ToLongTimeString();
      } else {
        pictureWarning.Show();
        labelCreationDate.Text = "?";
        labelModifiedDate.Text = "?";
        labelName.Text += " (not found)";
      }
    }
    #endregion // Constructors
  }
}
