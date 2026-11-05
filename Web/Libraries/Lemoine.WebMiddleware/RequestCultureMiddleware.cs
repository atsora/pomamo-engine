// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Http;

namespace Lemoine.WebMiddleware
{
  /// <summary>
  /// Middleware to set the UI culture of a request, so that the translated strings
  /// (<see cref="Lemoine.I18N.PulseCatalog"/>) of its response are in the requested language.
  ///
  /// The culture is taken, in this order of priority, from:
  /// <list type="number">
  /// <item>the <c>ui-culture</c> query parameter, for example <c>?ui-culture=fr</c></item>
  /// <item>the <c>culture</c> query parameter, for example <c>?culture=de-DE</c></item>
  /// <item>the <c>Accept-Language</c> header, only if the <c>WebService.Localization.AcceptLanguage</c> config key is true</item>
  /// </list>
  ///
  /// Without any of them, or with an unknown culture, the UI culture of the service is kept
  /// (from the <c>Language</c> config key or from the system).
  ///
  /// Only the UI culture is changed, not the culture that is used to format the dates and the numbers.
  /// </summary>
  public class RequestCultureMiddleware
  {
    /// <summary>
    /// Query parameter for the UI culture
    /// </summary>
    public static readonly string UI_CULTURE_QUERY_KEY = "ui-culture";

    /// <summary>
    /// Query parameter for the culture
    /// </summary>
    public static readonly string CULTURE_QUERY_KEY = "culture";

    static readonly string ACCEPT_LANGUAGE_KEY = "WebService.Localization.AcceptLanguage";
    static readonly bool ACCEPT_LANGUAGE_DEFAULT = false; // Browsers always send this header: keep the language of the service by default

    readonly ILog log = LogManager.GetLogger<RequestCultureMiddleware> ();

    readonly RequestDelegate m_next;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="next"></param>
    public RequestCultureMiddleware (RequestDelegate next)
    {
      m_next = next;
    }

    /// <summary>
    /// Is the Accept-Language header considered ?
    /// </summary>
    public static bool AcceptLanguage => Lemoine.Info.ConfigSet.LoadAndGet (ACCEPT_LANGUAGE_KEY, ACCEPT_LANGUAGE_DEFAULT);

    /// <summary>
    /// Middleware method
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      var uiCulture = GetRequestUICulture (context);
      if (uiCulture is not null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"InvokeAsync: set the UI culture {uiCulture.Name} for {context.Request.Path}");
        }
        // CurrentUICulture is stored in the execution context:
        // it flows to the next middlewares and is not propagated back to the caller
        CultureInfo.CurrentUICulture = uiCulture;
      }
      await m_next.Invoke (context);
    }

    /// <summary>
    /// Is a query key one of the keys of this middleware ?
    /// </summary>
    /// <param name="queryKey"></param>
    /// <returns></returns>
    public static bool IsCultureQueryKey (string queryKey) =>
      string.Equals (queryKey, UI_CULTURE_QUERY_KEY, StringComparison.OrdinalIgnoreCase)
      || string.Equals (queryKey, CULTURE_QUERY_KEY, StringComparison.OrdinalIgnoreCase);

    CultureInfo? GetRequestUICulture (Microsoft.AspNetCore.Http.HttpContext context)
    {
      var fromQuery = GetCulture (context.Request.Query[UI_CULTURE_QUERY_KEY].FirstOrDefault ())
        ?? GetCulture (context.Request.Query[CULTURE_QUERY_KEY].FirstOrDefault ());
      if (fromQuery is not null) {
        return fromQuery;
      }

      if (AcceptLanguage) {
        try {
          return context.Request.GetTypedHeaders ().AcceptLanguage
            .OrderByDescending (l => l.Quality ?? 1.0)
            .Where (l => 0 < (l.Quality ?? 1.0))
            .Select (l => GetCulture (l.Value.Value))
            .FirstOrDefault (c => c is not null);
        }
        catch (Exception ex) {
          log.Error ($"GetRequestUICulture: invalid Accept-Language header", ex);
        }
      }

      return null;
    }

    CultureInfo? GetCulture (string? name)
    {
      if (string.IsNullOrWhiteSpace (name) || name.Trim ().Equals ("*")) {
        return null;
      }
      try {
        return CultureInfo.GetCultureInfo (name.Trim (), predefinedOnly: true);
      }
      catch (CultureNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCulture: unknown culture {name}", ex);
        }
        return null;
      }
    }
  }
}
