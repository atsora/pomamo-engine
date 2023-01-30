// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IOperationSlotSplitDAO">IOperationSlotSplitDAO</see>
  /// </summary>
  public class OperationSlotSplitDAO
    : VersionableNHibernateDAO<OperationSlotSplit, IOperationSlotSplit, int>
    , IOperationSlotSplitDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationSlotSplitDAO).FullName);

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IOperationSlotSplit FindById (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("OperationSlotSplit")) {
          return base.FindById (machineId);
        }
      }

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<IOperationSlotSplit> FindByIdAsync (int machineId)
    {
      // TODO: await using when available
      using (var disableSeqScan = new DisableSeqScan ("OperationSlotSplit")) {
        return await base.FindByIdAsync (machineId);
      }
    }

    /// <summary>
    /// Specific FindByIdAndLock where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IOperationSlotSplit FindByIdAndLock (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("OperationSlotSplit")) {
          return base.FindByIdAndLock (machineId);
        }
      }
    
    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IOperationSlotSplit MakePersistent(IOperationSlotSplit entity)
    {
      using (var disableSeqScan = new DisableSeqScan ("OperationSlotSplit")) {
        return base.MakePersistent(entity);
      }
    }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IOperationSlotSplit> MakePersistentAsync (IOperationSlotSplit entity)
    {
      using (var disableSeqScan = new DisableSeqScan ("OperationSlotSplit")) {
        return await base.MakePersistentAsync (entity);
      }
    }

    /// <summary>
    /// Specific Reload where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IOperationSlotSplit Reload(IOperationSlotSplit entity)
    {
      using (var disableSeqScan = new DisableSeqScan ("OperationSlotSplit")) {
        return base.Reload(entity);
      }
    }
  }
}
