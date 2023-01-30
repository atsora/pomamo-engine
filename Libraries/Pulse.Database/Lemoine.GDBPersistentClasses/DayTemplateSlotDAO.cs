// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IDayTemplateSlotDAO">IDayTemplateSlotDAO</see>
  /// </summary>
  public class DayTemplateSlotDAO
    : RangeSlotDAO<DayTemplateSlot, IDayTemplateSlot>
    , IDayTemplateSlotDAO
  {
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    internal DayTemplateSlotDAO ()
      : base (false, true, true, true, false)
    {
    }
    #endregion // Constructors
  }
}
