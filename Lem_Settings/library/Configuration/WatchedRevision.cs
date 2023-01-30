// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Settings
{
  /// <summary>
  /// Class that watches a revision, update its status and progression
  /// </summary>
  public class WatchedRevision
  {
    #region Members
    // Modifications whose status might change
    readonly IList<IModification> m_watchedModifications = new List<IModification>();
    
    // Modifications whose status is completed (completed can still go in error because of the "Pending" status)
    readonly IList<IModification> m_completedModifications = new List<IModification>();
    readonly IList<IModification> m_errorModifications = new List<IModification>();
    
    // Link to the revision
    readonly IRevision m_revision = null;
    
    // List of all modification ids in this revision
    readonly IList<long> m_modificationsIds = new List<long>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof(WatchedRevision).FullName);

    #region Getters / Setters
    /// <summary>
    /// True if the revision is in progress
    /// In that case, it might block an item and data will possibly be updated later
    /// </summary>
    public bool InProgress {
      get {
        return m_watchedModifications.Count > 0;
      }
    }
    
    /// <summary>
    /// Total number of step to perform (serves as a base for a progress bar)
    /// This number can grow since child modifications can be created
    /// </summary>
    public int StepNumber {
      get {
        return m_watchedModifications.Count +
          m_completedModifications.Count +
          m_errorModifications.Count;
      }
    }
    
    /// <summary>
    /// Number of steps already done (for a progress bar)
    /// </summary>
    public int StepDone {
      get {
        return m_completedModifications.Count +
          m_errorModifications.Count;
      }
    }
    
    /// <summary>
    /// List of errors that happened during the progression of the revision
    /// Can be empty but not null
    /// </summary>
    public IList<string> Errors {
      get {
        var errors = new List<string>();
        
        if (String.IsNullOrEmpty(ItemId)) {
          errors.Add("Warning: couldn't parse comment '" + m_revision.Comment + "'");
        }

        foreach (var modification in m_errorModifications) {
          errors.Add(modification.AnalysisStatus.GetDescription());
        }

        return errors;
      }
    }
    
    /// <summary>
    /// Identifier of the item in LemSettings that created the revision
    /// Can be null or empty if it couldn't have been parsed from the comment of the revision
    /// </summary>
    public string ItemId { get; private set; }
    
    /// <summary>
    /// Sub-identifier of the item in LemSettings that created the revision
    /// Can be null or empty if it couldn't have been parsed from the comment of the revision
    /// </summary>
    public string ItemSubId { get; private set; }
    
    /// <summary>
    /// Return the id of the revision being watched
    /// </summary>
    public int RevisionId { get { return m_revision.Id; } }
    
    /// <summary>
    /// Return the date when the revision was created
    /// </summary>
    public DateTime CreationDate { get { return m_revision.DateTime; } }
    
    /// <summary>
    /// Get the completed modifications
    /// Can be empty but not null
    /// </summary>
    public IList<IModification> CompletedModifications { get { return m_completedModifications; } }
    
    /// <summary>
    /// Get the modifications done with an error
    /// Can be empty but not null
    /// </summary>
    public IList<IModification> ErrorModifications { get { return m_errorModifications; } }
    
    /// <summary>
    /// Get the modifications to be done
    /// Can be empty but not null
    /// </summary>
    public IList<IModification> WatchedModifications { get { return m_watchedModifications; } }
    
    /// <summary>
    /// Ids of the machines linked to the revision
    /// Can be empty but not null
    /// </summary>
    public IList<int> MachineIds { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="revision">revision to watch</param>
    public WatchedRevision(IRevision revision)
    {
      MachineIds = new List<int>();
      m_revision = revision;
      
      // Parse the comment to find the item id and subid
      var split = m_revision.Comment.Split(' ');
      try {
        int idx = split[1].LastIndexOf('.');
        ItemId = split[1].Substring(0, idx);
        ItemSubId = split[1].Substring(idx + 1);
      } catch (Exception e) {
        log.ErrorFormat("WatchedRevision - couldn't find the item id and subid in '{0}': {1}",
                        m_revision.Comment, e);
        ItemId = "";
        ItemSubId = "";
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Scan modifications directly related to the revision
    /// First step before calling successively the function Update()
    /// </summary>
    /// <param name="lastIdOk"></param>
    public void ScanModifications(long lastIdOk)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Find modifications linked to this revision, after lastIdOk
          var modifications = ModelDAOHelper.DAOFactory.ModificationDAO.FindByRevision(m_revision, lastIdOk);
          
          foreach (var modification in modifications) {
            // Check the machine before adding the modification
            bool modifValid = true;
            if (modification is IMachineModification) {
              var machineTmp = ((IMachineModification)modification).Machine;
              if (machineTmp != null) {
                var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById(machineTmp.Id);
                if (machine == null) {
                  // The modification is ignored because the machine doesn't exist anymore
                  log.WarnFormat("WatchedRevision - Found revision #{0} related to a deleted machine #{1}",
                                 m_revision.Id, machineTmp.Id);
                  modifValid = false;
                } else if (machine.MonitoringType.Id == (int)MachineMonitoringTypeId.Obsolete) {
                  // The modification is ignored because the machine is obsolete
                  log.WarnFormat("WatchedRevision - Found revision #{0} related to the obsolete machine #{1}",
                                 m_revision.Id, machine.Name);
                  modifValid = false;
                } else if (!MachineIds.Contains(machineTmp.Id)) {
                  // Add the machine to the list
                  MachineIds.Add(machineTmp.Id);
                }
              }
            }
            
            if (modifValid) {
              m_watchedModifications.Add(modification);
              m_modificationsIds.Add(((IDataWithId<long>)modification).Id);
            }
          }
        }
      }
      
      // Dispatch watched modifications if they are not in progress
      DispatchWatchedModifications();
    }
    
    void DispatchWatchedModifications()
    {
      for (int i = m_watchedModifications.Count - 1; i >= 0; i--) {
        // The modification moves if it's not in progress and not new
        if (!m_watchedModifications[i].AnalysisStatus.IsInProgress() &&
            !m_watchedModifications[i].AnalysisStatus.IsNew()) {
          if (m_watchedModifications[i].AnalysisStatus.IsCompletedSuccessfully() ||
            m_watchedModifications[i].AnalysisStatus == AnalysisStatus.Pending) {
            m_completedModifications.Add(m_watchedModifications[i]);
          }
          else {
            m_errorModifications.Add(m_watchedModifications[i]);
          }

          m_watchedModifications.RemoveAt(i);
        }
      }

      // Possibly move a completed modification in error (evolution of the status "pending")
      for (int i = m_completedModifications.Count - 1; i >= 0; i--) {
        if (!m_completedModifications[i].AnalysisStatus.IsCompletedSuccessfully () &&
            m_completedModifications[i].AnalysisStatus != AnalysisStatus.Pending) {
          m_errorModifications.Add (m_completedModifications[i]);
          m_completedModifications.RemoveAt (i);
        }
      }
    }
    
    /// <summary>
    /// Update the modifications inside the revision
    /// Detect their state and sort them accordingly
    /// </summary>
    public void Update()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction())
        {
          // Update the state of the existing watched modifications
          for (int i = 0; i < m_watchedModifications.Count; i++) {
            m_watchedModifications[i] = ModelDAOHelper.DAOFactory.ModificationDAO.FindById(
              ((IDataWithId<long>)m_watchedModifications[i]).Id);
          }
          
          // Find children
          int maxIndex = m_watchedModifications.Count;
          for (int i = 0; i < maxIndex; i++) {
            var children = m_watchedModifications[i].SubModifications;
            foreach (var child in children) {
              long childId = ((IDataWithId<long>)child).Id;
              if (!m_modificationsIds.Contains(childId)) {
                m_modificationsIds.Add(childId);
                m_watchedModifications.Add(child);
                maxIndex++; // So that we also check the children of this child
              }
            }
          }

          // Update the state of the existing completed modifications, if "Pending"
          for (int i = 0; i < m_completedModifications.Count; i++) {
            if (m_completedModifications[i].AnalysisStatus == AnalysisStatus.Pending) {
              m_completedModifications[i] = ModelDAOHelper.DAOFactory.ModificationDAO.FindById (
                ((IDataWithId<long>)m_completedModifications[i]).Id);
            }
          }
        }
      }
      
      // Dispatch watched modifications if they are not in progress
      DispatchWatchedModifications();
    }
    
    /// <summary>
    /// Get the id of the first modification in the revision
    /// </summary>
    /// <returns></returns>
    public long GetFirstModificationId()
    {
      long firstId = -1;
      
      foreach (var modification in m_watchedModifications) {
        long modificationId = ((IDataWithId<long>)modification).Id;
        if (firstId == -1 || modificationId < firstId) {
          firstId = modificationId;
        }
      }
      foreach (var modification in m_errorModifications) {
        long modificationId = ((IDataWithId<long>)modification).Id;
        if (firstId == -1 || modificationId < firstId) {
          firstId = modificationId;
        }
      }
      foreach (var modification in m_completedModifications) {
        long modificationId = ((IDataWithId<long>)modification).Id;
        if (firstId == -1 || modificationId < firstId) {
          firstId = modificationId;
        }
      }
      
      return firstId;
    }
    #endregion // Methods
  }
}
