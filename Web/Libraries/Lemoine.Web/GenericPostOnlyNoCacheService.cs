// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#if NSERVICEKIT
using NServiceKit.ServiceHost;
#endif // NSERVICEKIT

using Lemoine.Core.ExceptionManagement;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web.CommonRequestDTO;

namespace Lemoine.Web
{
  /// <summary>
  /// GenericNoCachePostOnlyService
  /// </summary>
  public abstract class GenericPostOnlyNoCacheService<IPostRequestDTO, IPostDTO>
    : IHandler
    , IBodySupport
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericNoCacheService<IPostRequestDTO>).FullName);

    Stream m_body;

#if NSERVICEKIT
    NServiceKit.CacheAccess.ICacheClient m_nserviceKitCacheClient = null;
#endif // NSERVICEKIT

    #region Getters / Setters
#if NSERVICEKIT
    /// <summary>
    /// Reference to the cache client
    /// 
    /// Not null if called after Get()
    /// </summary>
    public NServiceKit.CacheAccess.ICacheClient NServiceKitCacheClient {
      get { return m_nserviceKitCacheClient; }
    }    
#endif // NSERVICEKIT
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericPostOnlyNoCacheService ()
    {
    }
    #endregion // Constructors

    #region Methods
#if NSERVICEKIT
    /// <summary>
    /// Get (potentially in cache)
    /// 
    /// This method must not be run inside a transaction or a session
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="requestContext"></param>
    /// <param name="httpRequest"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(NServiceKit.CacheAccess.ICacheClient cacheClient,
                      IRequestContext requestContext,
                      IHttpRequest httpRequest,
                      IPostRequestDTO request)
    {
      throw new NotImplementedException ();
    }
#endif // NSERVICEKIT

#if NSERVICEKIT
    /// <summary>
    /// Response to POST request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    public object Post (IPostRequestDTO request,
                        NServiceKit.ServiceHost.IHttpRequest httpRequest)
    {
      var postDto = PostDTO.Deserialize<IPostDTO> (httpRequest);
      if (null == postDto) {
        log.Error ("Post: post DTO is null");
        return new ErrorDTO ("Invalid Post data", ErrorStatus.WrongRequestParameter);
      }

      return Task.Run (async () => await PostAsync (postDto)).Result;
    }
#else // !NSERVICEKIT

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Post (IPostRequestDTO request)
    {
      var postDto = PostDTO.Deserialize<IPostDTO> (m_body);
      if (null == postDto) {
        log.Error ("Post: post DTO is null");
        return new ErrorDTO ("Invalid Post data", ErrorStatus.WrongRequestParameter);
      }

      return await PostAsync (postDto);
    }
#endif // NSERVICEKIT

    /// <summary>
    /// To implement
    /// </summary>
    /// <param name="postDto"></param>
    /// <returns></returns>
    public abstract Task<object> PostAsync (IPostDTO postDto);

    #endregion // Methods

    #region IBodySupport
    /// <summary>
    /// <see cref="IBodySupport"/>
    /// </summary>
    /// <param name="body"></param>
    public void SetBody (Stream body)
    {
      m_body = body;
    }
    #endregion // IBodySupport
  }
}
