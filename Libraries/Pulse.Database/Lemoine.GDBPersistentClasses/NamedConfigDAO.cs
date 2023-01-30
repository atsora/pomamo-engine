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
  /// Implementation of <see cref="Lemoine.ModelDAO.INamedConfigDAO">INamedConfigDAO</see>
  /// </summary>
  public class NamedConfigDAO
    : VersionableNHibernateDAO<NamedConfig, INamedConfig, int>
    , INamedConfigDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (NamedConfigDAO).FullName);

    /// <summary>
    /// Find the config where the key is like a given filter
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="keyPattern"></param>
    /// <returns></returns>
    public IList<string> GetNames (string keyPattern)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<NamedConfig> ()
        .Add (Restrictions.Like ("Key", keyPattern))
        .SetCacheable (true)
        .SetProjection (Projections.Distinct (Projections.Property("Name")))
        .List<string> ();
    }

    /// <summary>
    /// Get the config for the specified named and key
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public INamedConfig GetConfig (string name, string key)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria <NamedConfig> ()
        .Add (Restrictions.Eq ("Name", name))
        .Add (Restrictions.Eq ("Key", key))
        .SetCacheable (true)
        .UniqueResult <INamedConfig> ();
    }
    
    /// <summary>
    /// Get a set of configs for the specified named and key pattern (with the SQL like syntax)
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="name"></param>
    /// <param name="keyPattern"></param>
    /// <returns></returns>
    public IList<INamedConfig> GetConfigs (string name, string keyPattern)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria <NamedConfig> ()
        .Add (Restrictions.Eq ("Name", name))
        .Add (Restrictions.Like ("Key", keyPattern))
        .SetCacheable (true)
        .List <INamedConfig> ();
    }
    
    /// <summary>
    /// Force a configuration, without first checking the previous configuration value
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void SetConfig (string name, string key, object v)
    {
      INamedConfig config = GetConfig (name, key);
      if (null == config) { // The configuration does not exist => create a new one
        log.DebugFormat ("SetConfig: " +
                         "create a new configuration for name={0} key={1}",
                         name, key);
        config = ModelDAOHelper.ModelFactory.CreateNamedConfig (name, key);
        config.Value = v;
        MakePersistent (config);
      }
      else { // The configuration already exists => update it
        log.DebugFormat ("SetConfig: " +
                         "the configuration already exists for name={0} key={1}, " +
                         "=> update value from {2} to {3}",
                         name, key,
                         config.Value, v);
        config.Value = v;
        MakePersistent (config);
      }
    }
  }
}
