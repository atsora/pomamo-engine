// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IPluginDAO">IPluginDAO</see>
  /// </summary>
  public class PluginDAO
    : VersionableNHibernateDAO<Plugin, IPlugin, int>
    , IPluginDAO
  {
    /// <summary>
    /// Find a plugin with its identifying name
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <returns></returns>
    public IPlugin FindByName(string identifyingName)
    {
      Debug.Assert(!string.IsNullOrEmpty(identifyingName), "name cannot be null or empty");
      
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<IPlugin>()
        .Add(Restrictions.Eq("IdentifyingName", identifyingName))
        .UniqueResult<IPlugin>();
    }
  }
}
