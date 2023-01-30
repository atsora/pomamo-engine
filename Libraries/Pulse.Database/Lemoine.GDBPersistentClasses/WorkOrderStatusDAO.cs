// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IWorkOrderStatusDAO">IWorkOrderStatusDAO</see>
  /// </summary>
  public class WorkOrderStatusDAO
    : VersionableNHibernateDAO<WorkOrderStatus, IWorkOrderStatus, int>
    , IWorkOrderStatusDAO
    
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderStatusDAO).FullName);
    
    /// <summary>
    /// Get List of WorkOrderStatus ordered by name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns>List of WorkOrderStatus ordered by name</returns>
    public IList<IWorkOrderStatus> FindAllOrderByName()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<WorkOrderStatus>()
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IWorkOrderStatus>();
    }

    /// <summary>
    /// Get work order statuses for a list of Ids
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IDictionary<int, IWorkOrderStatus>> GetWorkOrderStatusesByIdAsync (IEnumerable<int> ids, CancellationToken cancellationToken)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var workOrderStatuses = await FindAllAsync ();
        return workOrderStatuses
          .Where (x => ids.Contains (x.Id))
          .ToDictionary (x => x.Id);
      }
    }
  }
}
