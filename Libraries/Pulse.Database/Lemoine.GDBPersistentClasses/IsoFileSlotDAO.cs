// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

using NHibernate;
using NHibernate.Criterion;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IIsoFileSlotDAO">IIsoFileSlotDAO</see>
  /// </summary>
  public sealed class IsoFileSlotDAO :
    GenericMachineModuleSlotDAO<IsoFileSlot, IIsoFileSlot>, IIsoFileSlotDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IsoFileSlotDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    internal IsoFileSlotDAO ()
      : base (true)
    {
    }

    /// <summary>
    /// <see cref="IIsoFileSlotDAO"/>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public IIsoFileSlot FindLast (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IsoFileSlot> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .AddOrder (Order.Desc ("EndDateTime"))
        .SetMaxResults (1)
        .SetCacheable (true)
        .UniqueResult<IIsoFileSlot> ();
    }
  }
}
