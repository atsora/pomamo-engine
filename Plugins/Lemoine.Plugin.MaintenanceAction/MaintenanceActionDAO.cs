// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.GDBPersistentClasses;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate.Criterion;
using System.Diagnostics;
using System.Collections.Generic;

namespace Lemoine.Plugin.MaintenanceAction
{
  public class MaintenanceActionDAO
    : VersionableByMachineNHibernateDAO<MaintenanceAction, MaintenanceAction, int>
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public MaintenanceActionDAO ()
      : base ("Machine")
    { }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public MaintenanceAction FindById (int id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .Get<MaintenanceAction> (id);
    }

    /// <summary>
    /// Implementation of IMachineSlotDAO
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual IList<MaintenanceAction> FindAll (IMachine machine)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MaintenanceAction> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .AddOrder (Order.Asc ("CompletionDateTime"))
        .AddOrder (Order.Asc ("CreationDateTime"))
        .List<MaintenanceAction> ();
    }

    /// <summary>
    /// Implementation of IMachineSlotDAO
    /// </summary>
    /// <returns></returns>
    public virtual IList<MaintenanceAction> FindOpen ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MaintenanceAction> ()
        .Add (Restrictions.Not (Restrictions.Eq ("Status", MaintenanceActionStatus.Completed)))
        .AddOrder (Order.Asc ("CreationDateTime"))
        .List<MaintenanceAction> ();
    }

    /// <summary>
    /// Implementation of IMachineSlotDAO
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual IList<MaintenanceAction> FindOpen (IMachine machine)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MaintenanceAction> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.Not (Restrictions.Eq ("Status", MaintenanceActionStatus.Completed)))
        .AddOrder (Order.Asc ("CreationDateTime"))
        .List<MaintenanceAction> ();
    }

  }
}
