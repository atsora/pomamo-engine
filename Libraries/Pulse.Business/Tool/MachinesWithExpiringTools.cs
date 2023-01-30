// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.Business.Tool
{
  /// <summary>
  /// Return the machines that have tools that are close to expire
  /// </summary>
  public sealed class MachinesWithExpiringTools : IRequest<MachinesWithExpiringToolsResponse>
  {
    readonly IEnumerable<IMachine> m_subSetOfMachines = null;

    /// <summary>
    /// Max remaining tool life
    /// </summary>
    public TimeSpan MaxRemainingToolLife { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maxRemainingToolLife"></param>
    public MachinesWithExpiringTools (TimeSpan maxRemainingToolLife)
    {
      this.MaxRemainingToolLife = maxRemainingToolLife;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maxRemainingToolLife"></param>
    /// <param name="subSetOfMmachines"></param>
    public MachinesWithExpiringTools (TimeSpan maxRemainingToolLife, IEnumerable<IMachine> subSetOfMmachines)
    {
      this.MaxRemainingToolLife = maxRemainingToolLife;
      m_subSetOfMachines = subSetOfMmachines;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <returns></returns>
    public MachinesWithExpiringToolsResponse Get ()
    {
      var response = new MachinesWithExpiringToolsResponse ();

      IEnumerable<IMonitoredMachine> withExpiredOrInWarningToolsMachines;
      IEnumerable<IMonitoredMachine> withPotentiallyExpiredToolsMachines;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        withExpiredOrInWarningToolsMachines = ModelDAOHelper.DAOFactory.ToolLifeDAO
          .FindMachinesWithExpiredOrInWarningTools ()
          .Where (m => m.MonitoringType.Id == (int)MachineMonitoringTypeId.Monitored);
        withPotentiallyExpiredToolsMachines = ModelDAOHelper.DAOFactory.ToolLifeDAO
          .FindMachinesWithPotentiallyExpiredTools (this.MaxRemainingToolLife)
          .Where (m => m.MonitoringType.Id == (int)MachineMonitoringTypeId.Monitored);
      }
      if (null != m_subSetOfMachines) {
        withExpiredOrInWarningToolsMachines = withExpiredOrInWarningToolsMachines
          .Where (m => m_subSetOfMachines.Any (x => x.Id == m.Id));
        withPotentiallyExpiredToolsMachines = withPotentiallyExpiredToolsMachines
          .Where (m => m_subSetOfMachines.Any (x => x.Id == m.Id));
      }

      foreach (var machine in withPotentiallyExpiredToolsMachines.Union (withExpiredOrInWarningToolsMachines)) {
        var toolLivesByMachine = new ToolLivesByMachine (machine);
        var toolLives = Lemoine.Business.ServiceProvider
          .Get<ToolLivesByMachineResponse> (toolLivesByMachine);
        TestToolLivesByMachine (response, machine, toolLives);
      }

      return response;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<MachinesWithExpiringToolsResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    void TestToolLivesByMachine (MachinesWithExpiringToolsResponse response, IMachine machine, ToolLivesByMachineResponse toolLives)
    {
      if (toolLives.Expired || toolLives.Warning) {
        response.Machines.Add (machine);
        return;
      }

      // Versus MaxRemainingDuration
      if (toolLives.MinRemainingTime.HasValue) {
        var delta = DateTime.UtcNow.Subtract (toolLives.DateTime);
        var remainingTime = toolLives.MinRemainingTime.Value.Subtract (delta);
        if (remainingTime <= this.MaxRemainingToolLife) {
          response.Machines.Add (machine);
        }
        else {
          if (response.NextRemainingTime.HasValue) {
            if (response.NextRemainingTime.Value < remainingTime) {
              response.NextRemainingTime = remainingTime;
            }
          }
          else {
            response.NextRemainingTime = remainingTime;
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var cacheKey = "Business.Tool.MachinesWithExpiringTools."
        + this.MaxRemainingToolLife.TotalSeconds;
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
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (MachinesWithExpiringToolsResponse data)
    {
      if (data.NextRemainingTime.HasValue) {
        var cacheTimeOut = data.NextRemainingTime.Value;
        // TODO: when the cache is cleared by the tool life acquisition directly,
        // return directly cache time out
        // For the moment, limit it to 3 minutes
        if (TimeSpan.FromMinutes (3) < cacheTimeOut) {
          return TimeSpan.FromMinutes (3);
        }
        return cacheTimeOut;
      }
      else {
        return CacheTimeOut.CurrentLong.GetTimeSpan ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<MachinesWithExpiringToolsResponse> data)
    {
      return true;
    }
  }
}
