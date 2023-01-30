// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.FileRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.FileRepository.Corba
{
  /// <summary>
  /// File repo client factory with no Corba support
  /// </summary>
  public class FileRepoClientFactoryWithCorba
    : FileRepoClientFactoryNoCorba
    , IFileRepoClientFactory
  {
    ILog log = LogManager.GetLogger<FileRepoClientFactoryWithCorba> ();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="defaultMethod"></param>
    public FileRepoClientFactoryWithCorba (DefaultFileRepoClientMethod defaultMethod = DefaultFileRepoClientMethod.Multi)
      : base (defaultMethod)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="defaultMethod"></param>
    public FileRepoClientFactoryWithCorba (Func<DefaultFileRepoClientMethod> defaultMethod)
      : base (defaultMethod)
    {
    }

    /// <summary>
    /// <see cref="IFileRepoClientFactory"/>
    /// </summary>
    public override IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken)
    {
      if (Lemoine.Info.PulseInfo.UseFileRepositoryCorba) {
        return new Lemoine.FileRepository.Corba.FileRepoClientCorba ();
      }
      else {
        return base.CreateFileRepoClient (cancellationToken);
      }
    }

    /// <summary>
    /// <see cref="FileRepoClientFactoryNoCorba"/>
    /// </summary>
    protected override FileRepoClientMulti CreateMulti (CancellationToken cancellationToken)
    {
      var multiClient = base.CreateMulti (cancellationToken);
      cancellationToken.ThrowIfCancellationRequested ();
      multiClient.Add (new Lemoine.FileRepository.Corba.FileRepoClientCorba ());
      return multiClient;
    }
  }
}
