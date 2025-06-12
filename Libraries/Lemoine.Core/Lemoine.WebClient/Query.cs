// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
#if NETSTANDARD
using System.Net.Http;
#endif // NETSTANDARD
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Newtonsoft.Json;

namespace Lemoine.WebClient
{
  /// <summary>
  /// Query to the web service
  /// </summary>
  public class Query
  {
    static readonly string DEFAULT_PROTOCOL_KEY = "Query.Protocol.Default";
    static readonly string DEFAULT_PROTOCOL_DEFAULT = "http";

    static readonly string DEFAULT_PORT_KEY = "Query.Port.Default";
    static readonly string DEFAULT_PORT_DEFAULT = "5000";

    #region Members
#if NETSTANDARD
    readonly HttpClient m_httpClient;
#endif // NETSTANDARD
    readonly string m_webServiceUrl;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Query).FullName);

    /// <summary>
    /// Timeout
    /// 
    /// Warning: it affects directly the HttpClient
    /// 
    /// Default is 100s
    /// </summary>
    public TimeSpan Timeout
    {
      get {
#if NETSTANDARD
        return m_httpClient.Timeout;
#else // !NETSTANDARD
        throw new NotImplementedException ();
#endif // !NETSTANDARD
      }
      set {
#if NETSTANDARD
        m_httpClient.Timeout = value;
#endif // NETSTANDARD
      }
    }

    /// <summary>
    /// URL of the web service
    /// </summary>
    public string WebServiceUrl
    {
      get { return m_webServiceUrl; }
    }

#if NETSTANDARD
    /// <summary>
    /// Recommended constructor with a default web service URL
    /// </summary>
    /// <param name="httpClient">HttpClient to set for example with dependency injection</param>
    public Query (HttpClient httpClient)
      : this (httpClient, Lemoine.Info.PulseInfo.WebServiceUrl)
    {
    }
#endif // NETSTANDARD

    /// <summary>
    /// Alternative constructor with a default web service URL
    /// when no dependency injection is used and there is no common HttpClient
    /// </summary>
    public Query ()
      : this (Lemoine.Info.PulseInfo.WebServiceUrl)
    {
    }

    /// <summary>
    /// Alternative constructor with a specified specified web service URL
    /// when no dependency injection is used and there is no common HttpClient
    /// </summary>
    /// <param name="webServiceUrl"></param>
    public Query (string webServiceUrl)
#if NETSTANDARD
      : this (new HttpClient (), webServiceUrl)
    {
    }

    /// <summary>
    /// Recommended constructor with a specified specified web service URL
    /// </summary>
    /// <param name="httpClient">HttpClient to set for example with dependency injection</param>
    /// <param name="webServiceUrl"></param>
    public Query (HttpClient httpClient, string webServiceUrl)
    {
      m_httpClient = httpClient;
#else // !NETSTANDARD
    {
#endif // !NETSTANDARD
      if (string.IsNullOrEmpty (webServiceUrl)) {
        log.Error ("Query: webServiceUrl is null or empty");
      }
      else if (!webServiceUrl.StartsWith ("http://") && !webServiceUrl.StartsWith ("https://")) {
        log.Error ($"Query: webServiceUrl {webServiceUrl} does not start with http:// or https://");
      }
      m_webServiceUrl = webServiceUrl;
    }

    /// <summary>
    /// Return a string result
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public string StringResult (string request)
    {
      return StringResult (new RequestUrl (request));
    }

    /// <summary>
    /// Return a string result
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public string StringResult (RequestUrl requestUrl)
    {
#if NETSTANDARD
      return Task.Run (() => StringResultAsync (requestUrl)).Result;
    }

    /// <summary>
    /// Return a string result asynchronously
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public async Task<string> StringResultAsync (RequestUrl requestUrl, CancellationToken? cancellationToken = null)
    {
#endif // NETSTANDARD
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
#if !NETSTANDARD
      WebRequest request = WebRequest
        .Create (url);
      using (var response = request.GetResponse ()) {
        var stream = response.GetResponseStream ();
        using (StreamReader streamReader = new StreamReader (stream)) {
          var s = streamReader.ReadToEnd ();
          return s;
        }
      }      
#else // NETSTANDARD
      var request = new HttpRequestMessage () {
        RequestUri = new Uri (url),
        Method = HttpMethod.Get,
      };
      foreach (var header in requestUrl.CustomHeaders) {
        request.Headers.Add (header.Key, header.Value);
      }
      var token = cancellationToken ?? CancellationToken.None;
      using (var response = await m_httpClient.SendAsync (request, token)) {
        token.ThrowIfCancellationRequested ();
        var responseString = await response.Content.ReadAsStringAsync ();
        return responseString;
      }
#endif // NETSTANDARD
    }

    /// <summary>
    /// Return a binary result
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public byte[] BinaryResult (string request)
    {
      return BinaryResult (new RequestUrl (request));
    }

#if NETSTANDARD
    /// <summary>
    /// Return a binary result
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public async Task<byte[]> BinaryResultAsync (string request, CancellationToken? cancellationToken = null)
    {
      return await BinaryResultAsync (new RequestUrl (request), cancellationToken);
    }
#endif // NETSTANDARD

    /// <summary>
    /// Return a binary result
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public byte[] BinaryResult (RequestUrl requestUrl)
    {
#if NETSTANDARD
      return Task.Run (() => BinaryResultAsync (requestUrl)).Result;
    }

    /// <summary>
    /// Return a binary result asynchronously
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public async Task<byte[]> BinaryResultAsync (RequestUrl requestUrl, CancellationToken? cancellationToken = null)
    {
#endif // NETSTANDARD
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
#if !NETSTANDARD
      WebRequest request = WebRequest
        .Create (url);
      try {
        using (var response = request.GetResponse ()) {
          var stream = response.GetResponseStream ();
          using (BinaryReader reader = new BinaryReader (stream)) {
            const int bufferSize = 4096;
            using (var memoryStream = new MemoryStream ()) {
              byte[] buffer = new byte[bufferSize];
              int count;
              while ((count = reader.Read (buffer, 0, buffer.Length)) != 0) {
                memoryStream.Write (buffer, 0, count);
              }
              return memoryStream.ToArray ();
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"BinaryResult: exception for request {url}", ex);
        throw;
      }
#else // NETSTANDARD
      var request = new HttpRequestMessage () {
        RequestUri = new Uri (url),
        Method = HttpMethod.Get,
      };
      foreach (var header in requestUrl.CustomHeaders) {
        request.Headers.Add (header.Key, header.Value);
      }
      var token = cancellationToken ?? CancellationToken.None;
      try {
        using (var response = await m_httpClient.SendAsync (request, token)) {
          token.ThrowIfCancellationRequested ();
          var bytes = await response.Content.ReadAsByteArrayAsync ();
          return bytes;
        }
      }
      catch (Exception ex) {
        log.Error ($"BinaryResultAsync: exception for request {url}", ex);
        throw;
      }
#endif // NETSTANDARD
    }

    /// <summary>
    /// Return a unique result
    /// </summary>
    /// <param name="request"></param>
    /// <param name="postData">Post data. If null, GET is used</param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public T UniqueResult<T> (string request, string postData = null, string contentType = "text/plain", CancellationToken? cancellationToken = null)
    {
      return UniqueResult<T> (new RequestUrl (request), postData, contentType, cancellationToken);
    }

    /// <summary>
    /// Return a unique result
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <param name="postData">Post data. If null, GET is used</param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public T UniqueResult<T> (RequestUrl requestUrl, string postData = null, string contentType = "text/plain", CancellationToken? cancellationToken = null)
    {
      return UniqueResult<T, T> (requestUrl, postData, contentType);
    }

    /// <summary>
    /// Return a unique result
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <param name="postData">Post data. If null, GET is used</param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public I UniqueResult<I, T> (RequestUrl requestUrl, string postData = null, string contentType = "text/plain", CancellationToken? cancellationToken = null)
      where T : I
    {
#if !NETSTANDARD
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
      WebRequest request = WebRequest
        .Create (url);
      if (null != postData) {
        request.AddPostData (postData, contentType);
      }
      string json;
      using (var response = request.GetResponse ()) {
        var stream = response.GetResponseStream ();
        using (StreamReader streamReader = new StreamReader (stream)) {
          json = streamReader.ReadToEnd ();
        }
      }

      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        return default (T);
      }

      try {
        I result = JsonConvert.DeserializeObject<T> (json);
        return result;
      }
      catch (Exception ex) {
        log.Error ($"UniqueResult: deserialization error for url={url} text={json}", ex);
        throw;
      }
#else // NETSTANDARD
      return Task.Run (() => UniqueResultAsync<I, T> (requestUrl, postData, contentType, cancellationToken)).Result;
#endif // NETSTANDARD
    }

#if NETSTANDARD
    /// <summary>
    /// Return a unique result
    /// </summary>
    /// <param name="request"></param>
    /// <param name="postData">Post data. If null, GET is used</param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public async Task<T> UniqueResultAsync<T> (string request, string postData = null, string contentType = "text/plain", CancellationToken? cancellationToken = null)
    {
      return await UniqueResultAsync<T> (new RequestUrl (request), postData, contentType, cancellationToken);
    }

    /// <summary>
    /// Return a unique result
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <param name="postData">Post data. If null, GET is used</param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public async Task<T> UniqueResultAsync<T> (RequestUrl requestUrl, string postData = null, string contentType = "text/plain", CancellationToken? cancellationToken = null)
    {
      return await UniqueResultAsync<T, T> (requestUrl, postData, contentType, cancellationToken);
    }

    /// <summary>
    /// Return a unique result
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <param name="postData">Post data. If null, GET is used</param>
    /// <param name="contentType">content type for the Post data</param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public async Task<I> UniqueResultAsync<I, T> (RequestUrl requestUrl, string postData = null, string contentType = "text/plain", CancellationToken? cancellationToken = null)
      where T : I
    {
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
      var request = new HttpRequestMessage () {
        RequestUri = new Uri (url),
        Method = (postData is null) ? HttpMethod.Get : HttpMethod.Post,
      };
      if (null != postData) {
        request.Content = new StringContent (postData, Encoding.UTF8, contentType);
      }
      foreach (var header in requestUrl.CustomHeaders) {
        request.Headers.Add (header.Key, header.Value);
      }
      var token = cancellationToken ?? CancellationToken.None;
      string json;
      using (var response = await m_httpClient.SendAsync (request, token)) {
        token.ThrowIfCancellationRequested ();
        json = await response.Content.ReadAsStringAsync ();
      }

      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        return default (T);
      }

      try {
        I result = JsonConvert.DeserializeObject<T> (json);
        return result;
      }
      catch (Exception ex) {
        log.Error ($"UniqueResultAsync: deserialization error for {json}", ex);
        throw;
      }
    }
#endif // NETSTANDARD

    /// <summary>
    /// Return a list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public IList<T> List<T> (string request)
    {
      return List<T> (new RequestUrl (request));
    }

    /// <summary>
    /// Return a list
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public IList<T> List<T> (RequestUrl requestUrl)
    {
      return List<T, T> (requestUrl);
    }

    /// <summary>
    /// Return a list
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public IList<I> List<I, T> (RequestUrl requestUrl)
      where T : I
    {
#if !NETSTANDARD
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
      WebRequest request = WebRequest
        .Create (url);
      string json;
      using (WebResponse response = request.GetResponse ()) {
        Stream stream = response.GetResponseStream ();
        using (StreamReader streamReader = new StreamReader (stream)) {
          json = streamReader.ReadToEnd ();
        }
      }

      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        log.ErrorFormat ("List: " +
                         "List request failed with {0}",
                         json);
        throw new Exception ("List error");
      }

      IList<T> result = JsonConvert.DeserializeObject<IList<T>> (json);

      if (typeof (I).Equals (typeof (T))) {
        return (IList<I>)result;
      }
      else {
        IEnumerable<I> converted = result.Cast<I> ();
        return converted.ToList ();
      }
#else // NETSTANDARD
      return Task.Run (() => ListAsync<I, T> (requestUrl)).Result;
#endif // NETSTANDARD
    }

#if NETSTANDARD
    /// <summary>
    /// Return a list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<IList<T>> ListAsync<T> (string request)
    {
      return await ListAsync<T> (new RequestUrl (request));
    }

    /// <summary>
    /// Return a list
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public async Task<IList<T>> ListAsync<T> (RequestUrl requestUrl)
    {
      return await ListAsync<T, T> (requestUrl);
    }

    /// <summary>
    /// Return a list
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public async Task<IList<I>> ListAsync<I, T> (RequestUrl requestUrl, CancellationToken? cancellationToken = null)
      where T : I
    {
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
      var request = new HttpRequestMessage () {
        RequestUri = new Uri (url),
        Method = HttpMethod.Get,
      };
      foreach (var header in requestUrl.CustomHeaders) {
        request.Headers.Add (header.Key, header.Value);
      }
      var token = cancellationToken ?? CancellationToken.None;
      string json;
      using (var response = await m_httpClient.SendAsync (request, token)) {
        token.ThrowIfCancellationRequested ();
        json = await response.Content.ReadAsStringAsync ();
      }

      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        log.ErrorFormat ("List: " +
                         "List request failed with {0}",
                         json);
        throw new Exception ("List error");
      }

      IList<T> result = JsonConvert.DeserializeObject<IList<T>> (json);

      if (typeof (I).Equals (typeof (T))) {
        return (IList<I>)result;
      }
      else {
        IEnumerable<I> converted = result.Cast<I> ();
        return converted.ToList ();
      }
    }
#endif // NETSTANDARD

    /// <summary>
    /// Execute a request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public void Execute (string request)
    {
      Execute (new RequestUrl (request));
    }

    /// <summary>
    /// Execute a request
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public void Execute (RequestUrl requestUrl)
    {
#if !NETSTANDARD
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
      WebRequest request = WebRequest
        .Create (url);
      string json;
      using (var response = request.GetResponse ()) {
        var stream = response.GetResponseStream ();
        using (StreamReader streamReader = new StreamReader (stream)) {
          json = streamReader.ReadToEnd ();
        }
      }

      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>> (json);
        var errorMessage = dictionary["ErrorMessage"];
        log.ErrorFormat ("Execute: " +
                         "error {0} received, json={1}",
                         errorMessage,
                         json);
        throw new Exception (errorMessage);
      }
#else // NETSTANDARD
      Task.Run (() => ExecuteAsync (requestUrl)).Wait ();
#endif // NETSTANDARD
    }

#if NETSTANDARD
    /// <summary>
    /// Execute a request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task ExecuteAsync (string request)
    {
      await ExecuteAsync (new RequestUrl (request));
    }

    /// <summary>
    /// Execute a request
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <param name="cancellationToken">optional</param>
    /// <returns></returns>
    public async Task ExecuteAsync (RequestUrl requestUrl, CancellationToken? cancellationToken = null)
    {
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
      var request = new HttpRequestMessage () {
        RequestUri = new Uri (url),
        Method = HttpMethod.Get,
      };
      foreach (var header in requestUrl.CustomHeaders) {
        request.Headers.Add (header.Key, header.Value);
      }
      var token = cancellationToken ?? CancellationToken.None;
      string json;
      using (var response = await m_httpClient.SendAsync (request, token)) {
        token.ThrowIfCancellationRequested ();
        json = await response.Content.ReadAsStringAsync ();
      }

      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>> (json);
        var errorMessage = dictionary["ErrorMessage"];
        log.Error ($"Execute: error {errorMessage} received, json={json}");
        throw new Exception (errorMessage);
      }
    }
#endif // NETSTANDARD

    static string GetDefaultWebServiceUrl (string address)
    {
      var webServiceUrl = Lemoine.Info.PulseInfo.MainWebServiceUrl;
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = Lemoine.Info.PulseInfo.WebServiceUrl;
      }
      if (!string.IsNullOrEmpty (webServiceUrl)) {
        try {
          var webServiceUri = new Uri (Lemoine.Info.PulseInfo.MainWebServiceUrl);
          return $"{webServiceUri.Scheme}://{address}:{webServiceUri.Port}";
        }
        catch (Exception ex) {
          log.Error ($"GetDefaultWebServiceUrl: exception with url={webServiceUrl}", ex);
        }
      }

      var protocol = Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_PROTOCOL_KEY, DEFAULT_PROTOCOL_DEFAULT);
      var port = Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_PORT_KEY, DEFAULT_PORT_DEFAULT);
      return $"{protocol}://{address}:{port}";
    }

    /// <summary>
    /// Broadcast a request to all the other web services
    /// </summary>
    /// <param name="nameAddressUrls"></param>
    /// <param name="request"></param>
    public static void Broadcast (IEnumerable<Tuple<string, string, string>> nameAddressUrls, string request)
    {
      if (nameAddressUrls.Any ()) {
        var computerNames = Lemoine.Info.ComputerInfo.GetNames ();
        if (1 < nameAddressUrls.Count ()) {
          computerNames = computerNames.ToList ();
        }
        var ipAddresses = Lemoine.Info.ComputerInfo.GetIPAddresses ();
        foreach (var nameAddressUrl in nameAddressUrls) {
          if (!IsComputer (nameAddressUrl.Item1, nameAddressUrl.Item2, computerNames, ipAddresses)) { // broadcast then
            var webServiceUrl = string.IsNullOrEmpty (nameAddressUrl.Item3)
              ? GetDefaultWebServiceUrl (nameAddressUrl.Item2)
              : nameAddressUrl.Item3;
            new Lemoine.WebClient.Query (webServiceUrl).Execute (request);
          }
        }
      }
    }

#if NETSTANDARD
    /// <summary>
    /// Broadcast a request to all the other web services
    /// </summary>
    /// <param name="nameAddressUrls"></param>
    /// <param name="request"></param>
    /// <param name="rethrow">rethrow any exception</param>
    public static async System.Threading.Tasks.Task BroadcastAsync (IEnumerable<Tuple<string, string, string>> nameAddressUrls, string request, bool rethrow = true)
    {
      try {
        var computerNames = Lemoine.Info.ComputerInfo.GetNames ();
        var ipAddresses = Lemoine.Info.ComputerInfo.GetIPAddresses ();
        var tasks = nameAddressUrls
          .Where (x => !IsComputer (x.Item1, x.Item2, computerNames, ipAddresses))
          .Select (x => SendRequestAsync (x.Item2, x.Item3, request));
        await System.Threading.Tasks.Task.WhenAll (tasks);
      }
      catch (Exception ex) {
        log.Error ($"BroadcastAsync: exception rethrow={rethrow}", ex);
        if (rethrow) {
          throw;
        }
      }
    }

    static async System.Threading.Tasks.Task SendRequestAsync (string address, string optionalUrl, string request)
    {
      var webServiceUrl = string.IsNullOrEmpty (optionalUrl)
        ? GetDefaultWebServiceUrl (address)
        : optionalUrl;
      var query = new Lemoine.WebClient.Query (webServiceUrl);
      await query.ExecuteAsync (request);
    }
#endif // NETSTANDARD

    static bool IsComputer (string computerName, string computerAddress, IEnumerable<string> names, IEnumerable<string> ipAddresses)
    {
      foreach (var name in names) {
        if (!string.IsNullOrEmpty (computerName)
            && string.Equals (computerName, name, StringComparison.InvariantCultureIgnoreCase)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsComputer: {computerName} is the current computer");
          }
          return true;
        }
        if (!string.IsNullOrEmpty (computerAddress)
            && string.Equals (computerAddress, name, StringComparison.InvariantCultureIgnoreCase)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsComputer: {computerName} is the current computer");
          }
          return true;
        }
      }
      foreach (var ipAddress in ipAddresses) {
        if (!string.IsNullOrEmpty (computerAddress)
            && string.Equals (ipAddress, computerAddress, StringComparison.InvariantCultureIgnoreCase)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsComputer: ip={ipAddress} is the current computer");
          }
          return true;
        }
      }
      return false;
    }
  }

  /// <summary>
  /// Extensions to <see cref="WebRequest"/>
  /// </summary>
  public static class WebRequestExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WebRequestExtensions).FullName);

    /// <summary>
    /// Add a post data to a <see cref="WebRequest"/> and switch to a POST method
    /// </summary>
    /// <param name="request"></param>
    /// <param name="postData"></param>
    /// <param name="contentType"></param>
    public static void AddPostData (this WebRequest request, string postData, string contentType = "text/plain")
    {
      if (null == postData) {
        if (log.IsDebugEnabled) {
          log.Debug ($"AddPostData: postData is null => do nothing");
        }
        return;
      }

      var data = Encoding.UTF8.GetBytes (postData);

      request.Method = "POST";
      request.ContentType = contentType;
      request.ContentLength = data.Length;

      using (var stream = request.GetRequestStream ()) {
        stream.Write (data, 0, data.Length);
      }
    }

#if NETSTANDARD
    /// <summary>
    /// Add a post data to a <see cref="WebRequest"/> and switch to a POST method
    /// </summary>
    /// <param name="request"></param>
    /// <param name="postData"></param>
    /// <param name="contentType"></param>
    public static async Task AddPostDataAsync (this WebRequest request, string postData, string contentType = "text/plain")
    {
      if (null == postData) {
        if (log.IsDebugEnabled) {
          log.Debug ($"AddPostData: postData is null => do nothing");
        }
        return;
      }

      var data = Encoding.UTF8.GetBytes (postData);

      request.Method = "POST";
      request.ContentType = contentType;
      request.ContentLength = data.Length;

      using (var stream = request.GetRequestStream ()) {
        await stream.WriteAsync (data, 0, data.Length);
      }
    }
#endif // NETSTANDARD
  }
}
