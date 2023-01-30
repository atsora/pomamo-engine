// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of WorkOrderMachineAssociationDAO.
  /// </summary>
  public class WorkOrderMachineAssociationDAO
    : SaveOnlyByMachineNHibernateDAO<WorkOrderMachineAssociation, IWorkOrderMachineAssociation, long>
    , IWorkOrderMachineAssociationDAO
  {
    
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderMachineAssociationDAO).FullName);
    
    /// <summary>
    /// Find the matching work order / machine association 
    /// for a given machine, work order and begin date 
    /// if it exists
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="workOrder"></param>
    /// <param name="startDate"></param>
    /// <returns></returns>
    public IWorkOrderMachineAssociation FindMatching(IMachine machine, IWorkOrder workOrder, DateTime startDate)
    {        
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderMachineAssociation> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.Eq ("ModificationMachine", machine))
        .Add (Restrictions.Eq ("ModificationStatusMachine", machine))
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .Add (Restrictions.Eq ("Begin", startDate))
        // TODO: check this won't be a problem
        .UniqueResult<IWorkOrderMachineAssociation> ();
    }
  }
}
