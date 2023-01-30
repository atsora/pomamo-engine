// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

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
  /// File repo client factory that returned an exception
  /// 
  /// It may be used when no file repo client is required
  /// </summary>
  public class FileRepoClientFactoryNotImplemented
    : Lemoine.FileRepository.IFileRepoClientFactory
  {
    ILog log = LogManager.GetLogger<FileRepoClientFactoryNoCorba> ();

    /// <summary>
    /// <see cref="IFileRepoClientFactory"/>
    /// </summary>
    public IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken)
    {
      log.Fatal ("CreateFileRepoClient: it should never been called");
      Debug.Assert (false);
      throw new NotImplementedException ();
    }
  }
}
