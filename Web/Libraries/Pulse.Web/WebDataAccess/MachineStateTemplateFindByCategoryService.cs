// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// MachineStateTemplateFindByCategory Service.
  /// </summary>
  public class MachineStateTemplateFindByCategoryService : GenericCachedService<Pulse.Web.WebDataAccess.MachineStateTemplateFindByCategory>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateFindByCategoryService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineStateTemplateFindByCategoryService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.MachineStateTemplateFindByCategory request)
    {
      int categoryId = request.CategoryId;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IList<IMachineStateTemplate> machineStateTemplates = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindByCategory ((MachineStateTemplateCategory)categoryId);
        return new MachineStateTemplateDTOAssembler ().Assemble (machineStateTemplates);
      }
    }
    #endregion // Methods
  }
}
