// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IPackage.
  /// </summary>
  public interface IPackageDAO: IGenericDAO<IPackage, int>
  {
    /// <summary>
    /// Find a plugin with its identifying name
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <returns></returns>
    IPackage FindByIdentifyingName(string identifyingName);
  }
}
