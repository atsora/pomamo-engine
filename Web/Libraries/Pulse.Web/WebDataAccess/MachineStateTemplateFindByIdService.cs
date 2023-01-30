// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// MachineStateTemplateFindById Service.
  /// </summary>
  public class MachineStateTemplateFindByIdService : GenericCachedService<Pulse.Web.WebDataAccess.MachineStateTemplateFindById>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateFindByIdService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineStateTemplateFindByIdService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.MachineStateTemplateFindById request)
    {
      int id = request.Id;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMachineStateTemplate machineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (id);
        if (null == machineStateTemplate) {
          return new ErrorDTO ("No machine state template with id " + request.Id,
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          return new MachineStateTemplateDTOAssembler ().Assemble (machineStateTemplate);
        }
      }
    }
    #endregion // Methods
  }
}
