// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Cfg;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// PersistentClassModel
  /// </summary>
  public class PersistentClassModel: IPersistentClassModel
  {
    readonly ILog log = LogManager.GetLogger (typeof (PersistentClassModel).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public PersistentClassModel ()
    {
    }

    /// <summary>
    /// <see cref="IPersistentClassModel"/>
    /// </summary>
    public Assembly[] Assemblies => new Assembly[] { typeof (PersistentClassModel).Assembly };

    /// <summary>
    /// <see cref="IPersistentClassModel"/>
    /// </summary>
    /// <param name="sessionFactory"></param>
    /// <param name="configuration"></param>
    /// <param name="migrationSuccess"></param>
    /// <returns></returns>
    public bool AddDefaultValues (NHibernate.ISessionFactory sessionFactory, Configuration configuration, bool migrationSuccess)
    {
      DefaultValues defaultValues =
        new DefaultValues (sessionFactory,
                           configuration.GetProperty ("connection.connection_string"));
      // Keep a cache if the default values were completed only in case of a migration success
      // so that the default values are inserted again after a migration success
      // This is done not to skip any default values that may be inserted only after a migration success
      return defaultValues.ConnectAndInsertDefaultValues (migrationSuccess);
    }

    /// <summary>
    /// <see cref="IPersistentClassModel"/>
    /// </summary>
    /// <param name="sessionFactory"></param>
    /// <param name="configuration"></param>
    /// <param name="migrationSuccess"></param>
    /// <returns></returns>
    public async Task<bool> AddDefaultValuesAsync (NHibernate.ISessionFactory sessionFactory, Configuration configuration, bool migrationSuccess)
    {
      DefaultValues defaultValues =
        new DefaultValues (sessionFactory,
                           configuration.GetProperty ("connection.connection_string"));
      // Keep a cache if the default values were completed only in case of a migration success
      // so that the default values are inserted again after a migration success
      // This is done not to skip any default values that may be inserted only after a migration success
      return await defaultValues.ConnectAndInsertDefaultValuesAsync (migrationSuccess);
    }

    public ISessionAccumulator SessionAccumulator => AnalysisAccumulator.Instance;
  }
}
