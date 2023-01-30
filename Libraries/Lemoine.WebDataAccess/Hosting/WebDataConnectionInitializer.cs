// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.ModelDAO.Interfaces;

namespace Lemoine.WebDataAccess.Hosting
{
  /// <summary>
  /// WebDataConnectionInitializer
  /// </summary>
  /// <typeparam name="T">Type of the fallback connection initializer</typeparam>
  public class WebDataConnectionInitializer<T> : IConnectionInitializer, IDatabaseConnectionStatus
    where T : class, IConnectionInitializer, IDatabaseConnectionStatus
  {
    readonly ILog log = LogManager.GetLogger (typeof (WebDataConnectionInitializer<T>).FullName);

    readonly T m_fallbackConnectionInitializer;

    static readonly string WEB_SERVICE_URL_KEY = "WebServiceUrl";
    static readonly string WEB_SERVICE_URL_DEFAULT = "http://lctr:8081/";

    bool m_webConnectionStatusUp = false;
    bool m_fallback = false;

    public bool IsDatabaseConnectionUp => m_webConnectionStatusUp || (m_fallback && (m_fallbackConnectionInitializer?.IsDatabaseConnectionUp ?? false));

    /// <summary>
    /// Constructor
    /// </summary>
    public WebDataConnectionInitializer (T fallbackConnectionInitializer)
    {
      m_fallbackConnectionInitializer = fallbackConnectionInitializer;
    }

    /// <summary>
    /// <see cref="IConnectionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void CreateAndInitializeConnection (CancellationToken? cancellationToken = null)
    {
      // Web Service URL:
      // 1. from .exe.options or .exe.config
      // 2. from registry
      // 3. default value
      string webServiceUrl = Lemoine.Info.ConfigSet.LoadAndGet<string> (WEB_SERVICE_URL_KEY, "");
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = Lemoine.Info.PulseInfo.WebServiceUrl;
      }
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = WEB_SERVICE_URL_DEFAULT;
      }
      Debug.Assert (!string.IsNullOrEmpty (webServiceUrl));
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
        new Lemoine.WebDataAccess.WebModelFactory (webServiceUrl);
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var lctrTest = ModelDAOHelper.DAOFactory.ComputerDAO
            .GetLctr ();
        }
        m_webConnectionStatusUp = true;
      }
      catch (Exception ex) { // Fallback: GDBPersistentClassFactory
        if (m_fallbackConnectionInitializer is null) {
          log.Error ($"CreateAndInitializeConnection: no fallback connection initializer, throw", ex);
          throw;
        }
        else {
          m_fallback = true;
          log.Warn ($"CreateAndInitializeConnection: fallback to {m_fallbackConnectionInitializer}");
          m_fallbackConnectionInitializer.CreateAndInitializeConnection (cancellationToken);
        }
      }
    }

    /// <summary>
    /// <see cref="IConnectionInitializer"/>
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void CreateAndInitializeConnection (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      // Web Service URL:
      // 1. from .exe.options or .exe.config
      // 2. from registry
      // 3. default value
      string webServiceUrl = Lemoine.Info.ConfigSet.LoadAndGet<string> (WEB_SERVICE_URL_KEY, "");
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = Lemoine.Info.PulseInfo.WebServiceUrl;
      }
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = WEB_SERVICE_URL_DEFAULT;
      }
      Debug.Assert (!string.IsNullOrEmpty (webServiceUrl));
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
        new Lemoine.WebDataAccess.WebModelFactory (webServiceUrl);
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var lctrTest = ModelDAOHelper.DAOFactory.ComputerDAO
            .GetLctr ();
        }
        m_webConnectionStatusUp = true;
      }
      catch (Exception ex) { // Fallback: GDBPersistentClassFactory
        if (m_fallbackConnectionInitializer is null) {
          log.Error ($"CreateAndInitializeConnection: no fallback connection initializer, throw", ex);
          throw;
        }
        else {
          m_fallback = true;
          log.Warn ($"CreateAndInitializeConnection: fallback to {m_fallbackConnectionInitializer}");
          m_fallbackConnectionInitializer.CreateAndInitializeConnection (maxNbAttempt, cancellationToken);
        }
      }
    }

    /// <summary>
    /// <see cref="IConnectionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task CreateAndInitializeConnectionAsync (CancellationToken? cancellationToken = null)
    {
      // Web Service URL:
      // 1. from .exe.options or .exe.config
      // 2. from registry
      // 3. default value
      string webServiceUrl = Lemoine.Info.ConfigSet.LoadAndGet<string> (WEB_SERVICE_URL_KEY, "");
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = Lemoine.Info.PulseInfo.WebServiceUrl;
      }
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = WEB_SERVICE_URL_DEFAULT;
      }
      Debug.Assert (!string.IsNullOrEmpty (webServiceUrl));
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
        new Lemoine.WebDataAccess.WebModelFactory (webServiceUrl);
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var lctrTest = ModelDAOHelper.DAOFactory.ComputerDAO
            .GetLctr ();
        }
        m_webConnectionStatusUp = true;
      }
      catch (Exception ex) { // Fallback: GDBPersistentClassFactory
        if (m_fallbackConnectionInitializer is null) {
          log.Error ($"CreateAndInitializeConnectionAsync: no fallback connection initializer, throw", ex);
          throw;
        }
        else {
          m_fallback = true;
          log.Warn ($"CreateAndInitializeConnectionAsync: fallback to {m_fallbackConnectionInitializer}");
          await m_fallbackConnectionInitializer.CreateAndInitializeConnectionAsync (cancellationToken);
        }
      }
    }

    /// <summary>
    /// <see cref="IConnectionInitializer"/>
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task CreateAndInitializeConnectionAsync (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      // Web Service URL:
      // 1. from .exe.options or .exe.config
      // 2. from registry
      // 3. default value
      string webServiceUrl = Lemoine.Info.ConfigSet.LoadAndGet<string> (WEB_SERVICE_URL_KEY, "");
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = Lemoine.Info.PulseInfo.WebServiceUrl;
      }
      if (string.IsNullOrEmpty (webServiceUrl)) {
        webServiceUrl = WEB_SERVICE_URL_DEFAULT;
      }
      Debug.Assert (!string.IsNullOrEmpty (webServiceUrl));
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
        new Lemoine.WebDataAccess.WebModelFactory (webServiceUrl);
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var lctrTest = ModelDAOHelper.DAOFactory.ComputerDAO
            .GetLctr ();
        }
        m_webConnectionStatusUp = true;
      }
      catch (Exception ex) { // Fallback: GDBPersistentClassFactory
        if (m_fallbackConnectionInitializer is null) {
          log.Error ($"CreateAndInitializeConnectionAsync: no fallback connection initializer, throw", ex);
          throw;
        }
        else {
          m_fallback = true;
          log.Warn ($"CreateAndInitializeConnectionAsync: fallback to {m_fallbackConnectionInitializer}");
          await m_fallbackConnectionInitializer.CreateAndInitializeConnectionAsync (maxNbAttempt, cancellationToken);
        }
      }
    }
  }
}
