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

namespace Lemoine.Business.MachineStateTemplate
{
  /// <summary>
  /// Request class to get the machines with a specific machine state template
  /// </summary>
  public sealed class MachinesSpecificMachineStateTemplate
    : IRequest<MachinesSpecificMachineStateTemplateResponse>
  {
    #region Members
    readonly IMachineStateTemplate m_machineStateTemplate;
    readonly bool m_sort = true;
    readonly IEnumerable<IMachine> m_subSetOfMachines = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MachinesSpecificMachineStateTemplate).FullName);

    #region Getters / Setters
    /// <summary>
    /// Machine state template in parameter
    /// 
    /// not null
    /// </summary>
    public IMachineStateTemplate MachineStateTemplate
    {
      get { return m_machineStateTemplate; }
    }

    /// <summary>
    /// Sort the machine by machine state template slot start time
    /// </summary>
    public bool Sort
    {
      get { return m_sort; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor with the sort option by default
    /// </summary>
    /// <param name="machineStateTemplate">not null</param>
    public MachinesSpecificMachineStateTemplate (IMachineStateTemplate machineStateTemplate)
      : this (machineStateTemplate, true)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="sort"></param>
    public MachinesSpecificMachineStateTemplate (IMachineStateTemplate machineStateTemplate, bool sort)
    {
      Debug.Assert (null != machineStateTemplate);

      m_machineStateTemplate = machineStateTemplate;
      m_sort = sort;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="sort"></param>
    /// <param name="subSetOfMachines"></param>
    public MachinesSpecificMachineStateTemplate (IMachineStateTemplate machineStateTemplate, bool sort, IEnumerable<IMachine> subSetOfMachines)
    {
      Debug.Assert (null != machineStateTemplate);

      m_machineStateTemplate = machineStateTemplate;
      m_sort = sort;
      m_subSetOfMachines = subSetOfMachines;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public MachinesSpecificMachineStateTemplateResponse Get ()
    {
      var response = new MachinesSpecificMachineStateTemplateResponse ();
      var machineItems = new List<MachinesSpecificMachineStateTemplateMachineResponse> ();
      var applicableDateTime = response.DateTime;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineStateTemplate.MachinesSpecificMachineStateTemplate")) {
          IEnumerable<IMachine> machines;
          if (null != m_subSetOfMachines) {
            foreach (var machine in m_subSetOfMachines) {
              ModelDAOHelper.DAOFactory.MachineDAO
                .Lock (machine);
            }
            machines = m_subSetOfMachines;
          }
          else {
            machines = ModelDAOHelper.DAOFactory.MachineDAO
              .FindAll ();
          }
          var range = new UtcDateTimeRange (applicableDateTime, applicableDateTime, "[]");
          foreach (var machine in machines) {
            if (!this.Sort) {
              var observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
                .FindOverlapsRangeMatchingMachineStateTemplate (machine, range, this.MachineStateTemplate)
                .FirstOrDefault ();
              if (null != observationStateSlot) {
                var machineData = new MachinesSpecificMachineStateTemplateMachineResponse (machine);
                machineItems.Add (machineData);
              }
            }
            else { // Sort
              var machineStateTemplateSlot = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
                .FindOverlapsRangeMatchingMachineStateTemplate (machine, range, this.MachineStateTemplate)
                .FirstOrDefault ();
              if (null != machineStateTemplateSlot) {
                var machineData = new MachinesSpecificMachineStateTemplateMachineResponse (machine);
                machineData.StartDateTime = machineStateTemplateSlot.DateTimeRange.Lower;
                machineItems.Add (machineData);
              }
            }
          } // Loop on machines
        }
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
    public async Task<MachinesSpecificMachineStateTemplateResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var cacheKey = "Business.MachineStateTemplate.MachinesSpecificMachineStateTemplate." + this.MachineStateTemplate.Id;
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
    public TimeSpan GetCacheTimeout (MachinesSpecificMachineStateTemplateResponse data)
    {
      return CacheTimeOut.CurrentLong.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<MachinesSpecificMachineStateTemplateResponse> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
