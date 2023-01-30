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
  /// Implementation of <see cref="Lemoine.ModelDAO.IToolDAO">IToolDAO</see>
  /// </summary>
  public class ToolDAO
    : VersionableNHibernateDAO<Tool, ITool, int>
    , IToolDAO
  {

    /// <summary>
    /// Find tool by code (unique)
    /// </summary>
    /// <param name="toolCode"></param>
    /// <returns></returns>
    public ITool FindByCode(string toolCode) {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<Tool> ()
        .Add (Restrictions.Eq ("Code", toolCode))
        .SetCacheable (true)
        .UniqueResult<ITool> ();
    }

    /// <summary>
    /// Find tool by name (non-unique)
    /// </summary>
    /// <param name="toolName"></param>
    /// <returns></returns>
    public IList<ITool> FindByName(string toolName) {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Tool> ()
        .Add (Restrictions.Eq ("Name", toolName))
        .SetCacheable (true)
        .List<ITool> ();
    }
  }
}
