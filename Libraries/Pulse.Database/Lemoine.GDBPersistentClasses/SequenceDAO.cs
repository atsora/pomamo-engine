// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Sequence> ()
        .Add(Restrictions.Eq("Operation", operation))
        .AddOrder(Order.Asc ("Order"))
        .List<ISequence> ();
    }

    /// <summary>
    /// FindAll sequences associated with a path
    /// sorted by order
    /// </summary>
    /// <returns>list of sequences of a path</returns>
    public IList<ISequence> FindAllWithPath (IPath path)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Sequence> ()
        .Add(Restrictions.Eq("Path", path))
        .AddOrder(Order.Asc ("Order"))
        .List<ISequence> ();
    }
    
  }
}
