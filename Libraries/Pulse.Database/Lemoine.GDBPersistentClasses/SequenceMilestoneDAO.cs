// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ISequenceMilestoneDAODAO">ISequenceMilestoneDAODAO</see>
  /// </summary>
  public class SequenceMilestoneDAO
   : VersionableNHibernateDAO<SequenceMilestone, ISequenceMilestone, int>
    , ISequenceMilestoneDAO
  {
    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <returns></returns>
    public override ISequenceMilestone FindById (int machineModuleId)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("SequenceMilestone")) {
        return base.FindById (machineModuleId);
      }
    }

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<ISequenceMilestone> FindByIdAsync (int machineModuleId)
    {
      // TODO: await using when available
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("SequenceMilestone")) {
        return await base.FindByIdAsync (machineModuleId);
      }
    }

    /// <summary>
    /// Specific FindByIdAndLock where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override ISequenceMilestone FindByIdAndLock (int machineModuleId)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("SequenceMilestone")) {
        return base.FindByIdAndLock (machineModuleId);
      }
    }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ISequenceMilestone MakePersistent (ISequenceMilestone entity)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("SequenceMilestone")) {
        return base.MakePersistent (entity);
      }
    }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<ISequenceMilestone> MakePersistentAsync (ISequenceMilestone entity)
    {
      // Note: await using is available only in C# 8
      using (var disableSeqScan = new DisableSeqScan ("SequenceMilestone")) {
        return await base.MakePersistentAsync (entity);
      }
    }

    /// <summary>
    /// Specific Reload where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ISequenceMilestone Reload (ISequenceMilestone entity)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("SequenceMilestone")) {
        return base.Reload (entity);
      }
    }

  }
}
