// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
#if NETSTANDARD || NET48 || NETCOREAPP
using System.Text.Json;
using System.Text.Json.Serialization;
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)
using Lemoine.Cnc.Asp;
using Lemoine.CncEngine;
using Lemoine.Core.Log;

namespace Lem_CncService.Wcf
{
  /// <summary>
  /// Implementation of <see cref="IWcfService"/>
  /// </summary>
  [ServiceBehavior (InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
  public sealed class WcfService : IWcfService
  {
    static readonly string API_KEY_CONFIG_KEY = "Cnc.Remote.ApiKey";

    readonly ILog log = LogManager.GetLogger (typeof (WcfService).FullName);

    readonly Lemoine.CncEngine.Asp.GetService m_getService;
    readonly Lemoine.CncEngine.Asp.SetService m_setService;
    readonly Lemoine.CncEngine.Asp.DataService m_dataService;
    readonly Lemoine.CncEngine.Asp.XmlPostService m_xmlPostService;

    /// <summary>
    /// Constructor
    /// </summary>
    public WcfService (IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder)
    {
      m_getService = new Lemoine.CncEngine.Asp.GetService (acquisitionSet, acquisitionFinder);
      m_setService = new Lemoine.CncEngine.Asp.SetService (acquisitionSet, acquisitionFinder);
      m_dataService = new Lemoine.CncEngine.Asp.DataService (acquisitionSet, acquisitionFinder);
      m_xmlPostService = new Lemoine.CncEngine.Asp.XmlPostService (acquisitionSet, acquisitionFinder);
    }

    public Message Get (string acquisitionIdentifier, string moduleref, string method, string property, string param)
    {
      CheckApiKey ();

      var singleResponse = m_getService.Get (System.Threading.CancellationToken.None, acquisitionIdentifier, moduleref, method, property, param);
      return CreateJsonResponse (singleResponse);
    }

    public Message Set (string acquisitionIdentifier, string moduleref, string method, string property, string param, string v, string stringvalue, string longvalue, string intvalue, string doublevalue, string boolvalue)
    {
      CheckApiKey ();

      var singleResponse = m_setService.Set (System.Threading.CancellationToken.None, acquisitionIdentifier, moduleref, method, property, param, v, stringvalue, longvalue, intvalue, doublevalue, boolvalue);
      return CreateJsonResponse (singleResponse);
    }

    public Message GetData (string acquisitionIdentifier)
    {
      CheckApiKey ();

      IDictionary<string, object> result;
      try {
        result = m_dataService.GetData (System.Threading.CancellationToken.None, acquisitionIdentifier);
      }
      catch (Lemoine.CncEngine.Asp.UnknownAcquisitionException ex) {
        log.Error ($"GetData: unknown acquisition {acquisitionIdentifier}", ex);
        return CreateJsonResponse (ex.Message);
      }
      catch (Lemoine.CncEngine.Asp.FinalDataNullException ex) {
        log.Warn ($"GetData: final data null, initializing ?", ex);
        return CreateJsonResponse (ex.Message);
      }
      return CreateJsonResponse (result);
    }

    public Message PostXml (string acquisitionIdentifier)
    {
      CheckApiKey ();

      var xml = OperationContext.Current.RequestContext.RequestMessage.ToString ();

      IDictionary<string, object> result;
      try {
        result = m_xmlPostService.PostXml (System.Threading.CancellationToken.None, acquisitionIdentifier, xml);
      }
      catch (Lemoine.CncEngine.Asp.UnknownAcquisitionException ex) {
        log.Error ($"PostXml: unknown acquisition {acquisitionIdentifier}", ex);
        return CreateJsonResponse (ex.Message);
      }
      catch (Lemoine.CncEngine.Asp.FinalDataNullException ex) {
        log.Warn ($"PostXml: final data null, initializing ?", ex);
        return CreateJsonResponse (ex.Message);
      }
      catch (Exception ex) {
        log.Error ($"PostXml: unexpected exception", ex);
        throw;
      }
      return CreateJsonResponse (result);
    }

    string ConvertToJson (object r)
    {
#if NETSTANDARD || NET48 || NETCOREAPP
      var options = new JsonSerializerOptions {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };
      options.Converters.Add (new JsonStringEnumConverter ());
      return JsonSerializer.Serialize (r, options);
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
      var options = new JsonSerializerSettings {
        NullValueHandling = NullValueHandling.Ignore
      };
      options.Converters.Add (new StringEnumConverter ());
      return JsonConvert.SerializeObject (r, options);
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)
    }

    Message CreateJsonResponse (object r)
    {
      var s = ConvertToJson (r);
      var response = WebOperationContext.Current.CreateTextResponse (s, "application/json; charset=utf-8", Encoding.UTF8);
      return response;
    }

    Message CreateStringResponse (string s)
    {
      var response = WebOperationContext.Current.CreateTextResponse (s, "text/plain; charset=utf-8", Encoding.UTF8);
      return response;
    }

    void CheckApiKey ()
    {
      string apiKey;
      try {
        apiKey = Lemoine.Info.ConfigSet.Get<string> (API_KEY_CONFIG_KEY);
      }
      catch (Exception ex) {
        log.Error ($"CheckApiKey: no api key is defined. Please set one in {API_KEY_CONFIG_KEY} => connection refused", ex);
        throw;
      }
      if (string.IsNullOrEmpty (apiKey)) {
        if (log.IsWarnEnabled) {
          log.Warn ($"CheckApiKey: an empty api key is defined. Please set one in {API_KEY_CONFIG_KEY}");
        }
        return;
      }

      var apiKeyHeader = WebOperationContext.Current.IncomingRequest.Headers["X-API-KEY"];
      if (string.Equals (apiKey, apiKeyHeader)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckApiKey: success");
        }
        return;
      }

      log.Error ("CheckApiKey: missing or invalid api key");
      throw new InvalidOperationException ("Bad authentication");
    }

    public Message CreateApiKey ()
    {
      var apiKey = Lemoine.Info.ConfigSet.LoadAndGet (API_KEY_CONFIG_KEY, "");
      if (!string.IsNullOrEmpty (apiKey)) {
        return CreateStringResponse ("API Key is already set");
      }

      apiKey = System.Guid.NewGuid ().ToString ();
      Lemoine.Info.ConfigSet.SetPersistentConfig (API_KEY_CONFIG_KEY, apiKey);
      return CreateStringResponse (apiKey);
    }
  }
}
