// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Description of CncValueCurrentService
  /// </summary>
  public class CncValueCurrentService
    : GenericCachedService<CncValueCurrentRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncValueCurrentService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CncValueCurrentService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (CncValueCurrentRequestDTO request)
    {
      CncValueCurrentResponseDTO response = new CncValueCurrentResponseDTO ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // - Machine module
        IEnumerable<IMachineModule> machineModules = new List<IMachineModule> ();
        IMonitoredMachine machine = null;
        if (0 < request.MachineModuleId) {
          var machineModuleList = new List<IMachineModule> ();
          var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindByIdWithMonitoredMachine (request.MachineModuleId);
          if (null == machineModule) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown machine module with ID {0}",
                             request.MachineModuleId);
            return new ErrorDTO ("No machine module with the specified ID",
              ErrorStatus.WrongRequestParameter);
          }
          machineModuleList.Add (machineModule);
          machineModules = machineModuleList;
          machine = machineModule.MonitoredMachine;
          if (null == machine) {
            log.ErrorFormat ("GetWithoutCache: no monitored machine is associated to machine module {0}", machineModule.Id);
            return new ErrorDTO ("Machine module not associated to any monitored machine", ErrorStatus.WrongRequestParameter);
          }
        }
        if (0 < request.MachineId) {
          if (machineModules.Any ()) {
            if (request.MachineId != machineModules.First ().MonitoredMachine.Id) {
              log.ErrorFormat ("GetWithoutCache: " +
                               "specified machineId {0} and machineModuleId {1} are not compatible with each other",
                               request.MachineId, request.MachineModuleId);
              return new ErrorDTO ("Incompatible specified MachineId and MachineModuleId",
                ErrorStatus.WrongRequestParameter);
            }
          }
          else {
            machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindByIdWithMainMachineModulePerformanceFieldUnit (request.MachineId);
            if (null == machine) {
              log.ErrorFormat ("GetWithoutCache: " +
                               "unknown monitored machine with ID {0}",
                               request.MachineId);
              return new ErrorDTO ("No monitored machine with the specified ID",
                                   ErrorStatus.WrongRequestParameter);
            }
          }
        }
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "no valid machine or machine module was set in request. machineId={0} machineModuleId={1}",
                           request.MachineId, request.MachineModuleId);
          return new ErrorDTO ("No monitored machine or machine module set in the request",
                               ErrorStatus.WrongRequestParameter);
        }
        Debug.Assert (null != machine);

        // - Performance field
        var performanceField = machine.PerformanceField;
        if (log.IsDebugEnabled && (performanceField is null)) {
          log.Debug ($"GetWithoutCache: no performance field for monitored machine with ID {machine.Id}");
        }

        // - Fields
        IEnumerable<IField> fields = new List<IField> ();
        if ((null != request.FieldIds) && (request.FieldIds.Any ())) {
          fields = ModelDAOHelper.DAOFactory.FieldDAO
            .FindAllActiveWithUnit ()
            .Where (f => request.FieldIds.Contains (f.Id));
        }
        else if (!string.IsNullOrEmpty (request.PredefinedFields)
          && request.PredefinedFields.Equals ("all", StringComparison.InvariantCultureIgnoreCase)) {
          fields = ModelDAOHelper.DAOFactory.FieldDAO.FindAllActiveWithUnit ();
        }
        else { // Performance field of the main machine module
          if (null == machine.MainMachineModule) {
            log.ErrorFormat ("GetWithoutCache: no main machine module for machine id {0}", machine.Id);
            return new ErrorDTO ("No main machine module for the specified machine",
                                 ErrorStatus.WrongRequestParameter);
          }
          var machineModuleList = new List<IMachineModule> ();
          machineModuleList.Add (machine.MainMachineModule);
          machineModules = machineModuleList;
          if (null == performanceField) {
            log.InfoFormat ("GetWithoutCache: no performance field for machine id {0}", machine.Id);
            return new ErrorDTO ("No performance field for the specified machine",
              ErrorStatus.NotApplicable);
          }
          else { // null != performanceField
            if (log.IsDebugEnabled) {
              log.Debug ($"GetWithoutCache: consider performance field {performanceField?.Id}");
            }
            var fieldList = new List<IField> { performanceField };
            fields = fieldList;
          }
        }

        // - Machine modules
        if (!machineModules.Any ()) {
          machineModules = machine.MachineModules;
        }
        {
          var mainMachineModule = machine.MainMachineModule;
          machineModules = machineModules
            .OrderBy (machineModule => IsMainMachineModule (machineModule, mainMachineModule) ? 0 : 1);
        }

        foreach (var machineModule in machineModules) {
          using (IDAOTransaction transaction = session.BeginReadOnlyDeferrableTransaction ("Web.CncValueCurrent")) {
            IEnumerable<CncValueCurrentByMachineModuleFieldDTO> currentCncValueDtos = new List<CncValueCurrentByMachineModuleFieldDTO> ();
            if (1 == fields.Count ()) {
              var field = fields.First ();
              var currentCncValue = ModelDAOHelper.DAOFactory.CurrentCncValueDAO
                .Find (machineModule, field);
              if (null != currentCncValue) {
                var list = new List<CncValueCurrentByMachineModuleFieldDTO> ();
                var dto = new CncValueCurrentByMachineModuleFieldDTO (currentCncValue);
                list.Add (dto);
                currentCncValueDtos = list;
              }
            }
            else { // 1 != fields.Count
              var currentCncValues = ModelDAOHelper.DAOFactory.CurrentCncValueDAO
                .FindByMachineModule (machineModule)
                .Where (c => fields.Select (f => f.Id).Contains (c.Field.Id))
                .OrderBy (c => IsPerformanceField (c.Field, performanceField) ? 0 : 1);
              currentCncValueDtos = currentCncValues
                .Select (c => new CncValueCurrentByMachineModuleFieldDTO (c));
            }
            if (currentCncValueDtos.Any ()) {
              var byMachineModule =
                new CncValueCurrentByMachineModuleDTO (machineModule,
                                                       IsMainMachineModule (machineModule, machine.MainMachineModule));
              response.ByMachineModule.Add (byMachineModule);
              foreach (var currentCncValueDto in currentCncValueDtos) {
                byMachineModule.ByField.Add (currentCncValueDto);
              }
            }
          }
        }
      }

      return response;
    }

    bool IsMainMachineModule (IMachineModule machineModule, IMachineModule mainMachineModule)
    {
      Debug.Assert (null != machineModule);

      return (null != mainMachineModule) && machineModule.Id.Equals (mainMachineModule.Id);
    }

    bool IsPerformanceField (IField field, IField performanceField)
    {
      Debug.Assert (null != field);

      return (null != field) && (null != performanceField) && field.Id.Equals (performanceField.Id);
    }
    #endregion // Methods
  }
}
