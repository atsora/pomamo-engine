// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.CncEngine;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.FileRepository;
using Lemoine.Hosting.AsyncInitialization;
using Lemoine.I18N;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lem_CncCoreService
{
  public class LateInitializer : IAsyncInitializer
  {
    readonly ILog log = LogManager.GetLogger<LateInitializer> ();

    readonly ICncAcquisitionInitializer m_cncAcquisitionInitializer;

    /// <summary>
    /// Constructor
    /// </summary>
    public LateInitializer (ICncAcquisitionInitializer cncAcquisitionInitializer)
    {
      m_cncAcquisitionInitializer = cncAcquisitionInitializer;
    }

    public async Task InitializeAsync ()
    {
      if (await Task.Run<bool> (() => !Lemoine.FileRepository.FileRepoClient.Implementation.Test ())) {
        log.Error ("InitializeAsync: active file repository implementation is not valid right now but it may be ok later");
      }

      // TODO: really manage all the time the distant resources ?
      m_cncAcquisitionInitializer.CopyDistantResources (CancellationToken.None); // TODO: cancellation token

      // TODO: free licenses

      // TODO: different initializers...
    }
  }
}
