// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IBetweenCycles.
  /// </summary>
  public interface IBetweenCyclesDAO: IGenericByMachineUpdateDAO<IBetweenCycles, int>
  {
    /// <summary>
    /// Find the BetweenCycles item with the specified previous operation cycle
    /// </summary>
    /// <param name="previousCycle"></param>
    /// <returns></returns>
    IBetweenCycles FindWithPreviousCycle (IOperationCycle previousCycle);

    /// <summary>
    /// Find the BetweenCycles item with the specified next operation cycle
    /// </summary>
    /// <param name="nextCycle"></param>
    /// <returns></returns>
    IBetweenCycles FindWithNextCycle (IOperationCycle nextCycle);
  }
}
