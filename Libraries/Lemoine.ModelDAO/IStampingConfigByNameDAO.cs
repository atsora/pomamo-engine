// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO for <see cref="IStampingConfigByName"/>
  /// </summary>
  public interface IStampingConfigByNameDAO : IGenericUpdateDAO<IStampingConfigByName, int>
  {
    /// <summary>
    /// Find by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IStampingConfigByName FindByName (string name);

    /// <summary>
    /// Find all the items but for config (with an early fetch of the config)
    /// </summary>
    /// <returns></returns>
    IList<IStampingConfigByName> FindAllForConfig ();
  }
}
