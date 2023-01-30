// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Hosting.AsyncInitialization;
using Lemoine.I18N;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lem_AspService
{
  public class CatalogInitializer : IAsyncInitializer
  {
    readonly ICatalog m_catalog;
    public CatalogInitializer (ICatalog catalog)
    {
      m_catalog = catalog;
    }

    public Task InitializeAsync ()
    {
      Lemoine.I18N.PulseCatalog.Implementation = m_catalog;
      return Task.CompletedTask;
    }
  }
}
