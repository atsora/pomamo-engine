// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IToolPositionDAO">IToolPositionDAO</see>
  /// </summary>
  public class ToolPositionDAO
    : VersionableByMachineModuleNHibernateDAO<ToolPosition, IToolPosition, int>
    , IToolPositionDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public ToolPositionDAO ()
      : base ("MachineModule")
    { }

    /// <summary>
    /// Find by machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public IList<IToolPosition> FindByMachineModule(IMachineModule machineModule)
    {
      Debug.Assert(machineModule != null, "Machine module cannot be null");
      
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<IToolPosition>()
        .Add (Restrictions.Eq("MachineModule", machineModule))
        .List<IToolPosition>();
    }
    
    /// <summary>
    /// Get a specific position (machine module, magazine, pot)
    /// </summary>
    /// <param name="machineModule">cannot be null</param>
    /// <param name="magazine">can be null</param>
    /// <param name="pot">can be null</param>
    /// <returns></returns>
    public IToolPosition FindByPosition(IMachineModule machineModule, int? magazine, int? pot)
    {
      Debug.Assert(machineModule != null, "Machine module cannot be null");
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession()
        .CreateCriteria<IToolPosition>()
        .Add (Restrictions.Eq("MachineModule", machineModule));
      
      if (magazine == null) {
        criteria = criteria.Add(Restrictions.IsNull("Magazine"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq("Magazine", magazine));
      }

      if (pot == null) {
        criteria = criteria.Add(Restrictions.IsNull("Pot"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq("Pot", pot));
      }

      return criteria.UniqueResult<IToolPosition>();
    }
  }
}
