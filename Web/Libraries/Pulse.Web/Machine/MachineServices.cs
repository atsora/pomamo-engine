// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Services to machines
  /// </summary>
  public class MachineServices : NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request Machine/Group
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (GroupRequestDTO request)
    {
      return new GroupService ().Get (this.GetCacheClient (),
                                      base.RequestContext,
                                      base.Request,
                                      request);
    }

    /// <summary>
    /// Response to GET request Machine/Groups
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (GroupsRequestDTO request)
    {
      return new GroupsService ().Get (this.GetCacheClient (),
                                       base.RequestContext,
                                       base.Request,
                                       request);
    }

    /// <summary>
    /// Response to GET request Machine/GroupZoomIn
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (GroupZoomInRequestDTO request)
    {
      return new GroupZoomInService ().Get (this.GetCacheClient (),
                                       base.RequestContext,
                                       base.Request,
                                       request);
    }

    /// <summary>
    /// Response to GET request Machine/GroupZoomOut
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (GroupZoomOutRequestDTO request)
    {
      return new GroupZoomOutService ().Get (this.GetCacheClient (),
                                       base.RequestContext,
                                       base.Request,
                                       request);
    }

    /// <summary>
    /// Response to GET request Machine/FromGroups
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (MachinesFromGroupsRequestDTO request)
    {
      return new MachinesFromGroupsService ().Get (this.GetCacheClient (),
                                                  base.RequestContext,
                                                  base.Request,
                                                  request);
    }

    /// <summary>
    /// Response to GET request Machine/Name
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (NameRequestDTO request)
    {
      return new NameService ().Get (this.GetCacheClient (),
                                    base.RequestContext,
                                    base.Request,
                                    request);
    }

    /// <summary>
    /// Response to GET request Machine/Pie
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (PieRequestDTO request)
    {
      return new PieService ().Get (this.GetCacheClient (),
                                    base.RequestContext,
                                    base.Request,
                                    request);
    }
  }
}
#endif // NSERVICEKIT
