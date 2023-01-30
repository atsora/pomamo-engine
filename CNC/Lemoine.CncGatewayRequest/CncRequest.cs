// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Lemoine.CncGatewayRequest
{
  public class CncRequest
  {
    readonly HttpClient m_httpClient;
    readonly string m_baseUrl;
    readonly string m_apiKey;

    /// <summary>
    /// Recommended constructor
    /// </summary>
    /// <param name="httpClient">HttpClient to set for example with dependency injection</param>
    /// <param name="baseUrl">For example http://cnc:8082/ </param>
    /// <param name="apiKey">API key</param>
    public CncRequest (HttpClient httpClient, string baseUrl, string apiKey = "")
    {
      Debug.Assert (null != httpClient);
      Debug.Assert (!string.IsNullOrEmpty (baseUrl));

      m_httpClient = httpClient;
      m_baseUrl = baseUrl;
      m_apiKey = apiKey;
    }

    /// <summary>
    /// Alternative constructor when no dependency injection is used and there is no common HttpClient
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="apiKey">API key</param>
    public CncRequest (string baseUrl, string apiKey = "")
      : this (new HttpClient (), baseUrl, apiKey)
    {
    }

    async Task<object> GetSetAsync (string path, string parameters)
    {
      var uriBuilder = new UriBuilder (m_baseUrl) {
        Path = path
      };
      if (!string.IsNullOrEmpty (parameters)) {
        uriBuilder.Query = parameters;
      }
      // Note: this build for example url = http://cnc:8082/get?acquisition=...&moduleref=...&method=...&param=...

      var request = new HttpRequestMessage () {
        RequestUri = uriBuilder.Uri,
        Method = HttpMethod.Get,
      };
      if (!string.IsNullOrEmpty (m_apiKey)) {
        request.Headers.Add ("X-API-KEY", m_apiKey);
      }

      string responseString;
      using (var response = await m_httpClient.SendAsync (request)) {
        responseString = await response.Content.ReadAsStringAsync ();
      }

      var singleResponse = JsonConvert.DeserializeObject<SingleResponse> (responseString);
      if (singleResponse.Success) {
        return singleResponse.Result;
      }
      else {
        throw new Exception ($"Request failed with {singleResponse.Error}");
      }
    }

    /// <summary>
    /// Request a single value using a specific method.
    /// 
    /// Recommended for basic returned types only. Else, use <see cref="GetMethodAsync{T}(string, string, string, string)"/>
    /// </summary>
    /// <param name="moduleref">reference of the module to request on the cnc core service for the specified acquisition</param>
    /// <param name="method">name of the method</param>
    /// <param name="param">parameter</param>
    /// <param name="acquisition">Optional: it must include the ID of the acquisition if several acquisitions are active on the cnc (core) service</param>
    /// <returns></returns>
    public async Task<object> GetMethodAsync (string moduleref, string method, string param, string acquisition = null)
    {
      var parameters = HttpUtility.ParseQueryString (string.Empty);
      if (!string.IsNullOrEmpty (acquisition)) {
        parameters["acquisition"] = acquisition;
      }
      parameters["moduleref"] = moduleref;
      parameters["method"] = method;
      parameters["param"] = param;
      return await GetSetAsync ("get", parameters.ToString ());
    }

    /// <summary>
    /// Request a single value specifying its type and using a specific method
    /// </summary>
    /// <typeparam name="T">Expected returned type</typeparam>
    /// <param name="moduleref">reference of the module to request on the cnc core service for the specified acquisition</param>
    /// <param name="method">name of the method</param>
    /// <param name="param">parameter</param>
    /// <param name="acquisition">Optional: it must include the ID of the acquisition if several acquisitions are active on the cnc (core) service</param>
    /// <returns></returns>
    public async Task<T> GetMethodAsync<T> (string moduleref, string method, string param, string acquisition = null)
    {
      var result = await GetMethodAsync (moduleref, method, param, acquisition);
      if (result is Newtonsoft.Json.Linq.JObject jobjectResult) {
        return jobjectResult.ToObject<T> ();
      }
      else {
        throw new Exception ($"Unexpected type was returned");
      }
    }

    /// <summary>
    /// Request a single value using a specific method.
    /// 
    /// Recommended for basic returned types only. Else, use <see cref="GetPropertyAsync{T}(string, string, string)"/>
    /// </summary>
    /// <param name="moduleref">reference of the module to request on the cnc core service for the specified acquisition</param>
    /// <param name="property"></param>
    /// <param name="acquisition">Optional: it must include the ID of the acquisition if several acquisitions are active on the cnc (core) service</param>
    /// <returns></returns>
    public async Task<object> GetPropertyAsync (string moduleref, string property, string acquisition = null)
    {
      var parameters = HttpUtility.ParseQueryString (string.Empty);
      if (!string.IsNullOrEmpty (acquisition)) {
        parameters["acquisition"] = acquisition;
      }
      parameters["moduleref"] = moduleref;
      parameters["property"] = property;
      return await GetSetAsync ("get", parameters.ToString ());
    }

    /// <summary>
    /// Request a single value specifying its type and using a specific property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="moduleref">reference of the module to request on the cnc core service for the specified acquisition</param>
    /// <param name="property"></param>
    /// <param name="acquisition">Optional: it must include the ID of the acquisition if several acquisitions are active on the cnc (core) service</param>
    /// <returns></returns>
    public async Task<T> GetPropertyAsync<T> (string moduleref, string property, string acquisition = null)
    {
      var result = await GetPropertyAsync (moduleref, property, acquisition);
      if (result is Newtonsoft.Json.Linq.JObject jobjectResult) {
        return jobjectResult.ToObject<T> ();
      }
      else {
        throw new Exception ($"Unexpected type was returned");
      }
    }

    /// <summary>
    /// Set a single new value into the CNC using a method
    /// </summary>
    /// <param name="moduleref">reference of the module to request on the cnc core service for the specified acquisition</param>
    /// <param name="method"></param>
    /// <param name="param"></param>
    /// <param name="v"></param>
    /// <param name="acquisition">Optional: it must include the ID of the acquisition if several acquisitions are active on the cnc (core) service</param>
    /// <returns></returns>
    public async Task SetMethodAsync (string moduleref, string method, string param, object v, string acquisition = null)
    {
      var parameters = HttpUtility.ParseQueryString (string.Empty);
      if (!string.IsNullOrEmpty (acquisition)) {
        parameters["acquisition"] = acquisition;
      }
      parameters["moduleref"] = moduleref;
      parameters["method"] = method;
      parameters["param"] = param;
      parameters["v"] = v.ToString ();
      await GetSetAsync ("set", parameters.ToString ());
    }

    /// <summary>
    /// Set a single new value into the CNC using a property
    /// </summary>
    /// <param name="moduleref">reference of the module to request on the cnc core service for the specified acquisition</param>
    /// <param name="property"></param>
    /// <param name="v"></param>
    /// <param name="acquisition">Optional: it must include the ID of the acquisition if several acquisitions are active on the cnc (core) service</param>
    /// <returns></returns>
    public async Task SetPropertyAsync (string moduleref, string property, object v, string acquisition = null)
    {
      var parameters = HttpUtility.ParseQueryString (string.Empty);
      if (!string.IsNullOrEmpty (acquisition)) {
        parameters["acquisition"] = acquisition;
      }
      parameters["moduleref"] = moduleref;
      parameters["property"] = property;
      parameters["v"] = v.ToString ();
      await GetSetAsync ("set", parameters.ToString ());
    }

    /// <summary>
    /// Request several values in the same time using an XML format
    /// 
    /// The POST data must in the following format:
    /// &lt;root&gt;
    ///   &lt;moduleref ref="r"&gt;
    ///     &lt;get method = "GetInt" param="10"&gt;Ten&lt;/get&gt;
    ///     &lt;get method = "GetString" param="B"&gt;LetterB&lt;/get&gt;
    ///   &lt;/moduleref&gt;
    /// &lt;root&gt;
    /// </summary>
    /// <param name="xml"></param>
    /// <param name="acquisition">Optional: it must include the ID of the acquisition if several acquisitions are active on the cnc (core) service</param>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> PostXml (string xml, string acquisition = null)
    {
      var uriBuilder = new UriBuilder (m_baseUrl) {
        Path = "xml"
      };
      if (!string.IsNullOrEmpty (acquisition)) {
        var parameters = HttpUtility.ParseQueryString (string.Empty);
        parameters["acquisition"] = acquisition;
        uriBuilder.Query = parameters.ToString ();
      }
      // Note: this build url = http://cnc:8082/xml?acquisition=...

      var request = new HttpRequestMessage () {
        RequestUri = uriBuilder.Uri,
        Method = HttpMethod.Post,
        Content = new StringContent (xml, Encoding.ASCII, "text/xml"),
      };
      if (!string.IsNullOrEmpty (m_apiKey)) {
        request.Headers.Add ("X-API-KEY", m_apiKey);
      }

      string responseString;
      using (var response = await m_httpClient.SendAsync (request)) {
        responseString = await response.Content.ReadAsStringAsync ();
      }

      return JsonConvert.DeserializeObject<IDictionary<string, object>> (responseString);
    }
  }
}
