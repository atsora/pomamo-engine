// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Description of RevisionDialog.
  /// </summary>
  public partial class RevisionDialog : Form
  {
    #region Members
    readonly IDictionary<int, RevisionCell> m_inProgressCells = new Dictionary<int, RevisionCell>();
    readonly IDictionary<int, RevisionCell> m_doneCells = new Dictionary<int, RevisionCell>();
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (RevisionDialog).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RevisionDialog()
    {
      InitializeComponent();
      OnUpdateDone();
      RevisionManager.UpdateDone += OnUpdateDone; // Removed in Dispose()
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Select a revision created by a specific item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="subId"></param>
    public void SelectRevision(string id, string subId)
    {
      // Try to find the cell in the "in progress cells"
      foreach (var cell in m_inProgressCells.Values) {
        if (string.Equals(cell.ItemId, id) && string.Equals(cell.ItemSubId, subId)) {
          SelectCell(cell);
          return;
        }
      }
      
      // Try to find the cell in the other cells
      foreach (var cell in m_doneCells.Values) {
        if (string.Equals(cell.ItemId, id) && string.Equals(cell.ItemSubId, subId)) {
          SelectCell(cell);
          return;
        }
      }
    }
    
    void SelectCell(Control cell)
    {
      verticalScroll.ScrollTo(cell);
    }
    #endregion // Methods
    
    #region Event reactions
    void OnDetailsClicked(WatchedRevision revision)
    {
      var dialog = new RevisionDetailsDialog(revision);
      dialog.ShowDialog();
    }
    
    void OnUpdateDone()
    {
      // Browse revisions in progress
      var revisions = RevisionManager.InProgressRevisions;
      foreach (var revision in revisions) {
        if (m_inProgressCells.ContainsKey(revision.RevisionId)) {
          // Update existing cells
          m_inProgressCells[revision.RevisionId].UpdateContent();
        } else {
          // Create a new cell
          var cell = new RevisionCell(revision);
          cell.Anchor = AnchorStyles.Left | AnchorStyles.Right;
          cell.DetailsClicked += OnDetailsClicked;
          m_inProgressCells[revision.RevisionId] = cell;
          verticalScroll.AddControl(cell);
        }
      }
      
      // Browse done revisions
      revisions = RevisionManager.DoneRevisions;
      foreach (var revision in revisions) {
        if (m_inProgressCells.ContainsKey (revision.RevisionId)) {
          // Update the cell for the last time and move it to the done list
          m_inProgressCells[revision.RevisionId].UpdateContent ();
          m_doneCells[revision.RevisionId] = m_inProgressCells[revision.RevisionId];
          m_inProgressCells.Remove (revision.RevisionId);
        } else if (m_doneCells.ContainsKey (revision.RevisionId)) {
          // Update existing cell
          m_doneCells[revision.RevisionId].UpdateContent ();
        } else {
          // Create a new cell and add it to the done list
          var cell = new RevisionCell(revision);
          cell.Anchor = AnchorStyles.Left | AnchorStyles.Right;
          cell.DetailsClicked += OnDetailsClicked;
          m_doneCells[revision.RevisionId] = cell;
          verticalScroll.AddControl(cell);
        }
      }
    }
    #endregion // Event reactions
  }
}
