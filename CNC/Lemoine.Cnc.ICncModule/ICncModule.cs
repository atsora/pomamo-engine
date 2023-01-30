// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Threading;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Cnc module interface
  /// </summary>
  public interface ICncModule: IChecked, IDisposable
  {
    /// <summary>
    /// Cnc Acquisition ID
    /// </summary>
    int CncAcquisitionId {
      get; set;
    }
    
    /// <summary>
    /// Cnc Acquisition Name
    /// </summary>
    string CncAcquisitionName {
      get; set;
    }
    
    /// <summary>
    /// Set the data handler to warn the data handler the thread is still active
    /// </summary>
    /// <param name="dataHandler"></param>
    void SetDataHandler (IChecked dataHandler);
  }
}
