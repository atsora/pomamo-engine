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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineModuleDetectionDAO">IMachineModuleDetectionDAO</see>
  /// </summary>
  public class MachineModuleDetectionDAO
    : SaveOnlyByMachineModuleNHibernateDAO<MachineModuleDetection, IMachineModuleDetection, int>
    , IMachineModuleDetectionDAO
  {
    /// <summary>
    /// Get the list of IMachineModuleDetection that were inserted after the specified id
    /// with an eager fetch of the stamp
    /// 
    /// Sort them by ascending ID
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="lastId"></param>
    /// <param name="maxNumber"></param>
    /// <returns></returns>
    public IList<IMachineModuleDetection> FindAfter (IMachineModule machineModule, int lastId, int maxNumber)
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<MachineModuleDetection> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Gt ("Id", lastId))
        .Fetch (SelectMode.Fetch, "Stamp")
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (maxNumber)
        .List<IMachineModuleDetection> ();
    }

    /// <summary>
    /// Remove the items that are older than the specified age and whose id is smaller than the specified ID
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="beforeId"></param>
    /// <param name="maxAge"></param>
    public void Clean (IMachineModule machineModule, int beforeId, TimeSpan maxAge)
    {
      DateTime maxDateTime = DateTime.UtcNow.Subtract (maxAge);
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("Clean");
      query.SetParameter ("MachineModuleParameter", machineModule);
      query.SetParameter ("BeforeIdParameter", beforeId);
      query.SetParameter ("MaxDateTimeParameter", maxDateTime);
      query.ExecuteUpdate ();
    }
  }
}
