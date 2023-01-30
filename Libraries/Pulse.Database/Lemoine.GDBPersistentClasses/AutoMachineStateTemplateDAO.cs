// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IAutoMachineStateTemplateDAO">IAutoMachineStateTemplateDAO</see>
  /// </summary>
  public class AutoMachineStateTemplateDAO
    : VersionableNHibernateDAO<AutoMachineStateTemplate, IAutoMachineStateTemplate, int>
    , IAutoMachineStateTemplateDAO
  {
    /// <summary>
    /// Find the unique IAutoMachineStateTemplate that matches exactly the specified machine mode
    /// and machine state template
    /// 
    /// Note: The request is not cacheable because there might be (to confirm)
    /// problems with a lazy loading of MachineStateTemplate with the cache
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="currentMachineStateTemplate">not null</param>
    /// <returns></returns>
    public IAutoMachineStateTemplate Find (IMachineMode machineMode,
                                           IMachineStateTemplate currentMachineStateTemplate)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != currentMachineStateTemplate);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities (which may cause some LazyInitialization problems)
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<AutoMachineStateTemplate> ()
        .Add (Restrictions.Eq ("MachineMode.Id", machineMode.Id))
        .Add (Restrictions.Eq ("Current.Id", currentMachineStateTemplate.Id))
        .SetCacheable (true)
        .UniqueResult<IAutoMachineStateTemplate> ();
    }

    /// <summary>
    /// Find the unique IAutoMachineStateTemplate that matches exactly the specified machine mode
    /// with a 'any' machine state template criteria
    /// 
    /// The request is cacheable
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <returns></returns>
    public IAutoMachineStateTemplate Find (IMachineMode machineMode)
    {
      Debug.Assert (null != machineMode);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities (which may cause some LazyInitialization problems)
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<AutoMachineStateTemplate> ()
        .Add (Restrictions.Eq ("MachineMode.Id", machineMode.Id))
        .Add (Restrictions.IsNull ("Current"))
        .SetCacheable (true)
        .UniqueResult<IAutoMachineStateTemplate> ();
    }
  }
}
