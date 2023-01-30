// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// UniqueAcquisitionFinder
  /// </summary>
  public class AcquisitionFinderUnique: IAcquisitionFinder
  {
    readonly ILog log = LogManager.GetLogger (typeof (AcquisitionFinderUnique).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AcquisitionFinderUnique ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IAcquisitionFinder"/>
    /// </summary>
    /// <param name="acquisition"></param>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public bool IsMatch (Acquisition acquisition, string identifier)
    {
      return true;
    }
  }
}
