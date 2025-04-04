// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.ExceptionManagement;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web
{
  /// <summary>
  /// Base save service (not cached)
  /// </summary>
  public abstract class GenericSaveService<InputDTO> : IHandler, IRemoteIpSupport
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericSaveService<InputDTO>).FullName);

    /// <summary>
    /// Remote Ip
    /// </summary>
    public string RemoteIp { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericSaveService ()
    {
    }

    /// <summary>
    /// Get
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual object GetSync (InputDTO request)
    {
      log.Fatal ($"GetSync: not implemented");
      throw new NotImplementedException ("GetSync");
    }

    /// <summary>
    /// Get implementation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual Task<object> Get (InputDTO request)
    {
      var result = GetSync (request);
      return Task.FromResult (result);
    }

    /// <summary>
    /// Fetch IP of request
    /// </summary>
    /// <returns></returns>
    public string GetRequestRemoteIp ()
    {
      return this.RemoteIp;
    }

    /// <summary>
    /// <see cref="IRemoteIpSupport"/>
    /// </summary>
    /// <param name="remoteIp"></param>
    /// <returns></returns>
    public void SetRemoteIp (string remoteIp)
    {
      this.RemoteIp = remoteIp;
    }
  }
}
