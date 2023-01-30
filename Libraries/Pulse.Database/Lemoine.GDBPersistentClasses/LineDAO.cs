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
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ILineDAO">ILineDAO</see>
  /// </summary>
  public class LineDAO
    : VersionableNHibernateDAO<Line, ILine, int>
    , ILineDAO
  {
    /// <summary>
    /// Find all the lines that may match a component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<ILine> FindAllByComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Line> ()
        .CreateAlias ("Components", "AssociatedComponents")
        .Add (Restrictions.Eq ("AssociatedComponents.Id", ((IDataWithId)component).Id))
        .List<ILine> ();
    }
  }
}
