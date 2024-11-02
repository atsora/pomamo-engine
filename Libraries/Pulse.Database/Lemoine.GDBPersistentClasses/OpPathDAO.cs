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
  /// Implementation of <see cref="Lemoine.ModelDAO.IPathDAO">IPathDAO</see>
  /// </summary>
  public class OpPathDAO
    : VersionableNHibernateDAO<OpPath, IPath, int>
    , IPathDAO
  {
    /// <summary>
    /// Find path for a given operation and number
    /// </summary>
    /// <returns></returns>
    public IPath FindByOperationAndNumber (IOperation operation, int pathNumber)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OpPath> ()
        .Add(Restrictions.Eq("Operation", operation))
        .Add(Restrictions.Eq("Number", pathNumber))
        .UniqueResult<IPath> ();
    }
    
    /// <summary>
    /// FindAll paths associated with an operation
    /// sorted by Number
    /// </summary>
    /// <returns>list of paths of an operation</returns>
    public IList<IPath> FindAllWithOperation (IOperation operation)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OpPath> ()
        .Add(Restrictions.Eq("Operation", operation))
        .AddOrder(Order.Asc ("Number"))
        .List<IPath> ();
    }

    /// <summary>
    /// Initialize the associated sequences
    /// </summary>
    /// <param name="path"></param>
    public void InitializeSequences (IPath path)
    {
      NHibernateUtil.Initialize (path.Sequences);
    }
  }
}
