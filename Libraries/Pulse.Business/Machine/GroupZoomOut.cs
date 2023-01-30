// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using System.Collections.Generic;
using Lemoine.Extensions.Business.Group;
using System.Threading.Tasks;

namespace Lemoine.Business.Machine
{
  /// <summary>
  /// Response to a GroupZoomOut request
  /// </summary>
  public sealed class GroupZoomOutResponse
  {
    /// <summary>
    /// Is the returned data dynamic ?
    /// 
    /// null is returned if no zoom out is implemented for this group
    /// </summary>
    public bool? Dynamic { get; internal set; }

    /// <summary>
    /// Group parent.
    /// 
    /// null is returned if no zoom out is implemented for this group
    /// </summary>
    public string Parent { get; internal set; }
  }

  /// <summary>
  /// Request class to get ...
  /// </summary>
  public sealed class GroupZoomOut
    : IRequest<GroupZoomOutResponse>
  {
    #region Members
    readonly string m_childGroupId;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (GroupZoomOut).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="childGroupId">not null</param>
    public GroupZoomOut (string childGroupId)
    {
      Debug.Assert (null != childGroupId);

      m_childGroupId = childGroupId;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public GroupZoomOutResponse Get ()
    {
      var groupZoomExtensions = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.GlobalExtensions<IGroupZoomExtension> (ext => ext.Initialize ()));
      foreach (var groupZoomExtension in groupZoomExtensions) {
        string parent;
        var result = groupZoomExtension.ZoomOut (m_childGroupId, out parent);
        if (result) {
          var response = new GroupZoomOutResponse ();
          response.Parent = parent;
          response.Dynamic = groupZoomExtension.Dynamic;
          return response;
        }
      }

      if (log.IsWarnEnabled) {
        log.WarnFormat ("Get: no zoom out implementation for group {0}", m_childGroupId);
      }
      var r = new GroupZoomOutResponse ();
      r.Parent = null;
      return r;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<GroupZoomOutResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Machine.GroupZoomOut." + m_childGroupId;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (GroupZoomOutResponse data)
    {
      if (!data.Dynamic.HasValue) {
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      else if (data.Dynamic.Value) {
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      else { // !data.Dynamic
        return CacheTimeOut.Config.GetTimeSpan ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<GroupZoomOutResponse> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
