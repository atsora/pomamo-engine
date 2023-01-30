// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;
using Lemoine.Extensions.Business;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Business.Performance
{
  /// <summary>
  /// PerfRecorderFromExtensions: Performance recorder that loads the performance recorders from plugins
  /// </summary>
  public class PerfRecorderFromExtensions: IPerfRecorder, IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (PerfRecorderFromExtensions).FullName);

    readonly IExtensionsLoader m_extensionsLoader;
    IEnumerable<IPerfRecorderExtension> m_extensions = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public PerfRecorderFromExtensions (IExtensionsLoader extensionsLoader)
    {
      m_extensionsLoader = extensionsLoader;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      if (m_extensions is null) {
        m_extensionsLoader.LoadExtensions ();
        m_extensions = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Extension.GlobalExtensions<IPerfRecorderExtension> (ext => ext.Initialize ()));
        foreach (var extension in m_extensions) {
          extension.InitializeApplication ();
        }
      }
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      if (m_extensions is null) {
        await m_extensionsLoader.LoadExtensionsAsync (cancellationToken);
        m_extensions = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Extension.GlobalExtensions<IPerfRecorderExtension> (ext => ext.Initialize ()));
        foreach (var extension in m_extensions) {
          await extension.InitializeApplicationAsync ();
        }
      }
    }

    /// <summary>
    /// Implementation of <see cref="IPerfRecorder"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="duration"></param>
    public void Record (string key, TimeSpan duration)
    {
      InitializeApplication ();
      foreach (var extension in m_extensions) {
        extension.Record (key, duration);
      }
    }
  }
}
