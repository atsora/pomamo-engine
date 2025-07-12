// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.ExceptionManagement;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Extensions.Web.Services
{
  /// <summary>
  /// Base case for no cache service.
  /// </summary>
  public abstract class GenericSyncNoCacheService<InputDTO> : IHandler, ISyncNoCacheService<InputDTO>
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericSyncNoCacheService<InputDTO>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericSyncNoCacheService ()
    {
    }

    /// <summary>
    /// Get without cache
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public abstract object GetWithoutCache (InputDTO request);

    /// <summary>
    /// Get implementation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<object> Get (InputDTO request)
    {
      var result = GetWithoutCache (request);
      return Task.FromResult (result);
    }
  }
}
