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
  /// Implementation of <see cref="Lemoine.ModelDAO.IUserSlotDAO">IUserSlotDAO</see>
  /// </summary>
  public sealed class UserSlotDAO
    : GenericUserSlotDAO<UserSlot, IUserSlot>
    , IUserSlotDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    internal UserSlotDAO ()
      : base (true)
    {
    }

    /// <summary>
    /// Get all the user slots for the specified user and the specified time range
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public override IList<IUserSlot> GetListInRange (IUser user, UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<UserSlot> ()
        .Add (Restrictions.Eq ("User", user))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("BeginDay"))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<IUserSlot> ();
    }
  }
}
