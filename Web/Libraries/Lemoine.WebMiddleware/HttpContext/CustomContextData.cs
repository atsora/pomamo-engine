// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Cache;
using Microsoft.AspNetCore.Http;

namespace Lemoine.WebMiddleware.HttpContext
{
  /// <summary>
  /// Custom data in HttpContext
  /// </summary>
  public class CustomContextData
  {
    static readonly string CACHE_ACTION_KEY = "CacheAction";
    static readonly string CACHE_HIT_KEY = "CacheHit";
    static readonly string CACHE_TIMEOUT_KEY = "CacheTimeOut";
    static readonly string OUTPUT_DTO_KEY = "OutputDto";
    static readonly string POLICIES_KEY = "Policies";
    static readonly string SPECIFIC_USER_KEY = "SpecificUser";
    static readonly string RESPONSE_CONTENT_TYPE_KEY = "ResponseContentType";

    readonly ILog log = LogManager.GetLogger<CustomContextData> ();

    readonly Microsoft.AspNetCore.Http.HttpContext m_context;

    #region Getters / Setters
    /// <summary>
    /// Cache action
    /// </summary>
    public CacheAction CacheAction
    {
      get { return Get<CacheAction> (CACHE_ACTION_KEY, CacheAction.None); }
      set { Set<CacheAction> (CACHE_ACTION_KEY, value); }
    }

    /// <summary>
    /// Cache hit
    /// </summary>
    public bool CacheHit
    {
      get { return Get<bool> (CACHE_HIT_KEY, false); }
      set { Set<bool> (CACHE_HIT_KEY, value); }
    }

    /// <summary>
    /// Cache time out
    /// </summary>
    public TimeSpan CacheTimeOut
    {
      get { return Get<TimeSpan> (CACHE_TIMEOUT_KEY, TimeSpan.FromSeconds (0)); }
      set { Set<TimeSpan> (CACHE_TIMEOUT_KEY, value); }
    }

    /// <summary>
    /// Output DTO
    /// </summary>
    public object? OutputDTO
    {
      get { return Get<object?> (OUTPUT_DTO_KEY, null); }
      set { Set<object?> (OUTPUT_DTO_KEY, value); }
    }

    /// <summary>
    /// Policies
    /// </summary>
    public IList<string>? Policies
    {
      get { return Get<IList<string>?> (POLICIES_KEY, null); }
      set { Set<IList<string>?> (POLICIES_KEY, value); }
    }

    /// <summary>
    /// The data is specific to this user if not null
    /// </summary>
    public System.Security.Claims.ClaimsPrincipal? SpecificUser
    {
      get { return Get<System.Security.Claims.ClaimsPrincipal?> (SPECIFIC_USER_KEY, null); }
      set { Set<System.Security.Claims.ClaimsPrincipal?> (SPECIFIC_USER_KEY, value); }
    }

    /// <summary>
    /// Response type
    /// </summary>
    public string? ResponseContentType
    {
      get { return Get<string?> (RESPONSE_CONTENT_TYPE_KEY, null); }
      set { Set<object?> (RESPONSE_CONTENT_TYPE_KEY, value); }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CustomContextData (Microsoft.AspNetCore.Http.HttpContext context)
    {
      m_context = context;
    }
    #endregion // Constructors

    void Set<T> (string key, T v)
    {
      m_context.Items[key] = v;
    }

    T Get<T> (string key, T defaultValue)
    {
      object? o;
      if (m_context.Items.TryGetValue (key, out o)) {
        if (null == o) {
          return defaultValue;
        }
        else {
          return (T)o;
        }
      }
      else {
        return defaultValue;
      }
    }
  }
}
