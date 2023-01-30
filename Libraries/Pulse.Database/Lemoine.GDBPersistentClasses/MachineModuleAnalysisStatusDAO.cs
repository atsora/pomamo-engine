// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineModuleAnalysisStatusDAO">IMachineModuleAnalysisStatusDAO</see>
  /// </summary>
  public class MachineModuleAnalysisStatusDAO
    : VersionableNHibernateDAO<MachineModuleAnalysisStatus, IMachineModuleAnalysisStatus, int>
    , IMachineModuleAnalysisStatusDAO
  {
    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IMachineModuleAnalysisStatus FindById (int machineId)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MachineModuleAnalysisStatus")) {
          return base.FindById (machineId);
        }
      }

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<IMachineModuleAnalysisStatus> FindByIdAsync (int machineId)
    {
      // TODO: await using when available
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MachineModuleAnalysisStatus")) {
        return await base.FindByIdAsync (machineId);
      }
    }

    /// <summary>
    /// Specific FindByIdAndLock where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IMachineModuleAnalysisStatus FindByIdAndLock (int machineId)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MachineModuleAnalysisStatus")) {
          return base.FindByIdAndLock (machineId);
        }
      }
    
    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IMachineModuleAnalysisStatus MakePersistent(IMachineModuleAnalysisStatus entity)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MachineModuleAnalysisStatus")) {
        return base.MakePersistent(entity);
      }
    }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IMachineModuleAnalysisStatus> MakePersistentAsync (IMachineModuleAnalysisStatus entity)
    {
      // Note: await using is available only in C# 8
      using (var disableSeqScan = new DisableSeqScan ("MachineModuleAnalysisStatus")) {
        return await base.MakePersistentAsync (entity);
      }
    }

    /// <summary>
    /// Specific Reload where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IMachineModuleAnalysisStatus Reload(IMachineModuleAnalysisStatus entity)
    {
      using (DisableSeqScan disableSeqScan = new DisableSeqScan ("MachineModuleAnalysisStatus")) {
        return base.Reload(entity);
      }
    }
    
  }
}
