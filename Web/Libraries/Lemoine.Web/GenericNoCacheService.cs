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
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#endif // NSERVICEKIT

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web
{
  /// <summary>
  /// Base case for no cache service.
  /// </summary>
  public abstract class GenericNoCacheService<InputDTO> : IHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericNoCacheService<InputDTO>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericNoCacheService ()
    {
    }

    /// <summary>
    /// Get without cache
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual object GetWithoutCache (InputDTO request)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Get sync for NServiceKit
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object GetSync (InputDTO request)
    {
      return GetWithoutCache (request);
    }

    /// <summary>
    /// Get implementation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual Task<object> Get (InputDTO request)
    {
      var result = GetWithoutCache (request);
      return Task.FromResult (result);
    }
  }
}
