// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.WebClient
{
  /// <summary>
  /// Utility class to build a request URL
  /// </summary>
  public class RequestUrl
  {
    #region Members
    string m_url;
    bool m_firstParameter = true;
    IDictionary<string, string> m_customHeaders = new Dictionary<string, string> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (RequestUrl).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated Uri
    /// </summary>
    public Uri Uri => new Uri (this.ToString ());
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="baseUrl"></param>
    public RequestUrl (string baseUrl)
    {
      m_url = baseUrl.TrimStart (new char[] { '/' });
      m_firstParameter = !baseUrl.Contains ("?");
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a base URL
    /// </summary>
    /// <param name="webServiceUrl"></param>
    /// <returns></returns>
    public RequestUrl AddBase (string webServiceUrl)
    {
      m_url = webServiceUrl.TrimEnd (new char[] { '/' }) + "/" + m_url;
      return this;
    }
    
    /// <summary>
    /// Add a parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public RequestUrl Add (string key, string value)
    {
      m_url += m_firstParameter
        ? "?"
        : "&";
      m_firstParameter = false;
      m_url += string.Format ("{0}={1}",
                              key, value);
      return this;
    }
    
    /// <summary>
    /// Add an integer parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public RequestUrl Add (string key, int value)
    {
      return Add (key, value.ToString ());
    }
    
    /// <summary>
    /// Add a boolean parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public RequestUrl Add (string key, bool value)
    {
      return Add (key, value.ToString ().ToLowerInvariant ());
    }

    /// <summary>
    /// Add a custom header key: value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public RequestUrl AddHeader (string key, string value)
    {
      m_customHeaders[key] = value;
      return this;
    }

    /// <summary>
    /// ToString() method
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return m_url;
    }

    /// <summary>
    /// Additional custom headers
    /// </summary>
    public IDictionary<string, string> CustomHeaders => m_customHeaders;
    #endregion // Methods
  }
}
