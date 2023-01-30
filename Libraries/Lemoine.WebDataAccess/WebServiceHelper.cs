// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Lemoine.WebClient;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of WebServiceHelper.
  /// </summary>
  internal sealed class WebServiceHelper
  {
    #region Members
    Lemoine.WebDataAccess.Query m_query;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WebServiceHelper).FullName);

    #region Getters / Setters
    /// <summary>
    /// URL of the web service
    /// </summary>
    public static string Url {
      get
      {
        if (null != Instance.m_query) {
          return Instance.m_query.WebServiceUrl;
        }
        else {
          return "";
        }
      }
      set
      {
        Instance.m_query = new Lemoine.WebDataAccess.Query (value);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private WebServiceHelper()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Return a unique result
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public static I UniqueResult<I, T> (RequestUrl requestUrl)
      where T: I
    {
      return Instance.m_query.UniqueResult<I, T> (requestUrl);
    }

    /// <summary>
    /// Return a unique result
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public static async Task<I> UniqueResultAsync<I, T> (RequestUrl requestUrl)
      where T : I
    {
      return await Instance.m_query.UniqueResultAsync<I, T> (requestUrl);
    }

    /// <summary>
    /// Return a list
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public static IList<I> List<I, T> (RequestUrl requestUrl)
      where T: I
    {
      return Instance.m_query.List<I, T> (requestUrl);
    }

    /// <summary>
    /// Return a list
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public static async Task<IList<I>> ListAsync<I, T> (RequestUrl requestUrl)
      where T : I
    {
      return await Instance.m_query.ListAsync<I, T> (requestUrl);
    }

    /// <summary>
    /// Save a new object
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public static int Save (RequestUrl requestUrl)
    {
      return Instance.m_query.Save (requestUrl);
    }

    /// <summary>
    /// Save a new object
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public static async Task<int> SaveAsync (RequestUrl requestUrl)
    {
      return await Instance.m_query.SaveAsync (requestUrl);
    }

    /// <summary>
    /// Execute a request
    /// </summary>
    /// <param name="requestUrl"></param>
    public static void Execute (RequestUrl requestUrl)
    {
      Instance.m_query.Execute (requestUrl);
    }

    /// <summary>
    /// Execute a request
    /// </summary>
    /// <param name="requestUrl"></param>
    public static async Task ExecuteAsync (RequestUrl requestUrl)
    {
      await Instance.m_query.ExecuteAsync (requestUrl);
    }
    #endregion // Methods

    #region Instance
    static WebServiceHelper Instance
    {
      get { return Nested.instance; }
    }
    
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static WebServiceHelper instance = new WebServiceHelper ();
    }
    #endregion // Instance
  }
}
