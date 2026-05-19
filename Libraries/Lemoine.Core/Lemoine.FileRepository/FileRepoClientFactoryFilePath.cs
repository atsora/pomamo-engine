// Copyright (c) 2026 Atsora Solutions

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
  /// FileRepoClientFactoryFilePath
  /// </summary>
  public class FileRepoClientFactoryFilePath : IFileRepoClientFactory
  {
    readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientFactoryFilePath).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoClientFactoryFilePath ()
    {
    }

    /// <summary>
    /// <see cref="IFileRepoClientFactory"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken)
    {
      return new FileRepoClientFilePath ();
    }
  }
}
