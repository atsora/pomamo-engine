// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.IO;
using System.Net;
using Lemoine.WebClient;
using Lemoine.Core.Log;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of Query.
  /// </summary>
  public class Query: Lemoine.WebClient.Query
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Query).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor with a default web service URL
    /// </summary>
    public Query ()
    {
    }
    
    /// <summary>
    /// Constructor with a specified specified web service URL
    /// </summary>
    /// <param name="webServiceUrl"></param>
    public Query (string webServiceUrl)
      : base (webServiceUrl)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Save a new object
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public int Save (RequestUrl requestUrl)
    {
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
      WebRequest request = WebRequest
        .Create (url);
      string json;
      using (WebResponse response = request.GetResponse ())
      {
        Stream stream = response.GetResponseStream ();
        using (StreamReader streamReader = new StreamReader (stream)) {
          json = streamReader.ReadToEnd ();
        }
      }
      
      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        log.ErrorFormat ("Save: " +
                         "Save request failed with {0}",
                         json);
        throw new Exception ("Save error");
      }
      
      OkResponse result = JsonConvert.DeserializeObject<OkResponse> (json);
      
      Debug.Assert (0 != result.Id);
      return result.Id;
    }

    /// <summary>
    /// Save a new object
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public async Task<int> SaveAsync (RequestUrl requestUrl)
    {
      string url = requestUrl
        .AddBase (this.WebServiceUrl)
        .Add ("format", "json")
        .ToString ();
      WebRequest request = WebRequest
        .Create (url);
      string json;
      using (WebResponse response = await request.GetResponseAsync ()) {
        Stream stream = response.GetResponseStream ();
        using (StreamReader streamReader = new StreamReader (stream)) {
          json = await streamReader.ReadToEndAsync ();
        }
      }

      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        log.ErrorFormat ("Save: " +
                         "Save request failed with {0}",
                         json);
        throw new Exception ("Save error");
      }

      OkResponse result = JsonConvert.DeserializeObject<OkResponse> (json);

      Debug.Assert (0 != result.Id);
      return result.Id;
    }
    #endregion // Methods
  }
}
