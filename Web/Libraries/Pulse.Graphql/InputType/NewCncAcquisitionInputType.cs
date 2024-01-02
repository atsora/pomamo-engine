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
    public ICncAcquisition CreateCncAcquisition ()
    {
      bool error = false;
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("CreateCncAcquisition")) {
            var cncAcquisition = ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
            if (log.IsDebugEnabled) {
              log.Debug ($"CreateCncAcquisition: configName={this.CncConfigName} parameters={this.Parameters}");
            }
            // TODO: check the file exists first
            cncAcquisition.ConfigFile = this.CncConfigName + ".xml";
            if (Lemoine.Info.ConfigSet.LoadAndGet (KEY_PARAMS_KEY, KEY_PARAMS_DEFAULT)) {
              cncAcquisition.ConfigKeyParams = CncConfigParamValueInput.GetKeyParams (this.Parameters);
            }
            else {
              cncAcquisition.ConfigParameters = CncConfigParamValueInput.GetParametersString (this.Parameters);
            }
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
            return cncAcquisition;
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
