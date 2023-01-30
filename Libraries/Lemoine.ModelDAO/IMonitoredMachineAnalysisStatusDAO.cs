// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMonitoredMachineAnalysisStatus.
  /// </summary>
  public interface IMonitoredMachineAnalysisStatusDAO: IGenericUpdateDAO<IMonitoredMachineAnalysisStatus, int>
  {
    /// <summary>
    /// Get the minimum activity analysis date/time
    /// </summary>
    /// <returns></returns>
    DateTime? GetMinActivityAnalysisDateTime ();
    
    /// <summary>
    /// Get the last monitored machine analysis status
    /// </summary>
    /// <returns></returns>
    IMonitoredMachineAnalysisStatus GetLast ();
  }
}
