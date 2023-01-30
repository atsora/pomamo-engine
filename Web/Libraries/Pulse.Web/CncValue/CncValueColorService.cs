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
  /// Description of CncValueColorService
  /// </summary>
  public class CncValueColorService
    : GenericCncValueColorService<CncValueColorRequestDTO>
  {    
    static readonly ILog log = LogManager.GetLogger(typeof (CncValueColorService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CncValueColorService ()
      : base(new CncValueColorDAO ())
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="GenericCncValueColorService{TRequestDTO}"/>
    /// </summary>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override UtcDateTimeRange GetRange (CncValueColorRequestDTO requestDTO)
    {
      return ParseRange (requestDTO.Range);
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(CncValueColorRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        // Field
        IField field = null;
        if (0 < request.FieldId) {
          field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById (request.FieldId);
          if (null == field) {
            log.Error ($"GetWithoutCache: unknown field with ID {request.FieldId}");
            return new ErrorDTO ("No field with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
        }
        
        // MachineModule
        IMachineModule machineModule;
        if (0 < request.MachineModuleId) {
          machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindByIdWithMonitoredMachine (request.MachineModuleId);
          if (null == machineModule) {
            log.Error ($"GetWithoutCache: unknown machine module with ID {request.MachineModuleId}");
            return new ErrorDTO ("No machine module with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
        }
        else {
          int machineId = request.MachineId;
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindByIdWithMainMachineModulePerformanceFieldUnit (machineId);
          if (null == machine) {
            log.Error ($"GetWithoutCache: unknown monitored machine with ID {machineId}");
            return new ErrorDTO ("No monitored machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
          machineModule = machine.MainMachineModule;
          if (null == machineModule) {
            log.Error ($"GetWithoutCache: no main machine module for monitored machine with ID {machineId}");
            return new ErrorDTO ("No main machine module defined",
                                 ErrorStatus.MissingConfiguration);
          }
        }
        Debug.Assert (null != machineModule);
        
        if (null == field) { // Swith to performance field
          IMonitoredMachine machine = machineModule.MonitoredMachine;
          field = machine.PerformanceField;
          if (null == field) {
            if (log.IsInfoEnabled) {
              log.Info ($"GetWithoutCache: no performance field for monitored machine with ID {machine.Id}");
            }
            return new ErrorDTO ("No performance field defined",
                                 ErrorStatus.NotApplicable);
          }
        }
        Debug.Assert (null != field);
        
        UtcDateTimeRange range = ParseRange (request.Range);
        return GetResponse (machineModule, field, range, request.SkipDetails);
      }
    }
    #endregion // Methods
  }
}
