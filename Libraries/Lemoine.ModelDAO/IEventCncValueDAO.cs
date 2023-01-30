// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEventCncValue.
  /// </summary>
  public interface IEventCncValueDAO: IGenericDAO<IEventCncValue, int>
  {
    /// <summary>
    /// Find all the EventCncValue corresponding to a specified config
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    IList<IEventCncValue> FindWithConfig (IEventCncValueConfig config);
  }
}
