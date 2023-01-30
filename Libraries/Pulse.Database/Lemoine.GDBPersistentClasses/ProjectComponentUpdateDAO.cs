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
  /// Implementation of <see cref="Lemoine.ModelDAO.IProjectComponentUpdateDAO">IProjectComponentUpdateDAO</see>
  /// </summary>
  public class ProjectComponentUpdateDAO
    : SaveOnlyNHibernateDAO<ProjectComponentUpdate, IProjectComponentUpdate, long>
    , IProjectComponentUpdateDAO
  {  
    static readonly ILog log = LogManager.GetLogger(typeof (ProjectComponentUpdateDAO).FullName);
  
    /// <summary>
    /// Find all the ProjectComponentUpdate for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IProjectComponentUpdate> FindAllWithComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProjectComponentUpdate> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<IProjectComponentUpdate> ();
    }
  }
}
