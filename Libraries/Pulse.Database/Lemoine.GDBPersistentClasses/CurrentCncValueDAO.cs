// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICurrentCncValueDAO">ICurrentCncValueDAO</see>
  /// </summary>
  public class CurrentCncValueDAO
    : VersionableNHibernateDAO<CurrentCncValue, ICurrentCncValue, int>
    , ICurrentCncValueDAO
  {
    /// <summary>
    /// Find the ICurrentCncValue for the specified machine module and field
    /// Eager fetch on field
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <returns></returns>
    public ICurrentCncValue Find (IMachineModule machineModule, IField field)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CurrentCncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id))
        .Fetch (SelectMode.Fetch, "Field")
        .UniqueResult<ICurrentCncValue> ();
    }

    /// <summary>
    /// Find all the ICurrentCncValue for the specified machine module
    /// Eager fetch on field
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public IList<ICurrentCncValue> FindByMachineModule (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CurrentCncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Fetch (SelectMode.Fetch, "Field")
        .List<ICurrentCncValue> ();
    }
  }
}
