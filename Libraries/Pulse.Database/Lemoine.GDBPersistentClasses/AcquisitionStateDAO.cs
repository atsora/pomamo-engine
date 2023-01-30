// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IAcquisitionStateDAO">IAcquisitionStateDAO</see>
  /// </summary>
  public class AcquisitionStateDAO
    : VersionableNHibernateDAO<AcquisitionState, IAcquisitionState, int>
    , IAcquisitionStateDAO
  {
    /// <summary>
    /// Get an acquisition state
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="key"></param>
    /// <returns>One element or null</returns>
    public IAcquisitionState GetAcquisitionState (IMachineModule machineModule, AcquisitionStateKey key)
    {
      Debug.Assert (machineModule != null, "Machine module cannot be null");

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IAcquisitionState> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Key", key))
        .SetCacheable (true)
        .UniqueResult<IAcquisitionState> ();
    }

    /// <summary>
    /// Find by machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public IList<IAcquisitionState> FindByMachineModule (IMachineModule machineModule)
    {
      Debug.Assert (machineModule != null, "Machine module cannot be null");

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IAcquisitionState> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .SetCacheable (true)
        .List<IAcquisitionState> ();
    }
  }
}
