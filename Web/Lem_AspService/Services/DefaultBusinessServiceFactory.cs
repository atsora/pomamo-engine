// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Business;
using Lemoine.Core.Cache;

namespace Lem_AspService.Services
{
  public class DefaultBusinessServiceFactory : Lemoine.Business.IBusinessServiceFactory
  {
    readonly ICacheClient m_cacheClient;

    public DefaultBusinessServiceFactory (ICacheClient cacheClient)
    { 
      m_cacheClient = cacheClient;
    }

    public IService CreateBusiness ()
    {
      return new Lemoine.Business.CachedService (m_cacheClient);
    }
  }
}
