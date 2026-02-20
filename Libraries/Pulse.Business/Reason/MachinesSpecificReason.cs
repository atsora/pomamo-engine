// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Request class to get the machines with a specific reason
  /// </summary>
  public sealed class MachinesSpecificReason
    : IRequest<MachinesSpecificReasonResponse>
  {
    readonly IReason m_reason;
    readonly bool m_sort = true;
    readonly IEnumerable<IMachine> m_subSetOfMachines = null;

    static readonly ILog log = LogManager.GetLogger (typeof (MachinesSpecificReason).FullName);

    /// <summary>
    /// Reason in parameter
    /// 
    /// not null
    /// </summary>
    IReason Reason
    {
      get { return m_reason; }
    }

    /// <summary>
    /// Sort the machine by reason start time
    /// </summary>
    bool Sort
    {
      get { return m_sort; }
    }

    /// <summary>
    /// Constructor with the sort option by default
    /// </summary>
    /// <param name="reason">not null</param>
    public MachinesSpecificReason (IReason reason)
      : this (reason, true)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="sort"></param>
    public MachinesSpecificReason (IReason reason, bool sort)
    {
      Debug.Assert (null != reason);

      m_reason = reason;
      m_sort = sort;
    }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="sort"></param>
    /// <param name="subSetOfMachines"></param>
    public MachinesSpecificReason (IReason reason, bool sort, IEnumerable<IMachine> subSetOfMachines)
    {
      Debug.Assert (null != reason);

      m_reason = reason;
      m_sort = sort;
      m_subSetOfMachines = subSetOfMachines;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public MachinesSpecificReasonResponse Get ()
    {
      var response = new MachinesSpecificReasonResponse ();
      var machineItems = new List<MachinesSpecificReasonMachineResponse> ();
      var applicableDateTime = response.DateTime;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IEnumerable<IMonitoredMachine> machines;
        if (null != m_subSetOfMachines) {
          machines = m_subSetOfMachines
            .Select (m => ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (m.Id))
            .Where (m => null != m);
        }
        else {
          machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindAll ();
        }
        foreach (var machine in machines) {
          var currentReasonRequest = new CurrentReason (machine, CurrentReasonPeriod.None, false);
          var currentReasonResponse = Lemoine.Business.ServiceProvider
            .Get (currentReasonRequest);
          if ((null != currentReasonResponse) && (null != currentReasonResponse.Reason)
            && (currentReasonResponse.Reason.Id == m_reason.Id)) {
            if (this.Sort) {
              var sortCurrentReasonRequest = new CurrentReason (machine, CurrentReasonPeriod.Reason, false);
              var sortCurrentReasonResponse = Lemoine.Business.ServiceProvider
                .Get (currentReasonRequest);
              if ((null != sortCurrentReasonResponse) && (null != sortCurrentReasonResponse.Reason)
                && (sortCurrentReasonResponse.Reason.Id == m_reason.Id)) {
                var machineData = new MachinesSpecificReasonMachineResponse (machine);
                machineData.StartDateTime = sortCurrentReasonResponse.PeriodStart;
                machineItems.Add (machineData);
              }
            }
            else { // !this.Sort
              var machineData = new MachinesSpecificReasonMachineResponse (machine);
              machineItems.Add (machineData);
            }
          }
        } // Loop on machines
      }

      if (this.Sort) {
        var sorted = machineItems
          .Where (x => x.StartDateTime.HasValue)
          .OrderBy (x => x.StartDateTime.Value);
        var unsorted = machineItems
          .Where (x => !x.StartDateTime.HasValue);
        response.MachineItems = sorted.Concat (unsorted);
      }
      else {
        response.MachineItems = machineItems;
      }
      return response;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<MachinesSpecificReasonResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var cacheKey = "Business.Reason.MachinesSpecificReason." + this.Reason.Id;
      if (this.Sort) {
        cacheKey += ".Sort";
      }
      if (null != m_subSetOfMachines) {
        cacheKey += ".";
        cacheKey += string.Join (",", m_subSetOfMachines
          .OrderBy (m => m.Id)
          .Select (m => m.Id.ToString ())
          .ToArray ());
      }
      return cacheKey;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (MachinesSpecificReasonResponse data)
    {
      return CacheTimeOut.CurrentShort.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<MachinesSpecificReasonResponse> data)
    {
      return true;
    }
  }
}
