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
  /// Implementation of <see cref="Lemoine.ModelDAO.IStampDAO">IStampDAO</see>
  /// </summary>
  public class StampDAO
    : VersionableNHibernateDAO<Stamp, IStamp, int>
    , IStampDAO
  {
    
    /// <summary>
    /// FindAll stamps for a given IsoFile
    /// </summary>
    /// <returns>list of stamps of an IsoFile</returns>
    public IList<IStamp> FindAllWithIsoFile (IIsoFile isoFile)  {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Stamp> ()
        .Add(Restrictions.Eq("IsoFile", isoFile))
        .AddOrder(Order.Asc ("Id"))
        .List<IStamp> ();
    }
    
    /// <summary>
    /// Get all the stamps which have an associated position
    /// for a given IsoFile
    /// and return the result by ascending position
    /// </summary>
    /// <param name="isoFileId"></param>
    /// <returns></returns>
    public IList<IStamp> GetAllWithAscendingPosition (int isoFileId) {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Stamp>  ()
        .Add (Expression.Eq ("IsoFile.Id", isoFileId))
        .Add (Expression.IsNotNull ("Position"))
        .AddOrder (Order.Asc ("Position"))
        .List <IStamp> ();
    }
    
    /// <summary>
    /// FindAll stamps for a given sequence
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>
    public IList<IStamp> FindAllWithSequence (ISequence sequence) {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Stamp>  ()
        .Add (Expression.Eq ("Sequence", sequence))
        .List <IStamp> ();
    }
    
    /// <summary>
    /// Find all the stamps for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IStamp> FindAllWithComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Stamp> ()
        .Add (Expression.Eq ("Component", component))
        .List<IStamp> ();
    }
  }
}
