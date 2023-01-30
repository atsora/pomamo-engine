// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Hosting.AsyncInitialization;
using Lemoine.I18N;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lem_CncCoreService
{
  public class BusinessInitializer : IAsyncInitializer
  {
    readonly Lemoine.Business.IBusinessServiceFactory m_businessServiceFactory;
    public BusinessInitializer (Lemoine.Business.IBusinessServiceFactory businessServiceFactory)
    {
      m_businessServiceFactory = businessServiceFactory;
    }

    public async Task InitializeAsync ()
    {
      await Task.Run (() => Lemoine.Business.ServiceProvider.Service = m_businessServiceFactory.CreateBusiness ());
    }
  }
}
