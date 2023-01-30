// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IShiftTemplate.
  /// </summary>
  public interface IShiftTemplateDAO: IGenericUpdateDAO<IShiftTemplate, int>
  {
    /// <summary>
    /// List all ShiftTemplate with Eager Mode
    /// </summary>
    /// <returns></returns>
    IList<IShiftTemplate> FindAllForConfig();
  }
}
