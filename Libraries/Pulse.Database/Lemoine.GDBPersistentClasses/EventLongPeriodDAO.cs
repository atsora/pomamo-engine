// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IEventLongPeriodDAO">IEventLongPeriodDAO</see>
  /// </summary>
  public class EventLongPeriodDAO
    : SaveOnlyNHibernateDAO<EventLongPeriod, IEventLongPeriod, int>
    , IEventLongPeriodDAO
  {
  }
}
