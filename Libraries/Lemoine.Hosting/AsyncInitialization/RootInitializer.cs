// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Hosting.AsyncInitialization
{
  internal class RootInitializer
  {
    ILog log = LogManager.GetLogger<RootInitializer> ();

    readonly IEnumerable<IAsyncInitializer> m_initializers;

    public RootInitializer (IEnumerable<IAsyncInitializer> initializers)
    {
      m_initializers = initializers;
    }

    public async Task InitializeAsync ()
    {
      log.Info ("InitializeAsync: Starting async initialization");

      try {
        foreach (var initializer in m_initializers) {
          log.Info ($"InitializeAsync: Starting async initialization for {initializer.GetType ()}");
          try {
            await initializer.InitializeAsync ();
            log.Info ($"InitializeAsync: Async initialization for {initializer.GetType ()} completed");
          }
          catch (Exception ex) {
            log.Error ($"InitializeAsync: Async initialization for {initializer.GetType ()} failed", ex);
            throw;
          }
        }

        log.Info ("Async initialization completed");
      }
      catch (Exception ex) {
        log.Error ("InitializeAsync: Async initialization failed", ex);
        throw;
      }
    }
  }
}
