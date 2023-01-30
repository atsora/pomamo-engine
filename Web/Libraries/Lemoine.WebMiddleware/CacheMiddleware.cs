// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Cache;
using Lemoine.WebMiddleware.HttpContext;
using Lemoine.WebMiddleware.Response;
using Lemoine.Extensions.Web.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Lemoine.WebMiddleware
{
  /// <summary>
  /// Cache value
  /// </summary>
  struct CacheValue
  {
    /// <summary>
    /// Output DTO
    /// </summary>
    public object? OutputDto { get; }

    /// <summary>
    /// Associated policies
    /// </summary>
    public IList<string>? Policies { get; }

    /// <summary>
    /// Valid to a specific user is any
    /// </summary>
    public System.Security.Claims.ClaimsPrincipal? SpecificUser { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="outputDto"></param>
    /// <param name="policies"></param>
    /// <param name="specificUser"></param>
    public CacheValue (object? outputDto, IList<string>? policies, System.Security.Claims.ClaimsPrincipal? specificUser = null)
    {
      this.OutputDto = outputDto;
      this.Policies = policies;
      this.SpecificUser = specificUser;
    }
  }

  /// <summary>
  /// CacheLoggerMiddleware
  /// </summary>
  public class CacheMiddleware
  {
    readonly ILog log = LogManager.GetLogger<CacheMiddleware> ();

    readonly RequestDelegate m_next;
    readonly ICacheClient m_cacheClient;
    readonly ResponseWriter m_responseWriter;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CacheMiddleware (RequestDelegate next, ICacheClient cacheClient, ResponseWriter responseWriter)
    {
      m_next = next;
      m_cacheClient = cacheClient;
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context, IAuthorizationService authorizationService)
    {
      if (!context.Request.Method.Equals ("GET")) { // Cache is only valid with GET
        await m_next.Invoke (context);
      } // TODO: Cache with POST
      else { // GET
        try {
          var cacheKey = GetCacheKey (context);
          var cacheAction = context.ExtractCacheAction ();
          var customData = context.GetCustomData ();
          customData.CacheAction = cacheAction;

          if (cacheAction.HasFlag (CacheAction.InvalidCache)) {
            if (m_cacheClient.Remove (cacheKey)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"InvokeAsync: removing key {cacheKey} from cache failed although requested by cacheAction={cacheAction}, did it exist ?");
              }
            }
            else if (log.IsDebugEnabled) {
              log.Debug ($"InvokeAsync: cache {cacheKey} successfully removed, cacheAction={cacheAction}");
            }
          }

          if (cacheAction.HasFlag (CacheAction.ActionOnly)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"InvokeAsync: action only requested (cacheAction={cacheAction}) => return Ok");
            }
            await m_responseWriter.WriteToBodyAsync (context, new OkDTO ($"Cache key {cacheKey} cleared"));
            return;
          }

          if (cacheAction.HasFlag (CacheAction.UseCache)) { // Try to use the cache
            var cacheValue = m_cacheClient.Get<CacheValue?> (cacheKey);
            if (cacheValue?.OutputDto is not null) {
              if (log.IsDebugEnabled) {
                log.Debug ($"InvokeAsync: {cacheKey} in cache");
              }
              var contentType = context.GetResponseContentType ();

              var cachePolicies = cacheValue?.Policies;
              if (cachePolicies is not null) {
                foreach (var policy in cachePolicies) {
                  var authorizationResult =
                      await authorizationService.AuthorizeAsync (context.User, null, policy);

                  if (!authorizationResult.Succeeded) {
                    log.Error ($"InvokeAsync: policy={policy} not authorized");
                    var errorDto = new ErrorDTO ($"Not authorized, policy={policy}", ErrorStatus.AuthorizationError);
                    await m_responseWriter.WriteToBodyAsync (context, errorDto, contentType);
                    return;
                  }
                }
              }

              var specificUser = cacheValue?.SpecificUser;
              if ((specificUser is null) || (specificUser.Equals (context.User))) {
                var cacheOutputDto = cacheValue?.OutputDto ?? throw new NullReferenceException ();
                await m_responseWriter.WriteToBodyAsync (context, cacheOutputDto, contentType);
                customData.CacheHit = true;
                return;
              }
            }
          }

          await m_next.Invoke (context);

          var outputDto = customData.OutputDTO;
          var cacheTimeOut = customData.CacheTimeOut;
          if ((null != outputDto) && (TimeSpan.FromTicks (0) < cacheTimeOut)) { // Add in cache
            if (log.IsDebugEnabled) {
              log.Debug ($"InvokeAsync: add a cache for key {cacheKey}, timeout={cacheTimeOut}");
            }
            var cacheValue = new CacheValue (outputDto, customData.Policies, customData.SpecificUser);
            m_cacheClient.Add (cacheKey, cacheValue, cacheTimeOut);
          }
        }
        catch (Exception ex) {
          // Probably not necessary here. To put in a top exception layer
          if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
            try {
              log.Fatal ("Exception requires to exit", ex);
            }
            catch (Exception) { }

            Lemoine.Core.Environment.LogAndForceExit (ex, log);
          }
          throw;
        }
      }
    }
    #endregion // Constructors

    string GetCacheKey (Microsoft.AspNetCore.Http.HttpContext context)
    {
      var path = context.Request.Path.HasValue
        ? context.Request.Path.Value
        : "";
      var queryItems = context.Request.Query
        .Where (q => !ExcludeQueryFromCache (q.Key))
        .OrderBy (q => q.Key);
      var queryString = string.Join ('&', queryItems);
      return path + queryString;
    }

    bool ExcludeQueryFromCache (string queryKey)
    {
      switch (queryKey.ToLowerInvariant ()) {
      case "_":
      case "cache":
        return true;
      default:
        return false;
      }
    }
  }
}
