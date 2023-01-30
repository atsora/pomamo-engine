// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Generic NHiberate DAO for the IVersionable classes
  /// for tables that are partitioned by machine
  /// 
  /// This DAO allows save and update actions.
  /// </summary>
  public abstract class VersionableByMachineNHibernateDAO<T, I, ID>
    : BaseVersionableNHibernateDAO<T, I, ID>, IGenericByMachineUpdateDAO<I, ID>
    where T : class, I, Lemoine.Collections.IDataWithId<ID>
    where I : Lemoine.Model.IDataWithVersion, Lemoine.Model.IPartitionedByMachine
  {
    readonly ILog log = LogManager.GetLogger (typeof (VersionableByMachineNHibernateDAO<T, I, ID>).FullName);

    #region Members
    readonly string m_machineProperty;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Machine property (usually Machine)
    /// </summary>
    protected virtual string MachineProperty
    {
      get { return m_machineProperty; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineProperty">Usually Machine</param>
    protected VersionableByMachineNHibernateDAO (string machineProperty)
    {
      m_machineProperty = machineProperty;
    }

    /// <summary>
    /// Get a criterion on the machine
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    protected internal virtual ICriterion GetMachineCriterion (int machineId)
    {
      return Restrictions.Eq (this.MachineProperty + ".Id", machineId);
    }

    /// <summary>
    /// Get a criterion on the machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    protected internal virtual ICriterion GetMachineCriterion (IMachine machine)
    {
      Debug.Assert (null != machine);
      return Restrictions.Eq (this.MachineProperty, machine);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public I FindById (ID id, IMachine machine)
    {
      Debug.Assert (null != machine);
      return FindById (id, machine.Id);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public I FindById (ID id, int machineId)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"FindById: id={id} machineId={machineId} type={typeof (T)}");
      }

      if (TryGetInCache (id, out I entityInCache)) {
        return entityInCache;
      }

      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: run a request to get the entity id={0} type={1}", id, typeof (T));
      }
      var entity = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<T> ()
        .Add (Restrictions.Eq ("Id", id))
        .Add (GetMachineCriterion (machineId))
        .UniqueResult<I> ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: " +
                        "the entity id={0} type={1} was queried",
                        id, typeof (T));
      }
      return entity;
    }

    /// <summary>
    /// FindByIdAsync implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public async Task<I> FindByIdAsync (ID id, IMachine machine)
    {
      Debug.Assert (null != machine);
      return await FindByIdAsync (id, machine.Id);
    }

    /// <summary>
    /// FindByIdAsync implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public async Task<I> FindByIdAsync (ID id, int machineId)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"FindByIdAsync: id={id} machineId={machineId} type={typeof (T)}");
      }

      if (TryGetInCache (id, out I entityInCache)) {
        return entityInCache;
      }

      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindByIdAsync: run a request to get the entity id={0} type={1}", id, typeof (T));
      }
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<T> ()
        .Add (Restrictions.Eq ("Id", id))
        .Add (GetMachineCriterion (machineId));
      var entity = await criteria.UniqueResultAsync<I> ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindByIdAsync: " +
                        "the entity id={0} type={1} was queried",
                        id, typeof (T));
      }
      return entity;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public I FindByIdAndLock (ID id, IMachine machine)
    {
      return FindByIdAndLock (id, machine.Id);
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public I FindByIdAndLock (ID id, int machineId)
    {
      I entity = FindById (id, machineId);
      UpgradeLock (entity);
      return entity;
    }
  }
}
