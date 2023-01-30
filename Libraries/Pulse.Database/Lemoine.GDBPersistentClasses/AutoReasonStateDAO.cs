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
  /// Implementation of <see cref="Lemoine.ModelDAO.IAutoReasonStateDAO">IAutoReasonStateDAO</see>
  /// </summary>
  public class AutoReasonStateDAO
    : VersionableByMonitoredMachineNHibernateDAO<AutoReasonState, IAutoReasonState, int>
    , IAutoReasonStateDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonStateDAO).FullName);

    /// <summary>
    /// Get the applicationState for the specified machine and key
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public IAutoReasonState GetAutoReasonState (IMonitoredMachine machine, string key)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<AutoReasonState> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.Eq ("Key", key))
        .SetCacheable (true)
        .UniqueResult<IAutoReasonState> ();
    }
  }
}
