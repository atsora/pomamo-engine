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
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncAcquisitionDAO">ICncAcquisitionDAO</see>
  /// </summary>
  public class CncAcquisitionDAO
    : VersionableNHibernateDAO<CncAcquisition, ICncAcquisition, int>
    , ICncAcquisitionDAO
  {
    /// <summary>
    /// Find by Id an ICncAcquisition
    /// with an eager fetch of:
    /// <item>MachineModules</item>
    /// <item>MachineModules.MonitoredMachine</item>
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ICncAcquisition FindByIdWithMonitoredMachine (int id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAcquisition> ()
        .Add (Restrictions.Eq ("Id", id))
        .Fetch (SelectMode.Fetch, "MachineModules")
        .Fetch (SelectMode.Fetch, "MachineModules.MonitoredMachine")
        // .SetCacheable (true) // SetCacheable is not behaving well with Fetch (still the case ?)
        .UniqueResult<ICncAcquisition> ();
    }

    /// <summary>
    /// Find all ICncAcquisition
    /// with an eager fetch of:
    /// <item>Computer</item>
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<ICncAcquisition> FindAllWithChildren ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAcquisition> ()
        .Fetch (SelectMode.Fetch, "Computer")
        // .SetCacheable (true) // SetCacheable is not behaving well with Fetch (still the case ?)
        .List<ICncAcquisition> ();
    }

    /// <summary>
    /// Find all ICncAcquisition for a computer
    /// with an eager fetch of:
    /// <item>MachineModules</item>
    /// <item>MachineModules.MonitoredMachine</item>
    /// <item>MachineModules.MonitoredMachine.MonitoringType</item>
    /// </summary>
    /// <param name="computer">not null</param>
    /// <returns></returns>
    public IList<ICncAcquisition> FindAllForComputer (IComputer computer)
    {
      Debug.Assert (null != computer);

      IList<ICncAcquisition> result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAcquisition> ()
        .Add (Restrictions.Eq ("Computer.Id", computer.Id))
        .Fetch (SelectMode.Fetch, "MachineModules")
        .Fetch (SelectMode.Fetch, "MachineModules.MonitoredMachine")
        .Fetch (SelectMode.Fetch, "MachineModules.MonitoredMachine.MonitoringType")
        // Note: without the following line, some rows are duplicated.Add Is it a bug ? (TODO: still the case ?)
        .SetResultTransformer(NHibernate.Transform.Transformers.DistinctRootEntity)
        // .SetCacheable (true) // SetCacheable is not behaving well with Fetch (still the case ?)
        .List<ICncAcquisition> ();
      return result;
    }
  }
}
