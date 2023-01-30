// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.ModelDAO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Lemoine.Plugin.ProductionCapacityPerMachine
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters

    [PluginConf ("Double", "Score", Description = "Score to give to this extension", Multiple = false, Optional = true)]
    [DefaultValue (100.0)]
    public double Score { get; set; }

    [PluginConf ("Double", "Capacity", Description = "Capacity per hour", Multiple = false, Optional = false)]
    [DefaultValue (0.0)]
    public double CapacityPerHour { get; set; } = 0.0;

    [PluginConf ("MachineFilter", "Machine filter", Description = "optionally a machine filter", Multiple = false, Optional = true)]
    [DefaultValue (0)]
    public int MachineFilterId
    {
      get; set;
    } = 0;

    /// <summary>
    /// Sort priority of the group category
    /// </summary>
    [PluginConf ("Machine", "Machine", Description = "optionally some specific machines", Multiple = true, Optional = true)]
    public IEnumerable<int> MachineIds
    {
      get; set;
    } = new List<int> ();
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();
      if (0 != this.MachineFilterId) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginReadOnlyTransaction ("Lemoine.Plugin.ProductionCapacityPerMachine.Configuration.MachineFilter")) {
          var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (this.MachineFilterId);
          if (null == machineFilter) {
            log.Error ($"IsValid: machine filter with id {this.MachineFilterId} does not exist");
            errorList.Add ($"Machine filter with id {this.MachineFilterId} does not exist");
          }
        }
      }
      if (!(this.MachineIds is null) && this.MachineIds.Any ()) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginReadOnlyTransaction ("Lemoine.Plugin.ProductionCapacityPerMachine.Configuration.Machine")) {
          foreach (var machineId in this.MachineIds) {
            var machine = ModelDAOHelper.DAOFactory.MachineDAO
              .FindById (machineId);
            if (null == machine) {
              log.Error ($"IsValid: machine with id {machineId} does not exist");
              errorList.Add ($"Machine with id {machineId} does not exist");
            }
          }
        }
      }
      errors = errorList;
      return 0 == errorList.Count;
    }
    #endregion // Constructors
  }
}
