// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using System.Threading.Tasks;

namespace Lemoine.Business.Machine
{
  /// <summary>
  /// Request class to get a group extension from a group id
  /// </summary>
  internal sealed class GroupExtensionFromId
    : IRequest<Tuple<IGroupExtension, GroupIdExtensionMatch>>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupFromId).FullName);

    /// <summary>
    /// Group Id
    /// </summary>
    string GroupId { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="groupId">not null and not empty</param>
    public GroupExtensionFromId (string groupId)
    {
      Debug.Assert (!string.IsNullOrEmpty (groupId));

      this.GroupId = groupId.Trim ();
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found</returns>
    public Tuple<IGroupExtension, GroupIdExtensionMatch> Get ()
    {
      Debug.Assert (!string.IsNullOrEmpty (GroupId));

      if (log.IsDebugEnabled) {
        log.Debug ($"Get: group id {this.GroupId}");
      }

      var groupExtensions = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.GlobalExtensions<IGroupExtension> (ext => ext.Initialize ()));
      foreach (var groupExtension in groupExtensions) {
        var match = groupExtension.GetGroupIdExtensionMatch (this.GroupId);
        switch (match) {
        case GroupIdExtensionMatch.Yes:
        case GroupIdExtensionMatch.Empty:
        case GroupIdExtensionMatch.Dynamic:
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: group extension found for id {this.GroupId}");
          }
          return new Tuple<IGroupExtension, GroupIdExtensionMatch> (groupExtension, match);
        case GroupIdExtensionMatch.No:
          break;
        }
      }

      log.Warn ($"Get: no group extension with id {this.GroupId} was found");
      return null;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<Tuple<IGroupExtension, GroupIdExtensionMatch>> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Machine.GroupExtensionFromId." + this.GroupId;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<Tuple<IGroupExtension,GroupIdExtensionMatch>> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (Tuple<IGroupExtension,GroupIdExtensionMatch> data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
