// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
  /// Description of MachineStateTemplatesService
  /// </summary>
  public class MachineStateTemplatesService
    : GenericCachedService<MachineStateTemplatesRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplatesService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public MachineStateTemplatesService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(MachineStateTemplatesRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
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
        
        
        var result = new MachineStateTemplatesResponseDTO ();
        result.MachineStateTemplates = new List<MachineStateTemplateDTO> ();
        
        // - Get the granted machine state templates and restrict MachineStateTemplatess
        IList<IMachineStateTemplate> machineStateTemplates;
        if (null != role) {
          machineStateTemplates = ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO
            .GetGranted (role);
        }
        else {
          machineStateTemplates = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindAll ();
        }
        
        foreach (var machineStateTemplate in machineStateTemplates) {
          result.MachineStateTemplates
            .Add (new MachineStateTemplateDTOAssembler ().Assemble (machineStateTemplate));
        }
        
        return result;
      }
    }
  }
}
