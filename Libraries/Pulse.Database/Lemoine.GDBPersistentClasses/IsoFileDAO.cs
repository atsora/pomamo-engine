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
  /// Description of IsoFileDAO.
  /// </summary>
  public class IsoFileDAO
    : VersionableNHibernateDAO<IsoFile, IIsoFile, int>, IIsoFileDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IsoFileDAO).FullName);

    /// <summary>
    /// Try to get the IsoFile entity matching the stamping
    /// of a source Iso file into a target directory
    /// </summary>
    /// <param name="sourceIsoFileName"></param>
    /// <param name="sourceIsoFileDirectory"></param>
    /// <param name="targetIsoDirectory"></param>
    /// <returns></returns>
    public IList<IIsoFile> GetIsoFile (string sourceIsoFileName,
                                      string sourceIsoFileDirectory,
                                      string targetIsoDirectory)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IsoFile> ()
        .Add (Restrictions.Eq ("Name", sourceIsoFileName))
        .Add (Restrictions.Eq ("SourceDirectory", sourceIsoFileDirectory))
        .Add (Restrictions.Eq ("StampingDirectory", targetIsoDirectory))
        .List<IIsoFile> ();
    }

    /// <summary>
    /// Try to get the IsoFile from a program name
    /// 
    /// Because the like condition is pretty inefficient (the btree index does not support it)
    /// only exact matches are considered now
    /// </summary>
    /// <param name="programName"></param>
    /// <returns></returns>
    public IList<IIsoFile> GetIsoFile (string programName)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IsoFile> ()
        .Add (Restrictions.Eq ("Name", programName))
        /*
        // The like condition is inefficient
        // because the btree index can't manage the like condition
        .Add (Expression.Disjunction ()
              .Add (Expression.Eq ("Name", programName))
              .Add (Expression.Like ("Name", programName + ".%")))
        */
        .AddOrder (Order.Desc ("Id"))
        .List<IIsoFile> ();
    }
  }
}
