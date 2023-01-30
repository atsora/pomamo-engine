// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.GroupReason
{
  internal sealed class Group : IGroup
  {
    readonly string m_prefix;
    readonly IReason m_reason;

    internal Group (string prefix, IReason reason)
    {
      Debug.Assert (!string.IsNullOrEmpty (prefix));
      Debug.Assert (null != reason);

      m_prefix = prefix;
      m_reason = reason;
    }

    public string Id
    {
      get { return m_prefix + m_reason.Id; }
    }

    public string Name
    {
      get { return m_reason.Display; }
    }

    /// <summary>
    /// <see cref="IGroup"/>
    /// </summary>
    public string TreeName
    {
      get { return this.Name; }
    }

    public string CategoryReference
    {
      get { return "Reason"; }
    }

    public bool SingleMachine
    {
      get { return false; }
    }

    public IEnumerable<int> MachineIds
    {
      get
      {
        return GetMachines ().Select (m => m.Id);
      }
    }

    public IEnumerable<IMachine> GetMachines ()
    {
      var request = new Lemoine.Business.Reason.MachinesSpecificReason (m_reason, true);
      var response = Lemoine.Business.ServiceProvider.Get (request);
      return response.MachineItems.Select (i => i.Machine);
    }

    public IEnumerable<IMachine> GetMachines (IEnumerable<IMachine> machines)
    {
      var request = new Lemoine.Business.Reason.MachinesSpecificReason (m_reason, true);
      // TODO: ...
      var response = Lemoine.Business.ServiceProvider.Get (request);
      return response.MachineItems.Select (i => i.Machine);
    }

    public bool Dynamic
    {
      get { return true; }
    }

    public double? SortPriority
    {
      get { return null; }
    }

    public int SortKind
    {
      get { return 2; }
    }

    public bool ZoomInMachineSelection
    {
      get { return false; }
    }

    public bool? IncludeSpecificMonitoringType (MachineMonitoringTypeId machineMonitoringTypeId)
    {
      return null;
    }

    /// <summary>
    /// Not implemented (return null)
    /// 
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<IPartProductionDataCurrentShift> PartProductionCurrentShift => null;

    /// <summary>
    /// Not implemented (return null)
    /// 
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<UtcDateTimeRange, UtcDateTimeRange, IPartProductionDataRange> PartProductionRange => null;

    /// <summary>
    /// Not implemented (return null)
    /// 
    /// <see cref="IGroup"/>
    /// </summary>
    public Func<IMachine, bool> IsMachineAggregatingParts => null;
  }
}
