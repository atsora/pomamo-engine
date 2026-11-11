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
using NHibernate;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineModificationDAO">IMachineModificationDAO</see>
  /// </summary>
  public class MachineModificationDAO
    : GenericByMachineNHibernateDAO<MachineModification, IMachineModification, long>
    , IMachineModificationDAO
  {
    static readonly string UPSERT_ACTIVE_KEY = "machinemodificationstatus.upsert.active";
    static readonly bool UPSERT_ACTIVE_DEFAULT = false; // false until conflict errors are propagated to the parent table in case the table is partitioned. And not really useful after the additional condition AND NOT EXISTS is added
    // And not sure ON CONFLICT and RETURNING work nice together: keep false for the moment

    static readonly string UPSERT_ACTIVE_IN_PARTITION_KEY = "machinemodificationstatus.upsert.active.partition";
    static readonly bool UPSERT_ACTIVE_IN_PARTITION_DEFAULT = true;
    // In a partition (pgfkpart.machinemodificationstatus_pX), which is a real table, ON CONFLICT DO NOTHING works fine.
    // The condition AND NOT EXISTS of CreateNewAnalysisStatus is a separate request: it does not prevent
    // two concurrent transactions from inserting the same rows. With ON CONFLICT DO NOTHING, in READ COMMITTED,
    // the second transaction skips these rows instead of raising a unique violation; in SERIALIZABLE,
    // it raises a serialization failure instead of a unique violation (both are retried). No RETURNING clause is used

    readonly ILog log = LogManager.GetLogger (typeof (ModificationDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineModificationDAO ()
      : base ("ModificationMachine")
    { }

    /// <summary>
    /// Get a criterion on the machine
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    protected internal override ICriterion GetMachineCriterion (int machineId)
    {
      return Restrictions.Conjunction ()
        .Add (Restrictions.Eq ("ModificationMachine.Id", machineId))
        .Add (Restrictions.Disjunction ()
          .Add (Restrictions.IsNull ("ModificationStatusMachine"))
          .Add (Restrictions.Eq ("ModificationStatusMachine.Id", machineId)));
    }

    /// <summary>
    /// Get a criterion on the machine
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    ICriterion GetMachineCriterionWithStatus (int machineId)
    {
      return Restrictions.Conjunction ()
        .Add (Restrictions.Eq ("ModificationMachine.Id", machineId))
        .Add (Restrictions.Eq ("ModificationStatusMachine.Id", machineId)); // The machinemodificationstatus row must exist
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IMachineModification MakePersistent (IMachineModification entity)
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
    public override async Task<IMachineModification> MakePersistentAsync (IMachineModification entity)
    {
      await NHibernateHelper.GetCurrentSession ()
        .UpdateAsync (entity);
      return entity;
    }

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// </summary>
    /// <param name="entity"></param>
    public virtual void UpgradeLock (IMachineModification entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Lock (entity, NHibernate.LockMode.Upgrade);
    }

    /// <summary>
    /// Returns the number of remaining modifications to process
    /// before a specified modification is completed
    /// 
    /// This must be run in a read-write transaction (because the initialization of some modificationstatus
    /// may be required)
    /// </summary>
    /// <param name="modification">not null</param>
    /// <param name="createNewAnalysisStatusBefore"></param>
    /// <returns></returns>
    public virtual double GetNumberOfRemainingModifications (IMachineModification modification, bool createNewAnalysisStatusBefore = true)
    {
      Debug.Assert (null != modification);
      Debug.Assert (null != modification.Machine);

      double numberOfRemainingModifications = 0.0;
      IMachineModification inProgressModification = null;

      if (modification.AnalysisStatus.IsNew ()) {
        if (createNewAnalysisStatusBefore) {
          CreateNewAnalysisStatusNotOptimized (modification.Machine);
        }

        // TODO: optimize this request

        // Get the pending modifications before this modification
        IList<IMachineModification> beforeModifications = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<MachineModification> ()
          .Add (GetMachineCriterionWithStatus (modification.Machine.Id))
          .Add (Restrictions.IsNull ("ParentGlobal"))
          .Add (Restrictions.IsNull ("ParentMachine"))
          .Add (ModificationDAO.GetNotCompletedCriterion ())
          .Add (ModificationDAO.StrictlyBefore (modification))
          .AddOrder (Order.Desc ("StatusPriority"))
          .AddOrder (Order.Desc ("Priority"))
          .AddOrder (Order.Asc ("Id"))
          .List<IMachineModification> ();
        numberOfRemainingModifications = beforeModifications.Count + 1.0;
        if ((0 < beforeModifications.Count)
            && beforeModifications[0].AnalysisStatus.IsInProgress ()) {
          inProgressModification = beforeModifications[0];
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetNumberOfRemainingModifications: return {numberOfRemainingModifications} directly, there is no in progress modification");
          }
          return numberOfRemainingModifications;
        }
      }
      else if (modification.AnalysisStatus.IsInProgress ()) {
        inProgressModification = modification;
        numberOfRemainingModifications = 1.0;
      }
      else { // Not New and not InProgress => completed
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNumberOfRemainingModifications: modification with status {modification.AnalysisStatus} is completed => return 0");
        }
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
      if (log.IsDebugEnabled) {
        log.Debug ($"GetNumberOfRemainingModifications: {inProgressModification} completion is {completion} global result is {numberOfRemainingModifications}");
      }
      return numberOfRemainingModifications;
    }

    /// <summary>
    /// Returns the n first old pending modifications before a specified date/time
    /// that have either no analysis status, or a new/pending analysis status
    /// and that are "greater than" (lastModificationId, lastPriority)
    /// 
    /// This returns only the modifications for which the analysis status row has already been created
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="before"></param>
    /// <param name="maxResults"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    public IEnumerable<IMachineModification> GetPastPendingModifications (IMachine machine,
                                                                          long lastModificationId,
                                                                          int lastPriority,
                                                                          DateTime before,
                                                                          int maxResults,
                                                                          int minPriority)
    {
      // It is much more efficient to restrict the result to modifications for which the analysisstatus
      // row has already been created
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (GetMachineCriterionWithStatus (machine.Id))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .Add (Restrictions.IsNotNull ("StatusPriority"))
        .Add (ModificationDAO.AfterWithStatusPriorityOnly (lastModificationId, lastPriority)) // because StatusPriority not null
        .Add (Restrictions.Lt ("DateTime", before))
        .Add (Restrictions.Ge ("StatusPriority", minPriority))
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (maxResults)
        .List<IMachineModification> ();
    }


    /// <summary>
    /// Returns the first modification
    /// which has either no analysis status, or a new/pending analysis status
    /// 
    /// This returns only the modifications for which the analysis status row has already been created
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IMachineModification GetFirstPendingModification (IMachine machine)
    {
      const int lastModificationId = 0;
      const int lastPriority = Int32.MaxValue;
      return GetFirstPendingModification (machine, lastModificationId, lastPriority, 0);
    }

    /// <summary>
    /// Returns the first modification "greater than"
    /// (lastModificationId, lastPriority)
    /// which has either no analysis status, or a new/pending analysis status
    /// "greather than" the specified:
    /// <item>machine</item>
    /// <item>lastModificationId</item>
    /// <item>lastPriority</item>
    /// 
    /// This must be used in a PendingMachineModificationAnalysis
    /// where the global modifications are processed separately
    /// 
    /// This returns only the modifications for which the analysis status row has already been created
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    public IMachineModification GetFirstPendingModification (IMachine machine,
                                                            long lastModificationId,
                                                            int lastPriority,
                                                            int minPriority)
    {
      // It is much more efficient to restrict the result to modifications for which the analysisstatus
      // row has already been created
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (GetMachineCriterionWithStatus (machine.Id))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .Add (Restrictions.IsNotNull ("StatusPriority"))
        .Add (ModificationDAO.AfterWithStatusPriorityOnly (lastModificationId, lastPriority)) // because StatusPriority not null
        .Add (Restrictions.Ge ("StatusPriority", minPriority))
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IMachineModification> ();
    }

    /// <summary>
    /// Returns the first modification "greater than"
    /// (lastModificationId, lastPriority)
    /// which has either no analysis status, or a new/pending analysis status
    /// "greather than" the specified:
    /// <item>machine</item>
    /// <item>lastModificationId</item>
    /// <item>lastPriority</item>
    /// 
    /// This must be used in a PendingGlobalMachineModificationAnalysis
    /// where the global and machine modifications are processed simulataneously
    /// 
    /// This returns only the modifications for which the analysis status row has already been created
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="lastModificationId"></param>
    /// <param name="lastPriority"></param>
    /// <param name="minPriority"></param>
    /// <returns></returns>
    public IMachineModification GetFirstPendingGlobalMachineModification (IMachine machine,
                                                                         long lastModificationId,
                                                                         int lastPriority,
                                                                         int minPriority)
    {
      // It is much more efficient to restrict the result to modifications for which the analysisstatus
      // row has already been created
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (GetMachineCriterionWithStatus (machine.Id))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .Add (Restrictions.IsNotNull ("StatusPriority"))
        .Add (ModificationDAO.AfterWithStatusPriorityOnly (lastModificationId, lastPriority)) // because StatusPriority not null
        .Add (Restrictions.Ge ("StatusPriority", minPriority))
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IMachineModification> ();
    }

    /// <summary>
    /// Get the sub-modifications of the specified modification
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IModification> GetNotCompletedSubModifications (IMachineModification modification)
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
    public IEnumerable<IGlobalModification> GetNotCompletedSubGlobalModifications (IMachineModification modification)
    {
      if (!modification.AnalysisSubGlobalModifications) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetNotCompletedSubGlobalModifications: No sub-global modifications => return an empty list");
        }
        return new List<IGlobalModification> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.Eq ("ParentMachine", modification))
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
    public IEnumerable<IMachineModification> GetNotCompletedSubMachineModifications (IMachineModification modification)
    {
      if (!modification.AnalysisSubMachineModifications) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetNotCompletedSubMachineModifications: No sub-machine modifications => return an empty list");
        }
        return new List<IMachineModification> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (Restrictions.Eq ("ParentMachine", modification))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .AddOrder (Order.Desc ("StatusPriority"))
        .AddOrder (Order.Desc ("Priority"))
        .AddOrder (Order.Asc ("Id"))
        .List<IMachineModification> ();
    }

    /// <summary>
    /// Get the sub-machinemodifications of the specified modification
    /// that are not completed yet and for the specified machine Id
    /// 
    /// This must be run in a read-write transaction (because the initialization of some modifiationstatus
    /// may be required)
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IMachineModification> GetNotCompletedSubMachineModifications (IMachine machine,
                                                                                     IMachineModification modification)
    {
      if (!modification.AnalysisSubMachineModifications) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetNotCompletedSubMachineModifications: No sub-machine modifications => return an empty list");
        }
        return new List<IMachineModification> ();
      }

      CreateNewAnalysisStatusNoLimit (machine, false, ((IDataWithId<long>)modification).Id);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (GetMachineCriterionWithStatus (machine.Id))
        .Add (Restrictions.Eq ("ParentMachine", modification))
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
    public bool HasNotCompletedSubMachineModifications (IMachineModification modification)
    {
      if (!modification.AnalysisSubMachineModifications) {
        if (log.IsDebugEnabled) {
          log.Debug ("HasNotCompletedSubMachineModifications: No sub-machine modifications");
        }
        return false;
      }

      IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll ();
      foreach (IMachine machine in machines) {
        if (HasNotCompletedSubMachineModifications (modification, machine, true)) {
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
    public bool HasNotCompletedSubMachineModifications (IMachineModification modification, IMachine machine,
                                                        bool createNewAnalysisStatus)
    {
      if (createNewAnalysisStatus) {
        CreateNewAnalysisStatusNoLimit (machine, false, ((IDataWithId<long>)modification).Id);
      }
      IMachineModification machineModification = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (GetMachineCriterionWithStatus (machine.Id))
        .Add (Restrictions.Eq ("ParentMachine", modification))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .SetMaxResults (1)
        .UniqueResult<IMachineModification> ();
      return (null != machineModification);
    }

    /// <summary>
    /// Check if there are some sub-global modifications
    /// that are not completed yet
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public bool HasNotCompletedGlobalModifications (IMachineModification modification)
    {
      if (!modification.AnalysisSubGlobalModifications) {
        if (log.IsDebugEnabled) {
          log.Debug ("HasNotCompletedSubGlobalModifications: No sub-global modifications");
        }
        return false;
      }

      IGlobalModification globalModification = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.Eq ("ParentMachine", modification))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .SetMaxResults (1)
        .UniqueResult<IGlobalModification> ();
      if (null != globalModification) {
        return true;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Get ALL the sub-modifications of the specified modification
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IModification> GetAllSubModifications (IMachineModification modification)
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
    public IEnumerable<IGlobalModification> GetAllSubGlobalModifications (IMachineModification modification)
    {
      if (!modification.AnalysisSubGlobalModifications) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetAllSubGlobalModifications: No sub-global modifications => return an empty list");
        }
        return new List<IGlobalModification> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<GlobalModification> ()
        .Add (Restrictions.Eq ("ParentMachine", modification))
        .List<IGlobalModification> ();
    }

    /// <summary>
    /// Get ALL the sub-machinemodifications of the specified modification
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IEnumerable<IMachineModification> GetAllSubMachineModifications (IMachineModification modification)
    {
      if (!modification.AnalysisSubMachineModifications) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetAllSubMachineModifications: No sub-machine modifications => return an empty list");
        }
        return new List<IMachineModification> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (Restrictions.Eq ("ParentMachine", modification))
        .List<IMachineModification> ();
    }

    /// <summary>
    /// Delete the modifications of the specified analysis status
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="analysisStatus"></param>
    /// <param name="checkedThread">not null (except in unit tests)</param>
    /// <param name="itemNumberByStep"></param>
    /// <param name="maxNumberOfModifications">approximative...</param>
    /// <param name="maxRunningDateTime">optional</param>
    /// <returns>Completed</returns>
    public bool Delete (IMachine machine, AnalysisStatus analysisStatus, Lemoine.Threading.IChecked checkedThread, int itemNumberByStep, int maxNumberOfModifications, DateTime? maxRunningDateTime)
    {
      Debug.Assert (null != machine);
      Debug.Assert (0 < itemNumberByStep);
      Debug.Assert (0 < maxNumberOfModifications);
      Debug.Assert (itemNumberByStep < maxNumberOfModifications);

      int maxNumberOfSteps = maxNumberOfModifications / itemNumberByStep;

      for (int i = 0; i < maxNumberOfSteps; ++i) {
        checkedThread?.SetActive ();
        IList<long> modificationIds = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SELECT modificationid FROM machinemodificationstatus
WHERE machinemodificationstatusmachineid=:MachineId
  AND analysisstatusid=:AnalysisStatusId
ORDER BY modificationid DESC
LIMIT :Number")
          .AddScalar ("modificationid", NHibernate.NHibernateUtil.Int64)
          .SetParameter ("MachineId", machine.Id)
          .SetParameter ("AnalysisStatusId", (int)analysisStatus)
          .SetParameter ("Number", itemNumberByStep)
          .List<long> ();
        if (!modificationIds.Any ()) {
          if (log.IsInfoEnabled) {
            log.Info ("Delete: all the requested modifications were deleted");
          }
          return true;
        }
        foreach (var modificationId in modificationIds) {
          checkedThread?.SetActive ();
          NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (@"DELETE FROM machinemodificationstatus
WHERE machinemodificationstatusmachineid=:MachineId
  AND modificationid=:ModificationId")
            .SetParameter ("MachineId", machine.Id)
            .SetParameter ("ModificationId", modificationId, NHibernate.NHibernateUtil.Int64)
            .ExecuteUpdate ();
          checkedThread?.SetActive ();
          NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (@"DELETE FROM machinemodification
WHERE machinemodificationmachineid=:MachineId
  AND modificationid=:ModificationId")
            .SetParameter ("MachineId", machine.Id)
            .SetParameter ("ModificationId", modificationId, NHibernate.NHibernateUtil.Int64)
            .ExecuteUpdate ();
        }
        if (maxRunningDateTime.HasValue && (maxRunningDateTime.Value < DateTime.UtcNow)) {
          log.Warn ($"Delete: interrupt the process because the maximum running date/time {maxRunningDateTime} was reached");
          return false;
        }
      }

      log.Warn ($"Delete: not all the modifications were deleted because the maximum number of steps {maxNumberOfSteps} was reached (max number of items was {maxNumberOfModifications})");
      return false;
    }

    /// <summary>
    /// Delete the modifications of the specified analysis status that were completed before a specified date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="analysisStatus"></param>
    /// <param name="maxCompletionDateTime"></param>
    /// <param name="checkedThread">not null (except in unit tests)</param>
    /// <param name="itemNumberByStep"></param>
    /// <param name="maxNumberOfModifications">approximative...</param>
    /// <param name="maxRunningDateTime">optional</param>
    /// <returns>Completed</returns>
    public bool Delete (IMachine machine, AnalysisStatus analysisStatus, DateTime maxCompletionDateTime, Lemoine.Threading.IChecked checkedThread, int itemNumberByStep, int maxNumberOfModifications, DateTime? maxRunningDateTime)
    {
      Debug.Assert (null != machine);
      Debug.Assert (0 < itemNumberByStep);
      Debug.Assert (0 < maxNumberOfModifications);
      Debug.Assert (itemNumberByStep < maxNumberOfModifications);

      int maxNumberOfSteps = maxNumberOfModifications / itemNumberByStep;

      for (int i = 0; i < maxNumberOfSteps; ++i) {
        checkedThread?.SetActive ();
        IList<long> modificationIds = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SELECT modificationid FROM machinemodificationstatus
WHERE machinemodificationstatusmachineid=:MachineId
  AND analysisstatusid=:AnalysisStatusId
  AND analysisend IS NOT NULL
  AND analysisend < :MaxCompletionDateTime
ORDER BY modificationid DESC
LIMIT :Number")
          .AddScalar ("modificationid", NHibernate.NHibernateUtil.Int64)
          .SetParameter ("MachineId", machine.Id)
          .SetParameter ("AnalysisStatusId", (int)analysisStatus)
          .SetParameter ("MaxCompletionDateTime", maxCompletionDateTime, (NHibernate.Type.IType)new Lemoine.NHibernateTypes.UTCDateTimeFullType ())
          .SetParameter ("Number", itemNumberByStep)
          .List<long> ();
        if (!modificationIds.Any ()) {
          if (log.IsInfoEnabled) {
            log.Info ("Delete: all the requested modifications were deleted");
          }
          return true;
        }
        foreach (var modificationId in modificationIds) {
          checkedThread?.SetActive ();
          NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (@"DELETE FROM machinemodificationstatus
WHERE machinemodificationstatusmachineid=:MachineId
  AND modificationid=:ModificationId")
            .SetParameter ("MachineId", machine.Id)
            .SetParameter ("ModificationId", modificationId, NHibernate.NHibernateUtil.Int64)
            .ExecuteUpdate ();
          checkedThread?.SetActive ();
          NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (@"DELETE FROM machinemodification
WHERE machinemodificationmachineid=:MachineId
  AND modificationid=:ModificationId")
            .SetParameter ("MachineId", machine.Id)
            .SetParameter ("ModificationId", modificationId, NHibernate.NHibernateUtil.Int64)
            .ExecuteUpdate ();
        }
        if (maxRunningDateTime.HasValue && (maxRunningDateTime.Value < DateTime.UtcNow)) {
          log.Warn ($"Delete: interrupt the process because the maximum running date/time {maxRunningDateTime} was reached");
          return false;
        }
      }

      log.Warn ($"Delete: not all the modifications were deleted because the maximum number of steps {maxNumberOfSteps} was reached (max number of items was {maxNumberOfModifications})");
      return false;
    }

    /// <summary>
    /// Get the modifications in error strictly after the specified completion order
    /// </summary>
    /// <param name="completionOrder"></param>
    /// <returns></returns>
    public IList<IMachineModification> GetInErrorStrictlyAfter (int completionOrder)
    {
      var machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll (true);
      IEnumerable<IMachineModification> result = new List<IMachineModification> ();
      foreach (var machine in machines) {
        var byMachine = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<MachineModification> ()
          .Add (GetMachineCriterionWithStatus (machine.Id))
          .Add (Restrictions.IsNotNull ("AnalysisCompletionOrder"))
          .Add (Restrictions.Gt ("AnalysisCompletionOrder", completionOrder))
          .Add (ModificationDAO.GetInErrorCriterion ())
          .AddOrder (Order.Desc ("Priority"))
          .AddOrder (Order.Asc ("Id"))
          .List<IMachineModification> ();
        result = result.Concat (byMachine);
      }
      return result
        .OrderBy (m => ((IDataWithId<long>)m).Id)
        .OrderByDescending (m => m.StatusPriority)
        .ToList ();
    }

    /// <summary>
    /// Get a number of modifications with a specified analysis status
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public int GetNumber (AnalysisStatus analysisStatus)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
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
    /// Get the maximum modificationId from the specified machine
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public long? GetMaxModificationId (IMachine machine)
    {
      long? result = null;

      if (machine == null) {
        result = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SELECT MAX(modificationid) AS maxmodificationid
FROM machinemodification")
          .AddScalar ("maxmodificationid", NHibernate.NHibernateUtil.Int64)
          .UniqueResult<long?> ();
      }
      else {
        result = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SELECT MAX(modificationid) AS maxmodificationid
FROM machinemodification
WHERE machinemodificationmachineid=:MachineId")
          .AddScalar ("maxmodificationid", NHibernate.NHibernateUtil.Int64)
          .SetParameter ("MachineId", machine.Id)
          .UniqueResult<long?> ();
      }

      return result;
    }

    /// <summary>
    /// Get the maximum modificationId from the specified machine asynchronously
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public async Task<long?> GetMaxModificationIdAsync (IMachine machine)
    {
      long? result = null;

      if (machine == null) {
        result = await NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SELECT MAX(modificationid) AS maxmodificationid
FROM machinemodification")
          .AddScalar ("maxmodificationid", NHibernate.NHibernateUtil.Int64)
          .UniqueResultAsync<long?> ();
      }
      else {
        result = await NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SELECT MAX(modificationid) AS maxmodificationid
FROM machinemodification
WHERE machinemodificationmachineid=:MachineId")
          .AddScalar ("maxmodificationid", NHibernate.NHibernateUtil.Int64)
          .SetParameter ("MachineId", machine.Id)
          .UniqueResultAsync<long?> ();
      }

      return result;
    }

    /// <summary>
    /// Create rows in analysisstatus tables for the new modifications that don't have any yet.
    /// 
    /// Not optimized version
    /// </summary>
    /// <param name="machine">not null</param>
    void CreateNewAnalysisStatusNotOptimized (IMachine machine)
    {
      bool limitReached;
      CreateNewAnalysisStatus (machine, false, 0, long.MaxValue, out limitReached);
    }

    /// <summary>
    /// Create rows in analysisstatus tables for the new modifications that don't have any yet.
    /// 
    /// No limit version
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="serializable">is the transaction serializable ? Set false if unsafe</param>
    /// <param name="minModificationId">minimum modification id to consider (for optimization)</param>
    /// <returns>Maximum modification id that was inserted</returns>
    public long CreateNewAnalysisStatusNoLimit (IMachine machine, bool serializable, long minModificationId)
    {
      bool limitReached;
      return CreateNewAnalysisStatus (machine, serializable, minModificationId, long.MaxValue, out limitReached);
    }

    /// <summary>
    /// Create rows in analysisstatus tables for the new modifications that don't have any yet.
    /// 
    /// This is required to run it before using GetFirstPendingModification or GetPastPendingModifications
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="serializable">is the transaction serializable ? Set false if unsafe</param>
    /// <param name="minModificationId">minimum modification id to consider (for optimization)</param>
    /// <param name="limit">Maximum number of items to insert</param>
    /// <param name="limitReached">The limit was reached</param>
    /// <returns>Maximum modification id that was inserted</returns>
    public long CreateNewAnalysisStatus (IMachine machine, bool serializable, long minModificationId, long limit, out bool limitReached)
    {
      Debug.Assert (null != machine);

      var modificationIdsRequest = "SELECT modificationid " +
"FROM machinemodification " +
"WHERE machinemodificationmachineid=:MachineId " +
"  AND NOT EXISTS (SELECT 1 FROM machinemodificationstatus " +
"                  WHERE machinemodificationstatusmachineid=:MachineId " +
"                    AND machinemodificationstatus.modificationid = machinemodification.modificationid) " +
"  AND modificationid >= :MinModificationId " +
"ORDER BY modificationid " +
"LIMIT :Limit ;";
      var modificationIds = NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery (modificationIdsRequest)
        .AddScalar ("modificationid", NHibernate.NHibernateUtil.Int64)
        .SetParameter ("MachineId", machine.Id)
        .SetParameter ("MinModificationId", minModificationId)
        .SetParameter ("Limit", limit)
        .List<long> ();
      if (!modificationIds.Any ()) {
        limitReached = false;
        if (serializable) {
          var maxModificationId = GetMaxModificationId (machine);
          if (maxModificationId.HasValue) {
            return maxModificationId.Value;
          }
          else {
            return minModificationId;
          }
        }
        else {
          return minModificationId;
        }
      }

      var insertRequest = GetInsertStatusRequest (machine, modificationIds);
      NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery (insertRequest)
        .ExecuteUpdate ();
      if (limit < long.MaxValue) {
        var count = modificationIds.Count ();
        Debug.Assert (count <= limit);
        limitReached = (limit <= count);
      }
      else {
        limitReached = false;
      }
      return modificationIds.Max ();
    }

    string GetInsertStatusRequest (IMachine machine, IEnumerable<long> modificationIds)
    {
      var modificationIdStringArray = modificationIds
        .Select (m => m.ToString ())
        .ToArray ();
      var modificationIdsString = string.Join (",", modificationIdStringArray);

      // To limit the exclusive locks on the parent table,
      // test first directly in the partitioned table
      // Note: in insertRequest, all the not null empty fields must be specified else an update is run to initialize them
      if (StatusPartitionExists (machine)) {
        // Note: no conflict target in the upsert, so that it does not depend on the unique index of the partition
        var onConflict = IsUpsertActiveInPartition () ? "ON CONFLICT DO NOTHING" : "";
        return $"""
INSERT INTO pgfkpart.machinemodificationstatus_p{machine.Id} (modificationid, machinemodificationstatusmachineid, modificationstatuspriority, analysisiterations, analysistotalduration)
SELECT modificationid, machinemodificationmachineid, modificationpriority, 0, 0
FROM pgfkpart.machinemodification_p{machine.Id} m
WHERE machinemodificationmachineid={machine.Id}
  AND modificationid IN ({modificationIdsString})
{onConflict};
""";
      }
      else { // !partitionExists
        // Note: if the parent table is partitioned by pgfkpart, ON CONFLICT is not effective,
        // because the conflict happens in the partition the trigger inserts into
        var onConflict = Lemoine.Info.ConfigSet.LoadAndGet<bool> (UPSERT_ACTIVE_KEY, UPSERT_ACTIVE_DEFAULT)
          ? "ON CONFLICT (modificationid) DO NOTHING"
          : "";
        return $"""
INSERT INTO machinemodificationstatus (modificationid, machinemodificationstatusmachineid, modificationstatuspriority, analysisiterations, analysistotalduration)
SELECT modificationid, machinemodificationmachineid, modificationpriority, 0, 0
FROM machinemodification
WHERE machinemodificationmachineid={machine.Id}
  AND modificationid IN ({modificationIdsString})
{onConflict};
""";
      }
    }

    static bool IsUpsertActiveInPartition () =>
      Lemoine.Info.ConfigSet.LoadAndGet<bool> (UPSERT_ACTIVE_IN_PARTITION_KEY, UPSERT_ACTIVE_IN_PARTITION_DEFAULT);

    bool StatusPartitionExists (IMachine machine)
    {
      var partitionExistsRequest = $"""
SELECT EXISTS (
SELECT 1 FROM information_schema.tables
WHERE table_schema='pgfkpart'
  AND table_name='machinemodificationstatus_p{machine.Id}'
);
""";
      return NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery (partitionExistsRequest)
        .AddScalar ("exists", NHibernate.NHibernateUtil.Boolean)
        .UniqueResult<bool> ();
    }

    /// <summary>
    /// <see cref="IMachineModificationDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public bool IsConcurrentAnalysisStatusCreationSafe (IMachine machine)
    {
      Debug.Assert (null != machine);

      return IsUpsertActiveInPartition () && StatusPartitionExists (machine);
    }

    /// <summary>
    /// Get all machine modifications not completed that have a revision
    /// </summary>
    /// <param name="application">Name of the application that created the revision (can be null or empty)</param>
    /// <param name="minId">Minimum id to browse (strict)</param>
    /// <returns></returns>
    public IEnumerable<IMachineModification> FindNotCompletedWithRevision (string application, long minId)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (Restrictions.Gt ("Id", minId))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (ModificationDAO.GetNotCompletedCriterion ())
        .Add (Restrictions.IsNotNull ("Revision"));
      if (!string.IsNullOrEmpty (application)) {
        criteria = criteria
          .CreateAlias ("Revision", "r")
          .Add (Restrictions.Eq ("r.Application", application));
      }
      return criteria.List<IMachineModification> ();
    }

    /// <summary>
    /// Get all machine modifications related to a revision
    /// </summary>
    /// <param name="revision">Cannot be null</param>
    /// <param name="minId">Minimum ID of the modifications returned (strict)</param>
    /// <returns></returns>
    public IList<IMachineModification> FindByRevision (IRevision revision, long minId)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModification> ()
        .Add (Restrictions.Eq ("Revision", revision))
        .Add (Restrictions.Gt ("Id", minId))
        .List<IMachineModification> ();
    }
  }
}
