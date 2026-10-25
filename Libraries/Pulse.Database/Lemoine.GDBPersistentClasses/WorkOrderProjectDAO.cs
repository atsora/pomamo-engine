// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// WorkOrderProjectDAO
  /// </summary>
  public class WorkOrderProjectDAO : VersionableNHibernateDAO<WorkOrderProject, IWorkOrderProject, int>
    , IWorkOrderProjectDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkOrderProjectDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkOrderProjectDAO ()
    {
    }

    /// <summary>
    /// <see cref="IWorkOrderProjectDAO"/>
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IWorkOrderProject Get (IWorkOrder workOrder, IProject project)
    {
      Debug.Assert (null != workOrder);
      Debug.Assert (null != project);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderProject> ()
        .Add (Restrictions.Eq ("WorkOrder.Id", workOrder.Id))
        .Add (Restrictions.Eq ("Project.Id", project.Id))
        .UniqueResult<WorkOrderProject> ();
    }

    /// <summary>
    /// <see cref="IWorkOrderProjectDAO"/>
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <param name="project">not null</param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IWorkOrderProject> GetAsync (IWorkOrder workOrder, IProject project)
    {
      Debug.Assert (null != workOrder);
      Debug.Assert (null != project);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderProject> ()
        .Add (Restrictions.Eq ("WorkOrder.Id", workOrder.Id))
        .Add (Restrictions.Eq ("Project.Id", project.Id))
        .UniqueResultAsync<WorkOrderProject> ();
    }

    /// <summary>
    /// <see cref="IWorkOrderProjectDAO"/>
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IWorkOrderProject>> FindByWorkOrderAsync (IWorkOrder workOrder)
    {
      Debug.Assert (null != workOrder);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderProject> ()
        .Add (Restrictions.Eq ("WorkOrder.Id", workOrder.Id))
        .AddOrder (Order.Asc ("Id"))
        .ListAsync<IWorkOrderProject> ();
    }
  }
}
