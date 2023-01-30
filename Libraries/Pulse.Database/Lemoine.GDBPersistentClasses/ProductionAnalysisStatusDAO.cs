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
  /// Implementation of <see cref="Lemoine.ModelDAO.IProductionAnalysisStatusDAO">IProductionAnalysisStatusDAO</see>
  /// </summary>
  public class ProductionAnalysisStatusDAO
    : VersionableNHibernateDAO<ProductionAnalysisStatus, IProductionAnalysisStatus, int>
    , IProductionAnalysisStatusDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ProductionAnalysisStatusDAO).FullName);

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IProductionAnalysisStatus FindById (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("ProductionAnalysisStatus")) {
          return base.FindById (machineId);
        }
      }

    /// <summary>
    /// Specific FindById where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<IProductionAnalysisStatus> FindByIdAsync (int machineId)
    {
      // TODO: await using when available
      using (var disableSeqScan = new DisableSeqScan ("ProductionAnalysisStatus")) {
        return await base.FindByIdAsync (machineId);
      }
    }

    /// <summary>
    /// Specific FindByIdAndLock where the sequence scan is disabled
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public override IProductionAnalysisStatus FindByIdAndLock (int machineId)
    {
      using (var disableSeqScan = new DisableSeqScan ("ProductionAnalysisStatus")) {
          return base.FindByIdAndLock (machineId);
        }
      }
    
    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IProductionAnalysisStatus MakePersistent(IProductionAnalysisStatus entity)
    {
      using (var disableSeqScan = new DisableSeqScan ("ProductionAnalysisStatus")) {
        return base.MakePersistent(entity);
      }
    }

    /// <summary>
    /// Specific MakePersistent where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IProductionAnalysisStatus> MakePersistentAsync (IProductionAnalysisStatus entity)
    {
      using (var disableSeqScan = new DisableSeqScan ("ProductionAnalysisStatus")) {
        return await base.MakePersistentAsync (entity);
      }
    }

    /// <summary>
    /// Specific Reload where the sequence scan is disabled
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IProductionAnalysisStatus Reload(IProductionAnalysisStatus entity)
    {
      using (var disableSeqScan = new DisableSeqScan ("ProductionAnalysisStatus")) {
        return base.Reload(entity);
      }
    }
  }
}
