// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Net
{
  /// <summary>
  /// Extensions to <see cref="System.Net.Http.HttpClient"/>
  /// </summary>
  public static class HttpClientExtensions
  {
    readonly static ILog log = LogManager.GetLogger (typeof (HttpClientExtensions).FullName);

    /// <summary>
    /// Download a file using <see cref="System.Net.Http.HttpClient"/>
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="url">not null or empty</param>
    /// <param name="path">not null or empty</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static void Download (this HttpClient httpClient, string url, string path, CancellationToken cancellationToken = default)
    {
      Debug.Assert (!string.IsNullOrEmpty (url));
      Debug.Assert (!string.IsNullOrEmpty (path));

      if (log.IsDebugEnabled) {
        log.Debug ($"Download: url={url} to={path}");
      }

      var response = httpClient.GetAsync (url).Result;
      response.EnsureSuccessStatusCode ();
#if NET5_0_OR_GREATER
      using (var stream = response.Content.ReadAsStream (cancellationToken)) {
#else // !NET5_0_OR_GREATER
      using (var stream = response.Content.ReadAsStreamAsync ().Result) {
#endif // !NET5_0_OR_GREATER
        using (var fs = File.Create (path)) {
          stream.Seek (0, SeekOrigin.Begin);
          stream.CopyTo (fs);
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Download: url={url} to={path} completed");
      }
    }

    /// <summary>
    /// Download a file using <see cref="System.Net.Http.HttpClient"/>
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="url">not null or empty</param>
    /// <param name="path">not null or empty</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task DownloadAsync (this HttpClient httpClient, string url, string path, CancellationToken cancellationToken = default)
    {
      Debug.Assert (!string.IsNullOrEmpty (url));
      Debug.Assert (!string.IsNullOrEmpty (path));

      if (log.IsDebugEnabled) {
        log.Debug ($"DownloadAsync: url={url} to={path}");
      }

      var response = await httpClient.GetAsync (url, cancellationToken);
      response.EnsureSuccessStatusCode ();
#if NET5_0_OR_GREATER
      using (var stream = await response.Content.ReadAsStreamAsync (cancellationToken)) {
#else // !NET5_0_OR_GREATER
      using (var stream = await response.Content.ReadAsStreamAsync ()) {
#endif // !NET5_0_OR_GREATER
        using (var fs = File.Create (path)) {
          stream.Seek (0, SeekOrigin.Begin);
          stream.CopyTo (fs);
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"DownloadAsync: url={url} to={path} completed");
      }
    }
  }
}
#endif // !NET40
