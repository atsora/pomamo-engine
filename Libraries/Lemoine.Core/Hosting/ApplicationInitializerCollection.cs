// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Core.Hosting
{
  /// <summary>
  /// ApplicationInitializerCollection
  /// </summary>
  public class ApplicationInitializerCollection: IApplicationInitializerCollection, IEnumerable<IApplicationInitializer>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationInitializerCollection).FullName);

    readonly IList<IApplicationInitializer> m_applicationInitializers = new List<IApplicationInitializer> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationInitializerCollection (params IApplicationInitializer[] applicationInitializers)
    {
      foreach (var applicationInitializer in applicationInitializers) {
        m_applicationInitializers.Add (applicationInitializer);
      }
    }

    /// <summary>
    /// Add an application initializer
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    public IApplicationInitializerCollection Add (IApplicationInitializer application)
    {
      m_applicationInitializers.Add (application);
      return this;
    }

    /// <summary>
    /// <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerator<IApplicationInitializer> GetEnumerator ()
    {
      return m_applicationInitializers.GetEnumerator ();
    }

    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      foreach (var applicationInitializer in m_applicationInitializers) {
        applicationInitializer.InitializeApplication (cancellationToken);
        cancellationToken.ThrowIfCancellationRequested ();
      }
    }

    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      foreach (var applicationInitializer in m_applicationInitializers) {
        await applicationInitializer.InitializeApplicationAsync (cancellationToken);
        cancellationToken.ThrowIfCancellationRequested ();
      }
    }

    /// <summary>
    /// <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator ()
    {
      return m_applicationInitializers.GetEnumerator ();
    }
  }
}
#endif // !NET40
