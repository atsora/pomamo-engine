// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Description of RevisionDetailsDialog.
  /// </summary>
  public partial class RevisionDetailsDialog : Form
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RevisionDetailsDialog).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RevisionDetailsDialog(WatchedRevision revision)
    {
      InitializeComponent();
      
      // Context
      this.Text = "Task details (revision #" + revision.RevisionId + " at " + DateTime.Now.ToLongTimeString() + ")";
      
      // Details for each modifications
      AddModifications(imageList.Images[1], revision.ErrorModifications);
      AddModifications(imageList.Images[0], revision.WatchedModifications);
      AddModifications(imageList.Images[2], revision.CompletedModifications);
    }
    #endregion // Constructors
    
    #region Methods
    void AddModifications(Image image, IList<IModification> modifications)
    {
      foreach (var modification in modifications) {
        var cell = new RevisionDetailsCell(image, modification);
        cell.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        verticalScroll.AddControl(cell);
      }
    }
    #endregion // Methods
  }
}
