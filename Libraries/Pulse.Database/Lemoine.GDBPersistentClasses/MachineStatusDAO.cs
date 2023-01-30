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
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStatusDAO">IMachineStatusDAO</see>
  /// </summary>
  public class MachineStatusDAO
    : VersionableNHibernateDAO<MachineStatus, IMachineStatus, int>
    , IMachineStatusDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStatusDAO).FullName);

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IMachineStatus FindById (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
          return base.FindById (machineId);
        }
      }

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<IMachineStatus> FindByIdAsync (int machineId)
    {
      // TODO: await using when available
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
        return await base.FindByIdAsync (machineId);
      }
    }

    /// <summary>
    /// Specific FindByIdAndLock where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IMachineStatus FindByIdAndLock (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
          return base.FindByIdAndLock (machineId);
        }
      }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IMachineStatus MakePersistent(IMachineStatus entity)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
        return base.MakePersistent(entity);
      }
    }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IMachineStatus> MakePersistentAsync (IMachineStatus entity)
    {
      // Note: await using is available only in C# 8
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
        return await base.MakePersistentAsync (entity);
      }
    }

    /// <summary>
    /// Specific Reload where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IMachineStatus Reload(IMachineStatus entity)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
        return base.Reload(entity);
      }
    }

    /// <summary>
    /// Get the machine status for the specified machine ID
    /// with an early fetch of the reason and of the reason group
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public IMachineStatus FindByIdWithReasonGroup (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<MachineStatus> ()
            .Add (Restrictions.Eq ("Id", machineId))
            .Fetch (SelectMode.Fetch, "Reason")
            .Fetch (SelectMode.Fetch, "Reason.ReasonGroup")
            .UniqueResult<IMachineStatus> ();
        }
      }

    /// <summary>
    /// Get the machine status for the specified machine ID
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public IMachineStatus FindByIdWithMachineMode (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<MachineStatus> ()
            .Add (Restrictions.Eq ("Id", machineId))
            .Fetch (SelectMode.Fetch, "MachineMode")
            .UniqueResult<IMachineStatus> ();
        }
      }

    /// <summary>
    /// Get the machine status for the specified machine ID
    /// with an early fetch of the machine mode, the reason and of the reason group
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public IMachineStatus FindByIdWithMachineModeReasonGroup (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<MachineStatus> ()
            .Add (Restrictions.Eq ("Id", machineId))
            .Fetch (SelectMode.Fetch, "MachineMode")
            .Fetch (SelectMode.Fetch, "Reason")
            .Fetch (SelectMode.Fetch, "Reason.ReasonGroup")
            .UniqueResult<IMachineStatus> ();
        }
      }

    /// <summary>
    /// Get the machine status for the specified machine ID
    /// with an early fetch of the monitored machine, the machine mode, the reason and of the reason group
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public IMachineStatus FindByIdWithMonitoredMachineMachineModeReasonGroup (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<MachineStatus> ()
            .Add (Restrictions.Eq ("Id", machineId))
            .Fetch (SelectMode.Fetch, "MonitoredMachine")
            .Fetch (SelectMode.Fetch, "MachineMode")
            .Fetch (SelectMode.Fetch, "Reason")
            .Fetch (SelectMode.Fetch, "Reason.ReasonGroup")
            .UniqueResult<IMachineStatus> ();
        }
      }

    /// <summary>
    /// Get the machine statuses for a list of machines
    /// with an early fetch of:
    /// <item>the monitored machine</item>
    /// <item>the machine mode</item>
    /// <item>the reason</item>
    /// <item>the reason group</item>
    /// </summary>
    /// <param name="machineIds"></param>
    /// <returns></returns>
    public IList<IMachineStatus> FindAllForWithMonitoredMachineMachineModeReasonGroup(int[] machineIds)
    {
      using (var disableSeqScan = new DisableSeqScan ("MachineStatus")) {
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<MachineStatus> ()
            .Add (Restrictions.In ("MonitoredMachine.Id", machineIds))
            .Fetch (SelectMode.Fetch, "MonitoredMachine")
            .Fetch (SelectMode.Fetch, "MachineMode")
            .Fetch (SelectMode.Fetch, "Reason")
            .Fetch (SelectMode.Fetch, "Reason.ReasonGroup")
            .List<IMachineStatus> ();
      }
    }
  }
}
