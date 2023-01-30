// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IGlobalModificationDAO">IGlobalModificationDAO</see>
  /// </summary>
  public class GlobalModificationDAO
    : GenericNHibernateDAO<GlobalModification, IGlobalModification, long>
    , IGlobalModificationDAO
  {
    ILog log = LogManager.GetLogger (typeof (ModificationDAO).FullName);

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IGlobalModification MakePersistent (IGlobalModification entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Update (entity);
      return entity;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IGlobalModification> MakePersistentAsync (IGlobalModification entity)
    {
      await NHibernateHelper.GetCurrentSession ()
        .UpdateAsync (entity);
      return entity;
    }

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// </summary>
    /// <param name="entity"></param>
    public virtual void UpgradeLock (IGlobalModification entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Lock (entity, NHibernate.LockMode.Upgrade);
    }

    /// <summary>
    /// Returns the number of remaining modifications to process
    /// before a specified modification is completed
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="createNewAnalysisStatusBefore">not used here</param>
    /// <returns></returns>
    public virtual double GetNumberOfRemainingModifications (IGlobalModification modification, bool createNewAnalysisStatusBefore = true)
    {
      double numberOfRemainingModifications = 0.0;
      IGlobalModification inProgressModification = null;

      if (modification.AnalysisStatus.IsNew ()) {
        // Get the pending modifications before this modification
        IList<IGlobalModification> beforeModifications = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<GlobalModification> ()
          .Add (Restrictions.IsNull ("ParentGlobal"))
          .Add (Restrictions.IsNull ("ParentMachine"))
          .Add (ModificationDAO.GetNotCompletedCriterion ())
          .Add (ModificationDAO.StrictlyBefore (modification))
          .Add (Restrictions.IsNotNull ("StatusPriority"))
          .AddOrder (Order.Desc ("StatusPriority"))
          .AddOrder (Order.Desc ("Priority"))
          .AddOrder (Order.Asc ("Id"))
          .List<IGlobalModification> ();
        numberOfRemainingModifications = beforeModifications.Count + 1.0;
        if ((0 < beforeModifications.Count)
            && beforeModifications[0].AnalysisStatus.IsInProgress ()) {
          inProgressModification = beforeModifications[0];
        }
        else {
          log.DebugFormat ("GetNumberOfRemainingModifications: " +
                           "return {0} directly, there is no in progress modification",
                           numberOfRemainingModifications);
          return numberOfRemainingModifications;
        }
      }
      else if (modification.AnalysisStatus.IsInProgress ()) {
        inProgressModification = modification;
        numberOfRemainingModifications = 1.0;
      }
      else { // Not New and not InProgress => completed
        log.DebugFormat ("GetNumberOfRemainingModifications: " +
                         "modification with status {0} is completed " +
                         "=> return 0",
                         modification.AnalysisStatus);
        return 0.0;
      }

      // Process the recursively how far is the 'in progress' modification
      Debug.Assert (null != inProgressModification);
      if (null == inProgressModification) {
        return numberOfRemainingModifications;
      }
      double completion = ModelDAOHelper.DAOFactory.ModificationDAO
        .GetCompletion (inProgressModification);
      numberOfRemainingModifications -= completion;
      log.DebugFormat ("GetNumberOfRemainingModifications: " +
                       "{0} completion is {1} " +
                       "global result is {2}",
                       inProgressModification, completion, numberOfRemainingModifications);
      return numberOfRemainingModifications;
    }

    /// <summary>
    /// Returns the n first old pending modifications before a specified date/time
    /// that have either no analysis status, or a new/pending analysis status
    /// and that are "greater than" (lastModificationId, lastPriority)
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="before"></param>
    /// <param name="maxResults"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    public IEnumerable<IGlobalModification> GetPastPendingModifications (long lastModificationId,
                                                                         int lastPriority,
                                                                         DateTime before,
                                                                         int maxResults,
                                                                         int minPriority)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .Add (Restrictions.IsNotNull ("StatusPriority"))
        .Add (ModificationDAO.AfterWithStatusPriorityOnly (lastModificationId, lastPriority)) // Because StatusPriority not null
        .Add (Restrictions.Lt ("DateTime", before))
        .Add (Restrictions.Ge ("StatusPriority", minPriority))
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (maxResults)
        .List<IGlobalModification> ();
    }

    /// <summary>
    /// Returns the first modification
    /// which has either no analysis status, or a new/pending analysis status
    /// </summary>
    /// <returns></returns>
    public IGlobalModification GetFirstPendingModification ()
    {
      const int lastModificationId = 0;
      const int lastPriority = Int32.MaxValue;
      return GetFirstPendingModification (lastModificationId, lastPriority, 0);
    }

    /// <summary>
    /// Returns the first modification "greater than"
    /// (lastModificationId, lastPriority)
    /// which has either no analysis status, or a new/pending analysis status
    /// "greather than" the specified:
    /// <item>lastModificationId</item>
    /// <item>lastPriority</item>
    /// 
    /// This must be used in a PendingGlobalModificationAnalysis
    /// where the machine modifications are processed separately
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    public IGlobalModification GetFirstPendingModification (long lastModificationId,
                                                            int lastPriority,
                                                            int minPriority)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .Add (Restrictions.IsNotNull ("StatusPriority"))
        .Add (ModificationDAO.AfterWithStatusPriorityOnly (lastModificationId, lastPriority)) // Because StatusPriority not null
        .Add (Restrictions.Ge ("StatusPriority", minPriority))
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IGlobalModification> ();
    }

    /// <summary>
    /// Returns the first modification "greater than"
    /// (lastModificationId, lastPriority)
    /// which has either no analysis status, or a new/pending analysis status
    /// "greather than" the specified:
    /// <item>lastModificationId</item>
    /// <item>lastPriority</item>
    /// 
    /// This must be used in a PendingGlobalMachineModificationAnalysis
    /// where the global and machine modifications are processed simultaneously
    /// </summary>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    public IGlobalModification GetFirstPendingGlobalMachineModification (long lastModificationId,
                                                                         int lastPriority,
                                                                         int minPriority)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .Add (Restrictions.IsNotNull ("StatusPriority"))
        .Add (ModificationDAO.AfterWithStatusPriorityOnly (lastModificationId, lastPriority)) // because StatusPriority not null
        .Add (Restrictions.Ge ("StatusPriority", minPriority))
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IGlobalModification> ();
    }

    /// <summary>
    /// Get the sub-modifications of the specified modification
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IModification> GetNotCompletedSubModifications (IGlobalModification modification)
    {
      IEnumerable<IGlobalModification> subGlobalModifications = GetNotCompletedSubGlobalModifications (modification);
      IEnumerable<IMachineModification> subMachineModifications = GetNotCompletedSubMachineModifications (modification);
      return subGlobalModifications.Cast<IModification> ().Concat (subMachineModifications.Cast<IModification> ());
    }

    /// <summary>
    /// Get the sub-globalmodifications of the specified modification
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IGlobalModification> GetNotCompletedSubGlobalModifications (IGlobalModification modification)
    {
      if (!modification.AnalysisSubGlobalModifications) {
        log.DebugFormat ("GetNotCompletedSubGlobalModifications: " +
                         "No sub-global modifications " +
                         "=> return an empty list");
        return new List<IGlobalModification> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.Eq ("ParentGlobal", modification))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Desc ("Priority"))
        .AddOrder (Order.Asc ("Id"))
        .List<IGlobalModification> ();
    }

    /// <summary>
    /// Get the sub-machinemodifications of the specified modification
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IMachineModification> GetNotCompletedSubMachineModifications (IGlobalModification modification)
    {
      if (!modification.AnalysisSubMachineModifications) {
        log.DebugFormat ("GetNotCompletedSubMachineModifications: " +
                         "No sub-machine modifications " +
                         "=> return an empty list");
        return new List<IMachineModification> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (Restrictions.Eq ("ParentGlobal", modification))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Desc ("Priority"))
        .AddOrder (Order.Asc ("Id"))
        .List<IMachineModification> ();
    }

    /// <summary>
    /// Check if there are some sub-machine modifications
    /// that are not completed yet
    /// 
    /// This must be run in a read-write transaction (because the initialization of some modifiationstatus
    /// may be required)
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public bool HasNotCompletedSubMachineModifications (IGlobalModification modification)
    {
      if (!modification.AnalysisSubMachineModifications) {
        log.DebugFormat ("HasNotCompletedSubMachineModifications: " +
                         "No sub-machine modifications");
        return false;
      }

      IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll ();
      foreach (IMachine machine in machines) {
        if (HasNotCompletedSubMachineModifications (modification,
                                                    machine,
                                                    true)) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Check if there are some sub-machine modifications
    /// that are not completed yet for a specific machine
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="machine"></param>
    /// <param name="createNewAnalysisStatus">Create the 'New' analysis status first. This requires to have a read-write transaction</param>
    /// <returns></returns>
    public bool HasNotCompletedSubMachineModifications (IGlobalModification modification,
                                                        IMachine machine,
                                                        bool createNewAnalysisStatus)
    {
      if (createNewAnalysisStatus) {
        ModelDAOHelper.DAOFactory.MachineModificationDAO.CreateNewAnalysisStatusNoLimit (machine, false, ((IDataWithId<long>)modification).Id);
        // TODO: it could be optimized, setting a better value for minModificationId
      }
      IMachineModification machineModification = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (Restrictions.Eq ("ModificationMachine", machine))
        .Add (Restrictions.Eq ("ModificationStatusMachine", machine)) // The modificationstatus row must exist
        .Add (Restrictions.Eq ("ParentGlobal", modification))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .SetMaxResults (1)
        .UniqueResult<IMachineModification> ();
      return (null != machineModification);
    }

    /// <summary>
    /// Get ALL the sub-modifications of the specified modification
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IModification> GetAllSubModifications (IGlobalModification modification)
    {
      IEnumerable<IGlobalModification> subGlobalModifications = GetAllSubGlobalModifications (modification);
      IEnumerable<IMachineModification> subMachineModifications = GetAllSubMachineModifications (modification);
      return subGlobalModifications.Cast<IModification> ().Concat (subMachineModifications.Cast<IModification> ());
    }

    /// <summary>
    /// Get ALL the sub-globalmodifications of the specified modification
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IGlobalModification> GetAllSubGlobalModifications (IGlobalModification modification)
    {
      if (!modification.AnalysisSubGlobalModifications) {
        log.DebugFormat ("GetAllSubGlobalModifications: " +
                         "No sub-global modifications " +
                         "=> return an empty list");
        return new List<IGlobalModification> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.Eq ("ParentGlobal", modification))
        .List<IGlobalModification> ();
    }

    /// <summary>
    /// Get ALL the sub-machinemodifications of the specified modification
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IMachineModification> GetAllSubMachineModifications (IGlobalModification modification)
    {
      if (!modification.AnalysisSubMachineModifications) {
        log.DebugFormat ("GetAllSubMachineModifications: " +
                         "No sub-machine modifications " +
                         "=> return an empty list");
        return new List<IMachineModification> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (Restrictions.Eq ("ParentGlobal", modification))
        .List<IMachineModification> ();
    }

    /// <summary>
    /// Delete the modifications of the specified analysis status
    /// </summary>
    /// <param name="analysisStatus"></param>
    public void Delete (AnalysisStatus analysisStatus)
    {
      NHibernateHelper.GetCurrentSession ()
        .CreateQuery (@"delete from GlobalModification foo
where foo.AnalysisStatus=?")
        .SetParameter (0, analysisStatus)
        .ExecuteUpdate ();
    }

    /// <summary>
    /// Delete the modifications of the specified analysis status that are completed before a specific date/time
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <param name="maxCompletionDateTime"></param>
    public void Delete (AnalysisStatus analysisStatus, DateTime maxCompletionDateTime)
    {
      NHibernateHelper.GetCurrentSession ()
        .CreateQuery (@"delete from GlobalModification foo
where foo.AnalysisStatus=?
  and foo.AnalysisEnd<?")
        .SetParameter (0, analysisStatus)
        .SetParameter (1, maxCompletionDateTime)
        .ExecuteUpdate ();
    }

    /// <summary>
    /// Get the modifications in error strictly after the specified completion order
    /// </summary>
    /// <param name="completionOrder"></param>
    /// <returns></returns>
    public IList<IGlobalModification> GetInErrorStrictlyAfter (int completionOrder)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.IsNotNull ("AnalysisCompletionOrder"))
        .Add (Restrictions.Gt ("AnalysisCompletionOrder", completionOrder))
        .Add (ModificationDAO.GetInErrorCriterion ())
        .AddOrder (Order.Desc ("Priority"))
        .AddOrder (Order.Asc ("Id"))
        .List<IGlobalModification> ();
    }

    /// <summary>
    /// Get a number of modifications with a specified analysis status
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public int GetNumber (AnalysisStatus analysisStatus)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .SetProjection (Projections.RowCount ());
      if (AnalysisStatus.New == analysisStatus) {
        criteria.Add (Restrictions.Or (
          Restrictions.IsNull ("AnalysisStatus"),
          Restrictions.Eq ("AnalysisStatus", analysisStatus)));
      }
      else {
        criteria.Add (Restrictions.Eq ("AnalysisStatus", analysisStatus));
      }
      return criteria
        .UniqueResult<int> ();
    }

    /// <summary>
    /// Get the maximum modificationId
    /// </summary>
    /// <returns></returns>
    public long? GetMaxModificationId ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery (@"SELECT MAX(modificationid) AS maxmodificationid
FROM globalmodification")
        .AddScalar ("maxmodificationid", NHibernate.NHibernateUtil.Int64)
        .UniqueResult<long?> ();
    }

    /// <summary>
    /// Get the maximum modificationId asynchronously
    /// </summary>
    /// <returns></returns>
    public async Task<long?> GetMaxModificationIdAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery (@"SELECT MAX(modificationid) AS maxmodificationid
FROM globalmodification")
        .AddScalar ("maxmodificationid", NHibernate.NHibernateUtil.Int64)
        .UniqueResultAsync<long?> ();
    }

    /// <summary>
    /// Get all global modifications not completed that have a revision
    /// </summary>
    /// <param name="application">Name of the application that created the revision (can be null or empty)</param>
    /// <param name="minId">Minimum id to browse (strict)</param>
    /// <returns></returns>
    public IEnumerable<IGlobalModification> FindNotCompletedWithRevision (string application, long minId)
    {
      var elements = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.Gt ("Id", minId))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .Add (Restrictions.IsNotNull ("Revision"))
        .List<IGlobalModification> ();

      return string.IsNullOrEmpty (application) ? elements :
        elements.Where (elt => elt.Revision.Application == application);
    }

    /// <summary>
    /// Get all global modifications related to a revision
    /// </summary>
    /// <param name="revision">Cannot be null</param>
    /// <param name="minId">Minimum ID of the modifications returned (strict)</param>
    /// <returns></returns>
    public IList<IGlobalModification> FindByRevision (IRevision revision, long minId)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.Eq ("Revision", revision))
        .Add (Restrictions.Gt ("Id", minId))
        .List<IGlobalModification> ();
    }
  }
}
