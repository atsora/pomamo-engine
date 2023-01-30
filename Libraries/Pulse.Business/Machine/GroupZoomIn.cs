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
  /// Response to a GroupZoomIn request
  /// </summary>
  public sealed class GroupZoomInResponse
  {
    /// <summary>
    /// Is the returned data dynamic ?
    /// 
    /// null is returned if no zoom in is implemented for this group
    /// </summary>
    public bool? Dynamic { get; internal set; }

    /// <summary>
    /// Group children.
    /// 
    /// An empty list is returned if no zoom in is implemented for this group
    /// </summary>
    public IEnumerable<string> Children { get; internal set; }
  }

  /// <summary>
  /// Request class to get ...
  /// </summary>
  public sealed class GroupZoomIn
    : IRequest<GroupZoomInResponse>
  {
    #region Members
    readonly string m_parentGroupId;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (GroupZoomIn).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="parentGroupId">not null</param>
    public GroupZoomIn (string parentGroupId)
    {
      Debug.Assert (null != parentGroupId);

      m_parentGroupId = parentGroupId;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public GroupZoomInResponse Get ()
    {
      var groupZoomExtensions = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.GlobalExtensions<IGroupZoomExtension> (ext => ext.Initialize ()));
      foreach (var groupZoomExtension in groupZoomExtensions) {
        IEnumerable<string> children;
        var result = groupZoomExtension.ZoomIn (m_parentGroupId, out children);
        if (result) {
          var response = new GroupZoomInResponse ();
          response.Children = children;
          response.Dynamic = groupZoomExtension.Dynamic;
          return response;
        }
      }

      if (log.IsWarnEnabled) {
        log.WarnFormat ("Get: no zoom in implementation for group {0}", m_parentGroupId);
      }
      var r = new GroupZoomInResponse ();
      r.Children = new List<string> ();
      return r;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<GroupZoomInResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Machine.GroupZoomIn." + m_parentGroupId;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (GroupZoomInResponse data)
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
    public bool IsCacheValid (CacheValue<GroupZoomInResponse> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
