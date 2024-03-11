// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ISequenceDAO">ISequenceDAO</see>
  /// </summary>
  public class SequenceDAO
    : ISequenceDAO
  {
    ILog log = LogManager.GetLogger<SequenceDAO> ();

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual ISequence FindById (int id)
    {
      return (ISequence)NHibernateHelper.GetCurrentSession ()
        .Get ("opseq", id);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async virtual System.Threading.Tasks.Task<ISequence> FindByIdAsync (int id)
    {
      var o = await NHibernateHelper.GetCurrentSession ()
        .GetAsync ("opseq", id);
      return (ISequence)o;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual ISequence FindByIdAndLock (int id)
    {
      return (ISequence)NHibernateHelper.GetCurrentSession ()
        .Get ("opseq", id, NHibernate.LockMode.Upgrade);
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual ISequence MakePersistent (ISequence entity)
    {
      // TODO: SaveOrUpdate does not look reliable
      //       in the case an Id was set after a first Save
      //       but the transaction fails after that
      NHibernateHelper.GetCurrentSession ()
        .SaveOrUpdate ("opseq", entity);
      return entity;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task<ISequence> MakePersistentAsync (ISequence entity)
    {
      // TODO: SaveOrUpdate does not look reliable
      //       in the case an Id was set after a first Save
      //       but the transaction fails after that
      await NHibernateHelper.GetCurrentSession ()
        .SaveOrUpdateAsync ("opseq", entity);
      return entity;
    }

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// 
    /// Please note this is pretty inefficient when the machine is partitioned
    /// (the foreign key is not used for the moment)
    /// </summary>
    /// <param name="entity"></param>
    public virtual void UpgradeLock (ISequence entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Lock ("opseq", entity, NHibernate.LockMode.Upgrade);
    }

    /// <summary>
    /// Try to get an entity in cache
    /// 
    /// This is used by partitioned tables
    /// </summary>
    /// <param name="id"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    protected bool TryGetInCache (int id, out ISequence result)
    {
      // Note: entityInCache may be a MachineModificationProxy for example
      // that can't be converted directly to the right modification (e.g. ReasonMachineAssociation),
      // hence the exception management and the cast try
      try {
        var entityInCache = (Sequence)NHibernateHelper.GetCurrentSession ()
        .GetPersistentCacheOnly ("opseq", id);
        if ((null != entityInCache)
          && NHibernateUtil.IsInitialized (entityInCache)
          && (entityInCache is ISequence)) {
          var convertedEntity = (ISequence)entityInCache;
          if (log.IsDebugEnabled) {
            log.Debug ($"TryGetInCache: the entity id={id} type={typeof (Sequence)} is taken from cache");
          }
          result = convertedEntity;
          return true;
        }
        else {
          result = default (Sequence);
          return false;
        }
      }
      catch (Exception ex) {
        if (log.IsWarnEnabled) {
          log.Warn ("TryGetInCache: getting the item in the persistent cache failed", ex);
        }
        result = default (Sequence);
        return false;
      }
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public virtual IList<ISequence> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria ("opseq")
        .List<ISequence> ();
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public virtual async Task<IList<ISequence>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria ("opseq")
        .ListAsync<ISequence> ();
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="entity"></param>
    public virtual void MakeTransient (ISequence entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Delete ("opseq", entity);
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="entity"></param>
    public virtual async System.Threading.Tasks.Task MakeTransientAsync (ISequence entity)
    {
      await NHibernateHelper.GetCurrentSession ()
        .DeleteAsync ("opseq", entity);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="entity"></param>
    public virtual void Lock (ISequence entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Lock ("opseq", entity, NHibernate.LockMode.None);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="entity"></param>
    public virtual async System.Threading.Tasks.Task LockAsync (ISequence entity)
    {
      await NHibernateHelper.GetCurrentSession ()
        .LockAsync ("opseq", entity, NHibernate.LockMode.None);
    }

    /// <summary>
    /// FindAll sequences associated with an operation
    /// sorted by order
    /// </summary>
    /// <returns>list of sequences of an operation</returns>
    public IList<ISequence> FindAllWithOperation (IOperation operation)
    {
      if (operation is null) {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria ("opseq")
          .Add (Restrictions.IsNull ("Operation"))
          .AddOrder (Order.Asc ("Order"))
          .List<ISequence> ();
      }
      else {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria ("opseq")
          .Add (Restrictions.Eq ("Operation.Id", operation.Id))
          .AddOrder (Order.Asc ("Order"))
          .List<ISequence> ();
      }
    }

    /// <summary>
    /// FindAll sequences associated with a path
    /// sorted by order
    /// </summary>
    /// <returns>list of sequences of a path</returns>
    public IList<ISequence> FindAllWithPath (IPath path)
    {
      if (path is null) {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria ("opseq")
          .Add (Restrictions.IsNull ("Path"))
          .AddOrder (Order.Asc ("Order"))
          .List<ISequence> ();
      }
      else {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria ("opseq")
          .Add (Restrictions.Eq ("Path.Id", path.Id))
          .AddOrder (Order.Asc ("Order"))
          .List<ISequence> ();
      }
    }

    /// <summary>
    /// Reload an entity (for example after an update operation fails or because it was changed somewhere else)
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual ISequence Reload (ISequence entity)
    {
      if (NHibernateUtil.IsInitialized (entity)) {
        // Note: there are two methods to reload an entity
        // 1. Use Refresh, although they may be issues when elements of child collections have been deleted
        // 2. Use Evict () followed by Load ()
        // But do not use Merge that does not reload the data at all when it is already persistent
        NHibernateHelper.GetCurrentSession ().Evict (entity);
        var result = (ISequence)NHibernateHelper.GetCurrentSession ().Load ("opseq", entity.Id);
        NHibernateUtil.Initialize (result.Tool);
        NHibernateUtil.Initialize (result.Operation);
        NHibernateUtil.Initialize (result.Path);
        NHibernateUtil.Initialize (result.Detail);
        return result;
      }
      else {
        NHibernateUtil.Initialize (entity);
        var result = entity;
        NHibernateUtil.Initialize (result.Tool);
        NHibernateUtil.Initialize (result.Operation);
        NHibernateUtil.Initialize (result.Path);
        NHibernateUtil.Initialize (result.Detail);
        return result;
      }
    }

  }
}
