// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;
using Lemoine.Model;
using Lemoine.Extensions.Business.Group;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("Machine/Name Response DTO")]
  public class NameResponseDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    internal NameResponseDTO (IMachine machine)
    {
      Debug.Assert (null != machine);

      this.Id = machine.Id.ToString ();
      this.Display = machine.Display;
      this.Name = this.Display;
      this.TreeName = this.Display;
      this.Group = false;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="group">not null</param>
    internal NameResponseDTO (IGroup group)
    {
      Debug.Assert (null != group);

      this.Id = group.Id;
      this.Display = group.Name;
      this.Name = group.Name;
      this.TreeName = group.TreeName;
      this.Group = true;
    }

    /// <summary>
    /// Machine or group Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Default display, like in MachineDTO.
    /// 
    /// Same as Name
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Name (to keep the compatibility with the old service GetMachine)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Name to use in a tree
    /// </summary>
    public string TreeName { get; set; }

    /// <summary>
    /// Is it a group ?
    /// </summary>
    public bool Group { get; set; }
  }
}
