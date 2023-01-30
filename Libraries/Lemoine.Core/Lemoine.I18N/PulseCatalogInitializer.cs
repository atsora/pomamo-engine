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

namespace Lemoine.I18N
{
  /// <summary>
  /// PulseCatalogInitializer
  /// </summary>
  public class PulseCatalogInitializer: IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (PulseCatalogInitializer).FullName);

    readonly ICatalog m_catalog;

    /// <summary>
    /// Constructor
    /// </summary>
    public PulseCatalogInitializer (ICatalog catalog)
    {
      Debug.Assert (null != catalog);

      m_catalog = catalog;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      m_catalog.SetAsPulseCatalog ();
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      InitializeApplication (cancellationToken);
      return Task.CompletedTask;
    }
  }
}
#endif // !NET40
