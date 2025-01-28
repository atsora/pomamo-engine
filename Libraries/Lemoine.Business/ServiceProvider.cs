// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lemoine.Business
{
  /// <summary>
  /// Service provider for Lemoine.Business
  /// </summary>
  public sealed class ServiceProvider
  {
    // Default service
    IService m_defaultService;

    // Custom services management
    static volatile bool s_customServicesDefined;
    static readonly IDictionary<int, IService> s_customServicesPerThread = new Dictionary<int, IService> ();

    static readonly ILog log = LogManager.GetLogger(typeof (ServiceProvider).FullName);

    /// <summary>
    /// Service to use as default (not null)
    /// </summary>
    public static IService Service {
      get { return Instance.m_defaultService; }
      set
      {
        Debug.Assert (null != value);
        Instance.m_defaultService = value;
      }
    }

    /// <summary>
    /// Private constructor (singleton class!)
    /// </summary>
    private ServiceProvider()
    {
      m_defaultService = new DefaultService ();
    }

    #region Methods
    /// <summary>
    /// Set a custom service, for example for a unit test
    /// The custom service will be used for the thread setting it, until RemoveCustomService() is called
    /// </summary>
    /// <param name="customService"></param>
    public static void SetCustomService (IService customService)
    {
      // Add a custom service for the current thread
      lock (s_customServicesPerThread) {
        s_customServicesPerThread.Add (System.Threading.Thread.CurrentThread.ManagedThreadId, customService);
        s_customServicesDefined = true;
      }
    }

    /// <summary>
    /// Remove a custom service
    /// </summary>
    public static void RemoveCustomService ()
    {
      lock (s_customServicesPerThread) {
        s_customServicesPerThread.Remove (System.Threading.Thread.CurrentThread.ManagedThreadId);
        s_customServicesDefined = (s_customServicesPerThread.Count > 0);
      }
    }

    /// <summary>
    /// Run the request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static T Get<T> (IRequest<T> request)
    {
      IService service = Instance.m_defaultService;

      // Possibly find the right service before calling "Get"
      if (s_customServicesDefined) {
        lock (s_customServicesPerThread) {
          int currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
          if (s_customServicesPerThread.ContainsKey (currentThreadId)) {
            service = s_customServicesPerThread[currentThreadId];
          }
        }
      }

      return service.Get<T> (request);
    }

    /// <summary>
    /// Run the request asynchronously
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async System.Threading.Tasks.Task<T> GetAsync<T> (IRequest<T> request)
    {
      IService service = Instance.m_defaultService;

      // Possibly find the right service before calling "Get"
      if (s_customServicesDefined) {
        lock (s_customServicesPerThread) {
          int currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
          if (s_customServicesPerThread.ContainsKey (currentThreadId)) {
            service = s_customServicesPerThread[currentThreadId];
          }
        }
      }

      return await service.GetAsync<T> (request);
    }

    /// <summary>
    /// Get the data in cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns>null if no cache data</returns>
    public static CacheValue<T> GetCacheData<T> (IRequest<T> request)
    {
      IService service = Instance.m_defaultService;

      // Possibly find the right service before calling "Get"
      if (s_customServicesDefined) {
        lock (s_customServicesPerThread) {
          int currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
          if (s_customServicesPerThread.ContainsKey (currentThreadId)) {
            service = s_customServicesPerThread[currentThreadId];
          }
        }
      }

      return service.GetCacheData<T> (request);
    }
    #endregion // Methods

    #region Instance
    static ServiceProvider Instance
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

      internal static readonly ServiceProvider instance = new ServiceProvider ();
    }    
    #endregion // Instance
  }
}
