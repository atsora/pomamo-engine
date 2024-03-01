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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ISequenceDAO">ISequenceDAO</see>
  /// </summary>
  public class SequenceDAO
    : VersionableNHibernateDAO<Sequence, ISequence, int>
    , ISequenceDAO
  {
    /// <summary>
    /// FindAll sequences associated with an operation
    /// sorted by order
    /// </summary>
    /// <returns>list of sequences of an operation</returns>
    public IList<ISequence> FindAllWithOperation (IOperation operation)
    {
      if (operation is null) {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Sequence> ()
          .Add (Restrictions.IsNull ("Operation"))
          .AddOrder (Order.Asc ("Order"))
          .List<ISequence> ();
      }
      else {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Sequence> ()
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
          .CreateCriteria<Sequence> ()
          .Add (Restrictions.IsNull ("Path"))
          .AddOrder (Order.Asc ("Order"))
          .List<ISequence> ();
      }
      else {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Sequence> ()
          .Add (Restrictions.Eq ("Path.Id", path.Id))
          .AddOrder (Order.Asc ("Order"))
          .List<ISequence> ();
      }
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="entity"></param>
    public override void Lock (ISequence entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Lock ("opseq", entity, NHibernate.LockMode.None); // TODO: probably not necessary, to validate
    }

    /// <summary>
    /// Reload an entity (for example after an update operation fails or because it was changed somewhere else)
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ISequence Reload (ISequence entity)
    {
      if (NHibernateUtil.IsInitialized (entity)) {
        // Note: there are two methods to reload an entity
        // 1. Use Refresh, although they may be issues when elements of child collections have been deleted
        // 2. Use Evict () followed by Load ()
        // But do not use Merge that does not reload the data at all when it is already persistent
        NHibernateHelper.GetCurrentSession ().Evict (entity);
        var result = NHibernateHelper.GetCurrentSession ().Load<Sequence> (entity.Id);
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
