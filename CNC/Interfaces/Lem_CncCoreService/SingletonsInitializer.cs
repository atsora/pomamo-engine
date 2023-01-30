// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Hosting.AsyncInitialization;
using Lemoine.ModelDAO;
using Lemoine.ModelDAO.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lem_CncCoreService
{
  public class SingletonsInitializer : IAsyncInitializer
  {
    readonly ILog log = LogManager.GetLogger<SingletonsInitializer> ();

    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly IConnectionInitializer m_connectionInitializer;

    public SingletonsInitializer (IFileRepoClientFactory fileRepoClientFactory, IConnectionInitializer connectionInitializer)
    {
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_connectionInitializer = connectionInitializer;
    }

    public async Task InitializeAsync ()
    {
      log.Debug ("InitializeAsync: Creating ModelFactory /B");
      await m_connectionInitializer.InitializeAsync (0); // Delay the initial connection

      log.Debug ("InitilaizeAsync: set the FileRepoClient");
      m_fileRepoClientFactory.InitializeFileRepoClient (); // Required for the plugins synchronization + FileRepo implementation
    }
  }

}
