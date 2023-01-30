// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Cnc.Asp;
using Lemoine.CncEngine;
using Lemoine.Core.Log;

namespace Lemoine.CncEngine.Asp
{
  /// <summary>
  /// Set service
  /// </summary>
  public sealed class DataService
  {
    readonly ILog log = LogManager.GetLogger (typeof (DataService).FullName);

    readonly IAcquisitionSet m_acquisitionSet;
    readonly IAcquisitionFinder m_acquisitionFinder;

    /// <summary>
    /// Constructor
    /// </summary>
    public DataService (IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder)
    {
      m_acquisitionSet = acquisitionSet;
      m_acquisitionFinder = acquisitionFinder;
    }

    /// <summary>
    /// data request
    /// </summary>
    public IDictionary<string, object> GetData (CancellationToken cancellationToken, string acquisitionIdentifier)
    {
      var acquisition = m_acquisitionSet
        .GetAcquisitions (cancellationToken: cancellationToken)
        .FirstOrDefault (a => m_acquisitionFinder.IsMatch (a, acquisitionIdentifier));
      if (acquisition is null) {
        log.Error ($"GetDataAsync: no acquisition with identifier {acquisitionIdentifier}");
        throw new UnknownAcquisitionException ($"Error: unknown acquisition {acquisitionIdentifier}");
      }

      var finalData = acquisition.FinalData;
      if (null == finalData) {
        log.Error ($"GetDataAsync: no final data, initializing ?");
        throw new FinalDataNullException ("No final data, initializing ?");
      }

      return finalData;
    }
  }
}
