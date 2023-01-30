// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;
using NHibernate;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMonitoredMachineAnalysisStatusDAO">IMonitoredMachineAnalysisStatusDAO</see>
  /// </summary>
  public class MonitoredMachineAnalysisStatusDAO
    : VersionableNHibernateDAO<MonitoredMachineAnalysisStatus, IMonitoredMachineAnalysisStatus, int>
    , IMonitoredMachineAnalysisStatusDAO
  {
    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IMonitoredMachineAnalysisStatus FindById (int machineId)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MonitoredMachineAnalysisStatus")) {
        return base.FindById (machineId);
      }
    }

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<IMonitoredMachineAnalysisStatus> FindByIdAsync (int machineId)
    {
      // TODO: await using when available
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MonitoredMachineAnalysisStatus")) {
        return await base.FindByIdAsync (machineId);
      }
    }

    /// <summary>
    /// Specific FindByIdAndLock where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IMonitoredMachineAnalysisStatus FindByIdAndLock (int machineId)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MonitoredMachineAnalysisStatus")) {
        return base.FindByIdAndLock (machineId);
      }
    }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IMonitoredMachineAnalysisStatus MakePersistent (IMonitoredMachineAnalysisStatus entity)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MonitoredMachineAnalysisStatus")) {
        return base.MakePersistent (entity);
      }
    }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IMonitoredMachineAnalysisStatus> MakePersistentAsync (IMonitoredMachineAnalysisStatus entity)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MonitoredMachineAnalysisStatus")) {
        return await base.MakePersistentAsync (entity);
      }
    }

    /// <summary>
    /// Specific Reload where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IMonitoredMachineAnalysisStatus Reload (IMonitoredMachineAnalysisStatus entity)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MonitoredMachineAnalysisStatus")) {
        return base.Reload (entity);
      }
    }

    /// <summary>
    /// Get the minimum activity analysis date/time
    /// 
    /// Be careful: this method accesses to the data of all the machines
    /// (and may lock the database more than wanted).
    /// This is why it is not used any more in the analysis service
    /// </summary>
    /// <returns></returns>
    public DateTime? GetMinActivityAnalysisDateTime ()
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("MinActivityAnalysisDateTime");
      return query.UniqueResult<DateTime?> ();
    }

    /// <summary>
    /// Get the last monitored machine analysis status
    /// </summary>
    /// <returns></returns>
    public IMonitoredMachineAnalysisStatus GetLast ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachineAnalysisStatus> ()
        .AddOrder (Order.Desc ("ActivityAnalysisDateTime"))
        .SetMaxResults (1)
        .UniqueResult<IMonitoredMachineAnalysisStatus> ();
    }
  }
}
