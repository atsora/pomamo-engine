// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace Pomamo.CncModule
{
  /// <summary>
  /// 
  /// </summary>
  public interface ICncModule: IDisposable
  {
    /// <summary>
    /// ID
    /// </summary>
    int CncAcquisitionId
    {
      get; set;
    }

    /// <summary>
    /// Name
    /// </summary>
    string CncAcquisitionName
    {
      get; set;
    }
  }
}
