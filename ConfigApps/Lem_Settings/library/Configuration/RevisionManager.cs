// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of RevisionManager.
  /// </summary>
  public sealed class RevisionManager
  {
    #region Members
    readonly IList<WatchedRevision> m_watchList = new List<WatchedRevision> ();
    readonly IList<WatchedRevision> m_doneList = new List<WatchedRevision> ();
    bool m_initialized = false;
    long m_lastIdOk = 0;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (RevisionManager).FullName);

    #region Events
    /// <summary>
    /// Event emitted when an item is done with revisions
    /// First argument is the item id allowed to be used again
    /// Second argument is the sub id
    /// </summary>
    public static event Action<string, string> RevisionFinished;

    /// <summary>
    /// Event emitted when the number of watched revisions changes
    /// First argument is the number of argument
    /// </summary>
    public static event Action<int> RevisionNumberChanged;

    /// <summary>
    /// Event emitted when the data related to revisions have been updated
    /// </summary>
    public static event Action UpdateDone;
    #endregion // Events

    #region Getters / Setters
    /// <summary>
    /// Get the revisions in progress
    /// </summary>
    public static IList<WatchedRevision> InProgressRevisions
    {
      get {
        return Instance.m_watchList;
      }
    }

    /// <summary>
    /// Get the revisions that have finished
    /// (can be fully done or in error)
    /// </summary>
    public static IList<WatchedRevision> DoneRevisions
    {
      get {
        return Instance.m_doneList;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class!)
    /// </summary>
    RevisionManager () { }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Scan not completed revisions for the first time
    /// </summary>
    public static void FirstScan ()
    {
      Instance.FirstScanInstance ();
    }

    void FirstScanInstance ()
    {
      // Recall the last ID that have been found as ok
      try {
        m_lastIdOk = long.Parse (IniFilePreferences.Get (IniFilePreferences.Field.LAST_MODIFICATION_ID_OK));
        log.InfoFormat ("RevisionManager.FirstScanInstance - Last modification ok is {0}", m_lastIdOk);
      }
      catch (Exception e) {
        log.ErrorFormat ("RevisionManager.FirstScanInstance - Couldn't parse LAST_MODIFICATION_ID_OK '{0}': {1}",
                        IniFilePreferences.Get (IniFilePreferences.Field.LAST_MODIFICATION_ID_OK), e);
      }

      // Find all revisions that have at least one modification not completed
      long? maxIndex = null;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          maxIndex = ModelDAOHelper.DAOFactory.ModificationDAO.GetMaxModificationId ();
          if (maxIndex.HasValue && maxIndex.Value < m_lastIdOk) // In the case where we deal with several databases
{
            m_lastIdOk = -1;
          }

          var modifications = ModelDAOHelper.DAOFactory.ModificationDAO.FindNotCompletedWithRevision ("Lem_Settings", m_lastIdOk);
          var revisionIds = new List<int> ();
          foreach (var modification in modifications) {
            if (!revisionIds.Contains (modification.Revision.Id)) {
              m_watchList.Add (new WatchedRevision (modification.Revision));
              revisionIds.Add (modification.Revision.Id);
            }
          }
        }
      }

      // Scan all parent modifications
      foreach (var revision in m_watchList) {
        revision.ScanModifications (m_lastIdOk);
      }

      // Keep only revisions that have modifications in progress
      // This way, those being definitely stopped when LemSettings is open are ignored
      for (int i = m_watchList.Count - 1; i >= 0; i--) {
        if (!m_watchList[i].InProgress) {
          m_watchList.RemoveAt (i);
        }
      }

      // Adjust LAST_MODIFICATION_ID_OK based on the remaining watched revisions
      long adjustedFirstId = -1;
      foreach (var revision in m_watchList) {
        var tmp = revision.GetFirstModificationId ();
        if (adjustedFirstId == -1 || tmp < adjustedFirstId) {
          adjustedFirstId = tmp;
        }
      }
      if (adjustedFirstId != -1) {
        m_lastIdOk = adjustedFirstId - 1;
      }
      else if (maxIndex.HasValue) {
        m_lastIdOk = maxIndex.Value - 1;
      }

      IniFilePreferences.Set (IniFilePreferences.Field.LAST_MODIFICATION_ID_OK, m_lastIdOk.ToString ());

      // Notify about the number of revisions being watched
      if (m_watchList.Count > 0) {
        RevisionNumberChanged (m_watchList.Count);
      }

      m_initialized = true;
    }

    /// <summary>
    /// Check if an item is usable:
    /// return true if no revisions are in progress
    /// </summary>
    /// <param name="item"></param>
    public static bool IsUsable (IItem item)
    {
      return Instance.IsUsableInstance (item);
    }

    bool IsUsableInstance (IItem item)
    {
      foreach (var watchRevision in m_watchList) {
        if (watchRevision.ItemId == item.ID && (watchRevision.ItemSubId == item.SubID)) {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Add a revision in the watch list
    /// </summary>
    /// <param name="revision"></param>
    public static void AddRevision (IRevision revision)
    {
      Instance.AddRevisionInstance (revision);
    }

    void AddRevisionInstance (IRevision revision)
    {
      // Create a watched revision and compute its status
      var watchedRevision = new WatchedRevision (revision);
      watchedRevision.ScanModifications (m_lastIdOk);

      // Store the revision depending on its status
      if (watchedRevision.InProgress) {
        m_watchList.Add (watchedRevision);
      }
      else {
        m_doneList.Add (watchedRevision);
      }

      // Notify that the number of revisions changed
      RevisionNumberChanged (m_watchList.Count);
    }

    /// <summary>
    /// Remove a revision that has been done from the list
    /// </summary>
    /// <param name="revisionId"></param>
    public static void RemoveDoneRevision (int revisionId)
    {
      Instance.RemoveDoneRevisionInstance (revisionId);
    }

    void RemoveDoneRevisionInstance (int revisionId)
    {
      for (int i = 0; i < m_doneList.Count; i++) {
        if (m_doneList[i].RevisionId == revisionId) {
          m_doneList.RemoveAt (i);
          break;
        }
      }
    }
    #endregion // Methods

    #region Event reactions
    /// <summary>
    /// Function triggered periodically so that data related to revisions is updated
    /// </summary>
    public static void OnTimeOut ()
    {
      Instance.OnTimeOutInstance ();
    }

    void OnTimeOutInstance ()
    {
      if (!m_initialized) {
        return;
      }

      // Update the "Done" list (a pending modification can possibly be in error later)
      foreach (var revision in m_doneList) {
        revision.Update ();
      }

      // Update all revisions and possibly move them into the "Done" list
      for (int i = m_watchList.Count - 1; i >= 0; i--) {
        m_watchList[i].Update ();
        if (!m_watchList[i].InProgress) {
          var revision = m_watchList[i];
          m_doneList.Add (revision);
          m_watchList.RemoveAt (i);

          // The number of revisions in progress changed
          RevisionNumberChanged (m_watchList.Count);

          // An item is now enabled
          RevisionFinished (revision.ItemId, revision.ItemSubId);
        }
      }

      // Notify that the update is done
      if (UpdateDone != null) {
        UpdateDone ();
      }
    }

    #endregion // Event reactions

    #region Instance
    static RevisionManager Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested () { }
      internal static readonly RevisionManager instance = new RevisionManager ();
    }
    #endregion // Instance

  }

  public sealed class RevisionManagerApplicationInitializer : IApplicationInitializer
  {
    #region IApplicationInitializer
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      RevisionManager.FirstScan ();
    }

    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      InitializeApplication (cancellationToken);
      return Task.CompletedTask;
    }
    #endregion // IApplicationInitializer
  }
}
