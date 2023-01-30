// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICadModelDAO">ICadModelDAO</see>
  /// </summary>
  public class CadModelDAO
    : VersionableNHibernateDAO<CadModel, ICadModel, int>
    , ICadModelDAO
  {
    /// <summary>
    /// Find all the Cad models for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<ICadModel> FindAllWithComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<CadModel> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<ICadModel> ();
    }
  }
}
