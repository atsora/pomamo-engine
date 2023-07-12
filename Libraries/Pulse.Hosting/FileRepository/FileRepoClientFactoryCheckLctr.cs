// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.FileRepository;
using Lemoine.Threading;

namespace Pulse.Hosting.FileRepository
{
  /// <summary>
  /// <see cref="IFileRepoClientFactory"/> checking if the current server is lctr
  /// </summary>
  public class FileRepoClientFactoryCheckLctr : IFileRepoClientFactory
  {
    readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientFactoryCheckLctr).FullName);

    readonly ILctrChecker m_lctrChecker;
    readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim (1, 1);
    IFileRepoClientFactory m_fileRepoClientFactory = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoClientFactoryCheckLctr (ILctrChecker lctrChecker)
    {
      Debug.Assert (null != lctrChecker);

      m_lctrChecker = lctrChecker;
    }

    /// <summary>
    /// <see cref="IFileRepoClientFactory"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken)
    {
      if (m_fileRepoClientFactory is null) {
        using (var semaphoreHolder = SemaphoreSlimHolder.Create (m_semaphoreSlim, cancellationToken)) {
          if (m_fileRepoClientFactory is null) {
            var fileRepoDefaultMethod = m_lctrChecker.IsLctr ()
              ? DefaultFileRepoClientMethod.PfrDataDir
              : DefaultFileRepoClientMethod.Multi;
            m_fileRepoClientFactory = new FileRepoClientFactoryNoCorba (fileRepoDefaultMethod);
          }
        }
        cancellationToken.ThrowIfCancellationRequested ();
      }

      return m_fileRepoClientFactory.CreateFileRepoClient (cancellationToken);
    }
  }
}
