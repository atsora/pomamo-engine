// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Description of RevisionCell.
  /// </summary>
  public partial class RevisionCell : UserControl
  {
    #region Events
    /// <summary>
    /// Event emitted when the user asks for details
    /// The first argument is the revision
    /// </summary>
    public event Action<WatchedRevision> DetailsClicked;
    #endregion // Events
    
    #region Members
    readonly WatchedRevision m_watchedRevision = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (RevisionCell).FullName);

    #region Getters / Setters
    /// <summary>
    /// Id of the revision displayed by the cell
    /// </summary>
    public int RevisionId { get; private set; }
    
    /// <summary>
    /// Item id that created the revision
    /// </summary>
    public string ItemId { get; private set; }
    
    /// <summary>
    /// Item subid that created the revision
    /// </summary>
    public string ItemSubId { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RevisionCell(WatchedRevision revision)
    {
      InitializeComponent();
      
      m_watchedRevision = revision;
      RevisionId = revision.RevisionId;
      ItemId = revision.ItemId;
      ItemSubId = revision.ItemSubId;
      
      // Display static content
      IItem revisionItem = null;
      if (!string.IsNullOrEmpty(revision.ItemId)) {
        // Try to find the LemSettings item
        var items = ItemManager.GetItems(revision.ItemId);
        if (string.IsNullOrEmpty(revision.ItemSubId)) {
          // Only the ID is specified, we take the item if only one is retrieved
          if (items.Count == 1) {
            revisionItem = items[0];
          }
        } else {
          // The sub ID is also specified
          foreach (var item in items) {
            if (string.Equals(item.SubID, revision.ItemSubId)) {
              revisionItem = item;
              break;
            }
          }
        }
        
        // Item name of the revision
        if (revisionItem != null) {
          labelItem.Text = revisionItem.Title;
        }
        else {
          labelItem.Text = "Item #" + revision.ItemId + (!string.IsNullOrEmpty(revision.ItemSubId) ? "." + revision.ItemSubId : "");
        }
      } else {
        labelItem.Text = "?";
      }

      // Date of the revision
      labelDetails.Text = revision.CreationDate.ToLocalTime().ToLongDateString() + " " +
        revision.CreationDate.ToLocalTime().ToLongTimeString();
      
      // Machines involved
      if (revision.MachineIds.Count == 1) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById(revision.MachineIds[0]);
            if (machine != null) {
              labelDetails.Text += " on machine '" + machine.Name + "'";
            }
          }
        }
      } else if (revision.MachineIds.Count > 1) {
        labelDetails.Text += " on " + revision.MachineIds.Count + " machines";
      }

      // Update dynamic content
      UpdateContent ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Update data displayed in the cell
    /// </summary>
    public void UpdateContent()
    {
      // Progress
      if (m_watchedRevision.StepNumber <= 0) {
        labelProgress.Text = "Progress: -";
      }
      else {
        double value = 100.0 * (double)m_watchedRevision.StepDone / (double)m_watchedRevision.StepNumber;
        int iVal = (int)Math.Round (value);
        if (iVal >= 100) {
          labelProgress.Text = "Progress: finished";
        }
        else {
          labelProgress.Text = "Progress: " + iVal + "%";
        }
      }
      
      // Image
      if (m_watchedRevision.InProgress) {
        pictureBox.Image = imageList.Images[0];
      } else if (m_watchedRevision.ErrorModifications.Count > 0) {
        pictureBox.Image = imageList.Images[2];
      } else {
        pictureBox.Image = imageList.Images[1];
      }
      
      // Errors
      listErrors.ClearItems();
      foreach (var error in m_watchedRevision.Errors) {
        listErrors.AddItem(error);
      }

      if (listErrors.Count == 0) {
        baseLayout.RowStyles[3].Height = 0;
        this.Height = 60;
      } else {
        baseLayout.RowStyles[3].Height = 100;
        this.Height = 100;
      }
    }
    #endregion // Methods
    
    #region Event reactions
    void ButtonDetailsClick(object sender, EventArgs e)
    {
      DetailsClicked(m_watchedRevision);
    }
    #endregion // Event reactions
  }
}
