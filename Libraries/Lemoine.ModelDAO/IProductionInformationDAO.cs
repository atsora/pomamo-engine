// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IProductionInformation.
  /// </summary>
  public interface IProductionInformationDAO
    : IGenericByMachineDAO<IProductionInformation, long>
  {
  }
}
