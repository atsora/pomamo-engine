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

namespace Pulse.Web.MachineStateTemplate
{
  /// <summary>
  /// Description of NextMachineStateTemplateService
  /// </summary>
  public class NextMachineStateTemplateService
    : GenericCachedService<NextMachineStateTemplateRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (NextMachineStateTemplateService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public NextMachineStateTemplateService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }

    DateTime ParseDateTime (string s)
    {
      var bound = ConvertDTO.IsoStringToDateTimeUtc (s);
      Debug.Assert (bound.HasValue);
      return bound.Value;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(NextMachineStateTemplateRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        int currentMachineStateTemplateId = request.CurrentMachineStateTemplateId;
        IMachineStateTemplate currentMachineStateTemplate;
        if (0 == currentMachineStateTemplateId) {
          log.Debug ("GetWithoutCache: current machine state template ID is 0 => check At and MachineId");
          if (string.IsNullOrEmpty (request.At)) {
            log.Debug ("GetWithoutCache: current machine state template ID is 0 and At is not defined");
            currentMachineStateTemplate = null;
          }
          else { // At set
            if (0 == request.MachineId) {
              log.Error ($"GetWithoutCache: At is set but not MachineId");
              return new ErrorDTO ($"MachineId parameter is missing", ErrorStatus.WrongRequestParameter);
            }
            var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (request.MachineId);
            if (machine is null) {
              log.Error ($"GetWithoutCache: machine with id={request.MachineId} does not exist");
              return new ErrorDTO ($"Invalid Machine Id", ErrorStatus.WrongRequestParameter);
            }
            var at = ParseDateTime (request.At);
            var machineStateTemplateSlot = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
              .FindAt (machine, at);
            currentMachineStateTemplate = machineStateTemplateSlot?.MachineStateTemplate;
            if (log.IsDebugEnabled) {
              log.Debug ($"GetWithoutCache: machine state template Id={currentMachineStateTemplate?.Id} at {at} machineId={machine.Id}");
            }
          }
        }
        else {
          currentMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (currentMachineStateTemplateId);
          if (null == currentMachineStateTemplate) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown machine state template with ID {0}",
                             currentMachineStateTemplateId);
            return new ErrorDTO ("No machine state template with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
        }
        
        int roleId = request.RoleId;
        IRole role = null;
        if (0 != roleId) {
          role = ModelDAOHelper.DAOFactory.RoleDAO
            .FindById (roleId);
          if (null == role) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown role with ID {0}",
                             roleId);
            return new ErrorDTO ("No role with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
        }
        
        
        var result = new NextMachineStateTemplateResponseDTO ();
        result.MachineStateTemplates = new List<MachineStateTemplateDTO> ();
        
        // - Use MachineStateTemplateFlow
        IEnumerable<IMachineStateTemplate> nextMachineStateTemplates;
        if (null == currentMachineStateTemplate) {
          nextMachineStateTemplates = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindAll ();
        }
        else { // null != currentMachineStateTemplate
          nextMachineStateTemplates = ModelDAOHelper.DAOFactory.MachineStateTemplateFlowDAO
            .FindNext (currentMachineStateTemplate);
        }
        
        // - Get the granted machine state templates and restrict nextMachineStateTemplates
        if (null != role) {
          var grantedMachineStateTemplates = ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO
            .GetGranted (role);
          nextMachineStateTemplates = nextMachineStateTemplates
            .Intersect (grantedMachineStateTemplates);
        }
        
        foreach (var nextMachineStateTemplate in nextMachineStateTemplates) {
          result.MachineStateTemplates
            .Add (new MachineStateTemplateDTOAssembler ().Assemble (nextMachineStateTemplate));
        }
        
        return result;
      }
    }
  }
}
