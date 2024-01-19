// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Graphql.Type;

namespace Pulse.Graphql.InputType
{
  /// <summary>
  /// Class to create a <see cref="ICncAcquisition"/>
  /// 
  /// TODO: specify the name of the machine
  /// </summary>
  public class NewCncAcquisition
  {
    /// <summary>
    /// Check the configuration file exists first
    /// </summary>
    static readonly string CHECK_CONFIG_FILE_KEY = "Graphql.CncAcquisition.CheckConfigFile";
    static readonly bool CHECK_CONFIG_FILE_DEFAULT = true;

    /// <summary>
    /// Activate the new configuration key params
    /// </summary>
    static readonly string KEY_PARAMS_KEY = "Graphql.CncAcquisition.KeyParams";
    static readonly bool KEY_PARAMS_DEFAULT = true;

    readonly ILog log = LogManager.GetLogger<NewCncAcquisition> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public NewCncAcquisition ()
    {
    }

    /// <summary>
    /// Cnc config name
    /// </summary>
    public string? CncConfigName { get; set; }

    /// <summary>
    /// Parameters
    /// </summary>
    public IList<CncConfigParamValueInput> Parameters { get; set; } = new List<CncConfigParamValueInput> ();

    /// <summary>
    /// Create a new cnc acquisition
    /// </summary>
    /// <returns></returns>
    public CncAcquisitionResponse CreateCncAcquisition ()
    {
      bool error = false;
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var cncAcquisition = ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
          var response = new CncAcquisitionResponse (cncAcquisition);
          if (log.IsDebugEnabled) {
            log.Debug ($"CreateCncAcquisition: configName={this.CncConfigName} parameters={this.Parameters}");
          }
          cncAcquisition.ConfigFile = this.CncConfigName + ".xml";
          if (Lemoine.Info.ConfigSet.LoadAndGet (KEY_PARAMS_KEY, KEY_PARAMS_DEFAULT)) {
            cncAcquisition.ConfigKeyParams = CncConfigParamValueInput.GetKeyParams (this.Parameters);
          }
          else {
            cncAcquisition.ConfigParameters = CncConfigParamValueInput.GetParametersString (this.Parameters);
          }
          if (Lemoine.Info.ConfigSet.LoadAndGet (CHECK_CONFIG_FILE_KEY, CHECK_CONFIG_FILE_DEFAULT)
            && !response.CheckParameters (this.Parameters.ToDictionary (x => x.Name, x => x.Value))) {
            log.Error ("CreateCncAcquisition: load error or invalid parameters");
            response.UpdateAborted = true;
            error = true;
            return response;
          }

          using (var transaction = session.BeginTransaction ("CreateCncAcquisition")) {
            IComputer computer = ModelDAOHelper.DAOFactory.ComputerDAO
              .GetOrCreateLocal ();
            cncAcquisition.Computer = computer;
            if (!computer.IsLpst) {
              computer.IsLpst = true;
              ModelDAOHelper.DAOFactory.ComputerDAO.MakePersistent (computer);
            }
            ModelDAOHelper.DAOFactory.CncAcquisitionDAO.MakePersistent (cncAcquisition);
            var monitoredMachine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
            monitoredMachine.Name = $"CncAcquisition{cncAcquisition.Id}";
            monitoredMachine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO
              .FindById ((int)MachineMonitoringTypeId.Monitored);
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (monitoredMachine);
            var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (monitoredMachine, $"CncAcquisition{cncAcquisition.Id}");
            machineModule.CncAcquisition = cncAcquisition;
            ModelDAOHelper.DAOFactory.MachineModuleDAO.MakePersistent (machineModule);
            transaction.Commit ();
            return response;
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"CreateCncAcquisition: exception", ex);
        error = true;
        throw;
      }
      finally {
        if (!error) {
          ConfigUpdater.Notify ();
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type for a new cnc acquisition
  /// </summary>
  public class NewCncAcquisitionInputType : InputObjectGraphType<NewCncAcquisition>
  {
    readonly ILog log = LogManager.GetLogger (typeof (NewCncAcquisitionInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NewCncAcquisitionInputType ()
    {
      Name = "NewCncAcquisition";
      Field<string> ("cncConfigName", nullable: false);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<CncConfigParamValueInputType>>>> ("parameters");
    }
  }
}
