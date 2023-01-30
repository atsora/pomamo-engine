// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IUserShiftSlot.
  /// </summary>
  public interface IUserShiftSlotDAO
    : IGenericByUserUpdateDAO<IUserShiftSlot, int>
    , IGenericUserSlotDAO<IUserShiftSlot>
  {
  }
}
