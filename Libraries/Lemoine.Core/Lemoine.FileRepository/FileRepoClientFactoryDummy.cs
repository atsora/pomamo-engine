// Copyright (c) 2023 Atsora Solutions

using Lemoine.Core.Log;
using Lemoine.FileRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// FileRepoClientFactoryDummy
  /// </summary>
  public class FileRepoClientFactoryDummy: IFileRepoClientFactory
  {
    readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientFactoryDummy).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoClientFactoryDummy ()
    {
    }

    /// <summary>
    /// <see cref="IFileRepoClientFactory"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken)
    {
      return new FileRepoClientDummy ();
    }
  }
}
