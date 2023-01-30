// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// FileRepoClientFactoryInitializer
  /// </summary>
  public class FileRepoClientFactoryInitializer: IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientFactoryInitializer).FullName);

    readonly IFileRepoClientFactory m_fileRepoClientFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoClientFactoryInitializer (IFileRepoClientFactory fileRepoClientFactory)
    {
      Debug.Assert (null != fileRepoClientFactory);

      m_fileRepoClientFactory = fileRepoClientFactory;
    }

    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      m_fileRepoClientFactory.InitializeFileRepoClient ();
    }

    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      await Task.Delay (0);
      m_fileRepoClientFactory.InitializeFileRepoClient ();
    }
  }
}
#endif // !NET40
