// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IWorkOrderMachineStampDAO.
  /// </summary>
  public interface IWorkOrderMachineStampDAO: IGenericByMachineDAO<IWorkOrderMachineStamp, long>
  {
  }
}

