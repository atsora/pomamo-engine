// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Business.CncValue;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Description of RedStackLightService
  /// </summary>
  public class RedStackLightService
    : GenericCncValueColorService<RedStackLightRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RedStackLightService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public RedStackLightService ()
      : base (new RedStackLightDAO ())
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="GenericCncValueColorService{TRequestDTO}"/>
    /// </summary>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override UtcDateTimeRange GetRange (RedStackLightRequestDTO requestDTO)
    {
      return ParseRange (requestDTO.Range);
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (RedStackLightRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Field
        IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.StackLight);
        if (null == field) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown field with ID {0}",
                           request.MachineModuleId);
          return new ErrorDTO ("No field with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        // MachineModule
        IMachineModule machineModule;
        if (0 < request.MachineModuleId) {
          machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindByIdWithMonitoredMachine (request.MachineModuleId);
          if (null == machineModule) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown machine module with ID {0}",
                             request.MachineModuleId);
            return new ErrorDTO ("No machine module with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
        }
        else {
          int machineId = request.MachineId;
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindByIdWithMainMachineModulePerformanceFieldUnit (machineId);
          if (null == machine) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown monitored machine with ID {0}",
                             machineId);
            return new ErrorDTO ("No monitored machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
          machineModule = machine.MainMachineModule;
          if (null == machineModule) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "no main machine module for monitored machine with ID {0}",
                             machineId);
            return new ErrorDTO ("No main machine module defined",
                                 ErrorStatus.MissingConfiguration);
          }
        }
        Debug.Assert (null != machineModule);

        UtcDateTimeRange range = ParseRange (request.Range);
        return GetResponse (machineModule, field, range, request.SkipDetails);
      }
    }
    #endregion // Methods
  }
}
