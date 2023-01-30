// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.Info
{
  /// <summary>
  /// Get the address of the web service to target, for example http://lctr:5000/ or https://lctr:5001/
  /// </summary>
  public class WebServiceAddressService
    : GenericNoCacheService<WebServiceAddressRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WebServiceAddressService).FullName);

    static readonly string DEFAULT_PREFIX_KEY = "WebServiceAddress.DefaultPrefix";
    static readonly string DEFAULT_PREFIX_DEFAULT = "http://";

    static readonly string DEFAULT_PORT_KEY = "WebServiceAddress.DefaultPort";
    static readonly int DEFAULT_PORT_DEFAULT = -1; // -1: use Port config, 0: do not use any port number

    static readonly string WEB_SERVICE_PORT_KEY = "Port";
    static readonly int WEB_SERVICE_PORT_DEFAULT = 5000;

    static readonly string DEFAULT_SUFFIX_KEY = "WebServiceAddress.DefaultSuffix";
    static readonly string DEFAULT_SUFFIX_DEFAULT = "/"; // After the port number

    static readonly string MULTIPLIER_ATTEMPT_KEY = "WebServiceAddress.AttemptMultiplier";
    static readonly int MULTIPLIER_ATTEMPT_DEFAULT = 5;
    
    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public WebServiceAddressService ()
      : base ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(WebServiceAddressRequestDTO request)
    {
      using (var sesssion = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        var webServiceComputers = ModelDAOHelper.DAOFactory.ComputerDAO.GetWeb ();
        // Note: for the moment, take one randomly
        if (webServiceComputers.Any ()) {
          var multiplier = Lemoine.Info.ConfigSet.LoadAndGet<int> (MULTIPLIER_ATTEMPT_KEY,
                                                                   MULTIPLIER_ATTEMPT_DEFAULT);
          for (int i = 0; i < multiplier*webServiceComputers.Count; ++i) {
            // Get an address
            var random = new Random ();
            var webServiceComputer = webServiceComputers[random.Next (webServiceComputers.Count)];
            var address = webServiceComputer.Address;
            if (!webServiceComputer.IsLocal ()) {
              // Test if it responds
              try {
                string testUrl = GetTestUrl (webServiceComputer);
                var testRequest = WebRequest.Create (testUrl);
                string testJson;
                using (WebResponse testResponse = testRequest.GetResponse ()) {
                  Stream stream = testResponse.GetResponseStream ();
                  using (StreamReader streamReader = new StreamReader (stream)) {
                    testJson = streamReader.ReadToEnd ();
                  }
                }
                if (testJson.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
                  log.Warn ($"GetWithoutCache: error message {testJson} as response for address {address}");
                  continue;
                }
              }
              catch (Exception ex) {
                log.Warn ($"GetWithoutCache: web address {address} does not respond", ex);
                continue;
              }
            }
            
            // Return the address
            var response = new WebServiceAddressResponseDTO ();
            response.Address = address;
            response.Url = GetWebServiceUrl (webServiceComputer);
            return response;
          }
          
          log.Error ("GetWithoutCache: no valid web service could be found");
          return new ErrorDTO ("No valid web service", ErrorStatus.TransientProcessError);
        }
        else {
          log.Error ("GetWithoutCache: no web computer is registered");
          return new ErrorDTO ("No web computer is registered", ErrorStatus.MissingConfiguration);
        }
      }
    }

    string GetWebServiceUrl (IComputer computer)
    {
      Debug.Assert (null != computer);

      var webServiceUrl = computer.WebServiceUrl?.Trim ();
      if (!string.IsNullOrEmpty (webServiceUrl)) {
        if (!webServiceUrl.EndsWith ("/")) {
          if (log.IsWarnEnabled) {
            log.Warn ($"GetWebServiceUrl: recorded web service url {webServiceUrl} does not end with /, add it");
          }
          webServiceUrl += "/";
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"GetWebServiceUrl: web service url {webServiceUrl} from computerwebserviceurl column for address {computer.Address}");
        }
        return webServiceUrl;
      }

      var prefix = Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_PREFIX_KEY, DEFAULT_PREFIX_DEFAULT);
      var portNumber = Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_PORT_KEY, DEFAULT_PORT_DEFAULT);
      if (-1 == portNumber) {
        portNumber = Lemoine.Info.ConfigSet.LoadAndGet (WEB_SERVICE_PORT_KEY, WEB_SERVICE_PORT_DEFAULT);
      }
      if (portNumber < 0) {
        log.Fatal ($"GetWebServiceUrl: port number {portNumber} is negative which is unexpected, wrong configuration ?");
        throw new InvalidOperationException ("Invalid port number");
      }
      var portString = (0 == portNumber)
        ? ""
        : $":{portNumber}";
      var suffix = Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_SUFFIX_KEY, DEFAULT_SUFFIX_DEFAULT);
      if (! (suffix?.StartsWith ("/") ?? false)) {
        log.Error ($"GetWebServiceUrl: suffix {suffix} does not start with a /");
        if (string.IsNullOrEmpty (suffix)) {
          log.Error ($"GetWebServiceUrl: empty suffix => replace it by /");
        }
        suffix = "/";
      }
      webServiceUrl = $"{prefix}{computer.Address}{portString}{suffix}";
      if (!webServiceUrl.EndsWith ("/")) {
        if (log.IsWarnEnabled) {
          log.Warn ($"GetWebServiceUrl: default web service url {webServiceUrl} does not end with /, add it");
        }
        webServiceUrl += "/";
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetWebServiceUrl: default web service url {webServiceUrl} from {computer.Address}");
      }
      return webServiceUrl;
    }

    string GetTestUrl (IComputer computer)
    {
      return $"{GetWebServiceUrl (computer)}Test/?format=json";
    }
    #endregion // Methods
  }
}
