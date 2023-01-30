// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineFilterDAO">IMachineFilterDAO</see>
  /// </summary>
  public class MachineFilterDAO
    : VersionableNHibernateDAO<MachineFilter, IMachineFilter, int>
    , IMachineFilterDAO
  {
    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IMachineFilterDAO.FindAllMachineFilterForConfig" />
    /// 
    /// Note: this is not registered to be cacheable because of the eager fetch
    /// </summary>
    /// <returns>IList&lt;IMachineFilter&gt;</returns>
    public IList<IMachineFilter> FindAllMachineFilterForConfig(){
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<MachineFilter> ()
        .Fetch (SelectMode.Fetch, "Items")
        .Fetch (SelectMode.Fetch, "Items.Machine")
        .Fetch (SelectMode.Fetch, "Items.MachineCategory")
        .Fetch (SelectMode.Fetch, "Items.MachineSubCategory")
        .Fetch (SelectMode.Fetch, "Items.Department")
        .Fetch (SelectMode.Fetch, "Items.Company")
        .Fetch (SelectMode.Fetch, "Items.Cell")
        .SetResultTransformer(new DistinctRootEntityResultTransformer()) // Remove duplicate root entity
        .AddOrder(Order.Asc("Id"))
        .List<IMachineFilter> ();
    }
  
    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IMachineFilterDAO.DeleteMachineFilterItem" />
    /// 
    /// </summary>
    public void DeleteMachineFilterItem(IMachineFilterItem machineFilterItemToDelete)
    {
      if (null == machineFilterItemToDelete) {
        return;
      }
      NHibernateHelper.GetCurrentSession().Delete(machineFilterItemToDelete);
    }
  }
}
