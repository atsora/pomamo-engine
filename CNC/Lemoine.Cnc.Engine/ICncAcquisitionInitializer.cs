// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD || NET48 || NETCOREAPP

using System;
using System.Collections.Generic;
using System.Threading;
using Lemoine.Model;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// 
  /// </summary>
  public interface ICncAcquisitionInitializer
  {
    /// <summary>
    /// Copy the distant cnc resources
    /// </summary>
    void CopyDistantResources (CancellationToken cancellationToken);

    /// <summary>
    /// Get the cnc acquisitions that are registered for the current computer and/or service
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IEnumerable<ICncAcquisition> GetRegisteredCncAcquisitions (CancellationToken cancellationToken);
  }
}

#endif // NETSTANDARD || NET48 || NETCOREAPP
