// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.FileRepo
{
  /// <summary>
  /// Info services
  /// </summary>
  public class FileRepoServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request FileRepo/Test
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (TestRequestDTO request)
    {
      return new TestService().Get (this.GetCacheClient(),
                                    base.RequestContext,
                                    base.Request,
                                    request);
    }
    
    /// <summary>
    /// Response to GET request FileRepo/ListFilesInDirectory
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ListFilesInDirectoryRequestDTO request)
    {
      return new ListFilesInDirectoryService().Get (this.GetCacheClient(),
                                                    base.RequestContext,
                                                    base.Request,
                                                    request);
    }

    /// <summary>
    /// Response to GET request FileRepo/ListDirectoriesInDirectory
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ListDirectoriesInDirectoryRequestDTO request)
    {
      return new ListDirectoriesInDirectoryService().Get (this.GetCacheClient(),
                                                          base.RequestContext,
                                                          base.Request,
                                                          request);
    }
    
    /// <summary>
    /// Response to GET request FileRepo/GetString
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (GetStringRequestDTO request)
    {
      return new GetStringService().Get (this.GetCacheClient(),
                                         base.RequestContext,
                                         base.Request,
                                         request);
    }
    
    /// <summary>
    /// Response to GET request FileRepo/GetBinary
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (GetBinaryRequestDTO request)
    {
      return new GetBinaryService().Get (this.GetCacheClient(),
                                         base.RequestContext,
                                         base.Request,
                                         request);
    }
    
    /// <summary>
    /// Response to GET request FileRepo/GetLastModifiedDate
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (GetLastModifiedDateRequestDTO request)
    {
      return new GetLastModifiedDateService().Get (this.GetCacheClient(),
                                                   base.RequestContext,
                                                   base.Request,
                                                   request);
    }
  }
}
#endif // NSERVICEKIT
