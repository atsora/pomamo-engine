// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Generic NHibernate DAO
  /// for tables that are partitioned by machine
  /// </summary>
  public abstract class GenericByMachineNHibernateDAO<T, I, ID>
    : BaseGenericNHibernateDAO<T, I, ID>, IGenericByMachineDAO<I, ID>
    where T : class, I
    where I : Lemoine.Model.IPartitionedByMachine
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericByMachineNHibernateDAO<T, I, ID>).FullName);

    #region Members
    readonly IList<string> m_machineProperties = new List<string> ();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Machine properties (usually {"Machine"})
    /// </summary>
    protected virtual IList<string> MachineProperties
    {
      get { return m_machineProperties; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineProperty">Usually Machine</param>
    protected GenericByMachineNHibernateDAO (string machineProperty)
    {
      m_machineProperties.Add (machineProperty);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineProperties">Usually {"Machine"}</param>
    protected GenericByMachineNHibernateDAO (IList<string> machineProperties)
    {
      m_machineProperties = machineProperties;
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public I FindById (ID id, Lemoine.Model.IMachine machine)
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
    protected virtual I FindById (ID id, int machineId)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"FindById: id={id} machineId={machineId} type={typeof (T)}");
      }

      // Note: entityInCache may be a MachineModificationProxy for example
      // that can't be converted directly to the right modification (e.g. ReasonMachineAssociation),
      // hence the exception management and the cast try
      try {
        object entityInCache;
        if (typeof (T).IsSubclassOf (typeof (MachineModification))) {
          entityInCache = NHibernateHelper.GetCurrentSession ()
            .GetPersistentCacheOnly<MachineModification> (id);
        }
        else {
          entityInCache = NHibernateHelper.GetCurrentSession ()
            .GetPersistentCacheOnly<T> (id);
        }
        if ((null != entityInCache)
          && NHibernateUtil.IsInitialized (entityInCache)
          && (entityInCache is I)) {
          var convertedEntity = (I)(T)entityInCache;
          if (log.IsDebugEnabled) {
            log.Debug ($"FindById: the entity id={id} machineId={machineId} type={typeof (T)} is taken from cache");
          }
          return convertedEntity;
        }
      }
      catch (Exception ex) {
        if (log.IsWarnEnabled) {
          log.Warn ("FindById: getting the item in the persistent cache failed", ex);
        }
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
    /// Get a criterion on the machine
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    protected internal virtual ICriterion GetMachineCriterion (int machineId)
    {
      if (0 == m_machineProperties.Count) {
        return Expression.Sql ("TRUE");
      }
      else if (1 == this.MachineProperties.Count) {
        return Restrictions.Eq (this.MachineProperties[0] + ".Id", machineId);
      }
      else {
        Conjunction conjunction = Restrictions.Conjunction ();
        foreach (string machineProperty in this.MachineProperties) {
          conjunction.Add (Restrictions.Eq (machineProperty + ".Id", machineId));
        }
        return conjunction;
      }
    }

    /// <summary>
    /// Get a criterion on the machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    protected internal virtual ICriterion GetMachineCriterion (IMachine machine)
    {
      Debug.Assert (null != machine);
      if (0 == m_machineProperties.Count) {
        return Expression.Sql ("TRUE");
      }
      else if (1 == this.MachineProperties.Count) {
        return Restrictions.Eq (this.MachineProperties[0], machine);
      }
      else {
        Conjunction conjunction = Restrictions.Conjunction ();
        foreach (string machineProperty in this.MachineProperties) {
          conjunction.Add (Restrictions.Eq (machineProperty, machine));
        }
        return conjunction;
      }
    }
  }
}
