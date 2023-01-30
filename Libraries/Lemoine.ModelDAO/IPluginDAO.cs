// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IPlugin.
  /// </summary>
  public interface IPluginDAO: IGenericDAO<IPlugin, int>
  {
    /// <summary>
    /// Find a plugin with its identifying name
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <returns></returns>
    IPlugin FindByName(string identifyingName);
  }
}
