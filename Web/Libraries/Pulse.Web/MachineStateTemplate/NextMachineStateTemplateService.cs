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

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public NextMachineStateTemplateService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
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
          log.Debug ("GetWithoutCache: " +
                     "current machine state template ID 0");
          currentMachineStateTemplate = null;
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
    #endregion // Methods
  }
}
