// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IUserMachineSlotDAO">IUserMachineSlotDAO</see>
  /// </summary>
  public sealed class UserMachineSlotDAO
    : GenericUserSlotDAO<UserMachineSlot, IUserMachineSlot>
    , IUserMachineSlotDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    internal UserMachineSlotDAO ()
      : base (false)
    {
    }
    
    /// <summary>
    /// Find all the user shift slots in a specified UTC date/time range
    /// 
    /// At least utcFrom or utcTo must be not null
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IUserMachineSlot> FindAllInUtcRange (IUser user, UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<UserMachineSlot> ()
        .Add (Restrictions.Eq ("User", user))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<IUserMachineSlot>();
    }
  }
}
