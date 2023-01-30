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
  /// Implementation of <see cref="Lemoine.ModelDAO.IShiftTemplateSlotDAO">IShiftTemplateSlotDAO</see>
  /// </summary>
  public class ShiftTemplateSlotDAO
    : RangeSlotDAO<ShiftTemplateSlot, IShiftTemplateSlot>
    , IShiftTemplateSlotDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    internal ShiftTemplateSlotDAO ()
      : base (false, false, true, true, false)
    {
    }
  }
}
