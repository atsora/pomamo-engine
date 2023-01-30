// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

    #region Members
    #endregion // Members

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericSyncNoCacheService ()
    {
    }
    #endregion // Constructors

    #region Methods
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
    #endregion // Methods
  }
}
