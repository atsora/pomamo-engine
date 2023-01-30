// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using NHibernate.Cfg;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Interface to implement the new definition of a set of persistent classes
  /// </summary>
  public interface IPersistentClassModel
  {
    /// <summary>
    /// Assemblies where persistent classes are set
    /// </summary>
    Assembly[] Assemblies { get; }

    /// <summary>
    /// Add default values synchronously
    /// </summary>
    bool AddDefaultValues (NHibernate.ISessionFactory sessionFactory, Configuration configuration, bool migrationSuccess);

    /// <summary>
    /// Add default value asynchronously
    /// </summary>
    System.Threading.Tasks.Task<bool> AddDefaultValuesAsync (NHibernate.ISessionFactory sessionFactory, Configuration configuration, bool migrationSuccess);

    /// <summary>
    /// Session accumulator
    /// 
    /// nullable
    /// </summary>
    ISessionAccumulator SessionAccumulator { get; }
  }
}
