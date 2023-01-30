// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions
{
  /// <summary>
  /// <see cref="IExtensionsLoader"/> implementation that does nothing
  /// </summary>
  public sealed class ExtensionsLoaderDummy: IExtensionsLoader
  {
    readonly ILog log = LogManager.GetLogger (typeof (ExtensionsLoaderDummy).FullName);

    /// <summary>
    /// Associated extensions provider
    /// 
    /// <see cref="IExtensionsLoader"/>
    /// </summary>
    public IExtensionsProvider ExtensionsProvider => null;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ExtensionsLoaderDummy ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IExtensionsLoader"/>
    /// </summary>
    public void LoadExtensions ()
    {
      return;
    }

    /// <summary>
    /// <see cref="IExtensionsLoader"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task LoadExtensionsAsync (CancellationToken? cancellationToken = null)
    {
      await Task.Delay (0);
    }

  }
}
